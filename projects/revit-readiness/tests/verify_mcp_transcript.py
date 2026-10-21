"""Independent assertion of captured bytes, never an MCP client or protocol adapter."""
import argparse
import json
from pathlib import Path


def verify(directory, protocol):
    assert protocol in ('2025-06-18', '2025-11-25', '2026-07-28'), 'Unverified protocol revision'
    modern = protocol == '2026-07-28'
    directory = Path(directory)
    def read(name):
        rows = [json.loads(line) for line in (directory / name).read_text(encoding='utf-8').splitlines()]
        assert rows and all(row.get('jsonrpc') == '2.0' for row in rows), 'Empty or non-protocol traffic'
        return rows
    incoming = read('client-to-server.jsonl')
    outgoing = read('server-to-client.jsonl')
    if modern:
        assert not any(row.get('method') == 'initialize' for row in incoming), 'Older handshake cannot satisfy 2026-07-28'
    responses = {}
    for row in outgoing:
        if 'id' in row:
            assert row['id'] not in responses, 'Repeated response identity'
            responses[row['id']] = row
    observed = set(); clients = set(); seen_ids = set()
    initialized = False; initialization_seen = False
    for row in incoming:
        method = row.get('method')
        if not modern and method == 'notifications/initialized':
            assert initialization_seen and not initialized, 'Unexpected initialization completion'
            assert 'id' not in row, 'Initialization completion must be a notification'
            initialized = True; observed.add('initialized')
            continue
        if method not in ('initialize', 'server/discover', 'tools/list', 'tools/call'): continue
        # Legacy tools/list may omit its optional pagination params entirely.
        # Modern requests still require protocol metadata, and calls require arguments.
        params = row.get('params', {}) if not modern and method == 'tools/list' else row['params']
        if modern:
            meta = params['_meta']
            assert meta['io.modelcontextprotocol/protocolVersion'] == protocol, 'Wrong on-wire revision'
            clients.add(meta['io.modelcontextprotocol/clientInfo']['name'])
        elif method == 'initialize':
            assert not initialization_seen and not initialized, 'Repeated initialization'
            assert params['protocolVersion'] == protocol, 'Wrong requested revision'
            clients.add(params['clientInfo']['name'])
            initialization_seen = True
        else:
            assert method != 'server/discover', 'Modern discovery cannot establish legacy acceptance'
            assert initialized, 'Tool request before completed initialization'
        assert row['id'] not in seen_ids, 'Repeated request identity'
        seen_ids.add(row['id'])
        assert row['id'] in responses, 'Missing matching response'
        response = responses[row['id']]
        assert 'error' not in response and 'result' in response, 'Protocol-level failure'
        result = response['result']
        if method == 'initialize':
            assert result['protocolVersion'] == protocol, 'Wrong selected revision'
            observed.add('initialize')
        elif method == 'server/discover':
            assert protocol in result['supportedVersions'], 'Server did not advertise requested revision'
            observed.add('discover')
        elif method == 'tools/list':
            assert [tool['name'] for tool in result['tools']] == ['lab_protocol_probe'], 'Unexpected tool catalog'
            observed.add('list')
        else:
            assert params['name'] == 'lab_protocol_probe', 'Unexpected tool'
            failure = params['arguments']['fail']
            assert type(failure) is bool and result.get('isError') is failure, 'Lost explicit success/error flag'
            data = result['structuredContent']
            assert data['synthetic'] is True and data['revitAccess'] is False, 'Misleading probe result'
            assert data['code'] == ('ProbeFailure' if failure else 'ProbeSuccess'), 'Wrong structured outcome'
            observed.add('failure' if failure else 'success')
    expected = {'discover', 'list', 'success', 'failure'} if modern else {'initialize', 'initialized', 'list', 'success', 'failure'}
    assert observed == expected, 'Incomplete compatibility exchange'
    return {'protocol': protocol, 'clientNames': sorted(clients), 'checks': sorted(observed), 'pass': True}


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('directory', type=Path)
    parser.add_argument('--protocol', required=True)
    args = parser.parse_args()
    try: print(json.dumps(verify(args.directory, args.protocol), indent=2))
    except (AssertionError, KeyError, ValueError, OSError) as error:
        parser.exit(1, 'FAIL transcript: ' + str(error) + '\n')
