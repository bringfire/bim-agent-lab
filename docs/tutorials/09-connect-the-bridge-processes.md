# Ask the agent to prove the connection between processes

Participants: this follows the [bridge foundation and review story](08-build-and-test-the-bridge-foundation.md). Agents: use the [technical procedure](../how-to/build-and-verify-local-pipe-transport.md). Both refer to the same [Task 3 evidence](../../projects/revit-readiness/docs/verification/2026-10-21-local-pipe-transport.md).

## How the author moved the work forward

The author returned the independent reviewer's follow-up on the corrected Task 2 commit:

> Recommendation: proceed to Task 3. I close T2-01, T2-02, and T2-03 for the Task 2 scope. No new findings emerged.

This is a reviewer excerpt forwarded by the author, not an invented user prompt. The author had already approved the implementation plan and selected Native execution. The agent recorded the [review closure](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-2-follow-up.md), then continued with the next named task. The client choice, SDK version and compatibility decision remained settled.

Task 3 connects two processes on the same Windows machine. One represents the code that will run inside Revit; the other represents the external agent server. The useful BIM question is: **how do we know a request reaches the intended Revit session, and what happens when that session stops?** The Revit reader is a later step.

## What the agent was responsible for proving

- Choose one valid session, or return a clear unavailable/ambiguous error. Never silently switch to another model session after binding.
- Preserve separate requests from simultaneous clients, including a large Unicode result.
- Release every stalled connection within the specified time and admit a valid next request.
- Handle cancellation and shutdown without destroying resources still borrowed by unfinished work.
- Preserve unrelated or changed files when cleanup cannot safely finish.

The agent used actual Windows pipes and a separate .NET Framework process, alongside the modern .NET client. These tests exercise the operating system connection; they do not prove the Revit API integration or the desktop demonstration.

## What changed after testing

The Windows runtime exposed assumptions that the earlier in-memory tests could not settle. The managed group list omitted the logon identity needed for access restrictions. Two cancellation mechanisms competed over a pipe handle on .NET Framework; removing one without supplying a reliable wakeup then left a read pending. The agent investigated the failure, changed the ownership strategy and reran the actual interruption cases. Other tests required retryable file cleanup and correlated errors when a valid request could not be admitted. The final ownership review added a duplicate-start test: an unsuccessful new endpoint must preserve the already-running endpoint's session.

The teaching lesson is to ask for evidence at the boundary being built. A successful serializer test is useful, but it cannot establish how Windows closes a connection or who can access it. Likewise, a successful Windows connection cannot establish that Revit reads the intended model.

A **suggested prompt** for a similar increment:

> Continue the next approved task. Prove the connection with real processes on the target operating system. Show how you select the intended application session, prevent silent retargeting, and recover after cancellation, timeout or failed cleanup. Keep the compatibility regressions. Record surprises and their causes, preserve independent review history, and state exactly what still requires the real BIM application.

Use the verification record to distinguish the author's decisions, the agent's implementation choices and independent review. Task 2 has independent closure. Task 3's implementation evidence requires its own review; full R3 and live-model acceptance remain ahead.

## Review the failure path, not just the fix

The author returned independent feedback declining Task 3 closure, even though all supplied tests passed. The reviewer had paused the connection just long enough for its client to exit. One listener stopped working, shutdown failed to remove its session record, and a restart appeared ambiguous.

The reviewer identified the missing concept explicitly:

> Cleanup still depends on preceding work completing successfully.

This is why the person should ask a reviewer to explain the connection between failures. The earlier rule of thumb about several major findings is a signal, not a threshold: this single finding revealed a shared assumption worth checking elsewhere.

The agent reproduced the failure before changing code, then tested two separate obligations: recovering the connection and cleaning up after an unexpected worker failure. Applying the same reasoning to neighboring paths exposed cleanup skipped when a cancellation callback threw. Those additional observations came from the implementing agent's tests, not from the external reviewer.

The useful follow-up is not merely “catch that exception.” A **suggested prompt**, rather than a transcript of what the author typed, is:

> Reproduce the finding first. Apply the reviewer's diagnosis across the operation: if a preceding step throws, which cleanup must still happen, and who owns it? Prove recovery and retained error reporting separately. Keep resources alive until unfinished work releases them. Record the failed tests, the corrections and what still needs independent review.

The [correction evidence](../../projects/revit-readiness/docs/verification/2026-10-21-local-pipe-transport-review.md) shows five new cases passing on both runtimes. The person can now hand the [follow-up packet](../superpowers/reviews/revit-mcp-bridge/2026-10-21-task-3-review.md) to the reviewer. The coder's successful corrections do not declare the review closed or start Task 4.
