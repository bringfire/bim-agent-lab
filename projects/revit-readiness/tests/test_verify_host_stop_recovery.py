import json
from pathlib import Path
import shutil
import tempfile
import unittest
from verify_host_stop_recovery import check_restart, check_queued_stop, check_restoration
from test_verify_host_interruption_capture import json_change, trace_change, wire_change

ROOT = Path(__file__).resolve().parents[1]/'docs/verification/host-stop-recovery-evidence'


class RecoveryTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix='bal-stop-recovery-')
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)/'capture'
        shutil.copytree(ROOT, self.root)

    def test_original(self):
        self.assertTrue(check_restart(self.root)['pass'])
        self.assertTrue(check_queued_stop(self.root)['pass'])
        self.assertTrue(check_restoration(self.root)['pass'])


def change_report_consistently(root, key, value):
    def edit(result):
        result['structuredContent']['report'][key] = value
        result['content'][0]['text'] = json.dumps(result['structuredContent'])
    json_change(root/'restart/sdk/check.json', edit)
    json_change(root/'restart/sdk/sdk-client-result.json', lambda d:edit(d['check']))
    wire_change(root/'restart/sdk/wire/server-to-client.jsonl',
                lambda messages:edit(next(m['result'] for m in messages if 'report' in m.get('result',{}).get('structuredContent',{}))))


CONTROLS = {
    'same_session': lambda p: json_change(p/'restart/session.json', lambda x:x.update(BridgeSessionId='366f623ba0754e7a9cb2d1dd2d90d7ce')),
    'wrong_pid': lambda p: json_change(p/'restart/session.json', lambda x:x.update(RevitPid=123)),
    'no_new_hook': lambda p: trace_change(p/'restart/before.tsv','EventsHooked','Other'),
    'no_new_reader': lambda p: trace_change(p/'restart/after.tsv','ReaderEntered','Other'),
    'no_new_quiescence': lambda p: trace_change(p/'restart/after.tsv','QuiescedObserved','Other'),
    'no_completed_capture': lambda p: json_change(p/'restart/sdk/complete.json', lambda x:x.update(**{'pass':False})),
    'changed_report_consistent_with_wire': lambda p: change_report_consistently(p, 'RequestedInstances', 999),
    'modified_model_consistent_with_wire': lambda p: change_report_consistently(p, 'ModifiedAfter', True),
}


def negative(fn):
    def test(self):
        fn(self.root)
        with self.assertRaises((AssertionError, KeyError, ValueError)):
            check_restart(self.root)
    return test


for name, fn in CONTROLS.items():
    setattr(RecoveryTests, 'test_reject_'+name, negative(fn))


STOP_CONTROLS = {
    'queued_entered_executor': lambda p:trace_change(p/'queued-stop/after.tsv','Submitted','ExecutorEntered'),
    'queued_read': lambda p:trace_change(p/'queued-stop/after.tsv','Submitted','ReaderEntered'),
    'queued_identity': lambda p:json_change(p/'queued-stop/observation.json', lambda d:d['queued'].update({'queued-1':'0'*32})),
    'not_completed': lambda p:json_change(p/'queued-stop/observation.json', lambda d:d.update(captureCompleted=False)),
    'cleanup_release': lambda p:json_change(p/'queued-stop/observation.json', lambda d:d.update(cleanupRelease=True)),
    'driver_failed': lambda p:json_change(p/'queued-stop/observation.json', lambda d:d.update(driverExitCodes=[0,0,1,0])),
    'network_missing_while_held': lambda p:trace_change(p/'queued-stop/stopped-while-reader-held.tsv','NetworkStopped','Other'),
    'event_not_disposed': lambda p:trace_change(p/'queued-stop/after.tsv','EventDisposed','Other'),
    'hooks_never_removed': lambda p:trace_change(p/'queued-stop/after.tsv','EventsUnhooked','Other'),
    'automatic_release': lambda p:trace_change(p/'queued-stop/after.tsv','GateReleased','AutomaticRelease'),
    'reader_not_finished': lambda p:trace_change(p/'queued-stop/after.tsv','ReaderExited','Other'),
    'short_window': lambda p:json_change(p/'queued-stop/queued-1/sdk/post-release-observation.json',lambda d:d.update(elapsedMilliseconds=2)),
    'descriptor_retained': lambda p:json_change(p/'after-stop-state.json',lambda d:d.update(descriptorCount=1)),
    'revit_exited': lambda p:json_change(p/'after-stop-state.json',lambda d:d.update(revitAlive=False)),
    'deadline_changed': lambda p:(p/'queued-stop/Reader-deadline.txt').write_text('2026-10-22T00:00:00Z'),
}


def stop_negative(fn):
    def test(self):
        fn(self.root)
        with self.assertRaises((AssertionError, KeyError, ValueError)):
            check_queued_stop(self.root)
    return test


for name, fn in STOP_CONTROLS.items():
    setattr(RecoveryTests, 'test_stop_reject_'+name, stop_negative(fn))


RESTORE_CONTROLS = {
    'process_alive': lambda p:json_change(p/'after-close-state.json', lambda d:d.update(revitProcessCount=1)),
    'descriptor_retained': lambda p:json_change(p/'after-close-state.json', lambda d:d.update(descriptorCount=1)),
    'shutdown_never_returned': lambda p:trace_change(p/'shutdown.tsv','ApplicationShutdownReturned','Other'),
    'normal_target_changed': lambda p:json_change(p/'after-normal-restoration.json', lambda d:d['targets'][0].update(sha256='0'*64)),
    'intermediate_changed': lambda p:json_change(p/'after-first-undo.json', lambda d:d['targets'][0].update(sha256='0'*64)),
    'model_changed': lambda p:json_change(p/'closed-files.json', lambda d:d['sampleChecks'][0].update(sha256='0'*64)),
    'recovery_not_restored': lambda p:json_change(p/'after-normal-restoration.json', lambda d:d['recoveryStates'][0]['state'].update(State='Restoring')),
}


def restore_negative(fn):
    def test(self):
        fn(self.root)
        with self.assertRaises((AssertionError,KeyError,ValueError)):
            check_restoration(self.root)
    return test


for name, fn in RESTORE_CONTROLS.items():
    setattr(RecoveryTests, 'test_restore_reject_'+name, restore_negative(fn))


if __name__=='__main__':
    unittest.main()
