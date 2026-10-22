"""Independent five-case BIM comparison; no production evaluator imports.

Values/counts are the authored answer sheet. Identities come from the original
checklists, membership and displayed values from native Revit schedule exports.
Untouched optional empty values are expected null; deliberately cleared values
are expected empty strings. The fixture's SetText skips already-empty values,
but calls Set("") when clearing a populated value. Do not trim/coalesce actual
report values or regenerate this oracle
from a new returned report. The fixture API value expectation is checked, not
claimed to be distinguishable from an empty string by a displayed schedule.
"""
import argparse
import copy
import hashlib
import json
import re
from datetime import datetime
from pathlib import Path, PureWindowsPath
from verify_information_evidence import schedule_rows
EVIDENCE = Path(__file__).resolve().parents[1] / 'docs/verification/required-information-evidence'
ABSENT_ID = '6e5a3689-c170-4e43-b22b-e80a27806cdb-7fffffff'
CASES = ('complete','wall-defects','ceiling-negative','nc1','nc2')
ROWS = [
    ('Walls','BAL INFO Wall A','BAL-INFO-WA',3,'W-A','60 min','DEMO-ASM-01'),
    ('Walls','BAL INFO Wall B','BAL-INFO-WB',3,'W-B','60 min','DEMO-ASM-01'),
    ('Walls','BAL INFO Wall C','BAL-INFO-WC',3,'W-C',None,None),
    ('Walls','BAL INFO Wall D','BAL-INFO-WD',3,'W-D',None,None),
    ('Ceilings','BAL INFO Ceiling ACT','BAL-INFO-CACT',2,'C-ACT',None,None),
    ('Ceilings','BAL INFO Ceiling GWB','BAL-INFO-CGWB',2,'C-GWB',None,None),
]
PARAMETERS = {'Code':'BuiltInParameter.ALL_MODEL_TYPE_MARK','FireRating':'BuiltInParameter.FIRE_RATING',
              'AssemblyReference':'SharedParameter:cd425f17-1bb4-4d3f-a088-902728147491'}
SCOPE = 'Explicit checklist element UniqueIds; host basic walls and ceilings on the declared level, New in the declared phase. Types are checked once; links and other elements excluded. No view/selection filter or result cap.'

def read(path):return json.loads(Path(path).read_text(encoding='utf-8-sig'))

def normalized(value):
    if isinstance(value,dict):return {k:normalized(v) for k,v in value.items()}
    if isinstance(value,list):return sorted([normalized(v) for v in value],key=lambda v:json.dumps(v,sort_keys=True))
    return value

def exact(value):return json.dumps(normalized(value),sort_keys=True,ensure_ascii=False)

