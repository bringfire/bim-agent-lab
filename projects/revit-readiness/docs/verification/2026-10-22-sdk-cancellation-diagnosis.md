# SDK token cancellation: local completion can suppress remote notification

October 22, 2026. Implementing-agent diagnosis, not independent review or a production fix. The author said **“yes proceed to diagnose”** after the unexplained token-only result was identified as the next remaining item. This follows the [original failed attempts](2026-10-21-host-interruptions.md#test-driver-before-live-use); those captures remain intact.

## Finding and practical consequence

Official C# MCP SDK **2.2.0**, running on **.NET 10.0.13**, cancels the caller's local `CallToolAsync` wait without sending `notifications/cancelled` in this reproduced path. The receiving request stays active until an actual interruption reaches it. This happens with all three negotiated revisions: `2025-06-18`, `2025-11-25`, and `2026-07-28`.

The root cause is the interaction between the SDK's send-notification registration and its separate cancelable wait. It is a client-side SDK cancellation-order defect in this environment. No Revit API is needed to reproduce it. Protocol fallback is not its cause. These tests do not establish how the Stop buttons in Codex or Claude behave, nor whether another language SDK shares the defect.

The existing verification driver already uses an explicit, correlated notification through official SDK typed APIs. That path and EOF still work. Retain them and the original deadline/read guards. Do not equate a canceled client task with application invalidation. A server cannot react to a cancellation message that it never received; it can still enforce its own deadline and disconnect rules. No production source, dependency, installed file, desktop configuration or sample model changed for this diagnosis.

## Reproduced boundary

The new [diagnostic harness](../../tests/mcp-tests/SdkCancellationDiagnosis.cs) launches a child production MCP server and an owned real Windows pipe endpoint with a held synthetic executor. It selects that test process explicitly, not an existing Revit process. Official SDK `StreamClientTransport` and `CallToolAsync` own the MCP exchange. A successful context call precedes the held readiness call.

After actual pipe admission, the token case calls `CancellationTokenSource.Cancel()`. The explicit control sends an SDK-serialized cancellation for the exact observed request ID. Both keep stdin, the SDK session, executor response and executor quiescence open during a 750 ms observation window. Over 15 seconds of the original budget must remain. Captured wire prefixes distinguish the initial observation from the later rescue message.

| Observation | Token-only cancellation | Explicit SDK notification |
|---|---|---|
| Local wait throws cancellation | Yes | Yes |
| Cancellation messages before rescue | 0 | 1, matching request ID |
| Receiving context invalidated | No | Yes |
| Final-read guard rejects | No | Yes |
| Connection and executor remain held | Yes | Yes |
| Follow-up catalog works | Yes, after explicit rescue | Yes |

Each failed token case is then rescued with an explicit notification for that same request. The receiving context becomes inactive, its guard rejects, and the catalog still works. Release of the held executor and EOF happen only afterward. Every owned server exits naturally with code 0; none required forced termination. The absence claim is bounded by the captured window; source and the reduced experiment explain why no later notification is scheduled by that canceled registration.

## Causal explanation from inspected source

Pinned upstream C# source is commit [`6fa3825`](https://github.com/modelcontextprotocol/csharp-sdk/blob/6fa3825973949a9c4f0cd8af344e15a8db09dc35/src/ModelContextProtocol.Core/McpSessionHandler.cs). `RegisterCancellation` registers the callback that sends the notification. `SendRequestAsync` places that registration in a `using` block, then awaits `tcs.Task.WaitAsync(cancellationToken)`.

The matched runtime source establishes the other half:

1. [`CancellationPromise` in Task.cs](https://github.com/dotnet/runtime/blob/v10.0.13/src/libraries/System.Private.CoreLib/src/System/Threading/Tasks/Task.cs) registers the wait's own cancellation callback. This proxy does not inherit the original completion source's asynchronous-continuation setting.
2. [`CancellationTokenSource`](https://github.com/dotnet/runtime/blob/v10.0.13/src/libraries/System.Private.CoreLib/src/System/Threading/CancellationTokenSource.cs) invokes callbacks in reverse registration order and permits unregistering callbacks that have not run yet.
3. The wait's callback cancels the proxy. Its continuation can run inline, unwinding `SendRequestAsync` and disposing the send-notification registration before cancellation reaches it. [`CancellationTokenRegistration.Dispose`](https://github.com/dotnet/runtime/blob/v10.0.13/src/libraries/System.Private.CoreLib/src/System/Threading/CancellationTokenRegistration.cs) removes that pending registration.

The reduced, original .NET experiment records `cancel-enter → wait-unwound → registration-disposed → cancel-return`, with **no send callback**. Its control uses one cancellation callback to complete the original task instead of separately canceling the wait; the send callback runs before unwinding/disposal. This is a mechanism demonstration, not a proposed drop-in SDK patch: request-send races, cancellation-send failure, initialize's special rule and transport shutdown would all need upstream regression coverage.

The inspected upstream [`CancellationPropagation_RequestingCancellationCancelsPendingRequest` test](https://github.com/modelcontextprotocol/csharp-sdk/blob/6fa3825973949a9c4f0cd8af344e15a8db09dc35/tests/ModelContextProtocol.Tests/Protocol/CancellationTests.cs) asserts cancellation of the local wait but does not assert remote handler invalidation or an outgoing notification. That assertion cannot detect the failure reproduced here. This is a finding about that test, not a claim about the complete upstream suite.

Read-only current-source check: `git ls-remote` resolved upstream main to `2bd715b302c55210b03dc581bf25ee6e87a77789` and tag `v2.3.0` to `9a327829ef066c44a0d737c58bf3c7617d7c701c`. Both inspected `McpSessionHandler.cs` files retain the same registration/WaitAsync ordering. **No 2.3.0 package runtime test was performed.** Neither source inspection nor a version number establishes a fix. See [source provenance](sdk-cancellation-evidence/provenance.json) for URLs, revisions and hashes. No upstream issue or message was submitted.

## Reproduce without Revit

From the repository root, use a fresh evidence directory on each attempt:

```powershell
dotnet build projects/revit-readiness/tests/mcp-tests/BimAgentLab.Mcp.Tests.csproj -c Release -p:RestoreLockedMode=true
& projects/revit-readiness/tests/mcp-tests/bin/Release/net10.0/BimAgentLab.Mcp.Tests.exe --sdk-cancel-diagnosis projects/revit-readiness/samples/local/cancel-required-NEW --require-propagation
& projects/revit-readiness/tests/mcp-tests/bin/Release/net10.0/BimAgentLab.Mcp.Tests.exe --sdk-cancel-diagnosis projects/revit-readiness/samples/local/cancel-characterized-NEW --characterize
python projects/revit-readiness/tests/verify_sdk_cancellation_diagnosis.py projects/revit-readiness/docs/verification/sdk-cancellation-evidence/characterized-final
python -m unittest discover -s projects/revit-readiness/tests -p test_sdk_cancellation_evidence.py -v
```

The **require-propagation** probe intentionally exits 1 on the pinned SDK: three token cases fail; three explicit controls and the mechanism check pass. **Characterize** exits 0 only when it reproduces the known token failure and successful controls. That green result is not a fix. It is opt-in and separate from production acceptance. On a dependency/runtime change, rerun the propagation probe; investigate changed behavior rather than preserving the bug to satisfy characterization. The rescue is sent only when automatic notification and remote invalidation were absent, so a genuine future propagation pass does not receive a duplicate cancellation from the harness.

The Python checker independently parses raw wire prefixes, correlates request/notification IDs, verifies the post-cancellation catalog and absence of late success, checks original deadlines and natural cleanup, and verifies the reduced experiment's ordering. Its positive fixture and seven altered-evidence controls pass (two unittest methods). This is a second implementation's check, not external reviewer independence.

## Evidence, provenance and remaining work

[Retained evidence](sdk-cancellation-evidence/hashes.json) includes the initial harness file-sharing failure, actual propagation failures, characterization, mechanism and regression logs. The first compile attempt also needed `ValueTask.AsTask()`; compilation/setup failures are not evidence of the SDK defect. The live wire reader then needed `FileShare.ReadWrite` to coexist with the writer. Neither harness correction touched the SDK or server. Earlier captures lacking a pre-rescue wire snapshot are retained as earlier attempts; the current `characterized-final` capture includes that snapshot and conditionally sends rescue only when propagation was absent. `require-propagation-final` is the current intentionally failing probe. The exit records preserve expected 1 versus 0 separately from the shell's successful collection of that evidence.

Fresh **72 Tools cases** and **Protocol** regressions pass, including explicit cancellation/EOF, strict July and legacy rejection. The source/dependency boundaries are unchanged: official C# SDK 2.2.0 package consumption (Apache-2.0); upstream SDK source inspection (Apache-2.0) and .NET source inspection (MIT); original diagnostic harness and checker; no vendored or modified SDK implementation. TypeScript 1.26.0 remains precedent-only. No full Deployment/All run or live Revit/UI test was needed or performed while Revit remained open.

The unexplained observation is now diagnosed; reliable token-only propagation is **not fixed or accepted**. Carry this limitation and its probe into R3. A future SDK change needs actual remote-propagation tests, not only local cancellation assertions. Remaining host/native overlap limits, historical whole-install recovery layers and R3 acceptance remain separate work. The [participant account](../../../../docs/tutorials/13-install-with-a-recovery-plan.md#diagnose-the-dependency-before-changing-the-plugin) explains how the person directed this investigation.
