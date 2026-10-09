"""Typed proof that the shipped V46 meshes, materials and map names exist."""
import argparse,hashlib,json,struct,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'Tools/render-hotfix'))
from serialized_apk import apk_files,objects,Reader
p=argparse.ArgumentParser();p.add_argument('apk',type=Path);p.add_argument('--output',type=Path,required=True);args=p.parse_args()
index={};names={};materials={};textures=[];filters=[]
for entry,data in apk_files(args.apk):
    for o in objects(data):
        key=(entry.rsplit('/',1)[-1],o['pathId']);index[key]=(entry,o)
        if o['classId']==1:
            r=Reader(o['data']);n=r.unpack('<i');r.pos+=12*n+4;names[key]=r.string()
        elif o['classId']==21:
            r=Reader(o['data']);name=r.string()
            if name.startswith('V46_'):materials[name]=(entry,o,r.unpack('<iq'),[r.string() for _ in range(r.unpack('<i'))])
        elif o['classId']==28:
            name=Reader(o['data']).string()
            if name.startswith('V46_'):textures.append(name)
        elif o['classId']==33:
            r=Reader(o['data']);filters.append((entry,o,r.unpack('<iq'),r.unpack('<iq')))
def resolve(entry,obj,pointer):
    fileid,pathid=pointer;target=entry.rsplit('/',1)[-1] if fileid==0 else obj['externals'][fileid-1].rsplit('/',1)[-1]
    return (target,pathid),index.get((target,pathid))
heroes=[]
for entry,o,go,mesh in filters:
    gokey,_=resolve(entry,o,go)
    if not names.get(gokey,'').startswith('V46_'):continue
    _,resolved=resolve(entry,o,mesh);assert resolved and resolved[1]['classId']==43,'Missing real V46 mesh'
    heroes.append({'object':names[gokey],'meshEntry':resolved[0],'meshPathId':resolved[1]['pathId']})
for family in ('PressureVessel','EnergyCylinder','ArcMachine','GateModule','LightDock','HeavyDock','SpecialDock','DockApron','ServiceGantry'):
    assert any('V46_'+family in h['object'] for h in heroes),family+' missing from actual scene mesh references'
shaders=[]
for name,(entry,o,pointer,keywords) in materials.items():
    _,target=resolve(entry,o,pointer);assert target and target[1]['classId']==48
    shader='Universal Render Pipeline/Lit';assert struct.pack('<i',len(shader))+shader.encode() in target[1]['data']
    assert {'_NORMALMAP','_OCCLUSIONMAP','_METALLICSPECGLOSSMAP','_EMISSION'}.issubset(keywords)
    shaders.append({'material':name,'shader':shader,'keywords':keywords})
assert {'V46_HeroAmber','V46_HeroCyan','V46_HeroRed'}.issubset(materials)
for prefix in ('','Legacy_','Support_'):
    for label in ('BaseColor','Normal','MetallicSmoothness','Occlusion','Emission'):
        # Zero-valued support emission can legitimately be pruned by Unity.
        if prefix=='Support_' and label=='Emission':continue
        assert 'V46_'+prefix+label in textures,'Missing PBR texture '+prefix+label
result={'validated':True,'apkSha256':hashlib.sha256(args.apk.read_bytes()).hexdigest(),'actualV46MeshFilters':heroes,'resolvedHeroMaterials':shaders,'packedPbrTextures':sorted(set(textures)),'runtimeMeshConstructionRequired':False}
args.output.write_text(json.dumps(result,indent=2));print('V46_TYPED_PACKED_HERO_PBR_PASS')
