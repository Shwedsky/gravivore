from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json,html
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'docs/history/implementation-passes/chapter01-complete-concept'
font=lambda n:ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',n)
names=['Repair Hub','Relay Yard','Capacitor Field','Cutting Floor','Shield Dump','Hauler Graveyard','Magnetar Complex','Custodian Core']
ids=['repair-hub','relay-yard','capacitor-field','cutting-floor','shield-dump','hauler-graveyard','elite-arena','boss-arena']
functions=['Сервисная платформа / диагностика / ремонт','Открытый док / лёгкая сборка / связь','Конденсаторы / распределение / кабельные каналы','Портальный пресс / режущий центр / загрузка','Броневые кассеты / каретка / сортировка','Повреждённые корпуса / погрузочная структура','Магнитная арка / индукционные полюса','Экранированный реактор / пространство босса']
ref=Image.open(ROOT/'docs/visual-blueprints/chapter01/CHAPTER01_VISUAL_BLUEPRINT_V1.jpg').convert('RGB')
crops=[(350,55,733,258),(749,55,1131,257),(1147,55,1523,257),(350,374,733,586),(749,374,1131,586),(1147,374,1523,586),(350,711,733,929),(749,711,1131,929)]
for i,(name,region) in enumerate(zip(names,ids)):
    sheet=Image.new('RGB',(1240,875),(10,16,22));d=ImageDraw.Draw(sheet)
    d.text((20,13),f'{i+1}. {name}',font=font(28),fill='#d3e3ed');d.text((20,52),functions[i],font=font(19),fill='#85c4dc')
    concept=ref.crop(crops[i]);concept.thumbnail((390,265));sheet.paste(concept,(20,108))
    d.text((20,82),'Утверждённый Blueprint',font=font(17),fill='#b4c1ca')
    labels=['Вход','Центр / действующие актёры','Возле главной установки']
    for j,view in enumerate(['entry','centre','machine']):
        path=OUT/'gameplay-no-ui'/f'{i+1:02}_{region}_{view}.png'
        im=Image.open(path).convert('RGB');im.thumbnail((255,725));x=425+j*265
        sheet.paste(im,(x,108));d.text((x,82),labels[j],font=font(15),fill='#b4c1ca')
    d.text((20,390),'Одна игровая камера:',font=font(19),fill='#d3e3ed')
    d.text((20,423),'offset (3.4, 19.8, -13.6), FOV 48°',font=font(16),fill='#a8b8c4')
    d.text((20,454),'Linear / URP / ACES / Bloom 0.12',font=font(16),fill='#a8b8c4')
    d.text((20,810),'Реальные кадры Editor, без ретуши. UI снят на том же игровом такте. Android визуальный gate отдельно.',font=font(17),fill='#a8b8c4')
    sheet.save(OUT/f'{i+1:02}-{region}-concept-review.png')
overview=Image.new('RGB',(1480,2440),(10,16,22));d=ImageDraw.Draw(overview)
d.text((24,15),'GRAVIVORE — CHAPTER 01 / восемь функциональных секторов',font=font(29),fill='#d3e3ed')
d.text((24,55),'Blueprint / центр сектора / одна игровая камера / кадры Editor без UI',font=font(20),fill='#85c4dc')
for i in range(8):
    x=24+(i%2)*730;y=105+(i//2)*580
    d.text((x,y),f'{i+1}. {names[i]}',font=font(23),fill='#d3e3ed')
    concept=ref.crop(crops[i]);concept.thumbnail((380,245));overview.paste(concept,(x,y+43))
    game=Image.open(OUT/'gameplay-no-ui'/f'{i+1:02}_{ids[i]}_centre.png').convert('RGB');game.thumbnail((290,515));overview.paste(game,(x+410,y+43))
    d.text((x,y+305),functions[i].split(' / ')[0],font=font(19),fill='#a8b8c4')
d.text((24,2420-20),'Камера и актёры сохранены из APK 52. Технические проверки не заменяют оценку на Android-устройстве.',font=font(18),fill='#a8b8c4')
overview.save(OUT/'chapter01-concept-comparison.png')
for folder in ['gameplay-no-ui','gameplay-ui']:
    images=sorted((OUT/folder).glob('*.png'));images=[p for p in images if not p.name.startswith('silhouette_')]
    cols=6;rows=(len(images)+cols-1)//cols
    board=Image.new('RGB',(cols*200,rows*384),(10,16,22));d=ImageDraw.Draw(board)
    for i,p in enumerate(images):
        im=Image.open(p).convert('RGB');im.thumbnail((190,338));x=(i%cols)*200;y=(i//cols)*384
        board.paste(im,(x,y+26));d.text((x+2,y+4),p.stem[:28],font=font(11),fill='#c4d0d8')
    board.save(OUT/f'{folder}-all-positions.png')
print('CHAPTER_CONCEPT_BOARDS_CREATED')
