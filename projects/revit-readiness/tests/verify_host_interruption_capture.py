"""Independent comparison of actual-host explicit cancellation and EOF captures."""
import argparse
import json
from pathlib import Path
from verify_host_deadline_capture import trace
from verify_live_lifecycle_capture import read, utc, check_capture, TOOLS


def check_case(case, origin, stage, session):
    info = read(case/'observation.json')
    assert info['origin'] == origin and info['stage'] == stage
    assert info['revitPid'] == session['RevitPid']
    assert info['captureCompleted'] is True and info['clientExitCode'] == 0 and not info.get('cleanupRelease')
    assert info['inactiveObservedBeforeRelease'] is True and info['interruptionCompletedBeforeRelease'] is True
    assert read(case/'sdk/complete.json')['pass'] is True
    server_exit = read(case/'sdk/server-exit.json')
    assert server_exit['ExitCode'] == 0 and server_exit['forcedTermination'] is False
    ctx_result = read(case/'sdk/context.json')
    assert ctx_result['isError'] is False
    context = ctx_result['structuredContent']
    assert context['revitPid'] == session['RevitPid'] and context['bridgeSessionId'] == session['BridgeSessionId']
    incoming = [json.loads(s) for s in (case/'sdk/wire/client-to-server.jsonl').read_text().splitlines()]
    outgoing = [json.loads(s) for s in (case/'sdk/wire/server-to-client.jsonl').read_text().splitlines()]
    assert all(r['jsonrpc']=='2.0' for r in incoming+outgoing)
    requests = [r for r in incoming if 'id' in r]
    ids = [r['id'] for r in requests]
    assert len(ids) == len(set(ids))
    assert all(r['params']['_meta']['io.modelcontextprotocol/protocolVersion']=='2026-07-28' for r in requests)
    methods = [r['method'] for r in requests]
    expected = ['server/discover','tools/list','tools/call','tools/call']
    assert methods == expected + (['tools/list'] if origin=='cancel' else [])
    assert requests[2]['params']['name']=='revit_get_readiness_context'
    call = requests[3]
    assert call['params']['name']=='revit_check_required_type_information'
    assert call['params']['arguments']=={'contextToken':context['contextToken']}
    assert call['id'] == read(case/'sdk/check-started.json')['rpcRequestId']
    cancel = [r for r in incoming if 'id' not in r]
    if origin=='cancel':
        assert len(cancel)==1 and cancel[0]['method']=='notifications/cancelled'
        assert cancel[0]['params']['requestId']==call['id']
    else:
        assert not cancel, 'EOF stimulus must not include prior cancellation'
    response_ids = [r['id'] for r in outgoing]
    assert len(response_ids)==len(set(response_ids))
    assert set(response_ids)==set(ids)-{call['id']}, 'Unexpected response or late success'
    answers={r['id']:r for r in outgoing}
    assert all('error' not in r for r in outgoing)
    assert '2026-07-28' in answers[requests[0]['id']]['result']['supportedVersions']
    assert answers[requests[2]['id']]['result']==ctx_result
    assert json.loads(ctx_result['content'][0]['text'])==context
    for request in requests:
        if request['method']=='tools/list':
            assert {t['name'] for t in answers[request['id']]['result']['tools']}==TOOLS
    before, held, after=[trace(case/name) for name in ('before.tsv','held.tsv','after.tsv')]
    assert held[:len(before)]==before and after[:len(held)]==held
    fresh=after[len(before):]
    assert not any(r[4] in ('ControlRejected','AutomaticRelease') for r in fresh)
    submitted=[r[5] for r in fresh if r[4]=='Submitted']
    request=info['requestId']
    assert submitted==[context['requestId'],request], 'Unaccounted admission or wrong host request'
    own=[r for r in fresh if r[5]==request]
    def one(stage_name):
        matches=[r for r in own if r[4]==stage_name]
        assert len(matches)==1, stage_name
        return matches[0]
    admission, entered, gate, inactive, released, exited, response, quiesced = [one(s) for s in
        ('Submitted','ExecutorEntered','GateHeld','InactiveObserved','GateReleased','ExecutorExited','ResponseCompleted','QuiescedObserved')]
    assert all(r[3]==before[0][3] for r in (entered,gate,inactive,released,exited))
    assert gate[6:8]==['False','False'] and inactive[6:8]==['True','False']
    assert released[6:8]==exited[6:8]==['True','False']
    assert gate[8]==inactive[8]==released[8]==stage
    assert response[8]=='Canceled' and quiesced[8]=='RanToCompletion'
    assert int(admission[0])<int(entered[0])<int(gate[0])<int(inactive[0])<int(released[0])<int(exited[0])
    held_own=[r for r in held if r[5]==request]
    assert inactive in held_own and response in held_own
    assert not any(r[4] in ('GateReleased','ExecutorExited','QuiescedObserved','ReaderExited') for r in held_own)
    entries=[r for r in own if r[4]=='ReaderEntered']; exits=[r for r in own if r[4]=='ReaderExited']
    if stage=='Preparation':
        assert not entries and not exits
    else:
        assert len(entries)==len(exits)==1
        assert int(entered[0])<int(entries[0][0])<int(gate[0])<int(released[0])<int(exits[0][0])<int(exited[0])
        assert entries[0][3]==exits[0][3]==before[0][3]
    deadline=utc(info['originalDeadlineUtc'])
    assert (case/'held-deadline.txt').read_text(encoding='utf-8-sig')==info['originalDeadlineUtc']
    assert (case/'consumed-arm.txt').read_text().splitlines()==['45000','hold']
    assert 0<(deadline-utc(admission[1])).total_seconds()<=30
    interruption=read(case/'sdk/interrupted.json')
    assert interruption['mode']==origin
    assert interruption['serverExited'] is (origin=='eof')
    assert interruption['exitCode']==(0 if origin=='eof' else None)
    assert interruption['elapsedMilliseconds']<5000
    assert utc(gate[1])<=utc(info['interruptWrittenUtc'])<=utc(interruption['interruptedUtc'])<=utc(inactive[1])
    assert utc(interruption['observedUtc'])<=utc(info['releaseWrittenUtc'])<=utc(released[1])<deadline
    assert (deadline-utc(inactive[1])).total_seconds()>15, 'Expiry must not substitute for cancellation'
    post=read(case/'sdk/post-release-observation.json')
    assert post['elapsedMilliseconds']>=3000
    assert post['catalogCount']==(2 if origin=='cancel' else None)
    assert post['serverExited'] is (origin=='eof')
    assert utc(exited[1])<=utc(post['releasedUtc'])
    assert (utc(post['observedUntilUtc'])-utc(post['releasedUtc'])).total_seconds()>=3
    return {'origin':origin,'stage':stage,'requestId':request,'readerEntries':len(entries),
            'interruptionMilliseconds':interruption['elapsedMilliseconds'],'pass':True}


