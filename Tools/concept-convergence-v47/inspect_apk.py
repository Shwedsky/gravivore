"""Read shipped Unity object types and resolve V47 scene/skin/PBR references."""
import argparse,hashlib,json,struct,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'Tools/render-hotfix'))
from serialized_apk import apk_files,objects,Reader
parser=argparse.ArgumentParser();parser.add_argument('apk',type=Path);parser.add_argument('--output',type=Path,required=True)
args=parser.parse_args();index={};names={};components={};materials={};textures=set()
for entry,data in apk_files(args.apk):
    for obj in objects(data):
        key=(entry.rsplit('/',1)[-1],obj['pathId']);index[key]=(entry,obj)
        if obj['classId']==1:
            r=Reader(obj['data']);components[key]=[r.unpack('<iq') for _ in range(r.unpack('<i'))];r.pos+=4;names[key]=r.string()
        elif obj['classId']==21:
            r=Reader(obj['data']);name=r.string();pointer=r.unpack('<iq');keywords=[r.string() for _ in range(r.unpack('<i'))]
            materials[name]=(entry,obj,pointer,keywords)
        elif obj['classId']==28:textures.add(Reader(obj['data']).string())
def resolve(entry,obj,pointer):
    fileid,pathid=pointer
    if not 0<=fileid<=len(obj['externals']):return None,None
    target=entry.rsplit('/',1)[-1] if fileid==0 else obj['externals'][fileid-1].rsplit('/',1)[-1]
    key=(target,pathid);return key,index.get(key)
def referenced(entry,obj,classid):
    found={}
    for offset in range(12,len(obj['data'])-11,4):
        key,target=resolve(entry,obj,struct.unpack_from('<iq',obj['data'],offset))
        if target and target[1]['classId']==classid:found[key]=target
    return found
layer=[key for key,name in names.items() if key[0]=='level1' and name=='Chapter 01 Concept Convergence V47']
assert len(layer)==1,'V47 layer absent from shipped Chapter01 scene'
origins=[name for key,name in names.items() if key[0]=='level1' and name.startswith('V47 ') and ' / ' in name]
assert len(origins)==9,'Nine actual V47 encounter facility origins required'
static=[];skins=[]
for entry,obj in index.values():
    if obj['classId'] not in (33,137):continue
    r=Reader(obj['data']);goKey,go=resolve(entry,obj,r.unpack('<iq'))
    name=names.get(goKey,'')
    if obj['classId']==33:
        if not name.startswith('V47_'):continue
        meshKey,mesh=resolve(entry,obj,r.unpack('<iq'));assert mesh and mesh[1]['classId']==43,'Missing facility/deck mesh: '+name
        rendererMaterials=set();enabled=False
        for pointer in components[goKey]:
            _,target=resolve(go[0],go[1],pointer)
            if target and target[1]['classId']==23:
                enabled|=bool(target[1]['data'][12])
                rendererMaterials.update(Reader(o['data']).string() for e,o in referenced(*target,21).values())
        assert rendererMaterials.intersection({'V46_HeroRed','V47_WorkingDeck'}),'V47 static geometry lost its PBR material'
        static.append({'object':name,'enabled':enabled,'mesh':Reader(mesh[1]['data']).string(),'meshClassId':43,'meshEntry':mesh[0],'meshPathId':meshKey[1],'materials':sorted(rendererMaterials)})
    elif '_LOD' in name and any(name.startswith(n) for n in ('Scout_V1','Cutter_V1','Warden_V1','ArcDrone_V1','Carrier_V1','Magnetar_V1','Custodian_V3')):
        meshes=referenced(entry,obj,43);mats=referenced(entry,obj,21)
        assert any(Reader(m['data']).string()==name for e,m in meshes.values()),'Skin does not resolve its authored mesh: '+name
        materialNames={Reader(m['data']).string() for e,m in mats.values()}
        assert ('V46_HeroAmber' if name.startswith('Magnetar') else 'V46_HeroRed') in materialNames,'Actor lost rich material: '+name
        skins.append({'renderer':name,'meshClassId':43,'materials':sorted(materialNames)})
for family in ('DeploymentBay','FabricationBay','InductionStation','Deck0','Deck1','Deck2'):
    assert any(s['object'].removesuffix('(Clone)')=='V47_'+family and s['enabled'] for s in static),'Missing enabled shipped V47 family: '+family
assert any(s['mesh'].startswith('Combined Mesh') for s in static),'Static-batched V47 geometry absent'
for actor in ('Scout_V1','Cutter_V1','Warden_V1','ArcDrone_V1','Carrier_V1','Magnetar_V1','Custodian_V3'):
    for lod in range(3):assert any(s['renderer']==actor+'_LOD'+str(lod) for s in skins),'Missing shipped actor LOD: '+actor
pbr=[]
for name in ('V46_HeroAmber','V46_HeroRed','V47_WorkingDeck'):
    entry,obj,pointer,keywords=materials[name];_,shader=resolve(entry,obj,pointer)
    value='Universal Render Pipeline/Lit'
    assert shader and shader[1]['classId']==48 and struct.pack('<i',len(value))+value.encode() in shader[1]['data']
    assert {'_NORMALMAP','_METALLICSPECGLOSSMAP','_OCCLUSIONMAP','_EMISSION'}.issubset(keywords)
    mapped={Reader(t['data']).string() for e,t in referenced(entry,obj,28).values()}
    assert {'V46_BaseColor','V46_Normal','V46_MetallicSmoothness','V46_Occlusion'}.issubset(mapped)
    assert any(t.startswith('V46_Emission') for t in mapped)
    pbr.append({'material':name,'shader':value,'keywords':keywords,'resolvedTextures':sorted(mapped)})
evidence={'validated':True,'apkSha256':hashlib.sha256(args.apk.read_bytes()).hexdigest(),'productionScene':'level1','facilityOrigins':origins,'resolvedStaticGeometry':static,'resolvedActorSkins':skins,'pbr':pbr,'physicalAndroidExecution':False}
args.output.write_text(json.dumps(evidence,indent=2));print('V47_TYPED_SCENE_SKIN_PBR_PASS')
