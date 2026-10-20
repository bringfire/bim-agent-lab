"""Compare native Revit schedule exports and plugin reports with a hand-authored oracle.

Uses only Python's standard library. This is a verification aid, not an add-in dependency.
"""
import argparse
import csv
import io
import json
from pathlib import Path


def require(condition, message):
    if not condition:
        raise AssertionError(message)


def schedule_rows(path):
    raw = path.read_bytes()
    text = raw.decode("utf-16" if raw.startswith((b"\xff\xfe", b"\xfe\xff")) else "utf-8-sig")
    rows = list(csv.reader(io.StringIO(text), delimiter="\t"))
    header_index = next(i for i, row in enumerate(rows) if "CaseId" in row)
    headers = rows[header_index]
    return [dict(zip(headers, row)) for row in rows[header_index + 1:] if any(cell.strip() for cell in row)]


def verify(folder):
    # Literal requirements and expected values from the independent answer sheet.
    definitions = [
        ("Walls", "BAL INFO Wall A", "BAL-INFO-WA", 3, "W-A", "60 min", "DEMO-ASM-01"),
        ("Walls", "BAL INFO Wall B", "BAL-INFO-WB", 3, "W-B", "60 min", "DEMO-ASM-01"),
        ("Walls", "BAL INFO Wall C", "BAL-INFO-WC", 3, "W-C", "", ""),
        ("Walls", "BAL INFO Wall D", "BAL-INFO-WD", 3, "W-D", "", ""),
        ("Ceilings", "BAL INFO Ceiling ACT", "BAL-INFO-CACT", 2, "C-ACT", None, None),
        ("Ceilings", "BAL INFO Ceiling GWB", "BAL-INFO-CGWB", 2, "C-GWB", None, None),
    ]
    scenarios = [
        ("BAL-info-complete", {}, 0, 0),
        ("BAL-info-wall-defects", {("BAL INFO Wall B", "FireRating"): "", ("BAL INFO Wall D", "Code"): ""}, 2, 6),
        ("BAL-info-ceiling-negative", {("BAL INFO Wall B", "FireRating"): "", ("BAL INFO Wall D", "Code"): "", ("BAL INFO Ceiling ACT", "Code"): ""}, 3, 8),
    ]
    results = []
    for name, changes, finding_count, affected_count in scenarios:
        rows = schedule_rows(folder / (name + "-walls.tsv")) + schedule_rows(folder / (name + "-ceilings.tsv"))
        require(len(rows) == 16, name + ": schedule must contain exactly 16 cases")
        by_case = {row["CaseId"]: row for row in rows}
        require(len(by_case) == 16, name + ": duplicate schedule case")
        report = json.loads((folder / (name + ".readiness.json")).read_text(encoding="utf-8-sig"))
        evaluation = report["Evaluation"]
        require(report["RequestedInstances"] == 16 and report["ExaminedInstances"] == 16, name + ": scope count mismatch")
        require(report["CoverageComplete"] and evaluation["Complete"] and not report["ScopeIssues"], name + ": incomplete coverage")
        require(not report["ModifiedBefore"] and not report["ModifiedAfter"], name + ": document was modified")
        require(len(evaluation["Types"]) == 6, name + ": expected six used types")
        require(evaluation["FindingCount"] == finding_count, name + ": wrong finding count")
        require(len(evaluation["AffectedElements"]) == affected_count, name + ": wrong affected count")
        expected_cases = set()
        expected_affected = set()
        observed_findings = set()
        type_rows = {row["TypeName"]: row for row in evaluation["Types"]}
        for category, type_name, prefix, count, code, rating, assembly in definitions:
            case_ids = {f"{prefix}-{i:02d}" for i in range(1, count + 1)}
            expected_cases |= case_ids
            observed_type = type_rows[type_name]
            require(observed_type["Category"] == category, name + ": wrong category")
            require({e["CaseId"] for e in observed_type["Elements"]} == case_ids, name + ": wrong affected type membership")
            fields = {f["Field"]: f for f in observed_type["Fields"]}
            for field_name, baseline in [("Code", code), ("FireRating", rating), ("AssemblyReference", assembly)]:
                expected_value = changes.get((type_name, field_name), baseline)
                required = field_name == "Code" or type_name in ("BAL INFO Wall A", "BAL INFO Wall B")
                expected_status = ("Missing" if expected_value == "" else "Present") if required else "NotRequired"
                require(fields[field_name]["Status"] == expected_status, name + ": wrong field status " + type_name + "/" + field_name)
                if expected_status == "Missing":
                    observed_findings.add((type_name, field_name))
                    expected_affected |= case_ids
                    require(fields[field_name]["Reason"] == "BlankValue", name + ": expected a blank value, not an absent parameter")
                if baseline is not None:
                    require((fields[field_name]["ObservedValue"] or "").strip() == expected_value, name + ": report value differs from oracle")
                    for case_id in case_ids:
                        require(by_case[case_id]["Type"] == type_name, name + ": schedule type differs from oracle")
                        require(by_case[case_id][field_name].strip() == expected_value, name + ": native schedule value differs from oracle: " + case_id + "/" + field_name)
        require(set(by_case) == expected_cases, name + ": unexpected schedule membership")
        require(observed_findings == set(changes), name + ": wrong finding identities")
        require({e["CaseId"] for e in evaluation["AffectedElements"]} == expected_affected, name + ": wrong affected element identities")
        results.append({"scenario": name, "schedule_cases": 16, "findings": finding_count, "affected_instances": affected_count, "result": "PASS"})
    return results


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("folder", type=Path, help="Folder containing the three native schedule exports and readiness reports")
    args = parser.parse_args()
    print(json.dumps(verify(args.folder), indent=2))
