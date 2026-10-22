import hashlib,json,shutil,subprocess
from pathlib import Path
from datetime import datetime,timezone
repo=Path('D:/bim-agent-lab'); work=repo/'.superpowers/sdd/2026-10-20-revit-mcp-bridge'
def read(p):return json.loads(Path(p).read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def save(p,d):
    with Path(p).open('x',encoding='utf-8',newline='\n') as f:json.dump(d,f,indent=2);f.write('\n')
active=read(work/'active-r3-candidate.json'); run=Path(active['runRoot'])
checks=read(run/'offline-checks.json')
assert checks['allExitCode']==checks['readinessExitCode']==0
assert 'PASS available offline suites; desktop/Revit acceptance is separate.' in (run/'all.txt').read_text(encoding='utf-8-sig')
for protocol in checks['frozenServerProtocols']:
    result=read(run/'frozen-offline'/protocol/'sdk-client-result.json')
    assert result['pass'] is True and result['expected']=='BridgeUnavailable' and result['protocol']==protocol
package=read(run/'package-files.json')
for row in package: assert sha(Path(active['sourceRoot'])/row['path']).upper()==row['sha256']
inputs=read(run/'source-inputs.json')
for row in inputs:assert sha(repo/row['path']).upper()==row['worktreeSha256']
prior=read(run/'prior-installation.json')
for row in prior['targets']:assert sha(row['target']).upper()==row['sha256']
for row in prior['recoveryFiles']:assert sha(row['path']).upper()==row['sha256']
old=repo/'projects/revit-readiness/docs/verification/r3-followup-evidence'
old_hashes=read(old/'hashes.json')
for row in old_hashes:assert sha(old/row['path'])==row['sha256']
assert len(old_hashes)==53
extra=[]
for rel in ['projects/revit-readiness/global.json']:
    data=(repo/rel).read_bytes()
    committed=subprocess.check_output(['git','show',active['buildCommit']+':'+rel],cwd=repo)
    assert data.replace(b'\r\n',b'\n')==committed.replace(b'\r\n',b'\n')
    extra.append({'path':rel,'sha256':sha(repo/rel),'gitBlob':subprocess.check_output(['git','rev-parse',active['buildCommit']+':'+rel],cwd=repo,text=True).strip()})
save(run/'additional-build-inputs.json',extra)
save(run/'verification.json',{'checkedUtc':datetime.now(timezone.utc).isoformat(),
    'buildCommit':active['buildCommit'],'frozenPackageFilesUnchanged':len(package),
    'sourceInputsUnchanged':len(inputs),'additionalBuildInputs':len(extra),
    'priorInstalledTargetsUnchanged':len(prior['targets']),'priorRecoveryBytesVerified':len(prior['recoveryFiles']),
    'originalFollowUpEvidenceUnchanged':len(old_hashes),'allOfflineSuites':True,'readinessExcelIsolation':True,
    'frozenServerDiscoveryAndBridgeUnavailable':checks['frozenServerProtocols'],
    'strictJulyAndLegacyRejection':'Passed in All suite; separate from frozen server compatible calls',
    'ca1416Warnings':4,'newInstallation':False,'newLiveAcceptance':False,'fullAcceptance':'Open'})
pub=repo/'projects/revit-readiness/docs/verification/r3-current-candidate-evidence/preparation'
pub.mkdir(parents=True,exist_ok=False)
for name in ['freeze.json','package-files.json','source-inputs.json','additional-build-inputs.json',
             'prior-installation.json','prior-receipt.json','preflight.json','prior-physical-paths.json',
             'host-rebuild.txt','server-rebuild.txt','build.txt','all.txt','readiness.txt',
             'dotnet-info.txt','offline-checks.json','verification.json']:
    shutil.copyfile(run/name,pub/name)
for protocol in checks['frozenServerProtocols']:
    shutil.copyfile(run/(protocol+'.txt'),pub/(protocol+'.txt'))
for folder in ['case-preparation','frozen-offline']:
    shutil.copytree(run/folder,pub/folder)
scripts=pub/'capture-scripts';scripts.mkdir()
for name in ['freeze_r3_candidate.py','prepare_r3_cases.py','install_frozen_r3_candidate.ps1',
             'retain_r3_candidate_preparation.py']:
    shutil.copyfile(work/name,scripts/name)
(pub/'README.md').write_text('''# Frozen normal candidate preparation

These records establish preparation and offline checks, not installation or live desktop acceptance. Read [the candidate record](../../2026-10-22-r3-current-candidate.md).

- `freeze.json`, `package-files.json`, `source-inputs.json` and `additional-build-inputs.json`: clean build commit, frozen outputs and source identities. No binary or proprietary RVT is published here.
- Build logs and `dotnet-info.txt`: normal project selection, locked build/publish, SDK/runtime environment and four retained CA1416 warnings.
- `prior-installation.json`, `prior-receipt.json`, `preflight.json` and physical paths: existing installation/recovery checks and the proposed 97-target, 17-change plan. No candidate recovery receipt or installed freeze exists at this checkpoint.
- `all.txt`, `readiness.txt`: fresh full offline suites, strict July/legacy rejection and manual-export regressions.
- `frozen-offline`: SDK discovery and correlated BridgeUnavailable against the exact frozen compatible server, with Revit closed, on all three supported revisions. This is neither a successful Revit read nor either desktop's acceptance.
- `case-preparation`: five independent expected reports, unchanged sample hashes and the pending capture checklist. PID/session and native NC1 absence confirmation must be established before live calls.
- `capture-scripts`: exact local preparation/retention helpers. The installation wrapper was prepared and syntax-checked only; it was not executed. These are historical, machine-specific capture scripts, not a replacement for the supported installer.
- `verification.json`: implementing-agent rechecks after the suites; not an independent review.

The private package and rollback bytes remain outside Git. The original 53 follow-up evidence files remain unchanged. Later installation and live captures must use separate directories/manifests; do not rewrite this preparation snapshot.
''',encoding='utf-8',newline='\n')
files=[{'path':p.relative_to(pub).as_posix(),'bytes':p.stat().st_size,'sha256':sha(p)} for p in sorted(pub.rglob('*')) if p.is_file()]
save(pub/'hashes.json',files)
active['status']='FrozenVerifiedAwaitingConnections'
(work/'active-r3-candidate.json').write_text(json.dumps(active,indent=2)+'\n',encoding='utf-8')
print(f'PASS {len(files)} retained preparation hashes; package {len(package)}, prior targets {len(prior["targets"])}, recovery {len(prior["recoveryFiles"])}, old follow-up {len(old_hashes)} verified unchanged. Installation and live acceptance pending.')
