using System;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

namespace BimAgentLab.RevitBridge.Runtime;

// Original narrow interop for Windows token identity and restricted descriptor files.
// Win32 constants/contracts are documented in the Task 3 research record.
internal static class NativeWindows
{
    internal static void ValidateClient(NamedPipeServerStream pipe, string logonSid, int sessionId)
    {
        if (!GetNamedPipeClientProcessId(pipe.SafePipeHandle, out uint pid)) throw new Win32Exception(Marshal.GetLastWin32Error());
        using var process = System.Diagnostics.Process.GetProcessById(checked((int)pid));
        if (process.SessionId != sessionId || LogonSid(process.Id) != logonSid) throw new UnauthorizedAccessException("Foreign pipe client.");
    }
    internal static void ValidateServer(NamedPipeClientStream pipe, int expectedPid)
    {
        if (!GetNamedPipeServerProcessId(pipe.SafePipeHandle, out uint pid)) throw new Win32Exception(Marshal.GetLastWin32Error());
        if (pid != expectedPid) throw new UnauthorizedAccessException("Foreign pipe server.");
    }
    internal static string LogonSid(int pid)
    {
        using var process = OpenProcess(0x1000, false, pid); // QUERY_LIMITED_INFORMATION
        if (process.IsInvalid) throw new Win32Exception(Marshal.GetLastWin32Error());
        if (!OpenProcessToken(process, 8, out var token)) throw new Win32Exception(Marshal.GetLastWin32Error());
        using (token)
        {
            GetTokenInformation(token, 2, IntPtr.Zero, 0, out int length); // TokenGroups
            if (length <= 0 || length > 1024 * 1024) throw new InvalidDataException("Invalid token group buffer.");
            IntPtr buffer = Marshal.AllocHGlobal(length);
            try
            {
                if (!GetTokenInformation(token, 2, buffer, length, out _)) throw new Win32Exception(Marshal.GetLastWin32Error());
                int count = Marshal.ReadInt32(buffer), offset = IntPtr.Size == 8 ? 8 : 4;
                int stride = IntPtr.Size == 8 ? 16 : 8;
                if (count < 0 || count > (length - offset) / stride) throw new InvalidDataException("Invalid token groups.");
                for (int i = 0; i < count; i++)
                {
                    IntPtr group = IntPtr.Add(buffer, offset + i * stride);
                    if ((unchecked((uint)Marshal.ReadInt32(group, IntPtr.Size)) & 0xc0000000u) == 0xc0000000u)
                        return new SecurityIdentifier(Marshal.ReadIntPtr(group)).Value; // SE_GROUP_LOGON_ID
                }
                throw new UnauthorizedAccessException("Process has no logon SID.");
            }
            finally { Marshal.FreeHGlobal(buffer); }
        }
    }
    internal sealed class SecurityDescriptor : IDisposable
    {
        internal SecurityAttributes Attributes;
        internal SecurityDescriptor(string sid)
        {
            _ = new SecurityIdentifier(sid);
            if (!ConvertStringSecurityDescriptorToSecurityDescriptor("D:P(A;;GA;;;" + sid + ")", 1, out IntPtr value, out _))
                throw new Win32Exception(Marshal.GetLastWin32Error());
            Attributes = new SecurityAttributes { Length = Marshal.SizeOf(typeof(SecurityAttributes)), Descriptor = value };
        }
        public void Dispose() { if (Attributes.Descriptor != IntPtr.Zero) { LocalFree(Attributes.Descriptor); Attributes.Descriptor = IntPtr.Zero; } }
    }
    internal static void CreateRestrictedFile(string path, byte[] data, string sid)
    {
        using var security = new SecurityDescriptor(sid);
        using var handle = CreateFile(path, 0x40000000, 0, ref security.Attributes, 1, 0x80, IntPtr.Zero); // WRITE, CREATE_NEW
        if (handle.IsInvalid) throw new IOException("Cannot create private descriptor.", new Win32Exception(Marshal.GetLastWin32Error()));
        using var stream = new FileStream(handle, FileAccess.Write);
        stream.Write(data, 0, data.Length); stream.Flush(true);
    }
    internal static void ValidateFileAccess(string path, string sid)
    {
#if NET48
        var security = File.GetAccessControl(path);
#else
        var security = FileSystemAclExtensions.GetAccessControl(new FileInfo(path));
#endif
        var rules = security.GetAccessRules(true, true, typeof(SecurityIdentifier));
        if (!security.AreAccessRulesProtected || rules.Count != 1 ||
            !(rules[0] is FileSystemAccessRule rule) || rule.IdentityReference.Value != sid ||
            rule.AccessControlType != AccessControlType.Allow || rule.IsInherited)
            throw new UnauthorizedAccessException("Descriptor access is not restricted to this logon.");
    }
    internal static void DeleteOwnedFile(string path, byte[] expected)
    {
        var security = new SecurityAttributes { Length = Marshal.SizeOf(typeof(SecurityAttributes)) };
        using var handle = CreateFile(path, 0x80010000, 0, ref security, 3, 0x80, IntPtr.Zero); // READ | DELETE, OPEN_EXISTING
        if (handle.IsInvalid)
        {
            int error = Marshal.GetLastWin32Error(); if (error == 2) return;
            throw new IOException("Cannot acquire descriptor for cleanup.", new Win32Exception(error));
        }
        // Exclusive handle covers comparison AND delete disposition; no compare/delete gap.
        using var stream = new FileStream(handle, FileAccess.Read);
        if (stream.Length != expected.Length) throw new IOException("Descriptor changed; preserving it.");
        var actual = new byte[expected.Length]; int read = 0;
        while (read < actual.Length) { int n = stream.Read(actual, read, actual.Length - read); if (n == 0) break; read += n; }
        if (read != actual.Length || !actual.SequenceEqual(expected)) throw new IOException("Descriptor changed; preserving it.");
        byte delete = 1;
        if (!SetFileInformationByHandle(handle, 4, ref delete, 1)) throw new IOException("Descriptor removal failed.", new Win32Exception(Marshal.GetLastWin32Error()));
    }
    [StructLayout(LayoutKind.Sequential)] internal struct SecurityAttributes
    { public int Length; public IntPtr Descriptor; [MarshalAs(UnmanagedType.Bool)] public bool Inherit; }
    [DllImport("kernel32.dll", SetLastError = true)] static extern SafeFileHandle OpenProcess(uint access, bool inherit, int pid);
    [DllImport("advapi32.dll", SetLastError = true)] static extern bool OpenProcessToken(SafeFileHandle process, uint access, out SafeFileHandle token);
    [DllImport("advapi32.dll", SetLastError = true)] static extern bool GetTokenInformation(SafeFileHandle token, int kind, IntPtr buffer, int length, out int returned);
    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)] static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(string sddl, uint revision, out IntPtr descriptor, out uint size);
    [DllImport("kernel32.dll")] static extern IntPtr LocalFree(IntPtr value);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] static extern SafeFileHandle CreateFile(string path, uint access, uint sharing, ref SecurityAttributes security, uint creation, uint flags, IntPtr template);
    [DllImport("kernel32.dll", SetLastError = true)] static extern bool SetFileInformationByHandle(SafeFileHandle file, int kind, ref byte info, uint size);
    [DllImport("kernel32.dll", SetLastError = true)] static extern bool GetNamedPipeClientProcessId(SafePipeHandle pipe, out uint pid);
    [DllImport("kernel32.dll", SetLastError = true)] static extern bool GetNamedPipeServerProcessId(SafePipeHandle pipe, out uint pid);
}
