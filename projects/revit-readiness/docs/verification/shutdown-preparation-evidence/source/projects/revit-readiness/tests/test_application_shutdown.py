import copy
import unittest
from verify_application_shutdown import check_trace


def fixture():
    info = dict(captureCompleted=True, cleanupRelease=False, forcedTermination=False,
                processExited=True, descriptorAbsent=True, retiredWhileProcessAlive=True, driverExitCodes=[0, 0, 0],
                queued={'queued-1':'r1', 'queued-2':'r2', 'queued-3':'r3'})
    rows = []
    def event(stage, request='', detail='', thread='1'):
        rows.append([str(len(rows)+1), 'synthetic', str(len(rows)+1), thread, stage, request, '', '', detail])
    event('ApplicationClosingEntered'); event('LifecycleHeld', detail='ApplicationClosing')
    for rid in info['queued'].values():
        event('Submitted', rid, thread='2'); event('NativeRaiseBegin', thread='2'); event('NativeRaiseEnd', detail='Accepted', thread='2')
    event('LifecycleReleased', detail='ApplicationClosing'); event('ApplicationClosingReturned')
    event('ApplicationShutdownEntered'); event('StopEntered'); event('EventDisposed'); event('StopReturned'); event('EventsUnhooked')
    event('LifecycleHeld', detail='ShutdownAfterStop'); event('NetworkStopped', detail='RanToCompletion', thread='2')
    for rid in info['queued'].values():
        event('QuiescedObserved', rid, detail='RanToCompletion', thread='2')
        event('Pipe.LifetimeDisposed', rid, detail='1', thread='2')
    event('LifecycleReleased', detail='ShutdownAfterStop'); event('ApplicationShutdownReturned')
    return info, rows


class ShutdownTraceTests(unittest.TestCase):
    def test_synthetic_positive(self):
        self.assertTrue(check_trace(*fixture())['pass'])

    def test_missing_or_bad_terminal_facts(self):
        for field, bad in [('captureCompleted', False), ('cleanupRelease', True), ('forcedTermination', True),
                           ('processExited', False), ('descriptorAbsent', False), ('retiredWhileProcessAlive',False), ('driverExitCodes', [0,1,0]),
                           ('cleanupErrors',['failed']), ('error','primary failure')]:
            with self.subTest(field=field):
                info, rows = fixture(); info[field] = bad
                with self.assertRaises(AssertionError): check_trace(info, rows)

    def test_missing_events(self):
        info, rows = fixture()
        for n, row in enumerate(rows):
            if row[4] in ('NativeRaiseBegin','NativeRaiseEnd'): continue
            with self.subTest(stage=row[4], request=row[5]):
                altered = copy.deepcopy(rows); del altered[n]
                with self.assertRaises(AssertionError): check_trace(info, altered)

    def test_false_execution_or_failure(self):
        for stage in ('ExecutorEntered','ReaderEntered','ControlRejected','LifecycleAutomaticRelease','Pipe.LifetimeCleanupFailed'):
            info, rows = fixture(); extra=copy.deepcopy(rows[2]); extra[4]=stage; rows.insert(3, extra)
            with self.subTest(stage=stage), self.assertRaises(AssertionError): check_trace(info, rows)

    def test_late_admission_disposal_and_crossed_identity(self):
        for mode in ('admission','disposal','identity','thread','duplicate'):
            info, rows = fixture()
            if mode=='admission': rows.append(rows.pop(2))
            if mode=='disposal': rows.append(rows.pop(next(i for i,r in enumerate(rows) if r[4]=='Pipe.LifetimeDisposed')))
            if mode=='identity': next(r for r in rows if r[4]=='Pipe.LifetimeDisposed')[5]='foreign'
            if mode=='thread': next(r for r in rows if r[4]=='EventDisposed')[3]='2'
            if mode=='duplicate': rows.insert(3, copy.deepcopy(rows[2]))
            with self.subTest(mode=mode), self.assertRaises(AssertionError): check_trace(info, rows)


if __name__ == '__main__': unittest.main()
