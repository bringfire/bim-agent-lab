"""Check retained SDK cancellation characterization against actual wire prefixes.

This verifies a known limitation, not successful token cancellation or Revit acceptance.
Run against the --characterize capture; --require-propagation intentionally fails in C#.
"""
import json
from datetime import datetime
from pathlib import Path
import sys


def read_json(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def read_wire(path):
    return [json.loads(line) for line in path.read_text(encoding="utf-8-sig").splitlines() if line]


def verify(root):
    root = Path(root)
    for revision in ("2025-06-18", "2025-11-25", "2026-07-28"):
        for mode in ("token", "explicit"):
            folder = root / f"{revision}-{mode}"
            obs = read_json(folder / "observation.json")
            before = read_wire(folder / "wire-before-rescue.jsonl")
            incoming = read_wire(folder / "wire/client-to-server.jsonl")
            outgoing = read_wire(folder / "wire/server-to-client.jsonl")
            cleanup = read_json(folder / "cleanup.json")
            assert incoming[:len(before)] == before, "Observation is not an actual wire prefix"
            assert obs["protocol"] == revision and obs["mode"] == mode
            handshake = [m for m in outgoing if "result" in m and
                         ("protocolVersion" in m["result"] or "supportedVersions" in m["result"])]
            assert len(handshake) == 1
            h = handshake[0]["result"]
            assert h.get("protocolVersion") == revision or revision in h.get("supportedVersions", [])
            checks = [m for m in before if m.get("method") == "tools/call" and
                      m["params"]["name"] == "revit_check_required_type_information"]
            assert len(checks) == 1 and str(checks[0]["id"]) == obs["rpcRequestId"]
            request_id = checks[0]["id"]
            prefix_notifications = [m for m in before if m.get("method") == "notifications/cancelled"]
            notifications = [m for m in incoming if m.get("method") == "notifications/cancelled"]
            expected = mode == "explicit"
            assert len(prefix_notifications) == int(expected) == obs["notificationsBeforeRescue"]
            assert len(notifications) == obs["finalNotifications"] == 1
            assert notifications[0]["params"]["requestId"] == request_id, "Wrong cancellation identity"
            assert obs["localCanceled"] is True
            assert obs["remoteCanceledBeforeRescue"] is expected
            assert obs["guardRejectedBeforeRescue"] is expected
            for name in ("connected", "held", "rescued", "rescueGuardRejected"):
                assert obs[name] is True, name
            assert obs["remainingMilliseconds"] > 15000
            assert obs["originalDeadline"] == obs["finalDeadline"]
            stimulus = datetime.fromisoformat(obs["stimulusUtc"])
            observed = datetime.fromisoformat(obs["observedUtc"])
            deadline = datetime.fromisoformat(obs["originalDeadline"])
            assert (observed - stimulus).total_seconds() >= .7, "Missing observation window"
            assert (deadline - observed).total_seconds() > 15, "Expiry confounds cancellation"
            # The request must not acquire a success result after explicit cancellation.
            assert not any(m.get("id") == request_id and "result" in m for m in outgoing)
            catalog_calls = [m for m in incoming if m.get("method") == "tools/list"]
            assert len(catalog_calls) == 1 and incoming.index(catalog_calls[0]) > incoming.index(notifications[0])
            catalog = next(m["result"]["tools"] for m in outgoing if m.get("id") == catalog_calls[0]["id"])
            assert {t["name"] for t in catalog} == {"revit_get_readiness_context", "revit_check_required_type_information"}
            assert obs["followupTools"] == 2 and cleanup == {"forced": False, "ExitCode": 0}
    mechanisms = read_json(root / "mechanism.json")
    assert len(mechanisms) == 2
    failing, control = mechanisms
    assert failing["cancelWaitSeparately"] is True
    assert failing["events"] == ["cancel-enter", "wait-unwound", "registration-disposed", "cancel-return"]
    assert control["cancelWaitSeparately"] is False
    events = control["events"]
    assert events[0] == "cancel-enter" and events[1] == "send-callback"
    assert sorted(events) == sorted(["cancel-enter", "send-callback", "wait-unwound", "registration-disposed", "cancel-return"])
    assert events.index("wait-unwound") < events.index("registration-disposed")
    return 6


if __name__ == "__main__":
    count = verify(sys.argv[1])
    print(f"PASS {count} SDK characterization exchanges and mechanism control; known token failure remains")
