"""Typed V44 asset proof, plus the preserved V43 rendering inspector."""
import argparse,hashlib,json,struct,subprocess,sys,zipfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'Tools/render-hotfix'))
from serialized_apk import apk_files,objects,Reader
p=argparse.ArgumentParser();p.add_argument('apk',type=Path);p.add_argument('--output',type=Path,required=True);args=p.parse_args()
subprocess.run([sys.executable,str(ROOT/'Tools/render-hotfix/inspect_rendering.py'),str(args.apk),'--output',str(args.output.parent/'apk_rendering.json')],check=True,stdout=subprocess.DEVNULL)
materials={};index={};models=[];thumbnails=[];skin=[];gameNames={};components={}
for entry,data in apk_files(args.apk):
 obs=objects(data);index.update({(entry.rsplit('/',1)[-1],o['pathId']):(entry,o) for o in obs})
 for o in obs:
  if o['classId']==1:
   r=Reader(o['data']);n=r.unpack('<i');links=[r.unpack('<iq') for _ in range(n)];r.pos+=4;name=r.string()
   key=(entry.rsplit('/',1)[-1],o['pathId']);gameNames[key]=name;components[key]=links
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
def resolve(entry,obj,pointer):
 fileid,pathid=pointer
 targetfile=entry.rsplit('/',1)[-1] if fileid==0 else obj['externals'][fileid-1].rsplit('/',1)[-1]
 return (targetfile,pathid),index.get((targetfile,pathid))
baked=[]
for entry,obj in list(index.values()):
 if obj['classId']!=33:continue
 r=Reader(obj['data']);goPointer=r.unpack('<iq');meshPointer=r.unpack('<iq')
 goKey,go=resolve(entry,obj,goPointer)
 if not gameNames.get(goKey,'').startswith('VR3_'):continue
 meshKey,mesh=resolve(entry,obj,meshPointer)
 assert mesh and mesh[1]['classId']==43, 'Decorative MeshFilter must resolve to an actual Mesh'
 materialNames=[]
 for link in components[goKey]:
  _,component=resolve(go[0],go[1],link)
  if not component or component[1]['classId']!=23:continue
  rendererEntry,renderer=component
  for offset in range(12,len(renderer['data'])-11,4):
   pointer=struct.unpack_from('<iq',renderer['data'],offset)
   if not 0<=pointer[0]<=len(renderer['externals']):continue
   _,target=resolve(rendererEntry,renderer,pointer)
   if target and target[1]['classId']==21:materialNames.append(Reader(target[1]['data']).string())
 assert any(n in ('VR3_WornIndustrialAtlas','VR3_IndustrialTrimAtlas') for n in materialNames), 'Decorative renderer must resolve the production atlas'
 baked.append({'gameObject':gameNames[goKey],'mesh':Reader(mesh[1]['data']).string(),'meshClassId':43,'meshPathId':meshKey[1],'meshEntry':mesh[0],'materials':sorted(set(materialNames))})
assert len(baked)>100 and any(b['mesh'].startswith('Combined Mesh') for b in baked), 'Production static-batch geometry not verified'
budget=json.loads((ROOT/'docs/visual-replacement-v3/verification/mobile_render_budget.json').read_text(encoding='utf-8-sig'))
assert sum(b['mesh'].startswith('Combined Mesh') for b in baked)==budget['decorativeRenderers'], 'Packed decorative renderer count differs from the validated scene'
with zipfile.ZipFile(args.apk) as z:
 metadata=z.read('assets/bin/Data/Managed/Metadata/global-metadata.dat')
 assert all(s.encode() in metadata for s in ('ProductionUiSkinDefinition','ProductionUiSkinScope','VisualRank','WeaponThumbnail','WeaponEquipmentPresenter'))
 notices=z.read('assets/ThirdPartyNotices.txt').decode('utf-8');assert 'Catherine Laserna' in notices and 'CC BY 4.0' in notices
evidence={'validated':True,'apkSha256':hashlib.sha256(args.apk.read_bytes()).hexdigest(),'resolvedProductionMaterials':proof,'weaponAndEnvironmentObjects':models,'actualModelThumbnails':thumbnails,'injectedSkin':skin,'resolvedStaticGeometry':baked,'compiledRankAndUiTypes':True,'exeAttributionPacked':True,'executedOnAndroid':False}
args.output.write_text(json.dumps(evidence,indent=2));print('V44_TYPED_PRODUCTION_CONTENT_PASS')
