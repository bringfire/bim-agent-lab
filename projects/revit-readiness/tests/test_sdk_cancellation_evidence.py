import json
from pathlib import Path
import shutil
import tempfile
import unittest

from verify_sdk_cancellation_diagnosis import verify


class CancellationEvidenceTests(unittest.TestCase):
    source = Path(__file__).resolve().parents[1] / "docs/verification/sdk-cancellation-evidence/characterized-final"

    def test_retained_capture(self):
        self.assertEqual(6, verify(self.source))

    def test_corruptions_are_rejected(self):
        for corruption in ("prefix", "identity", "remote", "deadline", "forced", "mechanism", "catalog"):
            with self.subTest(corruption=corruption), tempfile.TemporaryDirectory() as temporary:
                root = Path(temporary) / "capture"
                shutil.copytree(self.source, root)
                case = root / "2026-07-28-token"
                if corruption == "prefix":
                    (case / "wire-before-rescue.jsonl").write_text('{}\n', encoding="utf-8")
                elif corruption == "identity":
                    path = case / "wire/client-to-server.jsonl"
                    messages = [json.loads(line) for line in path.read_text().splitlines()]
                    next(m for m in messages if m.get("method") == "notifications/cancelled")["params"]["requestId"] = "unrelated"
                    path.write_text(''.join(json.dumps(m) + '\n' for m in messages))
                elif corruption in ("remote", "deadline"):
                    path = case / "observation.json"
                    data = json.loads(path.read_text())
                    if corruption == "remote": data["remoteCanceledBeforeRescue"] = True
                    else: data["originalDeadline"] = data["finalDeadline"] = data["observedUtc"]
                    path.write_text(json.dumps(data))
                elif corruption == "forced":
                    (case / "cleanup.json").write_text('{"forced":true,"ExitCode":0}')
                elif corruption == "mechanism":
                    path = root / "mechanism.json"
                    data = json.loads(path.read_text())
                    data[0]["events"].insert(1, "send-callback")
                    path.write_text(json.dumps(data))
                else:
                    path = case / "wire/server-to-client.jsonl"
                    messages = [json.loads(line) for line in path.read_text().splitlines()]
                    next(m for m in messages if "tools" in m.get("result", {}))["result"]["tools"] = []
                    path.write_text(''.join(json.dumps(m) + '\n' for m in messages))
                with self.assertRaises(AssertionError): verify(root)


if __name__ == "__main__":
    unittest.main()
