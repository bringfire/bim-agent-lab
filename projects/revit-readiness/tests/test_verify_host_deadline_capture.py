"""Corrupt retained evidence to test the checker; no running Revit required."""
import copy
import json
from pathlib import Path
import shutil
import tempfile
import unittest
from verify_host_deadline_capture import check_case, check_run


EVIDENCE = Path(__file__).resolve().parents[1] / 'docs/verification/host-deadline-evidence'


class DeadlineEvidenceTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix='bal-host-deadline-')
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name) / 'capture'
        shutil.copytree(EVIDENCE, self.root)
        self.case = self.root / 'preparation-deadline'

    def change_json(self, path, change):
        data = json.loads(path.read_text(encoding='utf-8-sig'))
        change(data)
        path.write_text(json.dumps(data), encoding='utf-8')

    def change_trace(self, name, change):
        path = self.case / name
        rows = [line.split('\t') for line in path.read_text().splitlines()]
        change(rows)
        path.write_text(''.join('\t'.join(row) + '\n' for row in rows), encoding='utf-8')

    def rejected(self):
        with self.assertRaises((AssertionError, KeyError, ValueError)):
            check_run(self.root)

    def test_original_two_cases_and_followup(self):
        self.assertTrue(check_run(self.root)['pass'])

    def test_wrong_expected_stage(self):
        with self.assertRaises(AssertionError):
            check_case(self.case, 'Reader')

    def test_wrong_process(self):
        self.change_json(self.case / 'observation.json', lambda d: d.update(revitPid=123))
        self.rejected()

    def test_wrong_request_identity(self):
        self.change_json(self.case / 'observation.json', lambda d: d.update(requestId='0' * 32))
        self.rejected()

    def test_reset_deadline(self):
        self.change_json(self.case / 'observation.json', lambda d: d.update(originalDeadlineUtc='2026-10-22T00:56:31Z'))
        self.rejected()

    def test_failed_capture(self):
        self.change_json(self.case / 'observation.json', lambda d: d.update(captureCompleted=False))
        self.rejected()

    def test_cleanup_release(self):
        self.change_json(self.case / 'observation.json', lambda d: d.update(cleanupRelease=True))
        self.rejected()

    def test_reader_entered_during_preparation(self):
        self.change_trace('after.tsv', lambda rows: rows[-1].__setitem__(4, 'ReaderEntered'))
        self.rejected()

    def test_missing_reader_exit(self):
        self.case = self.root / 'reader-deadline'
        self.change_trace('after.tsv', lambda rows: rows.__setitem__(slice(None), [r for r in rows if r[4] != 'ReaderExited']))
        self.rejected()

    def test_missing_executor_exit(self):
        self.change_trace('after.tsv', lambda rows: rows[-2].__setitem__(4, 'Other'))
        self.rejected()

    def test_missing_quiesced(self):
        self.change_trace('after.tsv', lambda rows: rows[-1].__setitem__(4, 'Other'))
        self.rejected()

    def test_automatic_release(self):
        self.change_trace('after.tsv', lambda rows: next(r for r in reversed(rows) if r[4] == 'GateReleased').__setitem__(4, 'AutomaticRelease'))
        self.rejected()

    def test_truncated_held_trace(self):
        self.change_trace('held.tsv', lambda rows: rows.pop())
        # The last row may be Pipe.Closed; force removal of the original invalidation observation too.
        self.change_trace('held.tsv', lambda rows: rows.__setitem__(slice(None), [r for r in rows if r[4] != 'InactiveObserved']))
        self.rejected()

    def test_response_after_release(self):
        self.change_json(self.case / 'sdk/response-ready.json', lambda d: d.update(receivedUtc='2026-10-22T02:00:00Z'))
        self.rejected()

    def test_short_connected_window(self):
        self.change_json(self.case / 'sdk/post-release-observation.json', lambda d: d.update(elapsedMilliseconds=2))
        self.rejected()

    def test_duplicate_wire_success(self):
        path = self.case / 'sdk/wire/server-to-client.jsonl'
        rows = [json.loads(line) for line in path.read_text().splitlines()]
        duplicate = copy.deepcopy(rows[-2])
        duplicate['result'] = {'isError': False, 'structuredContent': {'report': {}}}
        rows.append(duplicate)
        path.write_text(''.join(json.dumps(r) + '\n' for r in rows))
        self.rejected()

    def test_changed_followup_model_data(self):
        # Alter all corresponding result copies so the ordinary wire consistency
        # checker still passes; the cross-capture continuity check must reject it.
        case = self.root / 'after-deadlines'
        def change_result(result):
            result['structuredContent']['report']['ExaminedInstances'] = 999
            result['content'][0]['text'] = json.dumps(result['structuredContent'])
        self.change_json(case / 'check.json', change_result)
        wire = case / 'wire/server-to-client.jsonl'
        rows = [json.loads(line) for line in wire.read_text().splitlines()]
        for row in rows:
            if 'report' in row.get('result', {}).get('structuredContent', {}):
                change_result(row['result'])
        wire.write_text(''.join(json.dumps(r) + '\n' for r in rows))
        self.rejected()


if __name__ == '__main__':
    unittest.main()
