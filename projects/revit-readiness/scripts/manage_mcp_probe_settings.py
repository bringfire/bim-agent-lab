"""Temporary CP0 registration only. Receipts contain private config backups; keep ignored/local."""
import argparse
import base64
import hashlib
import json
import os
from pathlib import Path
import tempfile
import tomllib

NAME = 'bim-agent-lab-protocol-probe'


def atomic(path, content):
    path = Path(path)
    fd, temp = tempfile.mkstemp(prefix='.bal-probe-', dir=path.parent)
    try:
        with os.fdopen(fd, 'wb') as stream:
            stream.write(content); stream.flush(); os.fsync(stream.fileno())
        os.replace(temp, path)
    finally:
        if os.path.exists(temp): os.unlink(temp)


def parse(content, format):
    text = content.decode('utf-8-sig')
    return json.loads(text) if format == 'json' else tomllib.loads(text)


def encode(content): return base64.b64encode(content).decode('ascii')
def decode(content): return base64.b64decode(content, validate=True)
def digest(content): return hashlib.sha256(content).hexdigest()
def save(receipt, data): atomic(receipt, json.dumps(data, indent=2).encode('utf-8'))


def prepare(config, format, command, evidence, receipt):
    config = Path(config).resolve(); receipt = Path(receipt).resolve()
    if receipt.exists(): raise ValueError('Receipt already exists; restore or inspect it first.')
    # Current desktop configuration must exist; do not invent its other settings.
    before = config.read_bytes(); data = parse(before, format)
    key = 'mcpServers' if format == 'json' else 'mcp_servers'
    if NAME in data.get(key, {}): raise ValueError('Existing probe entry is not owned by this operation.')
    entry = {'command': command, 'args': ['--wire-evidence', evidence]}
    block = ''
    if format == 'json':
        data.setdefault(key, {})[NAME] = entry
        after = (json.dumps(data, indent=2, ensure_ascii=False) + '\n').encode('utf-8')
    else:
        block = ('\n# BEGIN BIM AGENT LAB TEMPORARY PROBE\n[mcp_servers."'+NAME+'"]\ncommand = '
                 + json.dumps(command) + '\nargs = '+json.dumps(entry['args'])
                 + '\n# END BIM AGENT LAB TEMPORARY PROBE\n')
        after = before + block.encode('utf-8')
    parse(after, format)
    receipt.parent.mkdir(parents=True, exist_ok=True)
    record = {'state': 'Prepared', 'config': str(config), 'format': format, 'key': key,
              'before': encode(before), 'after': encode(after), 'beforeSha256': digest(before),
              'afterSha256': digest(after), 'entry': entry, 'block': block}
    # The durable receipt includes actual old bytes before any config write.
    with receipt.open('xb') as stream:
        stream.write(json.dumps(record, indent=2).encode('utf-8')); stream.flush(); os.fsync(stream.fileno())


def apply(receipt):
    receipt = Path(receipt); data = json.loads(receipt.read_text(encoding='utf-8'))
    config = Path(data['config']); current = config.read_bytes()
    if digest(current) != data['beforeSha256']: raise ValueError('Configuration changed since preparation.')
    after = decode(data['after'])
    if digest(after) != data['afterSha256']: raise ValueError('Prepared bytes changed.')
    atomic(config, after)
    data['state'] = 'Applied'; save(receipt, data)


def restore(receipt):
    receipt = Path(receipt); data = json.loads(receipt.read_text(encoding='utf-8'))
    config = Path(data['config']); current = config.read_bytes()
    before = decode(data['before'])
    if digest(before) != data['beforeSha256']: raise ValueError('Backup hash mismatch.')
    content = parse(current, data['format']); servers = content.get(data['key'], {})
    if NAME not in servers:
        data['state'] = 'Restored'; save(receipt, data); return
    if servers[NAME] != data['entry']: raise ValueError('Probe entry changed externally; preserved for review.')
    if digest(current) == data['afterSha256']:
        restored = before
    elif data['format'] == 'toml':
        block = data['block'].encode('utf-8')
        # Windows editors may normalize newlines while adding an unrelated table.
        if current.count(block) == 0:
            block = block.replace(b'\n', b'\r\n')
        if current.count(block) != 1: raise ValueError('Owned block changed; recovery requires review.')
        restored = current.replace(block, b'', 1)
    else:
        del servers[NAME]
        restored = (json.dumps(content, indent=2, ensure_ascii=False)+'\n').encode('utf-8')
    parse(restored, data['format'])
    if config.read_bytes() != current: raise ValueError('Concurrent edit detected before restoration.')
    atomic(config, restored)
    data['state'] = 'Restored'; save(receipt, data)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('action', choices=['prepare', 'apply', 'restore'])
    parser.add_argument('--receipt', type=Path, required=True)
    parser.add_argument('--config', type=Path)
    parser.add_argument('--format', choices=['json', 'toml'])
    parser.add_argument('--command'); parser.add_argument('--evidence')
    args = parser.parse_args()
    if args.action == 'prepare':
        if not all([args.config, args.format, args.command, args.evidence]): parser.error('prepare requires all config/entry fields')
        prepare(args.config,args.format,args.command,args.evidence,args.receipt)
    elif args.action == 'apply': apply(args.receipt)
    else: restore(args.receipt)
    print(args.action + ' completed; private backup retained at ' + str(args.receipt))
