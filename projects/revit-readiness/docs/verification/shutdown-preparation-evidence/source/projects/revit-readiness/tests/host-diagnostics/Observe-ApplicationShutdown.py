"""Capture actual human-initiated Revit shutdown with queued work.

Diagnostic-only lifecycle holds let the observer establish admission and disposal
without requesting impossible simultaneous execution on Revit's API thread.
No model edit, UI automation or termination of Revit/desktop applications.
"""
import argparse
import contextlib
import ctypes
from ctypes import wintypes
import datetime as dt
import json
from pathlib import Path
import subprocess
import time


def utc(): return dt.datetime.now(dt.timezone.utc).isoformat()


def cleanup_arms(root, owned):
    released = False
    for stage, identity in owned:
        for prefix in ('arm-', 'used-'):
            path=root/(prefix+stage+'.txt')
            try: stat=path.stat()
            except FileNotFoundError: continue
            if (stat.st_dev,stat.st_ino)!=identity: continue
            if prefix=='arm-':
                try: path.unlink()
                except FileNotFoundError: pass
            else:
                try:
                    with (root/('release-'+stage+'.signal')).open('x') as out: out.write('observer cleanup')
                    released=True
                except FileExistsError: pass
    return released


def finish_capture(root, evidence, owned, children, record):
    """One cleanup failure must not skip the other owned resources/evidence."""
    errors=[]
    def attempt(label, action):
        try: return action()
        except Exception as error:
            errors.append(label+': '+type(error).__name__+': '+str(error))
    if attempt('controls',lambda:cleanup_arms(root,owned)): record['cleanupRelease']=True
    attempt('finish gate',lambda:(evidence/'finish.signal').touch(exist_ok=True))
    for child in children:
        def finish_child():
            try: child.wait(timeout=15)
            except subprocess.TimeoutExpired:
                record['forcedTermination']=True; record['captureCompleted']=False
                child.kill(); child.wait(timeout=5) # our SDK driver only, never Revit
        attempt('SDK child',finish_child)
    record['driverExitCodes']=[p.poll() for p in children]
    record['capturedUntilUtc']=utc()
    attempt('trace',lambda:(evidence/'after.tsv').write_bytes((root/'events.tsv').read_bytes()))
    if errors: record['captureCompleted']=False; record['cleanupErrors']=errors
    # A failed final write must be reported without replacing an existing primary error.
    attempt('observation',lambda:(evidence/'observation.json').write_text(json.dumps(record,indent=2)+'\n',encoding='utf-8'))
    return errors


@contextlib.contextmanager
def process_observer(pid):
    kernel=ctypes.WinDLL('kernel32', use_last_error=True)
    kernel.OpenProcess.argtypes=[wintypes.DWORD,wintypes.BOOL,wintypes.DWORD]; kernel.OpenProcess.restype=wintypes.HANDLE
    kernel.WaitForSingleObject.argtypes=[wintypes.HANDLE,wintypes.DWORD]; kernel.WaitForSingleObject.restype=wintypes.DWORD
    kernel.CloseHandle.argtypes=[wintypes.HANDLE]; kernel.CloseHandle.restype=wintypes.BOOL
    handle=kernel.OpenProcess(0x00100000,False,pid) # SYNCHRONIZE; no terminate permission
    if not handle: raise ctypes.WinError(ctypes.get_last_error())
    def exited():
        status=kernel.WaitForSingleObject(handle,0)
        if status==0xffffffff: raise ctypes.WinError(ctypes.get_last_error())
        assert status in (0,258)
        return status==0
    try:
        assert not exited(), 'Revit already exited'
        yield exited
    finally: kernel.CloseHandle(handle)


