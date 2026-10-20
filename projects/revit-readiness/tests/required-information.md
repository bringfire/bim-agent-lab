# Required-information acceptance procedure

This procedure verifies the [first type-information check](../docs/required-type-information.md). Expected counts are declared before execution in the [answer sheet](../samples/expected-results/required-information.md). Actual observations belong in a dated [verification record](../docs/verification/2026-10-20-required-information.md).

## Prepare and preserve evidence

1. Record Git revision, Revit/API build, selected SDK, source sample path and SHA-256. Run `scripts/Test-Readiness.ps1` and `scripts/Build-Readiness.ps1 -IncludeFixtureTools`.
2. Follow the [sample recipe](../samples/model-recipes/required-information.md). Build and install before opening Revit. The separate fixture command edits only the named source copy and saves three prepared RVTs; the readiness command uses `TransactionMode.ReadOnly`.
3. Before running readiness, retain the generated native schedules and checklists, and hash all three RVTs. Revit may hold an open RVT with restrictive sharing; use a read-only file stream with `FileShare.ReadWrite` or close the file before hashing. Do not alter model content to work around file access.

## Test the three saved projects

Open each prepared file and run **Add-Ins > External Tools > BIM Agent Lab - Required type information**. Retain its JSON/text reports and a screenshot of the result. Each must examine exactly 16 declared instances across six types with complete coverage and no unexamined scope cases.

| Project | Missing type values | Distinct affected instances |
|---|---:|---:|
| `BAL-info-complete.rvt` | 0 | 0 |
| `BAL-info-wall-defects.rvt` | 2 | 6 |
| `BAL-info-ceiling-negative.rvt` | 3 | 8 |

Inspect the exact type/field identities, raw text, requirement source, rule IDs, category, and case membership. A matching total with the wrong elements is a failure. Wall C's blank optional rating/assembly must remain valid. The ceiling negative control must add two ceilings, not change the wall answer.

Run a command twice without editing. Compare stable report fields (ignore only `GeneratedUtc`); preserve the first report before it is overwritten. The modified flags must remain false/false on these freshly saved fixtures. Close the model without saving and recheck RVT hashes, including the original installed sample and initial source copy.

## Check independently

Open `BAL INFO Walls` and `BAL INFO Ceilings` schedules in Revit. They contain native parameter fields and one row per case, filtered by instance Mark. Inspect the missing cells and valid exceptions. The readiness reader and evaluator do not generate these schedule cells.

From the project folder, run:

```powershell
python .\tests\verify_information_evidence.py .\samples\local\required-information
```

This standard-library script compares all three native schedule exports and plugin reports against literal values from the answer sheet. It imports neither the evaluator nor fixture-generation code. Three PASS records are required. Python is a verification convenience, not an add-in runtime dependency; the same comparison can be made manually.

Keep expected answers, native observations, and plugin output distinct. Record failures and corrections rather than revising the expected answer to fit an unexplained result. Passing this procedure establishes only text completeness within the declared scope on the recorded host build; it does not validate rating/assembly correctness, the rest of Snowdon, geometry, quantities, or MCP.
