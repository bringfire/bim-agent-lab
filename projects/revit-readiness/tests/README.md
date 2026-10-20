# Verification strategy

The first command has a [manual Revit acceptance procedure](read-only-probe.md) and an [observed verification record](../docs/verification/2026-10-20-read-only-probe.md). The [tutorial](../../../docs/tutorials/02-build-a-read-only-revit-command.md) documents check/build/install commands. No automated unit-test suite exists yet; the current probe is verified inside its host. The layers below describe the larger readiness tool's intended verification.

| Level | What it establishes | Requires Revit? |
|---|---|---|
| Rule and aggregation tests | Missing versus not-required values; type/instance effects; valid exceptions; counts and units | No, using controlled normalized inputs |
| Interface/contract tests | Request scope, result structure, completeness, errors, timeouts, and repeated requests | Some can use a test double; real bridge behavior requires Revit |
| Host integration | Correct API values, valid dispatch, document selection, and unchanged model after queries | Yes |
| Independent acceptance | Agreement with known fixture cases, manual inspection, and matching schedules | Yes |

The [demo brief](../docs/demo-questions-and-sample-model.md) is the current acceptance target. Test both known defects and valid exceptions. Verify exclusions and partial results explicitly. A fixture with four findings must not count repeated type findings as extra affected instances.

Record the host version, sample revision, build revision, scope, units, expected/observed results, and limitations using the [verification template](../../../templates/verification-record.md). Offline tests alone do not establish Revit compatibility.