def check_queued(root, session, baseline):
    folder=root/'queued'; info=read(folder/'observation.json')
    assert info['captureCompleted'] is True and not info.get('cleanupRelease')
    assert info['driverExitCodes']==[0,0,0,0] and info['revitPid']==session['RevitPid']
    assert set(info['requests'])=={'cancel','eof','deadline'}
    context=check_capture(folder/'context/sdk','Context')
    assert context['bridgeSessionId']==session['BridgeSessionId'] and context['revitPid']==session['RevitPid']
    before, held, after=[trace(folder/name) for name in ('before.tsv','all-interrupted-while-held.tsv','after.tsv')]
    assert held[:len(before)]==before and after[:len(held)]==held
    fresh=after[len(before):]; blocker=info['blockerRequestId']
    def one(request,stage):
        found=[r for r in fresh if r[5]==request and r[4]==stage]
        assert len(found)==1,(request,stage)
        return found[0]
    gate=one(blocker,'GateHeld'); released=one(blocker,'GateReleased')
    assert gate[8]==released[8]=='Reader' and int(gate[0])<int(released[0])
    assert all(one(blocker,s) not in held for s in ('ReaderExited','ExecutorExited','QuiescedObserved'))
    assert one(blocker,'ReaderEntered') in held
    assert not any(r[4] in ('AutomaticRelease','ControlRejected') for r in fresh)
    assert check_capture(folder/'blocker/sdk','DeadlineExceeded')['requestId']==blocker
    deadline=utc((folder/'blocker/held-deadline.txt').read_text())
    results=[]
    for origin,entry in info['requests'].items():
        request=entry['requestId']; own=[r for r in fresh if r[5]==request]
        submitted=one(request,'Submitted'); response=one(request,'ResponseCompleted'); quiesced=one(request,'QuiescedObserved')
        assert int(gate[0])<int(submitted[0])<int(released[0])
        assert not any(r[4] in ('ExecutorEntered','ExecutorExited','ReaderEntered','ReaderExited') for r in own)
        assert response in held and quiesced in held and quiesced[8]=='RanToCompletion'
        sdk=folder/origin/'sdk'; assert read(sdk/'complete.json')['pass'] is True
        incoming=[json.loads(s) for s in (sdk/'wire/client-to-server.jsonl').read_text().splitlines()]
        calls=[r for r in incoming if r['method']=='tools/call']
        assert len(calls)==1 and calls[0]['params']['name']=='revit_check_required_type_information'
        assert calls[0]['params']['arguments']=={'contextToken':context['contextToken']}
        post=read(sdk/'post-release-observation.json')
        assert post['elapsedMilliseconds']>=3000 and utc(post['releasedUtc'])>=utc(released[1])
        assert utc(quiesced[1])<=utc(info['releaseWrittenUtc'])<=utc(released[1])
        if origin=='deadline':
            assert response[8]=='RanToCompletion:DeadlineExceeded' and response[7]=='True'
            assert check_capture(sdk,'DeadlineExceeded')['requestId']==request
            assert post['pass'] is True and post['catalogCount']==2
        else:
            assert response[8]=='Canceled' and quiesced[6:8]==['True','False']
            assert (deadline-utc(quiesced[1])).total_seconds()>15
            snapshot=trace(folder/origin/'interrupted.tsv')
            assert quiesced in snapshot and one(blocker,'ExecutorExited') not in snapshot
            stop=read(sdk/'interrupted.json'); end=read(sdk/'server-exit.json')
            assert stop['mode']==origin and stop['serverExited'] is (origin=='eof')
            assert end['ExitCode']==0 and end['forcedTermination'] is False
            assert utc(stop['observedUtc'])<utc(info['releaseWrittenUtc'])
            assert calls[0]['id']==read(sdk/'check-started.json')['rpcRequestId']
            notifications=[r for r in incoming if 'id' not in r]
            if origin=='cancel':
                assert len(notifications)==1 and notifications[0]['method']=='notifications/cancelled'
                assert notifications[0]['params']['requestId']==calls[0]['id']
                assert post['catalogCount']==2
            else:
                assert not notifications and post['catalogCount'] is None and stop['exitCode']==0
            outgoing=[json.loads(s) for s in (sdk/'wire/server-to-client.jsonl').read_text().splitlines()]
            expected={r['id'] for r in incoming if 'id' in r}-{calls[0]['id']}
            assert len(outgoing)==len(expected) and {r['id'] for r in outgoing}==expected
            assert all('error' not in r for r in outgoing)
            for r in outgoing:
                if 'tools' in r['result']: assert {t['name'] for t in r['result']['tools']}==TOOLS
        results.append({'origin':origin,'requestId':request,'executorEntries':0,'readerEntries':0,'quiescedWhileBlockerHeld':True})
    report=check_capture(root/'after-queued','ReadinessReport')
    assert report['bridgeSessionId']==session['BridgeSessionId']
    assert {k:v for k,v in report['report'].items() if k!='GeneratedUtc'}=={k:v for k,v in baseline['report'].items() if k!='GeneratedUtc'}
    final=trace(root/'session-through-queued.tsv')
    assert final[:len(after)]==after
    return results


