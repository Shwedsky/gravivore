"""Compare the actual authored Blender blockout with the user's approved board."""
import json
import hashlib
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'docs/visual-production-v2/modeling'
EVIDENCE=OUT/'g0-evidence'
reference=Path.home()/'Downloads'/'Концепт GRAVIVORE_ Мехи и окружение.png'
board=Image.open(reference).convert('RGB')
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',25)
small=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',20)
canvas=Image.new('RGB',(1600,1180),'#20272f')
draw=ImageDraw.Draw(canvas)
draw.text((35,25),'G-0 / APPROVED PROTOTYPE VS AUTHORED BLOCKOUT V1',font=font,fill='white')
draw.text((35,80),'APPROVED TIER 0 / supplied board crop',font=small,fill='#b3c1ce')
crop_box=(18,140,155,284)
crop=board.crop(crop_box)
crop.thumbnail((400,420))
crop=crop.resize((400,420))
canvas.paste(crop,(35,130))
draw.text((485,80),'ACTUAL BLENDER MESH / neutral swatches',font=small,fill='#b3c1ce')
render=Image.open(EVIDENCE/'04_G0_threequarter.png').convert('RGB')
render.thumbnail((1060,810))
canvas.paste(render,(485,130))
phone=Image.open(EVIDENCE/'05_G0_top_gameplay_angle.png').convert('RGB')
phone.thumbnail((243,324))
canvas.paste(phone,(105,600))
draw.text((35,565),'PHONE-SIZE ANGLE CHECK / Blender proxy',font=small,fill='#b3c1ce')
lines=['Preserved: low four-support stance; forward mandibles;',
       'cyan central cavity; pale layers over dark mechanisms.',
       '',
       'Blockout only: symmetry, shell curvature and core-rim integration',
       'need art-direction review. No final textures, rig, LOD or Unity handoff.',
       '',
       'All geometry authored in Blender. Zero donor parts.',
       'Prototype is a small supplied image crop, not a fabrication drawing.']
for i,line in enumerate(lines):
    draw.text((485,985+i*23),line,font=small,fill='white')
canvas.save(EVIDENCE/'07_G0_prototype_comparison.png')
manifest=dict(reference_filename=reference.name,
    reference_sha256=hashlib.file_digest(reference.open('rb'),'sha256').hexdigest(),
    crop_box_pixels=crop_box,images=[])
for p in sorted(EVIDENCE.glob('*.png')):
    with Image.open(p) as im:
        im.verify()
    with Image.open(p) as im:
        manifest['images'].append(dict(file=p.name,size=list(im.size),bytes=p.stat().st_size,
            sha256=hashlib.file_digest(p.open('rb'),'sha256').hexdigest()))
(OUT/'data/g0_evidence_manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print('Seven required evidence PNGs and original-board comparison recorded.')
