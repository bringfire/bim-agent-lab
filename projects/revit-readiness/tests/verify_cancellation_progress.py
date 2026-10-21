"""Check retained actual-endpoint progress evidence, independently of the C# harness."""
from __future__ import annotations
import argparse
import json
import re
from pathlib import Path


def verify(text: str) -> dict:
    modes = {f"{origin}-{outcome}-{when}" for origin in ("stop", "disconnect", "deadline")
             for outcome in ("return", "throw") for when in ("before", "after")}
    modes |= {"capacity", "client", "quiesced-fault"}
    assert "PIPE_SUITE_COMPLETE" in text and not re.search(r"^FAIL ", text, re.M)
    targets = {}
    target = None
    current = None
    block = []
    for line in text.splitlines():
        if line.startswith("PIPE_TARGET framework="):
            assert current is None
            target = line.split("=", 1)[1]
            assert target in {"net48", "net10.0"} and target not in targets
            targets[target] = set()
        elif line.startswith("PROGRESS_CHILD "):
            match = re.fullmatch(r"PROGRESS_CHILD mode=(\S+) exit=0", line)
            assert match and target and current is None, "Child failed or incomplete"
            current = match[1]
            assert current in modes and current not in targets[target]
            block = []
        elif line.startswith("PASS Cancellation progress child: "):
            assert current and line == "PASS Cancellation progress child: " + current
            assert block.count("PROGRESS_PROOF mode=" + current) == 1
            if current == "capacity":
                assert "WHILE_HELD openConnections=8" in block
                assert "WHILE_HELD capacity=8 overload=Busy restartBypass=false submissions=8" in block
            elif current == "client":
                assert "WHILE_HELD clientNetworkReleased=true immediateInvalidation=true" in block
            elif current == "quiesced-fault":
                assert "EXCEPTIONAL_QUIESCENCE disposed=9 capacityRecovered=true errorRetained=true" in block
            else:
                origin, _, when = current.split("-")
                expected = "True" if when == "before" else "False"
                assert f"WHILE_HELD mode={current} stopReturned=true networkReleased=true descriptorRemoved=true quiesced={expected}" in block
                if origin != "stop": assert "WHILE_HELD openConnections=8" in block
            targets[target].add(current)
            current = None
        elif current is not None:
            block.append(line)
    assert current is None and set(targets) == {"net48", "net10.0"}
    assert all(found == modes for found in targets.values()), "Missing matrix cases"
    return {"pass": True, "casesPerRuntime": 15, "targets": sorted(targets),
            "scope": "Held-callback network progress, bounded retention and exceptional quiescence; no Revit/A8"}


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("log", type=Path)
    args = parser.parse_args()
    print(json.dumps(verify(args.log.read_text(encoding="utf-8-sig")), indent=2))
