import hashlib, json, shutil, subprocess
from datetime import datetime, timezone
from pathlib import Path

repo = Path('D:/bim-agent-lab')
work = repo / '.superpowers/sdd/2026-10-20-revit-mcp-bridge'
active = work / 'active-r3-candidate.json'
state = json.loads(active.read_text(encoding='utf-8-sig'))
run, source = Path(state['runRoot']), Path(state['sourceRoot'])
project = repo / 'projects/revit-readiness'
def sha(p): return hashlib.sha256(Path(p).read_bytes()).hexdigest().upper()
def save(p, data):
    with Path(p).open('x', encoding='utf-8', newline='\n') as f:
        json.dump(data, f, indent=2); f.write('\n')
def git(*args): return subprocess.check_output(['git', *args], cwd=repo, text=True).strip()
assert git('rev-parse', 'HEAD') == state['buildCommit']
assert not git('status', '--porcelain'), 'Tracked checkout must still be clean at freeze'
assert not source.exists(), 'A frozen source package must never be overwritten'

inputs = []
for rel in git('ls-files', 'projects/revit-readiness/src', 'projects/revit-readiness/scripts',
               'projects/revit-readiness/third-party', 'global.json', 'Directory.Build.props',
               'NuGet.Config').splitlines():
    data = (repo / rel).read_bytes()
    committed = subprocess.check_output(['git', 'show', state['buildCommit'] + ':' + rel], cwd=repo)
    # Git worktree newline conversion is intentional; record both identities.
    assert data == committed or data.replace(b'\r\n', b'\n') == committed.replace(b'\r\n', b'\n'), rel
    inputs.append({'path':rel, 'worktreeSha256':sha(repo / rel),
                   'gitBlob':git('rev-parse', state['buildCommit']+':'+rel)})
save(run/'source-inputs.json', inputs)

relative = [Path('src/revit-addin/bin/Release/net48') / n for n in (
    'BimAgentLab.RevitReadiness.dll', 'BimAgentLab.Readiness.Core.dll',
    'BimAgentLab.RevitBridge.Runtime.dll', 'BimAgentLab.RevitBridge.Contract.dll')]
relative += [p.relative_to(project) for p in (project/'src/excel-report/bin/Release/net48').iterdir()
             if p.is_file() and (p.suffix == '.dll' or p.name in (
                'BimAgentLab.Readiness.Excel.exe','BimAgentLab.Readiness.Excel.exe.config'))]
for directory in ('third-party', 'samples/local/mcp-publish'):
    relative += [p.relative_to(project) for p in (project/directory).rglob('*') if p.is_file()]
relative += [Path('src/revit-addin/BimAgentLab.ReadinessBridge.addin.template'),
             Path('scripts/legacy-fixture-baseline.json')]
rows=[]
for rel in sorted(set(relative)):
    src, dest = project/rel, source/rel
    digest=sha(src); dest.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(src, dest)
    assert sha(dest) == sha(src) == digest
    rows.append({'path':rel.as_posix(), 'sha256':digest, 'bytes':dest.stat().st_size})
assert 'BIM_BRIDGE_DIAGNOSTICS' not in (project/'src/revit-addin/BimAgentLab.RevitReadiness.csproj').read_text()
save(run/'package-files.json', rows)

receipt = Path(state['priorReceipt'])
saved_receipt = json.loads(receipt.read_text(encoding='utf-8-sig'))
assert json.loads((receipt.parent/'state.json').read_text(encoding='utf-8-sig'))['State']=='Committed'
snapshots=run/'prior-installation'; snapshots.mkdir()
baseline=[]; recovery=[]
for i, op in enumerate(saved_receipt['Operations']):
    target=Path(op['Target']); assert sha(target)==op['After'], str(target)
    snapshot=snapshots/f'{i:03d}.bin'; shutil.copyfile(target, snapshot)
    assert sha(snapshot)==op['After']
    baseline.append({'target':str(target), 'sha256':op['After'], 'tag':op['Tag'],
                     'snapshot':snapshot.relative_to(run).as_posix()})
    for key, expected in [('Backup',op['Before']),('Staged',op['After'])]:
        if op.get(key):
            p=receipt.parent/op[key]; assert sha(p)==expected, str(p)
            recovery.append({'path':str(p),'sha256':expected})
shutil.copyfile(receipt,run/'prior-receipt.json')
save(run/'prior-installation.json', {'receiptPath':str(receipt), 'receiptSha256':sha(receipt),
    'targets':baseline,'recoveryFiles':recovery})
save(run/'freeze.json', {'buildCommit':state['buildCommit'],'sourceTree':git('rev-parse',state['buildCommit']+':projects/revit-readiness/src'),
    'normalBuild':True,'diagnosticCandidate':False,'frozenUtc':datetime.now(timezone.utc).isoformat(),
    'packageFiles':len(rows),'packageManifestSha256':sha(run/'package-files.json'),
    'sourceInputs':len(inputs),'sourceManifestSha256':sha(run/'source-inputs.json'),
    'priorTargets':len(baseline),'priorRecoveryFiles':len(recovery),'installedCandidate':False})
state['status']='FrozenNotInstalled'
active.write_text(json.dumps(state,indent=2)+'\n',encoding='utf-8')
print(f'PASS frozen normal candidate: {len(rows)} package files, {len(inputs)} source inputs; '
      f'prior installation {len(baseline)} targets and {len(recovery)} recovery files verified. No installed writes.')
