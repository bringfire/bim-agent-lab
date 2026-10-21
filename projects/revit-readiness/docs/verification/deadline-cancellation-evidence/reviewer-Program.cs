using System;
using System.IO;
using System.IO.Pipes;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Runtime;
using BimAgentLab.RevitBridge.Contract;
class Review {
 [DllImport("kernel32.dll")] static extern uint SetErrorMode(uint mode);
 static async Task<int> Main(string[] args) {
  SetErrorMode(3);
  if(args.Length==0 || args[0]=="--control") {
   var exe=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"BimAgentLab.Bridge.Tests.exe");
   using(var p=new Process { StartInfo=new ProcessStartInfo(exe,args.Length==0 ? "--deadline" : "--safe") { UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true } }) {
    p.Start(); var stdout=p.StandardOutput.ReadToEndAsync(); var stderr=p.StandardError.ReadToEndAsync();
    if(!p.WaitForExit(7000)) { p.Kill(); throw new Exception("Deadline child hung"); }
    Console.WriteLine("RUNTIME "+RuntimeInformation.FrameworkDescription+" CHILD_EXIT="+p.ExitCode);
    Console.Write(await stdout); Console.Write(await stderr);
   }
   return 0;
  }
  var directory=Path.Combine(Path.GetTempPath(),"codex-deadline-"+Guid.NewGuid().ToString("N"));
  var descriptor=BridgeIdentity.CreateCurrent(); var catalog=new SessionCatalog(directory);
  var endpoint=new PipeEndpoint(descriptor,catalog,new SystemBridgeClock());
  var response=new TaskCompletionSource<BridgeResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
  var quiesced=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
  endpoint.Start(c=> {
   if(args[0]!="--safe") c.Cancellation.Register(()=>throw new InvalidOperationException("INJECTED cancellation callback failure at original deadline")); else c.Cancellation.Register(()=>Console.WriteLine("CONTROL cancellation callback returned normally"));
   Console.WriteLine("SUBMITTED callback registered; awaiting server deadline"); Console.Out.Flush();
   return new DispatchTicket(response.Task,quiesced.Task);
  });
  using(var client=new NamedPipeClientStream(".",descriptor.PipeName,PipeDirection.InOut,PipeOptions.Asynchronous)) {
   await client.ConnectAsync(2000);
   var request=new BridgeRequest { ContractVersion=1, RequestId="deadline-callback", Operation=BridgeLimits.CheckInformation, ContextToken="test-token", DeadlineUtc=DateTimeOffset.UtcNow.AddSeconds(1).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'",CultureInfo.InvariantCulture) };
   await FrameCodec.WriteAsync(client,BridgeJson.Serialize(request),BridgeLimits.MaxRequestBytes,CancellationToken.None);
   await Task.Delay(2500);
   Console.WriteLine("SURVIVED server deadline");
   quiesced.TrySetResult(true); response.TrySetCanceled();
   try { await endpoint.StopAsync(); } catch(Exception e) { Console.WriteLine("STOP reported "+e); }
  }
  return 0;
 }
}

