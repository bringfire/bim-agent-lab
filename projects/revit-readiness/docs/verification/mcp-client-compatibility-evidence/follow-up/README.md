# Protocol policy and desktop settings follow-up

Observed 2026-10-20, America/New_York. See the [research interpretation](../../../../../../docs/research/mcp-desktop-protocol-investigation.md), [participant story](../../../../../../docs/tutorials/07-test-desktop-compatibility-first.md), and [agent procedure](../../../../../../docs/how-to/verify-desktop-mcp-compatibility.md).

- `sdk-default-comparison.json`: byte-for-byte result from the separate SDK diagnostic. All six expected outcomes were observed; process exit 0. This is not desktop acceptance.
- `codex-features-experiment.txt`: only the two relevant lines from the installed engine's feature listing during the experiment. Original local-server value was false; temporary value is true. The separate apps flag stayed false.
- `codex-feature-reload/`: six fresh desktop connections after the author reported a reload. Every connection still used the old initialization exchange and was rejected before tool discovery. Original protocol bytes are retained.
- `codex-feature-restart/`: six additional fresh connections after the author reported a full app restart, with the same older exchange and rejection.
- `codex-features-restored.txt`: the same two engine feature-list lines after temporary settings were removed; both false.
- `observations.json`: update/settings observations and SHA-256 inventory for the twenty-seven evidence files. Update values are transcribed observations, not raw full application logs. No private configuration or recovery bytes are included.

The temporary feature/probe settings were removed after testing, preserving unrelated changes; no probe process remained. The tested setting, reload and full restart did not resolve the mismatch in this desktop installation. No newer public Claude Windows package or documented resolving local-server setting was found in the inspected sources. The strict protocol requirement, production pin and original CP0 evidence remain unchanged.
