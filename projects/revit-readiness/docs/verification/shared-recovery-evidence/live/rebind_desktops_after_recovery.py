"""One-off PID-only rebind; private durable receipts precede all config writes."""
import copy,importlib.util,json,pathlib,re,sys
repo=pathlib.Path('D:/bim-agent-lab'); workspace=repo/'.superpowers/sdd/2026-10-20-revit-mcp-bridge'
spec=importlib.util.spec_from_file_location('settings',repo/'projects/revit-readiness/scripts/manage_mcp_probe_settings.py')
s=importlib.util.module_from_spec(spec);spec.loader.exec_module(s)
active=json.loads((workspace/'active-shared-recovery.json').read_text(encoding='utf-8-sig'))
pid=active['revitPid']; assert pid==178492
prior=json.loads((workspace/'active-desktop-acceptance.json').read_text(encoding='utf-8-sig'))
receipts=workspace/'shared-recovery-desktop-rebind';receipts.mkdir(exist_ok=False)
prepared=[]
for old in prior['settings']:
    config=pathlib.Path(old['config']);before=config.read_bytes();data=s.parse(before,old['format'])
    key='mcp_servers' if old['format']=='toml' else 'mcpServers';name=s.READINESS_NAME
    entry=data[key][name];assert entry['command']==old['entry']['command'] and entry['args']==old['entry']['args']
    assert set(entry)-{'enabled'}=={'command','args'}
    if old['client']=='codex': assert entry.get('enabled') is False,'Preserve the human-disabled connection'
    expected=copy.deepcopy(data);expected[key][name]['args'][1]=str(pid)
    needle=json.dumps(entry['args'][1]).encode();replacement=json.dumps(str(pid)).encode()
    assert before.count(needle)==1,'PID text is not unique; refuse a broad replacement'
    after=before.replace(needle,replacement,1)
    assert s.parse(after,old['format'])==expected,'Unexpected parsed config change'
    record={'kind':'readiness-pid-only-rebind','state':'Prepared','config':str(config),'format':old['format'],'before':s.encode(before),'after':s.encode(after),'beforeSha256':s.digest(before),'afterSha256':s.digest(after),'oldEntry':entry,'newEntry':expected[key][name],'note':'Private backup; do not use registration-removal restore on this PID-edit receipt. Restore only after comparing current complete bytes or preserving concurrent unrelated edits.'}
    path=receipts/(old['client']+'.json')
    with path.open('xb') as f:
        import os
        f.write(json.dumps(record,indent=2).encode());f.flush();os.fsync(f.fileno())
    prepared.append((path,record))
# Both complete backups are durable before either configuration is replaced.
for path,record in prepared:
    config=pathlib.Path(record['config']);assert s.digest(config.read_bytes())==record['beforeSha256'],'Concurrent edit; preserve it'
    s.atomic(config,s.decode(record['after']))
    assert s.digest(config.read_bytes())==record['afterSha256']
    record['state']='Applied';s.save(path,record)
summary={'newRevitPid':pid,'bridgeSessionId':active['bridgeSessionId'],'onlyChange':'owned readiness args[1] from 149972 to 178492','codexRemainsDisabled':True,'claudeNotLaunched':True,'unrelatedParsedSettingsPreserved':True,'fullByteChangesLimitedToUniqueQuotedPid':True,'clients':[{'client':p.stem,'config':d['config'],'beforeSha256':d['beforeSha256'],'afterSha256':d['afterSha256'],'oldPid':149972,'newPid':pid,'receipt':str(p),'state':d['state']} for p,d in prepared]}
out=pathlib.Path(active['liveRoot'])/'desktop-binding-refresh.json'
with out.open('x',encoding='utf-8') as f: json.dump(summary,f,indent=2);f.write('\n')
active['desktopRebindReceipts']=str(receipts)
active['status']='Fresh live recovery passed; both desktop readiness PID bindings updated; Codex remains disabled and Claude not launched'
(workspace/'active-shared-recovery.json').write_text(json.dumps(active,indent=2)+'\n',encoding='utf-8')
print('PASS both PID-only config changes; private backups retained; Codex remains disabled')
