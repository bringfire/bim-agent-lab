"""Hold one actual reader; interrupt three separately admitted waiting requests.

No UI automation or model edits. The production queue, budgets and guards remain
unchanged. The held reader and queued deadline call intentionally expire; explicit
cancellation and EOF calls must quiesce well before their deadlines.
"""
import argparse
import contextlib
import datetime as dt
import json
from pathlib import Path
import subprocess
import time


def utc():
    return dt.datetime.now(dt.timezone.utc).isoformat()


def run(a):
    root=Path(a.trace_directory); evidence=Path(a.evidence)
    evidence.mkdir(parents=True,exist_ok=False)
    trace=root/'events.tsv'; before=trace.read_bytes()
    assert not list(root.glob('arm-*.txt')) and not (root/'diagnostic-error.txt').exists()
    (evidence/'before.tsv').write_bytes(before)
    finish=evidence/'finish.signal'; arm=root/'arm-Reader.txt'
    record={'startedUtc':utc(),'revitPid':a.revit_pid,'requests':{},'scope':'Queued interruption diagnostic; not desktop A8 or physical disposal'}
    children=[]; blocker=None; armed=False
    def rows():
        return [line.split('\t') for line in trace.read_text().splitlines()][len(before.splitlines()):]
    def wait(predicate,seconds=8):
        until=time.monotonic()+seconds
        while time.monotonic()<until:
            if predicate(): return
            if any(p.poll() is not None for p in children): raise AssertionError('Driver exited before finish gate')
            time.sleep(.02)
        raise AssertionError('Missing expected boundary within observer limit')
    def sdk_command(name,token=None):
        command=[a.client,'--server',a.server,'--revit-pid',str(a.revit_pid),'--evidence',str(evidence/name/'sdk')]
        if token is not None: command+=['--context-token',token]
        return command
    with contextlib.ExitStack() as stack:
        try:
            (evidence/'context').mkdir()
            with (evidence/'context/stdout.txt').open('wb') as out, (evidence/'context/stderr.txt').open('wb') as err:
                subprocess.run(sdk_command('context')+['--scenario','readiness','--expected','Context'],stdout=out,stderr=err,check=True,timeout=15,creationflags=subprocess.CREATE_NO_WINDOW)
            token=json.loads((evidence/'context/sdk/context.json').read_text())['structuredContent']['contextToken']
            with arm.open('x',encoding='utf-8',newline='\n') as out: out.write('45000\nhold\n')
            armed=True
            def launch(name,command):
                folder=evidence/name; folder.mkdir()
                out=stack.enter_context((folder/'stdout.txt').open('wb'))
                err=stack.enter_context((folder/'stderr.txt').open('wb'))
                process=subprocess.Popen(command,stdout=out,stderr=err,creationflags=subprocess.CREATE_NO_WINDOW)
                children.append(process)
            launch('blocker',sdk_command('blocker',token)+['--scenario','readiness','--expected','DeadlineExceeded','--finish-file',str(finish)])
            wait(lambda:any(r[4]=='GateHeld' for r in rows()))
            gates=[r for r in rows() if r[4]=='GateHeld']; assert len(gates)==1
            blocker=gates[0][5]; record['blockerRequestId']=blocker
            (evidence/'blocker/held-deadline.txt').write_bytes((root/f'held-{blocker}-Reader.txt').read_bytes())
            for origin in ('cancel','eof','deadline'):
                previous={r[5] for r in rows() if r[4]=='Submitted'}
                command=sdk_command(origin,token)+['--finish-file',str(finish)]
                if origin=='deadline': command+=['--scenario','readiness','--expected','DeadlineExceeded']
                else: command+=['--scenario','interrupt','--interrupt-mode',origin,'--interrupt-file',str(evidence/origin/'interrupt.signal')]
                launch(origin,command)
                wait(lambda:len({r[5] for r in rows() if r[4]=='Submitted'}-previous)==1)
                request=({r[5] for r in rows() if r[4]=='Submitted'}-previous).pop()
                assert not any(r[4]=='ExecutorEntered' and r[5]==request for r in rows())
                record['requests'][origin]={'requestId':request,'admittedUtc':utc()}
                (evidence/origin/'queued.tsv').write_bytes(trace.read_bytes())
                if origin!='deadline':
                    record['requests'][origin]['interruptWrittenUtc']=utc()
                    (evidence/origin/'interrupt.signal').write_text(record['requests'][origin]['interruptWrittenUtc'])
                    wait(lambda:(evidence/origin/'sdk/interrupted.json').exists() and any(r[4]=='QuiescedObserved' and r[5]==request for r in rows()))
                    (evidence/origin/'interrupted.tsv').write_bytes(trace.read_bytes())
            # The last queued request expires after the blocker, still within its 45 s safety hold.
            wait(lambda:all((evidence/name/'sdk/response-ready.json').exists() for name in ('blocker','deadline')) and
                 all(any(r[4]=='QuiescedObserved' and r[5]==v['requestId'] for r in rows()) for v in record['requests'].values()),35)
            assert not any(r[4] in ('AutomaticRelease','GateReleased','ControlRejected') for r in rows())
            assert not any(r[4]=='ExecutorEntered' and r[5] in [v['requestId'] for v in record['requests'].values()] for r in rows())
            (evidence/'all-interrupted-while-held.tsv').write_bytes(trace.read_bytes())
            record['releaseWrittenUtc']=utc()
            (root/f'release-{blocker}-Reader.signal').write_text(record['releaseWrittenUtc'])
            wait(lambda:any(r[4]=='QuiescedObserved' and r[5]==blocker for r in rows()))
            record['finishWrittenUtc']=utc(); finish.write_text(record['finishWrittenUtc'])
            for child in children:
                child.wait(timeout=15); assert child.returncode==0
            record['captureCompleted']=True
        finally:
            if blocker:
                release=root/f'release-{blocker}-Reader.signal'
                if not release.exists(): release.write_text('observer cleanup'); record['cleanupRelease']=True
            elif armed and arm.exists(): arm.unlink()
            finish.touch(exist_ok=True)
            record['capturedUntilUtc']=utc()
            record['driverExitCodes']=[p.poll() for p in children]
            (evidence/'after.tsv').write_bytes(trace.read_bytes())
            (evidence/'observation.json').write_text(json.dumps(record,indent=2)+'\n')
    print('CAPTURE COMPLETE queued cancellation/EOF/deadline; independent comparison required')


if __name__=='__main__':
    parser=argparse.ArgumentParser()
    for name in ['trace-directory','evidence','client','server']: parser.add_argument('--'+name,required=True)
    parser.add_argument('--revit-pid',type=int,required=True)
    run(parser.parse_args())
