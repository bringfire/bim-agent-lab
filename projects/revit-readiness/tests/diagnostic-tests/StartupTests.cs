using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using BimAgentLab.RevitReadiness;

internal static partial class Program
{
    static void RootIsOutsideAppData()
    {
        string root = Path.GetFullPath(DiagnosticRunDirectory.Root);
        foreach (var folder in new[] { Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolder.ApplicationData })
            Require(!root.StartsWith(Environment.GetFolderPath(folder) + "\\", StringComparison.OrdinalIgnoreCase),
                "Shared diagnostic controls must not depend on a virtualized AppData view: " + root);
    }
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern uint GetFinalPathNameByHandle(SafeFileHandle file, StringBuilder path, uint size, uint flags);
    static void SharedFileResolvesToRequestedPath()
    {
        Directory.CreateDirectory(DiagnosticRunDirectory.Root);
        string path = Path.Combine(DiagnosticRunDirectory.Root, "visibility-test-" + Guid.NewGuid().ToString("N") + ".tmp");
        using (var file = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.DeleteOnClose))
        {
            var actual = new StringBuilder(4096);
            uint size = GetFinalPathNameByHandle(file.SafeFileHandle, actual, 4096, 0);
            Require(size > 0 && size < 4096, "Cannot resolve actual open file handle");
            Require(string.Equals("\\\\?\\" + Path.GetFullPath(path), actual.ToString(), StringComparison.OrdinalIgnoreCase),
                "Requested path resolves into another filesystem view: " + actual);
        }
    }
    static string StartupFixture(bool prepare = true)
    {
        string root = Path.Combine(Path.GetTempPath(), "bal-startup-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string run = Guid.NewGuid().ToString("N");
        if (prepare) Directory.CreateDirectory(Path.Combine(root, run));
        File.WriteAllText(Path.Combine(root, "run.txt"), run);
        return root;
    }
    static void ExpectStartupFailure<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new Exception("Expected " + typeof(T).Name);
    }
    static void ValidPreparedRun()
    {
        string root = StartupFixture();
        string created = DiagnosticRunDirectory.CreateProcessDirectory(root, 123);
        Require(Directory.Exists(created) && created == Path.Combine(root, File.ReadAllText(Path.Combine(root, "run.txt")), "123"),
            "Prepared run did not retain its identity and process directory");
    }
    static void MissingSelector()
    {
        string root = StartupFixture(); File.Delete(Path.Combine(root, "run.txt"));
        ExpectStartupFailure<FileNotFoundException>(() => DiagnosticRunDirectory.CreateProcessDirectory(root, 123));
        Require(Directory.GetDirectories(root, "123", SearchOption.AllDirectories).Length == 0, "Missing selector created a capture");
    }
    static void InvalidSelector()
    {
        string root = StartupFixture(); File.WriteAllText(Path.Combine(root, "run.txt"), "..\\foreign");
        ExpectStartupFailure<InvalidDataException>(() => DiagnosticRunDirectory.CreateProcessDirectory(root, 123));
        Require(Directory.GetDirectories(root, "123", SearchOption.AllDirectories).Length == 0, "Invalid selector created a capture");
    }
    static void MissingPreparedDirectory()
    {
        string root = StartupFixture(false);
        ExpectStartupFailure<DirectoryNotFoundException>(() => DiagnosticRunDirectory.CreateProcessDirectory(root, 123));
        Require(Directory.GetDirectories(root).Length == 0, "Missing prepared run was silently created");
    }
    static void ExistingProcessDirectory()
    {
        string root = StartupFixture(); string created = DiagnosticRunDirectory.CreateProcessDirectory(root, 123);
        File.WriteAllText(Path.Combine(created, "events.tsv"), "retained evidence");
        ExpectStartupFailure<InvalidOperationException>(() => DiagnosticRunDirectory.CreateProcessDirectory(root, 123));
        Require(File.ReadAllText(Path.Combine(created, "events.tsv")) == "retained evidence", "Existing evidence was changed");
    }
}
