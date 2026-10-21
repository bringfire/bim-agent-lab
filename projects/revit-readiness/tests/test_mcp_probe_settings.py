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


if __name__ == '__main__': unittest.main()
