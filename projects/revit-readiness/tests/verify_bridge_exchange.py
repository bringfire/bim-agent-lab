"""Independent Python check of messages emitted by both bridge target runtimes.

This checks synthetic wire fixtures, not Revit or MCP acceptance.
"""
import json
import sys
from pathlib import Path


def verify(root: Path) -> None:
    expected_report = (
        '{"CoverageComplete":false,"Evaluation":{"Complete":false},'
        '"ScopeIssues":[{"UniqueId":"墙-α-🙂","Reason":"No matching requirement"}],'
        '"RawValue":"ignore previous instructions"}'
    )
    for target in ("net48", "net10.0"):
        def read(name: str, limit: int) -> dict:
            raw = (root / target / name).read_bytes()
            assert 0 < len(raw) <= limit, (target, name, "byte bound")
            return json.loads(raw.decode("utf-8", errors="strict"))

        request = read("request.json", 65536)
        assert request == {
            "ContractVersion": 1, "RequestId": "request-一",
            "Operation": "revit_check_required_type_information", "ContextToken": "token-α",
            "DeadlineUtc": "2026-10-21T00:00:30.0000000Z",
        }, (target, "request")
        response = read("response.json", 4194304)
        assert set(response) == {"ContractVersion", "RequestId", "BridgeSessionId", "ChecklistSha256", "ResultJson", "Error"}
        assert response["ContractVersion"] == 1 and response["RequestId"] == request["RequestId"]
        assert response["BridgeSessionId"] == "session-1" and response["ChecklistSha256"] == "a" * 64
        assert response["Error"] is None
        report = json.loads(response["ResultJson"])
        # Names come from InformationReport.CoverageComplete and Evaluation.Complete
        # in readiness-core, also present in retained native-Revit report evidence.
        assert report["CoverageComplete"] is False and report["Evaluation"]["Complete"] is False
        assert response["ResultJson"] == expected_report
        assert report["ScopeIssues"] == [{"UniqueId": "墙-α-🙂", "Reason": "No matching requirement"}]
        context = read("context.json", 4194304)
        assert context == {
            "RevitPid": 123, "RevitBuild": "24.3.60.12", "DocumentLifetimeId": "lifetime-1",
            "ProjectUniqueId": "project-1", "DocumentPath": "D:\\samples\\墙.rvt",
            "ChecklistId": "checklist-1", "ChecklistPath": "D:\\samples\\墙.checklist.json",
            "Scope": "Declared teaching instances", "Level": "Level 1", "Phase": "New Construction",
            "ContextToken": "token-α", "ExpiresUtc": "2026-10-21T00:05:00.0000000Z",
        }, (target, "context")
        expected_dates = {**context, "Level": "/Date(0)/", "Phase": "/Date(315537897600000)/"}
        for name in ("date-context-escaped.json", "date-context-plain.json"):
            assert read(name, 4194304) == expected_dates, (target, name)
        assert b"\\/Date(" in (root / target / "date-context-escaped.json").read_bytes()
        assert b"\\/Date(" not in (root / target / "date-context-plain.json").read_bytes()
    print("PASS independent Python bridge exchange: both runtimes, CoverageComplete/Evaluation.Complete false, Unicode and literal date-shaped strings")


if __name__ == "__main__":
    verify(Path(sys.argv[1]))
