"""Read-only, independent hash audit for this machine's historical recovery run."""
import hashlib, json, pathlib, sys
from datetime import datetime, timezone

repo = pathlib.Path('D:/bim-agent-lab')
def read(p): return json.loads(pathlib.Path(p).read_text(encoding='utf-8-sig'))
def digest(p):
    p = pathlib.Path(p)
    return hashlib.sha256(p.read_bytes()).hexdigest().upper() if p.exists() else None
def require(value, message):
    if not value: raise RuntimeError(message)
active = read(repo / '.superpowers/sdd/2026-10-20-revit-mcp-bridge/active-shared-recovery.json')
run = pathlib.Path(active['runRoot'])
prepared = read(run / 'preparation.json')
root = pathlib.Path('C:/Users/bring/.bim-agent-lab/RevitReadiness/InstallRecovery')
mode = sys.argv[1]
out = pathlib.Path(sys.argv[2])
require(not out.exists(), 'Evidence output already exists')
package = read(repo / 'projects/revit-readiness/docs/verification/shared-recovery-evidence/preparation/package-manifest.json')
for f in package:
    require(digest(pathlib.Path(active['sourceRoot']) / f['path']) == f['sha256'].upper(), 'Package changed: '+f['path'])
for row in prepared['baselineTargets']:
    require(digest(run / row['snapshot']) == row['sha256'], 'Baseline snapshot changed')
records=[]
for saved in prepared['receipts']:
    folder=root/saved['transaction']; path=folder/'receipt.json'; r=read(path)
    require(digest(path) == saved['receiptSha256'] == (folder/'receipt.sha256').read_text().strip(), 'Receipt changed')
    count=0
    for op in r['Operations']:
        for hash_key, path_key in [('Before','Backup'),('After','Staged')]:
            if op[hash_key]:
                require(digest(folder/op[path_key]) == op[hash_key], 'Recovery bytes changed'); count+=1
        require(not pathlib.Path(op['Temporary']).exists(), 'Unexpected temporary')
    state=read(folder/'state.json')
    records.append({'transaction':r['Transaction'],'receiptSha256':digest(path),'state':state,'verifiedRecoveryFiles':count})
if mode in ('before','final'):
    expected={r['target']:r['sha256'] for r in prepared['baselineTargets']}
else:
    index=int(mode)
    expected=prepared['receipts'][index]['afterUndo']
    for earlier in records[:index+1]: require(earlier['state']['State']=='Restored','Undo did not finish')
    for saved in prepared['receipts'][:index+1]:
        for directory in saved['createdDirectories']: require(not pathlib.Path(directory).exists(),'Created directory remains')
targets=[]
for target, expected_hash in expected.items():
    actual=digest(target)
    require(actual==expected_hash,'Target mismatch: '+target)
    targets.append({'target':target,'expected':expected_hash,'actual':actual})
inventories=[]
for row in targets:
    p=pathlib.Path(row['target'])
    if p.name=='installed-files.json' and row['actual']:
        entries=read(p)
        for entry in entries:
            require(digest(p.parent/entry['File'])==entry['Sha256'],'Inventory payload mismatch: '+entry['File'])
        inventories.append({'target':str(p),'entries':len(entries),'sha256':row['actual']})
if mode=='before': require(all(r['state']['State']=='Committed' for r in records),'Historical layer no longer Committed')
if mode=='final': require(all(r['state']['State']=='Restored' for r in records),'Historical recovery incomplete')
result={'checkedUtc':datetime.now(timezone.utc).isoformat(),'mode':mode,'packageFiles':len(package),'baselineSnapshots':len(prepared['baselineTargets']),'receipts':records,'targets':targets,'inventories':inventories,'createdDirectories':[{ 'path':d,'exists':pathlib.Path(d).exists()} for s in prepared['receipts'] for d in s['createdDirectories']]}
out.parent.mkdir(parents=True,exist_ok=True)
out.write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
print(f'PASS {mode}: {len(targets)} target states, {len(package)} package files, 97 snapshots, 3 receipt/backup sets, {len(inventories)} inventories')
