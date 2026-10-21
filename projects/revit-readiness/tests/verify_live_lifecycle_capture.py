"""Check retained Task 7 SDK traffic and restoration evidence, not desktop or BIM oracle acceptance."""
import argparse
import json
import re
from pathlib import Path


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


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("directory", type=Path)
    args = parser.parse_args()
    manifest = read(args.directory / "capture-index.json")
    for name, expected in manifest["successfulCaptures"].items():
        check_capture(args.directory / name, expected)
    check_restoration(args.directory)
    print(f"PASS {len(manifest['successfulCaptures'])} SDK lifecycle captures, restoration snapshots and session/document transitions; not full host/A8 acceptance")


if __name__ == "__main__":
    main()
