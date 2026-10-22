"""Original-capture and corrupted-evidence controls; no running Revit needed."""
import json
from pathlib import Path
import shutil
import tempfile
import unittest
from verify_host_interruption_capture import check_run

ROOT = Path(__file__).resolve().parents[1] / 'docs/verification/host-interruption-evidence'


class InterruptionEvidenceTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix='bal-interrupt-check-')
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name) / 'capture'
        shutil.copytree(ROOT, self.root)

    def test_original(self):
        self.assertTrue(check_run(self.root)['pass'])


def json_change(path, fn):
    data = json.loads(path.read_text(encoding='utf-8-sig'))
    fn(data)
    path.write_text(json.dumps(data), encoding='utf-8')


def wire_change(path, fn):
    data = [json.loads(line) for line in path.read_text().splitlines()]
    fn(data)
    path.write_text(''.join(json.dumps(row)+'\n' for row in data))


def trace_change(path, old, new):
    data = [line.split('\t') for line in path.read_text().splitlines()]
    row = next(row for row in reversed(data) if row[4] == old)
    row[4] = new
    path.write_text(''.join('\t'.join(row)+'\n' for row in data))


CONTROLS = {
    'wrong_origin': lambda p: json_change(p/'cancel-Preparation/observation.json', lambda x:x.update(origin='eof')),
    'wrong_host': lambda p: json_change(p/'cancel-Preparation/observation.json', lambda x:x.update(revitPid=123)),
    'wrong_request': lambda p: json_change(p/'cancel-Preparation/observation.json', lambda x:x.update(requestId='0'*32)),
    'missing_cancel': lambda p: wire_change(p/'cancel-Preparation/sdk/wire/client-to-server.jsonl', lambda x:x.__setitem__(slice(None),[r for r in x if r['method'] != 'notifications/cancelled'])),
    'wrong_cancel_identity': lambda p: wire_change(p/'cancel-Preparation/sdk/wire/client-to-server.jsonl', lambda x:next(r for r in x if r['method']=='notifications/cancelled')['params'].update(requestId=3)),
    'eof_server_alive': lambda p: json_change(p/'eof-Reader/sdk/interrupted.json', lambda x:x.update(serverExited=False)),
    'cancel_server_exited': lambda p: json_change(p/'cancel-Reader/sdk/interrupted.json', lambda x:x.update(serverExited=True)),
    'forced_termination': lambda p: json_change(p/'eof-Preparation/sdk/server-exit.json', lambda x:x.update(forcedTermination=True)),
    'expired_instead': lambda p: json_change(p/'cancel-Preparation/observation.json', lambda x:x.update(originalDeadlineUtc='2026-10-22T00:00:00Z')),
    'missing_quiescence': lambda p: trace_change(p/'cancel-Preparation/after.tsv','QuiescedObserved','Other'),
    'reader_after_canceled_preparation': lambda p: trace_change(p/'cancel-Preparation/after.tsv','ExecutorExited','ReaderEntered'),
    'missing_reader_exit': lambda p: trace_change(p/'eof-Reader/after.tsv','ReaderExited','Other'),
    'automatic_release': lambda p: trace_change(p/'cancel-Reader/after.tsv','GateReleased','AutomaticRelease'),
    'short_observation': lambda p: json_change(p/'cancel-Reader/sdk/post-release-observation.json', lambda x:x.update(elapsedMilliseconds=2)),
    'incomplete_capture': lambda p: json_change(p/'eof-Reader/sdk/complete.json', lambda x:x.update(pass_=False, **{'pass':False})),
    'unexpected_late_response': lambda p: wire_change(p/'cancel-Reader/sdk/wire/server-to-client.jsonl', lambda x:x.append({'jsonrpc':'2.0','id':'unexpected','result':{'isError':False,'report':{}}})),
    'queued_executor_entered': lambda p: trace_change(p/'queued/after.tsv','Submitted','ExecutorEntered'),
    'queued_quiescence_missing': lambda p: trace_change(p/'queued/all-interrupted-while-held.tsv','QuiescedObserved','Other'),
    'queued_wrong_identity': lambda p: json_change(p/'queued/observation.json', lambda x:x['requests']['eof'].update(requestId='0'*32)),
    'queued_blocker_released_early': lambda p: json_change(p/'queued/observation.json', lambda x:x.update(releaseWrittenUtc='2026-10-22T00:00:00Z')),
    'queued_eof_used_cancel': lambda p: wire_change(p/'queued/eof/sdk/wire/client-to-server.jsonl', lambda x:x.append({'jsonrpc':'2.0','method':'notifications/cancelled','params':{'requestId':x[-1]['id']}})),
    'queued_failed_driver': lambda p: json_change(p/'queued/observation.json', lambda x:x.update(driverExitCodes=[0,0,1,0])),
    'queued_short_connected_window': lambda p: json_change(p/'queued/deadline/sdk/post-release-observation.json', lambda x:x.update(elapsedMilliseconds=2)),
    'stop_missing_network_completion': lambda p: trace_change(p/'executing-stop/held.tsv','NetworkStopped','Other'),
    'stop_event_not_disposed': lambda p: trace_change(p/'executing-stop/after.tsv','EventDisposed','Other'),
    'stop_hooks_retained': lambda p: trace_change(p/'executing-stop/after.tsv','EventsUnhooked','Other'),
    'stop_never_returned': lambda p: trace_change(p/'executing-stop/after.tsv','StopReturned','Other'),
    'stop_descriptor_retained': lambda p: json_change(p/'after-stop-state.json', lambda x:x.update(descriptorCount=1)),
    'stop_killed_revit': lambda p: json_change(p/'after-stop-state.json', lambda x:x.update(revitAlive=False)),
    'stop_automatic_release': lambda p: trace_change(p/'executing-stop/after.tsv','GateReleased','AutomaticRelease'),
}


def negative(fn):
    def test(self):
        fn(self.root)
        with self.assertRaises((AssertionError, KeyError, ValueError)):
            check_run(self.root)
    return test


for name, fn in CONTROLS.items():
    setattr(InterruptionEvidenceTests, 'test_reject_' + name, negative(fn))


if __name__ == '__main__':
    unittest.main()
