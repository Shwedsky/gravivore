"""Snapshot fresh regression receipts into V47 without changing old authority."""
import json,shutil,subprocess,xml.etree.ElementTree as ET
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'docs/history/implementation-passes/chapter01-visual-replacement-v3/v47'
history=ROOT/'docs/history';copied=[]
receipts=[
 'visual-stages/chapter01-visual-passes/concept-fidelity-v2/verification/five_minute_runtime.json',
 'visual-stages/art-spike/evidence/RUNTIME_PERFORMANCE.json',
 'visual-stages/chapter01-visual-passes/chapter01-production/verification/reachability.txt',
 'implementation-passes/chapter01-visual-replacement-v3/v45/verification/capsule_route_readability.json',
 'implementation-passes/chapter01-visual-replacement-v3/v45/verification/idle_life.json',
 'implementation-passes/chapter01-visual-replacement-v3/v46/verification/material_quality_audit.json',
 'implementation-passes/chapter01-visual-replacement-v3/v45/verification/mobile_render_budget.json',
 'visual-stages/chapter01-visual-passes/render-hotfix/verification/render_profile.json',
 'visual-stages/chapter01-visual-passes/render-hotfix/verification/compiled_shader_variants.txt']
for path in receipts:
    source=history/path
    if source.exists():
        target=OUT/'verification/regression_receipts'/Path(path).name;target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(source,target);copied.append(path)
captures={
 'weapon_source.png':'implementation-passes/chapter01-visual-replacement-v3/v44/internal/12a_weapon_source.png',
 'weapon_travel.png':'implementation-passes/chapter01-visual-replacement-v3/v44/internal/12b_weapon_travel.png',
 'weapon_impact.png':'implementation-passes/chapter01-visual-replacement-v3/v44/internal/12c_weapon_impact.png',
 'equipment_inventory.png':'implementation-passes/chapter01-visual-replacement-v3/v44/internal/09_equipment_inventory.png',
 'g0_rank1.png':'implementation-passes/chapter01-visual-replacement-v3/v44/internal/07_g0_rank1.png',
 'g0_rank5.png':'implementation-passes/chapter01-visual-replacement-v3/v44/internal/08_g0_rank5.png'}
for name,path in captures.items():
    source=history/path
    if source.exists():
        target=OUT/'internal/regression'/name;target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(source,target);copied.append(path)
summary={}
for name in ('EditModeFull','PlayModeFull','EditModeFinal','PlayModeFinal','CameraInventoryFinal','V47ReviewedCamera','ActorBindings'):
    path=OUT/'verification'/(name+'.xml')
    if not path.exists():continue
    run=ET.parse(path).getroot();summary[name]={k:run.attrib.get(k) for k in ('result','total','passed','failed','skipped','duration')}
    summary[name]['failures']=[{'name':t.attrib['fullname'],'message':t.findtext('failure/message')} for t in run.iter('test-case') if t.attrib.get('result')=='Failed']
    summary[name]['skips']=[{'name':t.attrib['fullname'],'reason':t.findtext('reason/message')} for t in run.iter('test-case') if t.attrib.get('result')=='Skipped']
if (OUT/'verification/CameraInventoryFinal.xml').exists():
    cases={t.attrib['fullname']:t.attrib['result'] for t in ET.parse(OUT/'verification/PlayModeFinal.xml').getroot().iter('test-case')}
    for t in ET.parse(OUT/'verification/CameraInventoryFinal.xml').getroot().iter('test-case'):
        assert t.attrib['fullname'] in cases;cases[t.attrib['fullname']]=t.attrib['result']
    summary['PlayModeResolved']={'method':'Full suite plus affected diagnostic-test rerun; original XML is preserved','fullRun':'PlayModeFinal.xml','affectedRerun':'CameraInventoryFinal.xml','total':len(cases),'passed':sum(s=='Passed' for s in cases.values()),'failed':sum(s=='Failed' for s in cases.values()),'skipped':sum(s=='Skipped' for s in cases.values()),'failures':[n for n,s in cases.items() if s=='Failed']}
(OUT/'verification/test_summary.json').write_text(json.dumps(summary,indent=2))
(OUT/'verification/regression_receipt_sources.json').write_text(json.dumps({'freshCopiesFromExistingRegressionHarnesses':copied,'historyIsNonAuthoritative':True},indent=2))
font=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',24)
def compare(name,capture):
    board=Image.new('RGB',(1080,1016),(10,17,21));draw=ImageDraw.Draw(board)
    for i,(phase,label) in enumerate([('before','V46 baseline: actual portrait camera'),('after','V47: actual portrait camera')]):
        draw.text((i*540+12,14),label,fill=(190,215,223),font=font)
        board.paste(Image.open(OUT/'internal'/phase/capture).convert('RGB').resize((540,960)),(i*540,56))
    board.save(OUT/'internal'/(name+'.jpg'),quality=93)
for name,capture in [('compare_relay','sector_relay-yard.png'),('compare_cutting','sector_cutting-floor.png'),('compare_boss','06_custodian_arena.png'),('compare_hub','01_repair_hub.png')]:compare(name,capture)
print(json.dumps({'tests':summary,'regressionReceiptsCopied':len(copied)}))
