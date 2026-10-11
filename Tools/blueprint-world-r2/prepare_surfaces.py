"""Author deterministic structural surfaces; no random decal/noise identity."""
from pathlib import Path
from PIL import Image, ImageDraw
import numpy as np

ROOT=Path(__file__).resolve().parents[2]
DEST=ROOT/'Assets/_Game/Content/BlueprintWorldR1/Textures'
SOURCE=ROOT/'ExternalAssetIntake/Current/quaternius-modular-scifi-megakit-standard/inspection'
# Preserve native donor relief/UVs while separating graphite structure from
# medium machinery and exposed metal. Original CC0 sources are untouched.
for number in range(1,4):
    source=np.asarray(Image.open(SOURCE/f'T_Trim_{number:02d}_BaseColor.png').convert('RGB').resize((1024,1024)),dtype=float)
    gray=source.mean(axis=2,keepdims=True)
    tint=np.array([.62,.68,.74]) if number!=2 else np.array([.64,.61,.57])
    body=np.clip((source*.75+gray*.25)*tint-5,0,255).astype('uint8')
    Image.fromarray(body).save(DEST/f'R1_Trim{number}_BaseColor.png')

palette=[(24,30,38),(87,100,114),(48,46,43),(164,112,35),
         (145,164,176),(7,133,184),(230,91,11),(202,22,7),
         (28,53,72),(91,50,30),(61,32,28),(76,91,89),
         (107,76,33),(29,34,39),(10,15,21),(59,74,81)]
size=1024;tile=256
base=Image.new('RGB',(size,size));height=Image.new('L',(size,size),148)
metal=np.zeros((size,size,4),dtype='uint8');ao=np.full((size,size),245,dtype='uint8');em=np.zeros((size,size,3),dtype='uint8')
for i,color in enumerate(palette):
    face=Image.new('RGB',(tile,tile),color);draw=ImageDraw.Draw(face)
    relief=Image.new('L',(tile,tile),148);depth=ImageDraw.Draw(relief)
    # A machined perimeter, inset panel hierarchy and purpose-aligned wear.
    draw.rectangle((3,3,252,252),outline=tuple(int(v*.32) for v in color),width=6)
    depth.rectangle((3,3,252,252),outline=65,width=6)
    draw.line((10,12,244,12),fill=tuple(min(255,int(v*1.45)) for v in color),width=2)
    draw.line((12,10,12,244),fill=tuple(min(255,int(v*1.2)) for v in color),width=2)
    for x,y in [(18,18),(237,18),(18,237),(237,237)]:
        draw.ellipse((x-3,y-3,x+3,y+3),fill=(19,23,27));depth.ellipse((x-3,y-3,x+3,y+3),fill=68)
        draw.line((x-2,y,x+2,y),fill=(105,114,117),width=1)
    if i in [0,1,4,11]:
        for y in [57,191]:
            draw.rounded_rectangle((34,y,222,y+9),radius=3,fill=tuple(int(v*.44) for v in color))
            depth.rounded_rectangle((34,y,222,y+9),radius=3,fill=95)
        draw.line((33,31,224,31),fill=tuple(min(255,int(v*1.22)) for v in color),width=1)
    if i in [2,8,9,10,12,15]:
        # Flush inspection hatch, nested plates and load-bearing edge scars.
        draw.rectangle((33,45,218,210),outline=tuple(int(v*.64) for v in color),width=2)
        depth.rectangle((33,45,218,210),outline=111,width=2)
        draw.rectangle((158,64,202,113),fill=tuple(int(v*.75) for v in color))
        for y in [72,83,94,105]:draw.line((165,y,196,y),fill=(16,21,25),width=2)
        for k in range(7):
            y=37+k*24
            draw.line((11,y,17+k%3*4,y+5),fill=(114,109,95),width=1)
        if i==9:
            for y in [87,137,194]:
                draw.line((42,y,113,y+9,165,y+3),fill=(41,28,22),width=3)
                draw.line((42,y+3,113,y+12),fill=(147,84,40),width=1)
    if i==3:
        for k in range(-10,20):
            x=k*37;draw.polygon([(x,0),(x+18,0),(x+274,256),(x+256,256)],fill=(24,26,28))
    if i==13:
        for x in range(20,245,12):
            for y in range(20,245,24):
                draw.rounded_rectangle((x,y,x+6,y+17),radius=2,fill=(5,9,13))
                depth.rounded_rectangle((x,y,x+6,y+17),radius=2,fill=48)
                draw.line((x-1,y,x-1,y+16),fill=(76,85,89),width=1)
    if i==14:
        for y in range(25,236,21):
            draw.rectangle((20,y,235,y+9),fill=(5,9,12));depth.rectangle((20,y,235,y+9),fill=54)
    x=i%4*tile;y=i//4*tile
    base.paste(face,(x,y));height.paste(relief,(x,y))
    metal[y:y+tile,x:x+tile,0]=190 if i in [0,1,4,11,13,14] else 85
    metal[y:y+tile,x:x+tile,3]=135 if i in [1,4,11] else 80
    local=np.asarray(relief);ao[y:y+tile,x:x+tile]=np.where(local<100,145,245)
    if i in [5,6,7]:
        yy,xx=np.mgrid[:tile,:tile];mask=(xx>21)&(xx<235)&(yy>21)&(yy<235)
        em[y:y+tile,x:x+tile][mask]=color
height=np.asarray(height,dtype=float)/255
dy,dx=np.gradient(height);normal=np.stack([-dx*2.2,-dy*2.2,np.ones_like(dx)],axis=2)
normal/=np.linalg.norm(normal,axis=2)[:,:,None]
base.save(DEST/'R1_Industrial_BaseColor.png')
for name,data in [('Normal',np.clip((normal*.5+.5)*255,0,255).astype('uint8')),('MetallicSmoothness',metal),('Occlusion',ao),('Emission',em)]:
    Image.fromarray(data).save(DEST/f'R1_Industrial_{name}.png')
print('R2_AUTHORED_SURFACE_HIERARCHY',len(palette),'functional cells')
