"""Lay out actual Blender inspection renders with measured labels."""
import json
import math
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'docs/visual-production-v2/modeling'
IMAGES = OUT / 'source-review'
FONT = ImageFont.truetype('C:/Windows/Fonts/arial.ttf', 18)
SMALL = ImageFont.truetype('C:/Windows/Fonts/arial.ttf', 14)

def sheet(name, records, columns=3, tile=360):
    height = tile + 65
    canvas = Image.new('RGB', (columns*tile, math.ceil(len(records)/columns)*height+60), '#20262d')
    draw = ImageDraw.Draw(canvas)
    draw.text((18,18), name.replace('_',' ').upper() + ' / ACTUAL SOURCE CLAY REVIEW', font=FONT, fill='white')
    for index, record in enumerate(records):
        x, y = index%columns*tile, index//columns*height+60
        key = record.get('id') if name in ('Scout','Cutter','Magnetar') else Path(record['path']).stem
        image_path = IMAGES/(key+'_threequarter.png')
        if not image_path.exists():
            image_path = ROOT/'.asset-intake-tmp/environment-tiles'/(key+'_threequarter.png')
        image = Image.open(image_path).convert('RGB')
        image.thumbnail((tile, tile))
        canvas.paste(image, (x+(tile-image.width)//2,y))
        draw.text((x+8,y+tile+5), key, font=SMALL, fill='white')
        draw.text((x+8,y+tile+27), f"{record['triangles']:,} base / {record['evaluated_triangles']:,} evaluated tris", font=SMALL, fill='#c0c8d0')
        draw.text((x+8,y+tile+45), f"{len(record['meshes'])} meshes / {len(record['armatures'])} rigs / {len(record['actions'])} actions", font=SMALL, fill='#c0c8d0')
    canvas.save(IMAGES/(name+'_contact_sheet.png'))

candidates = json.loads((OUT/'data/candidates_audit.json').read_text())
for category in ('scout','cutter','magnetar'):
    sheet(category.title(), [r for r in candidates if '/'+category+'/' in r['path']])
environment = json.loads((OUT/'data/environment_audit.json').read_text())
for category in sorted({Path(r['path']).parent.name for r in environment}):
    records = [r for r in environment if Path(r['path']).parent.name == category]
    for start in range(0, len(records), 24):
        sheet('Environment_'+category+'_'+str(start//24+1),records[start:start+24], columns=4, tile=270)
print('Contact sheets generated for', len(candidates), 'candidates and', len(environment), 'environment modules')
