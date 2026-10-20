# Verification strategy

The probe has a [manual Revit acceptance procedure](read-only-probe.md) and an [observed verification record](../docs/verification/2026-10-20-read-only-probe.md). The first readiness check adds 18 executable rule/aggregation tests, a [host acceptance procedure](required-information.md), a separate [fixture editor](fixture-tools/PrepareInformationFixture.cs), and an [independent schedule comparator](verify_information_evidence.py). See its [dated record](../docs/verification/2026-10-20-required-information.md) for observed results. No CI or MCP contract suite exists yet.

From the project folder, run `& .\scripts\Test-Readiness.ps1`. This builds and runs a net48 console test executable; it needs framework targeting/runtime support but no running Revit instance or third-party test library. It does not use `dotnet test`.

| Level | What it establishes | Requires Revit? |
|---|---|---|
| Rule and aggregation tests | Missing versus not-required values; type/instance effects; valid exceptions; counts and units | No, using controlled normalized inputs |
| Interface/contract tests | Request scope, result structure, completeness, errors, timeouts, and repeated requests | Some can use a test double; real bridge behavior requires Revit |
| Host integration | Correct API values, valid dispatch, document selection, and unchanged model after queries | Yes |
| Independent acceptance | Agreement with known fixture cases, manual inspection, and matching schedules | Yes |

The [information-check contract](../docs/required-type-information.md) is this increment's acceptance target. The [larger demo brief](../docs/demo-questions-and-sample-model.md) also includes physical conditions and quantities that remain planned. Test known defects and valid exceptions, and verify exclusions and partial results explicitly. Repeated type findings must not duplicate affected instance totals.

Record the host version, sample revision, build revision, scope, units, expected/observed results, and limitations using the [verification template](../../../templates/verification-record.md). Offline tests alone do not establish Revit compatibility.
