# Desktop-first compatibility without abandoning the new protocol

Date: 2026-10-20. Status: **author-directed policy amendment; compatible host and synthetic desktop CP0 verified**. Applies to the [bridge specification](2026-10-20-revit-mcp-bridge-design.md) and [implementation plan](../plans/2026-10-20-revit-mcp-bridge.md). Their prior R1/R2 closures apply to revision 2; this amendment is not represented as independently reviewed. [Fresh CP0 evidence](../../../projects/revit-readiness/docs/verification/2026-10-20-desktop-mcp-compatibility.md) proves synthetic compatibility; full live acceptance remains pending.

## Author direction and purpose

After [Claude Code passed the new protocol](../../../projects/revit-readiness/docs/verification/2026-10-20-claude-code-protocol.md), the author clarified:

> just FYI, the participants are very green and will not like the Claude Code terminal style interface. We should fallback in such a way that we don't kill the possibility that we can switch when Codex App and Claude Desktop get their shit together.

The selected direction is **Codex desktop and Claude Desktop for participants, using compatible protocol exchanges while preserving July 2026 support**. The author has chosen this direction; do not ask them to choose a terminal client again. The exact SDK remains **2.2.0**. The implementation approach below makes that choice concrete without adding participant-facing protocol controls.

## Compatibility contract

1. Keep the official SDK and one server implementation. Use SDK 2.2.0's normal multi-version behavior (`McpServerOptions.ProtocolVersion = null`, or omit the option) for the production host. Do not force an older date globally, install an SDK 1.x dependency, copy a legacy transport, or translate messages in a custom proxy.
2. The tested desktop acceptance candidates are `2025-06-18` for Codex desktop and `2025-11-25` for Claude Desktop. `2026-07-28` is also acceptable in either desktop. These are specific verification targets, not a claim that every version the SDK recognizes has been certified. An unlisted client/revision needs explicit verification and a recorded baseline update.
3. Keep a strict **2026-07-28** test policy in the same host composition path, exercised by the separate synthetic probe and SDK client. The strict policy must still reject legacy initialization. Compatibility mode must independently pass discovery, success and deliberate failure on all three targeted revisions. Retain the successful Claude Code evidence as additional cross-language proof; participants do not need Claude Code.
4. The production host must remain capable of new-protocol exchanges while accepting the desktop clients' older exchanges. Do not let a legacy-only test replace the new-protocol regression check. Protocol choice is SDK behavior, not a model instruction or a setting a novice must understand.
5. Readiness results, context validation, deadlines, cancellation, read-only boundaries, provenance and incomplete-coverage semantics remain identical. A compatibility connection is not permission to omit a field, coerce a false flag or lower model-verification standards.

## Revised acceptance

**A2:** SDK-based tests through the actual host verify structured success/error, discovery, clean stdio and shutdown on the three listed revisions. The strict July test remains required, including rejection of old initialization. The existing separate SDK diagnostic is supporting research, not a substitute for tests of our host.

**CP0:** Both actual desktop applications must independently discover the synthetic tool and make successful and deliberate-error calls against the same compatible host build. Record their exact versions, configuration and actual wire revision. A July SDK/Claude Code pass cannot substitute for either desktop. The original strict-server failures remain valid historical evidence; the amended synthetic checkpoint now **passes**, as documented in the fresh evidence above.

**A8/CP2:** Each desktop must perform the full live sequence already required: context, three principal samples, structured failure and NC1/NC2, with correct incomplete-coverage presentation. Its actual protocol must be one of the recorded accepted candidates above. Full live SDK-client coverage on July remains required under A2/A6. All other A1–A9 obligations remain.

The transcript checker must select assertions by the explicitly expected revision. July checks retain per-request metadata and reject an initialization handshake. Legacy checks validate the requested and selected initialization revision and completed lifecycle before list/calls. Both paths correlate identities and preserve exact results/error booleans. Never infer the protocol from the package version or change a captured date to make a comparison pass.

## Returning desktop demonstrations to July

When a newer desktop release or documented setting becomes available, run the strict synthetic probe against that actual application first. If it passes, run its complete A8 sequence against the compatible production build and update the accepted client/version/configuration evidence. Change the seminar baseline only after those checks pass. The server already supports July, so adopting a supporting client should not require a BIM evaluator, IPC or server-language rewrite.

Support in a release note is a candidate for testing, not automatic acceptance. Do not promise an upgrade date, activate experimental flags for participants, or claim every future protocol will work without another SDK/dependency review. No recurring monitoring is created by this policy.

## Teaching and review boundaries

Participants learn that the human decides which tradeoff serves the audience. Here a successful technical alternative was rejected because its interface was unsuitable for beginners. Agents get the exact compatibility policy, separate test expectations and upgrade procedure.

Include this amendment and its changed A2/A8/CP0 assertions in external implementation review. Preserve the prior review feedback/closure records unchanged. The architecture and lifetime protections remain reviewed requirements; the policy text alone does not close CP0 or R3. The later actual compatible-desktop proof closes CP0 and permits Task 2; R3 remains outstanding.
