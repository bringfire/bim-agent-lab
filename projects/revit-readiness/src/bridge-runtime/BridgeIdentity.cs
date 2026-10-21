using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using BimAgentLab.RevitBridge.Contract;

namespace BimAgentLab.RevitBridge.Runtime;

public static class BridgeIdentity
{
    public static BridgeSessionDescriptor CreateCurrent()
    {
        using var process = Process.GetCurrentProcess();
        var value = new BridgeSessionDescriptor { ContractVersion = 1, RevitPid = process.Id,
            ProcessStartUtc = process.StartTime.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", CultureInfo.InvariantCulture),
            WindowsSessionId = process.SessionId, LogonSid = NativeWindows.LogonSid(process.Id), BridgeSessionId = Guid.NewGuid().ToString("N") };
        value.PipeName = PipeName(value); return value;
    }
    internal static void ValidateShape(BridgeSessionDescriptor descriptor)
    {
        if (descriptor == null || descriptor.ContractVersion != 1 || descriptor.RevitPid <= 0 || descriptor.WindowsSessionId < 0 ||
            !Regex.IsMatch(descriptor.BridgeSessionId ?? "", "\\A[0-9a-f]{32}\\z") ||
            !Regex.IsMatch(descriptor.LogonSid ?? "", "\\AS-1-5-5-[0-9]+-[0-9]+\\z") || descriptor.PipeName != PipeName(descriptor))
            throw new InvalidDataException("Invalid bridge session descriptor.");
    }
    public static void ValidateLive(BridgeSessionDescriptor descriptor)
    {
        try
        {
            ValidateShape(descriptor);
            using var process = Process.GetProcessById(descriptor.RevitPid);
            using var current = Process.GetCurrentProcess();
            if (process.HasExited || process.StartTime.ToUniversalTime() != BridgeJson.ParseUtc(descriptor.ProcessStartUtc).UtcDateTime ||
                process.SessionId != descriptor.WindowsSessionId || process.SessionId != current.SessionId ||
                NativeWindows.LogonSid(process.Id) != descriptor.LogonSid || NativeWindows.LogonSid(current.Id) != descriptor.LogonSid)
                throw new InvalidDataException("Bridge process identity no longer matches.");
        }
        catch (Exception error) when (error is ArgumentException || error is InvalidOperationException || error is IOException || error is InvalidDataException ||
            error is System.ComponentModel.Win32Exception || error is UnauthorizedAccessException || error is System.Security.SecurityException)
        { throw BridgeFailures.Create("BridgeUnavailable", "The selected bridge identity is not available."); }
    }
    internal static string PipeName(BridgeSessionDescriptor descriptor) => "BimAgentLab.Revit." +
        descriptor.RevitPid.ToString(CultureInfo.InvariantCulture) + "." + BridgeJson.ParseUtc(descriptor.ProcessStartUtc).UtcTicks.ToString(CultureInfo.InvariantCulture) + "." +
        descriptor.WindowsSessionId.ToString(CultureInfo.InvariantCulture) + "." + descriptor.LogonSid + "." + descriptor.BridgeSessionId;
}

internal static class BridgeFailures
{
    internal static BridgeException Create(string code, string message, string requestId = "") => new BridgeException(new BridgeError
    { Code = code, Message = message, RequestId = requestId, Retryable = code == "BridgeUnavailable" || code == "Busy" });
}
