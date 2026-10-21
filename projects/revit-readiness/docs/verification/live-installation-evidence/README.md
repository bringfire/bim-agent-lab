# Task 7 initial live evidence

Read the [verification record](../2026-10-21-live-installation.md) for scope and limitations. `capture-index.json` names every successful capture and both failed/partial attempts; `hashes.json` records retained bytes. These are official-SDK exchanges against installed source `9a93f6d` and actual Revit 2024.3, not desktop acceptance or independent BIM-answer comparisons.

Receipts and journals are metadata only. Original backup bytes, staged binaries and working Revit models stay in private local storage. The retained before/after snapshots independently record exact restoration of the actual installation. The final read-only recheck matches all 97 installed targets and all six original sample/model-sidecar source files; the two currently open working models have not yet received their final post-close disk-hash check.

Do not repair or overwrite a failed capture. A deliberate retry has a different directory. Context tokens and session IDs here are historical, expire, and are not reusable client configuration. No desktop settings were changed for these tests.

Recheck from the repository root:

```powershell
python projects/revit-readiness/tests/verify_live_lifecycle_capture.py projects/revit-readiness/docs/verification/live-installation-evidence
python projects/revit-readiness/tests/test_verify_live_lifecycle_capture.py
```

Task 7 remains open. The verification record lists the pending observations, including instrumented read-boundary cancellation and shutdown.
