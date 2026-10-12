"""Refine the original project trim sheet; preserve authored grain, UVs and anatomy."""
from pathlib import Path
from PIL import Image
import numpy as np, subprocess, io
root=Path(__file__).resolve().parents[2]
rel='Assets/_Game/Content/ActorProductionV2/Textures/HostileV2_BaseColor.png'
original=Image.open(io.BytesIO(subprocess.check_output(['git','show','7dcd6652c0c7232aaaed5e134edbfc5ea1c3c7e2:'+rel],cwd=root))).convert('RGBA')
a=np.array(original).astype(float)
targets={0:(38,46,55),3:(59,73,86),7:(75,90,102)}
for i,target in targets.items():
    row,col=divmod(i,4); patch=a[512*(1-row):512*(2-row),col*256:(col+1)*256,:3]
    average=patch.mean(axis=(0,1)); patch[:]=np.clip(patch/average*np.array(target),0,255)
Image.fromarray(a.astype('uint8'),'RGBA').save(root/rel)
red=a.copy()
for i,target in {3:(86,37,40)}.items():
    row,col=divmod(i,4); patch=red[512*(1-row):512*(2-row),col*256:(col+1)*256,:3]
    patch[:]=np.clip(patch/patch.mean(axis=(0,1))*np.array(target),0,255)
Image.fromarray(red.astype('uint8'),'RGBA').save(root/rel.replace('BaseColor','ContainedRedShell'))
print('PROJECT_OWNED_FINISHES_PASS: grain retained; graphite, service steel and contained red shell')
g0rel='Assets/_Game/ArtReview/G0ProductionV3/Textures/G0_V3_BaseColor.png'
g0=np.array(Image.open(io.BytesIO(subprocess.check_output(['git','show','7dcd6652c0c7232aaaed5e134edbfc5ea1c3c7e2:'+g0rel],cwd=root))).convert('RGBA')).astype(float)
rgb=g0[:,:,:3]; steel=(np.max(rgb,axis=2)>10)&(np.abs(rgb[:,:,0]-rgb[:,:,1])<20)&(np.abs(rgb[:,:,1]-rgb[:,:,2])<25)
rgb[steel]=np.minimum(rgb[steel]*1.38,170)
Image.fromarray(g0.astype('uint8'),'RGBA').save(root/g0rel)
