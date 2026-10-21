"""Check retained Task 7 SDK traffic and restoration evidence, not desktop or BIM oracle acceptance."""
import argparse
import hashlib
import json
import re
from datetime import datetime
from pathlib import Path, PureWindowsPath
from verify_excel_evidence import workbook, table


TOOLS = {"revit_get_readiness_context", "revit_check_required_type_information"}


def read(path):
    return json.loads(Path(path).read_text(encoding="utf-8-sig"))


def check_capture(directory, expected):
    directory = Path(directory)
    assert read(directory / "complete.json")["pass"] is True, "Capture not successfully completed"
    incoming = [json.loads(s) for s in (directory / "wire/client-to-server.jsonl").read_text(encoding="utf-8").splitlines()]
    outgoing = [json.loads(s) for s in (directory / "wire/server-to-client.jsonl").read_text(encoding="utf-8").splitlines()]
    assert incoming and outgoing, "Empty traffic"
    responses = {}
    for row in outgoing:
        assert row["jsonrpc"] == "2.0"
        if "id" in row:
            assert row["id"] not in responses, "Duplicate response"
            responses[row["id"]] = row
    ids, methods, calls = set(), [], []
    for request in incoming:
        assert request["jsonrpc"] == "2.0"
        if "id" not in request:
            continue
        assert request["id"] not in ids, "Duplicate request"
        ids.add(request["id"])
        assert request["params"]["_meta"]["io.modelcontextprotocol/protocolVersion"] == "2026-07-28"
        response = responses[request["id"]]
        assert "error" not in response, "Protocol error instead of a tool result"
        result = response["result"]
        method = request["method"]
        methods.append(method)
        if method == "server/discover":
            assert "2026-07-28" in result["supportedVersions"]
        elif method == "tools/list":
            assert {tool["name"] for tool in result["tools"]} == TOOLS
        else:
            assert method == "tools/call" and request["params"]["name"] in TOOLS
            data = result["structuredContent"]
            assert re.fullmatch("[0-9a-f]{32}", data["requestId"])
            assert len(result["content"]) == 1 and result["content"][0]["type"] == "text"
            assert json.loads(result["content"][0]["text"]) == data, "Text/structured disagreement"
            saved = "context.json" if request["params"]["name"] == "revit_get_readiness_context" else "check.json"
            assert read(directory / saved)["structuredContent"] == data, "Saved result differs from wire"
            assert read(directory / saved)["isError"] is result["isError"]
            calls.append(result)
    assert set(responses) == ids, "Unsolicited response identity"
    assert methods[:2] == ["server/discover", "tools/list"] and calls
    result = calls[-1]
    data = result["structuredContent"]
    if expected == "Context":
        assert result["isError"] is False and data["contextToken"] and data["documentLifetimeId"]
    elif expected == "ReadinessReport":
        assert result["isError"] is False and isinstance(data["report"], dict)
    else:
        assert result["isError"] is True and data["code"] == expected and "report" not in data
    return data


def check_restoration(directory):
    directory = Path(directory)
    before = read(directory / "before-install.json")
    assert before == read(directory / "after-undo-first.json"), "First undo differs from original snapshot"
    assert before == read(directory / "after-undo-second.json"), "Second undo differs from original snapshot"
    first = read(directory / "start-first-descriptor.json")
    assert first == read(directory / "start-twice-descriptor.json")
    assert first["BridgeSessionId"] != read(directory / "restart-descriptor.json")["BridgeSessionId"]
    for name in ("before-explicit-start", "stop-first", "stop-second"):
        assert read(directory / (name + ".json"))["descriptorCount"] == 0
    old = read(directory / "after-modal-report/context.json")["structuredContent"]
    new = read(directory / "reopened-fresh-context/context.json")["structuredContent"]
    for field in ("documentPath", "projectUniqueId", "checklistSha256"):
        assert old[field] == new[field], field
    assert old["documentLifetimeId"] != new["documentLifetimeId"]


def utc(value):
    return datetime.fromisoformat(value.replace("Z", "+00:00"))


