"""Negative controls for the retained live-evidence checker. No Revit access."""
import copy
import contextlib
import io
import json
import shutil
import tempfile
import unittest
from unittest.mock import patch
from pathlib import Path
from verify_live_lifecycle_capture import check_capture, check_restoration, check_followup, read, main

EVIDENCE = Path(__file__).resolve().parents[1] / 'docs/verification/live-installation-evidence'


class CaptureCheckerTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix='bal-live-capture-check-')
        self.root = Path(self.temp.name).resolve()
        self.addCleanup(self.temp.cleanup)
        shutil.copytree(EVIDENCE / 'modal-deadline', self.root / 'case')
        self.case = self.root / 'case'

    def mutate_json(self, relative, change):
        path = self.case / relative
        data = read(path)
        change(data)
        path.write_text(json.dumps(data))

    def mutate_wire(self, filename, change):
        path = self.case / 'wire' / filename
        rows = [json.loads(s) for s in path.read_text().splitlines()]
        change(rows)
        path.write_text(''.join(json.dumps(row)+'\n' for row in rows))

    def rejected(self):
        with self.assertRaises((AssertionError, KeyError, ValueError)):
            check_capture(self.case, 'DeadlineExceeded')

    def test_original_and_wrong_expected_outcome(self):
        check_capture(self.case, 'DeadlineExceeded')
        with self.assertRaises(AssertionError):
            check_capture(self.case, 'ReadinessReport')

    def test_failed_completion(self):
        self.mutate_json('complete.json', lambda row: row.update({'pass':False}))
        self.rejected()

    def test_saved_data_differs_from_wire(self):
        self.mutate_json('check.json', lambda row: row['structuredContent'].update(code='ContextChanged'))
        self.rejected()

    def test_changed_protocol(self):
        self.mutate_wire('client-to-server.jsonl', lambda rows: rows[-1]['params']['_meta'].update({'io.modelcontextprotocol/protocolVersion':'2025-11-25'}))
        self.rejected()

    def test_missing_response(self):
        self.mutate_wire('server-to-client.jsonl', lambda rows: rows.pop())
        self.rejected()

    def test_late_duplicate_success(self):
        def add_success(rows):
            extra = copy.deepcopy(rows[-1])
            extra['result']['isError'] = False
            extra['result']['structuredContent'] = {'report':{}}
            rows.append(extra)
        self.mutate_wire('server-to-client.jsonl', add_success)
        self.rejected()

    def test_disagreeing_text(self):
        self.mutate_wire('server-to-client.jsonl', lambda rows: rows[-1]['result']['content'][0].update(text='{}'))
        self.rejected()

    def test_unsolicited_response(self):
        def add_unknown(rows):
            extra = copy.deepcopy(rows[-1])
            extra['id'] = 9999
            rows.append(extra)
        self.mutate_wire('server-to-client.jsonl', add_unknown)
        self.rejected()

    def test_followup_rejects_short_connected_observation(self):
        shutil.copytree(EVIDENCE, self.root, dirs_exist_ok=True)
        path = self.root / 'modal-deadline-connected/post-release-observation.json'
        changed = read(path)
        changed['elapsedMilliseconds'] = 0
        path.write_text(json.dumps(changed))
        with patch('sys.argv', ['checker', str(self.root)]), contextlib.redirect_stdout(io.StringIO()):
            with self.assertRaises(AssertionError): main()

    def corrupt_followup(self, name, change):
        shutil.copytree(EVIDENCE, self.root, dirs_exist_ok=True)
        check_followup(self.root)
        path = self.root / name
        data = read(path)
        change(data)
        path.write_text(json.dumps(data))
        with self.assertRaises(AssertionError): check_followup(self.root)

    def test_followup_rejects_client_closed_before_ui_release(self):
        self.corrupt_followup('modal-connected-ui-released.json', lambda d:d.update(clientCompleteAtUiRelease=True))

    def test_followup_rejects_cached_value(self):
        def change(d):
            for t in d['structuredContent']['report']['Evaluation']['Types']:
                if t['TypeName'] == 'BAL INFO Wall A':
                    next(f for f in t['Fields'] if f['Field'] == 'Code')['ObservedValue'] = 'W-A'
        self.corrupt_followup('same-token-after-value-edit/check.json', change)

    def test_followup_rejects_expired_restart_control(self):
        self.corrupt_followup('restart-stale-context/sdk-client-result.json', lambda d:d.update(completedUtc='2026-10-22T00:00:00Z'))

    def test_followup_rejects_timeout_as_stop(self):
        self.corrupt_followup('stop-incomplete-connections-retry/result.json', lambda d:d['results'][0].update(elapsedMilliseconds=5000))

    def test_followup_rejects_stale_workbook(self):
        self.corrupt_followup('manual-export/BAL-info-saved-as.readiness.json', lambda d:d.update(GeneratedUtc='2026-10-22T00:00:00Z'))

    def test_followup_rejects_changed_postclose_hash(self):
        self.corrupt_followup('final-closed-model-hashes.json', lambda d:d['verified'][0].update(sha256='0'*64))

    def test_restoration_and_document_lifetime_controls(self):
        for name in ('before-install.json','after-undo-first.json','after-undo-second.json',
                     'start-first-descriptor.json','start-twice-descriptor.json','restart-descriptor.json',
                     'before-explicit-start.json','stop-first.json','stop-second.json',
                     'after-modal-report/context.json','reopened-fresh-context/context.json'):
            path = self.root / name
            path.parent.mkdir(parents=True,exist_ok=True)
            shutil.copyfile(EVIDENCE/name,path)
        check_restoration(self.root)
        path = self.root/'after-undo-second.json'
        original = path.read_bytes()
        changed = read(path)
        changed['server']['exists'] = True
        path.write_text(json.dumps(changed))
        with self.assertRaises(AssertionError): check_restoration(self.root)
        path.write_bytes(original)
        path = self.root/'reopened-fresh-context/context.json'
        changed = read(path)
        changed['structuredContent']['documentLifetimeId'] = read(self.root/'after-modal-report/context.json')['structuredContent']['documentLifetimeId']
        path.write_text(json.dumps(changed))
        with self.assertRaises(AssertionError): check_restoration(self.root)


if __name__ == '__main__': unittest.main()
