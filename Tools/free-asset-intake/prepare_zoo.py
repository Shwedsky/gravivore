"""Prepare a code-free, model/material/texture-only scratch project.

No source scenes, prefabs, shaders, package code, plugins, Blender files or custom
importers enter Unity. Source model metadata is inspected; importer defaults are
used except safe material references. All GUIDs are namespaced per source.
"""
import json
import re
import shutil
import uuid
from pathlib import Path
from PIL import Image
from intake_v2 import ROOT, WORK, write_json

SOURCE_BY_STORE = {'92152':'env-creepycat-starter','159280':'env-sickhead-construction','82913':'env-karboosx-modular','86679':'env-dmitrii-industrial','143414':'env-seed-hunter','82234':'mechs-combat-drone','124342':'mechs-medium-striker','57540':'weapons-warzone','246331':'weapons-tower-defence','112251':'weapons-rts-assets','335800':'vfx-impact','356686':'vfx-black-hole','247933':'vfx-magic','266226':'vfx-fire','351840':'vfx-fog'}
ZIP_MATCH = [('animated mech','mechs-quaternius'),('modular sci fi guns','weapons-quaternius'),('modular scifi megakit','env-quaternius-megakit'),('sci-fi essentials','env-quaternius-essentials'),('molten maps','env-molten-maps'),('modular-space','env-kenney-space'),('space-station','env-kenney-station'),('ui-pack-space','ui-kenney'),('techlab','pipes-techlab'),('exe -','ui-exe'),('ui sci-fi files','ui-tiago')]

def identify(item):
    store=item.get('storeMetadata',{}).get('id')
    if store in SOURCE_BY_STORE:return SOURCE_BY_STORE[store]
    if item['payloadId']=='p00':return 'reference-concept-board'
    name=Path(item['path']).name.lower()
    return next((source for token,source in ZIP_MATCH if token in name), 'unresolved-'+item['payloadId'])

def models(item):
    base=ROOT/item['contentRoot']
    candidates=[p for p in base.rglob('*') if p.is_file() and p.suffix.lower()=='.fbx']
    # Prefer textured variant, omit export-format/color duplicates, retain distinct LODs.
    unique={}
    for p in sorted(candidates,key=lambda p:('flat colors' in str(p).lower(), 'textured' not in str(p).lower(),str(p))):
        unique.setdefault(p.stem.casefold(),p)
    return list(unique.values())

def category(source,p):
    name=p.stem.lower()
    if source.startswith('mechs-') or name.startswith('enemy_'):return 'MECHS_ENEMIES'
    if source.startswith('weapons-') or name.startswith('gun_'):return 'WEAPONS_TURRETS'
    if source.startswith('pipes-') or any(x in name for x in ['pipe','cable','conduit','duct']):return 'PIPES_SERVICES'
    if source.startswith('vfx-'):return 'VFX'
    if any(x in name for x in ['reactor','generator','engine','machine','tank','computer','monitor','container','crate','battery','barrel','station','turret','centrifuge','cryo','server']):return 'MACHINERY_HERO_PROPS'
    return 'ENVIRONMENT'

def representatives(source,all_models):
    if source.startswith('vfx-'):return set()
    if source.startswith('mechs-') or source.startswith('pipes-'):return set(all_models)
    tokens=['floor','wall','door','gate','ramp','fence','rail','stair','damage','broken','hole','pipe','generator','reactor','engine','container','crate','tank','light','column']
    if source=='env-quaternius-essentials':tokens=['enemy_eye','enemy_quad','enemy_trilo','prop_barrel','prop_container','prop_crate','prop_console','prop_generator','prop_computer','prop_pillar','gun_rifle']
    if source=='env-molten-maps':tokens=['generator','cryo','centrifuge','command','container','wall','floor','ramp','monitor','light']
    if source=='weapons-quaternius':tokens=['ar_1','ar_4','sniper_1','pistol_1','barrel','scope','body','stock','magazine']
    if source=='weapons-rts-assets':tokens=['structure_v1','structure_v2','structure_v3','laser_tower','turret_v1','vehicle_v1']
    picks=[]
    for token in tokens:
        hit=next((p for p in all_models if token in p.stem.lower() and p not in picks),None)
        if hit:picks.append(hit)
    limit=14 if source in {'env-quaternius-megakit','env-creepycat-starter'} else 10
    if not picks:picks=all_models[:6]
    # Enemy candidates always included even if the source is an environment kit.
    return set(picks[:limit]+[p for p in all_models if p.stem.lower().startswith('enemy_')])

