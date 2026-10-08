"""Typed Android APK rendering proof; zero project/package dependencies."""
import argparse,hashlib,json,struct,zipfile
from pathlib import Path
from serialized_apk import apk_files,objects,Reader,shader_assets

parser=argparse.ArgumentParser();parser.add_argument('apk',type=Path);parser.add_argument('--output',type=Path,required=True)
args=parser.parse_args()
expected={'Fidelity_IndustrialAtlas':('Universal Render Pipeline/Lit',{'_EMISSION','_METALLICSPECGLOSSMAP'}),
          'M_Phase6B_HostileSoft':('Universal Render Pipeline/Particles/Unlit',{'_SURFACE_TYPE_TRANSPARENT'})}
ui=list(shader_assets(args.apk,['UI/Default','UI/DefaultETC1']))
if {s['name'] for s in ui}!={'UI/Default','UI/DefaultETC1'}:raise RuntimeError('MAGENTA_GUARD missing actual packed runtime uGUI Shader object')
v3_names={'Custodian_V3','Emitter_M0','Broken_Edge_V3_0','Broken_Edge_V3_1','Service_Trench_V3','Collapsed_Hull_V3','HostileChargeV3','HostileTravelV3'}
index={};named_materials={};pipelines=[];v3_objects=[]
for entry,data in apk_files(args.apk):
    file_objects=objects(data)
    index.update({(entry.rsplit('/',1)[-1],o['pathId']):(entry,o) for o in file_objects})
    for obj in file_objects:
        if obj['classId']==1:
            r=Reader(obj['data']);count=r.unpack('<i');r.pos+=12*count+4;name=r.string()
            if name in v3_names:v3_objects.append({'name':name,'entry':entry,'pathId':obj['pathId']})
        if obj['classId']==21:
            r=Reader(obj['data']);name=r.string()
            if name in expected:
                pointer=r.unpack('<iq');keywords=[r.string() for _ in range(r.unpack('<i'))]
                named_materials[name]=(entry,obj,pointer,keywords)
        if obj['classId']==114:
            r=Reader(obj['data'],28)
            try:name=r.string()
            except (ValueError,UnicodeError,struct.error):continue
            if name in ('Gravivore_URP','Gravivore_URP_Renderer'):pipelines.append({'entry':entry,'name':name,'classId':114,'pathId':obj['pathId'],'bytes':len(obj['data'])})
if {p['name'] for p in pipelines}!={'Gravivore_URP','Gravivore_URP_Renderer'}:raise RuntimeError('Packed URP pipeline/renderer object missing')
if {p['name'] for p in v3_objects}!=v3_names:raise RuntimeError('Packed V3 GameObject missing')
with zipfile.ZipFile(args.apk) as archive:
    metadata=archive.read('assets/bin/Data/Managed/Metadata/global-metadata.dat')
    if b'RenderingBootDiagnostics' not in metadata:raise RuntimeError('Compiled v43 DEV shader diagnostics missing')
proof=[]
for name,(expected_shader,expected_keywords) in expected.items():
    if name not in named_materials:raise RuntimeError('Packed material missing: '+name)
    entry,obj,(file_id,path_id),keywords=named_materials[name]
    target_file=entry.rsplit('/',1)[-1] if file_id==0 else obj['externals'][file_id-1].rsplit('/',1)[-1]
    target=index.get((target_file,path_id))
    if not target or target[1]['classId']!=48:raise RuntimeError('Material shader reference does not resolve: '+name)
    shader_entry,shader_obj=target;needle=struct.pack('<i',len(expected_shader))+expected_shader.encode()
    if needle not in shader_obj['data']:raise RuntimeError('Unexpected shader for '+name)
    if not expected_keywords.issubset(keywords):raise RuntimeError('Packed material keywords missing: '+name)
    proof.append({'material':name,'entry':entry,'shader':expected_shader,'shaderEntry':shader_entry,'shaderPathId':path_id,'shaderBytes':len(shader_obj['data']),'keywords':keywords})
evidence={'validated':True,'apkSha256':hashlib.sha256(args.apk.read_bytes()).hexdigest(),'runtimeUiShaders':ui,'pipelineObjects':pipelines,'v3GameObjects':v3_objects,'compiledRenderingBootDiagnostics':True,'resolvedMaterialShaders':proof}
args.output.write_text(json.dumps(evidence,indent=2),encoding='utf8');print(json.dumps(evidence,indent=2))
