using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

namespace BimAgentLab.Mcp.Tests;
internal static partial class Program
{
    static string DeploymentProject = "", DeploymentEvidence = "";
    static void Equal<T>(T expected,T actual) { if(!EqualityComparer<T>.Default.Equals(expected,actual))throw new Exception($"Expected {expected}; actual {actual}"); }
    static async Task<int> DeploymentTests(string[] args)
    {
        DeploymentProject=Path.GetFullPath(args[1]);
        DeploymentEvidence=Path.Combine(DeploymentProject,"samples/local/deployment-tests",Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(DeploymentEvidence);
        if(args.Length>2 && args[2]=="legacy") {await Test("Documented legacy fixture migrates and undoes to no inventory",()=>DeploymentCase("legacy"));await Test("Unknown legacy fixture rejected with zero changes",()=>DeploymentPreflight("invalid-legacy"));return failures==0?0:1;}
        if(args.Length>2 && args[2]=="restore") { await Test("Interrupted restoration resumes across a partial journal record",()=>DeploymentCase("existing","RestoreMutation","exit"));return failures==0?0:1;}
        if(args.Length>2) {await Test("Fresh server/recovery parent",()=>DeploymentCase("fresh-parent"));await Test("Prepared plan cannot delete a directory it did not create",()=>DeploymentConflict("prepared-directory"));await Test("User mutex is shared across Windows sessions",DeploymentLease);return failures==0?0:1;}
        foreach(var scenario in new[]{"none","existing","explicit","normal","fresh-parent","legacy"})
            await Test("Deployment success, exact undo twice: "+scenario,()=>DeploymentCase(scenario));
        foreach(var scenario in new[]{"invalid-fixture","forbidden","modern-in-host","unowned","bad-inventory","missing-notice","invalid-legacy"})
            await Test("Deployment preflight zero mutations: "+scenario,()=>DeploymentPreflight(scenario));
        foreach(var boundary in new[]{"FirstCopy","ObsoleteDelete","FixtureCore","FixtureInventory","Manifest","MainInventory","BeforeCommit","AfterMutation"})
            foreach(var fault in new[]{"throw","exit"})
                await Test($"Deployment {fault} at {boundary}, fresh-process undo twice",()=>DeploymentCase("existing",boundary,fault));
        await Test("Recovery interrupted, trailing journal fragment, resumes twice",()=>DeploymentCase("existing","RestoreMutation","exit"));
        foreach(var scenario in new[]{"foreign","missing-backup","changed-stage","pending","backup-failure","changed-after-preflight","missing-last-backup","foreign-temp","prepared-directory"})
            await Test("Deployment recovery preflight: "+scenario,()=>DeploymentConflict(scenario));
        await Test("Deployment lease excludes another process and rejects disposed owner",DeploymentLease);
        await Test("Interrupted backup preparation leaves targets unchanged and permits new attempt",DeploymentPreparationExit);
        Console.WriteLine($"Deployment tests: {41-failures} passed; {failures} failed. Evidence: {DeploymentEvidence}");
        return failures==0?0:1;
    }
    static string DeploymentServerRoot(string root) => File.ReadAllText(Path.Combine(root,"server-root.txt"));
    static SortedDictionary<string,string> DeploymentSnapshot(string root)
    {
        var result=new SortedDictionary<string,string>(StringComparer.Ordinal);
        foreach(var tree in new[]{"addins","server"}) {
            var dir=tree=="server"?DeploymentServerRoot(root):Path.Combine(root,tree); if(!Directory.Exists(dir))continue;
            result[tree+"/"]="directory";
            foreach(var p in Directory.EnumerateDirectories(dir,"*",SearchOption.AllDirectories)) result[Path.GetRelativePath(root,p)+"/"]="directory";
            foreach(var p in Directory.EnumerateFiles(dir,"*",SearchOption.AllDirectories)) result[Path.GetRelativePath(root,p)]=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p)));
        }
        result["client-settings.json"]=File.ReadAllText(Path.Combine(root,"client-settings.json"));
        return result;
    }
    static async Task<int> DeploymentRun(string root,string mode,string scenario="existing",string boundary="",string fault="throw",bool undo=false)
    {
        var start=new ProcessStartInfo("pwsh") {UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
        foreach(var a in new[]{"-NoProfile","-File",Path.Combine(DeploymentProject,undo?"scripts/Undo-McpBridgeInstall.ps1":"tests/deployment-fault-host.ps1")})start.ArgumentList.Add(a);
        if(undo) {start.ArgumentList.Add("-ReceiptPath");start.ArgumentList.Add(File.ReadAllText(Path.Combine(root,"receipt-path.txt")));}
        else foreach(var a in new[]{"-Root",root,"-Mode",mode,"-Scenario",scenario,"-Boundary",boundary,"-Fault",fault})start.ArgumentList.Add(a);
        using var child=Process.Start(start)!;
        var output=child.StandardOutput.ReadToEndAsync();var error=child.StandardError.ReadToEndAsync();
        try {await child.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(45));}
        catch {child.Kill(true);await child.WaitForExitAsync();throw;}
        string log=await output+await error;
        File.WriteAllText(Path.Combine(root,$"{mode}-{boundary}-{Guid.NewGuid():N}.txt"),log);
        if(child.ExitCode!=0 && boundary=="" && mode=="Seed")throw new Exception(log);
        return child.ExitCode;
    }
    static async Task<string> DeploymentSeed(string scenario)
    {
        string root=Path.Combine(DeploymentEvidence,scenario+" space "+Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
        Equal(0,await DeploymentRun(root,"Seed",scenario));return root;
    }
    static void DeploymentCompare(SortedDictionary<string,string> expected,string root) => Equal(JsonSerializer.Serialize(expected),JsonSerializer.Serialize(DeploymentSnapshot(root)));
    static async Task DeploymentPreflight(string scenario)
    {
        string root=await DeploymentSeed(scenario);var before=DeploymentSnapshot(root);
        if(await DeploymentRun(root,"Apply",scenario)==0)throw new Exception("Invalid package accepted");
        DeploymentCompare(before,root);
    }
    static async Task DeploymentCase(string scenario,string boundary="",string fault="throw")
    {
        string root=await DeploymentSeed(scenario);var before=DeploymentSnapshot(root);
        File.WriteAllText(Path.Combine(root,"independent-before.json"),JsonSerializer.Serialize(before));
        Equal(0,await DeploymentRun(root,"Preflight",scenario)); DeploymentCompare(before,root);
        bool recoveryFault=boundary=="RestoreMutation";
        int code=await DeploymentRun(root,"Apply",scenario,recoveryFault?"":boundary,fault);
        if(boundary==""||recoveryFault)Equal(0,code);else if(fault=="exit")Equal(91,code);else if(code==0)throw new Exception("Fault hook was not reached");
        if(boundary=="") {
            var host=Path.Combine(root,"addins/BimAgentLab.RequiredInformation");
            if(Directory.EnumerateFiles(host,"*",SearchOption.AllDirectories).Any(p=>Path.GetFileName(p).StartsWith("RevitAPI")||Path.GetFileName(p).StartsWith("ModelContextProtocol")))throw new Exception("Package boundary crossed");
            if(scenario is "existing" or "explicit" or "legacy" or "fresh-parent")Equal(File.ReadAllText(Path.Combine(host,"BimAgentLab.Readiness.Core.dll")),File.ReadAllText(Path.Combine(root,"addins/BimAgentLab.FixtureTools/BimAgentLab.Readiness.Core.dll")));
            Equal(scenario!="normal",File.Exists(Path.Combine(root,"addins/BimAgentLab.ReadinessBridge.addin")));
            if(scenario!="normal") Equal(File.ReadAllText(Path.Combine(root,"source/third-party/mcp/csharp-sdk-LICENSE.txt")),File.ReadAllText(Path.Combine(DeploymentServerRoot(root),"third-party/csharp-sdk-LICENSE.txt")));
        }
        if(recoveryFault) {
            Equal(91,await DeploymentRun(root,"Restore",scenario,boundary,fault));
            var receipt=File.ReadAllText(Path.Combine(root,"receipt-path.txt"));File.AppendAllText(Path.Combine(Path.GetDirectoryName(receipt)!,"journal.jsonl"),"{incomplete");
        }
        Equal(0,await DeploymentRun(root,"Undo",undo:true));DeploymentCompare(before,root);
        Equal(0,await DeploymentRun(root,"Undo",undo:true));DeploymentCompare(before,root);
    }
    static async Task DeploymentConflict(string scenario)
    {
        string root=await DeploymentSeed("existing");var before=DeploymentSnapshot(root);
        if(scenario=="backup-failure") {
            if(await DeploymentRun(root,"Apply","existing","Backup","throw")==0)throw new Exception("Expected preparation failure");
            DeploymentCompare(before,root);return;
        }
        Equal(0,await DeploymentRun(root,scenario is "foreign" or "missing-last-backup" or "foreign-temp"?"Apply":"Prepare"));
        string receipt=File.ReadAllText(Path.Combine(root,"receipt-path.txt"));string folder=Path.GetDirectoryName(receipt)!;
        if(scenario=="pending") {if(await DeploymentRun(root,"Apply")==0)throw new Exception("Pending receipt ignored");DeploymentCompare(before,root);return;}
        if(scenario=="foreign")File.WriteAllText(Path.Combine(root,"addins/BimAgentLab.RequiredInformation/BimAgentLab.Readiness.Core.dll"),"foreign changed contents");
        if(scenario=="missing-backup")File.Delete(Directory.GetFiles(Path.Combine(folder,"backup"))[0]);
        if(scenario=="missing-last-backup")File.Delete(Directory.GetFiles(Path.Combine(folder,"backup")).Last());
        if(scenario=="prepared-directory") {
            using var json=JsonDocument.Parse(File.ReadAllText(receipt));
            Directory.CreateDirectory(json.RootElement.GetProperty("CreatedDirectories")[0].GetString()!);
        }
        if(scenario=="foreign-temp") {
            using var json=JsonDocument.Parse(File.ReadAllText(receipt));
            File.WriteAllText(json.RootElement.GetProperty("Operations")[0].GetProperty("Temporary").GetString()!,"foreign temporary bytes");
        }
        if(scenario=="changed-stage")File.WriteAllText(Directory.GetFiles(Path.Combine(folder,"staged"))[0],"foreign stage");
        if(scenario=="changed-after-preflight")File.WriteAllText(Path.Combine(root,"addins/BimAgentLab.RequiredInformation/BimAgentLab.Readiness.Core.dll"),"foreign pre-apply");
        var changed=DeploymentSnapshot(root);
        if(await DeploymentRun(root,"Undo",undo:true)==0)throw new Exception("Recovery must report conflict");
        DeploymentCompare(changed,root);
    }
    static async Task DeploymentPreparationExit()
    {
        string root=await DeploymentSeed("existing");var before=DeploymentSnapshot(root);
        Equal(91,await DeploymentRun(root,"Apply","existing","Backup","exit"));DeploymentCompare(before,root);
        Equal(0,await DeploymentRun(root,"Apply"));Equal(0,await DeploymentRun(root,"Undo",undo:true));DeploymentCompare(before,root);
    }
    static async Task DeploymentLease()
    {
        string root=await DeploymentSeed("existing");var before=DeploymentSnapshot(root);
        var holder=DeploymentRun(root,"HoldLease");
        try {
            var limit=DateTime.UtcNow.AddSeconds(10);
            while(!File.Exists(Path.Combine(root,"lease-held.txt"))) {if(DateTime.UtcNow>limit)throw new Exception("Lease owner did not start");await Task.Delay(20);}
            using var globalMutex=Mutex.OpenExisting("Global\\BimAgentLab.Install."+System.Security.Principal.WindowsIdentity.GetCurrent().User!.Value);
            if(await DeploymentRun(root,"Apply")==0)throw new Exception("Concurrent owner admitted");DeploymentCompare(before,root);
        }finally {File.WriteAllText(Path.Combine(root,"release-lease.txt"),"release");await holder;}
        Equal(0,await DeploymentRun(root,"DisposedLease"));DeploymentCompare(before,root);
        Equal(0,await DeploymentRun(root,"Preflight"));DeploymentCompare(before,root);
    }
}
