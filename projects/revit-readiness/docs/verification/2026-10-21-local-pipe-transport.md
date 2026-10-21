# Task 3: Windows-local pipe transport — 2026-10-21

**Subsequent review:** the external reviewer found a peer-exit/exceptional-cleanup P2 in this checkpoint despite the supplied suites passing. The [correction record](2026-10-21-local-pipe-transport-review.md) supersedes its implementation-completeness claim; original evidence remains preserved below. Independent follow-up is pending.

**Implementation verification; independent Task 3 review pending.** This establishes the tested Windows IPC increment, not live Revit or desktop A8 acceptance. Task 2 independently closed at `3cefe5116ec1368e0bfdcc387209bbb1e176df66`; its [follow-up](../../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-21-task-2-follow-up.md) is retained separately.

Source baseline: **`3cefe5116ec1368e0bfdcc387209bbb1e176df66`**, branch `feat/revit-mcp-bridge`. The implementation checkpoint is the commit titled **`feat: add restricted local Revit pipe transport`** that first adds this file. Find its exact hash with `git log --diff-filter=A --format="%H %s" -- projects/revit-readiness/docs/verification/2026-10-21-local-pipe-transport.md`.

## Implemented and observed

- Original native pipe factory with overlapped byte-mode I/O, intended-logon DACL, remote-client rejection configured, eight-instance bound and owned-handle cleanup.
- Restricted, atomically published descriptors; actual PID/start/session/logon validation; explicit selection; foreign/changed descriptor preservation and retryable cleanup.
- Lazy provider with a single binding, retry before binding, no automatic replay or retargeting after a stopped session.
- One five-second frame timer; original deadline/accept cap; cancellation and stop across receive, submission, reply and cleanup; lifetime retained until callback quiescence.
- A net48 child process serves simultaneous clients, including the net10 client run, preserving correlation and a Unicode result above 8192 bytes. A separate child impersonating a descriptor's pipe name receives zero request bytes because its actual PID is rejected.

## Verification

`Test-McpBridge.ps1 -Suite All` completed successfully with no build warnings/errors:

| Check | Observed result |
|---|---|
| Windows Pipe suite | **27 named cases per target**, net48 and net10.0 |
| Stalled-slot recovery | **64 recorded connections**, eight per scenario per target: no bytes, partial header, partial body, slow trickle; zero submissions; subsequent valid request succeeds; ninth client cannot bypass the cap |
| Independent timing oracle | Recomputes every accept/close interval from monotonic ticks, requires complete groups and successor checks; all cleanup durations satisfy the spec's five seconds plus one-second scheduling allowance |
| Foundation regressions | **33 cases per target**, both exchange directions and independent Python checks |
| MCP regressions | **19 C# protocol/stream cases**, ten Python protocol/settings methods, four SDK/wire comparisons; compatible June/November/July and strict July/legacy rejection retained |
| Existing readiness/Excel | **18 core tests, seven Excel cases**, process-isolated export from an unconfigured host |

The [timing summary](local-pipe-transport-evidence/timing-summary.json) contains measured maxima. The [inventory](local-pipe-transport-evidence/inventory.json) lists exact retained bytes/hashes. It includes test output, red reproductions, runtime/environment details, current assembly references, fresh foundation exchange and four protocol captures. The Python timing checker also rejects six deliberately corrupted/incomplete copies of the evidence.

## Reproductions and implementation choices

Initial catalog/identity tests reached behavioral RED on both targets after fixing test setup. Native TokenLogonSid supplied the independent identity oracle; the managed group list omitted that group. Original transport stubs failed the connection/lifetime cases before implementation.

Actual cancellation tests exposed a net48 closed-SafeHandle exception when explicit disposal competed with a framework CancelIoEx callback. Removing disposal then left an operation pending. Only the owned test executables were stopped during diagnosis. The final strategy uses owner-triggered closure for connected I/O, no competing native cancellation callbacks, and awaits I/O completion; connect retains a bounded cancellable attempt. Successful full runs leave no test processes running.

Further red cases established missing correlated deadline/submit errors and non-repeatable cleanup after a file lock. Those pass after correction. The short-deadline connect case uses the existing lifetime to avoid translating the same deadline into a second coarse timeout. The ninth-slot test accepts the observed net48 Win32 semaphore-timeout HRESULT or modern TimeoutException, while requiring no connection. Identity-negative cases retain internally consistent pipe names so they exercise actual OS identity validation rather than merely schema rejection.

Final author-side review added a failing duplicate-start regression: failed acquisition through a shared catalog could remove an existing endpoint's descriptor. Cleanup now requires this endpoint's successful publication. The regression verifies both failed Start and subsequent Stop preserve the original session and allow a valid call. This was an author-side correction, not an independent review finding.

No dependency, SDK pin, protocol policy, installed add-in or desktop registration changed. Native API contracts and the pinned Framework source were research references; no upstream implementation was copied. See [research](../../../../docs/research/revit-local-pipe-transport.md), [participant story](../../../../docs/tutorials/09-connect-the-bridge-processes.md) and [agent guide](../../../../docs/how-to/build-and-verify-local-pipe-transport.md).

## Limits and next boundary

Tests use synthetic executors and isolated catalog directories under ignored samples/local. They do not call Revit, create a production dispatch queue, install a bridge, modify a model or repeat desktop acceptance. The default production catalog path is implemented but is not populated by these tests.

Local native access denial with a foreign-logon DACL and actual descriptor ACL inspection were performed. **An actual alternate-logon client and remote-machine connection were not tested.** Remote rejection is configured and source-reviewed, not claimed as observed cross-machine denial. No accounts or security policies were changed.

Task 4's bounded dispatch queue and later Revit/MCP integration remain ahead. Use the [review packet](../../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-21-task-3-review-packet.md) for independent review of this increment. Full R3 and A1–A9 remain open to the extent described in the approved plan.
