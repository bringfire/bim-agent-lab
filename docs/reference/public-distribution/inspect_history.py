import collections, gzip, io, json, re, subprocess, zipfile
from pathlib import Path
from pypdf import PdfReader
from PIL import Image

import argparse
parser = argparse.ArgumentParser(description="Inspect reachable Git blobs; write potentially sensitive review output only to a fresh ignored directory.")
parser.add_argument("--repo", required=True)
parser.add_argument("--output", required=True)
parser.add_argument("--ref", action="append", dest="refs", required=True)
args = parser.parse_args()
root = Path(args.repo).resolve()
out = Path(args.output).resolve()
refs = args.refs
try:
    relative = out.relative_to(root)
except ValueError:
    parser.error("Output must be inside the repository's ignored scratch area.")
ignored = subprocess.run(["git", "-C", str(root), "check-ignore", "--quiet", str(relative / "review-output.json")])
if ignored.returncode != 0:
    parser.error("Output must be Git-ignored before scanning.")
if out.exists() and any(out.iterdir()):
    parser.error("Use a fresh output directory so old extracted data cannot contaminate the scan.")
(out / "extracted").mkdir(parents=True)

def git(*args):
    return subprocess.check_output(['git', '-C', str(root), *args])

objects = {}
for line in git('rev-list', '--objects', *refs).decode().splitlines():
    oid, _, path = line.partition(' ')
    objects[oid] = path
meta = subprocess.run(
    ['git', '-C', str(root), 'cat-file', '--batch-check'],
    input=('\n'.join(objects)+'\n').encode(), stdout=subprocess.PIPE, check=True).stdout.decode()
blobs = [(oid, int(size)) for oid, kind, size in map(str.split, meta.splitlines()) if kind == 'blob']
raw = subprocess.run(['git','-C',str(root),'cat-file','--batch'],
    input=('\n'.join(oid for oid,_ in blobs)+'\n').encode(),stdout=subprocess.PIPE,check=True).stdout
