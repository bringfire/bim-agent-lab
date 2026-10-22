"""Prepare isolated NC1/NC2 copies with a durable ownership inventory."""
import argparse
import copy
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
from datetime import datetime, timezone

MODEL='BAL-info-complete.rvt'
SIDECAR='BAL-info-complete.checklist.json'
PATHS={f'{case}/{name}' for case in ('nc1','nc2') for name in (MODEL,SIDECAR)}

def digest(path):
    h=hashlib.sha256()
    with Path(path).open('rb') as f:
        for block in iter(lambda:f.read(1024*1024),b''):h.update(block)
    return h.hexdigest()

def encoded(value):return (json.dumps(value,indent=2,ensure_ascii=False)+'\n').encode('utf-8')

def write_new(path,data):
    with Path(path).open('xb') as f:f.write(data);f.flush();os.fsync(f.fileno())

def is_link(path):return path.is_symlink() or getattr(path,'is_junction',lambda:False)()

def validate_source(checklist):
    if checklist['SchemaVersion']!=1 or checklist['ProjectFileName']!=MODEL:raise ValueError('Expected the complete sample sidecar')
    if len(checklist['Cases'])!=16 or len(checklist['Requirements'])!=6:raise ValueError('Expected 16 cases and six requirements')
    wa=[c for c in checklist['Cases'] if c['CaseId']=='BAL-INFO-WA-01']
    act=[r for r in checklist['Requirements'] if r['Source']=='BAL-TYPE-INFO-v1: BAL INFO Ceiling ACT']
    if len(wa)!=1 or len(act)!=1 or not re.fullmatch(r'[0-9a-f-]{36}-[0-9a-f]{8}',wa[0]['UniqueId']):raise ValueError('Missing or ambiguous control identities')
    return wa[0],act[0]

def prepare(source, output, fail_after=None):
    source,output=Path(source).resolve(),Path(output).absolute()
    if output.exists():
        state='Prepared'
        try:
            marker=json.loads((output/'complete.json').read_text(encoding='utf-8'))
            if marker.get('state')=='Ready':state='Ready'
        except (OSError,ValueError):pass
        raise FileExistsError(f'Output already exists ({state}); inspect or use owned cleanup, never overwrite')
    model,sidecar=source/MODEL,source/SIDECAR
    model_hash=digest(model); sidecar_bytes=sidecar.read_bytes()
    checklist=json.loads(sidecar_bytes.decode('utf-8-sig'));wa,act=validate_source(checklist)
    absent=wa['UniqueId'][:-8]+'7fffffff'
    if absent in {c['UniqueId'] for c in checklist['Cases']}:raise ValueError('Injected identity is already declared')
    nc1,nc2=copy.deepcopy(checklist),copy.deepcopy(checklist)
    next(c for c in nc1['Cases'] if c['CaseId']=='BAL-INFO-WA-01')['UniqueId']=absent
    nc2['Requirements']=[r for r in nc2['Requirements'] if r['TypeUniqueId']!=act['TypeUniqueId']]
    payloads={'nc1':encoded(nc1),'nc2':encoded(nc2)}
    # This records declared absence, not a claim that Revit has independently checked it.
    receipt={'schema':1,'root':str(output.resolve()),'state':'Prepared','preparedUtc':datetime.now(timezone.utc).isoformat(),
        'sourceModel':str(model),'sourceModelSha256':model_hash,'sourceChecklist':str(sidecar),
        'sourceChecklistSha256':hashlib.sha256(sidecar_bytes).hexdigest(),
        'absentUniqueId':absent,'absentElementId':'2147483647','absenceConfirmed':False,
        'originalCase':wa,'removedRequirement':act,'files':[]}
    for case in ('nc1','nc2'):
        receipt['files'].extend([{'path':case+'/'+MODEL,'bytes':model.stat().st_size,'sha256':model_hash},
            {'path':case+'/'+SIDECAR,'bytes':len(payloads[case]),'sha256':hashlib.sha256(payloads[case]).hexdigest()}])
    output.mkdir()  # Parent must already exist; never create an uncontrolled directory chain.
    write_new(output/'preparation.json',encoded(receipt))  # Durable recovery data precedes copies.
    for case in ('nc1','nc2'):
        folder=output/case;folder.mkdir()
        with model.open('rb') as incoming,(folder/MODEL).open('xb') as target:
            shutil.copyfileobj(incoming,target,1024*1024);target.flush();os.fsync(target.fileno())
        if case=='nc1' and fail_after=='first-copy':raise RuntimeError('Injected interruption after first copy')
        write_new(folder/SIDECAR,payloads[case])
        if case=='nc1' and fail_after=='first-sidecar':raise RuntimeError('Injected interruption after first sidecar')
    for file in receipt['files']:
        target=output/file['path']
        if target.stat().st_size!=file['bytes'] or digest(target)!=file['sha256']:raise ValueError('Copy verification failed; keep Prepared evidence')
    if digest(model)!=model_hash or sidecar.read_bytes()!=sidecar_bytes:raise ValueError('Source changed during preparation')
    marker={'state':'Ready','preparedReceiptSha256':digest(output/'preparation.json'),'completedUtc':datetime.now(timezone.utc).isoformat()}
    write_new(output/'complete.tmp',encoded(marker));os.replace(output/'complete.tmp',output/'complete.json')
    return receipt

