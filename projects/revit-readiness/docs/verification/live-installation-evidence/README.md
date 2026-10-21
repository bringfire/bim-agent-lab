# Task 7 live lifecycle evidence

Read the [verification record](../2026-10-21-live-installation.md) for scope and limitations. `capture-index.json` names 38 successful SDK captures and distinguishes failed/partial attempts and failed interpretations; `hashes.json` records retained bytes. These are official-SDK exchanges against installed source `9a93f6d` and actual Revit 2024.3, not desktop acceptance or independent BIM-answer comparisons.

Receipts and journals are metadata only. Original backup bytes, staged binaries and working Revit models stay in private local storage. The retained before/after snapshots independently record exact restoration of the actual installation. The final read-only recheck matches all 97 installed targets and all six original sample/model-sidecar source files; the later final-closed-model-hashes.json records 14 source/working-file comparisons after normal Revit shutdown.

Do not repair or overwrite a failed capture. A deliberate retry has a different directory. Context tokens and session IDs here are historical, expire, and are not reusable client configuration. No desktop settings were changed for these tests. The manual-export folder retains the actual XLSX as well as its JSON/text outputs and independent decoded XML data. The follow-up driver is test-only; installed production bytes remain from 9a93f6d. The separate save-as-fresh-20261021T213613Z run records human-operated Save As on PID 178136; the index keeps the original PID 82836 under initialRevitPid. Exact pre-save token rejection completed before expiry, then a new context/report verified continuity. Both prepared checklist snapshots are retained; no closed-file hash is claimed for the still-open new model.

Recheck from the repository root:

```powershell
python projects/revit-readiness/tests/verify_live_lifecycle_capture.py projects/revit-readiness/docs/verification/live-installation-evidence
python projects/revit-readiness/tests/test_verify_live_lifecycle_capture.py
```

Task 7 remains open. The verification record lists the pending observations, including instrumented read-boundary cancellation and shutdown.
