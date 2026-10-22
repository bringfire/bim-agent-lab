"""Compare actual session recovery and queued Stop, not desktop acceptance."""
import argparse
import json
from pathlib import Path
from verify_live_lifecycle_capture import check_capture, read, utc
from verify_host_deadline_capture import trace

PREVIOUS = Path(__file__).resolve().parents[1]/'docs/verification/host-interruption-evidence'


def check_restart(root):
    root = Path(root)
    folder = root/'restart'
    session, old = read(folder/'session.json'), read(PREVIOUS/'session.json')
    assert session['BridgeSessionId'] != old['BridgeSessionId']
    assert session['PipeName'] != old['PipeName']
    assert session['RevitPid'] == old['RevitPid']
    assert session['ProcessStartUtc'] == old['ProcessStartUtc']
    result = check_capture(folder/'sdk', 'ReadinessReport')
    context = read(folder/'sdk/context.json')['structuredContent']
    assert result['bridgeSessionId'] == context['bridgeSessionId'] == session['BridgeSessionId']
    assert context['revitPid'] == session['RevitPid']
    baseline = check_capture(PREVIOUS/'baseline', 'ReadinessReport')
    assert result['checklistSha256'] == baseline['checklistSha256']
    assert {k:v for k,v in result['report'].items() if k!='GeneratedUtc'} == {k:v for k,v in baseline['report'].items() if k!='GeneratedUtc'}
    assert result['report']['ModifiedBefore'] is False and result['report']['ModifiedAfter'] is False
    prior, before, after = [trace(p) for p in [PREVIOUS/'executing-stop/after.tsv', folder/'before.tsv', folder/'after.tsv']]
    assert before[:len(prior)] == prior and after[:len(before)] == before
    restart = before[len(prior):]
    assert len(restart)==1 and restart[0][4]=='EventsHooked' and restart[0][3]==prior[0][3]
    fresh = after[len(before):]
    assert [r[5] for r in fresh if r[4]=='Submitted'] == [context['requestId'], result['requestId']]
    own = [r for r in fresh if r[5]==result['requestId']]
    for stage in ('ExecutorEntered', 'ReaderEntered', 'ReaderExited', 'ExecutorExited', 'QuiescedObserved'):
        assert sum(r[4]==stage for r in own)==1, stage
    assert not any(r[4] in ('GateHeld', 'AutomaticRelease', 'ControlRejected') for r in fresh)
    return {'pass': True, 'bridgeSessionId': session['BridgeSessionId'], 'reportMatchesBaseline': True,
            'scope': 'Fresh-session recovery; old token was already expired, not isolated restart invalidation'}


def check_queued_stop(root):
    root = Path(root)
    folder = root/'queued-stop'
    info, session = read(folder/'observation.json'), read(root/'second-session.json')
    assert info['captureCompleted'] is True and not info.get('cleanupRelease')
    assert info['driverExitCodes'] == [0,0,0,0]
    assert info['revitPid'] == session['RevitPid']
    assert session['BridgeSessionId'] != read(root/'restart/session.json')['BridgeSessionId']
    context = check_capture(folder/'context/sdk', 'Context')
    assert context['bridgeSessionId'] == session['BridgeSessionId']
    assert context['revitPid'] == session['RevitPid']
    blocker, queued = info['blockerRequestId'], info['queued']
    assert set(queued) == {'queued-1', 'queued-2', 'queued-3'}
    assert len(set([blocker,*queued.values()])) == 4
    snapshots = [trace(folder/name) for name in ('before.tsv', 'preparation-held.tsv',
        'all-queued.tsv', 'stopped-while-reader-held.tsv', 'after.tsv')]
    for earlier, later in zip(snapshots, snapshots[1:]):
        assert later[:len(earlier)] == earlier, 'Trace prefix changed'
    before, preparation, admitted, held, after = snapshots
    fresh = after[len(before):]
    assert not any(r[4] in ('AutomaticRelease','ControlRejected') for r in fresh)
    assert [r[5] for r in fresh if r[4]=='Submitted'] == [context['requestId'],blocker,*queued.values()]
    api_thread = before[0][3]

    def one(stage, request=blocker, detail=None):
        rows = [r for r in fresh if r[4]==stage and (request is None or r[5]==request)
                and (detail is None or r[8]==detail)]
        assert len(rows)==1, (stage,request,detail)
        return rows[0]

    prep = one('GateHeld', detail='Preparation')
    prep_release = one('GateReleased', detail='Preparation')
    reader, read_hold, inject, enter, returned, inactive, release, reader_exit, executor_exit, quiesced = [
        one(stage,detail=detail) for stage,detail in [('ReaderEntered',None),('GateHeld','Reader'),
        ('StopInjected','Reader'),('StopEntered',None),('StopReturned',None),('InactiveObserved','Reader'),
        ('GateReleased','Reader'),('ReaderExited',None),('ExecutorExited',None),('QuiescedObserved',None)]]
    network, disposed, unhooked = [one(stage,request=None) for stage in ('NetworkStopped','EventDisposed','EventsUnhooked')]
    assert prep in preparation and prep_release not in admitted
    assert all(r in held for r in (reader,read_hold,inject,enter,returned,inactive,network,disposed))
    assert all(r not in held for r in (release,reader_exit,executor_exit,quiesced))
    ordered = [prep,prep_release,reader,read_hold,inject,enter,returned,release,reader_exit,executor_exit]
    assert [int(r[0]) for r in ordered] == sorted(int(r[0]) for r in ordered)
    assert all(r[3]==api_thread for r in ordered+[disposed,unhooked])
    assert network[8] == quiesced[8] == 'RanToCompletion'
    assert inactive[6:8] == release[6:8] == ['True','False']
    assert int(unhooked[0]) > int(enter[0])
    assert int(quiesced[0]) > int(executor_exit[0])
    deadlines = [(folder/f'{s}-deadline.txt').read_text() for s in ('Preparation','Reader')]
    assert deadlines[0] == deadlines[1], 'Original budget changed between stages'
    assert utc(release[1]) < utc(deadlines[0])
    assert utc(info['preparationReleaseUtc']) <= utc(prep_release[1])
    assert utc(info['readerReleaseUtc']) <= utc(release[1])
    assert utc(info['finishWrittenUtc']) >= max(utc(executor_exit[1]),utc(unhooked[1]))
    assert (folder/'Preparation-arm.txt').read_text().splitlines() == ['45000','hold']
    assert (folder/'Reader-arm.txt').read_text().splitlines() == ['45000','stop']
    for request in queued.values():
        submitted = one('Submitted',request)
        completed = one('QuiescedObserved',request)
        assert submitted in admitted and completed in held and completed[8]=='RanToCompletion'
        assert completed[7]=='False'
        assert not any(r[5]==request and r[4] in ('ExecutorEntered','ReaderEntered','ReaderExited','ExecutorExited') for r in fresh)
    for name, request in [('blocker',blocker),*queued.items()]:
        error = check_capture(folder/name/'sdk','BridgeUnavailable')
        assert error['requestId']==request and error['retryable'] is True
        incoming = [json.loads(s) for s in (folder/name/'sdk/wire/client-to-server.jsonl').read_text().splitlines()]
        calls = [r for r in incoming if r['method']=='tools/call']
        assert len(calls)==1 and calls[0]['params']['arguments']=={'contextToken':context['contextToken']}
        post = read(folder/name/'sdk/post-release-observation.json')
        assert post['pass'] is True and post['elapsedMilliseconds']>=3000 and post['catalogCount']==2
        assert utc(post['releasedUtc']) >= utc(executor_exit[1])
    state = read(root/'after-stop-state.json')
    assert state['revitAlive'] is True and state['revitPid']==session['RevitPid'] and state['descriptorCount']==0
    return {'pass': True, 'queuedRequests':3, 'queuedExecutorEntries':0, 'queuedReaderEntries':0,
            'networkCompletedWhileReaderHeld':True, 'eventDisposedOnApiThread':True,
            'hooksRemovedOnApiThread':True, 'scope':'Injected Stop; not application shutdown or physical lifetime disposal'}


