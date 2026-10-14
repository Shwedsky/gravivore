"""Promote the pinned accepted runtime closure; never copy proof tooling/scenes."""
import hashlib, json, re, shutil, subprocess, uuid
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SHA = '7d0e375f7b0a12a9d46982af685cabe3ce57d731'
OLD = 'Assets/_Game/ArtProof/EnergyChamber'
NEW = 'Assets/_Game/Content/Chapter01Architecture/EnergyChamber'
OUT = ROOT / 'docs/history/implementation-passes/chapter01-architecture-production-v1/verification'
def git(*a): return subprocess.check_output(['git', *a], cwd=ROOT)
def blob(p): return git('show', SHA + ':' + p)
def sha(b): return hashlib.sha256(b).hexdigest()
paths = git('ls-tree', '-r', '--name-only', SHA, '--', 'Assets').decode().splitlines()
guid_map = {}
for p in paths:
    if p.endswith('.meta'):
        match = re.search(rb'^guid: ([0-9a-f]{32})', blob(p), re.M)
        if match: guid_map[match[1].decode()] = p[:-5]
pending = [OLD + '/Prefabs/EnergyChamber_HybridProof.prefab', OLD + '/Prefabs/DistributionNode.prefab']
seen, shared, copied = set(), set(), []
while pending:
    p = pending.pop()
    if p in seen: continue
    seen.add(p)
    if not p.startswith(OLD + '/'):
        if p.startswith('Assets/'):
            shared.add(p)
            assert (ROOT/p).is_file(), p
            assert (ROOT/p).read_bytes().replace(b'\r\n', b'\n') == blob(p).replace(b'\r\n', b'\n'), p
        continue
    assert '/Editor/' not in p and '/Scenes/' not in p and 'Review' not in p, p
    data = blob(p)
    refs = data + blob(p + '.meta')
    pending.extend(guid_map[g.decode()] for g in re.findall(rb'guid: ([0-9a-f]{32})', refs) if g.decode() in guid_map and guid_map[g.decode()] != p)
    dest = p.replace(OLD, NEW)
    if p.endswith('EnergyChamber_HybridProof.prefab'):
        dest = dest.replace('EnergyChamber_HybridProof.prefab', 'EnergyChamber_Production.prefab')
        data = data.replace(b'Energy Chamber / hybrid proof V1', b'Energy Chamber / production')
    target = ROOT/dest
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_bytes(data)
    (ROOT/(dest+'.meta')).write_bytes(blob(p+'.meta'))
    copied.append(dict(source=p, production=dest, sourceSha256=sha(blob(p)), productionSha256=sha(data), guidPreserved=True))
for p in paths:
    if p.startswith(OLD+'/Licenses/') and not p.endswith('.meta'):
        dest = p.replace(OLD, NEW)
        (ROOT/dest).parent.mkdir(parents=True, exist_ok=True)
        (ROOT/dest).write_bytes(blob(p)); (ROOT/(dest+'.meta')).write_bytes(blob(p+'.meta'))
for p in git('ls-tree','-r','--name-only',SHA,'--','art').decode().splitlines():
    if p.startswith(('art/energy-chamber-hybrid-proof-v1/', 'art/energy-chamber-visual-polish-v1/', 'art/energy-chamber-vertical-nodes-v1/')) and p.endswith('.blend'):
        (ROOT/p).parent.mkdir(parents=True, exist_ok=True); (ROOT/p).write_bytes(blob(p))
        copied.append(dict(source=p, production=p, sourceSha256=sha(blob(p)), productionSha256=sha(blob(p)), editableSource=True))
for name in ['asset-origin.json','promoted-donors.json']:
    dest = OUT/name; dest.parent.mkdir(parents=True, exist_ok=True)
    dest.write_bytes(blob('docs/history/implementation-passes/energy-chamber-hybrid-proof-v1/'+name))
for folder in sorted({p.parent for p in (ROOT/NEW).rglob('*') if p.is_file()} | {ROOT/NEW, ROOT/'Assets/_Game/Content/Chapter01Architecture'}):
    meta = folder.with_name(folder.name+'.meta')
    if not meta.exists():
        meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n')
OUT.mkdir(parents=True, exist_ok=True)
(OUT/'promotion.json').write_text(json.dumps(dict(acceptedSha=SHA, assets=copied, sharedUnchanged=sorted(shared), proofScenesAndEditorExcluded=True),indent=2)+'\n')
print('Promoted',len(copied),'runtime/source assets;',len(shared),'shared references unchanged')
