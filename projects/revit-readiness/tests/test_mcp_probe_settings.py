import json
import sys
import subprocess
import tempfile
import tomllib
import unittest
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / 'scripts'))
import manage_mcp_probe_settings as settings


class SettingsTests(unittest.TestCase):
    def test_prepared_recovery_exists_before_mutation_and_restores_after_interruption(self):
        for fmt in ['json', 'toml']:
            with self.subTest(format=fmt), tempfile.TemporaryDirectory() as temp:
                root = Path(temp); config = root / ('config.' + fmt); receipt = root/'receipt.json'
                before = b'{"mcpServers":{"other":{"command":"keep"}}}' if fmt == 'json' else b'[mcp_servers.other]\ncommand = "keep"\n'
                config.write_bytes(before)
                settings.prepare(config, fmt, 'C:/probe.exe', 'C:/evidence', receipt)
                self.assertEqual(config.read_bytes(), before)
                self.assertTrue(receipt.exists(), 'No durable recovery record before configuration mutation')
                self.assertEqual(json.loads(receipt.read_text())['state'], 'Prepared')
                settings.apply(receipt)
                # Simulate death after config mutation but before completion marker.
                data = json.loads(receipt.read_text()); data['state'] = 'Prepared'; receipt.write_text(json.dumps(data))
                for _ in range(2):
                    result = subprocess.run([sys.executable, settings.__file__, 'restore', '--receipt', str(receipt)], capture_output=True, text=True)
                    self.assertEqual(result.returncode, 0, result.stderr)
                self.assertEqual(config.read_bytes(), before)

    def test_recovery_preserves_unrelated_edits_and_rejects_foreign_owned_entry(self):
        for fmt in ['json', 'toml']:
            with self.subTest(format=fmt), tempfile.TemporaryDirectory() as temp:
                root = Path(temp); config=root/('config.'+fmt); receipt=root/'receipt.json'
                config.write_text('{}' if fmt == 'json' else '', encoding='utf-8')
                settings.prepare(config, fmt, 'C:/probe.exe', 'C:/evidence', receipt); settings.apply(receipt)
                text=config.read_text()
                config.write_text(text.replace('C:/probe.exe','C:/foreign.exe'))
                foreign=config.read_bytes()
                with self.assertRaises(ValueError): settings.restore(receipt)
                self.assertEqual(config.read_bytes(),foreign)
                config.write_text(text)
                if fmt == 'json':
                    data=json.loads(text); data['other']='concurrent'; config.write_text(json.dumps(data))
                else: config.write_text(text+'\n[other]\nvalue="concurrent"\n')
                settings.restore(receipt)
                data=json.loads(config.read_text()) if fmt=='json' else tomllib.loads(config.read_text())
                self.assertIn('other',data)
                self.assertNotIn('bim-agent-lab-protocol-probe',data.get('mcpServers',data.get('mcp_servers',{})))

    def test_apply_refuses_changed_configuration(self):
        with tempfile.TemporaryDirectory() as temp:
            root=Path(temp); config=root/'config.json'; receipt=root/'receipt.json'; config.write_text('{}')
            settings.prepare(config,'json','C:/probe.exe','C:/evidence',receipt)
            config.write_text('{"new":true}')
            with self.assertRaises(ValueError): settings.apply(receipt)
            self.assertEqual(config.read_text(),'{"new":true}')


