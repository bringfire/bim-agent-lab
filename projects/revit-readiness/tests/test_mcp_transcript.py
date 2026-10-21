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


class LegacyTranscriptTests(unittest.TestCase):
    fixture = TranscriptTests.fixture
    write = TranscriptTests.write
    def legacy_fixture(self, directory, protocol):
        requests, responses = self.fixture(directory)
        requests[0] = dict(jsonrpc='2.0', id=1, method='initialize', params={
            'protocolVersion': protocol, 'capabilities': {}, 'clientInfo': {'name': 'legacy-fixture'}})
        responses[0]['result'] = {'protocolVersion': protocol, 'capabilities': {'tools': {}},
                                  'serverInfo': {'name': 'bim-agent-lab', 'version': '0.1.0'}}
        for row in requests[1:]: del row['params']['_meta']
        requests.insert(1, dict(jsonrpc='2.0', method='notifications/initialized'))
        self.write(directory, requests, responses)
        return requests, responses

    def test_accepted_legacy_revisions_preserve_complete_exchange(self):
        for protocol in ['2025-06-18', '2025-11-25']:
            with self.subTest(protocol=protocol), tempfile.TemporaryDirectory() as temp:
                root = Path(temp); self.legacy_fixture(root, protocol)
                result = verify(root, protocol)
                self.assertTrue(result['pass'])
                self.assertEqual(result['clientNames'], ['legacy-fixture'])
                self.assertEqual(result['checks'], ['failure', 'initialize', 'initialized', 'list', 'success'])

    def test_legacy_negotiation_order_correlation_and_results_cannot_be_relaxed(self):
        for case in ['requested_version', 'selected_version', 'missing_initialized', 'early_tools',
                     'duplicate_initialize', 'missing_false', 'wrong_code', 'wrong_id', 'missing_call']:
            with self.subTest(case=case), tempfile.TemporaryDirectory() as temp:
                root = Path(temp); requests, responses = self.legacy_fixture(root, '2025-06-18')
                if case == 'requested_version': requests[0]['params']['protocolVersion'] = '2025-11-25'
                elif case == 'selected_version': responses[0]['result']['protocolVersion'] = '2025-11-25'
                elif case == 'missing_initialized': requests.pop(1)
                elif case == 'early_tools': requests[1], requests[2] = requests[2], requests[1]
                elif case == 'duplicate_initialize': requests.insert(1, requests[0])
                elif case == 'missing_false': del responses[2]['result']['isError']
                elif case == 'wrong_code': responses[3]['result']['structuredContent']['code'] = 'ProbeSuccess'
                elif case == 'wrong_id': responses[3]['id'] = 99
                elif case == 'missing_call': requests.pop(); responses.pop()
                self.write(root, requests, responses)
                with self.assertRaises((AssertionError, ValueError, KeyError)): verify(root, '2025-06-18')

    def test_legacy_capture_cannot_satisfy_strict_july(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp); self.legacy_fixture(root, '2025-11-25')
            with self.assertRaises(AssertionError): verify(root, '2026-07-28')

    def test_legacy_tools_list_may_omit_optional_params(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp); requests, responses = self.legacy_fixture(root, '2025-11-25')
            del requests[2]['params']
            self.write(root, requests, responses)
            self.assertTrue(verify(root, '2025-11-25')['pass'])

    def test_unverified_protocol_is_rejected(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp); self.legacy_fixture(root, '2024-11-05')
            with self.assertRaises(AssertionError): verify(root, '2024-11-05')


if __name__ == '__main__': unittest.main()
