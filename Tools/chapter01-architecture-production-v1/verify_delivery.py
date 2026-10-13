"""Verify immutable accepted art, original GUIDs and additive Chapter integration."""
import hashlib, json, re, subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
BASE='e1669362fd3566f69939b547f263a3a600e3e1b7'
SOURCE='7d0e375f7b0a12a9d46982af685cabe3ce57d731'
OUT=ROOT/'docs/history/implementation-passes/chapter01-architecture-production-v1/verification'
def git(*a): return subprocess.check_output(['git',*a],cwd=ROOT)
def norm(data): return data.replace(b'\r\n',b'\n')
def docs(data): return {(int(t),i):b for t,i,b in re.findall(r'--- !u!(\d+) &(\d+)[^\n]*\n(.*?)(?=--- !u!|\Z)',norm(data).decode(),re.S)}
promotion=json.loads((OUT/'promotion.json').read_text())
assets=[]
for item in promotion['assets']:
    old=git('show',SOURCE+':'+item['source']); new=(ROOT/item['production']).read_bytes()
    if item['production'].endswith('EnergyChamber_Production.prefab'):
        olddocs,newdocs=docs(old),docs(new)
        immutable=[k for k in olddocs if k[0] in (4,23,33,1001)]
        # Unity may append new serialized default fields; compare existing field lines.
        mismatches=[]
        for k in immutable:
            if k not in newdocs or any(line.strip() and line not in newdocs[k] for line in olddocs[k].splitlines()): mismatches.append(str(k))
        assert not mismatches, ('Accepted presentation records changed',mismatches)
        assets.append(dict(path=item['production'],acceptedPresentationRecords=len(immutable),preserved=True))
    else:
        assert norm(old)==norm(new),item['production']
        assets.append(dict(path=item['production'],preserved=True,sha256=hashlib.sha256(new).hexdigest()))
    if item.get('guidPreserved'):
        assert norm(git('show',SOURCE+':'+item['source']+'.meta'))==norm((ROOT/(item['production']+'.meta')).read_bytes()),item['production']+'.meta'
scene='Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity'
b,c=docs(git('show',BASE+':'+scene)),docs((ROOT/scene).read_bytes())
unchanged=[k for k in b if k[0]!=1660057539]
assert all(b[k]==re.sub(r'  _architecturalCollisionRoots:\n  - \{fileID: \d+\}\n','',c[k]) for k in unchanged),'Existing Chapter fields changed'
changed=git('diff','--name-only',BASE).decode().splitlines()
for p in changed:
    assert not p.startswith(('Assets/_Game/Runtime/Gameplay/', 'Assets/_Game/Runtime/Core/', 'Assets/_Game/Runtime/Persistence/', 'Assets/_Game/Runtime/Platform/', 'Assets/_Game/Content/ActorProductionV2/', 'Assets/_Game/Content/Definitions/')),p
layout='Assets/_Game/Content/BlueprintWorldR1/Chapter01WorldLayout.asset'
old_layout=norm(git('show',BASE+':'+layout)).decode()
new_layout=norm((ROOT/layout).read_bytes()).decode()
for line in ['  - {x: 4, y: 0, z: 58}\n','  - {x: 18, y: 0, z: 58}\n']:
    assert new_layout.count(line)==2
    new_layout=new_layout.replace(line,'')
assert old_layout==new_layout,'Layout changed outside four architecture route waypoints'
guids={}; duplicates=[]
for p in (ROOT/'Assets').rglob('*.meta'):
    match=re.search(r'^guid: ([0-9a-f]{32})',p.read_text(errors='ignore'),re.M)
    if match:
        g=match[1]
        if g in guids: duplicates.append([str(guids[g]),str(p)])
        guids[g]=p
assert not duplicates,duplicates
metrics=json.loads((OUT/'energy-chamber-metrics.json').read_text())
assert not any(p.startswith('Assets/') and ('/ArtProof/' in p or '/Editor/' in p or p.endswith('.unity')) for p in metrics['dependencies'])
report=dict(validated=True,source=SOURCE,base=BASE,acceptedAssets=assets,sceneExistingFieldsPreserved=len(unchanged),sceneNewRecords=len(c)-len(b),duplicateGuids=duplicates,runtimeDependencies=len(metrics['dependencies']),noGameplayDomainOrActorChanges=True,layoutOnlyAddsFourRouteWaypoints=True)
(OUT/'preservation.json').write_text(json.dumps(report,indent=2)+'\n')
print('PASS immutable accepted assets:',len(assets),'Chapter records:',len(unchanged),'GUIDs:',len(guids),'runtime dependencies:',len(metrics['dependencies']))
