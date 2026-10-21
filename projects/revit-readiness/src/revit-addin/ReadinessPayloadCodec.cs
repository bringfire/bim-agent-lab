using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Script.Serialization;
using BimAgentLab.Readiness.Core;
using BimAgentLab.RevitBridge.Contract;

namespace BimAgentLab.RevitReadiness
{
    // Separate from Autodesk types so the actual net48 conversion can be tested offline.
    public static class ReadinessPayloadCodec
    {
        public static Checklist DecodeChecklist(BimAgentLab.RevitBridge.Runtime.ChecklistSnapshot snapshot)
        {
            try { return ParseChecklist(snapshot.Text); }
            catch (DecoderFallbackException)
            { throw new BridgeException(new BridgeError { Code = "ChecklistInvalid", Message = "The checklist must contain valid UTF-8.", Retryable = false }); }
        }
        public static Checklist ParseChecklist(string text)
        {
            try
            {
                // Syntax check only; never use JavaScriptSerializer's date-coercing values.
                new JavaScriptSerializer { MaxJsonLength = 4 * 1024 * 1024, RecursionLimit = 64 }
                    .DeserializeObject(text.Replace("\"\\/Date(", "\"/Date("));
                using (var stream = new MemoryStream(new UTF8Encoding(false, true).GetBytes(text)))
                {
                    var result = (Checklist)new DataContractJsonSerializer(typeof(Checklist)).ReadObject(stream);
                    if (result == null) throw new ArgumentException("Checklist required.");
                    result.Validate(); return result;
                }
            }
            catch (Exception error) when (error is ArgumentException || error is InvalidOperationException || error is SerializationException || error is FormatException || error is System.Xml.XmlException)
            { throw new BridgeException(new BridgeError { Code = "ChecklistInvalid", Message = "The checklist JSON/schema is invalid. Validate its identities and explicit rules.", Retryable = false }); }
        }
        public static string SerializeReport(InformationReport report)
        {
            try { return new JavaScriptSerializer { MaxJsonLength = 4 * 1024 * 1024 }.Serialize(report); }
            catch (InvalidOperationException)
            { throw new BridgeException(new BridgeError { Code = "ResponseTooLarge", Message = "The complete report exceeds the response limit.", Retryable = false }); }
        }
    }
}
