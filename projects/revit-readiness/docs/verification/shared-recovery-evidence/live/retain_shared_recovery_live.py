import hashlib,json,pathlib,shutil
from datetime import datetime,timezone
repo=pathlib.Path('D:/bim-agent-lab'); ws=repo/'.superpowers/sdd/2026-10-20-revit-mcp-bridge'
def read(p): return json.loads(pathlib.Path(p).read_text(encoding='utf-8-sig'))
def sha(p): return hashlib.sha256(pathlib.Path(p).read_bytes()).hexdigest()
a=read(ws/'active-shared-recovery.json');root=pathlib.Path(a['liveRoot'])
dest=repo/'projects/revit-readiness/docs/verification/shared-recovery-evidence/live'
assert not dest.exists()
expect=read(root/'expectation.json');result=read(root/'wrapper/capture/check.json')['structuredContent']
report=result['report'];assert report['ModifiedBefore'] is False and report['ModifiedAfter'] is False
assert sha(expect['modelPath'])==expect['modelSha256'] and sha(expect['checklistPath'])==expect['checklistSha256']
assert all(not row['Redirected'] for row in read(root/'desktop-physical-paths.json'))
newreceipt=pathlib.Path(a['currentReceipt']);receipt=read(newreceipt)
assert sha(newreceipt).upper()==(newreceipt.parent/'receipt.sha256').read_text().strip()
for op in receipt['Operations']:
    assert sha(op['Target']).upper()==op['After']
    for field,relative in [('Before','Backup'),('After','Staged')]:
        if op[field]:assert sha(newreceipt.parent/op[relative]).upper()==op[field]
shutil.copytree(root,dest)
for name in ['prepare_shared_recovery_live.py','rebind_desktops_after_recovery.py','retain_shared_recovery_live.py']:shutil.copyfile(ws/name,dest/name)
summary={'verifiedUtc':datetime.now(timezone.utc).isoformat(),'sourceCheckpoint':'ef47b20','revitPid':a['revitPid'],'bridgeSessionId':a['bridgeSessionId'],'documentPath':expect['modelPath'],'checklistPath':expect['checklistPath'],'modelSha256':sha(expect['modelPath']),'checklistSha256':sha(expect['checklistPath']),'protocol':'2026-07-28','expectedReportSource':'Authored independent oracle plus original native schedules/checklists; no report-derived expected answers','examined':report['ExaminedInstances'],'findings':report['Evaluation']['FindingCount'],'coverageComplete':report['CoverageComplete'],'evaluationComplete':report['Evaluation']['Complete'],'modifiedBefore':report['ModifiedBefore'],'modifiedAfter':report['ModifiedAfter'],'installedTargetsVerified':len(receipt['Operations']),'newReceipt':str(newreceipt),'desktopBindingsUpdated':True,'desktopCallsAfterRebind':'not performed','humanAction':'Author replied ready after opening the specified model and starting the bridge','fullR3':'pending'}
(dest/'verification.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf-8')
records=[{'path':p.relative_to(dest).as_posix(),'sha256':sha(p)} for p in sorted(dest.rglob('*')) if p.is_file()]
(dest/'hashes.json').write_text(json.dumps(records,indent=2)+'\n',encoding='utf-8')
print('PASS live report, unchanged disk inputs,97 installed files/new recovery bytes;',len(records),'retained hashes')
