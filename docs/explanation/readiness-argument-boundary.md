# Complete readiness argument boundary

Task 6 correction following review of `332934c`. Written before the production change. The author explicitly requested this boundary contract and systematic tests; the approved two-tool scope, SDK 2.2.0 and compatible/strict protocol policy stay unchanged.

## Responsibility at the handoff

For a recognized readiness tool in an SDK-valid MCP envelope, decide the complete application argument outcome before SDK typed conversion:

- **Accepted:** the whole argument shape is approved, every value that conversion will visit is safe, and the actual bridge request satisfies existing size/text constraints. Preserve the caller's valid argument tree unchanged.
- **Rejected:** return correlated, non-retryable InvalidRequest with no bridge admission. Once decided, no downstream conversion, logging or response construction may depend on the rejected argument subtree.

This replaces the earlier leaf-specific Unicode filter. A rejection marker alone cannot enforce the contract while rejected data remains on the execution path.

## Exact policy

| Tool | Accepted application arguments |
|---|---|
| Acquire context | Empty object, or omitted optional MCP arguments (existing no-argument invocation) |
| Check information | Object with exactly one property named contextToken; its value is a nonblank valid Unicode string, and the complete serialized bridge request fits the existing 64-KiB cap |

Explicit null, all other JSON types, missing required properties, extra properties, duplicate/undecodable property names, blank/invalid text and oversized requests reject. An escaped spelling of the correct property name and valid Unicode/escaped string values retain their decoded meaning. A rejected extra property's value never needs inspection. Arrays/objects in the token position reject on kind without traversing children. Count/property access/enumeration/string decoding are all potentially fallible; contain only expected argument representation failures in the validation stage.

MCP envelope parsing, method/name recognition failures, version negotiation and metadata remain SDK responsibilities. Unknown tools keep their existing unsupported-operation path. Do not turn arbitrary execution/transport errors into argument errors or catch exceptions around next/SendAsync to achieve that.

## Ownership and safe continuation

The incoming filter recognizes the operation, creates one request identity and admission budget, and prepares an owned outcome. An accepted outcome holds the validated bridge request and that same budget; a rejected outcome holds only safe correlation/error state, not rejected values. Typed dispatch consumes this outcome rather than starting a second budget. Direct typed test/API invocation uses the same preparation rules when no filter outcome exists.

For rejection, replace the entire arguments member with a fresh empty JsonObject and attach the server-owned outcome in SDK per-message Items. Do not enumerate, clone, serialize or repair the rejected subtree afterward. Preserve the SDK envelope and metadata. The typed handler sees the rejection before retained-slot acquisition, provider selection or IPC; the SDK still creates its ordinary result envelope. The marker/outcome cannot be supplied through JSON fields. The acceptance path leaves the raw tree untouched and uses the already validated request. No cancellation source or background work is created just to reject arguments.

Why accepted conversion is safe: the only possible trees are an empty object or one correctly named string property. The sole string has decoded successfully, and the actual private request has passed strict Unicode and byte-size validation. There are no unvisited application members or descendants.

Why rejected conversion can finish: the SDK receives a newly created empty arguments object, and response construction uses only safe server-owned outcome data. The old subtree is detached, not walked for cleanup. Failure while inspecting its names or values leads to the same whole-subtree replacement. This guarantee assumes a valid MCP envelope; unrelated malformed envelope members remain with the SDK.

## Tests derived from the contract

Generate cases by crossing both tools with omitted arguments and every JSON kind, malformed scalar/array/object text, missing/wrong/duplicate/escaped/undecodable names, extra properties, and pairs of defects in both orders. Include valid string controls (Unicode, escaped controls, date-shaped text, valid surrogate pairs and large allowed text), oversized text, and valid requests after rejection. Keep literal expected outcomes in the test generator, independent of the production validator.

Run raw stdio against the real executable and a real Windows pipe with a counting synthetic executor, on all three protocol revisions with tracing off/on. For every rejected case require the correlated InvalidRequest shape, matching text/structured result, no report, no change in bridge call count and successful subsequent valid calls. For accepted cases require exactly one bridge call and exact token preservation. Retain wire inputs without serializer repair of malformed escapes. Keep reviewer reproductions unchanged, earlier disconnect/held-callback tests, strict July and downstream failure classifications. Distinguish these transport proofs from live Autodesk/desktop acceptance.
