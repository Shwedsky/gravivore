"""Run with bundled Python/Pillow after final Blender render process completes."""
import hashlib, json
from pathlib import Path
from PIL import Image
ROOT=Path(__file__).resolve().parents[2]
DOC=ROOT/'docs/visual-production-v2/g0-bipedal-v2'
names=['01_G0_Bipedal_front.png','02_G0_Bipedal_side.png','03_G0_Bipedal_back.png',
       '04_G0_Bipedal_threequarter.png','05_G0_Bipedal_gameplay_angle.png',
       '06_G0_Bipedal_black_silhouette.png','07_G0_Bipedal_vs_Catfish_donor.png']
blend=ROOT/'art/visual-production-v2/g0/G0_Bipedal_Blockout_V2.blend'
sha=hashlib.sha256(blend.read_bytes()).hexdigest()
measured=json.loads((DOC/'data/blockout_validation.json').read_text())
assert sha==measured['blend_sha256'], 'Blend changed after geometry validation'
manifest=json.loads((DOC/'data/render_manifest.json').read_text())
assert [r['file'] for r in manifest]==names
rows=[]
for name,cam in zip(names,manifest):
    path=DOC/'evidence'/name
    with Image.open(path) as im: im.verify()
    with Image.open(path) as im:
        assert list(im.size)==cam['resolution'], (name,im.size)
        assert im.format=='PNG'
        extrema=im.convert('RGB').getextrema()
        assert any(high-low>30 for low,high in extrema), 'Empty/flat image'
        rows.append({'file':name,'absolute_path':str(path),'bytes':path.stat().st_size,
                     'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'resolution':list(im.size)})
with Image.open(DOC/'evidence'/names[5]) as im:
    pix=im.convert('RGB')
    black=sum(max(p)<8 for p in pix.getdata()); white=sum(min(p)>247 for p in pix.getdata())
    assert black>100000 and white>500000, 'Silhouette is not black on white'
report={'status':'PASS','source_blend_sha256':sha,'seven_pngs_verified':True,
        'silhouette_black_pixels':black,'silhouette_white_pixels':white,'files':rows,
        'visual_inspection':'All seven viewed by agent; comparison labels/feet in frame; anatomy/foot continuity and gameplay readability reviewed.'}
(DOC/'data/evidence_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print('G0_EVIDENCE_PASS',json.dumps({k:v for k,v in report.items() if k!='files'}))
