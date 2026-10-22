import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile
import unittest

sys.path.insert(0,str(Path(__file__).resolve().parents[1]/'scripts'))
from prepare_mcp_controls import prepare, cleanup

EVIDENCE=Path(__file__).resolve().parents[1]/'docs/verification/required-information-evidence'
SCRIPT=Path(__file__).resolve().parents[1]/'scripts/prepare_mcp_controls.py'


class ControlsTests(unittest.TestCase):
    def setUp(self):
        self.temp=tempfile.TemporaryDirectory(prefix='bal-mcp-controls-')
        self.addCleanup(self.temp.cleanup)
        self.root=Path(self.temp.name);self.source=self.root/'source';self.source.mkdir()
        (self.source/'BAL-info-complete.rvt').write_bytes(b'disposable byte-copy fixture\0'*50)
        shutil.copyfile(EVIDENCE/'BAL-info-complete.checklist.json',self.source/'BAL-info-complete.checklist.json')
        self.original={p.name:p.read_bytes() for p in self.source.iterdir()}
        self.output=self.root/'controls'

    def run_process(self,*args):
        return subprocess.run([sys.executable,str(SCRIPT),*map(str,args)],capture_output=True,text=True)

    def test_complete_and_cleanup(self):
        prepare(self.source,self.output)
        self.assertTrue((self.output/'complete.json').exists())
        receipt=json.loads((self.output/'preparation.json').read_text(encoding='utf-8'))
        self.assertEqual(len(receipt['files']),4)
        for item in receipt['files']:
            self.assertEqual(hashlib.sha256((self.output/item['path']).read_bytes()).hexdigest(),item['sha256'])
        nc1=json.loads((self.output/'nc1/BAL-info-complete.checklist.json').read_text(encoding='utf-8'))
        nc2=json.loads((self.output/'nc2/BAL-info-complete.checklist.json').read_text(encoding='utf-8'))
        original=json.loads(self.original['BAL-info-complete.checklist.json'])
        self.assertEqual(nc1['Cases'][0]['CaseId'],'BAL-INFO-WA-01')
        self.assertTrue(nc1['Cases'][0]['UniqueId'].endswith('-7fffffff'))
        nc1['Cases'][0]['UniqueId']=original['Cases'][0]['UniqueId'];self.assertEqual(nc1,original)
        self.assertEqual(nc2['Cases'],original['Cases'])
        self.assertEqual(nc2['Requirements'],[r for r in original['Requirements'] if r['Source']!='BAL-TYPE-INFO-v1: BAL INFO Ceiling ACT'])
        self.assertEqual({p.name:p.read_bytes() for p in self.source.iterdir()},self.original)
        result=self.run_process('cleanup','--output',self.output)
        self.assertEqual(result.returncode,0,result.stderr)
        self.assertFalse(self.output.exists())

    def test_existing_ready_refused(self):
        prepare(self.source,self.output)
        before={p.relative_to(self.output):p.read_bytes() for p in self.output.rglob('*') if p.is_file()}
        with self.assertRaises(FileExistsError):prepare(self.source,self.output)
        self.assertEqual(before,{p.relative_to(self.output):p.read_bytes() for p in self.output.rglob('*') if p.is_file()})

    def test_interruption_and_recovery_in_new_process(self):
        for stage in ('first-copy','first-sidecar'):
            with self.subTest(stage=stage):
                result=self.run_process('prepare','--source',self.source,'--output',self.output,'--fail-after',stage)
                self.assertNotEqual(result.returncode,0)
                self.assertTrue((self.output/'preparation.json').exists())
                self.assertFalse((self.output/'complete.json').exists())
                repeated=self.run_process('prepare','--source',self.source,'--output',self.output)
                self.assertNotEqual(repeated.returncode,0)
                self.assertIn('Prepared',repeated.stderr)
                recovered=self.run_process('cleanup','--output',self.output)
                self.assertEqual(recovered.returncode,0,recovered.stderr)
                self.assertFalse(self.output.exists())
        self.assertEqual({p.name:p.read_bytes() for p in self.source.iterdir()},self.original)

    def test_foreign_file_or_changed_bytes_retained(self):
        for foreign in ('extra','changed'):
            with self.subTest(foreign=foreign):
                prepare(self.source,self.output)
                target=self.output/('extra.txt' if foreign=='extra' else 'nc2/BAL-info-complete.rvt')
                original=target.read_bytes() if target.exists() else None
                target.write_bytes(b'foreign change')
                before={p.relative_to(self.output):p.read_bytes() for p in self.output.rglob('*') if p.is_file()}
                with self.assertRaises(ValueError):cleanup(self.output)
                self.assertEqual(before,{p.relative_to(self.output):p.read_bytes() for p in self.output.rglob('*') if p.is_file()})
                if original is None:target.unlink()
                else:target.write_bytes(original)
                cleanup(self.output)

    def test_cleanup_interrupt_reentry(self):
        prepare(self.source,self.output)
        result=self.run_process('cleanup','--output',self.output,'--fail-after','first-delete')
        self.assertNotEqual(result.returncode,0)
        self.assertTrue((self.output/'preparation.json').exists())
        recovered=self.run_process('cleanup','--output',self.output)
        self.assertEqual(recovered.returncode,0,recovered.stderr)
        self.assertFalse(self.output.exists())

    def test_bad_source_and_escape_receipt(self):
        (self.source/'BAL-info-complete.rvt').unlink()
        with self.assertRaises(FileNotFoundError):prepare(self.source,self.output)
        self.assertFalse(self.output.exists())
        (self.source/'BAL-info-complete.rvt').write_bytes(self.original['BAL-info-complete.rvt'])
        prepare(self.source,self.output)
        file=self.output/'preparation.json';data=json.loads(file.read_text(encoding='utf-8'))
        data['files'][0]['path']='../source/BAL-info-complete.rvt'
        file.write_text(json.dumps(data),encoding='utf-8')
        with self.assertRaises(ValueError):cleanup(self.output)
        self.assertEqual((self.source/'BAL-info-complete.rvt').read_bytes(),self.original['BAL-info-complete.rvt'])

    def test_changed_completion_marker_retained(self):
        prepare(self.source,self.output)
        marker=self.output/'complete.json'
        marker.write_text('{"state":"foreign"}',encoding='utf-8')
        before={p.relative_to(self.output):p.read_bytes() for p in self.output.rglob('*') if p.is_file()}
        with self.assertRaises(ValueError):cleanup(self.output)
        self.assertEqual(before,{p.relative_to(self.output):p.read_bytes() for p in self.output.rglob('*') if p.is_file()})


if __name__=='__main__':unittest.main()
