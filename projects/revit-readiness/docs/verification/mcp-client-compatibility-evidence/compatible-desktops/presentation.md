# Selected desktop presentation observations

These are test-only observations transcribed by the implementing agent, not full chat exports or independent reviewer findings. Unrelated sidebar/chat content is omitted.

Codex desktop: the actual tool results in the active conversation contained `ProbeSuccess` / `isError=false`, then `ProbeFailure` / `isError=true`. The assistant reported the deliberate failure as the expected test result.

Claude Desktop: fresh regular Claude-tab conversation **BIM Agent Lab compatibility test**, **Opus 5.5 High / Manual**, with the local-device connection visible. Test prompt entered by the agent under the author's instruction to test this app:

> For this BIM Agent Lab compatibility test, use lab_protocol_probe from bim-agent-lab-protocol-probe. Call it once with fail=false and once with fail=true. The second call is an intentional test error. Report the returned code and error status for each. This synthetic test has no Revit access. Do not use any other tools.

The author handled the permission prompt and replied **“always allowed.”** The visible answer said both calls returned as expected. Its table showed:

| Call | Code | Presented status |
|---|---|---|
| fail=false | ProbeSuccess | No error; normal result |
| fail=true | ProbeFailure | Error; intended test failure |

The answer also identified `synthetic: true` and `revitAccess: false`. It referred to a “remote-devices bridge”; that wording is the client's own description, not proof of additional architecture or a different protocol. The complete local capture identifies the runtime as `local-agent-mode-bim-agent-lab-protocol-probe` 1.0.0 on `2025-11-25`.
