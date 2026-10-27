"""Runner boundaries with a labelled subprocess fixture; no Revit is simulated as live evidence."""
import copy
import hashlib
import importlib.util
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest
from unittest.mock import patch
import time

PROJECT = Path(__file__).resolve().parents[1]
SCRIPT = PROJECT / 'scripts/invoke_readiness_mcp.py'
FIXTURES = PROJECT / 'docs/verification/mcp-readiness-evidence'


class RunnerTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix='bal runner spaces-')
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.server = self.root / 'synthetic-server.txt'
        self.server.write_text('normal', encoding='utf-8')
        self.output = self.root / 'attempt'
        self.expectation = self.root / 'expectation.json'

    def prepare(self, case='complete'):
        group = 'complete-and-context-switch' if case == 'complete' else case
        source = FIXTURES / ('2026-10-22-' + group) / ('sdk-july-' + case)
        expected = json.loads((source / 'expectation.json').read_text(encoding='utf-8-sig'))
        model = self.root / 'synthetic model.rvt'
        checklist = model.with_suffix('.checklist.json')
        model.write_bytes(b'SYNTHETIC TEST BYTES; not a Revit model')
        checklist.write_text('{}', encoding='utf-8')
        old_model, old_checklist = expected['modelPath'], expected['checklistPath']
        expected.update(modelPath=str(model), checklistPath=str(checklist))
        expected['expectedReport'].update(DocumentPath=str(model), ChecklistPath=str(checklist))
        for key, path in [('model', model), ('checklist', checklist)]:
            expected[key + 'Sha256'] = hashlib.sha256(path.read_bytes()).hexdigest()
        expected['oracleSources'] = []  # Independent oracle is still read by the actual comparator.
        if case == 'nc1':
            proof = self.root / 'synthetic-absence.json'
            proof.write_text('{"kind":"synthetic test observation"}', encoding='utf-8')
            expected['absenceEvidence'] = {'path': str(proof), 'sha256': hashlib.sha256(proof.read_bytes()).hexdigest()}
        self.expected = expected
        self.expectation.write_text(json.dumps(expected), encoding='utf-8')
        config = dict(source=str(source), oldModel=old_model, oldChecklist=old_checklist,
                      model=str(model), checklist=str(checklist), expectation=str(self.expectation))
        (self.root / 'fixture.json').write_text(json.dumps(config), encoding='utf-8')
        return expected

    def invoke(self, mode='normal', timeout=5, **overrides):
        self.server.write_text('exit' if mode == 'cleanup-error' else mode, encoding='utf-8')
        if not SCRIPT.exists():
            self.fail('The SDK capture runner is not implemented.')
        spec = importlib.util.spec_from_file_location('runner_under_test', SCRIPT)
        module = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(module)
        options = dict(expectation=self.expectation, output=self.output, server=self.server,
                       client=[sys.executable, str(PROJECT / 'tests/runner_fixture_client.py')],
                       case=self.expected['case'], revit_pid=149972, protocol='2026-07-28',
                       timeout_seconds=timeout)
        options.update(overrides)
        try:
            if mode == 'cleanup-error':
                with patch.object(module, 'retire', side_effect=OSError('synthetic process cleanup failure')):
                    result = module.run(**options)
            elif mode == 'slow-verifier':
                original = module.verify
                def delayed(*args):
                    result = original(*args)
                    time.sleep(0.6)
                    return result
                with patch.object(module, 'verify', side_effect=delayed):
                    result = module.run(**options)
            else:
                result = module.run(**options)
            return True, result
        except Exception as error:
            return False, str(error)

    def test_all_five_cases_publish_only_after_real_comparison(self):
        for case, coverage, complete, examined in [('complete', True, True, 16),
                ('wall-defects', True, True, 16), ('ceiling-negative', True, True, 16),
                ('nc1', False, True, 15), ('nc2', False, False, 16)]:
            with self.subTest(case=case):
                self.prepare(case); self.output = self.root / case
                ok, result = self.invoke()
                self.assertTrue(ok, result)
                marker = json.loads((self.output / 'capture/comparison-complete.json').read_text(encoding='utf-8'))
                self.assertEqual((marker['coverageComplete'], marker['evaluationComplete'], marker['examinedInstances']),
                                 (coverage, complete, examined))
                self.assertEqual((self.output / 'expectation.json').read_bytes(), self.expectation.read_bytes())
                self.assertTrue((self.output / 'prepared.json').exists())
                self.assertTrue((self.output / 'context-validated.json').exists())
                self.assertTrue((self.output / 'capture/complete.json').exists())
                self.assertFalse((self.output / 'failed.json').exists())

    def test_bad_inputs_rejected_before_output_or_process(self):
        for kind in ['model', 'checklist', 'pid', 'oracle', 'unconfirmed-nc1', 'proof', 'future']:
            with self.subTest(kind=kind):
                self.prepare('nc1' if kind in ('unconfirmed-nc1', 'proof') else 'complete')
                self.output = self.root / kind
                data = copy.deepcopy(self.expected)
                if kind in ('model', 'checklist'): Path(data[kind + 'Path']).write_bytes(b'changed')
                elif kind == 'pid': data['revitPid'] = 1
                elif kind == 'oracle': data['expectedReport']['CoverageComplete'] = False
                elif kind == 'unconfirmed-nc1': data['absenceConfirmed'] = False
                elif kind == 'proof': Path(data['absenceEvidence']['path']).write_bytes(b'changed')
                else: data['preparedUtc'] = '2999-01-01T00:00:00Z'
                self.expectation.write_text(json.dumps(data), encoding='utf-8')
                ok, _ = self.invoke()
                self.assertFalse(ok)
                self.assertFalse(self.output.exists(), 'Preflight failure must not create a capture or launch the SDK.')

    def test_existing_attempt_is_never_overwritten(self):
        self.prepare(); self.output.mkdir(); sentinel = self.output / 'sentinel'
        sentinel.write_bytes(b'prior or foreign evidence')
        ok, _ = self.invoke()
        self.assertFalse(ok)
        self.assertEqual(list(self.output.iterdir()), [sentinel])
        self.assertEqual(sentinel.read_bytes(), b'prior or foreign evidence')

    def test_wrong_context_never_releases_read(self):
        for mode in ['wrong-model', 'wrong-session', 'wrong-hash', 'context-error', 'change-before-read']:
            with self.subTest(mode=mode):
                self.prepare(); self.output = self.root / mode
                ok, _ = self.invoke(mode)
                self.assertFalse(ok)
                self.assertTrue((self.output / 'prepared.json').exists())
                self.assertTrue((self.output / 'failed.json').exists())
                self.assertFalse((self.output / 'release-context').exists())
                self.assertFalse((self.output / 'capture/check.json').exists())
                self.assertFalse((self.output / 'capture/comparison-complete.json').exists())

    def test_failed_sdk_or_invalid_capture_cannot_publish_success(self):
        for mode in ['exit', 'corrupt-report', 'missing-wire', 'missing-completion',
                     'false-completion', 'change-after-read', 'stale-report']:
            with self.subTest(mode=mode):
                self.prepare(); self.output = self.root / mode
                ok, _ = self.invoke(mode)
                self.assertFalse(ok)
                self.assertTrue((self.output / 'failed.json').exists())
                self.assertTrue((self.output / 'capture/partial.json').exists())
                self.assertFalse((self.output / 'capture/comparison-complete.json').exists())

    def test_timeout_stops_owned_client_and_keeps_partial_evidence(self):
        self.prepare()
        ok, _ = self.invoke('hold', timeout=0.5)
        self.assertFalse(ok)
        self.assertTrue((self.output / 'failed.json').exists())
        self.assertTrue((self.output / 'capture/partial.json').exists())
        self.assertFalse((self.output / 'capture/comparison-complete.json').exists())
        pid = int((self.output / 'capture/fixture-pid').read_text())
        result = subprocess.run(['pwsh', '-NoProfile', '-Command',
            f'if (Get-Process -Id {pid} -ErrorAction SilentlyContinue) {{ exit 1 }}'], capture_output=True)
        self.assertEqual(result.returncode, 0, 'The runner must retire its owned SDK child before returning.')

    def test_budget_is_not_restarted_for_verification(self):
        self.prepare()
        ok, _ = self.invoke('slow-verifier', timeout=0.5)
        self.assertFalse(ok, 'An expired original budget must prevent success publication.')
        self.assertTrue((self.output / 'failed.json').exists())
        self.assertFalse((self.output / 'capture/comparison-complete.json').exists())

    def test_cleanup_error_cannot_erase_original_failure_record(self):
        self.prepare()
        ok, _ = self.invoke('cleanup-error')
        self.assertFalse(ok)
        self.assertTrue((self.output / 'failed.json').exists(), 'Failure evidence must not depend on successful cleanup.')
        failure = json.loads((self.output / 'failed.json').read_text(encoding='utf-8'))
        self.assertIn('exit 7', failure['message'])
        self.assertIn('synthetic process cleanup failure', failure['cleanupError'])

    def test_powershell_preserves_existing_attempt_with_spaces(self):
        self.prepare(); self.output.mkdir(); sentinel = self.output / 'prior.txt'; sentinel.write_bytes(b'preserve')
        script = PROJECT / 'scripts/Invoke-ReadinessMcp.ps1'
        self.assertTrue(script.exists())
        result = subprocess.run(['pwsh', '-NoProfile', '-File', str(script), '-Case', 'complete',
            '-ExpectationPath', str(self.expectation), '-EvidenceDirectory', str(self.output),
            '-RevitPid', '149972', '-ServerPath', str(self.server), '-ClientPath', sys.executable],
            capture_output=True, text=True)
        self.assertNotEqual(result.returncode, 0)
        self.assertIn('NEW evidence directory', result.stderr)
        self.assertEqual(list(self.output.iterdir()), [sentinel])
        self.assertEqual(sentinel.read_bytes(), b'preserve')

    def test_powershell_default_paths_reach_preflight(self):
        self.prepare(); self.output.mkdir()
        result = subprocess.run(['pwsh', '-NoProfile', '-File',
            str(PROJECT / 'scripts/Invoke-ReadinessMcp.ps1'), '-Case', 'complete',
            '-ExpectationPath', str(self.expectation), '-EvidenceDirectory', str(self.output),
            '-RevitPid', '149972'], capture_output=True, text=True)
        self.assertNotEqual(result.returncode, 0)
        self.assertIn('NEW evidence directory', result.stderr,
                      'Default paths must resolve in the script body before Python preflight.')
        self.assertEqual(list(self.output.iterdir()), [])


if __name__ == '__main__': unittest.main()
