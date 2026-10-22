"""Capture a bounded diagnostic hold through the official SDK client, on Windows.

The human opens the disposable model and starts its bridge first. This observer
writes diagnostic controls only; it never edits a model or implements MCP.
Acceptance is checked separately from these orchestration assertions.
"""
import argparse
import datetime as dt
import json
from pathlib import Path
import subprocess
import time


def utc():
    return dt.datetime.now(dt.timezone.utc).isoformat()


def run(args):
    root = Path(args.trace_directory).resolve()
    evidence = Path(args.evidence).resolve()
    evidence.mkdir(parents=True, exist_ok=False)
    trace = root / "events.tsv"
    initial = trace.read_bytes()
    assert b"ApplicationStartup" in initial and b"EventsHooked" in initial
    assert not list(root.glob("arm-*.txt"))
    assert not (root / "diagnostic-error.txt").exists()
    (evidence / "before.tsv").write_bytes(initial)
    arm = root / f"arm-{args.stage}.txt"
    finish = evidence / "finish.signal"
    interrupt = evidence / "interrupt.signal"
    capture = evidence / "sdk"
    command = [args.client, "--server", args.server, "--scenario", "readiness",
               "--revit-pid", str(args.revit_pid), "--expected", "DeadlineExceeded",
               "--evidence", str(capture), "--finish-file", str(finish)]
    if args.origin == "stop":
        command[command.index("DeadlineExceeded")] = "BridgeUnavailable"
    if args.origin in ("cancel", "eof"):
        command = [args.client, "--server", args.server, "--scenario", "interrupt",
                   "--revit-pid", str(args.revit_pid), "--interrupt-mode", args.origin,
                   "--interrupt-file", str(interrupt), "--evidence", str(capture), "--finish-file", str(finish)]
    record = {"stage": args.stage, "revitPid": args.revit_pid,
              "origin": args.origin,
              "traceDirectory": str(root), "startedUtc": utc(), "command": command,
              "scope": "Diagnostic Revit/SDK capture; not desktop A8 or physical-disposal proof"}
    child = None
    request_id = None
    armed = False
    try:
        # Exclusive creation prevents overwriting another operator's control.
        with arm.open("x", encoding="utf-8", newline="\n") as stream:
            armed = True
            stream.write("45000\n" + ("stop" if args.origin == "stop" else "hold") + "\n")
        with (evidence / "client-stdout.txt").open("wb") as stdout, (evidence / "client-stderr.txt").open("wb") as stderr:
            child = subprocess.Popen(command, stdout=stdout, stderr=stderr,
                                     creationflags=subprocess.CREATE_NO_WINDOW)
            start = time.monotonic()
            while time.monotonic() - start < 42:
                rows = [line.split("\t") for line in trace.read_text(encoding="utf-8").splitlines()]
                new_rows = rows[len(initial.splitlines()):]
                gates = [r for r in new_rows if r[4] == "GateHeld" and r[8] == args.stage]
                if gates:
                    assert len(gates) == 1
                    request_id = gates[0][5]
                    record["requestId"] = request_id
                    if args.origin in ("cancel", "eof") and not interrupt.exists():
                        record["interruptWrittenUtc"] = utc()
                        interrupt.write_text(record["interruptWrittenUtc"], encoding="utf-8")
                    deadline = root / f"held-{request_id}-{args.stage}.txt"
                    if deadline.exists():
                        record["originalDeadlineUtc"] = deadline.read_text(encoding="utf-8-sig")
                        (evidence / "held-deadline.txt").write_bytes(deadline.read_bytes())
                        (evidence / "consumed-arm.txt").write_bytes((root / f"used-{request_id}-{args.stage}.txt").read_bytes())
                    inactive = [r for r in new_rows if r[4] == "InactiveObserved" and r[5] == request_id]
                    observed_file = "response-ready.json" if args.origin in ("deadline", "stop") else "interrupted.json"
                    stopped = args.origin != "stop" or any(r[4] == "NetworkStopped" for r in new_rows)
                    if inactive and stopped and (capture / observed_file).exists():
                        record["inactiveObservedBeforeRelease"] = True
                        record["responseReceivedBeforeRelease" if args.origin in ("deadline", "stop") else "interruptionCompletedBeforeRelease"] = True
                        (evidence / "held.tsv").write_bytes(trace.read_bytes())
                        break
                assert child.poll() is None, "SDK client exited before held response"
                time.sleep(0.02)
            else:
                raise AssertionError("No held, inactive response within observer limit")
            release = root / f"release-{request_id}-{args.stage}.signal"
            record["releaseWrittenUtc"] = utc()
            with release.open("x", encoding="utf-8") as stream:
                stream.write(record["releaseWrittenUtc"])
            until = time.monotonic() + 8
            while time.monotonic() < until:
                current = trace.read_text(encoding="utf-8")
                rows = [line.split("\t") for line in current.splitlines()]
                stages = {r[4] for r in rows if r[5] == request_id}
                if {"GateReleased", "ExecutorExited", "QuiescedObserved"} <= stages:
                    break
                time.sleep(0.02)
            else:
                raise AssertionError("Executor did not quiesce after explicit release")
            record["finishWrittenUtc"] = utc()
            finish.write_text(record["finishWrittenUtc"], encoding="utf-8")
            child.wait(timeout=15)
            assert child.returncode == 0, "SDK capture failed"
            record["captureCompleted"] = True
    finally:
        # Always release our claimed hold on observer failure, leaving evidence intact.
        if request_id:
            release = root / f"release-{request_id}-{args.stage}.signal"
            if not release.exists():
                release.write_text("observer cleanup " + utc(), encoding="utf-8")
                record["cleanupRelease"] = True
        elif armed and arm.exists():
            arm.unlink()  # only the exclusive control created by this invocation
        if child is not None and child.poll() is None:
            finish.touch(exist_ok=True)
        record["capturedUntilUtc"] = utc()
        record["clientExitCode"] = child.poll() if child is not None else None
        (evidence / "after.tsv").write_bytes(trace.read_bytes())
        (evidence / "observation.json").write_text(json.dumps(record, indent=2) + "\n", encoding="utf-8")
    print(f"CAPTURE COMPLETE {args.stage}; independent acceptance comparison still required")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--stage", choices=["Preparation", "Reader"], required=True)
    parser.add_argument("--origin", choices=["deadline", "cancel", "eof", "stop"], default="deadline")
    parser.add_argument("--revit-pid", type=int, required=True)
    for name in ["trace-directory", "evidence", "client", "server"]:
        parser.add_argument("--" + name, required=True)
    run(parser.parse_args())
