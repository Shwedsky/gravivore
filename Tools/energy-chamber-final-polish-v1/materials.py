"""Final project-owned surfaces. No mesh edits, external downloads or runtime VFX."""
from pathlib import Path
import json
import numpy as np
from PIL import Image, ImageDraw, ImageFont

root=Path(__file__).resolve().parents[2]
out=root/'Assets/_Game/ArtProof/EnergyChamber/Textures/FinalPolishV1'
out.mkdir(parents=True,exist_ok=True)
evidence=root/'docs/history/implementation-passes/energy-chamber-final-polish-v1'
n=512
y,x=np.mgrid[0:n,0:n]/n
rng=np.random.default_rng(77013)
grain=rng.normal(0,1,(n,n))

finishes={
    'BaseIron':((91,86,75),.51,.31,'cast'),
    'BaseCassette':((122,122,112),.29,.39,'paint'),
    'ChamberCasing':((181,176,158),.24,.46,'paint'),
    'Frame':((91,101,101),.69,.46,'cast'),
    'PipeSteel':((147,151,141),.79,.57,'brush'),
    'NodeCasing':((137,158,158),.27,.44,'paint'),
    'Interface':((126,134,122),.66,.48,'brush'),
}
records=[]
for name,(rgb,metal,smooth,kind) in finishes.items():
    # Wiped handling edges, a few fine machining lines, seam dirt. All opaque.
    border=np.minimum.reduce([x,1-x,y,1-y])
    edge=np.exp(-border*65)
    handling=edge*(.55+.45*np.sin(x*41+y*63)**2)
    scratches=np.zeros_like(x)
    for _ in range(14 if kind=='brush' else 6):
        u,v=rng.uniform(.025,.975,2)
        scratches+=((abs(y-v)<.0015)&(abs(x-u)<rng.uniform(.015,.065)))
    variation=grain*1.4+handling*9+scratches*9
    if kind=='brush':variation+=rng.normal(0,.8,(n,1))
    # Small dark witness line at tile seam; broad material values do the work.
    seam=border<.003
    variation-=seam*7
    base=np.clip(np.array(rgb)[None,None,:]+variation[...,None],0,255).astype('uint8')
    relief=grain*(.05 if kind=='cast' else .018)-scratches*.07-seam*.05
    dx=np.roll(relief,-1,1)-np.roll(relief,1,1)
    dy=np.roll(relief,-1,0)-np.roll(relief,1,0)
    normal=np.stack([-dx*.6,-dy*.6,np.ones_like(x)],axis=-1)
    normal/=np.linalg.norm(normal,axis=-1,keepdims=True)
    normal=((normal*.5+.5)*255).astype('uint8')
    ms=np.zeros((n,n,4),dtype='uint8')
    ms[:,:,0]=np.clip(metal*255+handling*16,0,255)
    ms[:,:,3]=np.clip(smooth*255-handling*14+grain*2,0,255)
    ao=np.repeat(np.where(seam,239,255).astype('uint8')[...,None],3,axis=2)
    for suffix,data in [('BaseColor',base),('Normal',normal),('MetallicSmoothness',ms),('Occlusion',ao)]:
        Image.fromarray(data).save(out/f'EC_{name}_{suffix}.png')
    records.append(dict(role=name,rgb=rgb,metallic=metal,smoothness=smooth,finish=kind))

for name,rgb in [('CoreWarm',(128,58,17)),('CoreInnerWarm',(116,47,13)),('NodeCool',(32,91,109))]:
    warm=name!='NodeCool'
    # Interleaved induction waves and narrow meandering discharge filaments.
    # Dark troughs remain visible; avoid checker grids and uniformly white glow.
    phase=y*(4 if warm else 5)+.095*np.sin(x*np.pi*6)+.035*np.sin(x*np.pi*14+y*np.pi*4)
    loop=(.5+.5*np.cos(phase*np.pi*2))**6
    filament=np.zeros_like(x)
    for k in range(5 if warm else 3):
        path=(k+.5)/(5 if warm else 3)+.025*np.sin(y*np.pi*(4+k%2*2)+k*1.7)
        distance=np.abs((x-path+.5)%1-.5)
        filament=np.maximum(filament,np.exp(-(distance/(.009 if warm else .012))**2))
    cavity=.5+.5*np.sin(x*12*np.pi+y*2*np.pi)
    mask=np.clip(.12+.29*loop+.46*filament+.09*cavity,0,1)
    # Opaque absorption slits make the inner sleeve visibly different from skin.
    band=(y*(6 if warm else 8))%1
    mask*=np.where(band<.045,.30,1)
    if name=='CoreInnerWarm':mask=np.clip(mask*.75+.1*loop,0,1)
    base=np.clip(np.array(rgb)[None,None,:]*(.68+.48*mask[...,None])+grain[...,None]*.55,0,255).astype('uint8')
    emission=np.repeat((mask*255).astype('uint8')[...,None],3,axis=2)
    relief=loop*.025+grain*.003
    dx=np.roll(relief,-1,1)-np.roll(relief,1,1);dy=np.roll(relief,-1,0)-np.roll(relief,1,0)
    normal=np.stack([-dx,-dy,np.ones_like(x)],axis=-1);normal/=np.linalg.norm(normal,axis=-1,keepdims=True)
    normal=((normal*.5+.5)*255).astype('uint8')
    ms=np.zeros((n,n,4),dtype='uint8');ms[:,:,0]=24;ms[:,:,3]=np.clip(135+loop*28,0,255)
    ao=np.repeat(np.where(band<.045,238,255).astype('uint8')[...,None],3,axis=2)
    for suffix,data in [('BaseColor',base),('Emission',emission),('Normal',normal),('MetallicSmoothness',ms),('Occlusion',ao)]:
        Image.fromarray(data).save(out/f'EC_{name}_{suffix}.png')

# Existing opaque plaques get clear technical typography and the established
# paired channels / compression-chevron motif; no extra plaque geometry.
im=Image.new('RGB',(1024,256),(35,42,39));d=ImageDraw.Draw(im)
font=ImageFont.truetype('C:/Windows/Fonts/consolab.ttf',58)
small=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',25)
d.rectangle((9,9,1014,246),outline=(133,143,127),width=3)
for c in [40,99]:d.line([(c,60),(c+33,103),(c,146)],fill=(166,117,45),width=14)
d.text((172,32),'GRV / C-07',font=font,fill=(191,199,182))
d.text((178,110),'CONTAINMENT / LOCKED',font=small,fill=(150,165,153))
d.text((178,158),'01 FEED   02 RETURN',font=small,fill=(150,165,153))
for i in range(8):d.rectangle((785+i*22,166,795+i*22,206),fill=(132,148,133))
d.line((776,52,976,52),fill=(53,130,137),width=6)
d.line((776,74,928,74),fill=(53,130,137),width=6)
# Tiny localized scraped border corners, rather than randomized dirt everywhere.
d.line((13,18,68,18),fill=(165,170,148),width=2)
d.line((960,238,1009,238),fill=(165,170,148),width=2)
im.save(out/'EC_Markings_BaseColor.png')
(evidence/'material-hierarchy.json').write_text(json.dumps(dict(finishes=records,energy='Opaque interleaved induction waves, narrow filaments, absorption troughs; separate inner sleeve. No animation, transparent pass, extra lights or geometry.',details='Existing brackets, gantry feet, mating plates, risers and hatches differentiated by Interface finish; existing opaque plaques updated. Local handled-edge wear in color/smoothness.'),indent=2))
print('FINAL_SURFACE_MAPS',len(list(out.glob('*.png'))))
