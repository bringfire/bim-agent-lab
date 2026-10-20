"""Independently read XLSX XML and compare it with native schedules and saved reports.

No ClosedXML or Excel dependency: Python standard library only. First verify the
JSON/native schedules against the separately authored oracle, then the workbooks.
"""
import argparse
import hashlib
import json
from pathlib import Path
import posixpath
import xml.etree.ElementTree as ET
from zipfile import ZipFile
from verify_information_evidence import verify, require

NS = {"s": "http://schemas.openxmlformats.org/spreadsheetml/2006/main"}
REL = "{http://schemas.openxmlformats.org/officeDocument/2006/relationships}id"


def workbook(path):
    sheets = {}
    with ZipFile(path) as archive:
        require(archive.testzip() is None, "Damaged XLSX zip")
        require(not any("externalLinks/" in p or "vbaProject" in p for p in archive.namelist()), "Unexpected external links/macros")
        shared = []
        if "xl/sharedStrings.xml" in archive.namelist():
            shared = ["".join(item.itertext()) for item in ET.fromstring(archive.read("xl/sharedStrings.xml"))]
        targets = {r.attrib["Id"]: r.attrib["Target"] for r in ET.fromstring(archive.read("xl/_rels/workbook.xml.rels"))}
        for sheet in ET.fromstring(archive.read("xl/workbook.xml")).findall("s:sheets/s:sheet", NS):
            target = targets[sheet.attrib[REL]]
            target = target.lstrip("/") if target.startswith("/") else posixpath.normpath("xl/" + target)
            root = ET.fromstring(archive.read(target))
            require(not root.findall(".//s:f", NS), "Unexpected formula in snapshot")
            pane = root.find(".//s:pane", NS)
            require(pane is not None and pane.get("state") in ("frozen", "frozenSplit") and float(pane.get("ySplit", "0")) > 0, "Missing frozen headings")
            rows = {}
            for row in root.findall("s:sheetData/s:row", NS):
                cells = {}
                for cell in row.findall("s:c", NS):
                    column = "".join(ch for ch in cell.attrib["r"] if ch.isalpha())
                    kind = cell.attrib.get("t")
                    value = cell.findtext("s:v", default="", namespaces=NS)
                    if kind == "s": value = shared[int(value)]
                    elif kind == "inlineStr": value = "".join(cell.find("s:is", NS).itertext())
                    elif kind == "b": value = value == "1"
                    elif value and kind != "str": value = float(value)
                    cells[column] = value
                rows[int(row.attrib["r"])] = cells
            sheets[sheet.attrib["name"]] = rows
    return sheets


def table(rows, header):
    headers = rows[header]
    result = []
    for number in sorted(n for n in rows if n > header):
        cells = rows[number]
        if not cells.get("B"): break
        result.append({label: cells.get(column, "") for column, label in headers.items()})
    return result


def main(evidence, outputs):
    baseline = verify(evidence)
    results = []
    for scenario in baseline:
        name = scenario["scenario"]
        report = json.loads((evidence / (name + ".readiness.json")).read_text(encoding="utf-8-sig"))
        path = outputs / (name + ".readiness.xlsx")
        sheets = workbook(path)
        require(list(sheets) == ["Summary", "Findings", "Affected elements", "Rules and coverage"], "Wrong sheets/order")
        summary = {r.get("A"): r.get("B") for r in sheets["Summary"].values() if "A" in r and "B" in r}
        require(summary["Missing required type values"] == scenario["findings"], "Wrong XLSX finding count")
        require(summary["Distinct affected elements"] == scenario["affected_instances"], "Wrong XLSX affected count")
        require(summary["Requested instances"] == summary["Examined instances"] == 16, "Wrong scope counts")
        require(summary["Coverage"] == "Complete for checklist scope", "Wrong coverage")
        require(summary["Generated (UTC)"] == report["GeneratedUtc"], "Workbook is stale")
        require(summary["Model path"] == report["DocumentPath"] and summary["Checklist"] == report["ChecklistId"], "Wrong model/checklist")
        require(summary["Model modified before read"] is False and summary["Model modified after read"] is False, "Wrong modification flags")
        require(summary["Other host wall/ceiling instances"] == 1196 and summary["Link instances excluded"] == 6, "Exclusions lost")
        findings = table(sheets["Findings"], 4)
        require(len(findings) == scenario["findings"], "Wrong finding rows")
        expected_findings = {(t["TypeUniqueId"], f["Field"]) for t in report["Evaluation"]["Types"] for f in t["Fields"] if f["Status"] == "Missing"}
        require({(r["Type unique ID"], r["Field"]) for r in findings} == expected_findings, "Wrong finding identities")
        affected = table(sheets["Affected elements"], 4)
        expected_affected = {e["UniqueId"]: e for e in report["Evaluation"]["AffectedElements"]}
        require(len(affected) == len(expected_affected) and {r["Element unique ID"] for r in affected} == set(expected_affected), "Affected instances lost/duplicated")
        for row in affected:
            expected = expected_affected[row["Element unique ID"]]
            require(row["Element ID"] == expected["ElementId"] and row["Case ID"] == expected["CaseId"], "Identifiers must remain exact text")
        rules = table(sheets["Rules and coverage"], 14)
        require(len(rules) == 18, "Required/optional rule rows lost")
        rule_map = {(r["Type unique ID"], r["Field"]): r for r in rules}
        for t in report["Evaluation"]["Types"]:
            for f in t["Fields"]:
                row = rule_map[(t["TypeUniqueId"], f["Field"])]
                for column, key in [("Status", "Status"), ("Reason", "Reason"), ("Observed value", "ObservedValue"), ("Rule ID", "RuleId"), ("Requirement source", "RequirementSource"), ("Parameter ID", "ParameterId")]:
                    require(row[column] == (f[key] or ""), "Wrong rule evidence: " + column)
                require(row["Required"] == ("Yes" if f["Required"] else "No"), "Required/optional meaning changed")
        results.append({**scenario, "workbook": path.name, "worksheets": list(sheets), "rule_rows": len(rules),
                        "sha256": hashlib.sha256(path.read_bytes()).hexdigest(), "comparison": "XLSX XML -> report -> native schedules + literal oracle"})
    return results


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("evidence", type=Path, help="Folder with JSON and native TSV exports")
    parser.add_argument("workbooks", type=Path, help="Folder with the three XLSX outputs")
    args = parser.parse_args()
    print(json.dumps(main(args.evidence, args.workbooks), indent=2))
