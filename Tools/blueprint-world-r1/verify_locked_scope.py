"""Compare locked art/gameplay with the merged branch before world-only authoring."""
import hashlib,json,re,subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
BASE='db8828fdd33f157fc0e2591eaad387b906520175'
MAIN='38e0ed8fe5c95ff41ab5dbb246f45ed26bdcf9cc'
OUTPUT=ROOT/'docs/history/implementation-passes/chapter01-blueprint-world-r1/verification/locked-scope.json'
def git(*args):return subprocess.check_output(['git',*args],cwd=ROOT)
def content(path,data):
    if Path(path).suffix in ['.fbx','.png','.ogg','.wav','.ttf','.jpg']:return data
    return data.replace(b'\r\n',b'\n').rstrip()
actors=[('VisualSlice','Scout_V1'),('VisualSlice','Cutter_V1'),('VisualSlice','Magnetar_V1'),
        ('Chapter01Production','Warden_V1'),('Chapter01Production','ArcDrone_V1'),('Chapter01Production','Carrier_V1'),('Chapter01V3','Custodian_V3')]
coordinate_defs={'S04_SpawnSpot_CapacitorField.asset':'_worldOrigin','S04_SpawnSpot_CuttingFloor.asset':'_worldOrigin',
 'S04_SpawnSpot_HaulerGraveyard.asset':'_worldOrigin','S04_SpawnSpot_RelayYard.asset':'_worldOrigin','S04_SpawnSpot_ShieldDump.asset':'_worldOrigin',
 'S09_MagnetarGuard.asset':'_spawnPosition','S09_CustodianM0.asset':'_startPosition'}
def protected(p):
    name=Path(p).name
    return (p.startswith('Assets/_Game/Runtime/Gameplay/') and name!='StrongOrdinarySpots.cs' or
            p.startswith('Assets/_Game/Runtime/Persistence/') or
            p.startswith('Assets/_Game/Runtime/Presentation/UI/') or
            p.startswith('Assets/_Game/Runtime/Presentation/AudioVfx/') or
            p.startswith('Assets/_Game/Runtime/Presentation/Combat/') or
            p.startswith('Assets/_Game/Runtime/Presentation/Player/') or
            p.startswith('Assets/_Game/Content/Presentation/') or
            p.startswith('Assets/_Game/Content/Audio/') or
            p.startswith('Assets/_Game/ArtReview/G0ProductionV3/') or
            p.startswith('Assets/_Game/Content/Definitions/') and name not in coordinate_defs and name!='S08_Chapter01World.asset' or
            any(p.startswith(f'Assets/_Game/Content/{folder}/{kind}/{actor}.') for folder,actor in actors for kind in ['Models','Prefabs']) or
            p.startswith('Assets/_Game/Content/VisualSlice/') and '/G0_' in p or
            p.startswith('Assets/_Game/Content/VisualReplacementV3/') and (name.startswith(('M0_','UI_')) or name.startswith('ProductionUiSkin')) or
            p.startswith('Assets/_Game/Content/SurfaceHeroV46/') and name.startswith(('V46_Hero','V46_Actor')))
records=[];failures=[]
for path in git('ls-tree','-r','--name-only',BASE).decode().splitlines():
    if not protected(path):continue
    original=content(path,git('show',BASE+':'+path));current=content(path,(ROOT/path).read_bytes())
    same=original==current
    records.append(dict(path=path,unchanged=same,sha256=hashlib.sha256(current).hexdigest()))
    if not same:failures.append(path)
semantics=[]
for name,key in coordinate_defs.items():
    p='Assets/_Game/Content/Definitions/'+name
    original=git('show',BASE+':'+p).decode();current=(ROOT/p).read_text()
    def without_coordinate(text):
        return '\n'.join(line.rstrip() for line in text.splitlines() if not line.lstrip().startswith(key+':'))
    same=without_coordinate(original)==without_coordinate(current)
    semantics.append(dict(path=p,onlyAllowedCoordinateChanged=same,coordinateField=key))
    if not same:failures.append(p)
report=dict(synchronizedMainSHA=MAIN,preImplementationMergedBranchSHA=BASE,validated=not failures,actorsRedesigned=False,g0Redesigned=False,
            magnetarCustodianRedesigned=False,uiStyleRedesigned=False,
            method='Byte-exact binary comparison; text normalized to LF. Encounter configs compared with only the authorized position field removed.',
            protectedFiles=len(records),records=records,encounterSemantics=semantics,failures=failures)
OUTPUT.parent.mkdir(parents=True,exist_ok=True);OUTPUT.write_text(json.dumps(report,indent=2)+'\n')
print('LOCKED_SCOPE',len(records),'protected files; failures',failures)
if failures:raise SystemExit(1)