def check_save_as(directory, run, metadata):
    """Validate a coordinated Save As observation separately from the earlier expired attempt."""
    directory = Path(directory)
    capture = directory / run
    before = read(capture / 'before-save-as/context.json')['structuredContent']
    after = read(capture / 'after-save-as-fresh-report/context.json')['structuredContent']
    preparation = read(capture / 'preparation.json')
    timing = read(capture / 'before-stale-token-check.json')
    acquired = utc(read(capture / 'before-save-as/sdk-client-result.json')['completedUtc'])
    completed = utc(read(capture / 'after-save-as-old-token/sdk-client-result.json')['completedUtc'])
    expires = utc(before['expiresUtc'])
    started = utc(timing['checkedAtUtc'])
    assert utc(preparation['utc']) <= acquired <= started <= completed < expires, 'Save As test did not finish before expiry'
    assert timing['expiresUtc'] == before['expiresUtc'] and timing['userReportedSaved'] is True
    assert abs(timing['secondsRemaining'] - (expires - started).total_seconds()) < 0.0001

    requests = [json.loads(s) for s in (capture / 'after-save-as-old-token/wire/client-to-server.jsonl').read_text().splitlines()]
    calls = [r['params'] for r in requests if r['method'] == 'tools/call']
    assert len(calls) == 1 and calls[0]['name'] == 'revit_check_required_type_information'
    assert calls[0]['arguments'] == {'contextToken':before['contextToken']}, 'Must send the actual pre-save token without reacquisition'
    rejected = read(capture / 'after-save-as-old-token/check.json')
    assert rejected['isError'] is True and rejected['structuredContent']['code'] == 'ContextChanged'
    assert 'report' not in rejected['structuredContent']
    for key in ('revitPid', 'bridgeSessionId', 'projectUniqueId', 'revitBuild', 'checklistId', 'scope', 'level', 'phase'):
        assert before[key] == after[key], key
    assert before['revitPid'] == metadata['revitPid'] == preparation['revitPid']
    assert before['documentLifetimeId'] != after['documentLifetimeId']
    assert before['contextToken'] != after['contextToken']
    old_path, new_path = PureWindowsPath(before['documentPath']), PureWindowsPath(after['documentPath'])
    assert old_path != new_path
    assert old_path == PureWindowsPath(preparation['sourceModel'])
    assert new_path == PureWindowsPath(preparation['targetModel']) and preparation['modelAlreadyExists'] is False

    checklists = []
    for name, context, model in [('source', before, old_path), ('target', after, new_path)]:
        path = capture / (name + '-checklist.json')
        digest = hashlib.sha256(path.read_bytes()).hexdigest()
        assert digest == preparation[name + 'ChecklistSha256'] == context['checklistSha256']
        assert PureWindowsPath(context['checklistPath']) == model.with_suffix('.checklist.json')
        data = read(path)
        assert data['ProjectFileName'] == model.name
        checklists.append(data)
    assert {k:v for k,v in checklists[0].items() if k != 'ProjectFileName'} == {k:v for k,v in checklists[1].items() if k != 'ProjectFileName'}, 'Only checklist filename may change'

    report = read(capture / 'after-save-as-fresh-report/check.json')['structuredContent']['report']
    for report_key, context_key in [('DocumentPath','documentPath'), ('ChecklistPath','checklistPath'), ('ProjectUniqueId','projectUniqueId')]:
        assert report[report_key] == after[context_key]
    assert report['RequestedInstances'] == report['ExaminedInstances'] == 16
    assert report['CoverageComplete'] is True and report['Evaluation']['Complete'] is True
    assert report['ModifiedBefore'] is False and report['ModifiedAfter'] is False
    assert report['Evaluation']['FindingCount'] == 0 and not report['Evaluation']['AffectedElements']
    assert not report['ScopeIssues'] and not report['Evaluation']['UnexaminedElements']
    assert len(report['Evaluation']['Types']) == 6
    assert sum(len(t['Fields']) for t in report['Evaluation']['Types']) == 18
    # This is a continuity comparison with a retained earlier report, not the Task 8 independent BIM oracle.
    reference = read(directory / metadata['referenceReport'])['structuredContent']['report']
    changes = {'GeneratedUtc', 'DocumentPath', 'ChecklistPath'}
    assert {k:v for k,v in report.items() if k not in changes} == {k:v for k,v in reference.items() if k not in changes}


