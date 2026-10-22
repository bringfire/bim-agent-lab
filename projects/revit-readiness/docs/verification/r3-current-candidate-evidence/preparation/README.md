# Frozen normal candidate preparation

These records establish preparation and offline checks, not installation or live desktop acceptance. Read [the candidate record](../../2026-10-22-r3-current-candidate.md).

- `freeze.json`, `package-files.json`, `source-inputs.json` and `additional-build-inputs.json`: clean build commit, frozen outputs and source identities. No binary or proprietary RVT is published here.
- Build logs and `dotnet-info.txt`: normal project selection, locked build/publish, SDK/runtime environment and four retained CA1416 warnings.
- `prior-installation.json`, `prior-receipt.json`, `preflight.json` and physical paths: existing installation/recovery checks and the proposed 97-target, 17-change plan. No candidate recovery receipt or installed freeze exists at this checkpoint.
- `all.txt`, `readiness.txt`: fresh full offline suites, strict July/legacy rejection and manual-export regressions.
- `frozen-offline`: SDK discovery and correlated BridgeUnavailable against the exact frozen compatible server, with Revit closed, on all three supported revisions. This is neither a successful Revit read nor either desktop's acceptance.
- `case-preparation`: five independent expected reports, unchanged sample hashes and the pending capture checklist. PID/session and native NC1 absence confirmation must be established before live calls.
- `capture-scripts`: exact local preparation/retention helpers. The installation wrapper was prepared and syntax-checked only; it was not executed. These are historical, machine-specific capture scripts, not a replacement for the supported installer.
- `verification.json`: implementing-agent rechecks after the suites; not an independent review.

The private package and rollback bytes remain outside Git. The original 53 follow-up evidence files remain unchanged. Later installation and live captures must use separate directories/manifests; do not rewrite this preparation snapshot.
