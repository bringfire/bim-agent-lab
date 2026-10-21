# Private bridge foundation — 2026-10-21

**Task 2 passes its offline acceptance on net48 and net10.0.** This implements the messages, framing, budgets and lifetime foundation. It does not establish a running pipe endpoint, live Revit bridge, full desktop A8 or external R3 closure.

The author explicitly requested Task 2, the updated plan, preserved compatibility tests and both teaching layers. Native execution continued on `feat/revit-mcp-bridge` from **`9b4eb516819f4c6fc5d90ccdefc7751b0705e694`**. The implementation checkpoint is the Git commit titled **`feat: define bounded Revit bridge messages and framing`** that first adds this record; locate it with `git log --diff-filter=A --format="%H %s" -- projects/revit-readiness/docs/verification/2026-10-21-bridge-foundations.md`. This avoids placing an invented self-referential hash in the commit.

Participants: [how the author directed the step](../../../../docs/tutorials/08-build-and-test-the-bridge-foundation.md). Agents: [source map, reproduction and ownership obligations](../../../../docs/how-to/build-and-verify-bridge-foundations.md). Authority: [plan revision 3, Task 2](../../../../docs/superpowers/plans/2026-10-20-revit-mcp-bridge.md#task-2-plain-messages-framing-and-shared-request-budgets).

## Results

| Check | Observed result | Limit |
|---|---|---|
| Compilable stubs | 27 failures on each target, for missing behavior | A preceding test-source compiler failure is not counted as red proof |
| Locked multi-target build | Zero warnings/errors | No Revit assemblies or installation involved |
| Bridge behavioral tests | 27 passes / zero failures on each target | Stream doubles and controlled callbacks; real pipes/queue/read boundary come later |
| Bidirectional exchange | net48 consumes net10.0 messages and vice versa | Synthetic request, context and opaque report |
| Independent Python comparison | Exact values, Unicode, identities and both false coverage flags preserved | Does not compare live model observations |
| Preserved MCP regression | 19 C# cases, 10 Python test methods and four SDK/wire comparisons pass | Offline SDK tests, not repeated desktop UI calls |
| Existing readiness regression | 18 core, 7 Excel and isolated export-host checks pass | No new Revit session or model mutation |
| Assembly/dependency inspection | Zero new external NuGet packages; no Autodesk/MCP/core references; modern runtime has no System.Web.Extensions reference | Framework dependencies still require the documented machine setup |

The MCP checks retain compatible `2025-06-18`, `2025-11-25`, `2026-07-28`, strict July success and strict rejection of both older revisions. C# SDK 2.2.0 and the production compatibility policy are unchanged. Actual prior desktop observations remain in the [CP0 record](2026-10-20-desktop-mcp-compatibility.md); this task did not register a server or ask the author to reload either app.

## Retained evidence

The [inventory](bridge-foundations-evidence/inventory.json) hashes public lab-only artifacts: red test output, the malformed-JSON diagnostic, final framing/independent-exchange output, the complete offline MCP regression, readiness regression, emitted messages from both runtimes, and built assembly reference/hash observations. Executables and local generated work remain ignored. Evidence files describe this machine/run; paths and synthetic values are not deployment instructions.

The final All run includes the independent Python exchange call and the timestamp correction. A separate final Framing run checks the same source. No desktop capture was rewritten or relabeled.

## What changed and why

The contract is plain data shared across frameworks. Framing counts bytes, validates strict UTF-8, handles fragmented/coalesced input and rejects incomplete/oversized frames. A bounded response writer substitutes a small correlated `ResponseTooLarge` error instead of truncating. ResultJson remains opaque; no readiness evaluator is duplicated.

The budget uses original admission/receipt deadlines and monotonic elapsed time. A request snapshot cannot be altered by a caller holding the original mutable DTO. Linked cancellation remains available through the execution context. Tests show that preparation-time cancellation/expiry blocks the next guarded action, and that cleanup waits for Quiesced even after Response is canceled. The future executor must use these guards at the actual read/publication boundaries; it does not exist yet.

The first JSON implementation used the framework infoset reader as though it validated the whole input. Tests disproved that assumption for an extra object after the first. The final decoder combines the planned complete-input runtime parsers with explicit original-shape/Unicode checks. The extra surrogate case caught a second behavior: net48's parser could normalize a broken character. Both cases are retained in the tests. A final negative timestamp test exposed optional fractional formatting accepting `30.Z`; explicit supported formats now reject a decimal point with no digits. Encoding uses a framework DataContract serializer with a bounded stream, not a new library package.

All new bridge code is original lab code using platform APIs. The external SDK and its previously attributed stdin adaptation remain unchanged. No TypeScript SDK 1.26.0 or third-party bridge source was copied or installed.

## Still required

Task 3 is next: actual Windows-local pipes, descriptor validation, explicit Revit-instance selection, first-frame timeout and slot recovery. Later tasks still owe production dispatch, Revit API execution, both tools, reversible installation, three principal sample cases, NC1/NC2, complete desktop A8 and external R3 review. Task 2's zero-reader-call simulations are not the production read-boundary tests required by the reviewed plan. No readiness pass may be inferred from a synthetic zero-findings/incomplete-coverage payload.