def check_followup(directory):
    """Check relationships between observations, without treating them as instrumentation."""
    directory = Path(directory)
    context = lambda case: read(directory / case / "context.json")["structuredContent"]
    report = lambda case: read(directory / case / "check.json")["structuredContent"]["report"]
    def sent_token(case):
        rows = [json.loads(s) for s in (directory / case / "wire/client-to-server.jsonl").read_text().splitlines()]
        calls = [r for r in rows if r.get("method") == "tools/call" and r["params"]["name"] == "revit_check_required_type_information"]
        assert len(calls) == 1
        return calls[0]["params"]["arguments"]["contextToken"]

    old, new = context("before-save-as-context-retry"), context("after-user-save-as-report")
    assert old["projectUniqueId"] == new["projectUniqueId"]
    assert old["documentPath"] != new["documentPath"] and old["documentLifetimeId"] != new["documentLifetimeId"]
    # Preserve the limitation: this token was already expired, so it cannot isolate Save As invalidation.
    attempted = read(directory / "after-user-save-as-expired-prior-token/sdk-client-result.json")
    assert utc(old["expiresUtc"]) < utc(attempted["completedUtc"])
    assert sent_token("after-user-save-as-expired-prior-token") == old["contextToken"]

    def wall_code(value):
        walls = [t for t in value["Evaluation"]["Types"] if t["TypeName"] == "BAL INFO Wall A"]
        assert len(walls) == 1
        return next(f["ObservedValue"] for f in walls[0]["Fields"] if f["Field"] == "Code")
    before, changed, restored = (report(n) for n in ("before-value-edit-report", "same-token-after-value-edit", "reopened-after-value-edit-report"))
    assert sent_token("same-token-after-value-edit") == context("before-value-edit-report")["contextToken"]
    assert [wall_code(r) for r in (before, changed, restored)] == ["W-A", "W-A-LIVE", "W-A"]
    assert [(r["ModifiedBefore"], r["ModifiedAfter"]) for r in (before, changed, restored)] == [(False, False), (True, True), (False, False)]
    assert wall_code(report("after-value-edit-undo-report")) == "W-A-LIVE", "Do not rewrite unsuccessful Undo as restoration"

    old = context("reopened-after-value-edit-report")
    assert sent_token("restart-stale-context") == old["contextToken"]
    assert utc(read(directory / "restart-stale-context/sdk-client-result.json")["completedUtc"]) < utc(old["expiresUtc"]), "Restart test must precede token expiry"
    assert read(directory / "stop-with-context.json")["descriptorCount"] == 0
    assert read(directory / "restart-with-context-descriptor.json")["BridgeSessionId"] != old["bridgeSessionId"]

    for case, release in read(directory / "capture-index.json")["connectedObservationCaptures"].items():
        ready = read(directory / case / "response-ready.json")
        observed = read(directory / case / "post-release-observation.json")
        ui = read(directory / release)
        assert ready["pass"] is True and observed["pass"] is True and ui["clientCompleteAtUiRelease"] is False
        assert observed["elapsedMilliseconds"] >= 3000 and observed["catalogCount"] == 2
        assert utc(ready["receivedUtc"]) < utc(ui["utc"]) <= utc(observed["releasedUtc"]) < utc(observed["observedUntilUtc"])
        requests = [json.loads(s) for s in (directory / case / "wire/client-to-server.jsonl").read_text().splitlines()]
        assert requests[-1]["method"] == "tools/list"
        assert sum(r["method"] == "tools/list" for r in requests) == 2

    stopped = read(directory / "stop-incomplete-connections-retry/result.json")
    control = read(directory / "incomplete-connections-no-stop-control/result.json")
    assert stopped["pass"] is True and stopped["descriptorRemoved"] is True and stopped["revitProcessAlive"] is True
    assert {r["mode"] for r in stopped["results"]} == {"no-bytes", "partial-header", "partial-body"}
    assert all(r["bytesReturned"] == 0 and 0 <= r["elapsedMilliseconds"] < 4500 for r in stopped["results"])
    assert 0 <= stopped["descriptorWaitMilliseconds"] < 2000
    assert control["pass"] is False and control["descriptorRemoved"] is False
    assert all(r["elapsedMilliseconds"] >= 4900 for r in control["results"])
    assert read(directory / "restart-after-partial-descriptor.json")["BridgeSessionId"] != read(directory / "stop-incomplete-connections-retry/ready.json")["descriptor"]["BridgeSessionId"]

    manual = read(directory / "manual-export/BAL-info-saved-as.readiness.json")
    assert {k:v for k,v in manual.items() if k != "GeneratedUtc"} == {k:v for k,v in restored.items() if k != "GeneratedUtc"}
    assert manual["CoverageComplete"] is True and manual["Evaluation"]["Complete"] is True
    assert manual["Evaluation"]["FindingCount"] == 0 and not manual["Evaluation"]["AffectedElements"]
    sheets = workbook(directory / "manual-export/BAL-info-saved-as.readiness.xlsx")
    assert list(sheets) == ["Summary", "Findings", "Affected elements", "Rules and coverage"]
    summary = {r.get("A"):r.get("B") for r in sheets["Summary"].values() if "A" in r and "B" in r}
    assert summary["Generated (UTC)"] == manual["GeneratedUtc"]
    assert summary["Model path"] == manual["DocumentPath"] and summary["Checklist"] == manual["ChecklistId"]
    assert summary["Missing required type values"] == summary["Distinct affected elements"] == 0
    assert summary["Requested instances"] == summary["Examined instances"] == 16
    assert summary["Coverage"] == "Complete for checklist scope"
    assert summary["Model modified before read"] is False and summary["Model modified after read"] is False
    assert summary["Other host wall/ceiling instances"] == 1196 and summary["Link instances excluded"] == 6
    assert not table(sheets["Findings"], 4) and not table(sheets["Affected elements"], 4)
    rules = table(sheets["Rules and coverage"], 14)
    assert len(rules) == 18
    rule_map = {(r["Type unique ID"], r["Field"]):r for r in rules}
    text = (directory / "manual-export/BAL-info-saved-as.readiness.txt").read_text()
    assert text.count("instances; Pass") == 6
    for t in manual["Evaluation"]["Types"]:
        assert t["TypeName"] in text
        assert all(e["CaseId"] in text for e in t["Elements"])
        for f in t["Fields"]:
            row = rule_map[(t["TypeUniqueId"], f["Field"])]
            for col, key in [("Status", "Status"), ("Reason", "Reason"), ("Observed value", "ObservedValue"), ("Rule ID", "RuleId"), ("Requirement source", "RequirementSource"), ("Parameter ID", "ParameterId")]:
                assert row[col] == (f[key] or "")
            assert row["Required"] == ("Yes" if f["Required"] else "No")

    shutdown = read(directory / "normal-application-shutdown.json")
    assert shutdown["revitProcessAlive"] is False and shutdown["descriptorCount"] == 0
    hashes = read(directory / "final-closed-model-hashes.json")
    recorded = {r["path"]:r["sha256"].lower() for r in hashes["verified"]}
    assert hashes["pass"] is True and len(recorded) == 14
    for row in read(directory / "model-inputs-before.json"):
        for field in ("source", "copy"):
            assert recorded[row[field]] == row["sha256"].lower()
    saved = read(directory / "saved-as-closed-hash.json")
    assert recorded[saved["path"]] == saved["sha256"].lower()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("directory", type=Path)
    args = parser.parse_args()
    manifest = read(args.directory / "capture-index.json")
    for name, expected in manifest["successfulCaptures"].items():
        check_capture(args.directory / name, expected)
    check_restoration(args.directory)
    if manifest.get("connectedObservationCaptures"):
        check_followup(args.directory)
    for run, metadata in manifest.get("freshSaveAsRuns", {}).items():
        check_save_as(args.directory, run, metadata)
    print(f"PASS {len(manifest['successfulCaptures'])} SDK lifecycle captures, restoration snapshots and session/document transitions; not full host/A8 acceptance")


if __name__ == "__main__":
    main()
