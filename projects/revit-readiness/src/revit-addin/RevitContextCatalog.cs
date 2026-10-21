using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimAgentLab.Readiness.Core;
using BimAgentLab.RevitBridge.Contract;
using BimAgentLab.RevitBridge.Runtime;

namespace BimAgentLab.RevitReadiness
{
    internal sealed class RevitContextSnapshot
    {
        internal Document Document;
        internal Checklist Checklist;
        internal ContextBinding Binding;
        internal string Level, Phase, RevitBuild;
    }
    internal sealed class RevitContextCatalog
    {
        readonly string sessionId;
        readonly int apiThread = Thread.CurrentThread.ManagedThreadId;
        readonly List<OpenDocument> documents = new List<OpenDocument>();
        sealed class OpenDocument { internal Document Document; internal string Path, Lifetime; }
        internal RevitContextCatalog(string sessionId) { this.sessionId = sessionId; }
        internal RevitContextSnapshot Capture(UIApplication application)
        {
            RequireApiThread(); InvalidateClosedDocuments();
            var document = application.ActiveUIDocument?.Document;
            if (document == null) throw Failure("NoProject", "Open a saved project before requesting readiness context.");
            if (document.IsFamilyDocument || document.IsLinked) throw Failure("UnsupportedDocument", "Use a host project document.");
            if (string.IsNullOrWhiteSpace(document.PathName)) throw Failure("UnsavedProject", "Save the project and supply its checklist.");
            string path;
            try
            {
                if (!Path.IsPathRooted(document.PathName)) throw new ArgumentException();
                path = Path.GetFullPath(document.PathName);
            }
            catch (ArgumentException) { throw Failure("UnsupportedDocument", "Use a saved filesystem project with a local checklist sidecar."); }
            var identity = documents.FirstOrDefault(d => d.Document.Equals(document));
            if (identity == null)
            { identity = new OpenDocument { Document = document, Path = path, Lifetime = Guid.NewGuid().ToString("N") }; documents.Add(identity); }
            else if (!string.Equals(identity.Path, path, StringComparison.OrdinalIgnoreCase))
            { identity.Path = path; identity.Lifetime = Guid.NewGuid().ToString("N"); }
            string checklistPath = Path.ChangeExtension(path, ".checklist.json");
            ChecklistSnapshot bytes;
            try { bytes = ChecklistSnapshot.Read(checklistPath); }
            catch (FileNotFoundException) { throw Failure("ChecklistMissing", "Supply the saved project's .checklist.json sidecar."); }
            catch (DirectoryNotFoundException) { throw Failure("ChecklistMissing", "The saved project's sidecar directory is unavailable."); }
            catch (Exception error) when (error is InvalidDataException || error is IOException || error is UnauthorizedAccessException)
            { throw Failure("ChecklistInvalid", "The checklist cannot be read within the four MiB limit."); }
            var checklist = ReadinessPayloadCodec.DecodeChecklist(bytes);
            if (checklist.ProjectUniqueId != document.ProjectInformation.UniqueId ||
                !string.Equals(checklist.ProjectFileName, Path.GetFileName(path), StringComparison.OrdinalIgnoreCase))
                throw Failure("ChecklistMismatch", "The checklist belongs to a different project or file.");
            var level = document.GetElement(checklist.LevelUniqueId) as Level;
            var phase = document.GetElement(checklist.PhaseUniqueId) as Phase;
            if (level == null || phase == null) throw Failure("ChecklistMismatch", "Checklist level or phase is absent from this project.");
            return new RevitContextSnapshot { Document = document, Checklist = checklist, Level = level.Name, Phase = phase.Name,
                RevitBuild = application.Application.VersionBuild,
                Binding = new ContextBinding { BridgeSessionId = sessionId, DocumentLifetimeId = identity.Lifetime,
                    ProjectUniqueId = document.ProjectInformation.UniqueId, DocumentPath = path,
                    ChecklistId = checklist.ChecklistId, ChecklistPath = checklistPath, ChecklistSha256 = bytes.Sha256 } };
        }
        internal void InvalidateClosedDocuments()
        { RequireApiThread(); documents.RemoveAll(d => !d.Document.IsValidObject); }
        internal void InvalidateSavedAs(Document document)
        { RequireApiThread(); documents.RemoveAll(d => !d.Document.IsValidObject || d.Document.Equals(document)); }
        internal void Clear() { RequireApiThread(); documents.Clear(); }
        void RequireApiThread()
        { if (Thread.CurrentThread.ManagedThreadId != apiThread) throw new InvalidOperationException("Document catalog requires Revit API context."); }
        internal static BridgeException Failure(string code, string message) => new BridgeException(new BridgeError { Code = code, Message = message, Retryable = false });
    }
}
