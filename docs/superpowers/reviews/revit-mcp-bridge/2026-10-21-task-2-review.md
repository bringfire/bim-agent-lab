# Task 2 implementation review and author dispositions

**Subsequent status:** [independent follow-up](2026-10-21-task-2-follow-up.md) closed T2-01/02/03 at `3cefe5116ec1368e0bfdcc387209bbb1e176df66` and recommended Task 3. The original correction-stage record below is retained as history.

Received through the author on 2026-10-21. Reviewed commit: **`eefb1563fdf7073dc7065668da355b0642f99709`**. This is a focused independent review of Task 2, not closure of the full R3 implementation/live-evidence review. The reviewer's identity was not supplied in this message. The original feedback and the implementing agent's dispositions are kept separate below.

## Original reviewer feedback

Links below use repository-relative paths for portability; line references describe the reviewed commit.

> **Recommendation: revise Task 2 before proceeding to Task 3.** I found one data-preservation defect and two smaller issues in `eefb1563fdf7073dc7065668da355b0642f99709`.
>
> 1. **T2-01 — P2: net48 decoding changes valid string values.**
>    [BridgeJson.cs (line 54)](../../../../projects/revit-readiness/src/bridge-runtime/BridgeJson.cs) uses `JavaScriptSerializer`, which interprets escaped Microsoft-style date strings.
>
>    I reproduced this using the production serializer and decoder with `ReadinessContextResult.Level = "/Date(0)/"`:
>    - **net48:** returns `1970-01-01 00:00:00Z`.
>    - **net10:** returns `/Date(0)/`.
>    Shape validation passes, but subsequent conversion changes the value. This violates the shared contract's string-preservation guarantee.
>
>    **Correction:** preserve validated string tokens verbatim during DTO conversion. Add round-trip and cross-runtime tests for date-shaped strings, including escaped and unescaped slashes.
> 2. **T2-02 — P3: malformed JSON bypasses the normalized error contract.**
>    [BridgeJson.cs (line 62)](../../../../projects/revit-readiness/src/bridge-runtime/BridgeJson.cs) does not catch `FormatException` from the JSON infoset reader. A raw newline inside `RequestId` produces that exception on **both runtimes**, instead of the `InvalidDataException` used for other malformed inputs.
>
>    **Correction:** normalize this parser failure and test raw newline, tab, and NUL inside quoted strings. Task 3 should be able to classify malformed input consistently.
> 3. **T2-03 — P3: the independent fixture checks the wrong completeness member.**
>    [verify_bridge_exchange.py (line 34)](../../../../projects/revit-readiness/tests/verify_bridge_exchange.py) checks `Evaluation.CoverageComplete`; the actual evaluator exposes `Evaluation.Complete`. The C# fixture repeats the same incorrect name.
>
>    **Correction:** use `CoverageComplete` and `Evaluation.Complete` in both fixtures and regenerate the retained evidence. The current test proves opaque-string preservation, but its claimed correspondence to the two real completeness flags is inaccurate.
>
> I reran **all supplied offline MCP/bridge suites and the readiness/Excel regression suite successfully**. The additional decoder probes above exposed cases those tests omit. The reviewed framing and lifetime code preserves the intended ownership boundaries; I found no additional defect there.
>
> The checkout remains clean and unchanged. No installation, desktop UI verification, or live Revit testing was performed.

## Implementing agent's dispositions

All three findings reproduced before their respective corrections. The [revision verification record](../../../../projects/revit-readiness/docs/verification/2026-10-21-bridge-foundations-review.md) holds red/green evidence, exact source baseline, fresh regressions and limitations.

| Finding | Disposition and implementation | Proof |
|---|---|---|
| T2-01 | Accepted. Both targets now materialize DTOs from validated original infoset tokens. Complete-input parsers provide syntax checks only; their converted values are discarded. The net48 syntax-only copy uses the equivalent unescaped spelling of the special date prefix to avoid implicit date conversion and overflow. | Original net48 timestamp substitution reproduced. Literal values now survive both runtime round trips and bidirectional exchange, including both slash spellings, offset/negative/large date-shaped values, opaque results and nested errors. Extra-object/trailing-comma rejection remains tested. |
| T2-02 | Accepted. Normalize infoset `FormatException` to `InvalidDataException`, preserving the inner cause. Stable `BridgeException` domain failures retain their separate path. | Newline, tab and NUL each reproduced the leak on both targets. All now normalize; escaped forms remain valid string data. |
| T2-03 | Accepted. Check the actual `InformationReport.CoverageComplete` and `Evaluation.Complete` definitions and retained Revit output, correct C#/Python fixtures, regenerate fresh exchange evidence. | Corrected checker rejects original evidence with missing `Complete`. New evidence preserves both exact false members; corrupted names/flags are rejected independently. Original evidence remains marked historical rather than silently rewritten. |

**Author-side correction and verification are not independent reviewer closure.** Follow-up review is pending. Task 3 was not started. Framing, budget and lifetime implementations are unchanged by this correction; live pipe/Revit/desktop A8 acceptance and the rest of R3 remain outstanding.

## Concept to carry into later work

The implementing agent's synthesis is to separate **syntax validity, schema validity, value preservation, and the authority for an expected answer**. A parser can accept a message and still reinterpret a string. A checker can use a different language and still repeat the implementation's mistaken field name. This is a proposed lesson supported by the reproduced cases, not a new diagnosis attributed to the reviewer. The [participant story](../../../tutorials/08-build-and-test-the-bridge-foundation.md#use-independent-review-to-challenge-the-tests) and [agent guide](../../../how-to/build-and-verify-bridge-foundations.md#task-2-review-corrections) record it at their respective levels.

## Follow-up review packet

Review `eefb1563fdf7073dc7065668da355b0642f99709` through the commit that adds this document, titled `fix: preserve bridge strings and correct completeness evidence`. Use the corrected guide's commands, inspect the date-prefix syntax-only handling and original-token materialization, verify the retained hashes, and independently challenge string preservation and error classification. Check the fixture names against the core definitions rather than just comparing the C# and Python fixture literals. Report whether T2-01/02/03 close at this scope, and whether the syntax/schema/value/oracle separation is consistently maintained. Do not infer live Revit acceptance from these tests.