def expected_report(case, model_path, checklist_path):
    assert case in CASES, 'Unknown case'
    principal = case if case in CASES[:3] else 'complete'
    stem='BAL-info-'+principal
    checklist=read(EVIDENCE/(stem+'.checklist.json'))
    native=schedule_rows(EVIDENCE/(stem+'-walls.tsv'))+schedule_rows(EVIDENCE/(stem+'-ceilings.tsv'))
    assert len(native)==16 and len({r['CaseId'] for r in native})==16
    by_case={r['CaseId']:r for r in native}
    identities={r['CaseId']:r for r in checklist['Cases']}
    requirements={r['Source']:r for r in checklist['Requirements']}
    assert len(identities)==16 and len(requirements)==6
    missing = {'complete':set(),'wall-defects':{('BAL INFO Wall B','FireRating'),('BAL INFO Wall D','Code')},
               'ceiling-negative':{('BAL INFO Wall B','FireRating'),('BAL INFO Wall D','Code'),('BAL INFO Ceiling ACT','Code')},
               'nc1':set(),'nc2':set()}[case]
    types, affected, unexamined = [], [], []
    for category,name,prefix,count,code,rating,assembly in ROWS:
        source='BAL-TYPE-INFO-v1: '+name
        requirement=requirements[source]
        assert requirement['Category']==category
        refs=[]
        for i in range(1,count+1):
            cid=f'{prefix}-{i:02d}'
            assert by_case[cid]['Type']==name
            assert set(identities[cid])=={'CaseId','ElementId','UniqueId'}
            if case!='nc1' or cid!='BAL-INFO-WA-01':refs.append(copy.deepcopy(identities[cid]))
        unknown=case=='nc2' and name=='BAL INFO Ceiling ACT'
        row={'TypeUniqueId':requirement['TypeUniqueId'],'TypeName':name,'Category':category,
             'Status':'Unexamined' if unknown else 'NeedsReview' if any(n==name for n,f in missing) else 'Pass',
             'Elements':refs,'Fields':[]}
        for field,value in zip(PARAMETERS,(code,rating,assembly)):
            required=field=='Code' or name in ('BAL INFO Wall A','BAL INFO Wall B')
            assert requirement[field+'Required'] is required
            is_missing=(name,field) in missing
            if is_missing:value=''
            for i in range(1,count+1):
                cell=by_case[f'{prefix}-{i:02d}'].get(field)
                if cell is not None: assert cell.strip()==(value or ''), 'Native value disagrees with authored answer sheet'
            status,reason=('Unexamined','NoMatchingRequirement') if unknown else ('Missing','BlankValue') if is_missing else ('Present','NonemptyText') if required else ('NotRequired','ExplicitChecklistException')
            row['Fields'].append({'RuleId':'TYPE-INFO-'+field,'Field':field,'Required':None if unknown else required,
                'Status':status,'Reason':reason,'ObservedValue':value,'ParameterId':PARAMETERS[field],
                'RequirementSource':None if unknown else source})
        if unknown:unexamined.extend(refs)
        if row['Status']=='NeedsReview':affected.extend(refs)
        types.append(row)
    assert set(by_case)==set(identities)
    expected = {'DocumentPath':str(PureWindowsPath(model_path)),'ProjectUniqueId':checklist['ProjectUniqueId'],
        'RevitBuild':'24.3.60.12','ChecklistId':checklist['ChecklistId'],'ChecklistPath':str(PureWindowsPath(checklist_path)),
        'Scope':SCOPE,'Level':'L1 - Block 43','Phase':'New Construction','Units':'Instance counts and text; no measured quantities.',
        'RequestedInstances':16,'ExaminedInstances':15 if case=='nc1' else 16,
        'OtherHostWallCeilingInstances':1197 if case=='nc1' else 1196,'LinkInstancesExcluded':6,
        'ScopeIssues':[{'CaseId':'BAL-INFO-WA-01','UniqueId':ABSENT_ID,'Status':'Unexamined','Reason':'Element not found'}] if case=='nc1' else [],
        'Evaluation':{'Types':types,'FindingCount':len(missing),'AffectedElements':affected,'UnexaminedElements':unexamined,'Complete':case!='nc2'},
        'CoverageComplete':case not in ('nc1','nc2'),'ModifiedBefore':False,'ModifiedAfter':False}
    return expected

def verify_report(report, case, model_path, checklist_path):
    expected=expected_report(case,model_path,checklist_path)
    timestamp=datetime.fromisoformat(report['GeneratedUtc'].replace('Z','+00:00'))
    assert timestamp.tzinfo is not None and timestamp.utcoffset().total_seconds()==0, 'Timestamp must be UTC'
    actual=copy.deepcopy(report);actual.pop('GeneratedUtc')
    actual['DocumentPath']=str(PureWindowsPath(actual['DocumentPath']))
    actual['ChecklistPath']=str(PureWindowsPath(actual['ChecklistPath']))
    assert exact(actual)==exact(expected), 'Report differs from independent case expectations'
    return {'case':case,'pass':True,'findings':expected['Evaluation']['FindingCount'],
        'affectedInstances':len(expected['Evaluation']['AffectedElements']),'examinedInstances':expected['ExaminedInstances'],
        'coverageComplete':expected['CoverageComplete'],'evaluationComplete':expected['Evaluation']['Complete']}

def verify(directory, case):
    directory=Path(directory); prepared=read(directory/'expectation.json')
    assert prepared['case']==case
    context,result=verify_wire(directory)
    assert result['isError'] is False and context['isError'] is False
    data=result['structuredContent'];ctx=context['structuredContent']
    for response in (result,context):
        assert len(response['content'])==1 and response['content'][0]['type']=='text'
        assert exact(json.loads(response['content'][0]['text']))==exact(response['structuredContent'])
    assert data['bridgeSessionId']==ctx['bridgeSessionId']==prepared['bridgeSessionId']
    assert data['checklistSha256']==ctx['checklistSha256']==prepared['checklistSha256']
    assert ctx['revitPid']==prepared['revitPid']
    assert exact(prepared['expectedReport'])==exact(expected_report(case,prepared['modelPath'],prepared['checklistPath'])), 'Frozen oracle differs from its independent sources'
    for context_key,report_key in [('projectUniqueId','ProjectUniqueId'),('revitBuild','RevitBuild'),
        ('checklistId','ChecklistId'),('scope','Scope'),('level','Level'),('phase','Phase')]:
        assert ctx[context_key]==prepared['expectedReport'][report_key], 'Context disagrees with independent expectation: '+context_key
    assert PureWindowsPath(ctx['documentPath'])==PureWindowsPath(prepared['modelPath'])
    assert PureWindowsPath(ctx['checklistPath'])==PureWindowsPath(prepared['checklistPath'])
    at=lambda s:datetime.fromisoformat(s.replace('Z','+00:00'))
    assert at(prepared['preparedUtc'])<=at(data['report']['GeneratedUtc'])<at(ctx['expiresUtc'])
    if case=='nc1':assert prepared['absenceConfirmed'] is True, 'Independent absent-ID check is still pending'
    return verify_report(data['report'],case,prepared['modelPath'],prepared['checklistPath'])

