"""Opaque contained-energy surfaces: orange central core, cool peripheral cores."""
from pathlib import Path
import numpy as np
from PIL import Image
root=Path(__file__).resolve().parents[2]
out=root/'Assets/_Game/ArtProof/EnergyChamber/Textures/VerticalNodesV1';out.mkdir(parents=True,exist_ok=True)
n=512;y,x=np.mgrid[0:n,0:n]/n
rng=np.random.default_rng(407)
grain=rng.normal(0,1,(n,n))
for name,rgb in [('CoreWarm',(139,67,25)),('NodeCool',(38,108,133))]:
    # Horizontal containment bands / narrow axial divisions reveal a mechanism,
    # rather than a uniformly luminous tube. All maps stay opaque.
    band=(y*9)%1
    active=np.where(band<.12,.15,np.where(band>.90,.42,.85))
    active*=np.where((x*12)%1<.06,.22,1)
    active*=.88+.12*np.sin(y*30+x*12)
    base=np.clip(np.array(rgb)[None,None,:]*(.55+.45*active[...,None])+grain[...,None]*.8,0,255).astype('uint8')
    emission=np.repeat((np.clip(active,0,1)*255).astype('uint8')[...,None],3,axis=2)
    relief=np.where(band<.12,.06,0)+grain*.005
    dx=np.roll(relief,-1,1)-np.roll(relief,1,1);dy=np.roll(relief,-1,0)-np.roll(relief,1,0)
    normal=np.stack([-dx,-dy,np.ones_like(dx)],axis=-1);normal/=np.linalg.norm(normal,axis=-1,keepdims=True)
    normal=((normal*.5+.5)*255).astype('uint8')
    metal=np.zeros((n,n,4),dtype='uint8');metal[:,:,0]=35;metal[:,:,3]=np.where(band<.12,100,154)
    ao=np.repeat(np.where(band<.12,221,255).astype('uint8')[...,None],3,axis=2)
    for suffix,data in [('BaseColor',base),('Emission',emission),('Normal',normal),('MetallicSmoothness',metal),('Occlusion',ao)]:Image.fromarray(data).save(out/f'EC_{name}_{suffix}.png')
print('CONTAINED_ENERGY_MAPS_AUTHORED')
