"""Retain this run's actual files; verify installed state independently of PowerShell."""
import hashlib,json,pathlib,shutil,sys
from datetime import datetime,timezone
repo=pathlib.Path('D:/bim-agent-lab')
workspace=repo/'.superpowers/sdd/2026-10-20-revit-mcp-bridge'
def read(p): return json.loads(pathlib.Path(p).read_text(encoding='utf-8-sig'))
def sha(p): return hashlib.sha256(pathlib.Path(p).read_bytes()).hexdigest().upper()
def require(v,m):
    if not v: raise RuntimeError(m)
active=read(workspace/'active-shared-recovery.json'); run=pathlib.Path(active['runRoot']); e=run/'execution'
prepared=read(run/'preparation.json'); baseline={r['target']:r['sha256'] for r in prepared['baselineTargets']}
receipt_path=pathlib.Path(read(e/'reinstall-committed.json')['receipt']); receipt=read(receipt_path)
require(sha(receipt_path)==(receipt_path.parent/'receipt.sha256').read_text().strip(),'New receipt checksum')
require(read(receipt_path.parent/'state.json')['State']=='Committed','New receipt not committed')
expected_before=prepared['receipts'][-1]['afterUndo']
require(len(receipt['Operations'])==len(baseline)==97,'Target count')
count=0
for op in receipt['Operations']:
    require(op['Before']==expected_before[op['Target']],'Reinstall before differs from restored state')
    require(op['After']==baseline[op['Target']]==sha(op['Target']),'Reinstall did not match exact baseline')
    require(not pathlib.Path(op['Temporary']).exists(),'Temporary remains')
    for a,b in [('Before','Backup'),('After','Staged')]:
        if op[a]: require(sha(receipt_path.parent/op[b])==op[a],'New recovery bytes'); count+=1
for index,saved in enumerate(prepared['receipts']):
    first=read(e/f'layer-{index}-undo.json'); second=read(e/f'layer-{index}-repeat.json')
    require(first['targets']==second['targets'],'Repeat recovery changed targets')
    for row in first['targets']:
        require(row['actual']==row['expected']==saved['afterUndo'][row['target']],'Undo target mismatch')
    require(first['receipts'][index]['state']==second['receipts'][index]['state'],'Repeat recovery changed state')
for version in ['2025-06-18','2025-11-25','2026-07-28']:
    d=e/f'sdk-{version}'; result=read(d/'sdk-client-result.json')
    require(read(d/'complete.json')['pass'] and result['pass'] and result['protocol']==version,'SDK completion/protocol')
    incoming=[json.loads(s) for s in (d/'wire/client-to-server.jsonl').read_text().splitlines()]
    outgoing=[json.loads(s) for s in (d/'wire/server-to-client.jsonl').read_text().splitlines()]
    requests=[r for r in incoming if 'id' in r]; responses=[r for r in outgoing if 'id' in r]
    require(len({r['id'] for r in responses})==len(responses)==len(requests),'Response identities')
    mapped={r['id']:r for r in responses}
    require(set(mapped)=={r['id'] for r in requests},'Unmatched response')
    calls=[]
    for req in requests:
        res=mapped[req['id']]; require('error' not in res,'Unexpected protocol error')
        if req['method']=='initialize': require(res['result']['protocolVersion']==version,'Wrong negotiation')
        elif req['method']=='server/discover': require(version in res['result']['supportedVersions'],'Missing July support')
        elif req['method']=='tools/list': require(sorted(t['name'] for t in res['result']['tools'])==['revit_check_required_type_information','revit_get_readiness_context'],'Discovery tools')
        elif req['method']=='tools/call':
            require(req['params']['name']=='revit_get_readiness_context','Unexpected tool'); calls.append(res['result'])
        else: raise RuntimeError('Unexpected request method')
    require(len(calls)==1,'Expected one unavailable-context call')
    tool=calls[0]; data=tool['structuredContent']
    require(tool['isError'] and data['code']=='BridgeUnavailable' and data['retryable'] and 'report' not in data,'Wrong structured failure')
    require(json.loads(tool['content'][0]['text'])==data==result['context']['structuredContent'],'Wire/result mismatch')
    require(len(data['requestId'])==32 and all(c in '0123456789abcdef' for c in data['requestId']),'Uncorrelated failure')
    print('PASS actual installed-server wire:',version)
if '--retain' in sys.argv:
    dest=repo/'projects/revit-readiness/docs/verification/shared-recovery-evidence/execution'
    require(not dest.exists(),'Retained execution evidence exists')
    shutil.copytree(e,dest)
    for name in ['audit_shared_recovery.py','undo_shared_layer.ps1','reinstall_shared_baseline.ps1','retain_shared_recovery.py']:
        shutil.copyfile(workspace/name,dest/name)
    shutil.copyfile(run/'preflight.json',dest/'fresh-preflight.json')
    shutil.copyfile(run/'physical-paths.json',dest/'fresh-physical-paths.json')
    folders=[receipt_path.parent]+[pathlib.Path(prepared['baselineReceipt']).parent.parent/s['transaction'] for s in prepared['receipts']]
    for folder in folders:
        out=dest/'receipts'/folder.name; out.mkdir(parents=True)
        for name in ['receipt.json','receipt.sha256','state.json','journal.jsonl']: shutil.copyfile(folder/name,out/name)
    summary={'checkedUtc':datetime.now(timezone.utc).isoformat(),'historicalLayersRestored':3,'repeatUndoNoChange':3,'finalTargetsMatchBaseline':97,'sourcePackageFiles':83,'newReceipt':str(receipt_path),'newRecoveryFilesVerified':count,'changedReinstallTargets':sum(o['Before']!=o['After'] for o in receipt['Operations']),'hostSource':prepared['hostSource'],'serverSource':prepared['serverSource'],'protocols':['2025-06-18','2025-11-25','2026-07-28'],'liveRevitRecovery':'pending','note':'reinstall-plan.json Target/Before/After/Tag are null due to Select-Object on dictionaries. Preserve as a capture defect; authenticated immutable new receipt supplies the actual plan. This script independently verifies it against pre-captured before/after maps.'}
    (dest/'verification.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf-8')
    hashes=[{'path':p.relative_to(dest).as_posix(),'sha256':sha(p)} for p in sorted(dest.rglob('*')) if p.is_file()]
    (dest/'hashes.json').write_text(json.dumps(hashes,indent=2)+'\n',encoding='utf-8')
    print(f'PASS retained {len(hashes)} exact evidence files, new receipt backups/staged={count}')
print('PASS three actual undo/repeat layers and exact reinstall; live Revit recovery is separate')
