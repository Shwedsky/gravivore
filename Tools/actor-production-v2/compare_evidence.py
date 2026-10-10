"""Compose labeled QA boards from actual renders; never changes authority bytes."""
from pathlib import Path
import argparse, json, hashlib
from PIL import Image, ImageOps, ImageDraw, ImageFont
ROOT=Path(__file__).resolve().parents[2]
E=ROOT/'docs/history/implementation-passes/chapter01-actor-production-v2'
parser=argparse.ArgumentParser(); parser.add_argument('--donor-previews',type=Path,required=True); args=parser.parse_args()
reference=ROOT/'docs/visual-blueprints/chapter01-actors/CHAPTER01_ACTOR_VISUAL_TARGETS_V2.png'
expected='67960c1254133c48b1de97d2dd635a5969cea49b949a7e0e8981afd34738099f'
assert hashlib.sha256(reference.read_bytes()).hexdigest()==expected
board=Image.open(reference).convert('RGB'); w,h=board.size
out=E/'comparisons'; out.mkdir(exist_ok=True)
actors=['Scout','Cutter','Warden','ArcDrone','Carrier','Magnetar','Custodian']
donors=['quaternius-animated-mech-pack-George.png','quaternius-scifi-essentials-Enemy_QuadShell.png','msgdi-medium-mech-striker-MediumMechStriker.png','quaternius-scifi-essentials-Enemy_EyeDrone.png','unityfan-scifi-vehicle-012-sci-fi_vehicle_013_2.png','quaternius-scifi-essentials-Enemy_Trilobite.png','quaternius-scifi-essentials-Enemy_Trilobite.png']
try: font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',26); small=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',18)
except OSError: font=small=ImageFont.load_default()
def fit(img,width,height): return ImageOps.contain(img.convert('RGB'),(width,height),Image.Resampling.LANCZOS)
manifest=[]
for index,(actor,donor) in enumerate(zip(actors,donors)):
 image=Image.new('RGB',(1480,980),(19,26,34)); draw=ImageDraw.Draw(image); draw.text((24,15),actor+' / approved concept vs authored source vs Unity production camera',font=font,fill='white')
 left=(.137+index*.1224)*w; right=(.137+(index+1)*.1224)*w
 crop=board.crop((int(left),int(h*.09),min(w,int(right)),int(h*.661)))
 for img,x,width,label in [(crop,20,340,'Owner PNG crop / unchanged authority'),(Image.open(E/'blender'/(actor+'_threequarter.png')),390,640,'Actual editable Blender source'),(Image.open(E/'unity'/(actor+'_gameplay_0.png')),1060,400,'Actual portrait camera + G-0')]:
  rendered=fit(img,width,840); image.paste(rendered,(x+(width-rendered.width)//2,83)); draw.text((x,58),label,font=small,fill=(184,207,226))
 draw.text((24,944),'Concept interpretation and device acceptance remain review gates. Camera view is not zoomed to enlarge the actor.',font=small,fill=(200,204,210))
 path=out/(actor+'_concept_Unity.jpg'); image.save(path,quality=92)
 compare=Image.new('RGB',(1600,850),(24,30,38)); d=ImageDraw.Draw(compare)
 d.text((20,15),actor+' / inspected donor mechanics vs project-owned production source',font=font,fill='white')
 for p,x in [(args.donor_previews/donor,20),(E/'blender'/(actor+'_threequarter.png'),810)]:
  img=fit(Image.open(p),770,740); compare.paste(img,(x+(770-img.width)//2,65))
 d.text((20,800),'Donor inspection reference / no source bytes reused',font=small,fill=(184,207,226))
 d.text((810,800),'100% newly authored geometry, rig, clips and surfaces',font=small,fill=(184,207,226))
 compare.save(out/(actor+'_donor_production.jpg'),quality=92)
 manifest.append({'actor':actor,'concept':str(path.relative_to(ROOT)),'donor_preview':donor,'donor_reused':False,'note':'For Magnetar/Custodian this is a joint/support reference, not a full actor donor.' if index>=5 else 'Comparison of exact inspected mechanics source.'})
(E/'COMPARISON_MANIFEST.json').write_text(json.dumps({'authority_sha256':expected,'items':manifest},indent=2))
studio=Image.new('RGB',(1920,1100),(19,26,34)); sd=ImageDraw.Draw(studio)
sd.text((20,18),'Complete authored family / independent studio framing / actual scale is in Unity Complete_family.png',font=font,fill='white')
phones=Image.new('RGB',(1520,1450),(19,26,34)); pd=ImageDraw.Draw(phones)
pd.text((20,15),'Actual portrait camera / automatic LOD / unchanged G-0',font=font,fill='white')
for i,actor in enumerate(actors):
 x=(i%4)*480; y=70+(i//4)*510
 img=fit(Image.open(E/'blender'/(actor+'_threequarter.png')),470,470); studio.paste(img,(x+(480-img.width)//2,y+28)); sd.text((x+12,y),actor,font=font,fill='white')
 x=(i%4)*380; y=65+(i//4)*690
 phones.paste(Image.open(E/'unity'/(actor+'_phone.png')).convert('RGB'),(x+10,y+30)); pd.text((x+12,y),actor,font=font,fill='white')
sd.text((1460,630),'7 hostile actors\nProject-owned sources\n3 LODs / Generic rigs\n30 FPS in-place clips\nDevice gate pending',font=font,fill=(200,220,235),spacing=18)
pd.text((1160,850),'Camera from main:\nOffset 0 / 14.8 / -11.2\nFOV 46 / portrait 9:16\nG-0 unscaled: 1.771 m\nDevice gate pending',font=small,fill=(200,220,235),spacing=15)
studio.save(out/'Complete_authored_family.jpg',quality=92); phones.save(out/'Complete_phone_family.jpg',quality=92)
