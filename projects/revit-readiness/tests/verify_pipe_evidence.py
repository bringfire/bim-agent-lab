"""Check complete Windows pipe test evidence and recompute every frame interval."""
from __future__ import annotations

import argparse
import json
import re
from pathlib import Path


def verify(text: str) -> dict:
    assert "PIPE_SUITE_COMPLETE" in text, "Incomplete pipe evidence"
    assert not re.search(r"^FAIL ", text, re.M), "Failed behavioral case"
    current = None
    targets: dict[str, dict] = {}
    pattern = re.compile(r"FRAME_TIMING mode=(\S+) connection=(\d+) accepted=(\d+) closed=(\d+) frequency=(\d+) seconds=([0-9.]+)")
    for line in text.splitlines():
        if line.startswith("PIPE_TARGET framework="):
            current = line.split("=", 1)[1]
            assert current in {"net48", "net10.0"} and current not in targets
            targets[current] = {"frames": {}, "successors": 0, "summary": False}
        elif line.startswith("FRAME_TIMING"):
            assert current is not None
            match = pattern.fullmatch(line)
            assert match is not None
            mode, connection, accepted, closed, frequency, reported = match.groups()
            assert int(frequency) > 0 and int(closed) >= int(accepted)
            elapsed = (int(closed) - int(accepted)) / int(frequency)
            assert 4.98 <= elapsed <= 6.0, (current, mode, elapsed)
            assert abs(elapsed - float(reported)) < 0.000002, "Printed duration does not match ticks"
            frames = targets[current]["frames"].setdefault(mode, {})
            assert connection not in frames, "Duplicate connection timing"
            frames[connection] = elapsed
        elif line.startswith("PASS All eight "):
            assert current is not None
            targets[current]["successors"] += 1
        elif line.startswith("Pipe tests:"):
            assert current is not None and not targets[current]["summary"]
            assert re.fullmatch(r"Pipe tests: 27 passed; 0 failed\.", line), line
            targets[current]["summary"] = True
    assert set(targets) == {"net48", "net10.0"}
    for target, data in targets.items():
        assert data["summary"] and data["successors"] == 4
        assert set(data["frames"]) == {"no-bytes", "partial-header", "partial-body", "trickle"}
        for frames in data["frames"].values():
            assert set(frames) == {str(n) for n in range(1, 9)}, (target, frames)
    return {"pass": True, "targets": {key: {"cases": 27, "stalledConnections": 32,
        "maximumCleanupSeconds": max(value for group in data["frames"].values() for value in group.values())}
        for key, data in targets.items()}, "scope": "Windows IPC; no Revit or desktop A8 acceptance"}


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("log", type=Path)
    args = parser.parse_args()
    print(json.dumps(verify(args.log.read_text(encoding="utf-8-sig")), indent=2))