def check_stop(root,session):
    folder=root/'executing-stop'; info=read(folder/'observation.json')
    assert info['origin']=='stop' and info['stage']=='Reader' and info['revitPid']==session['RevitPid']
    assert info['captureCompleted'] is True and info['clientExitCode']==0 and not info.get('cleanupRelease')
    error=check_capture(folder/'sdk','BridgeUnavailable'); request=info['requestId']
    assert error['requestId']==request and error['retryable'] is True
    context=read(folder/'sdk/context.json')['structuredContent']
    assert context['bridgeSessionId']==session['BridgeSessionId']
    before,held,after=[trace(folder/name) for name in ('before.tsv','held.tsv','after.tsv')]
    assert held[:len(before)]==before and after[:len(held)]==held
    fresh=after[len(before):]
    def one(stage):
        found=[r for r in fresh if r[4]==stage and (r[5]==request or stage=='NetworkStopped')]
        assert len(found)==1,stage
        return found[0]
    reader,gate,inject,enter,network,disposed,unhooked,returned,inactive,release,reader_exit,executor_exit,quiesced=[one(s) for s in
        ('ReaderEntered','GateHeld','StopInjected','StopEntered','NetworkStopped','EventDisposed','EventsUnhooked',
         'StopReturned','InactiveObserved','GateReleased','ReaderExited','ExecutorExited','QuiescedObserved')]
    assert network[8]=='RanToCompletion' and quiesced[8]=='RanToCompletion'
    assert all(r in held for r in (reader,gate,inject,enter,network,disposed,unhooked,returned,inactive))
    assert all(r not in held for r in (release,reader_exit,executor_exit,quiesced))
    assert all(r[3]==before[0][3] for r in (reader,gate,inject,enter,disposed,unhooked,returned,inactive,release,reader_exit,executor_exit))
    assert int(reader[0])<int(gate[0])<int(inject[0])<int(enter[0])<int(returned[0])<int(release[0])<int(reader_exit[0])<int(executor_exit[0])
    assert inactive[6:8]==release[6:8]==['True','False']
    assert not any(r[4] in ('AutomaticRelease','ControlRejected') for r in fresh)
    assert (folder/'consumed-arm.txt').read_text().splitlines()==['45000','stop']
    assert (folder/'held-deadline.txt').read_text()==info['originalDeadlineUtc']
    assert utc(release[1])<utc(info['originalDeadlineUtc'])
    post=read(folder/'sdk/post-release-observation.json')
    assert post['pass'] is True and post['elapsedMilliseconds']>=3000 and post['catalogCount']==2
    assert utc(post['releasedUtc'])>=utc(executor_exit[1])
    state=read(root/'after-stop-state.json')
    assert state['revitAlive'] is True and state['revitPid']==session['RevitPid'] and state['descriptorCount']==0
    check_capture(root/'after-stop-unavailable','BridgeUnavailable')
    return {'pass':True,'requestId':request,'networkStoppedWhileReaderHeld':True,
            'eventDisposedOnApiThread':True,'hooksRemoved':True,'descriptorRemoved':True,'revitStillAlive':True}