def verify_wire(directory, outcome='ReadinessReport'):
    directory=Path(directory);meta=read(directory/'capture.json')
    assert outcome in ('ReadinessReport','ContextChanged')
    protocol=meta['protocolVersion']
    assert protocol in ('2025-06-18','2025-11-25','2026-07-28')
    assert len(meta['files'])==2 and {r['path'] for r in meta['files']}=={'wire/client-to-server.jsonl','wire/server-to-client.jsonl'}
    rows={}
    for record in meta['files']:
        data=(directory/record['path']).read_bytes()
        assert data and data.endswith(b'\n') and len(data)==record['bytes']
        assert hashlib.sha256(data).hexdigest()==record['sha256']
        rows[record['path']]=[json.loads(line) for line in data.splitlines()]
    incoming,outgoing=rows['wire/client-to-server.jsonl'],rows['wire/server-to-client.jsonl']
    assert all(r['jsonrpc']=='2.0' for r in incoming+outgoing)
    requests=[r for r in incoming if 'id' in r];responses=[r for r in outgoing if 'id' in r]
    assert len(requests)==len({r['id'] for r in requests})
    assert len(responses)==len({r['id'] for r in responses})
    replies={r['id']:r for r in responses};assert set(replies)=={r['id'] for r in requests}
    first=requests[0];reply=replies[first['id']]
    if protocol=='2026-07-28':
        assert first['method']=='server/discover' and protocol in reply['result']['supportedVersions']
        assert all(r.get('params',{}).get('_meta',{}).get('io.modelcontextprotocol/protocolVersion')==protocol for r in requests)
    else:
        assert first['method']=='initialize' and first['params']['protocolVersion']==reply['result']['protocolVersion']==protocol
        initialized=[r for r in incoming if r.get('method')=='notifications/initialized']
        assert len(initialized)==1 and 'id' not in initialized[0]
    catalogs=[r for r in requests if r['method']=='tools/list']
    assert catalogs and requests.index(catalogs[0])>0
    for r in catalogs:
        tools=replies[r['id']]['result']['tools']
        assert len(tools)==2 and {t['name'] for t in tools}=={'revit_get_readiness_context','revit_check_required_type_information'}
    matched=[]
    canonical=lambda value:json.dumps(value,sort_keys=True,ensure_ascii=False)
    for key,tool,filename in [('contextRequestId','revit_get_readiness_context','context.json'),('reportRequestId','revit_check_required_type_information','check.json')]:
        candidates=[r for r in requests if replies[r['id']].get('result',{}).get('structuredContent',{}).get('requestId')==meta[key]]
        assert len(candidates)==1
        request=candidates[0];assert request['method']=='tools/call' and request['params']['name']==tool
        result=replies[request['id']]['result'];body=result['structuredContent']
        assert re.fullmatch('[0-9a-f]{32}',body['requestId']) and body['contractVersion']==1
        assert len(result['content'])==1 and result['content'][0]['type']=='text'
        assert canonical(json.loads(result['content'][0]['text']))==canonical(body)
        assert canonical(read(directory/filename))==canonical(result)
        matched.append((request,result))
    (context_request,context),(check_request,check)=matched
    assert requests.index(catalogs[0])<requests.index(context_request)<requests.index(check_request)
    assert context_request['params']['arguments']=={} and context['isError'] is False
    assert check_request['params']['arguments']=={'contextToken':context['structuredContent']['contextToken']}
    if outcome=='ReadinessReport':assert check['isError'] is False and isinstance(check['structuredContent']['report'],dict)
    else:assert check['isError'] is True and check['structuredContent']['code']=='ContextChanged' and 'report' not in check['structuredContent']
    return context,check

if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('directory');parser.add_argument('--case',choices=CASES,required=True)
    args=parser.parse_args();print(json.dumps(verify(args.directory,args.case),indent=2))
