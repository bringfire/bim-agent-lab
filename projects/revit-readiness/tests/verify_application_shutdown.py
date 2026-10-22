"""Verify diagnostic actual-application shutdown; synthetic trace tests are separate."""
import argparse
import json
from pathlib import Path
from verify_live_lifecycle_capture import check_capture
from verify_host_deadline_capture import trace


def check_trace(info, rows):
    assert info['captureCompleted'] is True and info['processExited'] is True and info['descriptorAbsent'] is True
    assert info['retiredWhileProcessAlive'] is True
    assert not info.get('cleanupRelease') and not info.get('forcedTermination') and not info.get('cleanupErrors') and not info.get('error')
    assert info['driverExitCodes'] == [0,0,0]
    assert set(info['queued']) == {'queued-1','queued-2','queued-3'}
    ids = set(info['queued'].values()); assert len(ids)==3
    assert all(len(row)==9 for row in rows)
    assert not any(r[4] in ('ExecutorEntered','ReaderEntered','ControlRejected','LifecycleAutomaticRelease',
                             'AutomaticRelease','Pipe.LifetimeCleanupFailed','Pipe.CancellationCallbackFailed') for r in rows)
    def one(stage, request=None, detail=None):
        matches = [i for i,r in enumerate(rows) if r[4]==stage and (request is None or r[5]==request) and (detail is None or r[8]==detail)]
        assert len(matches)==1, (stage,request,detail,matches)
        return matches[0]
    closing = one('ApplicationClosingEntered')
    held = one('LifecycleHeld', detail='ApplicationClosing')
    release = one('LifecycleReleased', detail='ApplicationClosing')
    close_return = one('ApplicationClosingReturned')
    shutdown = one('ApplicationShutdownEntered')
    stop = one('StopEntered'); stopped = one('StopReturned'); disposed = one('EventDisposed'); unhooked = one('EventsUnhooked')
    after_stop = one('LifecycleHeld', detail='ShutdownAfterStop')
    final_release = one('LifecycleReleased', detail='ShutdownAfterStop')
    returned = one('ApplicationShutdownReturned')
    network = one('NetworkStopped', detail='RanToCompletion')
    assert closing < held < release < close_return < shutdown < stop < stopped < after_stop < final_release < returned
    assert stop < disposed < stopped and stop < unhooked < after_stop
    assert stop < network < final_release
    assert all(rows[i][3]==rows[closing][3] for i in (held,release,close_return,shutdown,stop,stopped,disposed,unhooked,after_stop,final_release,returned))
    assert {r[5] for r in rows if r[4]=='Submitted'} == ids
    assert {r[5] for r in rows if r[4]=='Pipe.LifetimeDisposed'} == ids
    for request in ids:
        assert held < one('Submitted',request) < release
        assert stop < one('QuiescedObserved',request,'RanToCompletion') < final_release
        assert stop < one('Pipe.LifetimeDisposed',request) < final_release
    return {'pass':True,'queuedRequests':3,'physicalLifetimeDisposals':3,
            'scope':'Actual shutdown with queued work under diagnostic lifecycle holds; not concurrent reader execution or native Raise overlap proof'}


def verify(root):
    root=Path(root); info=json.loads((root/'observation.json').read_text(encoding='utf-8'))
    before, after=trace(root/'before.tsv'),trace(root/'after.tsv')
    assert after[:len(before)]==before
    result=check_trace(info,after[len(before):])
    context=check_capture(root/'context/sdk','Context')
    session=json.loads((root/'session.json').read_text(encoding='utf-8-sig'))
    assert context['revitPid']==info['revitPid']==session['RevitPid']
    assert context['bridgeSessionId']==session['BridgeSessionId']
    for name,request in info['queued'].items():
        error=check_capture(root/name/'sdk','BridgeUnavailable')
        assert error['requestId']==request and error['retryable'] is True
        assert error['requestId'] != context['requestId']
        incoming=[json.loads(line) for line in (root/name/'sdk/wire/client-to-server.jsonl').read_text(encoding='utf-8').splitlines()]
        calls=[item['params'] for item in incoming if item.get('method')=='tools/call']
        assert len(calls)==1 and calls[0]['name']=='revit_check_required_type_information'
        assert calls[0]['arguments']=={'contextToken':context['contextToken']}
    return result


if __name__=='__main__':
    parser=argparse.ArgumentParser(); parser.add_argument('directory'); args=parser.parse_args()
    print(json.dumps(verify(args.directory),indent=2))
