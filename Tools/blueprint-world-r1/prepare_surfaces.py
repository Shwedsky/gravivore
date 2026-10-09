"""Convert CC0 ORM channels for URP and author the machinery surface atlas."""
from pathlib import Path
from PIL import Image
import numpy as np
ROOT=Path(__file__).resolve().parents[2]
DEST=ROOT/'Assets/_Game/Content/BlueprintWorldR1/Textures'
DEST.mkdir(parents=True,exist_ok=True)
SOURCE=ROOT/'ExternalAssetIntake/Current/quaternius-modular-scifi-megakit-standard/inspection'
for number in range(1,4):
    prefix=f'T_Trim_{number:02d}'
    for suffix in ['BaseColor','Normal']:
        source=Image.open(SOURCE/f'{prefix}_{suffix}.png').convert('RGB')
        source.thumbnail((1024,1024))
        source.save(DEST/f'R1_Trim{number}_{suffix}.png')
    orm=Image.open(SOURCE/f'{prefix}_ORM.png').convert('RGB')
    orm.thumbnail((1024,1024)); a=np.array(orm)
    out=np.zeros((*a.shape[:2],4),dtype=np.uint8)
    out[:,:,0]=a[:,:,2];out[:,:,3]=255-a[:,:,1]
    Image.fromarray(out).save(DEST/f'R1_Trim{number}_MetallicSmoothness.png')
    Image.fromarray(a[:,:,0]).save(DEST/f'R1_Trim{number}_Occlusion.png')
    if number==1:
        em=Image.open(SOURCE/'T_Trim_01_Emissive.png').convert('RGB');em.thumbnail((1024,1024));em.save(DEST/'R1_Trim1_Emission.png')

# Eight deliberately separate surface identities; emission occupies only the
# final three cells. Donor gradient UVs are re-authored, not just recolored.
size=1024;tile=256
palette=[(66,79,90),(106,123,135),(56,72,83),(155,102,35),
         (175,186,190),(12,130,175),(210,86,13),(210,32,12)]
base=np.zeros((size,size,3),dtype=np.uint8)
metal=np.zeros((size,size,4),dtype=np.uint8)
ao=np.full((size,size),255,dtype=np.uint8)
em=np.zeros((size,size,3),dtype=np.uint8)
height=np.zeros((size,size),dtype=np.float32)
rng=np.random.default_rng(48101)
for i,color in enumerate(palette):
    x=(i%4)*tile;y=(i//4)*tile
    yy,xx=np.mgrid[0:tile,0:tile]
    grain=rng.normal(0,1.9,(tile,tile))
    ribs=(np.cos(xx*.19)*.6+np.cos(yy*.32)*.5)
    field=np.asarray(color)[None,None,:]+(grain+ribs)[:,:,None]
    edge=np.minimum.reduce([xx,yy,tile-1-xx,tile-1-yy])
    field[edge<5]*=.36;field[(edge>=5)&(edge<7)]=np.clip(np.asarray(color)*1.28,0,255)
    # Service panels have real recessed boundaries and brushed microrelief.
    h=np.full((tile,tile),.6,dtype=np.float32);h[edge<5]=.22;h[(edge>=5)&(edge<7)]=.72
    h+=np.sin(xx*.4)*.008+grain*.0007
    if i in [0,1,2,4]:
        slot=(xx>28)&(xx<tile-28)&((yy%57)<2)
        field[slot]*=.61;h[slot]=.35
        scratch=(rng.random((tile,tile))<.012)&(edge<16)&(edge>7)
        field[scratch]=np.clip(np.array(color)*1.42,0,255)
    if i==3:
        stripe=((xx+yy)//23)%2==0
        field[stripe]*=.18
    base[y:y+tile,x:x+tile]=np.clip(field,0,255).astype(np.uint8)
    metal[y:y+tile,x:x+tile,0]=145 if i==0 else 190 if i in [1,2,4] else 105
    metal[y:y+tile,x:x+tile,3]=np.clip(102+grain*1.4+(i==4)*50,0,255).astype(np.uint8)
    ao[y:y+tile,x:x+tile]=np.where(edge<5,170,245)
    if i>=5:
        mask=(edge>14)&(xx>22)&(xx<tile-22)
        em[y:y+tile,x:x+tile][mask]=color
    height[y:y+tile,x:x+tile]=h
dy,dx=np.gradient(height)
normal=np.stack([-dx*3,-dy*3,np.ones_like(dx)],axis=2)
normal/=np.linalg.norm(normal,axis=2)[:,:,None]
for name,data in [('BaseColor',base),('MetallicSmoothness',metal),('Occlusion',ao),('Emission',em),('Normal',np.clip((normal*.5+.5)*255,0,255).astype(np.uint8))]:
    Image.fromarray(data).save(DEST/f'R1_Industrial_{name}.png')
print('URP surface maps prepared:',len(list(DEST.glob('*.png'))))
