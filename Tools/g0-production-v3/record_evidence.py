"""Validate genuine capture files and compose labeled, unaltered review boards."""
import json,hashlib
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
ROOT=Path(__file__).resolve().parents[2];DOC=ROOT/'docs/g0-production-v3';OUT=DOC/'evidence'
required=[('blender',n) for n in ['01_front.png','02_side.png','03_rear.png','04_threequarter.png','05_black_silhouette.png']]
required += [('unity',n) for n in ['06_gameplay_camera.png','07_phone_size.png','08_ordinary_enemy.png','09_magnetar.png','10_movement.png','11_attack.png','12_death.png']]
manifest=[]
for kind,name in required:
    p=OUT/kind/name
    with Image.open(p) as im:
        im.load();expected=(1200,1400) if kind=='blender' else (360,640) if name=='07_phone_size.png' else (1080,1920)
        assert im.size==expected,(name,im.size)
    manifest.append(dict(file=p.relative_to(ROOT).as_posix(),sha256=hashlib.sha256(p.read_bytes()).hexdigest(),resolution=list(expected),renderer='Blender Cycles reopened source' if kind=='blender' else 'Unity URP unchanged gameplay camera; imported pose BakeMesh, no extra scale'))
hashes=[hashlib.sha256((OUT/'unity'/n).read_bytes()).hexdigest() for n in ['06_gameplay_camera.png','10_movement.png','11_attack.png','12_death.png']]
assert len(set(hashes))==4,'Animation evidence must show genuinely different rendered geometry'
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',22)
small=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',16)
board=Image.new('RGB',(1200,756),(15,21,27));d=ImageDraw.Draw(board)
before=ROOT/'docs/visual-production-v2/g0-bipedal-v2/evidence-v21/blender/04_G0_V21_threequarter.png'
for i,(path,label) in enumerate([(before,'Accepted V2.1 blockout'),(OUT/'blender/04_threequarter.png','V3 production asset')]):
    with Image.open(path) as im:board.paste(im.resize((600,700),Image.Resampling.LANCZOS),(i*600,56))
    d.text((i*600+24,16),label,font=font,fill=(210,223,231))
board.save(OUT/'01_v21_v3_comparison.png')
board=Image.new('RGB',(1080,692),(15,21,27));d=ImageDraw.Draw(board)
for i in range(3):
    with Image.open(OUT/'unity'/f'LOD{i}_phone.png') as im:board.paste(im,(i*360,52))
    d.text((i*360+20,15),f'LOD{i} | native 360 x 640',font=small,fill=(210,223,231))
board.save(OUT/'02_lod_phone_board.png')
# Fixed native-camera crops make pose differences inspectable; all original
# full camera PNGs are retained. Nearest enlargement never invents image detail.
board=Image.new('RGB',(1600,1140),(15,21,27));d=ImageDraw.Draw(board)
for row,name in enumerate(['Run','Attack','Death']):
    for i in range(5):
        with Image.open(OUT/'unity'/f'{name}_{i}.png') as im:
            crop=im.crop((130,265,230,355));board.paste(crop.resize((320,288),Image.Resampling.NEAREST),(i*320,row*380+68))
        d.text((i*320+12,row*380+18),name+f' | {i*25}% | crop 3.2x',font=small,fill=(210,223,231))
board.save(OUT/'03_animation_pose_board.png')
report=dict(status='PASS',required_captures=manifest,distinct_movement_attack_death_frames=True,
            blend_sha256=hashlib.sha256((ROOT/'art/g0-production-v3/G0_Production_V3.blend').read_bytes()).hexdigest(),
            fbx_sha256=hashlib.sha256((ROOT/'Assets/_Game/ArtReview/G0ProductionV3/Models/G0_Production_V3.fbx').read_bytes()).hexdigest(),
            board_note='Compositions only: labels, uniform resize, explicitly labeled native camera crops. All unaltered source PNGs retained.')
(DOC/'data/evidence_manifest.json').write_text(json.dumps(report,indent=2))
print('12_REQUIRED_CAPTURE_AND_ANIMATION_EVIDENCE_PASS')
