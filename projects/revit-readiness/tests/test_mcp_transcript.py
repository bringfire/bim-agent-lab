import json
import tempfile
import unittest
from pathlib import Path
from verify_mcp_transcript import verify


class TranscriptTests(unittest.TestCase):
    def fixture(self, directory):
        meta = {'io.modelcontextprotocol/protocolVersion': '2026-07-28',
                'io.modelcontextprotocol/clientInfo': {'name': 'independent-fixture'}}
        requests = [dict(jsonrpc='2.0', id=i, method=method, params={'_meta': meta, **extra})
                    for i, method, extra in [
                        (1, 'server/discover', {}), (2, 'tools/list', {}),
                        (3, 'tools/call', {'name': 'lab_protocol_probe', 'arguments': {'fail': False}}),
                        (4, 'tools/call', {'name': 'lab_protocol_probe', 'arguments': {'fail': True}})]]
        results = [{'supportedVersions': ['2026-07-28']}, {'tools': [{'name': 'lab_protocol_probe'}]},
                   {'isError': False, 'structuredContent': {'code': 'ProbeSuccess', 'synthetic': True, 'revitAccess': False}},
                   {'isError': True, 'structuredContent': {'code': 'ProbeFailure', 'synthetic': True, 'revitAccess': False}}]
        responses = [dict(jsonrpc='2.0', id=i, result=r) for i, r in enumerate(results, 1)]
        self.write(directory, requests, responses)
        return requests, responses

    def write(self, directory, requests, responses):
        for name, values in [('client-to-server', requests), ('server-to-client', responses)]:
            (directory / (name + '.jsonl')).write_text(''.join(json.dumps(v) + '\n' for v in values), encoding='utf-8')

    def test_complete_exchange(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp); self.fixture(root)
            self.assertEqual(verify(root, '2026-07-28')['protocol'], '2026-07-28')

    def test_missing_false_flag_wrong_protocol_id_and_diagnostics_are_rejected(self):
        for case in ['missing_false', 'older_protocol', 'wrong_id', 'missing_call', 'diagnostic']:
            with self.subTest(case=case), tempfile.TemporaryDirectory() as temp:
                root = Path(temp); requests, responses = self.fixture(root)
                if case == 'missing_false': del responses[2]['result']['isError']
                elif case == 'older_protocol': requests[0]['params']['_meta']['io.modelcontextprotocol/protocolVersion'] = '2025-11-25'
                elif case == 'wrong_id': responses[3]['id'] = 99
                elif case == 'missing_call': requests.pop(); responses.pop()
                self.write(root, requests, responses)
                if case == 'diagnostic':
                    with (root/'server-to-client.jsonl').open('a', encoding='utf-8') as f: f.write('debug output\n')
                with self.assertRaises((AssertionError, ValueError)): verify(root, '2026-07-28')


if __name__ == '__main__': unittest.main()
