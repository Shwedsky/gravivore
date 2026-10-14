"""Reject geometric, collision, placement, route or unrelated gameplay changes."""
import hashlib,json,re,subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'docs/history/implementation-passes/energy-chamber-runtime-presentation-v1'
manifest=json.loads((OUT/'verification/architecture-freeze.json').read_text())
for name,digest in manifest['hashes'].items():
    assert hashlib.sha256((ROOT/name).read_bytes()).hexdigest()==digest,'Frozen file changed: '+name
prefab='Assets/_Game/Content/Chapter01Architecture/EnergyChamber/Prefabs/EnergyChamber_Production.prefab'
base=subprocess.check_output(['git','show',manifest['baselineHead']+':'+prefab],cwd=ROOT).decode()
current=(ROOT/prefab).read_text()
def records(text):
    pieces=re.split(r'^--- !u!(\d+) &(-?\d+)(?: stripped)?\s*$',text,flags=re.M);out={}
    for i in range(1,len(pieces),3):out[(int(pieces[i]),pieces[i+1])]=pieces[i+2]
    return out
before=records(base);after=records(current)
def authority(key,data):
    if key[0]==205:return None
    if key[0]==23:data=re.sub(r'  m_Materials:\n(?:  - .*\n)+','',data)
    if key[0]==1001:
        data=re.sub(r"    - target:.*\n      propertyPath: '?m_Materials\.[^\n]*\n      value:[^\n]*\n      objectReference:[^\n]*\n",'',data)
    return data.strip()
for key,data in before.items():
    assert key in after,'Frozen object removed: '+str(key)
    assert authority(key,data)==authority(key,after[key]),'Frozen prefab transform/collision/settings changed: '+str(key)
extra=[k for k in after if k not in before]
assert not extra,'Unexpected new prefab component/geometry: '+str(extra)
protected=('Assets/_Game/Runtime/Core/','Assets/_Game/Runtime/Gameplay/','Assets/_Game/Runtime/Persistence/','Assets/_Game/Runtime/Platform/','Assets/_Game/Content/ActorProductionV2/','Assets/_Game/Content/Definitions/')
changed=subprocess.check_output(['git','diff','--name-only',manifest['baselineHead']],cwd=ROOT).decode().splitlines()
assert not [p for p in changed if p.startswith(protected)],'Unrelated gameplay/content touched'
receipt=dict(frozenFiles=len(manifest['hashes']),prefabRecords=len(before),addedPrefabRecords=len(extra),meshesUVsSourcesCollidersTransformsSceneLayoutUnchanged=True,protectedGameplayChanges=[],baselineHead=manifest['baselineHead'])
(OUT/'verification/architecture-freeze-verified.json').write_text(json.dumps(receipt,indent=2)+'\n')
print('Frozen architecture verified:',len(manifest['hashes']),'source files;',len(before),'prefab records; exact scene/layout.')
