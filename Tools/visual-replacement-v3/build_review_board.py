"""Build factual comparison boards from production captures and Git V43 evidence."""
import io,subprocess
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
ROOT=Path(__file__).resolve().parents[2];OUT=ROOT/'docs/history/implementation-passes/chapter01-visual-replacement-v3/v44/internal'
font=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',25)
def before(path):
 result=subprocess.run(['git','show','origin/main:'+path],cwd=ROOT,check=True,stdout=subprocess.PIPE)
 return Image.open(io.BytesIO(result.stdout)).convert('RGB')
def board(name,items):
 sheet=Image.new('RGB',(540*len(items),1020),(10,17,21));draw=ImageDraw.Draw(sheet)
 for i,(label,picture) in enumerate(items):
  draw.text((i*540+16,12),label,font=font,fill=(180,214,225));sheet.paste(picture.resize((540,960)),(i*540,60))
 sheet.save(OUT/(name+'.jpg'),quality=92)
board('before_after_repair',[('V43 archived production frame',before('docs/concept-fidelity-v2/internal/01_repair_active.png')),('V44 repair / layered construction',Image.open(OUT/'01_repair_hub.png'))])
board('weapon_rank_progression',[('V43 archived equipped M-0',before('docs/chapter01-v3/internal/g0_bipedal_with_m0.png')),('V44 M-0 rank 1',Image.open(OUT/'07_g0_rank1.png')),('V44 M-0 rank 5',Image.open(OUT/'08_g0_rank5.png'))])
board('encounter_and_equipment',[('Magnetar production encounter',Image.open(OUT/'05_magnetar_encounter.png')),('Custodian / production boss HUD',Image.open(OUT/'10_boss_hud.png')),('Actual-model equipment display',Image.open(OUT/'09_equipment_inventory.png'))])
board('causal_weapon_attack',[('SOURCE on external emitter',Image.open(OUT/'12a_weapon_source.png')),('TRAVEL from real muzzle',Image.open(OUT/'12b_weapon_travel.png')),('IMPACT at destination',Image.open(OUT/'12c_weapon_impact.png'))])
for raw in (OUT/'structure').glob('*.ppm'):
 Image.open(raw).save(raw.with_suffix('.png'))
print('PRODUCTION_REVIEW_BOARDS_PASS')
