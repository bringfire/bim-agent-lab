"""Synthetic SDK-output producer for runner process tests. Never live MCP evidence."""
import json
import os
from pathlib import Path
import sys
import time
from datetime import datetime, timedelta, timezone

args = dict(zip(sys.argv[1::2], sys.argv[2::2]))
server = Path(args['--server'])
config = json.loads((server.parent / 'fixture.json').read_text(encoding='utf-8'))
mode = server.read_text(encoding='utf-8')
source = Path(config['source'])
out = Path(args['--evidence']); out.mkdir()
pause = Path(args['--pause-file'])

def write(name, value):
    (out / name).write_text(json.dumps(value), encoding='utf-8')

write('partial.json', {'synthetic': True, 'pid': int(args['--revit-pid']), 'protocol': args['--protocol']})
(out / 'fixture-pid').write_text(str(os.getpid()))
if mode == 'exit': sys.exit(7)
if mode == 'hold': time.sleep(60); sys.exit(8)
now = datetime.now(timezone.utc)
replacements = {config['oldModel']: config['model'], config['oldChecklist']: config['checklist']}
expectation = json.loads(Path(config['expectation']).read_text(encoding='utf-8'))

def transform(value):
    if isinstance(value, str): return replacements.get(value, value)
    if isinstance(value, list): return [transform(x) for x in value]
    if not isinstance(value, dict): return value
    result = {k: transform(v) for k, v in value.items()}
    if 'expiresUtc' in result: result['expiresUtc'] = (now + timedelta(minutes=5)).isoformat()
    if 'GeneratedUtc' in result and mode != 'stale-report': result['GeneratedUtc'] = now.isoformat()
    if 'checklistSha256' in result: result['checklistSha256'] = expectation['checklistSha256']
    if 'structuredContent' in result:
        result['content'] = [{'type': 'text', 'text': json.dumps(result['structuredContent'])}]
    return result

context = transform(json.loads((source / 'context.json').read_text(encoding='utf-8-sig')))
if mode == 'wrong-model': context['structuredContent']['documentPath'] += '.other'
if mode == 'wrong-session': context['structuredContent']['bridgeSessionId'] = '0' * 32
if mode == 'wrong-hash': context['structuredContent']['checklistSha256'] = '0' * 64
if mode == 'context-error': context = {'isError': True, 'structuredContent': {'code': 'BridgeUnavailable'}}
if mode == 'change-before-read': Path(config['checklist']).write_bytes(b'changed')
write('context.json', context)
write('context-ready.json', {'token': context.get('structuredContent', {}).get('contextToken'), 'waitingFor': str(pause)})
until = time.monotonic() + 10
while not pause.exists():
    if time.monotonic() > until: sys.exit(9)
    time.sleep(0.01)
check = transform(json.loads((source / 'check.json').read_text(encoding='utf-8-sig')))
if mode == 'corrupt-report':
    check['structuredContent']['report']['Evaluation']['FindingCount'] = 99
    check['content'][0]['text'] = json.dumps(check['structuredContent'])
write('check.json', check)
if mode == 'change-after-read': Path(config['checklist']).write_bytes(b'changed')
(out / 'wire').mkdir()
for name in ['client-to-server.jsonl', 'server-to-client.jsonl']:
    if mode == 'missing-wire' and name == 'server-to-client.jsonl': continue
    rows = [transform(json.loads(line)) for line in (source / 'wire' / name).read_bytes().splitlines()]
    for row in rows:
        if row.get('result', {}).get('structuredContent', {}).get('requestId') == check['structuredContent']['requestId']:
            row['result'] = check
    (out / 'wire' / name).write_text(''.join(json.dumps(row) + '\n' for row in rows), encoding='utf-8')
write('sdk-client-result.json', {'synthetic': True, 'protocol': args['--protocol'], 'pass': True})
if mode != 'missing-completion': write('complete.json', {'pass': mode != 'false-completion'})
