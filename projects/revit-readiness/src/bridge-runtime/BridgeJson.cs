using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;
using System.Xml.Linq;
#if NET48
using System.Web.Script.Serialization;
#else
using System.Text.Json;
#endif
using BimAgentLab.RevitBridge.Contract;

namespace BimAgentLab.RevitBridge.Runtime;

public static class BridgeJson
{
    static readonly string[] UtcFormats = new[] { "yyyy-MM-dd'T'HH:mm:ss'Z'" }
        .Concat(Enumerable.Range(1, 7).Select(n => "yyyy-MM-dd'T'HH:mm:ss." + new string('f', n) + "'Z'")).ToArray();
    public static byte[] Serialize<T>(T value)
    {
        int limit = LimitFor(typeof(T));
        ValidateValue(value);
        using var stream = new BoundedOutput(limit);
        try { new DataContractJsonSerializer(typeof(T)).WriteObject(stream, value); }
        catch (Exception) when (stream.LimitExceeded) { throw new PayloadTooLargeException(); }
        byte[] bytes = stream.ToArray();
        FrameCodec.ValidateUtf8(bytes);
        return bytes;
    }

    public static T Deserialize<T>(byte[] bytes)
    {
        if (bytes == null) throw new ArgumentNullException(nameof(bytes));
        int limit = LimitFor(typeof(T));
        if (bytes.Length == 0 || bytes.Length > limit) throw new InvalidDataException("Invalid bridge JSON size.");
        FrameCodec.ValidateUtf8(bytes);
        try
        {
            // Validate original tokens first. Framework DTO conversion may reinterpret
            // valid strings (notably Microsoft /Date(...)/ spellings on net48).
            XElement root;
            var quotas = new XmlDictionaryReaderQuotas
            { MaxDepth = 8, MaxStringContentLength = limit, MaxArrayLength = limit, MaxBytesPerRead = 4096, MaxNameTableCharCount = 16384 };
            using (var reader = JsonReaderWriterFactory.CreateJsonReader(bytes, quotas))
            {
                var document = XDocument.Load(reader);
                root = document.Root ?? throw new InvalidDataException("Missing bridge object.");
                ValidateShape(root, typeof(T));
            }
            // The infoset reader alone accepts trailing objects/commas. A separate
            // complete-input syntax check is necessary; never use its converted values.
#if NET48
            var serializer = new JavaScriptSerializer { MaxJsonLength = limit, RecursionLimit = 8 };
            // These two slash spellings are equivalent JSON string data. Normalize only
            // this prefix in a throwaway syntax-check copy to bypass automatic date
            // interpretation, which can also overflow for perfectly valid string data.
            string syntaxOnly = FrameCodec.StrictUtf8.GetString(bytes).Replace("\"\\/Date(", "\"/Date(");
            serializer.DeserializeObject(syntaxOnly);
#else
            using var syntaxOnly = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 8 });
#endif
            T value = (T)ReadValidatedObject(root, typeof(T));
            ValidateValue(value);
            return value;
        }
        catch (XmlException error) { throw new InvalidDataException("Malformed bridge JSON.", error); }
        catch (FormatException error) { throw new InvalidDataException("Malformed bridge JSON.", error); }
        catch (SerializationException error) { throw new InvalidDataException("Malformed bridge JSON.", error); }
#if NET48
        catch (ArgumentException error) { throw new InvalidDataException("Malformed bridge JSON.", error); }
        catch (InvalidOperationException error) { throw new InvalidDataException("Malformed bridge JSON.", error); }
#else
        catch (JsonException error) { throw new InvalidDataException("Malformed bridge JSON.", error); }
