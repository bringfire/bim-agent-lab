using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
#if NET48
using System.Web.Script.Serialization;
#else
using System.Text.Json;
using System.Text.Encodings.Web;
#endif

namespace BimAgentLab.Readiness.Core
{
    public sealed class Checklist
    {
        public int SchemaVersion { get; set; }
        public string ChecklistId { get; set; }
        public string ProjectUniqueId { get; set; }
        public string ProjectFileName { get; set; }
        public string LevelUniqueId { get; set; }
        public string PhaseUniqueId { get; set; }
        public List<ElementReference> Cases { get; set; }
        public List<TypeRequirement> Requirements { get; set; }

        public void Validate()
        {
            if (SchemaVersion != 1 || string.IsNullOrWhiteSpace(ChecklistId) || string.IsNullOrWhiteSpace(ProjectUniqueId) ||
                string.IsNullOrWhiteSpace(ProjectFileName) || string.IsNullOrWhiteSpace(LevelUniqueId) || string.IsNullOrWhiteSpace(PhaseUniqueId))
                throw new ArgumentException("Checklist schema, identity, document, level, and phase must be explicit.");
            if (Cases == null || Cases.Count == 0 || Cases.Any(c => c == null || string.IsNullOrWhiteSpace(c.UniqueId) || string.IsNullOrWhiteSpace(c.CaseId)) ||
                Cases.Select(c => c.UniqueId).Distinct().Count() != Cases.Count || Cases.Select(c => c.CaseId).Distinct().Count() != Cases.Count)
                throw new ArgumentException("Checklist cases must have unique element and case identities.");
            TypeInformation.Evaluate(new TypeObservation[0], Requirements);
        }
    }
    public static class JsonFile
    {
#if !NET48
        // These files are JSON data, never embedded in HTML. Preserve literal Unicode
        // and retain the legacy case-insensitive property binding for manual inputs.
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true, IncludeFields = true, MaxDepth = 64,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
#endif
        public static T Read<T>(string path)
        {
            if (new FileInfo(path).Length > 4 * 1024 * 1024) throw new ArgumentException("JSON input exceeds the 4 MiB limit.");
#if NET48
            return new JavaScriptSerializer { MaxJsonLength = 4 * 1024 * 1024 }.Deserialize<T>(File.ReadAllText(path));
#else
            return JsonSerializer.Deserialize<T>(File.ReadAllText(path), Options);
#endif
        }
        public static void Write(string path, object value)
        {
#if NET48
            File.WriteAllText(path, new JavaScriptSerializer { MaxJsonLength = int.MaxValue }.Serialize(value), new UTF8Encoding(false));
#else
            File.WriteAllText(path, JsonSerializer.Serialize(value, value == null ? typeof(object) : value.GetType(), Options), new UTF8Encoding(false));
#endif
        }
    }
}
