# Required-information evidence, 2026-10-20

These are observations from the [dated verification](../2026-10-20-required-information.md), not freshly generated expected answers. The independently authored [answer sheet](../../../samples/expected-results/required-information.md) defines the intended cases.

| Evidence | Contents |
|---|---|
| `BAL-info-*.checklist.json` | Explicit document/file, level/phase, case UniqueIds, and sourced type requirements |
| `BAL-info-*-walls.tsv` / `*-ceilings.tsv` | Unmodified native Revit schedule exports, one row per marked case |
| `BAL-info-*.readiness.json` | Actual command output with raw type values, rules, affected cases, and exclusions |
| `source-inspection.json` | Source type/parameter observations and level/phase inventory |
| `ceiling-negative-first-run.json`, `repeat-comparison.json` | First read and comparison with the second read; only GeneratedUtc differs |
| `hashes-before-readiness.json`, `hashes-after-close.json` | Unchanged prepared RVTs, initial copy, and original installed sample |
| `assembly-hashes.json` | Built and installed DLL equality |
| `independent-comparison.json` | Three passing comparisons against the literal answer sheet |
| `*-result.jpg`, `native-*-schedule.jpg` | Actual Revit dialogs and native schedule views |

From the repository root, reproduce the text comparison without Revit:

```powershell
python .\projects\revit-readiness\tests\verify_information_evidence.py .\projects\revit-readiness\docs\verification\required-information-evidence
```

The recorded absolute model paths identify the authoring run; they are not portable participant paths. Regeneration creates new UniqueIds and hashes, so rerun the acceptance procedure on a participant's copies. RVTs and Autodesk API binaries are not included. Some schedule cells are clipped in the UI screenshot; the native TSV exports preserve their full contents.
