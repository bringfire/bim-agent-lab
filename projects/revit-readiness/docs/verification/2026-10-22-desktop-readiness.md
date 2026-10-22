# Actual desktop readiness verification — in progress

Resumed October 22, 2026 from the [night checkpoint](../../../../docs/build-story/2026-10-21-night-checkpoint.md). The author asked to test Codex desktop and Claude Desktop and operate Revit manually. No Computer Use is involved.

## Fresh live bridge before desktop registration

The author opened the disposable `BAL-info-saveas-fresh.rvt`, started the bridge and replied “ready.” Actual Revit PID **149972**, session **a3163353950948eabd8cfb8a4955d860**, serves the installed external server from source **d88de67** with normal host **9a93f6d**. All **97 installed target hashes** match receipt **68dac9...**. No deployment or installation test ran while Revit was open.

An official-SDK `2026-07-28` capture passed discovery, context and readiness. The independent Python wire checker verifies response identities and text/structured/saved-result agreement. Every report field matches the retained earlier baseline except `GeneratedUtc`: **16 examined instances, zero findings, both completeness flags true, both modified flags false**. [Comparison](desktop-readiness-evidence/2026-10-22/continuity-comparison.json), [complete capture](desktop-readiness-evidence/2026-10-22/sdk-july-continuity/complete.json), [full report](desktop-readiness-evidence/2026-10-22/sdk-july-continuity/check.json). This is a continuity check; it is not the independent five-case BIM oracle or actual desktop acceptance.

## Desktop configuration prepared and applied

Current installed applications are **Codex 26.1016.6573.0** and **Claude Desktop 3.1623.4.0**. Claude updated since the previous night's inspection. Each requires actual verification; the synthetic CP0 record does not establish acceptance of these versions.

Opened-file handles resolve the installed executable and both configuration files directly, without the observed AppData redirection: [physical paths](desktop-readiness-evidence/2026-10-22/before-settings-physical-paths.json). The tested settings helper prepared private original-byte recovery receipts for **both** applications before either settings mutation. Separate comparisons removed only the proposed owned entry from parsed after-state and proved equality with each original configuration. The helper then applied one `bim-agent-lab-revit-readiness` entry per application. Both use the absolute installed executable, **`--revit-pid 149972`**, and separate **`--wire-evidence-root`** parents. [Owned entry observations](desktop-readiness-evidence/2026-10-22/settings-observation.json) omit private full configuration contents.

At registration, the author was asked to reload Codex's MCP servers and fully quit/reopen Claude Desktop, keeping Revit and its model open. Actual desktop calls were then pending; the Codex result below supersedes that status. No tool permission settings or protocol overrides were changed. Full recovery records remain private under the active plan workspace; never publish their full configuration backups or restore an entire stale configuration over unrelated changes.

## Codex desktop: initial real readiness exchange passes

The author reported restarting Codex. In this desktop conversation, the agent discovered the two production tools, called `revit_get_readiness_context`, then supplied that returned token to `revit_check_required_type_information`. These were actual Codex tools, not an SDK subprocess substituted for the desktop.

The retained [single-session snapshot](desktop-readiness-evidence/2026-10-22/codex-initial/snapshot.json) includes initialization, initialized notification, catalog and both calls. Codex's wire client is **`codex-mcp-client` / `0.163.0-alpha.11.1`**; both sides use **`2025-06-18`**, allowed by the approved compatibility amendment. The desktop application package remains **26.1016.6573.0**. These are distinct version identifiers. The server still consumes C# SDK **2.2.0**; the earlier live July capture and strict July regression are retained.

Context request `312449bac55a46fba291101010c80149` and report request `2c9d5c963ed74bcb866c3a431988573e` bind the same Revit session, checklist hash and active `BAL-info-saveas-fresh.rvt`. The [complete report](desktop-readiness-evidence/2026-10-22/codex-initial/check.json) has **16 requested/examined instances, six types, 18 field rows, zero findings, no unexamined cases, 1,196 excluded other host wall/ceiling instances and six excluded links**. Both completeness flags are true; both modified flags are false. This supports a pass for the declared required-information scope on `L1 - Block 43`, phase `New Construction`, not a whole-model readiness or quantity certification.

The [independent wire comparison](desktop-readiness-evidence/2026-10-22/codex-initial/comparison.json) verifies response correlation, the exact returned token, both tool names, text/structured/saved-result agreement, model identity and checklist binding. Every report field equals the retained live SDK result except `GeneratedUtc`. This is **continuity evidence**, not a fresh independent BIM oracle.

The new [checker](../../tests/verify_desktop_continuity.py) operates on an immutable snapshot of one session, with complete JSONL lines and matching request/response IDs. It does not combine discovery from one process with answers from another. A directory listing initially reported zero bytes for the open response file, but opening and reading that file returned all **26,110 bytes**. Source inspection confirms explicit trace flushing; no runtime correction was made. The saved snapshot is a prefix of an active session, not evidence of EOF or shutdown.

Four [test methods](../../tests/test_verify_desktop_continuity.py) pass, covering the actual capture and **23 negative controls**. Semantic controls recompute checksums so rejection must come from the changed protocol, missing/duplicate responses, wrong identity/token, altered values or false flags, rather than a stale hash alone. Consistently altered saved and wire reports are also rejected. [Test record](desktop-readiness-evidence/2026-10-22/codex-initial/checker-tests.json). No installed binaries, dependencies or settings changed in this increment; no installation tests ran with Revit open.

The author said Claude was working and asked to wait. Claude is left running; its actual readiness result is not yet claimed. The requested next step is a fresh normal Claude Desktop chat using the two tools, with the answer retained. No further Revit action is needed for this initial comparison. Codex's three principal samples, NC1/NC2 and stale-token-after-document-switch check also remain outstanding.

The [evidence hashes](desktop-readiness-evidence/2026-10-22/hashes.json) cover this checkpoint. Task 7's remaining host observations, both desktops' A8 five-case/error/presentation checks and R3 remain open. Do not treat server discovery alone as completion.
