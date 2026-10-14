"""Author deterministic, tileable PBR surface maps; accepted mesh UVs are untouched."""
import json,hashlib
from pathlib import Path
import numpy as np
from PIL import Image
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/_Game/Content/Chapter01Architecture/EnergyChamber/Textures/RuntimePresentationV1'
OUT.mkdir(parents=True,exist_ok=True)
# sRGB surface means, metallic, smoothness, maintenance wear, normal relief.
ROLES={
 'Graphite':((48,57,63),.68,.26,.035,.42),
 'Machined':((143,163,176),.82,.64,.08,.22),
 'Casing':((88,103,114),.66,.43,.09,.35),
 'Pipe':((108,126,140),.85,.70,.06,.19),
 'Recess':((14,18,21),.12,.16,.015,.30),
 'Copper':((125,81,48),.75,.55,.12,.28),
 'Deck':((77,88,92),.45,.30,.20,.65),
 'ColdPanel':((158,174,183),.68,.47,.12,.32)
}
N=512
y,x=np.mgrid[0:N,0:N];u=x/N;v=y/N
receipts=[]
for index,(role,(color,metal,smooth,wear,relief)) in enumerate(ROLES.items()):
    rng=np.random.default_rng(6201+index)
    grain=rng.normal(0,1,(N,N))
    broad=np.zeros((N,N))
    for frequency,amplitude in [(1,.55),(2,.25),(5,.13),(13,.07)]:
        broad+=amplitude*np.sin(2*np.pi*(frequency*u+(frequency+1)*v)+rng.uniform(0,2*np.pi))*np.cos(2*np.pi*((frequency+2)*u-frequency*v)+rng.uniform(0,2*np.pi))
    brushed=np.sin(2*np.pi*v*127+np.sin(2*np.pi*u*3))*.5
    marks=np.zeros((N,N));engraving=np.zeros((N,N))
    # Sparse maintenance scuffs / engraved scratches. No random color splashes.
    for line in range(72 if role=='Deck' else 30):
        row=rng.integers(0,N);start=rng.integers(0,N);length=rng.integers(8,86)
        columns=(start+np.arange(length))%N
        marks[row,columns]+=rng.uniform(.25,.8)
        engraving[(row+1)%N,columns]+=rng.uniform(.2,.6)
    rub=np.maximum(0,np.sin(2*np.pi*(2*u-v)+.3))**12
    scuff=np.minimum(1,marks+rub*.4)*wear
    variation=1+.12*broad+.015*grain+.012*brushed+scuff
    patina=np.maximum(0,-broad-.2)*wear
    albedo=np.array(color)[None,None,:]*variation[:,:,None]
    albedo-=patina[:,:,None]*np.array([3,7,9])[None,None,:]
    albedo-=engraving[:,:,None]*9
    albedo=np.uint8(np.clip(albedo,0,255))
    m=np.clip(metal-.18*patina-.045*grain,0,1)
    s=np.clip(smooth+.10*broad+.035*grain+.06*brushed+.17*scuff-.10*engraving,.08,.87)
    packed=np.stack((m,np.zeros_like(m),np.zeros_like(m),s),axis=-1)
    height=grain*.009+brushed*.007+marks*.035-engraving*.065
    dx=(np.roll(height,-1,1)-np.roll(height,1,1))*relief
    dy=(np.roll(height,-1,0)-np.roll(height,1,0))*relief
    normal=np.stack((-dx,-dy,np.ones_like(dx)),axis=-1);normal/=np.linalg.norm(normal,axis=-1,keepdims=True)
    normal=np.uint8(np.clip((normal*.5+.5)*255,0,255))
    ao=np.uint8(np.clip((1-engraving*.24-np.maximum(0,-broad-.35)*.07)*255,0,255))
    maps={'BaseColor':albedo,'MetallicSmoothness':np.uint8(packed*255),'Normal':normal,'Occlusion':np.stack((ao,ao,ao),axis=-1)}
    for kind,pixels in maps.items():
        path=OUT/f'EC_{role}_{kind}.png';Image.fromarray(pixels).save(path,optimize=True)
        receipts.append(dict(file=path.relative_to(ROOT).as_posix(),sha256=hashlib.sha256(path.read_bytes()).hexdigest(),size=N,role=role,kind=kind))
record=dict(author='GRAVIVORE project-owned deterministic surface authoring',roles={k:dict(srgbMean=c,metallic=m,smoothness=s,wear=w,normalRelief=n)for k,(c,m,s,w,n)in ROLES.items()},maps=receipts)
evidence=ROOT/'docs/history/implementation-passes/energy-chamber-runtime-presentation-v1/verification'
(evidence/'surface-authoring.json').write_text(json.dumps(record,indent=2)+'\n')
print('Authored',len(receipts),'512px maps across',len(ROLES),'distinct material roles.')
