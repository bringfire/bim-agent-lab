# Decision: continue with verified explicit cancellation

Date: October 22, 2026. Author direction: **“follow your recommendations”**, following the recommendation to continue with explicit cancellation, retain the SDK limitation and its reproducer, and carry it into final independent review.

## Scope of the decision

Continue the remaining shutdown, recovery and acceptance work using official SDK typed cancellation notifications in the verification client. Keep C# SDK **2.2.0**, the existing desktop compatibility policy and strict-July regression. Do not fork or patch the SDK for this increment. [Diagnosis and evidence](../../../../projects/revit-readiness/docs/verification/2026-10-22-sdk-cancellation-diagnosis.md) establish why cancellation of the local SDK wait is insufficient.

This accepts a tested way to generate the cancellation stimulus; it does **not** waive A3, weaken the original deadline/read guards, mark token-only propagation fixed, or close R3. The bridge must still invalidate work after an actual cancellation notification or disconnect, reject a new read when inactive, and suppress late success. An already-started synchronous Revit read may finish before its resources retire.

The explicit-message approach applies to the verification client we control. Codex and Claude decide what their own Stop buttons send. Neither button's behavior has been established. Do not infer desktop cancellation from a harness pass or call it a supported user-facing feature until a real application trace and receiving-side evidence prove it. Existing desktop readiness results remain valid for the actions actually tested.

## Carry forward

- Retain `--require-propagation` as a failing SDK defect probe and label `--characterize` as preservation of known behavior, not a fix.
- Continue existing explicit cancellation, EOF, deadline, bounded-admission and late-result tests. Do not substitute one cancellation origin for another.
- Include this decision, the diagnosis and unchanged failing probe in the external R3 packet. Author acceptance of the approach is not reviewer closure.
- On a proposed SDK/runtime upgrade, run the remote-propagation probe and the normal regressions before selecting the dependency. Source or release-note claims alone are insufficient. Avoid sending duplicate automatic and explicit cancellation during upgrade tests.
- Keep the reproducer available for an upstream report. No external issue/message is authorized or sent by this decision.

The current scope is a read-only readiness query. Any later editing capability needs its own operation/transaction design and acceptance; cancellation must not be treated as a rollback guarantee.

No normative spec change is needed: explicit notifications exercise the existing cancellation contract. The plan gains this recorded execution decision and continues under Native execution. Remaining host overlap limits, historical shared installation recovery and final acceptance are separate work. See the [participant story](../../../tutorials/13-install-with-a-recovery-plan.md#continue-with-a-tested-alternative).
