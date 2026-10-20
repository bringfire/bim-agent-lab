# Independent answer sheet: required type information

Authored from the declared teaching cases before running the readiness command. Verify these values in the native Revit schedules; do not generate this answer sheet from a readiness report.

Each wall type has three instances. Each ceiling type has two. The six types cover 16 explicitly marked instances; the remainder of the Snowdon sample is outside this check.

| Type | Case prefix | Complete code | Complete rating | Complete assembly | Required fields |
|---|---|---|---|---|---|
| BAL INFO Wall A | BAL-INFO-WA | W-A | 60 min | DEMO-ASM-01 | Code, rating, assembly |
| BAL INFO Wall B | BAL-INFO-WB | W-B | 60 min | DEMO-ASM-01 | Code, rating, assembly |
| BAL INFO Wall C | BAL-INFO-WC | W-C | blank | blank | Code only; non-rated control |
| BAL INFO Wall D | BAL-INFO-WD | W-D | blank | blank | Code only |
| BAL INFO Ceiling ACT | BAL-INFO-CACT | C-ACT | Not required | Not required | Code only |
| BAL INFO Ceiling GWB | BAL-INFO-CGWB | C-GWB | Not required | Not required | Code only |

Append `-01`, `-02`, and for walls `-03` to obtain the case Marks.

| Saved copy | Deliberate differences from complete data | Type-field findings | Distinct affected instances |
|---|---|---:|---:|
| BAL-info-complete.rvt | None | 0 | 0 |
| BAL-info-wall-defects.rvt | Wall B rating blank; Wall D code blank | 2 | 6 walls |
| BAL-info-ceiling-negative.rvt | The two wall gaps plus ACT code blank | 3 | 6 walls + 2 ceilings |

Wall C must pass even though its rating and assembly are blank. Each missing type-field produces one finding regardless of the number of instances. Multiple findings on a type must not count its instances twice.

The [independent comparison script](../../tests/verify_information_evidence.py) checks the exported Revit schedule cells against these literal values, then compares the separate readiness report. It does not import the evaluator or fixture code. It also checks exact case membership, complete coverage, and unchanged modified flags. Hash comparisons and UI inspection are additional host checks.
