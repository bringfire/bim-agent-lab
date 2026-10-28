"""Selected-year installation with real built payloads in disposable roots only."""
from pathlib import Path
import subprocess
import tempfile
import unittest

PROJECT = Path(__file__).resolve().parents[1]


class RevitDeploymentTests(unittest.TestCase):
    def run_case(self, scenario):
        with tempfile.TemporaryDirectory(prefix='bal-year-deployment-') as root:
            result = subprocess.run(['pwsh', '-NoProfile', '-File', str(PROJECT / 'tests/revit-year-deployment.ps1'), '-Root', root, '-Scenario', scenario], cwd=PROJECT, capture_output=True, text=True, timeout=120)
            self.assertEqual(0, result.returncode, result.stdout + result.stderr)
            self.assertIn('PASS ', result.stdout)

    def test_real_assembly_metadata_distinguishes_both_hosts(self):
        self.run_case('metadata')

    def test_modern_payload_roundtrip_preserves_2024(self):
        self.run_case('roundtrip')

    def test_wrong_year_destination_is_preflight_failure(self):
        self.run_case('wrong-year')

    def test_probe_receipt_roundtrip_preserves_2024(self):
        self.run_case('probe')

    def test_legacy_binary_in_modern_folder_is_rejected(self):
        self.run_case('mixed')

    def test_interrupted_probe_recovers_without_touching_2024(self):
        self.run_case('probe-interrupted')

    def test_invalid_probe_template_is_rejected_before_install(self):
        self.run_case('probe-template')

    def test_reinstall_and_stacked_undo_preserve_first_install_then_absence(self):
        self.run_case('reinstall')


if __name__ == '__main__':
    unittest.main()
