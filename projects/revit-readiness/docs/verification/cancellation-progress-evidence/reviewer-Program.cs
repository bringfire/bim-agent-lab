using System;
using System.IO;
using System.IO.Pipes;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Runtime;
using BimAgentLab.RevitBridge.Contract;
class Review {
 static async Task Main() {
  Console.WriteLine("RUNTIME "+RuntimeInformation.FrameworkDescription);
  foreach(var mode in new[]{"stop","disconnect"}) await Run(mode);
 }
 static async Task Run(string mode) {
  var directory=Path.Combine(Path.GetTempPath(),"codex-held-"+Guid.NewGuid().ToString("N"));
  var descriptor=BridgeIdentity.CreateCurrent(); var catalog=new SessionCatalog(directory);
  var ready=new TaskCompletionSource<BridgeExecutionContext>(TaskCreationOptions.RunContinuationsAsynchronously);
  var entered=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
  var stopReturned=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
  var response=new TaskCompletionSource<BridgeResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
  var quiesced=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
  var closed=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
  using(var release=new ManualResetEventSlim()) {
   var endpoint=new PipeEndpoint(descriptor,catalog,new SystemBridgeClock(),e=>{if(e.Stage=="Closed" && e.ConnectionId==1)closed.TrySetResult(true);});
   endpoint.Start(c=> {
    c.Cancellation.Register(()=> { entered.TrySetResult(true); if(!release.Wait(5000))throw new Exception("Harness release timeout"); });
    ready.TrySetResult(c); return new DispatchTicket(response.Task,quiesced.Task);
   });
   using(var client=new NamedPipeClientStream(".",descriptor.PipeName,PipeDirection.InOut,PipeOptions.Asynchronous)) {
    await client.ConnectAsync(2000);
    var request=new BridgeRequest { ContractVersion=1,RequestId="held-"+mode,Operation=BridgeLimits.CheckInformation,ContextToken="test",DeadlineUtc=DateTimeOffset.UtcNow.AddSeconds(15).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'",CultureInfo.InvariantCulture) };
    await FrameCodec.WriteAsync(client,BridgeJson.Serialize(request),BridgeLimits.MaxRequestBytes,CancellationToken.None);
    var context=await Within(ready.Task);
    Task stop=null;
    try {
     if(mode=="disconnect") {
      client.Dispose();await Within(entered.Task);
      var slots=new NamedPipeClientStream[8]; int connected=0;
      try {
       for(int i=0;i<slots.Length;i++) {
        slots[i]=new NamedPipeClientStream(".",descriptor.PipeName,PipeDirection.InOut,PipeOptions.Asynchronous);
        try { await slots[i].ConnectAsync(200); connected++; }
        catch(Exception e) when(e is TimeoutException || e is IOException) { Console.WriteLine("HELD DISCONNECT slot="+(i+1)+" failed="+e.GetType().Name);break; }
       }
       Console.WriteLine("HELD DISCONNECT concurrentConnections="+connected+" expected=8");
      } finally {foreach(var slot in slots)slot?.Dispose();}
     }
     stop=Task.Run(async()=>{var pending=endpoint.StopAsync();stopReturned.TrySetResult(true);await pending;});
     await Within(entered.Task);quiesced.TrySetResult(true);
     await Task.Delay(1000);
     Console.WriteLine("BEFORE RELEASE mode="+mode+" executorQuiesced="+quiesced.Task.IsCompleted+" StopAsyncReturned="+stopReturned.Task.IsCompleted+" StopCompleted="+stop.IsCompleted+" ConnectionClosedEvent="+closed.Task.IsCompleted+" DescriptorExists="+File.Exists(Path.Combine(directory,descriptor.BridgeSessionId+".json")));
    } finally {release.Set();quiesced.TrySetResult(true);response.TrySetCanceled();if(stop!=null)await Within(stop);else await Within(endpoint.StopAsync());}
    Console.WriteLine("AFTER RELEASE mode="+mode+" StopCompleted="+stop.IsCompleted+" ConnectionClosedEvent="+closed.Task.IsCompleted+" DescriptorExists="+File.Exists(Path.Combine(directory,descriptor.BridgeSessionId+".json")));
   }
  }
 }
 static async Task<T> Within<T>(Task<T> task){if(await Task.WhenAny(task,Task.Delay(5000))!=task)throw new Exception("Harness timeout");return await task;}
 static async Task Within(Task task){if(await Task.WhenAny(task,Task.Delay(5000))!=task)throw new Exception("Harness timeout");await task;}
}