def prepare():
    inspection=json.loads((WORK/'reports/inspection.json').read_text(encoding='utf-8'))
    zoo=WORK/'unity_zoo'
    for d in ['Assets/Editor','Assets/Intake','Packages','ProjectSettings']:(zoo/d).mkdir(parents=True,exist_ok=True)
    shutil.copyfile(ROOT/'Tools/free-asset-intake/IntakeZoo.cs',zoo/'Assets/Editor/IntakeZoo.cs')
    manifest=json.loads((ROOT/'Packages/manifest.json').read_text())
    manifest['dependencies'].pop('com.unity.ide.visualstudio',None)
    write_json(zoo/'Packages/manifest.json',manifest)
    (zoo/'ProjectSettings/ProjectVersion.txt').write_text('m_EditorVersion: 6000.3.0f1\n',encoding='utf-8')
    catalog=[]; audit=[]
    for item in inspection:
        source=identify(item)
        item['sourceId']=source
        if source=='reference-concept-board' or source.startswith('ui-'):continue
        base=ROOT/item['contentRoot']; dest=zoo/'Assets/Intake'/source
        dest.mkdir(parents=True,exist_ok=True)
        chosen=models(item); reps=representatives(source,chosen)
        # Only art extensions in this allowlist may enter the scratch Unity project.
        copy_files=chosen+[p for p in base.rglob('*') if p.is_file() and p.suffix.lower() in {'.png','.jpg','.jpeg','.tga','.dds','.tif','.tiff','.psd','.mat'}]
        if source.startswith('vfx-'):continue # technical YAML audit only; no misleading fallback-shader VFX render.
        guids={}
        for p in copy_files:
            meta=Path(str(p)+'.meta')
            if meta.exists():
                text=meta.read_text(encoding='utf-8-sig',errors='replace')
                match=re.search(r'^guid: ([0-9a-f]{32})',text,re.M)
                if match:guids[match[1]]=uuid.uuid5(uuid.NAMESPACE_URL,source+'/'+match[1]).hex
        def rewrite(text):
            return re.sub(r'\b[0-9a-f]{32}\b',lambda m:guids.get(m[0],m[0]),text)
        for p in copy_files:
            rel=p.relative_to(base).as_posix()
            suffix=uuid.uuid5(uuid.NAMESPACE_URL,rel).hex[:8]
            # Flatten paths to avoid Windows long-path failures; identity remains in catalog/audit.
            target=dest/(p.stem+'__'+suffix+p.suffix.lower())
            if p.suffix.lower() in {'.png','.jpg','.jpeg','.tga','.dds','.tif','.tiff','.psd'}:
                try:
                    with Image.open(p) as im:
                        # Preview-only clamp; original resolutions stay in technical inventory.
                        im.thumbnail((1024,1024)); im.convert('RGBA').save(target.with_suffix('.png'))
                    target=target.with_suffix('.png')
                except (OSError,ValueError) as error:
                    audit.append({'sourceId':source,'originalPath':rel,'excludedTexture':str(error)})
                    continue
            elif p.suffix.lower()=='.mat':
                text=rewrite(p.read_text(encoding='utf-8-sig',errors='replace'))
                # Shader dependencies are audited separately. Scratch materials use a Unity built-in
                # shader solely to expose serialized source map/color properties for conversion.
                text=re.sub(r'm_Shader:.*', 'm_Shader: {fileID: 46, guid: 0000000000000000f0000000000000000, type: 0}',text)
                target.write_text(text,encoding='utf-8')
            else:shutil.copyfile(p,target)
            meta=Path(str(p)+'.meta')
            if meta.exists():
                text=meta.read_text(encoding='utf-8-sig',errors='replace')
                expected='ModelImporter:' if p.suffix.lower()=='.fbx' else 'TextureImporter:' if p.suffix.lower()!='.mat' else 'NativeFormatImporter:'
                if expected not in text:raise ValueError('Custom/unexpected importer rejected: '+str(meta))
                text=rewrite(text)
                Path(str(target)+'.meta').write_text(text,encoding='utf-8')
            audit.append({'sourceId':source,'originalPath':rel,'scratchPath':str(target.relative_to(zoo)),'sha256Original':__import__('intake_v2').digest(p),'normalization':'1024px preview texture / shader substituted / GUID namespaced as applicable'})
            if p in chosen:catalog.append({'sourceId':source,'assetPath':target.relative_to(zoo).as_posix(),'originalPath':rel,'category':category(source,p),'render':p in reps})
        print(source,'models',len(chosen),'render',len(reps),'safe files',len(copy_files),flush=True)
    write_json(zoo/'catalog.json',{'entries':catalog})
    write_json(WORK/'reports/normalization_audit.json',audit)
    write_json(WORK/'reports/inspection.json',inspection)
    if any(p.suffix.lower() not in {'.fbx','.png','.jpg','.jpeg','.tga','.dds','.mat','.meta'} for p in (zoo/'Assets/Intake').rglob('*') if p.is_file()):raise ValueError('Art import allowlist violated')
    print('ART_ONLY_PROJECT_READY',len(catalog),'models')

if __name__=='__main__':prepare()
