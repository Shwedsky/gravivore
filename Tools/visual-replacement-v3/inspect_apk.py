"""Typed V44 asset proof, plus the preserved V43 rendering inspector."""
import argparse,hashlib,json,struct,subprocess,sys,zipfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'Tools/render-hotfix'))
from serialized_apk import apk_files,objects,Reader
p=argparse.ArgumentParser();p.add_argument('apk',type=Path);p.add_argument('--output',type=Path,required=True);args=p.parse_args()
subprocess.run([sys.executable,str(ROOT/'Tools/render-hotfix/inspect_rendering.py'),str(args.apk),'--output',str(args.output.parent/'apk_rendering.json')],check=True,stdout=subprocess.DEVNULL)
materials={};index={};models=[];thumbnails=[];skin=[]
for entry,data in apk_files(args.apk):
 obs=objects(data);index.update({(entry.rsplit('/',1)[-1],o['pathId']):(entry,o) for o in obs})
 for o in obs:
  if o['classId']==1:
   r=Reader(o['data']);n=r.unpack('<i');r.pos+=12*n+4;name=r.string()
   if name.startswith('M0_Rank') or name.startswith('VR3_'):models.append({'name':name,'entry':entry,'classId':1})
  if o['classId']==21:
   r=Reader(o['data']);name=r.string()
   if name.startswith('VR3_'):
    pointer=r.unpack('<iq');keywords=[r.string() for _ in range(r.unpack('<i'))];materials[name]=(entry,o,pointer,keywords)
  if o['classId'] in (28,213):
   try:name=Reader(o['data']).string()
   except (ValueError,UnicodeError,struct.error):continue
   if name.startswith('M0_Thumbnail'):thumbnails.append({'name':name,'classId':o['classId'],'entry':entry})
  if o['classId']==114:
   try:name=Reader(o['data'],28).string()
   except (ValueError,UnicodeError,struct.error):continue
   if name=='ProductionUiSkin':skin.append({'name':name,'classId':114,'entry':entry})
proof=[]
for name in ('VR3_WornIndustrialAtlas','VR3_IndustrialTrimAtlas'):
 entry,o,(fileid,pathid),keywords=materials[name]
 targetfile=entry.rsplit('/',1)[-1] if fileid==0 else o['externals'][fileid-1].rsplit('/',1)[-1]
 shaderentry,shader=index[(targetfile,pathid)]
 assert shader['classId']==48
 value='Universal Render Pipeline/Lit';assert struct.pack('<i',len(value))+value.encode() in shader['data']
 assert '_METALLICSPECGLOSSMAP' in keywords
 assert ('_EMISSION' if 'Worn' in name else '_NORMALMAP') in keywords
 proof.append({'name':name,'shader':value,'shaderPathId':pathid,'shaderEntry':shaderentry,'keywords':keywords})
assert skin and all(any(m['name'].startswith('M0_Rank'+str(r)) for m in models) for r in range(1,6))
assert all(any(t['name']=='M0_Thumbnail'+str(r) for t in thumbnails) for r in range(1,6))
assert len({m['name'] for m in models if m['name'].startswith('VR3_')})>10
with zipfile.ZipFile(args.apk) as z:
 metadata=z.read('assets/bin/Data/Managed/Metadata/global-metadata.dat')
 assert all(s.encode() in metadata for s in ('ProductionUiSkinDefinition','ProductionUiSkinScope','VisualRank','WeaponThumbnail','WeaponEquipmentPresenter'))
 notices=z.read('assets/ThirdPartyNotices.txt').decode('utf-8');assert 'Catherine Laserna' in notices and 'CC BY 4.0' in notices
evidence={'validated':True,'apkSha256':hashlib.sha256(args.apk.read_bytes()).hexdigest(),'resolvedProductionMaterials':proof,'weaponAndEnvironmentObjects':models,'actualModelThumbnails':thumbnails,'injectedSkin':skin,'compiledRankAndUiTypes':True,'exeAttributionPacked':True,'executedOnAndroid':False}
args.output.write_text(json.dumps(evidence,indent=2));print('V44_TYPED_PRODUCTION_CONTENT_PASS')