def run(a):
    root, evidence, descriptor=Path(a.trace_directory),Path(a.evidence),Path(a.descriptor)
    assert int(root.name)==a.revit_pid
    session=json.loads(descriptor.read_text(encoding='utf-8-sig'))
    assert session['RevitPid']==a.revit_pid
    assert not list(root.glob('arm-*.txt')) and not (root/'diagnostic-error.txt').exists()
    evidence.mkdir(parents=True,exist_ok=False)
    trace=root/'events.tsv'; before=trace.read_bytes(); (evidence/'before.tsv').write_bytes(before)
    (evidence/'session.json').write_bytes(descriptor.read_bytes())
    record={'startedUtc':utc(),'revitPid':a.revit_pid,'queued':{},'captureCompleted':False,'forcedTermination':False}
    owned=[]; children=[]; finish=evidence/'finish.signal'
    def rows():
        text=trace.read_bytes(); assert text.startswith(before)
        return [s.split('\t') for s in text.decode('utf-8').splitlines()[len(before.splitlines()):]] if text.endswith(b'\n') else []
    def wait(predicate,seconds=8,allow_exit=False):
        until=time.monotonic()+seconds
        while time.monotonic()<until:
            if predicate(): return
            if not allow_exit and any(p.poll() is not None for p in children): raise AssertionError('SDK child exited before finish gate')
            assert not (root/'diagnostic-error.txt').exists(), 'Diagnostic recorder failed'
            time.sleep(.02)
        raise AssertionError('Missing boundary before observer timeout')
    def cmd(name,token=None):
        args=[a.client,'--server',a.server,'--scenario','readiness','--revit-pid',str(a.revit_pid),'--evidence',str(evidence/name/'sdk')]
        return args+(['--context-token',token] if token else [])
    with contextlib.ExitStack() as stack:
        try:
            exited=stack.enter_context(process_observer(a.revit_pid))
            (evidence/'context').mkdir()
            with (evidence/'context/stdout.txt').open('wb') as out,(evidence/'context/stderr.txt').open('wb') as err:
                subprocess.run(cmd('context')+['--expected','Context'],stdout=out,stderr=err,timeout=15,check=True,creationflags=subprocess.CREATE_NO_WINDOW)
            context=json.loads((evidence/'context/sdk/context.json').read_text(encoding='utf-8'))['structuredContent']
            assert context['revitPid']==a.revit_pid and context['bridgeSessionId']==session['BridgeSessionId']
            wait(lambda:any(r[4]=='Pipe.LifetimeDisposed' and r[5]==context['requestId'] for r in rows()))
            # Baseline excludes the context call from the shutdown-only trace slice.
            before=trace.read_bytes(); assert before.endswith(b'\n')
            (evidence/'before.tsv').write_bytes(before)
            for stage in ('ApplicationClosing','ShutdownAfterStop'):
                arm=root/('arm-'+stage+'.txt')
                assert not any((root/(prefix+stage+suffix)).exists() for prefix,suffix in [('used-','.txt'),('release-','.signal'),('held-','.txt')])
                with arm.open('x',encoding='utf-8') as out:
                    st=arm.stat(); owned.append((stage,(st.st_dev,st.st_ino))); out.write('45000\n')
            (evidence/'ready.json').write_text(json.dumps({'readyUtc':utc(),'action':'Close Revit normally without saving disposable changes','contextExpiresUtc':context['expiresUtc']}),encoding='utf-8')
            wait(lambda:any(r[4]=='LifecycleHeld' and r[8]=='ApplicationClosing' for r in rows()),seconds=180)
            assert dt.datetime.now(dt.timezone.utc)<dt.datetime.fromisoformat(context['expiresUtc'].replace('Z','+00:00'))
            for name in ('queued-1','queued-2','queued-3'):
                previous={r[5] for r in rows() if r[4]=='Submitted'}
                folder=evidence/name; folder.mkdir()
                out=stack.enter_context((folder/'stdout.txt').open('wb')); err=stack.enter_context((folder/'stderr.txt').open('wb'))
                children.append(subprocess.Popen(cmd(name,context['contextToken'])+['--expected','BridgeUnavailable','--finish-file',str(finish)],stdout=out,stderr=err,creationflags=subprocess.CREATE_NO_WINDOW))
                wait(lambda:len({r[5] for r in rows() if r[4]=='Submitted'}-previous)==1)
                rid=({r[5] for r in rows() if r[4]=='Submitted'}-previous).pop(); record['queued'][name]=rid
                assert not any(r[4] in ('ExecutorEntered','QuiescedObserved','ResponseCompleted') and r[5] in record['queued'].values() for r in rows())
            (evidence/'all-queued.tsv').write_bytes(trace.read_bytes())
            (root/'release-ApplicationClosing.signal').write_text(utc(),encoding='utf-8')
            wait(lambda:any(r[4]=='LifecycleHeld' and r[8]=='ShutdownAfterStop' for r in rows()))
            wait(lambda:any(r[4]=='NetworkStopped' and r[8]=='RanToCompletion' for r in rows()) and
                 all(any(r[4]=='Pipe.LifetimeDisposed' and r[5]==rid for r in rows()) for rid in record['queued'].values()) and
                 all((evidence/name/'sdk/response-ready.json').exists() for name in record['queued']))
            assert not exited() and not descriptor.exists()
            (evidence/'retired-before-exit.tsv').write_bytes(trace.read_bytes())
            record['retiredWhileProcessAlive']=True
            (root/'release-ShutdownAfterStop.signal').write_text(utc(),encoding='utf-8')
            wait(exited,seconds=20)
            record['processExited']=True; record['descriptorAbsent']=not descriptor.exists()
            finish.write_text(utc(),encoding='utf-8')
            for child in children: child.wait(timeout=15); assert child.returncode==0
            record['driverExitCodes']=[p.returncode for p in children]
            assert not any(r[4] in ('LifecycleAutomaticRelease','ControlRejected','ExecutorEntered','ReaderEntered') for r in rows())
            record['captureCompleted']=True
        except BaseException as error:
            record['error']=type(error).__name__+': '+str(error)
            record['captureCompleted']=False
            raise
        finally:
            errors=finish_capture(root,evidence,owned,children,record)
            if errors: print('CAPTURE CLEANUP ERRORS: '+repr(errors))
    assert record['captureCompleted'] and not errors, 'Capture or cleanup failed'
    print('CAPTURE COMPLETE; run independent application-shutdown verifier')


if __name__=='__main__':
    parser=argparse.ArgumentParser()
    for name in ('trace-directory','evidence','client','server','descriptor'): parser.add_argument('--'+name,required=True)
    parser.add_argument('--revit-pid',type=int,required=True)
    run(parser.parse_args())
