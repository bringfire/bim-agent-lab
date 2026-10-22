# Actual desktop readiness verification — in progress

Resumed October 22, 2026 from the [night checkpoint](../../../../docs/build-story/2026-10-21-night-checkpoint.md). The author asked to test Codex desktop and Claude Desktop and operate Revit manually. No Computer Use is involved.

## Fresh live bridge before desktop registration

The author opened the disposable `BAL-info-saveas-fresh.rvt`, started the bridge and replied “ready.” Actual Revit PID **149972**, session **a3163353950948eabd8cfb8a4955d860**, serves the installed external server from source **d88de67** with normal host **9a93f6d**. All **97 installed target hashes** match receipt **68dac9...**. No deployment or installation test ran while Revit was open.

An official-SDK `2026-07-28` capture passed discovery, context and readiness. The independent Python wire checker verifies response identities and text/structured/saved-result agreement. Every report field matches the retained earlier baseline except `GeneratedUtc`: **16 examined instances, zero findings, both completeness flags true, both modified flags false**. [Comparison](desktop-readiness-evidence/2026-10-22/continuity-comparison.json), [complete capture](desktop-readiness-evidence/2026-10-22/sdk-july-continuity/complete.json), [full report](desktop-readiness-evidence/2026-10-22/sdk-july-continuity/check.json). This is a continuity check; it is not the independent five-case BIM oracle or actual desktop acceptance.

## Desktop configuration prepared and applied

Current installed applications are **Codex 26.1016.6573.0** and **Claude Desktop 3.1623.4.0**. Claude updated since the previous night's inspection. Each requires actual verification; the synthetic CP0 record does not establish acceptance of these versions.

Opened-file handles resolve the installed executable and both configuration files directly, without the observed AppData redirection: [physical paths](desktop-readiness-evidence/2026-10-22/before-settings-physical-paths.json). The tested settings helper prepared private original-byte recovery receipts for **both** applications before either settings mutation. Separate comparisons removed only the proposed owned entry from parsed after-state and proved equality with each original configuration. The helper then applied one `bim-agent-lab-revit-readiness` entry per application. Both use the absolute installed executable, **`--revit-pid 149972`**, and separate **`--wire-evidence-root`** parents. [Owned entry observations](desktop-readiness-evidence/2026-10-22/settings-observation.json) omit private full configuration contents.

The author has been asked to reload Codex's MCP servers and fully quit/reopen Claude Desktop, keeping Revit and its model open. **Actual desktop tool exchanges are pending.** No tool permission settings or protocol overrides were changed. Full recovery records remain private under the active plan workspace; never publish their full configuration backups or restore an entire stale configuration over unrelated changes.

The [evidence hashes](desktop-readiness-evidence/2026-10-22/hashes.json) cover this checkpoint. Task 7's remaining host observations, both desktops' A8 five-case/error/presentation checks and R3 remain open. Do not treat server discovery alone as completion.
