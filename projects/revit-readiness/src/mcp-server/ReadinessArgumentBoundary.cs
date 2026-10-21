using BimAgentLab.RevitBridge.Contract;
using BimAgentLab.RevitBridge.Runtime;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BimAgentLab.RevitReadiness.Mcp;

// A prepared outcome owns only safe application data. Rejected trees are never retained.
internal sealed record PreparedReadinessCall(string RequestId, RequestBudget Budget, BridgeRequest? Request);
internal readonly record struct ArgumentInput(bool Present, JsonNode? Value);

internal static class ReadinessArgumentBoundary
{
    internal static bool Recognized(string? operation) => operation is BridgeLimits.GetContext or BridgeLimits.CheckInformation;

    internal static bool TryRecognize(JsonObject parameters, out string? operation)
    {
        operation = null;
        try
        {
            if (parameters["name"] is JsonValue name && name.GetValueKind() == JsonValueKind.String)
                operation = name.GetValue<string>();
            return Recognized(operation);
        }
        catch (Exception error) when (RepresentationError(error))
        {
            // Invalid MCP envelope/name: leave it to the SDK, not application validation.
            return false;
        }
    }

    internal static PreparedReadinessCall Prepare(string operation, IBridgeClock clock, Func<ArgumentInput> readArguments)
    {
        string id = Guid.NewGuid().ToString("N");
        var budget = RequestBudget.Admit(clock); // one budget, including conversion/dispatch time
        try
        {
            var input = readArguments();
            string? token = null;
            if (operation == BridgeLimits.GetContext)
            {
                // Omitted optional MCP arguments means no arguments; explicit null is not an object.
                if (input.Present && (input.Value is not JsonObject empty || empty.Count != 0)) return Rejected();
            }
            else
            {
                if (!input.Present || input.Value is not JsonObject arguments || arguments.Count != 1) return Rejected();
                // SDK JsonNode lookup options may be case-insensitive. The application
                // schema requires the actual decoded property name to match ordinally.
                var property = arguments.Single();
                if (!StringComparer.Ordinal.Equals(property.Key, "contextToken")
                    || property.Value is not JsonValue scalar || scalar.GetValueKind() != JsonValueKind.String) return Rejected();
                token = scalar.GetValue<string>();
                if (string.IsNullOrWhiteSpace(token)) return Rejected();
            }
            var request = new BridgeRequest { ContractVersion = 1, RequestId = id, Operation = operation,
                ContextToken = token, DeadlineUtc = BridgeJson.FormatUtc(budget.DeadlineUtc) };
            BridgeJson.Serialize(request); // actual request: strict Unicode and existing serialized-byte cap
            return new(id, budget, request);
        }
        catch (Exception error) when (RepresentationError(error) || error is InvalidDataException || error is IOException)
        {
            // Only argument inspection/conversion and in-memory request validation are in this catch.
            // No SDK continuation, provider, IPC or execution failure can enter it.
            return Rejected();
        }
        PreparedReadinessCall Rejected() => new(id, budget, null);
    }

    static bool RepresentationError(Exception error) => error is InvalidOperationException or ArgumentException or JsonException;
}
