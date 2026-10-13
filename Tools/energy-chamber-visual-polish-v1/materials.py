"""Project-owned PBR finishes; one metre tiles, no external downloads."""
from pathlib import Path
import json, numpy as np
from PIL import Image, ImageDraw, ImageFont
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/_Game/ArtProof/EnergyChamber/Textures/PolishV1'
OUT.mkdir(parents=True,exist_ok=True)
rng=np.random.default_rng(771)
n=512
y,x=np.mgrid[0:n,0:n]/n
grain=rng.normal(0,1,(n,n))
brush=rng.normal(0,1,(n,1))*2
# RGB in display space; metallic and smoothness are linear control maps.
finishes={
 'Graphite':((68,67,62),.48,.38,'cast'),
 'Shell':((167,166,153),.24,.48,'paint'),
 'Steel':((151,160,160),.84,.63,'brush'),
 'Deck':((86,91,89),.45,.34,'deck'),
 'Ceramic':((115,83,53),.15,.40,'paint'),
 'Warning':((158,109,38),.12,.40,'paint'),
 'Coil':((27,59,63),.66,.58,'brush'),
 'Cold':((36,114,128),.20,.48,'paint'),
 'Copper':((129,88,55),.73,.57,'brush'),
 'Gasket':((29,31,30),.02,.25,'cast'),
 'Plate':((100,106,101),.46,.43,'paint')}
records=[]
for name,(rgb,metal,smooth,kind) in finishes.items():
    relief=np.zeros((n,n))
    variation=grain*.9
    if kind=='brush': variation+=brush*.55; relief+=grain*.035
    elif kind=='cast': variation+=grain*1.2;relief+=grain*.10
    elif kind=='deck':
        # Small stamped grip, rather than broad hazard stripes.
        dash=((x*24+y*24)%1<.09)&((x*24-y*24)%1<.65)
        relief+=dash*.65;variation+=dash*7
    else: relief+=grain*.03
    # Restrained machining scratches; no large random damage.
    for i in range(18 if kind=='brush' else 7):
        u,v=rng.uniform(.05,.95,2);length=rng.uniform(.012,.06)
        scratch=(abs(y-v)<.0008)&(abs(x-u)<length)
        variation+=scratch*6;relief-=scratch*.12
    color=np.clip(np.array(rgb)[None,None,:]+variation[...,None],0,255).astype('uint8')
    dx=np.roll(relief,-1,1)-np.roll(relief,1,1);dy=np.roll(relief,-1,0)-np.roll(relief,1,0)
    norm=np.stack([-dx*.7,-dy*.7,np.ones_like(dx)],axis=-1)
    norm/=np.linalg.norm(norm,axis=-1,keepdims=True)
    normal=((norm*.5+.5)*255).astype('uint8')
    ms=np.zeros((n,n,4),dtype='uint8');ms[:,:,0]=int(metal*255);ms[:,:,3]=np.clip(smooth*255+variation*1.5,0,255)
    ao=np.repeat(np.clip(253-np.maximum(0,-relief)*15,235,255).astype('uint8')[...,None],3,axis=2)
    for suffix,data in [('BaseColor',color),('Normal',normal),('MetallicSmoothness',ms),('Occlusion',ao)]:
        Image.fromarray(data).save(OUT/f'EC_{name}_{suffix}.png')
    records.append(dict(role=name,rgb=rgb,metallic=metal,smoothness=smooth,finish=kind))

# Existing project-owned palette retains the donor's authored UV panel layout.
# Lift black painted strips into readable charcoal and reduce the blue cast.
src=ROOT/'Assets/_Game/Content/SurfaceHeroV46/Textures/V46_Legacy_BaseColor.png'
a=np.asarray(Image.open(src).convert('RGB'),dtype=float)/255
lum=a.mean(axis=2,keepdims=True)
a=lum*.38+a*.62
a=np.maximum(a,np.array([.22,.23,.22]))
a=np.clip(a*1.16,0,1)
Image.fromarray((a*255).astype('uint8')).save(OUT/'EC_DonorPalette.png')

# Opaque plaque atlas: the double compression chevron and paired service bars
# recur on the crown, hatch and node panels. No decal sorting / alpha overdraw.
im=Image.new('RGB',(1024,256),(33,39,38));d=ImageDraw.Draw(im)
font=ImageFont.truetype('C:/Windows/Fonts/consolab.ttf',56)
small=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',22)
d.rectangle((8,8,1015,247),outline=(115,126,120),width=4)
for c in [48,107]:d.line([(c,55),(c+36,101),(c,147)],fill=(162,114,44),width=15)
d.text((174,35),'GRV / C-07',font=font,fill=(181,191,181))
d.text((178,116),'CONTAINMENT / LOCKED',font=small,fill=(137,154,148))
d.text((178,162),'FEED 01   |   RETURN 02',font=small,fill=(137,154,148))
for i in range(9):d.rectangle((760+i*22,173,770+i*22,212),fill=(122,136,129))
d.line((750,51,977,51),fill=(58,135,142),width=6)
d.line((750,72,905,72),fill=(58,135,142),width=6)
im.save(OUT/'EC_Markings_BaseColor.png')
report=ROOT/'docs/history/implementation-passes/energy-chamber-visual-polish-v1'
(report/'material-hierarchy.json').write_text(json.dumps(dict(finishes=records,donorPaletteSource=str(src.relative_to(ROOT)),markings='Project-owned compression chevrons, paired channels, C-07 containment plaque'),indent=2))
print('POLISH_MATERIAL_MAPS',len(list(OUT.glob('*.png'))))
