"""Original function-specific PBR maps; no external downloads or vendor code."""
from pathlib import Path
import numpy as np
from PIL import Image, ImageFilter
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/_Game/Content/ConnectedConceptSectors/Textures'
OUT.mkdir(parents=True,exist_ok=True)
rng=np.random.default_rng(5204);n=512;y,x=np.mgrid[0:n,0:n]
grain=rng.normal(0,1.9,(n,n));blur=np.asarray(Image.fromarray(np.uint8(rng.uniform(0,255,(n,n)))).filter(ImageFilter.GaussianBlur(17)),dtype=float);blur=(blur-blur.mean())*.9
for name,color,metal,smooth in [('HeatSteel',(98,73,61),212,140),('PressPaint',(110,120,123),112,116),('FabricationDeck',(65,70,75),128,98)]:
    heat=(np.sin(x*.025)*.5+.5)*8 if name=='HeatSteel' else 0
    rgb=np.stack([np.clip(c+grain+blur+heat*(1 if i==0 else -.35),0,255) for i,c in enumerate(color)],axis=-1).astype('uint8')
    Image.fromarray(rgb).save(OUT/f'CS_{name}_BaseColor.png')
    mm=np.zeros((n,n,4),dtype='uint8');mm[:,:,0]=metal;mm[:,:,3]=np.clip(smooth+grain*1.4-blur*2,0,255);Image.fromarray(mm).save(OUT/f'CS_{name}_MetallicSmoothness.png')
    normal=np.zeros((n,n,3),dtype='uint8');normal[:,:,0]=np.clip(128+np.gradient(grain,axis=1)*.8,0,255);normal[:,:,1]=np.clip(128+np.gradient(grain,axis=0)*.8,0,255);normal[:,:,2]=255;Image.fromarray(normal).save(OUT/f'CS_{name}_Normal.png')
    Image.fromarray(np.uint8(np.clip(247-blur*.25,228,255))).save(OUT/f'CS_{name}_Occlusion.png')
print('CONNECTED_ORIGINAL_PBR_READY')