def cleanup(output, fail_after=None):
    output=Path(output).absolute()
    if is_link(output):raise ValueError('Refuse linked output root')
    receipt=json.loads((output/'preparation.json').read_text(encoding='utf-8'))
    if receipt.get('schema')!=1 or receipt.get('root')!=str(output.resolve()):raise ValueError('Wrong ownership root')
    if len(receipt['files'])!=4 or {f['path'] for f in receipt['files']}!=PATHS:raise ValueError('Unexpected owned paths')
    marker=output/'complete.json'
    if marker.exists():
        state=json.loads(marker.read_text(encoding='utf-8'))
        if state.get('state')!='Ready' or state.get('preparedReceiptSha256')!=digest(output/'preparation.json'):
            raise ValueError('Completion/ownership evidence changed; retained')
    allowed={'preparation.json','complete.json','nc1','nc2'}
    if any(p.name not in allowed or is_link(p) for p in output.iterdir()):raise ValueError('Foreign entry; retained')
    for case in ('nc1','nc2'):
        folder=output/case
        if folder.exists() and (not folder.is_dir() or any(p.name not in (MODEL,SIDECAR) or not p.is_file() or is_link(p) for p in folder.iterdir())):
            raise ValueError('Foreign control entry; retained')
    targets=[]
    for entry in receipt['files']:
        target=output/entry['path']
        if target.resolve().parent.parent!=output.resolve() or is_link(target):raise ValueError('Path escapes owned root')
        if target.exists():
            if target.stat().st_size!=entry['bytes'] or digest(target)!=entry['sha256']:raise ValueError('Foreign modification; retained')
            targets.append((target,entry))
    # Entire inventory/bytes preflight first; recheck each remaining file immediately before deletion.
    for index,(target,entry) in enumerate(targets):
        if digest(target)!=entry['sha256']:raise ValueError('File changed during cleanup; retained')
        target.unlink()
        if index==0 and fail_after=='first-delete':raise RuntimeError('Injected cleanup interruption')
    for case in ('nc1','nc2'):
        folder=output/case
        if folder.exists():folder.rmdir()
    (output/'complete.json').unlink(missing_ok=True)
    (output/'preparation.json').unlink()
    output.rmdir()

if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('action',choices=['prepare','cleanup']);parser.add_argument('--source');parser.add_argument('--output',required=True)
    parser.add_argument('--fail-after',choices=['first-copy','first-sidecar','first-delete'],help='Verification-only interruption injection')
    args=parser.parse_args()
    if args.action=='prepare':
        if not args.source:parser.error('prepare requires --source')
        result=prepare(args.source,args.output,args.fail_after)
        print(json.dumps({'state':'Ready','output':str(Path(args.output).resolve()),'absentUniqueId':result['absentUniqueId'],'absenceConfirmed':False}))
    else:cleanup(args.output,args.fail_after);print('Owned control files removed; source preserved')
