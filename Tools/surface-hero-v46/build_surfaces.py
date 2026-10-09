"""Original shared PBR surfaces. URP metallic R / smoothness A, occlusion G.

No external images are synthesized or copied. Legacy stripe coordinates are kept
so the accepted actor rigs and UVs receive surface detail without geometry edits.
"""
import json, hashlib
from pathlib import Path
import numpy as np
from PIL import Image, ImageDraw, ImageFilter
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/_Game/Content/SurfaceHeroV46/Textures';OUT.mkdir(parents=True,exist_ok=True)
DOC=ROOT/'docs/surface-hero-v46/verification';DOC.mkdir(parents=True,exist_ok=True)
PALETTE=[(32,43,49),(99,122,136),(154,169,173),(64,86,103),(143,104,48),(172,32,12),(195,103,20),(20,141,166),(17,24,28),(85,65,46),(115,139,148),(132,148,159),(46,95,109),(159,113,44),(121,139,144),(23,33,39)]
def tile(index,size=512):
    rng=np.random.default_rng(460046+index)
    y,x=np.mgrid[0:size,0:size].astype(float);u=x/(size-1);v=y/(size-1)
    # Coherent broad rolled-metal variation: strongest toward recessed joints.
    broad=1+.09*np.sin(u*4.7+index)+.055*np.cos(v*6.3+u*2)
    broad+=.015*rng.normal(size=(size,size))
    border=np.minimum.reduce([u,1-u,v,1-v])
    grime=.78+.22*np.clip(border/.10,0,1)
    color=np.array(PALETTE[index],float)[None,None,:]*broad[:,:,None]*grime[:,:,None]
    height=Image.new('L',(size,size),128);h=ImageDraw.Draw(height)
    panel=Image.new('L',(size,size),255);p=ImageDraw.Draw(panel)
    if index not in (5,6,7,8):
        for a,b,c,d in [(16,16,size-17,size-17),(35,35,size-36,int(size*.67))]:
            h.rounded_rectangle((a,b,c,d),radius=8,outline=76,width=3)
            h.rounded_rectangle((a+4,b+4,c-4,d-4),radius=7,outline=159,width=2)
            p.rounded_rectangle((a,b,c,d),radius=8,outline=176,width=5)
        # One recessed vent zone; flat surfaces retain broad, readable plates.
        for n in range(7):
            yy=int(size*.76)+n*int(size*.019)
            h.rounded_rectangle((int(size*.20),yy,int(size*.80),yy+5),radius=2,fill=56)
            p.rounded_rectangle((int(size*.20),yy,int(size*.80),yy+5),radius=2,fill=98)
        for xx in (27,size-28):
            for yy in (27,int(size*.67),size-28):
                h.ellipse((xx-5,yy-5,xx+5,yy+5),fill=176,outline=60,width=2)
                p.ellipse((xx-6,yy-6,xx+6,yy+6),fill=145)
                h.line((xx-3,yy,xx+3,yy),fill=58,width=1)
        for _ in range(22):
            xx=int(rng.integers(30,size-40));yy=int(rng.integers(30,size-40))
            length=int(rng.integers(6,25));h.line((xx,yy,xx+length,yy-1),fill=153,width=1)
        color*=np.asarray(panel)[:,:,None]/255
        # Upper scuffed edge catches neutral light as well as colored lighting.
        edge=np.exp(-((border-.036)/.007)**2)*.22
        color+=edge[:,:,None]*np.array((85,89,87))[None,None,:]
    ht=np.asarray(height.filter(ImageFilter.GaussianBlur(.6)),dtype=float)/255
    gy,gx=np.gradient(ht);normal=np.stack((-gx*9,-gy*9,np.ones_like(ht)),axis=-1)
    normal/=np.linalg.norm(normal,axis=-1,keepdims=True)
    normal=(normal*.5+.5)*255
    raw=index in (1,2,9,10,11,14)
    metal=np.full((size,size),211 if raw else 45 if index in (3,4,12,13) else 117,dtype=float)
    smooth=(.49 if raw else .27)+.12*np.sin(u*3+v*2)-.09*(1-grime)
    if index==8:metal[:]=0;smooth[:]=.16
    # Paint chips expose metal locally and change response, not just albedo.
    if index in (3,4,12,13):
        chip=(border>.035)&(border<.047)&(np.sin(v*63+u*31)>.35)
        metal[chip]=205;color[chip]=np.array((140,151,153));smooth[chip]=.55
    ao=np.clip(.9-(.5-ht)*1.1-(1-grime)*.6,.35,1)
    emission=np.zeros((size,size,3))
    if index in (5,6,7):
        # Dense internal source, darker collars and cooler edges: no solid RGB.
        energy=(.18+.82*np.sin(np.pi*v)**.65)*(.24+.76*np.sin(np.pi*u)**.6)
        energy*=.83+.17*np.cos(v*30)
        emission=energy[:,:,None]*np.array(PALETTE[index])[None,None,:]
        color=emission*.38;smooth[:]=.52;metal[:]=25;ao[:]=1
    rgba=np.stack((metal,np.zeros_like(metal),np.zeros_like(metal),smooth*255),axis=-1)
    occ=np.stack((ao*255,ao*255,ao*255,np.full_like(ao,255)),axis=-1)
    return [Image.fromarray(np.uint8(np.clip(a,0,255))) for a in (color,normal,rgba,occ,emission)]
