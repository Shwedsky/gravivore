"""Typed V44/V45 asset proof, plus the preserved V43 rendering inspector."""
import argparse,hashlib,json,struct,subprocess,sys,zipfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'Tools/render-hotfix'))
from serialized_apk import apk_files,objects,Reader
p=argparse.ArgumentParser();p.add_argument('apk',type=Path);p.add_argument('--output',type=Path,required=True);p.add_argument('--corrective',action='store_true');args=p.parse_args()
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
   if name.startswith('VR3_') or args.corrective and name.startswith('V45_'):
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
 assert any(n in ('VR3_WornIndustrialAtlas','VR3_IndustrialTrimAtlas') or args.corrective and n.startswith('V45_') for n in materialNames), 'Decorative renderer must resolve a verified production material'
 baked.append({'gameObject':gameNames[goKey],'mesh':Reader(mesh[1]['data']).string(),'meshClassId':43,'meshPathId':meshKey[1],'meshEntry':mesh[0],'materials':sorted(set(materialNames))})
assert len(baked)>100 and any(b['mesh'].startswith('Combined Mesh') for b in baked), 'Production static-batch geometry not verified'
budget=json.loads((ROOT/('docs/history/implementation-passes/chapter01-visual-replacement-v3/v45/verification/mobile_render_budget.json' if args.corrective else 'docs/history/implementation-passes/chapter01-visual-replacement-v3/v44/verification/mobile_render_budget.json')).read_text(encoding='utf-8-sig'))
if args.corrective:
 assert len(baked)==budget['allVR3Renderers'], 'Every enabled or retired production MeshFilter must resolve, including the corrective module instances'
else:
 assert sum(b['mesh'].startswith('Combined Mesh') for b in baked)==budget['decorativeRenderers'], 'Packed decorative renderer count differs from the validated scene'
assert any(key[0]=='level1' and name=='Chapter 01 Visual Replacement V3' for key,name in gameNames.items()), 'Packed Chapter01 scene root missing'
assert all(b['meshEntry'].endswith('/level1') for b in baked if b['mesh'].startswith('Combined Mesh')), 'Decorative batches must belong to the actual Chapter01 scene'
with zipfile.ZipFile(args.apk) as z:
 metadata=z.read('assets/bin/Data/Managed/Metadata/global-metadata.dat')
 assert all(s.encode() in metadata for s in ('ProductionUiSkinDefinition','ProductionUiSkinScope','VisualRank','WeaponThumbnail','WeaponEquipmentPresenter'))
 notices=z.read('assets/ThirdPartyNotices.txt').decode('utf-8');assert 'Catherine Laserna' in notices and 'CC BY 4.0' in notices
corrective=[];correctiveBatches=[];correctiveSettings={}
if args.corrective:
 assert any(key[0]=='level1' and name=='Chapter 01 Active Industrial Facility V45' for key,name in gameNames.items()), 'V45 production scene root missing'
 assert sum(name.startswith('Serviced enemy dock ') or name.startswith('Heavy fabrication nest ') for key,name in gameNames.items() if key[0]=='level1')==9, 'Nine packed spawn origin structures required'
 assert all(s.encode() in metadata for s in ('AmbientPatrolState','EnemyAmbientMotionSettings','ConfigureAmbientMotion')), 'Compiled bounded idle movement missing'
 for name,(entry,obj,(fileid,pathid),keywords) in materials.items():
  if not name.startswith('V45_'):continue
  key,target=resolve(entry,obj,(fileid,pathid));assert target and target[1]['classId']==48
  expected='Universal Render Pipeline/Lit';assert struct.pack('<i',len(expected))+expected.encode() in target[1]['data']
  corrective.append({'material':name,'shader':expected,'shaderEntry':target[0],'keywords':keywords})
 assert len(corrective)==8, 'V45 shared palette incomplete'
 for entry,obj in list(index.values()):
  if obj['classId']==33:
   r=Reader(obj['data']);goKey,go=resolve(entry,obj,r.unpack('<iq'));meshKey,mesh=resolve(entry,obj,r.unpack('<iq'))
   if not gameNames.get(goKey,'').startswith('Baked service detail '):continue
   assert mesh and mesh[1]['classId']==43, 'V45 sector detail must resolve to an actual class-43 mesh'
   correctiveBatches.append({'gameObject':gameNames[goKey],'mesh':Reader(mesh[1]['data']).string(),'entry':mesh[0],'meshPathId':meshKey[1]})
  if obj['classId']!=114:continue
  r=Reader(obj['data'],28)
  try:name=r.string()
  except (ValueError,UnicodeError,struct.error):continue
  if name=='EnemyAmbientMotion':
   values=r.unpack('<6f');expected=(.9,.5,1.4,3.6,2.4,95)
   assert all(abs(a-b)<.0001 for a,b in zip(values,expected)), 'Packed idle settings differ from the tested source'
   correctiveSettings['ambientMotion']={'entry':entry,'radius':values[0],'speed':values[1],'minimumPause':values[2],'maximumPause':values[3],'travelSeconds':values[4],'turnSpeed':values[5]}
  if name=='S08_Chapter01World':
   worldid=r.string();assert worldid=='chapter01-scrap-exclusion'
   basin=r.unpack('<3f');center=r.unpack('<3f');size=r.unpack('<2f');r.pos+=8
   zones=[]
   for _ in range(r.unpack('<i')):
    zone=r.string();position=r.unpack('<3f');r.pos+=12+16;zones.append({'id':zone,'position':position})
   gates=[]
   for _ in range(2):
    gate=r.string();position=r.unpack('<3f');r.pos+=12;gates.append({'id':gate,'position':position})
   arena=r.unpack('<3f');radius=r.unpack('<f')
   assert size==(56.,118.) and gates[0]['position'][2]==38 and gates[1]['position'][2]==58 and arena==(0.,0.,72.) and radius==5
   correctiveSettings['world']={'entry':entry,'worldId':worldid,'groundCenter':center,'groundSize':size,'repairOrigin':basin,'zones':zones,'gates':gates,'bossArenaCenter':arena,'bossArenaRadius':radius}
 assert len(correctiveBatches)>30 and set(correctiveSettings)=={'ambientMotion','world'}, 'Packed corrective geometry/layout/motion settings are incomplete'
evidence={'validated':True,'apkSha256':hashlib.sha256(args.apk.read_bytes()).hexdigest(),'productionSceneLevel':'assets/bin/Data/level1','productionSceneRootVerified':True,'resolvedProductionMaterials':proof,'weaponAndEnvironmentObjects':models,'actualModelThumbnails':thumbnails,'injectedSkin':skin,'resolvedStaticGeometry':baked,'compiledRankAndUiTypes':True,'exeAttributionPacked':True,'executedOnAndroid':False}
if args.corrective:
 evidence.update(correctivePalette=corrective,correctiveSceneAndNineOriginsVerified=True,boundedIdleTypesPacked=True,correctiveStaticBatches=correctiveBatches,correctiveSerializedSettings=correctiveSettings)
args.output.write_text(json.dumps(evidence,indent=2));print('V45_TYPED_CORRECTIVE_PRODUCTION_CONTENT_PASS' if args.corrective else 'V44_TYPED_PRODUCTION_CONTENT_PASS')
