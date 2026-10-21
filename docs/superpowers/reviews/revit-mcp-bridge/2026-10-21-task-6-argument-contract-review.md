# Task 6 complete argument-contract review

**Subsequent independent disposition:** [Task 6 and T6-02 closed at b4cdb1f](2026-10-21-task-6-closure.md); T6-01 shutdown closure at 332934c stands. The submission/findings/dispositions below preserve their original checkpoint. Installation and live acceptance remain outstanding.

Reviewed baseline: `332934cb144148a9d8af79fe57cd745ac110f6a0`. This is author-relayed independent feedback, followed by separate author dispositions. Prior [boundary review](2026-10-21-task-6-boundary-review.md) and evidence remain historical.

## Independent follow-up supplied by the author

> **Task 6 remains open at `332934c`: one P2 validation gap remains.**
>
> Both original reproducers pass unchanged. The disconnect finding is **closed**: with tracing off/on, shutdown completes in roughly 15–16 ms while execution remains held, and the final-read guard rejects further work. The original malformed-string cases now return correlated `InvalidRequest`.
>
> **Remaining P2 — other invalid argument shapes still escape validation.** These arguments still produce JSON-RPC `-32603`:
>
> - `{"contextToken":["\ud800"]}`
> - `{"contextToken":"opaque","extra":"\ud800"}`
>
> I reproduced five variants, including an extra malformed argument to the context-acquisition tool. Equivalent shapes containing valid text correctly return `InvalidRequest`.
>
> The SDK-before-handler diagnosis is correct. However, the filter protects only a string `contextToken`; SDK conversion still encounters malformed text elsewhere before ordinary validation runs. Rejected argument shapes for both tools need handling at that earlier boundary.
>
> All 50 tool tests, full regressions—including strict July—readiness/Excel/isolation checks, and the Revit API build passed. The checkout remains clean. Installation and live acceptance remain unverified.

Local reproduction source and evidence: `C:/Users/bring/AppData/Local/Temp/codex-task6-independent-review/332934c/README.md` and `shape_unicode_review.py`. Copies used for author reruns are unchanged. The original EOF/Unicode probes are also unchanged.

The author additionally directed a complete boundary contract: accepted arguments satisfy the whole schema and are safe to convert; rejected arguments produce correlated InvalidRequest without bridge admission and without decoding rejected data again. Test both tools across shapes, malformed/nested values and combinations. Keep envelope/protocol ownership with the SDK and downstream failures separate. This is the authorized correction, not a new tool or protocol policy.

## Author dispositions

- **T6-01 disconnect:** independently closed at 332934c. Preserve its host and regression tests unchanged.
- **T6-02 complete argument rejection:** reproduced all five new failures before editing. The prior correction located the right boundary but still handed rejected data to conversion. Replace the whole rejected subtree and carry a safe owned outcome, rather than adding branches for the five examples.

The [complete boundary model](../../../explanation/readiness-argument-boundary.md) was written before production changes. It defines both accepted schemas, optional omitted arguments versus explicit null, fallible property/name access, owned preparation and the proof for each continuation. An accepted request is serialized with its real ID/deadline before SDK conversion, and its original budget continues into typed dispatch. A rejected outcome contains no original argument tree. The SDK receives a fresh empty argument object; the handler rejects the owned outcome before retained admission or IPC.

The initial generated matrix revealed 221 failures per 310-case exchange against unchanged production. The first complete-gate implementation left one failure per exchange: SDK JsonNode lookup inherited case-insensitive options, so ContextToken incorrectly matched contextToken. The validator now enumerates the single actual property and compares its name ordinally. This came from the schema-derived matrix, not another independent review finding. Earlier failed attempts remain evidence.

No SDK version/source copying, runtime, Revit, model, installer or desktop-settings changes. No broad catch around SDK continuation or bridge execution. Both teaching layers are updated. Task 6 remains **open pending independent follow-up of T6-02**; Task 7 and live acceptance remain ahead.

## Independent follow-up request

Review the correction commit against 332934c and the [complete contract](../../../explanation/readiness-argument-boundary.md). Challenge recognition versus envelope ownership, fallible argument inspection, ordinal names, full subtree replacement, accepted-input preservation, safe rejected outcomes, zero admission and one original ID/budget through conversion. Verify every rejection can finish without touching rejected descendants. Challenge combinations beyond the five original reproductions and the generated matrix. Do not broaden argument catches into continuation/execution.

[Fresh evidence](../../../../projects/revit-readiness/docs/verification/2026-10-21-mcp-argument-contract.md) records 59 Tools tests, 1,860 application cases, six SDK method controls, three handoff cases, unchanged reviewer probes and full regressions. Inspect failed attempts as well as the final result; compressed transcripts retain exact uncompressed hashes. T6-01 is already independently closed; determine T6-02 disposition. Task 7, live Autodesk/desktop A8 and full R3 remain outside this increment. No review was transmitted automatically.