def check_restoration(root):
    root = Path(root)
    state = read(root/'after-close-state.json')
    assert state['userReportedClosed'] is True and state['revitProcessCount']==state['descriptorCount']==0
    before, final = trace(root/'queued-stop/after.tsv'), trace(root/'shutdown.tsv')
    assert final[:len(before)]==before
    fresh = final[len(before):]
    stages = [r[4] for r in fresh]
    assert stages.count('ApplicationShutdownEntered')==stages.count('ApplicationShutdownReturned')==1
    assert stages.index('ApplicationShutdownEntered')<stages.index('StopEntered')<stages.index('StopReturned')<stages.index('ApplicationShutdownReturned')
    assert all(r[3]==before[0][3] for r in fresh if r[4]!='NetworkStopped')
    assert all(r[8]=='RanToCompletion' for r in fresh if r[4]=='NetworkStopped')
    previous = PREVIOUS.parent
    first = read(previous/'host-diagnostic-startup-evidence/corrected-receipt.json')
    second = read(previous/'host-diagnostic-evidence/receipt.json')
    intermediate, restored = read(root/'after-first-undo.json'), read(root/'after-normal-restoration.json')
    for receipt, observed in [(first,intermediate),(second,restored)]:
        assert observed['pass'] is True
        expected = {op['Target']:op['Before'].lower() for op in receipt['Operations']}
        targets = {item['path']:item['sha256'] for item in observed['targets']}
        assert len(targets)==len(observed['targets'])==97
        assert targets==expected, 'Restored targets/inventories differ from prepared backups'
    assert {row['transaction'] for row in restored['recoveryStates']}=={first['Transaction'],second['Transaction']}
    assert all(row['state']['State']=='Restored' for row in restored['recoveryStates'])
    baseline = read(previous/'host-diagnostic-startup-evidence/after-normal-close.json')
    files = read(root/'closed-files.json')
    assert files['userReportedClosed'] is True and files['diagnosticArms']==0 and files['diagnosticError'] is False
    assert {row['path']:row['sha256'] for row in files['sampleChecks']} == {row['path']:row['sha256'] for row in baseline['sampleChecks']}
    paths = read(root/'pre-restore-physical-paths.json')
    assert len(paths)==5
    for row in paths:
        if row['Requested'].endswith('BimAgentLab.RevitReadiness.dll'):
            assert row['Redirected'] is False
        else:
            assert row['Redirected'] is True and '\\LocalCache\\Local\\BimAgentLab\\' in row['Physical']
    return {'pass':True,'normalCandidate':'9a93f6d','restoredTargets':97,'unchangedSavedModelsAndSidecars':4,
            'scope':'Idle application shutdown and normal-build restoration; shared server remains redirected'}


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('directory', type=Path)
    root = parser.parse_args().directory
    print(json.dumps({'restart':check_restart(root), 'queuedStop':check_queued_stop(root), 'restoration':check_restoration(root)}, indent=2))
