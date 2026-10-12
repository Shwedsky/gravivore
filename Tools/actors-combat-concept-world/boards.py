from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
root=Path(__file__).resolve().parents[2]
out=root/'docs/history/implementation-passes/actors-combat-concept-world'
reference=Image.open(root/'docs/visual-blueprints/chapter01-actors/CHAPTER01_ACTOR_VISUAL_TARGETS_V2.png').convert('RGB')
font_path=Path('C:/Windows/Fonts/segoeui.ttf')
font=ImageFont.truetype(str(font_path),22)
small=ImageFont.truetype(str(font_path),16)
actors=['G0','Scout','Cutter','Warden','ArcDrone','Carrier','Magnetar','Custodian']
def screenshot(name):
    p=out/name
    return Image.open(p).convert('RGB')
def fit(im,w,h):
    im=im.copy();im.thumbnail((w,h),Image.Resampling.LANCZOS);return im
rows=[]
for i,actor in enumerate(actors):
    if i<6:
        pre=f'before/cold_{actor}_turn_2.png' if actor!='G0' else 'before/cold_G0.png'
        cold=f'after/cold_{actor}_turn_2.png'
        warm=f'after/warm_{actor}_turn_2.png'
        fight=f'after/cold_{actor}_live_strike.png' if actor!='G0' else 'after/cold_G0_actual_gravity.png'
        quiet=f'after/cold_{actor}_no_emission.png' if actor!='G0' else 'after/cold_G0_actual_move.png'
    else:
        pre=f'before/{actor}_Idle.png';cold=f'after/{actor}_Idle.png';warm=f'after/{actor}_Charge.png' if actor=='Magnetar' else f'after/{actor}_Telegraph.png'
        fight=f'after/{actor}_Vent.png' if actor=='Magnetar' else f'after/{actor}_AttackLine.png';quiet=f'after/{actor}_Death.png'
    board=Image.new('RGB',(1480,650),(12,17,23));d=ImageDraw.Draw(board)
    d.text((24,12),actor+' — unchanged gameplay camera / sRGB evidence',font=font,fill=(225,235,240))
    w,h=reference.size;concept=reference.crop((int(w*(i/8+.004)),int(h*.10),int(w*((i+1)/8-.004)),int(h*.65)))
    concept=fit(concept,230,550);board.paste(concept,(16,75));d.text((16,50),'Approved V2',font=small,fill='white')
    quiet_label='Movement' if actor=='G0' else 'Death' if i>=6 else 'No emission'
    for col,(label,path) in enumerate([('APK53 surfaces / same scene',pre),('Cold / final',cold),('Warm or windup / final',warm),('Action / final',fight),(quiet_label,quiet)]):
        x=260+col*242;d.text((x,52),label,font=small,fill=(175,199,215));im=fit(screenshot(path),235,550);board.paste(im,(x,82))
    board.save(out/(f'{i:02d}-{actor.lower()}-concept-world-review.png'))
    rows.append(board)
sheet=Image.new('RGB',(1480,80+len(rows)*650),(12,17,23));draw=ImageDraw.Draw(sheet)
draw.text((22,18),'GRAVIVORE — actor concept / original surface / cold / warm / action',font=font,fill='white')
for i,row in enumerate(rows): sheet.paste(row,(0,80+i*650))
sheet.save(out/'actor-concept-world-comparison.png')
print('Eight review boards and comparison assembled from actual captures; no retouch.')
