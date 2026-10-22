import copy, hashlib, json, sys
from datetime import datetime, timezone
from pathlib import Path
repo=Path('D:/bim-agent-lab'); project=repo/'projects/revit-readiness'
sys.path.insert(0,str(project/'tests'))
from verify_mcp_evidence import expected_report, exact, EVIDENCE, ABSENT_ID
active=json.loads((repo/'.superpowers/sdd/2026-10-20-revit-mcp-bridge/active-r3-candidate.json').read_text(encoding='utf-8-sig'))
run=Path(active['runRoot']); root=run/'case-preparation'
root.mkdir(exist_ok=False)
principal=project/'samples/local/mcp-live-task7/a10630e39dae448ba55abbfe0bc7ab6b/models'
controls=project/'samples/local/mcp-task8/f82eea66e10b43ebaafd7fe40ea4bc87/controls'
def sha(p): return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def read(p): return json.loads(Path(p).read_text(encoding='utf-8-sig'))
def save(p,d):
    with Path(p).open('x',encoding='utf-8',newline='\n') as f: json.dump(d,f,indent=2);f.write('\n')
rows=[]
for case in ('complete','wall-defects','ceiling-negative','nc1','nc2'):
    stem='BAL-info-'+(case if case not in ('nc1','nc2') else 'complete')
    folder=controls/case if case in ('nc1','nc2') else principal
    model=folder/(stem+'.rvt'); checklist=folder/(stem+'.checklist.json')
    authored=read(EVIDENCE/(stem+'.checklist.json'))
    if case=='nc1':
        next(c for c in authored['Cases'] if c['CaseId']=='BAL-INFO-WA-01')['UniqueId']=ABSENT_ID
    if case=='nc2':
        authored['Requirements']=[r for r in authored['Requirements'] if r['Source']!='BAL-TYPE-INFO-v1: BAL INFO Ceiling ACT']
    assert exact(read(checklist))==exact(authored), case+' unexpected checklist mutation'
    row={'case':case,'preparedUtc':datetime.now(timezone.utc).isoformat(),
        'buildCommit':active['buildCommit'],'modelPath':str(model),'modelSha256':sha(model),
        'checklistPath':str(checklist),'checklistSha256':sha(checklist),
        'expectedReport':expected_report(case,str(model),str(checklist)),
        'status':'Prepared, no live calls; bind fresh PID/session and confirm model before testing',
        'absenceConfirmed':False if case=='nc1' else None}
    save(root/(case+'.json'),row); rows.append(row)
assert rows[0]['modelSha256']==rows[3]['modelSha256']==rows[4]['modelSha256']
sources=[project/'tests/verify_mcp_evidence.py', project/'scripts/prepare_mcp_controls.py']
sources += sorted(EVIDENCE.glob('*.checklist.json')) + sorted(EVIDENCE.glob('*.tsv'))
save(root/'sources.json',[{'path':p.relative_to(repo).as_posix(),'sha256':sha(p)} for p in sources])
save(root/'capture-plan.json',{
    'buildCommit':active['buildCommit'],'installedCandidateFrozen':False,
    'candidatePolicy':'Do not replace/rebuild installed acceptance binaries during this sequence. Any candidate change starts a new sequence.',
    'baselineRequired':['application version','actual AI model label','UI surface','permission mode','full relevant launch settings','negotiated protocol','installed hashes','receipt','fresh Revit PID/session'],
    'perCaseRequired':['actual user prompt','actual public answer and message identity/time','tool request ID','unchanged wire and tool result','independent comparison','separate presentation assessment','modified flags and post-close file hashes'],
    'desktops':[{ 'application':app,'discovery':'Pending','baseline':'Pending',
        'cases':[{'case':r['case'],'data':'Pending','presentation':'Pending'} for r in rows],
        'unexpiredStaleRejection':'Pending','freshContextRecovery':'Pending'} for app in ('Codex desktop','Claude Desktop Chat')],
    'supplementalJulySdk':'Pending live; offline frozen binary control separate',
    'nativeHostChecks':'Pending normal candidate; diagnostic evidence remains separately labelled',
    'desktopStop':'Unverified; explicit SDK controls are not a substitute',
    'finalAcceptance':'Open'})
print('PASS five independent case expectations; model/sidecar hashes frozen, exact control mutations checked; no Revit/desktop calls.')
