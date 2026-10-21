using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;
using BimAgentLab.RevitBridge.Runtime;
namespace BimAgentLab.Bridge.Tests;
internal static partial class Program
{
    static async Task<int> PipeHost(string directory)
    {
        var descriptor = BridgeIdentity.CreateCurrent();
        var endpoint = new PipeEndpoint(descriptor, new SessionCatalog(directory), new SystemBridgeClock());
        endpoint.Start(context =>
        {
            var response = Reply(context, descriptor.BridgeSessionId);
            if (context.Request.RequestId == "cross-runtime-large") response.ResultJson = LargeReport;
            return new DispatchTicket(Task.FromResult(response), Task.CompletedTask);
        });
        Console.WriteLine("READY " + descriptor.RevitPid); Console.Out.Flush();
        Console.ReadLine();
        await endpoint.StopAsync();
        Console.WriteLine("STOPPED"); return 0;
    }
    static async Task<int> PipeImposter(string descriptorPath)
    {
        var descriptor = BridgeJson.Deserialize<BridgeSessionDescriptor>(File.ReadAllBytes(descriptorPath));
        using var pipe = LocalPipeFactory.Create(descriptor.PipeName, descriptor.LogonSid);
        Console.WriteLine("READY"); Console.Out.Flush();
        await pipe.WaitForConnectionAsync();
        int received = await pipe.ReadAsync(new byte[1], 0, 1, CancellationToken.None);
        Console.WriteLine("BYTES " + received); return received == 0 ? 0 : 1;
    }
}
