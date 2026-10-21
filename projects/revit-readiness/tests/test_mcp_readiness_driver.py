"""Exercise the real SDK driver without Revit, using an explicitly absent PID."""
import json
import subprocess
import tempfile
import time
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CLIENT = ROOT / "tests/mcp-client/bin/Release/net10.0/BimAgentLab.Mcp.Client.exe"
SERVER = ROOT / "src/mcp-server/bin/Release/net10.0/BimAgentLab.RevitReadiness.Mcp.exe"


class ReadinessDriverTests(unittest.TestCase):
    def test_keeps_sdk_connection_until_explicit_release(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            evidence, release = root / "capture", root / "finish"
            with subprocess.Popen([str(CLIENT), "--server", str(SERVER),
                    "--scenario", "readiness", "--revit-pid", "2147483647",
                    "--expected", "BridgeUnavailable", "--evidence", str(evidence),
                    "--finish-file", str(release)], stdout=subprocess.PIPE,
                    stderr=subprocess.PIPE, text=True) as process:
                try:
                    deadline = time.monotonic() + 15
                    while not (evidence / "response-ready.json").exists() and process.poll() is None and time.monotonic() < deadline:
                        time.sleep(.02)
                    self.assertTrue((evidence / "response-ready.json").exists(), "Driver must expose response-ready while retaining the SDK connection")
                    time.sleep(.25)
                    self.assertIsNone(process.poll())
                    self.assertFalse((evidence / "complete.json").exists())
                    release.write_text("UI transition completed\n")
                    stdout, stderr = process.communicate(timeout=15)
                    self.assertEqual(process.returncode, 0, stdout + stderr)
                    self.assertTrue(json.loads((evidence / "complete.json").read_text())["pass"])
                    observation = json.loads((evidence / "post-release-observation.json").read_text())
                    self.assertGreaterEqual(observation["elapsedMilliseconds"], 3000)
                    self.assertEqual(observation["catalogCount"], 2)
                    wires = list((evidence / "wire").rglob("client-to-server.jsonl"))
                    self.assertEqual(len(wires), 1)
                    calls = [json.loads(line)["method"] for line in wires[0].read_text().splitlines()]
                    self.assertEqual(calls.count("tools/list"), 2)
                finally:
                    if process.poll() is None:
                        # Only this disposable test driver may be terminated on failed assertions.
                        process.kill()
                        process.communicate()

    def test_existing_release_is_rejected_before_capture(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            release = root / "finish"
            release.write_text("prior attempt")
            result = subprocess.run([str(CLIENT), "--server", str(SERVER),
                "--scenario", "readiness", "--evidence", str(root / "capture"),
                "--finish-file", str(release)], capture_output=True, text=True, timeout=15)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("Finish-release file must be absent", result.stderr)
            self.assertFalse((root / "capture").exists())
            self.assertEqual(release.read_text(), "prior attempt")


if __name__ == "__main__":
    unittest.main()
