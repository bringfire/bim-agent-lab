using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using System.Text.Json;

// Verification driver only: all MCP transport and tool dispatch use the official SDK.
internal static class ReadinessScenario
{
    public static async Task<int> Run(string server,List<string> serverArgs,string evidence,string protocol,string expected,int? pid,string? token,string? pauseFile)
    {
        if(Directory.Exists(evidence))throw new ArgumentException("Use a new evidence directory for each attempt.");
        if(pauseFile is not null && File.Exists(pauseFile))throw new ArgumentException("Pause-release file must be absent at admission.");
        Directory.CreateDirectory(evidence);
        var json=new JsonSerializerOptions{WriteIndented=true};
        File.WriteAllText(Path.Combine(evidence,"partial.json"),JsonSerializer.Serialize(new{startedUtc=DateTime.UtcNow,expected,protocol,pid},json));
        if(pid.HasValue){serverArgs.Add("--revit-pid");serverArgs.Add(pid.Value.ToString());}
        if(serverArgs.Contains("--wire-evidence"))throw new ArgumentException("Readiness driver owns its wire directory.");
        serverArgs.Add("--wire-evidence");serverArgs.Add(Path.Combine(evidence,"wire"));
        using var stop=new CancellationTokenSource(TimeSpan.FromMinutes(7));
        var transport=new StdioClientTransport(new StdioClientTransportOptions{Command=server,Arguments=serverArgs,Name="lab readiness verification client"});
        var client=await McpClient.CreateAsync(transport,new McpClientOptions{ProtocolVersion=protocol},cancellationToken:stop.Token);
        bool pass=false;
        try {
        var tools=await client.ListToolsAsync(cancellationToken:stop.Token);
        CallToolResult? context=null,check=null;
        if(token is null) {
            context=await client.CallToolAsync("revit_get_readiness_context",new Dictionary<string,object?>(),cancellationToken:stop.Token);
            File.WriteAllText(Path.Combine(evidence,"context.json"),JsonSerializer.Serialize(context,json));
            if(context.IsError==false)token=context.StructuredContent?.GetProperty("contextToken").GetString();
        }
        if(pauseFile is not null && token is not null) {
            File.WriteAllText(Path.Combine(evidence,"context-ready.json"),JsonSerializer.Serialize(new{token,waitingFor=pauseFile},json));
            Console.WriteLine("Context acquired; waiting for explicit test-preparation release file.");
            while(!File.Exists(pauseFile))await Task.Delay(100,stop.Token);
        }
        if(token is not null && expected!="Context") {
            check=await client.CallToolAsync("revit_check_required_type_information",new Dictionary<string,object?>{{"contextToken",token}},cancellationToken:stop.Token);
            File.WriteAllText(Path.Combine(evidence,"check.json"),JsonSerializer.Serialize(check,json));
        }
        var result=check??context??throw new InvalidDataException("No result.");
        var data=result.StructuredContent??throw new InvalidDataException("No structured result.");
        var names=tools.Select(t=>t.Name).Order(StringComparer.Ordinal).ToArray();
        pass=client.NegotiatedProtocolVersion==protocol
            && names.SequenceEqual(new[]{"revit_check_required_type_information","revit_get_readiness_context"})
            && data.TryGetProperty("requestId",out var id) && Guid.TryParseExact(id.GetString(),"N",out _)
            && result.Content.OfType<TextContentBlock>().Single().Text==data.GetRawText();
        pass &= expected switch {
            "Context"=>context?.IsError==false && token is not null,
            "ReadinessReport"=>check?.IsError==false && data.TryGetProperty("report",out var report) && report.ValueKind==JsonValueKind.Object,
            _=>result.IsError==true && data.TryGetProperty("code",out var code) && code.GetString()==expected && !data.TryGetProperty("report",out _)
        };
        File.WriteAllText(Path.Combine(evidence,"sdk-client-result.json"),JsonSerializer.Serialize(new{clientKind="SDK harness, not desktop A8 acceptance",protocol=client.NegotiatedProtocolVersion,tools=names,expected,context,check,pass,completedUtc=DateTime.UtcNow},json));
        } finally { await client.DisposeAsync(); }
        // Completion is published only after the SDK transport has shut down.
        // Completion describes capture/test outcome; a failed assertion is retained, never overwritten as a retry.
        File.WriteAllText(Path.Combine(evidence,"complete.json"),JsonSerializer.Serialize(new{pass},json));
        Console.WriteLine($"{(pass?"PASS":"FAIL")} SDK readiness: expected={expected}; evidence={evidence}");
        return pass?0:1;
    }
}
