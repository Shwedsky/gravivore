"""Check actual PNGs, Unity board composition, source preservation and verification outputs."""
from pathlib import Path
import json, hashlib, xml.etree.ElementTree as ET, subprocess
from PIL import Image, ImageChops
ROOT=Path(__file__).resolve().parents[2]
DOC=ROOT/'docs/visual-production-v2/g0-bipedal-v2'
EVIDENCE=DOC/'evidence-v21'; rows=[]
for p in sorted(EVIDENCE.rglob('*.png')):
    with Image.open(p) as im:
        im.load(); assert len(im.getcolors(256) or [])!=1, p
        rows.append({'path':str(p),'size':list(im.size),'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'bytes':p.stat().st_size})
required=['01_G0_scale_090_gameplay.png','02_G0_scale_100_gameplay.png','03_G0_scale_110_gameplay.png',
 '04_G0_scale_comparison_board.png','05_G0_vs_ordinary_enemy.png','06_G0_vs_Magnetar.png','07_G0_gate_clearance.png','08_G0_phone_size_readability.png']
assert all((EVIDENCE/'unity'/n).exists() for n in required)
for board,names in [('04_G0_scale_comparison_board.png',required[:3]),('08_G0_phone_size_readability.png',['phone_090.png','phone_100.png','phone_110.png'])]:
    with Image.open(EVIDENCE/'unity'/board) as full:
        for i,n in enumerate(names):
            with Image.open(EVIDENCE/'unity'/n) as part:
                crop=full.crop((i*part.width,0,(i+1)*part.width,part.height))
                assert ImageChops.difference(crop,part).getbbox() is None,(board,n)
metrics=json.loads((DOC/'data-v21/proportion_metrics.json').read_text())
assert hashlib.sha256((ROOT/'art/visual-production-v2/g0/G0_Bipedal_Blockout_V2.blend').read_bytes()).hexdigest()==metrics['source_sha256']
tests={}
for kind in ['EditMode','PlayMode']:
    p=DOC/'verification-v21'/f'{kind}.xml'
    if p.exists():
        tree=ET.parse(p).getroot();tests[kind]={k:tree.attrib.get(k) for k in ['result','total','passed','failed','skipped']}
validation=DOC/'verification-v21/Validate.log'
build=ROOT/'Builds/Android/gravivore-dev-0.1.0+2.apk'
summary={'blender_reopen':json.loads((DOC/'data-v21/blend_reopen_validation.json').read_text()),'original_v2_preserved':True,
 'all_requested_captures_present':True,'boards_are_exact_unity_pixels':True,'pngs':rows,'tests':tests,
 'project_validation':'PASS' if validation.exists() and 'Application will terminate with return code 0' in validation.read_text(errors='replace') else 'not yet verified',
 'apk':str(build) if build.exists() else None,
 'device_performance':'Not measured','review_only':True}
(DOC/'data-v21/evidence_manifest.json').write_text(json.dumps(rows,indent=2),encoding='utf-8')
(DOC/'data-v21/verification_summary.json').write_text(json.dumps(summary,indent=2),encoding='utf-8')
print(json.dumps({k:v for k,v in summary.items() if k not in ['pngs','blender_reopen']},indent=2))
