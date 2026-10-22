"""Recoverable lab MCP registration. Receipts contain private config backups; keep ignored/local."""
import argparse
import base64
import hashlib
import json
import os
from pathlib import Path
import tempfile
import tomllib

NAME = 'bim-agent-lab-protocol-probe'
READINESS_NAME = 'bim-agent-lab-revit-readiness'


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


def prepare(config, format, command, evidence, receipt, *, name=NAME, revit_pid=None):
    if name not in (NAME, READINESS_NAME): raise ValueError('Unknown lab entry name.')
    if name == READINESS_NAME:
        if type(revit_pid) is not int or not 0 < revit_pid <= 2147483647:
            raise ValueError('Readiness registration requires a positive Windows Revit PID.')
    elif revit_pid is not None: raise ValueError('The synthetic probe does not select Revit.')
    config = Path(config).resolve(); receipt = Path(receipt).resolve()
    if receipt.exists(): raise ValueError('Receipt already exists; restore or inspect it first.')
    # Current desktop configuration must exist; do not invent its other settings.
    before = config.read_bytes(); data = parse(before, format)
    key = 'mcpServers' if format == 'json' else 'mcp_servers'
    if name in data.get(key, {}): raise ValueError('Existing lab entry is not owned by this operation.')
    arguments = ['--wire-evidence', evidence] if name == NAME else ['--revit-pid', str(revit_pid), '--wire-evidence-root', evidence]
    entry = {'command': command, 'args': arguments}
    block = ''
    if format == 'json':
        data.setdefault(key, {})[name] = entry
        after = (json.dumps(data, indent=2, ensure_ascii=False) + '\n').encode('utf-8')
    else:
        label = 'TEMPORARY PROBE' if name == NAME else 'READINESS'
        block = ('\n# BEGIN BIM AGENT LAB '+label+'\n[mcp_servers."'+name+'"]\ncommand = '
                 + json.dumps(command) + '\nargs = '+json.dumps(entry['args'])
                 + '\n# END BIM AGENT LAB '+label+'\n')
        after = before + block.encode('utf-8')
    parse(after, format)
    receipt.parent.mkdir(parents=True, exist_ok=True)
    record = {'state': 'Prepared', 'config': str(config), 'format': format, 'key': key, 'name': name,
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
    # Receipts from CP0 predate the explicit name; keep their original identity.
    name = data.get('name', NAME)
    if name not in (NAME, READINESS_NAME): raise ValueError('Unknown receipt entry name.')
    if name not in servers:
        data['state'] = 'Restored'; save(receipt, data); return
    if servers[name] != data['entry']: raise ValueError('Lab entry changed externally; preserved for review.')
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
        del servers[name]
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
    parser.add_argument('--name', choices=[NAME, READINESS_NAME], default=NAME)
    parser.add_argument('--revit-pid', type=int)
    args = parser.parse_args()
    if args.action == 'prepare':
        if not all([args.config, args.format, args.command, args.evidence]): parser.error('prepare requires all config/entry fields')
        prepare(args.config,args.format,args.command,args.evidence,args.receipt,name=args.name,revit_pid=args.revit_pid)
    elif args.action == 'apply': apply(args.receipt)
    else: restore(args.receipt)
    print(args.action + ' completed; private backup retained at ' + str(args.receipt))
