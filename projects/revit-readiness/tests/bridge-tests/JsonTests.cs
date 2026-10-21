using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;
using BimAgentLab.RevitBridge.Runtime;

namespace BimAgentLab.Bridge.Tests;

internal static partial class Program
{
    static async Task JsonTests()
    {
        await Test("Request wire preserves required members and Unicode", () => Sync(() =>
        {
            var request = BridgeJson.Deserialize<BridgeRequest>(Utf8(RequestJson));
            Equal("request-一", request.RequestId); Equal("token-α", request.ContextToken);
            Equal(Deadline, BridgeJson.Deserialize<BridgeRequest>(BridgeJson.Serialize(request)).DeadlineUtc);
        }));
        await Test("Malformed, duplicate, unknown, missing and wrongly typed envelope members fail", () => Sync(() =>
        {
            foreach (string invalid in new[] {
                "{}", "null", "[]", RequestJson + "{}", RequestJson + " garbage",
                RequestJson.Substring(0, RequestJson.Length - 1) + ",}",
                RequestJson.Replace("\"RequestId\"", "RequestId"),
                RequestJson.Replace("\"RequestId\"", "'RequestId'"),
                RequestJson.Replace("\"request-一\"", "\"\\uD800\""),
                RequestJson.Replace("\"RequestId\"", "/* comment */ \"RequestId\""),
                RequestJson.Replace("\"ContractVersion\":1", "\"ContractVersion\":1,\"Contract\\u0056ersion\":1"),
                RequestJson.Replace("\"ContractVersion\":1", "\"ContractVersion\":\"1\""),
                RequestJson.Replace("\"ContractVersion\":1", "\"ContractVersion\":1.0"),
                RequestJson.Replace("\"ContractVersion\":1", "\"contractVersion\":1"),
                RequestJson.Replace("\"ContractVersion\":1", "\"Extra\":0,\"ContractVersion\":1"),
                RequestJson.Replace("\"ContractVersion\":1", "\"ContractVersion\":1,\"ContractVersion\":1"),
                RequestJson.Replace("\"request-一\"", "null"), RequestJson.Replace("\"request-一\"", "\" \""),
                RequestJson.Replace("\"ContextToken\":\"token-α\",", "") })
            {
                try { Throws<InvalidDataException>(() => BridgeJson.Deserialize<BridgeRequest>(Utf8(invalid))); }
                catch (Exception error) { throw new Exception("Invalid input did not produce InvalidDataException: " + invalid + "; " + error.GetType().Name + ": " + error.Message, error); }
            }
            Throws<InvalidDataException>(() => BridgeJson.Deserialize<BridgeRequest>(new byte[] { 0xc3, 0x28 }));
        }));
        await Test("Unknown version and operation report stable contract errors", () => Sync(() =>
        {
            Equal("UnsupportedContract", Throws<BridgeException>(() => BridgeJson.Deserialize<BridgeRequest>(Utf8(RequestJson.Replace("\"ContractVersion\":1", "\"ContractVersion\":2")))).Error.Code);
            Equal("UnsupportedOperation", Throws<BridgeException>(() => BridgeJson.Deserialize<BridgeRequest>(Utf8(RequestJson.Replace(BridgeLimits.CheckInformation, "open_model")))).Error.Code);
            var request = Request(); request.Operation = BridgeLimits.GetContext; request.ContextToken = null;
            Equal(BridgeLimits.GetContext, BridgeJson.Deserialize<BridgeRequest>(BridgeJson.Serialize(request)).Operation);
            request.ContextToken = "unexpected";
            Throws<InvalidDataException>(() => BridgeJson.Serialize(request));
        }));
        await Test("UTC timestamps reject culture-dependent, local and invalid values", () => Sync(() =>
        {
            Equal(Epoch.AddSeconds(30), BridgeJson.ParseUtc(Deadline));
            Equal(Epoch.AddSeconds(30), BridgeJson.ParseUtc("2026-10-21T00:00:30Z"));
            Equal(Epoch.AddSeconds(30).AddMilliseconds(100), BridgeJson.ParseUtc("2026-10-21T00:00:30.1Z"));
            foreach (var invalid in new[] { "10/21/2026 12:00:30 AM", "2026-10-21T00:00:30", "2026-10-21T00:00:30+02:00", "2026-99-21T00:00:30Z", "2026-10-21T00:00:30.Z", "2026-10-21T00:00:30.00000000Z" })
                Throws<InvalidDataException>(() => BridgeJson.Deserialize<BridgeRequest>(Utf8(RequestJson.Replace(Deadline, invalid))));
        }));
        await Test("Opaque report preserves casing, false flags, Unicode and prompt-like text", () => Sync(() =>
        {
            string content = Report + new string(' ', 9000);
            var response = BridgeJson.Deserialize<BridgeResponse>(BridgeJson.SerializeResponse(Response(content)));
            Equal(content, response.ResultJson); Equal(new string('a', 64), response.ChecklistSha256); Equal(null, response.Error);
        }));
        await Test("Response requires exactly one result/error and matching error request identity", () => Sync(() =>
        {
            var response = Response(); response.Error = new BridgeError { Code = "Busy", Message = "Retry later.", RequestId = "request-一", Retryable = false };
            Throws<InvalidDataException>(() => BridgeJson.Serialize(response));
            response.ResultJson = null;
            var bytes = BridgeJson.Serialize(response); Check(Encoding.UTF8.GetString(bytes).Contains("\"Retryable\":false"), "False omitted");
            var decoded = BridgeJson.Deserialize<BridgeResponse>(bytes); Equal(false, decoded.Error!.Retryable);
            response.Error.RequestId = "other"; Throws<InvalidDataException>(() => BridgeJson.Serialize(response));
            response.Error = null; Throws<InvalidDataException>(() => BridgeJson.Serialize(response));
            Throws<InvalidDataException>(() => BridgeJson.Deserialize<BridgeResponse>(Utf8(Encoding.UTF8.GetString(bytes).Replace(",\"Retryable\":false", ""))));
        }));
        await Test("Oversized response becomes a small correlated error without truncation or mutation", () => Sync(() =>
        {
            var response = Response(new string('界', 1500000));
            byte[] bytes = BridgeJson.SerializeResponse(response);
            Check(bytes.Length < 1024, "Oversize error not bounded");
            var decoded = BridgeJson.Deserialize<BridgeResponse>(bytes);
            Equal("ResponseTooLarge", decoded.Error!.Code); Equal("request-一", decoded.Error.RequestId);
            Equal("session-1", decoded.BridgeSessionId); Equal(null, decoded.ResultJson); Equal(false, decoded.Error.Retryable);
            Equal(1500000, response.ResultJson!.Length); Equal(null, response.Error);
        }));
        await Test("Context DTO preserves paths and rejects incomplete identity", () => Sync(() =>
        {
            var value = ContextResult(); var decoded = BridgeJson.Deserialize<ReadinessContextResult>(BridgeJson.Serialize(value));
            Equal(value.DocumentPath, decoded.DocumentPath); Equal(value.ExpiresUtc, decoded.ExpiresUtc);
            value.RevitPid = 0; Throws<InvalidDataException>(() => BridgeJson.Serialize(value));
        }));
        await Test("Date-shaped strings survive round trips with escaped and unescaped slashes", () => Sync(() =>
        {
            foreach (string text in new[] { "/Date(0)/", "/Date(-1)/", "/Date(0+0100)/", "/Date(315537897600000)/", "/Date(9223372036854775807)/" })
            {
                var context = ContextResult(); context.Level = text;
                string wire = Encoding.UTF8.GetString(BridgeJson.Serialize(context));
                Check(wire.Contains("\\/Date("), "Test must exercise the Framework date escape spelling");
                Equal(text, BridgeJson.Deserialize<ReadinessContextResult>(Utf8(wire)).Level);
                Equal(text, BridgeJson.Deserialize<ReadinessContextResult>(Utf8(wire.Replace("\\/", "/"))).Level);
                Throws<InvalidDataException>(() => BridgeJson.Deserialize<ReadinessContextResult>(Utf8(wire + "{}")));
                Throws<InvalidDataException>(() => BridgeJson.Deserialize<ReadinessContextResult>(Utf8(wire.Substring(0, wire.Length - 1) + ",}")));
            }
            foreach (string text in new[] { "\\/Date(0)\\/", "\"/Date(0)/\"" })
            {
                var literal = ContextResult(); literal.Level = text;
                Equal(text, BridgeJson.Deserialize<ReadinessContextResult>(BridgeJson.Serialize(literal)).Level);
            }
        }));
        await Test("Date-shaped opaque results and nested error strings stay literal", () => Sync(() =>
        {
            Equal("/Date(0)/", BridgeJson.Deserialize<BridgeResponse>(BridgeJson.SerializeResponse(Response("/Date(0)/"))).ResultJson);
            var response = Response(); response.ResultJson = null;
            response.Error = new BridgeError { Code = "Busy", Message = "/Date(0)/", RequestId = response.RequestId, Retryable = false };
            Equal("/Date(0)/", BridgeJson.Deserialize<BridgeResponse>(BridgeJson.SerializeResponse(response)).Error!.Message);
        }));
        foreach (char control in new[] { '\n', '\t', '\0' })
            await Test("Raw control in quoted JSON normalizes to InvalidDataException: " + (int)control, () => Sync(() =>
            {
                string invalid = RequestJson.Replace("request-一", "before" + control + "after");
                Throws<InvalidDataException>(() => BridgeJson.Deserialize<BridgeRequest>(Utf8(invalid)));
            }));
        await Test("Escaped controls remain valid literal string content", () => Sync(() =>
        {
            var context = ContextResult(); context.Level = "before\n\t\0after";
            Equal(context.Level, BridgeJson.Deserialize<ReadinessContextResult>(BridgeJson.Serialize(context)).Level);
        }));
    }
}
