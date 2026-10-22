"""Reject corrupted desktop evidence without accessing Revit or either desktop."""
import copy
import hashlib
import json
from pathlib import Path
import shutil
import tempfile
import unittest

from verify_desktop_continuity import check_desktop_continuity

EVIDENCE = Path(__file__).resolve().parents[1] / 'docs/verification/desktop-readiness-evidence/2026-10-22'


class DesktopContinuityTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix='bal-desktop-continuity-')
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        shutil.copytree(EVIDENCE / 'codex-initial', self.root / 'case')
        shutil.copytree(EVIDENCE / 'sdk-july-continuity', self.root / 'sdk-july-continuity')
        self.case = self.root / 'case'
        self.original = {p.relative_to(self.case):p.read_bytes() for p in self.case.rglob('*') if p.is_file()}

    def edit(self, name, change, lines=False):
        path = self.case / name
        text = path.read_text(encoding='utf-8-sig')
        data = [json.loads(s) for s in text.splitlines()] if lines else json.loads(text)
        change(data)
        text = ''.join(json.dumps(row)+'\n' for row in data) if lines else json.dumps(data)
        path.write_text(text, encoding='utf-8')

    def rehash(self):
        def change(meta):
            for file in meta['files']:
                data = (self.case / file['path']).read_bytes()
                file.update(bytes=len(data), sha256=hashlib.sha256(data).hexdigest())
        self.edit('snapshot.json', change)

    def test_actual_capture(self):
        result = check_desktop_continuity(self.case)
        self.assertEqual(result['examinedInstances'], 16)
        self.assertEqual(result['findingCount'], 0)
        self.assertEqual(result['protocolVersion'], '2025-06-18')

    def test_semantic_corruptions(self):
        incoming = 'wire/client-to-server.jsonl'
        outgoing = 'wire/server-to-client.jsonl'
        cases = [
            ('request protocol', incoming, lambda x:x[0]['params'].update(protocolVersion='2025-11-25')),
            ('response protocol', outgoing, lambda x:x[0]['result'].update(protocolVersion='2025-11-25')),
            ('client identity', incoming, lambda x:x[0]['params']['clientInfo'].update(name='sdk-harness')),
            ('missing initialized', incoming, lambda x:x.pop(1)),
            ('missing catalog', incoming, lambda x:x.pop(2)),
            ('wrong catalog', outgoing, lambda x:x[1]['result']['tools'].pop()),
            ('duplicate request', incoming, lambda x:x.append(copy.deepcopy(x[-1]))),
            ('duplicate response', outgoing, lambda x:x.append(copy.deepcopy(x[-1]))),
            ('missing response', outgoing, lambda x:x.pop()),
            ('uncorrelated response', outgoing, lambda x:x[-1].update(id=999)),
            ('wrong token', incoming, lambda x:x[-1]['params']['arguments'].update(contextToken='other')),
            ('extra argument', incoming, lambda x:x[-1]['params']['arguments'].update(extra='ignored')),
            ('reversed calls', incoming, lambda x:x.__setitem__(slice(-2,None), list(reversed(x[-2:])))),
            ('tool error', outgoing, lambda x:x[-1]['result'].update(isError=True)),
            ('text disagreement', outgoing, lambda x:x[-1]['result']['content'][0].update(text='{}')),
            ('saved disagreement', 'check.json', lambda x:x['structuredContent']['report'].update(ExaminedInstances=15)),
            ('metadata request id', 'snapshot.json', lambda x:x.update(reportRequestId='a'*32)),
            ('metadata session', 'snapshot.json', lambda x:x.update(bridgeSessionId='b'*32)),
        ]
        for name, file, change in cases:
            with self.subTest(name=name):
                for path, data in self.original.items(): (self.case / path).write_bytes(data)
                self.edit(file, change, file.startswith('wire/'))
                self.rehash()  # Test semantics, not merely a stale checksum.
                with self.assertRaises((AssertionError, KeyError, ValueError)):
                    check_desktop_continuity(self.case)

    def test_consistently_changed_report_is_not_continuity(self):
        for field, value in [('CoverageComplete', False), ('ModifiedAfter', True), ('ExaminedInstances', 15)]:
            with self.subTest(field=field):
                for path, data in self.original.items(): (self.case / path).write_bytes(data)
                def alter(result):
                    result['structuredContent']['report'][field] = value
                    result['content'][0]['text'] = json.dumps(result['structuredContent'])
                self.edit('check.json', alter)
                self.edit('wire/server-to-client.jsonl', lambda rows:alter(rows[-1]['result']), True)
                self.rehash()
                with self.assertRaises(AssertionError): check_desktop_continuity(self.case)

    def test_partial_line_and_changed_bytes(self):
        wire = self.case / 'wire/client-to-server.jsonl'
        for index, data in enumerate([wire.read_bytes().rstrip(b'\n'), wire.read_bytes()+b'{}\n']):
            wire.write_bytes(data)
            if index == 0: self.rehash()  # A complete checksum does not make a partial line acceptable.
            with self.assertRaises(AssertionError): check_desktop_continuity(self.case)


if __name__ == '__main__':
    unittest.main()
