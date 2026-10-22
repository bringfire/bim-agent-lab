"""Compare diagnostic host events with independently decoded SDK wire evidence."""
import argparse
import json
from pathlib import Path
from verify_live_lifecycle_capture import check_capture, read, utc


def trace(path):
    rows = [line.split('\t') for line in path.read_text(encoding='utf-8').splitlines()]
    assert rows and len(rows) < 4096
    assert all(len(row) == 9 for row in rows)
    assert [int(r[0]) for r in rows] == list(range(1, len(rows) + 1)), 'Missing or duplicate trace rows'
    assert all(int(a[2]) <= int(b[2]) for a, b in zip(rows, rows[1:])), 'Monotonic trace order'
    assert rows[0][4] == 'ApplicationStartup'
    return rows


def check_case(directory, stage):
    directory = Path(directory)
    observation = read(directory / 'observation.json')
    assert observation['stage'] == stage
    assert observation['captureCompleted'] is True and observation['clientExitCode'] == 0
    assert not observation.get('cleanupRelease', False)
    assert observation['inactiveObservedBeforeRelease'] is True
    assert observation['responseReceivedBeforeRelease'] is True
    error = check_capture(directory / 'sdk', 'DeadlineExceeded')
    request = observation['requestId']
    assert request == error['requestId'], 'Error/trace correlation'
    context = read(directory / 'sdk/context.json')['structuredContent']
    assert context['revitPid'] == observation['revitPid']
    before, held, after = [trace(directory / name) for name in ('before.tsv', 'held.tsv', 'after.tsv')]
    assert held[:len(before)] == before and after[:len(held)] == held, 'Snapshots are exact prefixes'
    fresh = after[len(before):]
    assert not any(r[4] in ('ControlRejected', 'AutomaticRelease') for r in fresh)
    assert not any(r[5] == request for r in before)
    own = [r for r in fresh if r[5] == request]

    def one(name):
        matches = [r for r in own if r[4] == name]
        assert len(matches) == 1, f'Expected exactly one {name}'
        return matches[0]

    submitted, entered = one('Submitted'), one('ExecutorEntered')
    gate, inactive, released = one('GateHeld'), one('InactiveObserved'), one('GateReleased')
    exited, response, quiesced = one('ExecutorExited'), one('ResponseCompleted'), one('QuiescedObserved')
    assert gate[8] == inactive[8] == released[8] == stage
    assert response[8] == 'RanToCompletion:DeadlineExceeded'
    assert quiesced[8] == 'RanToCompletion'
    assert inactive[7] == 'True', 'Original budget must expire during the hold'
    assert entered[6:8] == gate[6:8] == ['False', 'False']
    assert released[7] == exited[7] == 'True'
    api_thread = before[0][3]
    assert all(r[3] == api_thread for r in (entered, gate, inactive, released, exited))
    assert int(submitted[0]) < int(entered[0]) < int(gate[0]) < int(inactive[0]) < int(released[0]) < int(exited[0])
    # Response/Quiesced observers are asynchronous; don't invent exact task-completion order.
    held_own = [r for r in held if r[5] == request]
    assert inactive in held_own and response in held_own
    assert not any(r[4] in ('GateReleased', 'ExecutorExited', 'QuiescedObserved', 'ReaderExited') for r in held_own)
    reader_entries = [r for r in own if r[4] == 'ReaderEntered']
    reader_exits = [r for r in own if r[4] == 'ReaderExited']
    if stage == 'Preparation':
        assert not reader_entries and not reader_exits, 'Expired preparation must not enter reader'
    else:
        assert stage == 'Reader' and len(reader_entries) == len(reader_exits) == 1
        a, b = reader_entries[0], reader_exits[0]
        assert a[3] == b[3] == api_thread
        assert int(entered[0]) < int(a[0]) < int(gate[0]) < int(released[0]) < int(b[0]) < int(exited[0])
    deadline = utc(observation['originalDeadlineUtc'])
    assert (directory / 'held-deadline.txt').read_text(encoding='utf-8-sig') == observation['originalDeadlineUtc']
    assert (directory / 'consumed-arm.txt').read_text(encoding='utf-8-sig').splitlines() == ['45000', 'hold']
    remaining = (deadline - utc(submitted[1])).total_seconds()
    assert 0 < remaining <= 30, 'Admission cannot restart or exceed the original budget'
    assert utc(gate[1]) < deadline <= utc(inactive[1])
    ready = read(directory / 'sdk/response-ready.json')
    post = read(directory / 'sdk/post-release-observation.json')
    release_time = utc(observation['releaseWrittenUtc'])
    assert ready['pass'] is True and deadline <= utc(ready['receivedUtc']) <= release_time
    assert utc(inactive[1]) <= release_time <= utc(released[1]) <= utc(observation['finishWrittenUtc'])
    assert post['pass'] is True and post['catalogCount'] == 2
    assert post['elapsedMilliseconds'] >= 3000
    assert utc(post['releasedUtc']) >= utc(exited[1])
    assert (utc(post['observedUntilUtc']) - utc(post['releasedUtc'])).total_seconds() >= 3
    assert utc(observation['capturedUntilUtc']) >= utc(post['observedUntilUtc'])
    return {'stage': stage, 'requestId': request, 'readerEntries': len(reader_entries),
            'heldSeconds': (utc(released[1]) - utc(gate[1])).total_seconds(),
            'connectedObservationMilliseconds': post['elapsedMilliseconds'], 'pass': True}


def check_run(directory):
    directory = Path(directory)
    results = [check_case(directory / name, stage) for name, stage in
               [('preparation-deadline', 'Preparation'), ('reader-deadline', 'Reader')]]
    baseline = check_capture(directory / 'baseline', 'ReadinessReport')
    followup = check_capture(directory / 'after-deadlines', 'ReadinessReport')
    for field in ('bridgeSessionId', 'checklistSha256'):
        assert baseline[field] == followup[field]
    for case in ('preparation-deadline', 'reader-deadline'):
        context = read(directory / case / 'sdk/context.json')['structuredContent']
        assert context['bridgeSessionId'] == baseline['bridgeSessionId']
        assert context['checklistSha256'] == baseline['checklistSha256']
    session = read(directory / 'session.json')
    assert session['BridgeSessionId'] == baseline['bridgeSessionId']
    for case in ('baseline', 'after-deadlines', 'preparation-deadline/sdk', 'reader-deadline/sdk'):
        assert read(directory / case / 'context.json')['structuredContent']['revitPid'] == session['RevitPid']
    # Continuity comparison of all model data, not the Task 8 independent BIM oracle.
    assert {k:v for k,v in baseline['report'].items() if k != 'GeneratedUtc'} == {
        k:v for k,v in followup['report'].items() if k != 'GeneratedUtc'}
    assert followup['report']['ModifiedBefore'] is False and followup['report']['ModifiedAfter'] is False
    final = trace(directory / 'session-through-followup.tsv')
    latest = trace(directory / 'reader-deadline/after.tsv')
    assert final[:len(latest)] == latest
    own = [r for r in final if r[5] == followup['requestId']]
    assert sum(r[4] == 'ReaderEntered' for r in own) == sum(r[4] == 'ReaderExited' for r in own) == 1
    assert sum(r[4] == 'QuiescedObserved' for r in own) == 1
    return {'pass': True, 'cases': results, 'scope': 'Actual diagnostic host/SDK deadline controls; desktop A8 and physical disposal not proven'}


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('directory', type=Path)
    print(json.dumps(check_run(parser.parse_args().directory), indent=2))