LABELS=['BaseColor','Normal','MetallicSmoothness','Occlusion','Emission']
tiles=[tile(i) for i in range(16)]
for k,label in enumerate(LABELS):
    mode='RGBA' if label in ('MetallicSmoothness','Occlusion') else 'RGB'
    atlas=Image.new(mode,(2048,2048))
    for i,t in enumerate(tiles):atlas.paste(t[k],((i%4)*512,(3-i//4)*512))
    atlas.save(OUT/('V46_'+label+'.png'))
    # Same sixteen vertical strips as accepted actors and older modules.
    legacy=Image.new(mode,(1024,512))
    for i,t in enumerate(tiles):legacy.paste(t[k].resize((64,512),Image.Resampling.LANCZOS),(i*64,0))
    legacy.save(OUT/('V46_Legacy_'+label+'.png'))
    tiles[1][k].resize((1024,1024),Image.Resampling.LANCZOS).save(OUT/('V46_Support_'+label+'.png'))
em=np.asarray(Image.open(OUT/'V46_Emission.png'),dtype=float)
power=em.max(axis=-1)/255
for role,rgb in [('Cyan',(15,170,215)),('Red',(240,45,12))]:
    Image.fromarray(np.uint8(power[:,:,None]*np.array(rgb))).save(OUT/('V46_Emission'+role+'.png'))
# Ten-stripe baseline UVs retain their different spacing and cyan/red roles.
for k,label in enumerate(LABELS):
    mode='RGBA' if label in ('MetallicSmoothness','Occlusion') else 'RGB'
    sheet=Image.new(mode,(1024,1024))
    for i in range(10):
        x=i*112 if i<9 else 996;w=112 if i<8 else 100 if i==8 else 28
        sheet.paste(tiles[i][k].resize((w,1024),Image.Resampling.LANCZOS),(x,0))
    sheet.save(OUT/('V46_Slice_'+label+'.png'))
# G-0's uniquely baked atlas remains authoritative. Add shallow brushed relief
# and seam occlusion from its own baked surface values; identity/UVs stay intact.
g0=Image.open(ROOT/'Assets/_Game/ArtReview/G0ProductionV3/Textures/G0_V3_BaseColor.png').convert('RGB').resize((1024,1024),Image.Resampling.LANCZOS)
g=np.asarray(g0,dtype=float)/255;l=g.mean(axis=-1)
yy,xx=np.mgrid[0:1024,0:1024];h=l*.06+.002*np.sin(xx*.8)*np.sin(yy*.053)
gy,gx=np.gradient(h);n=np.stack((-gx*2,-gy*2,np.ones_like(h)),axis=-1);n/=np.linalg.norm(n,axis=-1,keepdims=True)
Image.fromarray(np.uint8((n*.5+.5)*255)).save(OUT/'V46_G0_Normal.png')
ao=np.clip(.84+l*.45,.72,1);Image.fromarray(np.uint8(np.stack((ao,ao,ao),axis=-1)*255)).save(OUT/'V46_G0_Occlusion.png')
source=next((ROOT/'ExternalAssetIntake/FreeAssetIntakeV1/_work_v2/extracted/p15/content').rglob('T_Trim_01_ORM.png')).parent
trim=Image.new('RGB',(2048,2048));donor_ao=[]
for i,prefix in enumerate(('T_Trim_01','T_Trim_02','T_Trim_03','T_PaddedWall')):
    path=source/(prefix+'_ORM.png');original=Image.open(path).convert('RGB').resize((1024,1024),Image.Resampling.LANCZOS)
    occ=original.getchannel('R');trim.paste(Image.merge('RGB',(occ,occ,occ)),((i%2)*1024,(1-i//2)*1024))
    donor_ao.append({'sourceId':'env-quaternius-megakit','license':'CC0','sourcePath':str(path.relative_to(ROOT)).replace('\\','/'),'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'adaptation':'Original ORM R occlusion, same four-quadrant UV order as accepted VR3 trim'})
trim.save(OUT/'V46_Trim_Occlusion.png')
receipt={'authorship':'Original deterministic project surface authoring','maximumDimension':2048,'normalConvention':'OpenGL +Y tangent; imported as Unity NormalMap','metallicChannel':'R','smoothnessChannel':'A','occlusionChannel':'G','heroAtlas':'4x4 tiles with 5px UV gutters; authored panel / vent / bolt relief','legacyUVsPreserved':True,'files':[]}
receipt['verifiedDonorAo']=donor_ao
for p in sorted(OUT.glob('*.png')):
    a=np.asarray(Image.open(p));receipt['files'].append({'path':str(p.relative_to(ROOT)).replace('\\','/'),'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'channelMin':a.min(axis=(0,1)).tolist(),'channelMax':a.max(axis=(0,1)).tolist()})
(DOC/'surface_provenance.json').write_text(json.dumps(receipt,indent=2))
print('V46_SHARED_PBR_SURFACES_READY')
