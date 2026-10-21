using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;
using BimAgentLab.RevitBridge.Runtime;

namespace BimAgentLab.Bridge.Tests;

internal static partial class Program
{
    static async Task PipeTests()
    {
        await Test("Catalog refuses zero or ambiguous instances and honors explicit PID", () => Sync(() =>
        {
            Equal("BridgeUnavailable", Throws<BridgeException>(() => new SessionCatalog(() => Array.Empty<BridgeSessionDescriptor>()).Resolve(null)).Error.Code);
            var one = ManualDescriptor(); var two = ManualDescriptor(); two.RevitPid++;
            var catalog = new SessionCatalog(() => new[] { one, two });
            Equal("AmbiguousRevitInstance", Throws<BridgeException>(() => catalog.Resolve(null)).Error.Code);
            Equal(one.BridgeSessionId, catalog.Resolve(one.RevitPid).BridgeSessionId);
            Equal("BridgeUnavailable", Throws<BridgeException>(() => catalog.Resolve(int.MaxValue)).Error.Code);
        }));
        await Test("Native identity verifies process start, logon, session and pipe identity", () => Sync(() =>
        {
            var descriptor = ManualDescriptor(); BridgeIdentity.ValidateLive(descriptor);
            foreach (Action<BridgeSessionDescriptor> change in new Action<BridgeSessionDescriptor>[] {
                d => d.RevitPid = int.MaxValue, d => d.ProcessStartUtc = "2000-01-01T00:00:00.0000000Z",
                d => d.WindowsSessionId++, d => d.LogonSid = "S-1-5-5-0-0", d => d.PipeName = "foreign",
                d => d.ContractVersion = 2, d => d.BridgeSessionId = "../foreign" })
            {
                var invalid = ManualDescriptor(); change(invalid);
                // Keep advertised naming consistent so PID/start/session/logon cases
                // reach the actual OS identity check instead of only failing shape.
                if (invalid.PipeName != "foreign") invalid.PipeName = TestPipeName(invalid);
                Equal("BridgeUnavailable", Throws<BridgeException>(() => BridgeIdentity.ValidateLive(invalid)).Error.Code);
            }
        }));
        await Test("Published descriptors are restricted, selected and removed only by their owner", () => Sync(() =>
        {
            string dir = Scratch("catalog"); var catalog = new SessionCatalog(dir);
            var own = ManualDescriptor(); var other = ManualDescriptor(); var second = new SessionCatalog(dir);
            catalog.Publish(own);
            Equal(own.BridgeSessionId, new SessionCatalog(dir).Resolve(own.RevitPid).BridgeSessionId);
            string path = Path.Combine(dir, own.BridgeSessionId + ".json");
#if NET48
            var security = File.GetAccessControl(path);
#else
            var security = System.IO.FileSystemAclExtensions.GetAccessControl(new FileInfo(path));
#endif
            var rules = security.GetAccessRules(true, true, typeof(SecurityIdentifier));
            Equal(1, rules.Count);
            Equal(own.LogonSid, rules[0]!.IdentityReference.Value);
            Check(security.AreAccessRulesProtected, "Descriptor must not inherit broad access");
            Throws<IOException>(() => second.Publish(own));
            second.Publish(other);
            Equal("AmbiguousRevitInstance", Throws<BridgeException>(() => catalog.Resolve(own.RevitPid)).Error.Code);
            catalog.RemoveOwned(own);
            Equal(other.BridgeSessionId, catalog.Resolve(null).BridgeSessionId);
            second.RemoveOwned(other);
            Equal(0, Directory.GetFiles(dir).Length);
        }));
        await Test("Stale and changed descriptors cannot be selected or silently deleted", () => Sync(() =>
        {
            string dir = Scratch("changed"); var catalog = new SessionCatalog(dir); var own = ManualDescriptor();
            catalog.Publish(own); string path = Path.Combine(dir, own.BridgeSessionId + ".json");
            byte[] before = File.ReadAllBytes(path);
            File.WriteAllText(path, "{}");
            Equal("BridgeUnavailable", Throws<BridgeException>(() => new SessionCatalog(dir).Resolve(null)).Error.Code);
            Throws<IOException>(() => catalog.RemoveOwned(own)); Equal("{}", File.ReadAllText(path));
            File.WriteAllBytes(path, before);
            using (var locked = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                Throws<IOException>(() => catalog.RemoveOwned(own));
            Check(File.Exists(path), "Failed cleanup must preserve evidence for retry");
            catalog.RemoveOwned(own);
            Equal(0, Directory.GetFiles(dir).Length);
            File.WriteAllBytes(path, before); // newly inherited broad ACL, same otherwise-valid descriptor
            Equal("BridgeUnavailable", Throws<BridgeException>(() => new SessionCatalog(dir).Resolve(null)).Error.Code);
            File.Delete(path);
        }));
        await PipeTransportTests();
    }
    static string Scratch(string name)
    {
        string path = Path.Combine(Path.GetFullPath("samples/local/pipe-tests"), Guid.NewGuid().ToString("N"), name);
        Directory.CreateDirectory(path); return path;
    }
    static BridgeSessionDescriptor ManualDescriptor()
    {
        using var process = Process.GetCurrentProcess();
        using var identity = WindowsIdentity.GetCurrent();
        var descriptor = new BridgeSessionDescriptor { ContractVersion = 1, RevitPid = process.Id,
            ProcessStartUtc = process.StartTime.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", System.Globalization.CultureInfo.InvariantCulture),
            WindowsSessionId = process.SessionId, LogonSid = ReadTestLogonSid(identity.Token),
            BridgeSessionId = Guid.NewGuid().ToString("N") };
        descriptor.PipeName = TestPipeName(descriptor);
        return descriptor;
    }
    static string TestPipeName(BridgeSessionDescriptor descriptor) => "BimAgentLab.Revit." + descriptor.RevitPid + "." +
        DateTimeOffset.ParseExact(descriptor.ProcessStartUtc, "yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeUniversal).UtcDateTime.Ticks + "." + descriptor.WindowsSessionId + "." + descriptor.LogonSid + "." + descriptor.BridgeSessionId;
    // Independent oracle: TokenLogonSid (28), not WindowsIdentity.Groups, which omits it.
    static string ReadTestLogonSid(IntPtr token)
    {
        GetTokenInformation(token, 28, IntPtr.Zero, 0, out int size);
        IntPtr buffer = Marshal.AllocHGlobal(size);
        try
        {
            Check(GetTokenInformation(token, 28, buffer, size, out _), "TokenLogonSid query failed");
            Equal(1, Marshal.ReadInt32(buffer));
            return new SecurityIdentifier(Marshal.ReadIntPtr(buffer, IntPtr.Size == 8 ? 8 : 4)).Value;
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }
    [DllImport("advapi32.dll", SetLastError = true)]
    static extern bool GetTokenInformation(IntPtr token, int kind, IntPtr buffer, int length, out int returned);
}
