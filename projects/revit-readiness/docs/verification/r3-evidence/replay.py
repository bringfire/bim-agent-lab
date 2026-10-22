"""Read-only replay of retained R3 evidence; no host launch or installation.

Run from any directory: python <this file>. Original manifest failures fail closed.
This verifies supplied captures, not a fresh Revit/desktop execution.
"""
import hashlib
import json
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[5]
EVIDENCE = ROOT / 'projects/revit-readiness/docs/verification'
TESTS = ROOT / 'projects/revit-readiness/tests'
sys.path.insert(0, str(TESTS))
from verify_mcp_evidence import verify, verify_stale
from verify_desktop_continuity import check_desktop_continuity


def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def main():
    manifests = [EVIDENCE / p for p in (
        'desktop-launch-evidence/hashes.json',
        'desktop-readiness-evidence/2026-10-22/hashes.json',
        'shared-installation-evidence/hashes.json',
        'shutdown-preparation-evidence/hashes.json',
        'shared-recovery-evidence/preparation/hashes.json',
        'shared-recovery-evidence/execution/hashes.json',
        'shared-recovery-evidence/live/hashes.json',
        'host-deadline-evidence/hashes.json',
        'host-diagnostic-evidence/hashes.json',
        'host-diagnostic-startup-evidence/hashes.json',
        'host-interruption-evidence/hashes.json',
        'host-stop-recovery-evidence/hashes.json',
        'live-installation-evidence/hashes.json',
        'mcp-client-compatibility-evidence/compatible-desktops/manifest.json',
        'sdk-cancellation-evidence/hashes.json',
        'readiness-runner-evidence/2026-10-22/manifest.json',
    )] + sorted((EVIDENCE / 'mcp-readiness-evidence').glob('*/evidence-manifest.json'))
    hashes = []
    for manifest in manifests:
        data = read(manifest)
        entries = data.get('files', data) if isinstance(data, dict) else data
        if isinstance(entries, dict):
            entries = [{'path': name, 'sha256': digest} for name, digest in entries.items()]
        for entry in entries:
            target = (manifest.parent / entry['path']).resolve()
            assert target.is_relative_to(manifest.parent.resolve()), str(target)
            content = target.read_bytes()
            assert hashlib.sha256(content).hexdigest() == entry['sha256'].lower(), str(target)
            if 'bytes' in entry:
                assert len(content) == entry['bytes'], str(target)
        hashes.append({'manifest': manifest.relative_to(ROOT).as_posix(), 'verified': len(entries)})
    comparisons = []
    for marker in sorted((EVIDENCE / 'mcp-readiness-evidence').rglob('comparison-complete.json')):
        if marker.parent.name == 'recovery':
            continue  # Replay each recovery through its enclosing switch assertion.
        saved = read(marker)
        saved = saved.get('result', saved)
        result = verify(marker.parent, saved['case']) if 'case' in saved else verify_stale(marker.parent)
        comparisons.append({'directory': marker.parent.relative_to(EVIDENCE).as_posix(), 'result': result})
    recovered = verify(EVIDENCE / 'shared-recovery-evidence/live/wrapper/capture', 'complete')
    continuity = []
    for snapshot in sorted((EVIDENCE / 'desktop-readiness-evidence/2026-10-22').rglob('snapshot.json')):
        continuity.append({'directory': snapshot.parent.relative_to(EVIDENCE).as_posix(),
                           'result': check_desktop_continuity(snapshot.parent)})
    commands = []
    for script, directory in (
        ('verify_live_lifecycle_capture.py', 'live-installation-evidence'),
        ('verify_host_deadline_capture.py', 'host-deadline-evidence'),
        ('verify_host_interruption_capture.py', 'host-interruption-evidence'),
        ('verify_host_stop_recovery.py', 'host-stop-recovery-evidence'),
        ('verify_application_shutdown.py', 'shutdown-preparation-evidence/live/application-close-01'),
        ('verify_sdk_cancellation_diagnosis.py', 'sdk-cancellation-evidence/characterized-final'),
    ):
        run = subprocess.run([sys.executable, str(TESTS / script), str(EVIDENCE / directory)],
                             capture_output=True, text=True, encoding='utf-8', errors='replace')
        assert run.returncode == 0, f'{script}: {run.stdout}\n{run.stderr}'
        commands.append({'script': script, 'directory': directory, 'exitCode': run.returncode,
                         'stdout': run.stdout, 'stderr': run.stderr})
    print(json.dumps({'scope': 'Retained evidence replay, not new host execution',
                      'manifests': hashes, 'hashEntriesVerified': sum(x['verified'] for x in hashes),
                      'comparisons': comparisons, 'postReinstallWrapper': recovered,
                      'desktopContinuity': continuity, 'checkers': commands, 'pass': True}, indent=2))


if __name__ == '__main__':
    main()
