"""Original deterministic wear atlas; adapted EXE/CC0 UI utility atlas."""
import json,math,random,hashlib
from pathlib import Path
from PIL import Image,ImageDraw,ImageOps,ImageEnhance
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/_Game/Content/VisualReplacementV3/Textures'
OUT.mkdir(parents=True,exist_ok=True)
palette=[(.055,.078,.092),(.27,.34,.38),(.49,.56,.57),(.14,.18,.20),(.35,.23,.10),(.85,.035,.012),(1,.28,.027),(.035,.54,.65),(.014,.022,.029),(.12,.071,.043),(.21,.27,.29),(.34,.39,.40),(.10,.17,.20),(.42,.30,.12),(.20,.25,.27),(.032,.049,.061)]
base=Image.new('RGBA',(1024,512));em=Image.new('RGBA',base.size,(0,0,0,255));metal=Image.new('RGBA',base.size)
rng=random.Random(440044)
for i,c in enumerate(palette):
 for y in range(512):
  for x in range(i*64,(i+1)*64):
   wear=1+rng.uniform(-.10,.10)+.11*math.sin(y*.033)*math.cos(x*.071)
   # Toned finish with localized grease, rather than random rainbow stock color.
   grime=.72 if ((x*17+y*7)%211)<13 and i not in (5,6,7) else 1
   base.putpixel((x,y),tuple(int(min(1,v*wear*grime)*255) for v in c)+(255,))
   if i in (5,6,7):em.putpixel((x,y),tuple(int(v*255) for v in c)+(255,))
   metal.putpixel((x,y),(175 if i in (1,2,10,11,14) else 90,0,0,105 if i in (1,2) else 65))
draw=ImageDraw.Draw(base)
for i in (0,1,2,3,10,11,14):
 for n in range(42):
  x=i*64+rng.randrange(6,52);y=rng.randrange(10,496);length=rng.randrange(3,16)
  draw.line((x,y,min(i*64+59,x+length),y+rng.randrange(-1,2)),fill=tuple(int(min(1,c+.08)*255) for c in palette[i])+(255,),width=1)
for label,im in [('BaseColor',base),('Emission',em),('MetallicSmoothness',metal)]:im.save(OUT/('VR3_'+label+'.png'))
# The CC0 MegaKit trim detail is fundamental to the low-poly modules. Retain it
# in one four-quadrant atlas with adapted cold-metal albedo and packed PBR maps.
source=next((ROOT/'ExternalAssetIntake/FreeAssetIntakeV1/_work_v2/extracted/p15/content').rglob('T_Trim_01_BaseColor.png')).parent
texture_receipts=[]
for kind in ('BaseColor','Normal','MetallicSmoothness'):
 sheet=Image.new('RGBA',(2048,2048),(128,128,255,255) if kind=='Normal' else (40,50,58,255))
 for index,prefix in enumerate(('T_Trim_01','T_Trim_02','T_Trim_03','T_PaddedWall')):
  suffix='ORM' if kind=='MetallicSmoothness' else kind
  texture_source=source/(prefix+'_'+suffix+'.png')
  texture_receipts.append({'sourceId':'quaternius-megakit','license':'CC0','sourcePath':str(texture_source.relative_to(ROOT)).replace('\\','/'),'sha256':hashlib.sha256(texture_source.read_bytes()).hexdigest(),'output':'VR3_Trim_'+kind+'.png','adaptation':'Cold-metal albedo; original trim UV detail; quadrant atlas; packed metallic/smoothness'})
  im=Image.open(source/(prefix+'_'+suffix+'.png')).convert('RGBA').resize((1024,1024),Image.Resampling.LANCZOS)
  if kind=='BaseColor':
   grey=ImageOps.grayscale(im);im=ImageOps.colorize(grey,(10,17,22),(132,154,161)).convert('RGBA')
  elif kind=='MetallicSmoothness':
   _,rough,metallic,_=im.split();im=Image.merge('RGBA',(metallic,Image.new('L',im.size,0),Image.new('L',im.size,0),ImageOps.invert(rough)))
  # Pillow top-left becomes Unity high-v; invert quadrant y when packing.
  sheet.paste(im,((index%2)*1024,(1-index//2)*1024))
 sheet.save(OUT/('VR3_Trim_'+kind+'.png'))
(ROOT/'docs/visual-replacement-v3/texture_provenance.json').write_text(json.dumps(texture_receipts,indent=2))
# EXE frames use their actual contours; recolor only and keep transparency.
manifest=json.loads((ROOT/'docs/visual-replacement-v3/ingestion_manifest.json').read_text(encoding='utf-8'))
for e in manifest:
 if not e['sourcePath'].endswith('.png'):continue
 im=Image.open(ROOT/e['sourcePath']).convert('RGBA')
 if e['sourceId']=='ui-exe':
  alpha=im.getchannel('A');tint=Image.new('RGBA',im.size,(180,216,227,255));tint.putalpha(alpha);im=tint
 im.save(OUT/(e['outputName']+'.png'))
print('VR3_SURFACES_PASS')
