import hashlib,json,pathlib,sys
from datetime import datetime,timezone
repo=pathlib.Path('D:/bim-agent-lab')
project=repo/'projects/revit-readiness'
sys.path.insert(0,str(project/'tests'))
from verify_mcp_evidence import expected_report,exact,read
workspace=repo/'.superpowers/sdd/2026-10-20-revit-mcp-bridge'
active=read(workspace/'active-shared-recovery.json')
root=pathlib.Path(active['runRoot'])/'live-recovery'
if root.exists(): raise RuntimeError('Preserve existing live attempt')
old=read(project/'docs/verification/shutdown-preparation-evidence/live/normal-recovery-wrapper/expectation.json')
def sha(p): return hashlib.sha256(pathlib.Path(p).read_bytes()).hexdigest()
descriptors=[]
for path in pathlib.Path('C:/Users/bring/AppData/Local/BimAgentLab/RevitBridge/sessions').glob('*.json'):
    d=read(path)
    if d['RevitPid']==178492: descriptors.append((path,d))
assert len(descriptors)==1,'Expected one new Revit bridge session'
path,descriptor=descriptors[0]
assert sha(old['modelPath'])==old['modelSha256']
assert sha(old['checklistPath'])==old['checklistSha256']
checklist=read(old['checklistPath'])
original=read(project/'docs/verification/required-information-evidence/BAL-info-complete.checklist.json')
original['ProjectFileName']=pathlib.Path(old['modelPath']).name
assert exact(checklist)==exact(original),'Save As checklist differs beyond its filename'
for source in old['oracleSources']:
    p=pathlib.Path(source['path'])
    if not p.is_absolute(): p=repo/p
    assert sha(p)==source['sha256'],'Original independent source changed'
old['preparedUtc']=datetime.now(timezone.utc).isoformat()
old['revitPid']=descriptor['RevitPid'];old['bridgeSessionId']=descriptor['BridgeSessionId']
old['expectedReport']=expected_report('complete',old['modelPath'],old['checklistPath'])
root.mkdir()
(root/'expectation.json').write_text(json.dumps(old,indent=2)+'\n',encoding='utf-8')
(root/'descriptor.json').write_bytes(path.read_bytes())
active['liveRoot']=str(root);active['revitPid']=descriptor['RevitPid'];active['bridgeSessionId']=descriptor['BridgeSessionId']
active['status']='Fresh independent expectation prepared; live wrapper about to run'
(workspace/'active-shared-recovery.json').write_text(json.dumps(active,indent=2)+'\n',encoding='utf-8')
print('PASS frozen independent expectation; PID',descriptor['RevitPid'],'session',descriptor['BridgeSessionId'])
