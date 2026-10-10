"""Allowlisted production ingestion; verifies V2 rights and exact local evidence.

No source code, shaders, stock scenes, fonts or Asset Store files are copied.
Adapted model exports are produced by build_assets.py; local originals stay ignored.
"""
import csv, hashlib, json, re, shutil, sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'Tools/free-asset-intake'))
from prepare_zoo import identify, models
WORK=ROOT/'ExternalAssetIntake/FreeAssetIntakeV1/_work_v2'
OUT=ROOT/'Assets/_Game/Content/VisualReplacementV3'
DOCS=ROOT/'docs/history/implementation-passes/chapter01-visual-replacement-v3/v44'
PICKS={
 'env-quaternius-megakit':['Platform_Metal','Platform_DarkPlates','Platform_Rails_2','Platform_Ramp_2','Platform_Stairs_2','ShortWall_MetalPlates_Straight','Door_Frame_Square','Door_DarkMetal','TopCables_Straight','TopCables_Corner','Column_Pipes','Prop_PipeHolder','Prop_Vent_Big'],
 'env-molten-maps':['Generator','Generator Pile Large','Cryo Tube','Centrifuge','Command','Floor Metal Square Grate'],
 'weapons-quaternius':['Barrel_AR_1','Barrel_Sniper_1','Body_AR_1','Scope_1'],
 'env-quaternius-essentials':['Enemy_QuadShell','Enemy_Trilobite','Enemy_EyeDrone','Prop_Crate'],
}
def digest(path):
 with path.open('rb') as stream:return hashlib.file_digest(stream,'sha256').hexdigest()
def norm(s):return re.sub('[^a-z0-9]','',s.lower())
def run():
 DOCS.mkdir(parents=True,exist_ok=True)
 (OUT/'Licenses').mkdir(parents=True,exist_ok=True)
 rights={r['sourceId']:r for r in csv.DictReader((ROOT/'docs/free-asset-intake-v2/ASSET_PROVENANCE_RESOLVED.csv').open(encoding='utf-8-sig'))}
 inspections=json.loads((WORK/'reports/inspection.json').read_text(encoding='utf-8'))
 manifest=[]
 for item in inspections:
  sid=identify(item)
  if sid not in PICKS and sid not in ('ui-exe','ui-kenney'):continue
  r=rights[sid]
  assert r['rawRedistributionAllowed']=='True' and r['productionUseAllowed']=='True' and r['license'] in ('CC0','CC_BY_4_0'),sid
  base=ROOT/item['contentRoot']
  proof=next((p for p in base.rglob('*') if p.is_file() and ('license' in p.name.lower() and p.suffix=='.txt' or sid=='ui-exe' and p.name=='crediting_guide.pdf')),None)
  assert proof is not None,sid
  if sid!='ui-exe':
   assert 'CC0' in proof.read_text(encoding='utf-8-sig'),sid
   expected=re.search(r'SHA256 ([a-f0-9]{64})',r['licenseEvidence']).group(1)
   assert digest(proof)==expected,(sid,'license changed')
  shutil.copy2(proof,OUT/'Licenses'/(sid+proof.suffix))
  if sid in PICKS:
   candidates=models(item)
   for wanted in PICKS[sid]:
    exact=[p for p in candidates if norm(p.stem)==norm(wanted)]
    variants=[p for p in candidates if norm(p.stem).startswith(norm(wanted))]
    chosen=next(iter(exact or sorted(variants,key=lambda p:('orange' not in p.stem.lower(),len(p.stem)))),None)
    assert chosen is not None,(sid,wanted)
    manifest.append(dict(sourceId=sid,sourcePath=chosen.relative_to(ROOT).as_posix(),sourceSha256=digest(chosen),license=r['license'],sourceUrl=r['sourceUrl'],outputName='VR3_'+norm(wanted),role=wanted))
  else:
   pngs=sorted(p for p in base.rglob('*.png') if 'Normal Assets' in str(p) or sid=='ui-kenney')
   for p in pngs:
    if sid=='ui-exe' and p.parent.name in ('inventory','buttons','dividers'):
     manifest.append(dict(sourceId=sid,sourcePath=p.relative_to(ROOT).as_posix(),sourceSha256=digest(p),license=r['license'],sourceUrl=r['sourceUrl'],outputName='UI_'+norm(p.parent.name+'_'+p.stem),role=p.stem))
   if sid=='ui-kenney':
    p=next(p for p in pngs if 'bar' in p.stem.lower())
    manifest.append(dict(sourceId=sid,sourcePath=p.relative_to(ROOT).as_posix(),sourceSha256=digest(p),license=r['license'],sourceUrl=r['sourceUrl'],outputName='UtilityBar',role=p.stem))
 (DOCS/'ingestion_manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
 print('Verified production donors:',len(manifest))
 for entry in manifest:print(entry['outputName'],entry['sourcePath'])
if __name__=='__main__':run()
