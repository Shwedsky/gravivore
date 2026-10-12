from pathlib import Path
import subprocess, hashlib, json, sys

ROOT=Path(__file__).resolve().parents[2]
OWNER=ROOT.parents[1]
OUT=ROOT/'docs/history/implementation-passes/capacitor-field-proof/verification'
OUT.mkdir(parents=True,exist_ok=True)
BASE='9e0713ca08deebf43890cf23d20e954d23b7265c'
def git(*args): return subprocess.check_output(['git',*args],cwd=ROOT)
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
owner_file=OUT/'owner-originals.json'
if not owner_file.exists():
    paths=list((OWNER/'Builds/Android').glob('*'))+list((OWNER/'ExternalAssetIntake/Current/quaternius-modular-scifi-megakit-standard').glob('*.zip'))+list((OWNER/'ExternalAssetIntake/Current/molten-maps-scifi').glob('*.zip'))
    rows=[dict(path=str(p.relative_to(OWNER)),size=p.stat().st_size,sha256=sha(p)) for p in paths if p.is_file()]
    owner_file.write_text(json.dumps(rows,indent=2),encoding='utf-8')
owner=json.loads(owner_file.read_text(encoding='utf-8'))
owner_bad=[r['path'] for r in owner if not (OWNER/r['path']).exists() or sha(OWNER/r['path'])!=r['sha256']]
baseline50=OWNER/'Builds/Android/gravivore-dev-0.1.0+50.apk'
assert sha(baseline50)=='6d172d4125c80b39960e50f16aa76b3f85055cf8394e6bda42a67b998bf630ca'
protected=[];changed=[]
dirty=set(git('diff','--name-only',BASE).decode().splitlines())
for raw in git('ls-tree','-r','--name-only',BASE).decode().splitlines():
    keep=raw.startswith(('Assets/_Game/Runtime/','Assets/_Game/Content/ActorProductionV2/','Assets/_Game/Content/VisualSlice/','Assets/_Game/Content/BlueprintWorldR1/Models/','Assets/_Game/Content/BlueprintWorldR1/Textures/','Assets/_Game/Content/BlueprintWorldR1/Materials/','Assets/_Game/Content/BlueprintWorldR1/Prefabs/'))
    keep |= raw.startswith('Assets/_Game/Content/Definitions/') and raw!='Assets/_Game/Content/Definitions/S01_CameraFollowSettings.asset'
    if not keep:continue
    protected.append(raw)
    if raw not in dirty:continue
    prior=git('show',BASE+':'+raw);current=(ROOT/raw).read_bytes()
    if Path(raw).suffix in ['.cs','.meta','.asset','.prefab','.mat','.controller','.json','.asmdef','.md','.xml','.txt'] or Path(raw).name=='.gitkeep':prior=prior.replace(b'\r\n',b'\n');current=current.replace(b'\r\n',b'\n')
    if prior!=current:changed.append(raw)
report=dict(base=BASE,apk50BuildSource='57dfde50485a0db3d966e980bd94dddd1ea48488',protectedFiles=len(protected),protectedDifferences=changed,ownerOriginals=len(owner),ownerDifferences=owner_bad,apk50Sha256=sha(baseline50),passed=not changed and not owner_bad)
(OUT/'preservation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
if '--strict' in sys.argv and not report['passed']:sys.exit(1)

# Compare only paired captures made at the same game tick/camera. This is
# execution evidence for the real post-processing pass, not a visual score.
on=OUT.parent/'gameplay-no-ui/postprocess_on.png'
off=OUT.parent/'gameplay-no-ui/postprocess_off.png'
if on.exists() and off.exists():
    import numpy as np
    from PIL import Image
    a=np.asarray(Image.open(on).convert('RGB'),dtype=np.int16)
    b=np.asarray(Image.open(off).convert('RGB'),dtype=np.int16)
    if a.shape!=b.shape:raise RuntimeError('Paired render evidence dimensions differ')
    delta=np.abs(a-b)
    pixels=dict(width=int(a.shape[1]),height=int(a.shape[0]),changedPixels=int(np.any(delta>0,axis=2).sum()),
                meanAbsoluteChannelDifference=float(delta.mean()),maxChannelDifference=int(delta.max()),
                method='Same camera and game tick, sRGB SDR render target; only URP post-processing enable flag changed.')
    (OUT/'postprocessing-pixels.json').write_text(json.dumps(pixels,indent=2),encoding='utf-8')
    if pixels['changedPixels']==0:raise RuntimeError('URP post-processing did not affect rendered pixels')
