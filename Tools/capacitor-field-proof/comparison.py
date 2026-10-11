"""Evidence layout only: unretouched production captures and owner board crop."""
from pathlib import Path
import json, shutil
from PIL import Image, ImageDraw, ImageFont
ROOT=Path(__file__).resolve().parents[2]
OWNER=ROOT.parents[1]
OUT=ROOT/'docs/history/implementation-passes/capacitor-field-proof'
BASE=OWNER/'.codex-worktrees/chapter01-r2-actors-v2-integration/docs/history/implementation-passes/chapter01-r2-actors-v2-integration/full-chapter/internal'
(OUT/'baseline-apk50').mkdir(parents=True,exist_ok=True)
for folder in ['', 'no-ui/']:
    src=BASE/folder/'03_capacitor-field.png'
    shutil.copyfile(src,OUT/'baseline-apk50'/('with-ui.png' if not folder else 'no-ui.png'))
board=Image.open(ROOT/'docs/visual-blueprints/chapter01/CHAPTER01_VISUAL_BLUEPRINT_V1.jpg').convert('RGB')
concept=board.crop((1144,50,1527,266));concept.save(OUT/'concept-sector-03.png')
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',25)
small=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',18)
page=Image.new('RGB',(1450,1120),(12,18,25));draw=ImageDraw.Draw(page)
draw.text((30,20),'GRAVIVORE / CAPACITOR FIELD / PLAYABLE ENVIRONMENT PROOF',font=font,fill=(211,222,232))
for x,title in [(30,'Owner Blueprint / Sector 03'),(500,'APK 50 / archived gameplay'),(970,'New sector / production camera')]:draw.text((x,70),title,font=small,fill=(133,190,210))
concept.thumbnail((440,500));page.paste(concept,(30,110))
draw.text((30,405),'Original board crop; no reconstruction.',font=small,fill=(172,183,193))
draw.text((30,455),'New fixed follow camera:',font=small,fill=(172,183,193))
draw.text((30,487),'offset (3.4, 19.8, -13.6), FOV 48',font=small,fill=(172,183,193))
draw.text((30,519),'Same camera for all new gameplay views.',font=small,fill=(172,183,193))
draw.text((30,570),'Editor production-scene evidence.',font=small,fill=(172,183,193))
draw.text((30,602),'Android device acceptance remains pending.',font=small,fill=(172,183,193))
for x,path in [(500,OUT/'baseline-apk50/no-ui.png'),(970,OUT/'gameplay-no-ui/02_combat.png')]:
    frame=Image.open(path).convert('RGB');frame.thumbnail((440,920));page.paste(frame,(x,110))
draw.text((30,1050),'Screenshots are unretouched. APK 50 keeps its original camera; the new paired UI/no-UI views share the revised working camera.',font=small,fill=(155,171,182))
page.save(OUT/'concept-apk50-new-comparison.png')
names=['01_entry','02_combat','03_installation_approach','04_edge','05_return_exit']
sheet=Image.new('RGB',(1350,1020),(12,18,25));d=ImageDraw.Draw(sheet)
for i,name in enumerate(names):
    d.text((i*270+8,10),name,font=small,fill=(211,222,232))
    for j,folder in enumerate(['gameplay-ui','gameplay-no-ui']):
        frame=Image.open(OUT/folder/(name+'.png')).convert('RGB');frame.thumbnail((264,470));sheet.paste(frame,(i*270+3,45+j*485))
sheet.save(OUT/'five-gameplay-positions.png')
print('CF_REAL_CAPTURE_COMPARISONS_WRITTEN')
