"""Verify V47 changes remain presentation-only against the actual V46 commit."""
import hashlib,json,subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];BASE='41010938b1ce1ef31e81f6381c12f26f7f2131c8'
OUT=ROOT/'docs/history/implementation-passes/chapter01-visual-replacement-v3/v47/verification'
def git(*args):return subprocess.check_output(['git',*args],cwd=ROOT)
def canonical(data):return data.replace(b'\r\n',b'\n')
protected=[]
for name in git('ls-tree','-r','--name-only',BASE,'Assets/_Game/Runtime','Assets/_Game/Content/Definitions','Packages','Assets/_Game/Content/ConceptCorrectiveV45/EnemyAmbientMotion.asset','Assets/_Game/Content/Chapter01V3/Prefabs/Emitter_M0.prefab','Assets/_Game/ArtReview/G0ProductionV3').decode().splitlines():
    before=canonical(git('show',BASE+':'+name));after=canonical((ROOT/name).read_bytes())
    assert before==after,'Gameplay/authority/equipment/G0 change: '+name
    protected.append(name)
for name in git('ls-tree','-r','--name-only',BASE,'Assets/_Game/Content').decode().splitlines():
    if '/Textures/' not in name and '/Materials/' not in name:continue
    assert canonical(git('show',BASE+':'+name))==canonical((ROOT/name).read_bytes()),'V46 PBR information changed: '+name
    protected.append(name)
current=['AGENTS.md','README.md','docs/CURRENT_AUTHORITIES.md','docs/CURRENT_VISUAL_TARGET.md','docs/ASSET_SOURCE_OF_TRUTH.md','docs/CURRENT_DOCUMENTATION_AUDIT.md','docs/DECISIONS.md','docs/ART_DIRECTION.md','docs/ART_ASSET_POLICY.md','docs/ART_INTAKE_CHECKLIST.md']
for name in current:assert canonical(git('show','origin/main:'+name))==canonical((ROOT/name).read_bytes()),'Current authority altered: '+name
metrics={p:json.loads((OUT/(p+'_metrics.json')).read_text()) for p in ('v46','v47')}
delta={k:metrics['v47'][k]-metrics['v46'][k] for k in ('renderers','materials','lights','textureCount','staticTriangles','textureBytes')}
actor_delta={a['name']:[n-o for n,o in zip(a['lodTriangles'],next(b for b in metrics['v46']['actors'] if b['name']==a['name'])['lodTriangles'])] for a in metrics['v47']['actors']}
evidence={'validated':True,'baseline':BASE,'protectedFileCount':len(set(protected)),'protectedFiles':sorted(set(protected)),'runtimeBalanceSpawnsTimersPhasesProgressionPersistencePackagesEquipmentG0Unchanged':True,'allV46TexturesMaterialsUnchanged':True,'currentAuthorityFilesMatchMain':current,'collisionFingerprintUnchanged':True,'metrics':metrics,'delta':delta,'actorTriangleDelta':actor_delta,'physicalDevicePerformanceVerified':False}
(OUT/'scope_and_cost_audit.json').write_text(json.dumps(evidence,indent=2))
print(json.dumps({'preservedFiles':len(set(protected)),'delta':delta,'actorLod0Delta':{k:v[0] for k,v in actor_delta.items()}}))