def check_run(directory):
    directory=Path(directory)
    session=read(directory/'session.json')
    cases=[check_case(directory/(origin+'-'+stage),origin,stage,session)
           for origin in ('cancel','eof') for stage in ('Preparation','Reader')]
    before=check_capture(directory/'baseline','ReadinessReport')
    after=check_capture(directory/'after-interruptions','ReadinessReport')
    assert before['bridgeSessionId']==after['bridgeSessionId']==session['BridgeSessionId']
    assert before['checklistSha256']==after['checklistSha256']
    assert {k:v for k,v in before['report'].items() if k!='GeneratedUtc'}=={k:v for k,v in after['report'].items() if k!='GeneratedUtc'}
    assert after['report']['ModifiedBefore'] is False and after['report']['ModifiedAfter'] is False
    final=trace(directory/'session-through-followup.tsv')
    last=trace(directory/'eof-Reader/after.tsv')
    assert final[:len(last)]==last
    own=[r for r in final if r[5]==after['requestId']]
    assert all(sum(r[4]==s for r in own)==1 for s in ('ReaderEntered','ReaderExited','QuiescedObserved'))
    queued=check_queued(directory,session,before)
    stopped=check_stop(directory,session)
    return {'pass':True,'cases':cases,'queued':queued,'executingStop':stopped,'scope':'Diagnostic host interruption/Stop; not application shutdown, SDK token propagation, physical disposal or desktop A8'}


if __name__=='__main__':
    parser=argparse.ArgumentParser(); parser.add_argument('directory',type=Path)
    print(json.dumps(check_run(parser.parse_args().directory),indent=2))
