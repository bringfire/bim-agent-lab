"""Verify one initial legacy desktop exchange, not the Task 8 independent BIM oracle.

Input is an immutable, complete-line snapshot containing initialization, discovery,
one context acquisition and one report. A live trace directory is not input.
This deliberately does not establish five-case acceptance or process shutdown.
"""
import argparse
import hashlib
import json
import re
from datetime import datetime
from pathlib import Path

TOOLS = {'revit_get_readiness_context', 'revit_check_required_type_information'}


def read(path):
    return json.loads(Path(path).read_text(encoding='utf-8-sig'))


def canonical(value):
    # Python equality alone treats True and 1 as interchangeable.
    return json.dumps(value, ensure_ascii=False, sort_keys=True, separators=(',', ':'))


def check_desktop_continuity(directory):
    directory = Path(directory)
    meta = read(directory / 'snapshot.json')
    protocol = meta['protocolVersion']
    assert protocol in {'2025-06-18', '2025-11-25'}, 'Use the separate July verifier for modern exchanges'
    expected_files = {'wire/client-to-server.jsonl', 'wire/server-to-client.jsonl'}
    assert len(meta['files']) == 2 and {f['path'] for f in meta['files']} == expected_files
    records = {}
    for file in meta['files']:
        data = (directory / file['path']).read_bytes()
        assert data and data.endswith(b'\n'), 'Incomplete JSONL prefix'
        assert len(data) == file['bytes'] and hashlib.sha256(data).hexdigest() == file['sha256'], 'Changed snapshot bytes'
        records[file['path']] = [json.loads(line) for line in data.splitlines()]
    incoming, outgoing = (records[name] for name in ('wire/client-to-server.jsonl', 'wire/server-to-client.jsonl'))
    for row in incoming + outgoing:
        assert row['jsonrpc'] == '2.0'
    requests = [r for r in incoming if 'id' in r]
    ids = [r['id'] for r in requests]
    assert len(ids) == len(set(ids)), 'Duplicate request'
    responses = [r for r in outgoing if 'id' in r]
    response_ids = [r['id'] for r in responses]
    assert len(response_ids) == len(set(response_ids)), 'Duplicate response'
    assert set(response_ids) == set(ids), 'Missing or uncorrelated response'
    responses = {r['id']:r for r in responses}
    assert [r['method'] for r in requests] == ['initialize', 'tools/list', 'tools/call', 'tools/call']
    assert len(incoming) == 5 and incoming[0] == requests[0]
    assert incoming[1]['method'] == 'notifications/initialized' and 'id' not in incoming[1]
    init = requests[0]['params']
    assert canonical(init['clientInfo']) == canonical(meta['clientInfo'])
    assert init['protocolVersion'] == protocol
    for response in responses.values():
        assert 'error' not in response and 'result' in response, 'JSON-RPC failure'
    initialized = responses[requests[0]['id']]['result']
    assert initialized['protocolVersion'] == protocol
    assert initialized['serverInfo']['name'] == 'bim-agent-lab'
    catalog = responses[requests[1]['id']]['result']['tools']
    assert len(catalog) == 2 and {t['name'] for t in catalog} == TOOLS

    values = []
    for request, name, saved in zip(requests[2:], ['revit_get_readiness_context', 'revit_check_required_type_information'], ['context.json', 'check.json']):
        assert request['params']['name'] == name
        result = responses[request['id']]['result']
        assert result['isError'] is False
        data = result['structuredContent']
        assert data['contractVersion'] == 1 and type(data['contractVersion']) is int
        assert re.fullmatch('[0-9a-f]{32}', data['requestId'])
        assert len(result['content']) == 1 and result['content'][0]['type'] == 'text'
        assert canonical(json.loads(result['content'][0]['text'])) == canonical(data), 'Text/structured disagreement'
        assert canonical(read(directory / saved)) == canonical(result), 'Saved/wire disagreement'
        values.append(data)
    context, result = values
    assert context['requestId'] == meta['contextRequestId']
    assert result['requestId'] == meta['reportRequestId'] != context['requestId']
    assert context['revitPid'] == meta['revitPid']
    assert context['bridgeSessionId'] == result['bridgeSessionId'] == meta['bridgeSessionId']
    assert context['checklistSha256'] == result['checklistSha256']
    assert requests[2]['params']['arguments'] == {}
    assert requests[3]['params']['arguments'] == {'contextToken':context['contextToken']}
    assert isinstance(context['contextToken'], str) and context['contextToken']
    assert context['documentLifetimeId']
    report = result['report']
    for key in ['DocumentPath', 'ProjectUniqueId', 'RevitBuild', 'ChecklistId', 'ChecklistPath', 'Scope', 'Level', 'Phase']:
        assert report[key] == context[key[0].lower()+key[1:]], key
    assert datetime.fromisoformat(report['GeneratedUtc'].replace('Z', '+00:00')) < datetime.fromisoformat(context['expiresUtc'].replace('Z', '+00:00'))
    reference = read(directory / meta['referenceReport'])['structuredContent']
    assert reference['bridgeSessionId'] == result['bridgeSessionId']
    assert reference['checklistSha256'] == result['checklistSha256']
    without_time = lambda value:{k:v for k,v in value.items() if k != 'GeneratedUtc'}
    assert canonical(without_time(report)) == canonical(without_time(reference['report'])), 'Report differs from SDK continuity baseline'
    assert report['CoverageComplete'] is True and report['Evaluation']['Complete'] is True
    assert report['ModifiedBefore'] is False and report['ModifiedAfter'] is False
    return {'pass':True, 'scope':'Initial desktop continuity; not independent BIM oracle, five-case A8 or shutdown acceptance',
            'protocolVersion':protocol, 'clientInfo':init['clientInfo'], 'contextRequestId':context['requestId'],
            'reportRequestId':result['requestId'], 'bridgeSessionId':result['bridgeSessionId'],
            'requestedInstances':report['RequestedInstances'], 'examinedInstances':report['ExaminedInstances'],
            'findingCount':report['Evaluation']['FindingCount'], 'coverageComplete':report['CoverageComplete'],
            'evaluationComplete':report['Evaluation']['Complete'], 'modifiedBefore':report['ModifiedBefore'],
            'modifiedAfter':report['ModifiedAfter'], 'reportFieldsExcludedFromComparison':['GeneratedUtc']}


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('directory')
    print(json.dumps(check_desktop_continuity(parser.parse_args().directory), indent=2))
