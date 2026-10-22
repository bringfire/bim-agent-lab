import copy
import hashlib
import json
from pathlib import Path
import shutil
import tempfile
import unittest
from verify_mcp_evidence import verify_wire, verify, expected_report

SOURCE=Path(__file__).resolve().parents[1]/'docs/verification/desktop-readiness-evidence/2026-10-22/codex-initial'

class CaseWireTests(unittest.TestCase):
    def setUp(self):
        self.temp=tempfile.TemporaryDirectory(prefix='bal-case-wire-');self.addCleanup(self.temp.cleanup)
        self.root=Path(self.temp.name)/'case';shutil.copytree(SOURCE,self.root)
        (self.root/'capture.json').write_bytes((self.root/'snapshot.json').read_bytes())
        self.original={p.relative_to(self.root):p.read_bytes() for p in self.root.rglob('*') if p.is_file()}

    def change(self,name,action):
        p=self.root/name
        lines=name.endswith('.jsonl')
        data=[json.loads(x) for x in p.read_text(encoding='utf-8-sig').splitlines()] if lines else json.loads(p.read_text(encoding='utf-8-sig'))
        action(data)
        p.write_text(''.join(json.dumps(x)+'\n' for x in data) if lines else json.dumps(data),encoding='utf-8')

    def rehash(self):
        def update(meta):
            for row in meta['files']:
                data=(self.root/row['path']).read_bytes();row.update(bytes=len(data),sha256=hashlib.sha256(data).hexdigest())
        self.change('capture.json',update)

    def test_actual_pair(self):
        context,check=verify_wire(self.root)
        self.assertFalse(context['isError']);self.assertFalse(check['isError'])

    def prepare_expectation(self):
        ctx=json.loads((self.root/'context.json').read_text(encoding='utf-8-sig'))['structuredContent']
        prepared=dict(case='complete',preparedUtc='2026-10-22T00:00:00Z',
            modelPath=ctx['documentPath'],checklistPath=ctx['checklistPath'],
            checklistSha256=ctx['checklistSha256'],revitPid=ctx['revitPid'],bridgeSessionId=ctx['bridgeSessionId'],
            expectedReport=expected_report('complete',ctx['documentPath'],ctx['checklistPath']))
        (self.root/'expectation.json').write_text(json.dumps(prepared),encoding='utf-8')

    def test_full_binding(self):
        self.prepare_expectation()
        self.assertTrue(verify(self.root,'complete')['pass'])

    def test_context_semantics_cannot_disagree_with_report(self):
        self.prepare_expectation()
        for field in ('projectUniqueId','revitBuild','checklistId','scope','level','phase'):
            with self.subTest(field=field):
                for p,data in self.original.items():(self.root/p).write_bytes(data)
                def change_result(result):
                    result['structuredContent'][field]='wrong'
                    result['content'][0]['text']=json.dumps(result['structuredContent'])
                self.change('context.json',change_result)
                self.change('wire/server-to-client.jsonl',lambda rows:change_result(rows[-2]['result']))
                self.rehash()
                with self.assertRaises(AssertionError):verify(self.root,'complete')

    def test_semantic_corruptions(self):
        inputs='wire/client-to-server.jsonl';outputs='wire/server-to-client.jsonl'
        controls=[
            (inputs,lambda x:x[-1]['params']['arguments'].update(contextToken='wrong')),
            (inputs,lambda x:x[-1]['params'].update(name='different_tool')),
            (inputs,lambda x:x.append(copy.deepcopy(x[-1]))),
            (outputs,lambda x:x.pop()),
            (outputs,lambda x:x[-1].update(id=99)),
            (outputs,lambda x:x.append(copy.deepcopy(x[-1]))),
            (outputs,lambda x:x[-1]['result'].update(isError=True)),
            (outputs,lambda x:x[-1]['result']['content'][0].update(text='{}')),
            (outputs,lambda x:x[0]['result'].update(protocolVersion='2025-11-25')),
            (outputs,lambda x:x[1]['result']['tools'].pop()),
            ('capture.json',lambda x:x.update(reportRequestId='f'*32)),
            ('capture.json',lambda x:x.update(contextRequestId='e'*32)),
            ('capture.json',lambda x:x.update(protocolVersion='2024-11-05')),
            ('check.json',lambda x:x['structuredContent']['report'].update(CoverageComplete=False)),
        ]
        for name,action in controls:
            with self.subTest(name=name,action=action):
                for p,data in self.original.items():(self.root/p).write_bytes(data)
                self.change(name,action);self.rehash()
                with self.assertRaises((AssertionError,KeyError,ValueError)):verify_wire(self.root)

if __name__=='__main__':unittest.main()
