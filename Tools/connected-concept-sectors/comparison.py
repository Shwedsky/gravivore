"""Unretouched owner concept crops and actual production-camera game captures."""
from pathlib import Path
import subprocess
from PIL import Image, ImageDraw, ImageFont
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'docs/history/implementation-passes/connected-concept-sectors'
font=lambda s:ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',s)
board=Image.open(ROOT/'docs/visual-blueprints/chapter01/CHAPTER01_VISUAL_BLUEPRINT_V1.jpg').convert('RGB')
cap=board.crop((1148,56,1524,276));cut=board.crop((350,381,733,574))
cap.save(OUT/'concept-capacitor.png');cut.save(OUT/'concept-cutting.png')
canvas=Image.new('RGB',(1160,1220),(10,16,22));draw=ImageDraw.Draw(canvas)
draw.text((24,18),'GRAVIVORE — ДВА ФУНКЦИОНАЛЬНЫХ СЕКТОРА',font=font(28),fill=(220,232,242))
draw.text((24,58),'Blueprint / игровая сцена / одна рабочая камера',font=font(18),fill=(133,184,208))
for row,(title,concept,name,notes) in enumerate([
    ('Capacitor Field / холодный энергетический узел',cap,'02_combat','Вертикальные камеры, силовая шина, кабельные траншеи'),
    ('Cutting Floor / тяжёлая производственная линия',cut,'07_cutting_combat','Портальная рама, режущий центр, приводы и роликовая загрузка')]):
    y=102+row*538;draw.text((24,y),title,font=font(21),fill=(216,230,238))
    thumb=concept.copy();thumb.thumbnail((366,260));canvas.paste(thumb,(24,y+60))
    draw.text((24,y+306),'Исходный фрагмент Blueprint.',font=font(16),fill=(161,176,188))
    # Wrap the functional comparison without retouching or cropping game pixels.
    words=notes.split();line='';offset=0
    for word in words:
        test=line+' '+word if line else word
        if draw.textlength(test,font=font(17))>360:
            draw.text((24,y+345+offset),line,font=font(17),fill=(161,176,188));offset+=27;line=word
        else:line=test
    draw.text((24,y+345+offset),line,font=font(17),fill=(161,176,188))
    for x,folder,label in [(424,'gameplay-no-ui','Без UI'),(784,'gameplay-ui','С UI и действующими актёрами')]:
        image=Image.open(OUT/folder/(name+'.png')).convert('RGB');image=image.resize((260,462),Image.Resampling.LANCZOS)
        canvas.paste(image,(x,y+60));draw.text((x,y+37),label,font=font(15),fill=(151,197,217))
draw.text((24,1180),'Реальные кадры сцены Editor, без ретуши. Android-проверка на устройстве отдельно.',font=font(17),fill=(148,167,181))
canvas.save(OUT/'two-sectors-concept-comparison.png')

names=['01_entry','02_combat','03_installation_approach','04_edge','05_return_exit','06_cutting_entry','07_cutting_combat','08_cutting_approach','09_cutting_edge','10_cutting_exit','11_transfer_west','12_transfer_junction','13_transfer_east']
labels=['CF: вход','CF: бой','CF: установка','CF: край','CF: выход','Cut: вход','Cut: бой','Cut: установка','Cut: край','Cut: выход','Переход: запад','Переход: узел','Переход: восток']
for folder in ['gameplay-no-ui','gameplay-ui']:
    sheet=Image.new('RGB',(1320,1430),(10,16,22));d=ImageDraw.Draw(sheet);d.text((20,12),'Одна рабочая камера — '+('без UI' if folder.endswith('no-ui') else 'с UI'),font=font(25),fill=(220,232,242))
    for i,(name,label) in enumerate(zip(names,labels)):
        x=12+(i%5)*264;y=56+(i//5)*453
        picture=Image.open(OUT/folder/(name+'.png')).convert('RGB').resize((235,418),Image.Resampling.LANCZOS)
        d.text((x,y),label,font=font(16),fill=(157,195,214));sheet.paste(picture,(x,y+24))
    sheet.save(OUT/(folder+'-positions.png'))
# Keep the previous delivered cold camera frame even when regression tests write
# into inherited history folders; read its immutable Git object.
previous='docs/history/implementation-passes/capacitor-field-proof/gameplay-no-ui/02_combat.png'
(OUT/'apk51-capacitor-baseline.png').write_bytes(subprocess.check_output(['git','show','3793b632790a791f8de861010eaedc236ecc77d1:'+previous],cwd=ROOT))
print('TWO_SECTOR_CONCEPT_AND_ALL_POSITION_BOARDS_READY')