class ReadinessSettingsTests(unittest.TestCase):
    def prepare(self, config, fmt, receipt, pid='149000'):
        return subprocess.run([sys.executable, settings.__file__, 'prepare', '--receipt', str(receipt),
            '--config', str(config), '--format', fmt, '--command', 'C:/shared/readiness.exe',
            '--evidence', 'D:/evidence with spaces', '--name', 'bim-agent-lab-revit-readiness',
            '--revit-pid', pid], capture_output=True, text=True)

    def test_real_cli_prepares_readiness_identity_pid_and_session_root_before_writes(self):
        for fmt in ['json', 'toml']:
            with self.subTest(format=fmt), tempfile.TemporaryDirectory() as temp:
                config=Path(temp)/('config.'+fmt); receipt=Path(temp)/'receipt.json'
                before=b'{"mcpServers":{"other":{"command":"keep"}}}' if fmt=='json' else b'[mcp_servers.other]\ncommand="keep"\n'
                config.write_bytes(before)
                result=self.prepare(config,fmt,receipt)
                self.assertEqual(result.returncode,0,result.stderr)
                self.assertEqual(config.read_bytes(),before)
                record=json.loads(receipt.read_text()); self.assertEqual(record['state'],'Prepared')
                self.assertEqual(record['name'],'bim-agent-lab-revit-readiness')
                self.assertEqual(record['entry']['args'],['--revit-pid','149000','--wire-evidence-root','D:/evidence with spaces'])
                settings.apply(receipt)
                parsed=settings.parse(config.read_bytes(),fmt); servers=parsed[record['key']]
                self.assertEqual(servers['other'],{'command':'keep'})
                self.assertEqual(servers[record['name']],record['entry'])
                # Restore in another process, even with the old Prepared state marker.
                record['state']='Prepared'; receipt.write_text(json.dumps(record))
                for _ in range(2):
                    result=subprocess.run([sys.executable,settings.__file__,'restore','--receipt',str(receipt)],capture_output=True,text=True)
                    self.assertEqual(result.returncode,0,result.stderr)
                self.assertEqual(config.read_bytes(),before)

    def test_readiness_recovery_preserves_probe_and_concurrent_unrelated_edits(self):
        for fmt in ['json','toml']:
            with self.subTest(format=fmt), tempfile.TemporaryDirectory() as temp:
                config=Path(temp)/('config.'+fmt); probe=Path(temp)/'probe.json'; receipt=Path(temp)/'receipt.json'
                config.write_text('{}' if fmt=='json' else '')
                settings.prepare(config,fmt,'C:/probe.exe','D:/probe',probe); settings.apply(probe)
                result=self.prepare(config,fmt,receipt); self.assertEqual(result.returncode,0,result.stderr)
                settings.apply(receipt)
                if fmt=='json':
                    d=json.loads(config.read_text()); d['later']='keep'; config.write_text(json.dumps(d))
                else: config.write_text(config.read_text()+'\n[later]\nvalue="keep"\n')
                settings.restore(receipt)
                d=settings.parse(config.read_bytes(),fmt); key='mcpServers' if fmt=='json' else 'mcp_servers'
                self.assertIn(settings.NAME,d[key]); self.assertNotIn('bim-agent-lab-revit-readiness',d[key]); self.assertIn('later',d)

    def test_readiness_foreign_entry_is_preserved(self):
        for fmt in ['json','toml']:
            with self.subTest(format=fmt), tempfile.TemporaryDirectory() as temp:
                config=Path(temp)/('config.'+fmt); receipt=Path(temp)/'receipt.json'
                config.write_text('{}' if fmt=='json' else '')
                result=self.prepare(config,fmt,receipt); self.assertEqual(result.returncode,0,result.stderr)
                settings.apply(receipt)
                config.write_text(config.read_text().replace('149000','149001'))
                before=config.read_bytes()
                with self.assertRaises(ValueError): settings.restore(receipt)
                self.assertEqual(config.read_bytes(),before)

    def test_invalid_pid_never_prepares_or_writes(self):
        for pid in ['0','-1','2147483648']:
            with self.subTest(pid=pid), tempfile.TemporaryDirectory() as temp:
                config=Path(temp)/'config.json'; receipt=Path(temp)/'receipt.json'; config.write_text('{}')
                result=self.prepare(config,'json',receipt,pid)
                self.assertNotEqual(result.returncode,0)
                self.assertFalse(receipt.exists()); self.assertEqual(config.read_text(),'{}')

    def test_legacy_receipt_without_name_restores_its_probe(self):
        for fmt in ['json','toml']:
            with self.subTest(format=fmt), tempfile.TemporaryDirectory() as temp:
                config=Path(temp)/('config.'+fmt); receipt=Path(temp)/'receipt.json'
                before=b'{}' if fmt=='json' else b''; config.write_bytes(before)
                settings.prepare(config,fmt,'C:/probe.exe','D:/probe',receipt); settings.apply(receipt)
                data=json.loads(receipt.read_text()); del data['name']; receipt.write_text(json.dumps(data))
                result=subprocess.run([sys.executable,settings.__file__,'restore','--receipt',str(receipt)],capture_output=True,text=True)
                self.assertEqual(result.returncode,0,result.stderr)
                self.assertEqual(config.read_bytes(),before)


if __name__ == '__main__': unittest.main()
