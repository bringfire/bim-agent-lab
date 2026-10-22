"""Observe native Stop with three queued requests using existing diagnostic arms.

No UI automation or installed-code change. Preparation holds admission open;
the same request subsequently enters Reader/Stop in valid Revit API context.
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


def cleanup_arms(root, owned):
    released = False
    def same(path, identity):
        try:
            stat = path.stat()
            return (stat.st_dev, stat.st_ino) == identity
        except FileNotFoundError:
            return False
    for stage, arm, identity in owned:
        if same(arm, identity):
            try:
                arm.unlink()
            except FileNotFoundError:
                pass  # Revit may have moved the arm to its consumed name.
        for used in root.glob(f'used-*-{stage}.txt'):
            if not same(used, identity):
                continue
            request = used.name[len('used-'):-len(f'-{stage}.txt')]
            release = root/f'release-{request}-{stage}.signal'
            try:
                with release.open('x') as out:
                    out.write('observer cleanup')
                released = True
            except FileExistsError:
                pass
    return released


def run(a):
    root, evidence = Path(a.trace_directory), Path(a.evidence)
    evidence.mkdir(parents=True, exist_ok=False)
    trace = root/'events.tsv'
    before = trace.read_bytes()
    assert not list(root.glob('arm-*.txt')) and not (root/'diagnostic-error.txt').exists()
    (evidence/'before.tsv').write_bytes(before)
    finish = evidence/'finish.signal'
    record = {'startedUtc': utc(), 'revitPid': a.revit_pid, 'queued': {}}
    children, owned_arms = [], []
    blocker = None

    def rows():
        return [line.split('\t') for line in trace.read_text().splitlines()][len(before.splitlines()):]

    def wait(predicate, seconds=8):
        until = time.monotonic()+seconds
        while time.monotonic() < until:
            if predicate():
                return
            if any(p.poll() is not None for p in children):
                raise AssertionError('SDK driver exited before the finish gate')
            time.sleep(.02)
        raise AssertionError('Missing boundary within observer limit')

    def command(name, token=None):
        value = [a.client, '--server', a.server, '--revit-pid', str(a.revit_pid),
                 '--scenario', 'readiness', '--evidence', str(evidence/name/'sdk')]
        return value + (['--context-token', token] if token is not None else [])

    with contextlib.ExitStack() as stack:
        try:
            (evidence/'context').mkdir()
            with (evidence/'context/stdout.txt').open('wb') as out, (evidence/'context/stderr.txt').open('wb') as err:
                subprocess.run(command('context')+['--expected', 'Context'], stdout=out, stderr=err,
                               check=True, timeout=15, creationflags=subprocess.CREATE_NO_WINDOW)
            token = json.loads((evidence/'context/sdk/context.json').read_text())['structuredContent']['contextToken']
            for stage, mode in [('Preparation', 'hold'), ('Reader', 'stop')]:
                arm = root/f'arm-{stage}.txt'
                with arm.open('x', encoding='utf-8', newline='\n') as out:
                    identity = arm.stat()
                    owned_arms.append((stage, arm, (identity.st_dev, identity.st_ino)))
                    out.write(f'45000\n{mode}\n')

            def launch(name):
                folder = evidence/name
                folder.mkdir()
                out = stack.enter_context((folder/'stdout.txt').open('wb'))
                err = stack.enter_context((folder/'stderr.txt').open('wb'))
                children.append(subprocess.Popen(command(name, token)+['--expected', 'BridgeUnavailable',
                    '--finish-file', str(finish)], stdout=out, stderr=err, creationflags=subprocess.CREATE_NO_WINDOW))

            launch('blocker')
            wait(lambda: any(r[4]=='GateHeld' and r[8]=='Preparation' for r in rows()))
            blocker = next(r[5] for r in rows() if r[4]=='GateHeld')
            record['blockerRequestId'] = blocker
            (evidence/'preparation-held.tsv').write_bytes(trace.read_bytes())
            for name in ('queued-1', 'queued-2', 'queued-3'):
                previous = {r[5] for r in rows() if r[4]=='Submitted'}
                launch(name)
                wait(lambda: len({r[5] for r in rows() if r[4]=='Submitted'}-previous)==1)
                request = ({r[5] for r in rows() if r[4]=='Submitted'}-previous).pop()
                assert not any(r[4]=='ExecutorEntered' and r[5]==request for r in rows())
                record['queued'][name] = request
            (evidence/'all-queued.tsv').write_bytes(trace.read_bytes())
            record['preparationReleaseUtc'] = utc()
            (root/f'release-{blocker}-Preparation.signal').write_text(record['preparationReleaseUtc'])
            wait(lambda: any(r[4]=='GateHeld' and r[8]=='Reader' for r in rows()))
            wait(lambda: all(any(r[4]==stage for r in rows()) for stage in
                            ('NetworkStopped', 'EventDisposed', 'StopReturned', 'InactiveObserved'))
                 and all(any(r[4]=='QuiescedObserved' and r[5]==request for r in rows()) for request in record['queued'].values())
                 and all((evidence/name/'sdk/response-ready.json').exists() for name in ('blocker', *record['queued'])))
            assert not any(r[4] in ('AutomaticRelease', 'ControlRejected', 'ReaderExited') for r in rows())
            assert not any(r[4]=='ExecutorEntered' and r[5] in record['queued'].values() for r in rows())
            (evidence/'stopped-while-reader-held.tsv').write_bytes(trace.read_bytes())
            for stage, _, _ in owned_arms:
                (evidence/f'{stage}-arm.txt').write_bytes((root/f'used-{blocker}-{stage}.txt').read_bytes())
                (evidence/f'{stage}-deadline.txt').write_bytes((root/f'held-{blocker}-{stage}.txt').read_bytes())
            record['readerReleaseUtc'] = utc()
            (root/f'release-{blocker}-Reader.signal').write_text(record['readerReleaseUtc'])
            # Stop polls the network task without waiting. If it was still pending,
            # the existing Idling hook completes retirement after API execution resumes.
            wait(lambda: any(r[4]=='QuiescedObserved' and r[5]==blocker for r in rows())
                 and any(r[4]=='EventsUnhooked' for r in rows()))
            record['finishWrittenUtc'] = utc()
            finish.write_text(record['finishWrittenUtc'])
            for child in children:
                child.wait(timeout=15)
                assert child.returncode == 0
            record['captureCompleted'] = True
        finally:
            # Do not remove an arm once consumed: a subsequently created arm is not ours.
            # Release any claimed stage, including a claim made just as the observer failed.
            if cleanup_arms(root, owned_arms):
                record['cleanupRelease'] = True
            finish.touch(exist_ok=True)
            record['driverExitCodes'] = [p.poll() for p in children]
            record['capturedUntilUtc'] = utc()
            (evidence/'after.tsv').write_bytes(trace.read_bytes())
            (evidence/'observation.json').write_text(json.dumps(record, indent=2)+'\n')
    print('CAPTURE COMPLETE queued Stop; independent comparison required')


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    for name in ('trace-directory', 'evidence', 'client', 'server'):
        parser.add_argument('--'+name, required=True)
    parser.add_argument('--revit-pid', type=int, required=True)
    run(parser.parse_args())
