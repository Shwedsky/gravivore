from pathlib import Path
import hashlib,json,subprocess,sys
ROOT=Path(__file__).resolve().parents[2];OWNER=ROOT.parents[1]
OUT=ROOT/'docs/history/implementation-passes/chapter01-complete-concept/verification'
BASE='7546d20b3aa82d90807edf5808c9bf628d0eed33'
def git(*args):return subprocess.check_output(['git',*args],cwd=ROOT,stderr=subprocess.DEVNULL)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
prior=json.loads((OUT/'owner-originals.json').read_text(encoding='utf-8'))
owner_bad=[r['path'] for r in prior if not(OWNER/r['path']).exists() or sha(OWNER/r['path'])!=r['sha256']]
dirty=set(git('diff','--name-only',BASE).decode().splitlines());protected=[];changed=[]
for p in git('ls-tree','-r','--name-only',BASE).decode().splitlines():
    keep=p.startswith(('Assets/_Game/Runtime/','Assets/_Game/Content/ActorProductionV2/','Assets/_Game/Content/VisualSlice/','Assets/_Game/Content/Definitions/','Assets/_Game/Content/CapacitorFieldProof/','Assets/_Game/Content/ConnectedConceptSectors/','Assets/_Game/Content/BlueprintWorldR1/Models/','Assets/_Game/Content/BlueprintWorldR1/Textures/','Assets/_Game/Content/BlueprintWorldR1/Materials/','Assets/_Game/Content/BlueprintWorldR1/Prefabs/'))
    if not keep:continue
    protected.append(p)
    if p not in dirty:continue
    a=git('show',BASE+':'+p);b=(ROOT/p).read_bytes()
    if Path(p).suffix in ['.cs','.meta','.asset','.prefab','.mat','.controller','.json','.asmdef','.md','.txt'] or Path(p).name=='.gitkeep':a=a.replace(b'\r\n',b'\n');b=b.replace(b'\r\n',b'\n')
    if a!=b:changed.append(p)
record=dict(baselineCommit=BASE,protectedFiles=len(protected),protectedDifferences=changed,ownerOriginals=len(prior),ownerDifferences=owner_bad,apk52Sha256=sha(OWNER/'Builds/Android/gravivore-dev-0.1.0+52.apk'),passed=not changed and not owner_bad)
assert record['apk52Sha256']=='8a20191e0e1bf1a59b7c1d0d9def4ac8339abe50a59e60683e442346628bed57'
(OUT/'preservation.json').write_text(json.dumps(record,indent=2),encoding='utf-8')
print(json.dumps(record,indent=2))
if '--strict' in sys.argv and not record['passed']:sys.exit(1)
