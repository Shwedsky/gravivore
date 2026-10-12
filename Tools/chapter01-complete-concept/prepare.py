from pathlib import Path
import hashlib, json, subprocess
from PIL import Image, ImageFilter
import numpy as np

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/_Game/Content/Chapter01CompleteConcept'
SOURCE=ROOT/'art/chapter01-complete-concept'
PASS=ROOT/'docs/history/implementation-passes/chapter01-complete-concept'
for p in [OUT/'Textures',OUT/'Models',SOURCE,PASS/'verification']:
    p.mkdir(parents=True,exist_ok=True)
# The shared modelling helpers are original GRAVIVORE source. This copy isolates
# the completed chapter from changes to the two approved sector deliveries.
lib=(ROOT/'Tools/connected-concept-sectors/meshlib.py').read_text(encoding='utf-8')
lib=lib.replace("Content/ConnectedConceptSectors", "Content/Chapter01CompleteConcept").replace("art/connected-concept-sectors", "art/chapter01-complete-concept").replace("implementation-passes/connected-concept-sectors", "implementation-passes/chapter01-complete-concept")
(Path(__file__).parent/'meshlib.py').write_text(lib.rstrip()+'\n',encoding='utf-8')
rng=np.random.default_rng(5308); n=512
y,x=np.mgrid[0:n,0:n]
for name,color,metal,smooth in [('ServicePaint',(82,109,122),.57,.42),('ArmorShell',(116,125,130),.73,.38),('Scorched',(65,57,49),.79,.26)]:
    noise=rng.normal(0,1.6,(n,n)); grain=(np.sin(y*.65)+np.sin(x*.12))*.6
    broad=np.asarray(Image.fromarray(rng.integers(0,255,(n,n),dtype=np.uint8)).filter(ImageFilter.GaussianBlur(24)),dtype=float)-127
    variation=noise+grain+broad*.12
    rgb=np.clip(np.array(color)[None,None,:]+variation[:,:,None],0,255).astype('uint8')
    if name=='Scorched':
        patch=np.maximum(0,broad-2)*1.1
        rgb=np.clip(rgb.astype(float)+patch[:,:,None]*np.array([1.4,.5,-.2]),0,255).astype('uint8')
    Image.fromarray(rgb).save(OUT/'Textures'/f'CH_{name}_BaseColor.png')
    dy,dx=np.gradient((noise+grain)*.07)
    normal=np.stack((128-dx*8,128-dy*8,np.full((n,n),254)),axis=-1)
    Image.fromarray(np.clip(normal,0,255).astype('uint8')).save(OUT/'Textures'/f'CH_{name}_Normal.png')
    rgba=np.zeros((n,n,4),dtype='uint8');rgba[:,:,0]=int(metal*255);rgba[:,:,3]=np.clip(smooth*255+variation*2,0,255)
    Image.fromarray(rgba).save(OUT/'Textures'/f'CH_{name}_MetallicSmoothness.png')
    Image.fromarray(np.clip(248+variation*.3,225,255).astype('uint8')).save(OUT/'Textures'/f'CH_{name}_Occlusion.png')
owner=ROOT.parents[1]
receipt=PASS/'verification/owner-originals.json'
if not receipt.exists():
    paths=list((owner/'Builds/Android').glob('*'))+list((owner/'ExternalAssetIntake/Current').rglob('*.zip'))
    rows=[dict(path=str(p.relative_to(owner)),size=p.stat().st_size,sha256=hashlib.sha256(p.read_bytes()).hexdigest()) for p in paths if p.is_file()]
    receipt.write_text(json.dumps(rows,indent=2),encoding='utf-8')
(PASS/'verification/baseline.json').write_text(json.dumps(dict(deliveryCommit='7546d20b3aa82d90807edf5808c9bf628d0eed33',apk52Source='179b1de57b55b924fec14ec51d81357e96eb5f35',apk52Sha256='8a20191e0e1bf1a59b7c1d0d9def4ac8339abe50a59e60683e442346628bed57'),indent=2),encoding='utf-8')
print('CHAPTER_SOURCE_AND_PBR_READY')
