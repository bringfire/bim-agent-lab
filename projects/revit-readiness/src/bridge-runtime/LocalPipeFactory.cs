using System;
using System.IO.Pipes;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Win32.SafeHandles;
namespace BimAgentLab.RevitBridge.Runtime;
public static class LocalPipeFactory
{
    public static NamedPipeServerStream Create(string pipeName, string logonSid) => Create(pipeName, logonSid, false);
    internal static NamedPipeServerStream Create(string pipeName, string logonSid, bool first)
    {
        if (!Regex.IsMatch(pipeName ?? "", "\\ABimAgentLab\\.Revit\\.[A-Za-z0-9.-]{1,220}\\z")) throw new ArgumentException("Invalid local pipe name.");
        using var security = new NativeWindows.SecurityDescriptor(logonSid);
        // DUPLEX | OVERLAPPED [| FIRST_PIPE_INSTANCE], BYTE/WAIT | REJECT_REMOTE_CLIENTS.
        var handle = CreateNamedPipe("\\\\.\\pipe\\" + pipeName, 3u | 0x40000000u | (first ? 0x80000u : 0), 8, 8, 8192, 8192, 0, ref security.Attributes);
        if (handle.IsInvalid) { int error = Marshal.GetLastWin32Error(); handle.Dispose(); throw new Win32Exception(error); }
        try { return new NamedPipeServerStream(PipeDirection.InOut, true, false, handle); }
        catch { handle.Dispose(); throw; } // success transfers the owned SafePipeHandle to the stream
    }
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern SafePipeHandle CreateNamedPipe(string name, uint openMode, uint pipeMode, uint maxInstances, uint outputBytes, uint inputBytes, uint timeout, ref NativeWindows.SecurityAttributes security);
}
