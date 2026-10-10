"""R2 may change presentation only; freeze all R1 gameplay/topology sources."""
import hashlib,json,subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
BASE='67236348a2be8732f5fbd88eb2093da18f5f7251'
VERIFY=ROOT/'docs/history/implementation-passes/chapter01-blueprint-world-r2/verification'

def git(*args):return subprocess.check_output(['git',*args],cwd=ROOT).decode().splitlines()
def frozen(path):
    if path=='Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity':return False
    if path.startswith('Assets/_Game/Content/BlueprintWorldR1/'):
        return '/Chapter01WorldLayout.asset' in path
    return (path.startswith(('Assets/_Game/Runtime/','Assets/_Game/Content/','Assets/_Game/ArtReview/G0ProductionV3/')) or
        path=='Tools/blueprint-world-r1/layout.json')
paths=git('ls-tree','-r','--name-only',BASE)
changed=git('diff','--name-only',BASE)
failed=[p for p in changed if frozen(p)]
records=[]
for name in ['Tools/blueprint-world-r1/layout.json','Assets/_Game/Content/BlueprintWorldR1/Chapter01WorldLayout.asset','Assets/_Game/Content/Definitions/S08_Chapter01World.asset']:
    before=subprocess.check_output(['git','show',BASE+':'+name],cwd=ROOT).replace(b'\r\n',b'\n')
    now=(ROOT/name).read_bytes().replace(b'\r\n',b'\n')
    records.append(dict(path=name,unchanged=before==now,sha256=hashlib.sha256(now).hexdigest()))
    if before!=now:failed.append(name)
report=dict(validated=not failed,r1AcceptedTechnicalBaseline=BASE,protectedTrackedFiles=sum(frozen(p) for p in paths),
    topologyCoordinatesCollisionMinimapGameplayUnchanged=not failed,failures=failed,criticalAuthorityFiles=records,
    method='Git worktree diff against R1 delivery HEAD for all runtime, content outside allowed world presentation, G0 source and layout. Critical layout/definition bytes separately compared after LF normalization.')
VERIFY.mkdir(parents=True,exist_ok=True);(VERIFY/'preservation.json').write_text(json.dumps(report,indent=2)+'\n')
print('R2_PRESERVATION',report['protectedTrackedFiles'],'frozen files; failures',failed)
if failed:raise SystemExit(1)
