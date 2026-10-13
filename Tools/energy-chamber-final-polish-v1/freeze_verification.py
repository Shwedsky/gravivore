"""Verify the owner-accepted proof without authoring or rendering assets."""
from pathlib import Path
from datetime import datetime,timezone
import hashlib,json,subprocess,xml.etree.ElementTree as ET

root=Path(__file__).resolve().parents[2]
evidence=root/'docs/history/implementation-passes/energy-chamber-final-polish-v1'
accepted='f14a0c7c55e39b94d64494c5e756a7fecabc2f7e'
asset_root='Assets/_Game/ArtProof/EnergyChamber'
def git(*args):return subprocess.check_output(['git','-c','core.safecrlf=false',*args],cwd=root)
def lines(*args):return git(*args).decode().splitlines()
def sha(data):return hashlib.sha256(data).hexdigest()
def normalized(data):return data.replace(b'\r\n',b'\n')

tracked=set(lines('ls-files'))
assets=sorted(p for p in tracked if p.startswith(asset_root+'/') or p in (asset_root+'.meta','Assets/_Game/ArtProof.meta'))
assert not lines('diff','--name-only',accepted,'--',asset_root,asset_root+'.meta','Assets/_Game/ArtProof.meta')
assert not lines('ls-files','--others','--exclude-standard','--',asset_root)
frozen=[]
for p in assets:
    blob=git('show',f'{accepted}:{p}')
    current=(root/p).read_bytes()
    assert (current==blob if p.endswith(('.png','.fbx')) else normalized(current)==normalized(blob)),p
    assert git('show',f'HEAD:{p}')==blob,p
    frozen.append(dict(path=p,gitBlobSha256=sha(blob)))
    if not p.endswith('.meta'):assert p+'.meta' in tracked,p
source_art=[]
for p in sorted(p for p in tracked if p.startswith('art/energy-chamber-') and p.endswith('.blend')):
    blob=git('show',f'{accepted}:{p}')
    assert (root/p).read_bytes()==blob and git('show',f'HEAD:{p}')==blob,p
    source_art.append(dict(path=p,sha256=sha(blob)))
assert len(source_art)==3

dependencies=(evidence/'verification/device-project-dependencies.txt').read_text().splitlines()
assert len(dependencies)==247
for p in dependencies:
    assert p in tracked and p+'.meta' in tracked,p
    assert (root/p).exists() and (root/(p+'.meta')).exists(),p
shared=sorted(p for p in dependencies if not p.startswith(asset_root+'/'))
for p in shared:
    assert git('show',f'HEAD:{p}')==git('show',f'origin/main:{p}'),p
    assert git('show',f'HEAD:{p}.meta')==git('show',f'origin/main:{p}.meta'),p+'.meta'
source='Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity'
camera='Assets/_Game/Content/Definitions/S01_CameraFollowSettings.asset'
g0='Assets/_Game/Content/VisualSlice/Prefabs/G0_V3_Live_Tier0.prefab'
for p in [source,camera,g0]:
    assert normalized((root/p).read_bytes())==normalized(git('show',f'origin/main:{p}')),p

changed=lines('diff','--name-only','origin/main...HEAD')
allowed=('Assets/_Game/ArtProof/','Tools/energy-chamber-','art/energy-chamber-','docs/history/implementation-passes/energy-chamber-')
assert all(p=='Assets/_Game/ArtProof.meta' or p.startswith(allowed) for p in changed)
assert not lines('diff','--name-only','origin/main...HEAD','--','Assets/_Game/Content','Assets/_Game/Scripts','Assets/_Game/Editor','ProjectSettings','Packages','Assets/ThirdParty')

receipt=json.loads((evidence/'verification/preservation-and-delivery.json').read_text())
assert receipt['camera']['g0Scale']==dict(x=1.,y=1.,z=1.)
assert abs(receipt['camera']['g0Bounds']['y']-1.771)<.002
context=receipt['chapter']
assert context['sourceGeometryMaterialsTransformsPreserved'] and context['sourceRenderers']==476 and context['sourceLights']==17
assert sha(normalized(git('show',f'origin/main:{source}')))==context['sourceLfSha256']
tests={name:ET.parse(evidence/f'verification/{name}.xml').getroot().attrib for name in ['EditMode','PlayMode']}
assert tests['EditMode']['passed']=='535' and tests['PlayMode']['passed']=='2'
assert all(t['result']=='Passed' and t['failed']=='0' for t in tests.values())
apk=Path(receipt['apk']['path'])
assert sha(apk.read_bytes())==receipt['apk']['sha256']
assert receipt['apk']['receipt']['result']=='Succeeded'
screens=sorted((evidence/'screenshots').glob('*.png'))
assert len(screens)==16
for p in screens:
    rel=p.relative_to(root).as_posix()
    assert rel in tracked and p.read_bytes()==git('show',f'{accepted}:{rel}'),rel

# Include staged finalization documentation in the complete PR inventory.
pending=lines('diff','--cached','--name-only')
inventory=sorted(set(changed+pending))
(evidence/'verification/complete-branch-files.txt').write_text('\n'.join(inventory)+'\n',encoding='utf-8')
(evidence/'verification/shared-production-dependencies.txt').write_text('\n'.join(shared)+'\n',encoding='utf-8')
main_paths=set(lines('ls-tree','-r','--name-only','origin/main'))
record=dict(verdict='ENERGY CHAMBER HYBRID PROOF — COMPLETE',acceptedPresentationCommit=accepted,
    verifiedAtUtc=datetime.now(timezone.utc).isoformat(),main=git('rev-parse','origin/main').decode().strip(),
    architectureAndPresentationUnchanged=True,assets=frozen,sourceArt=source_art,trackedDependencyCount=len(dependencies),
    allRequiredDependenciesAndMetaTracked=True,sharedDependencies=shared,productionAndGameplayFilesChanged=[],
    g0Scale=receipt['camera']['g0Scale'],g0Height=receipt['camera']['g0Bounds']['y'],chapter=context,tests=tests,
    apk=receipt['apk'],screenshots=[dict(path=p.relative_to(root).as_posix(),sha256=sha(p.read_bytes())) for p in screens],
    temporaryCleanup=dict(path='.utmp/',kind='Untracked native build debug cache in isolated proof worktree',removedFiles=55,removedBytes=4632691),
    retainedLocalGeneratedFiles='Unity-generated ProjectSettings and .vsconfig remain local, untracked, and outside the delivery. Review APK and standalone build project are preserved.',
    currentDirectPathConflicts=[p for p in inventory if p in main_paths],
    furtherVisualWorkRecommended=False)
assert not record['currentDirectPathConflicts']
(evidence/'verification/final-freeze.json').write_text(json.dumps(record,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
print(f'FROZEN_PROOF_VERIFIED: {len(assets)} immutable tracked proof files; 247 tracked dependencies/metas; {len(shared)} unchanged shared dependencies; 16 unchanged screenshots; no production/gameplay edits')
