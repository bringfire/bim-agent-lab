"""Synthetic transition/error over real retained wire fixtures; never live proof."""
import copy
from datetime import datetime, timedelta
import hashlib
import json
from pathlib import Path
import shutil
import tempfile
import unittest
from verify_mcp_evidence import expected_report, read, verify_stale

EVIDENCE=Path(__file__).resolve().parents[1]/'docs/verification'


class StaleEvidenceTests(unittest.TestCase):
    def setUp(self):
        self.temp=tempfile.TemporaryDirectory(prefix='bal-stale-evidence-');self.addCleanup(self.temp.cleanup)
        self.root=Path(self.temp.name)/'stale'
        shutil.copytree(EVIDENCE/'desktop-readiness-evidence/2026-10-22/codex-initial',self.root)
        shutil.copyfile(self.root/'snapshot.json',self.root/'capture.json')
        shutil.copytree(EVIDENCE/'mcp-readiness-evidence/2026-10-22-wall-defects/codex-wall-defects-02',self.root/'recovery')
        ctx=read(self.root/'context.json')['structuredContent']
        prepared=dict(case='complete',preparedUtc='2026-10-22T00:00:00Z',modelPath=ctx['documentPath'],
            checklistPath=ctx['checklistPath'],checklistSha256=ctx['checklistSha256'],
            revitPid=ctx['revitPid'],bridgeSessionId=ctx['bridgeSessionId'],
            expectedReport=expected_report('complete',ctx['documentPath'],ctx['checklistPath']))
        self.write('expectation.json',prepared)
        old=read(self.root/'check.json')['structuredContent']
        error=dict(contractVersion=1,requestId=old['requestId'],code='ContextChanged',message='Synthetic document-switch test')
        result=dict(isError=True,structuredContent=error,content=[dict(type='text',text=json.dumps(error))])
        self.write('check.json',result)
        rows=[json.loads(x) for x in (self.root/'wire/server-to-client.jsonl').read_bytes().splitlines()]
        rows[-1]['result']=result
        (self.root/'wire/server-to-client.jsonl').write_text(''.join(json.dumps(x)+'\n' for x in rows),encoding='utf-8')
        meta=read(self.root/'capture.json')
        for row in meta['files']:
            data=(self.root/row['path']).read_bytes();row.update(bytes=len(data),sha256=hashlib.sha256(data).hexdigest())
        self.write('capture.json',meta)
        expires=datetime.fromisoformat(ctx['expiresUtc'].replace('Z','+00:00'))
        self.write('transition.json',dict(recoveryCase='wall-defects',humanAction='Synthetic fixture only',
            contextReceivedUtc=(expires-timedelta(minutes=4)).isoformat(),
            switchConfirmedUtc=(expires-timedelta(minutes=3)).isoformat(),
            checkStartedUtc=(expires-timedelta(minutes=2)).isoformat(),
            checkFinishedUtc=(expires-timedelta(minutes=1)).isoformat()))
        self.original={p.relative_to(self.root):p.read_bytes() for p in self.root.rglob('*') if p.is_file()}

    def write(self,name,value):
        (self.root/name).write_text(json.dumps(value,indent=2),encoding='utf-8')

    def test_transition_and_independent_recovery(self):
        result=verify_stale(self.root)
        self.assertTrue(result.get('pass'))
        self.assertEqual(result.get('code'),'ContextChanged')
        self.assertEqual(result['recovery']['findings'],2)

    def test_bad_transition_metadata_rejected(self):
        changes=[
            lambda m:m.update(checkFinishedUtc='2026-10-23T00:00:00Z'),
            lambda m:m.update(checkStartedUtc='2026-10-21T00:00:00Z'),
            lambda m:m.update(switchConfirmedUtc='2026-10-23T00:00:00Z'),
            lambda m:m.update(contextReceivedUtc='2026-10-21T00:00:00Z'),
            lambda m:m.update(checkFinishedUtc='2026-10-22T12:00:00'),
            lambda m:m.update(humanAction=''),
            lambda m:m.update(recoveryCase='ceiling-negative'),
        ]
        for change in changes:
            with self.subTest(change=change):
                for p,data in self.original.items():(self.root/p).write_bytes(data)
                meta=read(self.root/'transition.json');change(meta);self.write('transition.json',meta)
                with self.assertRaises((AssertionError,KeyError,ValueError)):verify_stale(self.root)

    def test_original_binding_must_match_expectation(self):
        for key,value in [('modelPath','D:/wrong.rvt'),('bridgeSessionId','wrong'),('revitPid',1),('checklistSha256','wrong')]:
            with self.subTest(key=key):
                for p,data in self.original.items():(self.root/p).write_bytes(data)
                prepared=read(self.root/'expectation.json');prepared[key]=value;self.write('expectation.json',prepared)
                with self.assertRaises(AssertionError):verify_stale(self.root)

if __name__=='__main__':unittest.main()