pos = 0
binary = []; extraction_errors = []; emails = collections.defaultdict(set)
flags = []; urls = collections.defaultdict(set); archives = []; image_files = []
counts = collections.Counter(); extracted_bytes = 0
patterns = {
    'credential_assignment': re.compile(r'(?i)(?:api[_-]?key|access[_-]?token|refresh[_-]?token|client[_-]?secret|password|passwd|authorization)\s*[\"\']?\s*[:=]\s*[\"\']([^\s\"\']{8,})'),
    'jwt': re.compile(r'eyJ[A-Za-z0-9_-]{12,}\.[A-Za-z0-9_-]{12,}\.[A-Za-z0-9_-]{10,}'),
    'url_userinfo': re.compile(r'https?://[^/\s\"<>]+:[^/\s\"<>]+@'),
    'sensitive_query': re.compile(r'(?i)[?&](?:token|key|secret|sig|signature|password|auth|X-Amz-Signature)=[^&\s\"<>]{8,}'),
    'private_network': re.compile(r'(?<![\d.])(?:10\.\d+\.\d+\.\d+|192\.168\.\d+\.\d+|172\.(?:1[6-9]|2\d|3[01])\.\d+\.\d+)(?![\d.])'),
    'confidential_label': re.compile(r'(?i)\b(?:confidential|do not distribute|internal use only|not for distribution|NDA)\b'),
}
for oid,size in blobs:
    end=raw.index(b'\n',pos); header=raw[pos:end].decode().split(); pos=end+1
    assert header[0]==oid and int(header[2])==size
    data=raw[pos:pos+size]; pos+=size+1
    name=objects[oid]; ext=Path(name).suffix.lower(); counts[ext]+=1
    text=''
    try:
        if data.startswith(b'\x1f\x8b'):
            text=gzip.decompress(data).decode('utf8',errors='strict')
            archives.append({'oid':oid,'path':name,'kind':'gzip','uncompressed_bytes':len(text.encode('utf8'))})
        elif data.startswith(b'%PDF'):
            r=PdfReader(io.BytesIO(data)); text='\n'.join(p.extract_text() or '' for p in r.pages)
            text+='\n'+str(r.metadata)
            for page in r.pages:
                for a in page.get('/Annots',[]): text+='\n'+str(a.get_object())
            archives.append({'oid':oid,'path':name,'kind':'pdf','pages':len(r.pages),'metadata':str(r.metadata)})
            (out / (oid+'.pdf.txt')).write_text(text,encoding='utf8')
            (out / (oid+'.pdf')).write_bytes(data)
        elif data.startswith(b'PK\x03\x04'):
            z=zipfile.ZipFile(io.BytesIO(data)); members=z.namelist()
            text='\n'.join(z.read(x).decode('utf8',errors='strict') for x in members if x.endswith(('.xml','.rels')))
            archives.append({'oid':oid,'path':name,'kind':'zip','members':members})
            (out / (oid+'.zip.txt')).write_text(text,encoding='utf8')
        elif ext in ['.png','.jpg','.jpeg','.gif']:
            im=Image.open(io.BytesIO(data)); text=str(im.info)
            image_files.append({'oid':oid,'path':name,'size':size,'dimensions':im.size,'metadata':text})
            (out / (oid+ext)).write_bytes(data)
        elif data.startswith((b'\xff\xfe',b'\xfe\xff')):
            text=data.decode('utf16')
        elif b'\0' not in data:
            text=data.decode('utf8',errors='strict')
        elif data[:500].count(b'\0')>50:
            text=data.decode('utf-16-le',errors='strict')
        elif data == bytes.fromhex('00ff0d0a2a'):
            binary.append({'oid':oid,'path':name,'bytes':size,'magic':data[:8].hex()})
        else:
            text=data.decode('utf8',errors='strict')
    except Exception as e:
        extraction_errors.append({'oid':oid,'path':name,'error':str(e)})
    extracted_bytes+=len(text.encode('utf8'))
    if text: (out/'extracted'/(oid+'.txt')).write_text(text,encoding='utf8')
    for email in re.findall(r'[A-Za-z0-9_.+%-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}',text): emails[email].add(name)
    for url in re.findall(r'https?://[^\s<>\"\)]+',text):
        if any(x in url.lower() for x in ['share','artifact','localhost','127.0.0.1','ngrok','tunnel']): urls[url].add(name)
    for rule,pattern in patterns.items():
        for m in pattern.finditer(text):
            flags.append({'rule':rule,'oid':oid,'path':name,'line':text[:m.start()].count('\n')+1,'value':m.group(0)[:300]})

allpaths=set()
for line in git('log','--format=','--name-only',*refs).decode().splitlines():
    if line: allpaths.add(line)
restricted=[p for p in sorted(allpaths) if re.search(r'(?i)(\.(rvt|rfa|rte|rft|ifc|3dm|dll|exe|nupkg|pfx|p12|pem|key|zip|7z|mp4|wav)$|(^|/)(\.env(?:\..*)?|auth\.json|credentials|claude_desktop_config\.json|config\.toml)$)',p)]
report={'source_commit':git('rev-parse','HEAD').decode().strip(),'refs':refs,
    'commit_count':int(git('rev-list','--count',*refs)), 'unique_blobs':len(blobs),'blob_bytes':sum(s for _,s in blobs),
    'extracted_text_bytes':extracted_bytes,'current_files':len(git('ls-files','-z').split(b'\0'))-1,
    'historical_paths':len(allpaths),'extensions':dict(counts),'binary_unhandled':binary,'extraction_errors':extraction_errors,
    'restricted_paths':restricted,'emails':{k:sorted(v) for k,v in emails.items()},'urls_for_review':{k:sorted(v) for k,v in urls.items()},
    'flags':flags,'documents':archives,'images':image_files,
    'commit_identities':git('log','--format=%an <%ae> | %cn <%ce>',*refs).decode().splitlines()}
(out/'inventory-private.json').write_text(json.dumps(report,indent=2),encoding='utf8')
print(json.dumps({k:report[k] for k in ['source_commit','commit_count','unique_blobs','blob_bytes','extracted_text_bytes','current_files','historical_paths','extensions','binary_unhandled','extraction_errors','restricted_paths']},indent=2))
print(json.dumps({'flag_counts':dict(collections.Counter(f['rule'] for f in flags)), 'email_count':len(emails),'documents':[(d['path'],d['oid']) for d in archives],'images':image_files},indent=2))