#endif
    }

    public static byte[] SerializeResponse(BridgeResponse response)
    {
        try { return Serialize(response); }
        catch (PayloadTooLargeException)
        {
            return Serialize(new BridgeResponse
            {
                ContractVersion = BridgeLimits.ContractVersion, RequestId = response.RequestId,
                BridgeSessionId = response.BridgeSessionId, ChecklistSha256 = response.ChecklistSha256,
                Error = new BridgeError { Code = "ResponseTooLarge", Message = "The complete response exceeds the bridge limit.", RequestId = response.RequestId, Retryable = false }
            });
        }
    }

    public static DateTimeOffset ParseUtc(string value)
    {
        if (value == null || !value.EndsWith("Z", StringComparison.Ordinal) ||
            !DateTimeOffset.TryParseExact(value, UtcFormats, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
            throw new InvalidDataException("Bridge timestamp must be invariant ISO-8601 UTC with Z.");
        return parsed;
    }
    public static string FormatUtc(DateTimeOffset value) => value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", CultureInfo.InvariantCulture);

    internal static void ValidateRequest(BridgeRequest request)
    {
        Required(request.RequestId); Required(request.Operation); ParseUtc(request.DeadlineUtc);
        ValidateVersion(request.ContractVersion, request.RequestId);
        switch (request.Operation)
        {
            case BridgeLimits.GetContext:
                if (request.ContextToken != null) throw new InvalidDataException("Context acquisition cannot carry a token.");
                break;
            case BridgeLimits.CheckInformation: Required(request.ContextToken); break;
            default: throw DomainError("UnsupportedOperation", "Unknown bridge operation.", request.RequestId);
        }
    }

    static void ValidateValue<T>(T value)
    {
        switch (value)
        {
            case BridgeRequest request: ValidateRequest(request); break;
            case BridgeResponse response:
                Required(response.RequestId); Required(response.BridgeSessionId);
                ValidateVersion(response.ContractVersion, response.RequestId);
                if (response.ChecklistSha256 != null && (response.ChecklistSha256.Length != 64 ||
                    response.ChecklistSha256.Any(c => !((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))))
                    throw new InvalidDataException("Invalid checklist SHA-256.");
                if ((response.ResultJson == null) == (response.Error == null))
                    throw new InvalidDataException("Response must contain exactly one result or error.");
                if (response.ResultJson != null) Required(response.ResultJson); // Opaque BIM data, never parsed here.
                if (response.Error != null)
                {
                    ValidateValue(response.Error);
                    if (!StringComparer.Ordinal.Equals(response.RequestId, response.Error.RequestId))
                        throw new InvalidDataException("Error request identity does not match its envelope.");
                }
                break;
            case BridgeError error: Required(error.Code); Required(error.Message); Required(error.RequestId); break;
            case ReadinessContextResult context:
                if (context.RevitPid <= 0) throw new InvalidDataException("Invalid Revit process identity.");
                Required(context.RevitBuild); Required(context.DocumentLifetimeId); Required(context.ProjectUniqueId);
                Required(context.DocumentPath); Required(context.ChecklistId); Required(context.ChecklistPath);
                Required(context.Scope); Required(context.Level); Required(context.Phase); Required(context.ContextToken);
                ParseUtc(context.ExpiresUtc);
                break;
            default: throw new InvalidDataException("Missing or unsupported bridge object.");
        }
    }

    static void Required(string? value)
    {
        if (value == null || string.IsNullOrWhiteSpace(value)) throw new InvalidDataException("Required bridge string is missing or blank.");
        ValidateUnicode(value);
    }
    static void ValidateUnicode(string value)
    {
        try { FrameCodec.StrictUtf8.GetByteCount(value); }
        catch (System.Text.EncoderFallbackException error) { throw new InvalidDataException("Bridge string contains an invalid Unicode surrogate.", error); }
    }
    static void ValidateVersion(int version, string requestId)
    { if (version != BridgeLimits.ContractVersion) throw DomainError("UnsupportedContract", "Unknown bridge contract version.", requestId); }
    static BridgeException DomainError(string code, string message, string requestId) =>
        new BridgeException(new BridgeError { Code = code, Message = message, RequestId = requestId, Retryable = false });
    static int LimitFor(Type type)
    {
        if (type == typeof(BridgeRequest)) return BridgeLimits.MaxRequestBytes;
        if (type == typeof(BridgeResponse) || type == typeof(BridgeError) || type == typeof(ReadinessContextResult)) return BridgeLimits.MaxResponseBytes;
        throw new ArgumentException("Unsupported private bridge DTO.", nameof(type));
    }

    static void ValidateShape(XElement root, Type type)
    {
        if ((string?)root.Attribute("type") != "object" || root.Attributes().Any(a => a.Name != "type"))
            throw new InvalidDataException("Expected a plain bridge object.");
        var members = new Dictionary<string, XElement>(StringComparer.Ordinal);
        var properties = type.GetProperties();
        foreach (var member in root.Elements())
        {
            string name = member.Name.ToString();
            var property = properties.SingleOrDefault(p => p.Name == name);
            if (property == null || members.ContainsKey(name)) throw new InvalidDataException("Unknown or duplicate bridge member.");
            members.Add(name, member);
            string? kind = (string?)member.Attribute("type");
            if (kind == "null" && Optional(type, name))
            {
                if (member.HasElements || member.Value.Length != 0) throw new InvalidDataException("Invalid null member.");
                continue;
            }
            if (property.PropertyType == typeof(BridgeError)) { ValidateShape(member, typeof(BridgeError)); continue; }
            if (member.HasElements || member.Attributes().Any(a => a.Name != "type")) throw new InvalidDataException("Invalid scalar member.");
            if (property.PropertyType == typeof(string) && kind == "string") { ValidateUnicode(member.Value); continue; }
            if (property.PropertyType == typeof(bool) && kind == "boolean" && (member.Value == "true" || member.Value == "false")) continue;
            if (property.PropertyType == typeof(int) && kind == "number" &&
                int.TryParse(member.Value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int number) &&
                number.ToString(CultureInfo.InvariantCulture) == member.Value) continue;
            throw new InvalidDataException("Wrong bridge member type.");
        }
        foreach (var property in properties)
            if (!Optional(type, property.Name) && !members.ContainsKey(property.Name))
                throw new InvalidDataException("Missing bridge member.");
    }
    static bool Optional(Type type, string name) =>
        (type == typeof(BridgeRequest) && name == nameof(BridgeRequest.ContextToken)) ||
        (type == typeof(BridgeResponse) && (name == nameof(BridgeResponse.ChecklistSha256) || name == nameof(BridgeResponse.ResultJson) || name == nameof(BridgeResponse.Error)));

    static object ReadValidatedObject(XElement root, Type type)
    {
        // Only the allowlisted plain DTO types reach here, after ValidateShape.
        // JSON escapes are decoded by the infoset reader; strings have no other coercion.
        object result = Activator.CreateInstance(type)!;
        foreach (var property in type.GetProperties())
        {
            var member = root.Element(property.Name);
            if (member == null || (string?)member.Attribute("type") == "null") continue;
            object value;
            if (property.PropertyType == typeof(string)) value = member.Value;
            else if (property.PropertyType == typeof(int)) value = int.Parse(member.Value, CultureInfo.InvariantCulture);
            else if (property.PropertyType == typeof(bool)) value = member.Value == "true";
            else if (property.PropertyType == typeof(BridgeError)) value = ReadValidatedObject(member, typeof(BridgeError));
            else throw new InvalidDataException("Unsupported bridge member type.");
            property.SetValue(result, value);
        }
        return result;
    }

    sealed class PayloadTooLargeException : IOException
    { public PayloadTooLargeException() : base("Bridge JSON exceeds its byte limit.") { } }
    sealed class BoundedOutput : MemoryStream
    {
        readonly int limit;
        internal bool LimitExceeded { get; private set; }
        internal BoundedOutput(int limit) { this.limit = limit; }
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count > limit - Length) { LimitExceeded = true; throw new PayloadTooLargeException(); }
            base.Write(buffer, offset, count);
        }
        public override void WriteByte(byte value)
        {
            if (Length >= limit) { LimitExceeded = true; throw new PayloadTooLargeException(); }
            base.WriteByte(value);
        }
    }
}
