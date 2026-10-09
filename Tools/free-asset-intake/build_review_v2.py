"""Publish measured inventory and annotated evidence, never raw intake assets."""
import csv
import json
import math
import re
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont, ImageOps
from intake_v2 import ROOT, WORK, digest, write_json

DOCS=ROOT/'docs/free-asset-intake-v2'
UNKNOWN='NOT_MEASURED'
USE_MODELS={
    'env-quaternius-megakit':['Platform_Metal','Platform_DarkPlates','Platform_Rails','Platform_Ramp','Platform_Stairs','ShortWall_MetalPlates','Door_Frame','Door_DarkMetal','TopCables','Column_Pipes','Prop_PipeHolder','Prop_Vent','Prop_Light'],
    'env-molten-maps':['Generator','Cryo Tube','Centrifuge','Command','Container','Floor Metal Square Grate','Floor Hazard'],
    'env-creepycat-starter':['Floor_Squared_01','Floor_Squared_02','Floor_Hole_01','Wall_Gear_01','Wall_Pipes_01','DoorWay_01','Crate_01']
}

def item_decision(row,decision):
    sid=row['sourceId'];name=row['assetName'];kind=row['assetType'];path=row['originalPath'].lower()
    if decision=='USE':
        if kind=='model' and any(token.casefold() in name.casefold() for token in USE_MODELS.get(sid,[])):
            return 'USE','Selected visible geometry family; material/scale pass and source license constraints apply.'
        if sid=='ui-exe' and kind=='ui_image' and 'normal assets' in path and row['fileFormat'].lower()=='.png':
            return 'USE','Selected normal-resolution frame art; slicing/atlas/recolor and CC BY attribution required.'
        if kind=='prefab_or_graph':return 'DONOR','Source prefab is an audit/reference only; rebuild safe selected geometry without source behavior.'
        return 'REJECT','Outside the selected visible families, or a duplicated/source-preview UI representation.'
    if decision=='DONOR':
        if kind=='ui_image' and any(t in path for t in ['preview','sample','sheet','cover']):return 'REJECT','Source preview or sheet, not a new production component.'
        if sid=='env-dmitrii-industrial' and any(t in name.lower() for t in ['hangar','grass','road','concrete_fence']):return 'REJECT','Real-world architecture/dressing outside the selected industrial donor roles.'
        if sid=='weapons-rts-assets' and 'vehicle' in name.lower():return 'REJECT','Unchanged RTS vehicle outside the selected weapon/support donor role.'
        return 'DONOR','Parts/maps/reference only; follow source/item restrictions in ASSET_SELECTION_MATRIX; no unchanged source behavior.'
    return decision,'Source-wide rejection or unresolved license/technical gate applies.'

def inventory():
    inspected=json.loads((WORK/'reports/inspection.json').read_text(encoding='utf-8'))
    by_source={x['sourceId']:x for x in inspected}
    rows=[]
    fields=['sourceId','assetName','originalPath','assetType','fileFormat','sourceDecision','itemDecision','selectionReason','triangleCount','vertexCount','submeshCount','materialCount','importedTextureCount','maxPreviewTextureResolution','sourceImageCount','sourceMaxImageResolution','pbrMapNameHints','shaderFamily','LOD','animationClips','rigged','boneCount','skinnedMeshCount','colliders','lights','particleSystems','maxParticlesPerSystem','serializedParticleCapSum','rigidbodies','scriptsComponents','shaderDependencies','mobileRisk','urpRisk','spriteCount','atlasUsage','sourceAvailability','nativeResolution','nineSliceCandidate','hudComponents','inventoryComponents','healthStatComponents','minimapComponents','measurement','notes']
    def common(sid,path,kind):
        source=by_source[sid]
        imgs=source['images']
        maps=sorted({token for token in ['albedo','basecolor','base_color','diffuse','normal','rough','metal','occlusion','emiss','specular','_orm'] if any(token in x['path'].lower() for x in imgs)})
        deps=source['customDependencies']
        if not isinstance(deps,str):deps=json.dumps(deps,ensure_ascii=False)
        return dict(sourceId=sid,assetName=Path(path).stem,originalPath=path,assetType=kind,fileFormat=Path(path).suffix,sourceImageCount=len(imgs),sourceMaxImageResolution=max([max(x.get('width',0),x.get('height',0)) for x in imgs],default=0),pbrMapNameHints='SOURCE_SCOPE_ONLY:'+(';'.join(maps) or 'none'),shaderDependencies=deps,notes='Source image counts/max include previews; map-name hints do not confirm assignment to this asset.')
    with (WORK/'reports/unity_models.csv').open(encoding='utf-8-sig',newline='') as f:
        for raw in csv.DictReader(f):
            row=common(raw['sourceId'],raw['originalPath'],'model')
            row.update({k:v for k,v in raw.items() if k in fields})
            row['importedTextureCount']=raw['textureCount']
            row['notes']+=' Texture metrics are imported FBX bindings before neutral conversion; zero does not mean source maps are absent. LOD is imported hierarchy only; exported _LOD1 alternatives are separate rows. Animations enumerated, not playback validated. Counts include each renderer mesh instance.'
            rows.append(row)
    for source in inspected:
        sid=source['sourceId'];base=ROOT/source['contentRoot']
        if sid=='reference-concept-board':continue
        for asset in source['yamlAssets']:
            path=asset['path']
            if Path(path).suffix.lower() not in {'.prefab','.vfx','.shadergraph','.shader'}:continue
            row=common(sid,path,'vfx_prefab' if sid.startswith('vfx-') else 'prefab_or_graph')
            raw=(base/path).read_bytes();yaml=raw.startswith((b'%YAML',b'\xef\xbb\xbf%YAML'))
            c=asset['classes']
            text=raw.decode('utf-8-sig',errors='replace') if yaml else ''
            material_lists=re.findall(r'm_Materials:\s*\n((?:\s+-\s*\{[^\n]+\}\s*\n)+)',text)
            material_refs={ref for group in material_lists for ref in re.findall(r'\{[^\n]+\}',group) if 'fileID: 0' not in ref}
            row['materialCount']=len(material_refs) if yaml else UNKNOWN
            def count(*ids):return sum(int(c.get(str(i),0)) for i in ids) if yaml else UNKNOWN
            row.update(colliders=count(64,65,135,136,143,154),lights=count(108),particleSystems=count(198),rigidbodies=count(54),skinnedMeshCount=count(137),scriptsComponents=';'.join(asset['scripts']) if yaml else UNKNOWN,LOD=str(asset['lod']) if yaml else UNKNOWN,shaderFamily='SOURCE_DEPENDENCIES_NOT_IMPORTED',shaderDependencies=';'.join(asset['shaderGuids']),maxParticlesPerSystem=';'.join(asset['particleMax']) if yaml else UNKNOWN,serializedParticleCapSum=sum(map(int,asset['particleMax'])) if yaml else UNKNOWN,measurement='STATIC_YAML_COMPONENT_AUDIT' if yaml else 'BINARY_PREFAB_NOT_IMPORTED',urpRisk='NOT_RUNTIME_VALIDATED',mobileRisk='HIGH_OVERDRAW_REVIEW' if sid.startswith('vfx-') else 'PREFAB_HIERARCHY_NOT_RENDERED')
            if not yaml:row['notes']+=' Binary prefab deliberately not imported; component/mesh counts are unknown, not zero.'
            else:row['notes']+=' Serialized caps are not simultaneous live-particle measurements; script GUID references do not mean third-party code was executed. Nested prefab contents may be external and are not expanded.'
            rows.append(row)
        if sid.startswith('ui-'):
            formats=source['extensionCounts'];availability=';'.join(k for k in ['.psd','.fig','.svg','.ttf','.otf'] if formats.get(k,0)) or 'PNG_ONLY'
            if sid=='ui-exe':availability+=';Figma external link only, not local .fig'
            for image in source['images']:
                path=image['path'];row=common(sid,path,'ui_image')
                tokens=path.lower();sheet=any(x in tokens for x in ['cover','sample','preview','spritesheet','sheet'])
                row.update(spriteCount='UNKNOWN_SUBRECTS' if sheet or sid=='ui-tiago' else '1_FILE_VARIANT_NOT_UNIQUE_COMPONENT',atlasUsage='SOURCE_SHEET_UNSLICED' if sheet or sid=='ui-tiago' else 'NO_PRODUCTION_ATLAS_BUILT',sourceAvailability=availability,nativeResolution=f"{image.get('width','?')}x{image.get('height','?')}",nineSliceCandidate='YES_INFERRED_NEEDS_BORDER_AUTHORING' if any(x in tokens for x in ['button','panel','window','inventory','tile','frame']) else 'NO_OR_UNCERTAIN',hudComponents='frame/buttons/dividers' if sid=='ui-exe' else 'panel/button/bar/glyph variants' if sid=='ui-kenney' else 'frames; Icons8 excluded',inventoryComponents='selected/unselected slots' if 'inventory' in tokens else 'custom layout required',healthStatComponents='bar skins only; custom readable numbers/fill required',minimapComponents='custom frame composition; no map behavior',measurement='PIL_NATIVE_IMAGE_DIMENSIONS',mobileRisk='ATLAS_AND_OVERDRAW_REVIEW',urpRisk='UI_IMPORT_SLICE_REQUIRED')
                row['notes']+=' UI image totals include colors/states/upscaled duplicates and source sheets. No fabricated sprite count, atlas, 9-slice borders or finished HUD.'
                rows.append(row)
    selections=json.loads((ROOT/'Tools/free-asset-intake/selection_v2.json').read_text(encoding='utf-8'))
    decisions={s['sourceId']:s['decision'] for s in selections['sources']}
    for row in rows:
        row['sourceDecision']=decisions[row['sourceId']]
        row['itemDecision'],row['selectionReason']=item_decision(row,row['sourceDecision'])
    with (DOCS/'TECHNICAL_ASSET_INVENTORY.csv').open('w',encoding='utf-8',newline='') as f:
        writer=csv.DictWriter(f,fieldnames=fields,extrasaction='ignore');writer.writeheader()
        for row in rows:writer.writerow({k:row.get(k,UNKNOWN) for k in fields})
    summary={'rows':len(rows),'measuredModels':sum(r['assetType']=='model' for r in rows),'staticPrefabGraphRows':sum(r['assetType'] in {'vfx_prefab','prefab_or_graph'} for r in rows),'uiImageRows':sum(r['assetType']=='ui_image' for r in rows)}
    write_json(DOCS/'INVENTORY_SUMMARY.json',summary)
    return summary

def font(size,bold=False):
    return ImageFont.truetype('C:/Windows/Fonts/'+('segoeuib.ttf' if bold else 'segoeui.ttf'),size)

def make_sheet(filename,title,subtitle,tiles,ui=False):
    cols=4;tw=320;th=300;header=110
    image=Image.new('RGB',(cols*tw,header+math.ceil(len(tiles)/cols)*th+34),(14,22,29));draw=ImageDraw.Draw(image)
    draw.text((18,12),title,font=font(28,True),fill=(218,237,245));draw.text((18,52),subtitle,font=font(16),fill=(163,189,203))
    draw.text((18,78),'GRAVIVORE / FREE ASSET INTAKE V2 • comparison evidence, no production integration',font=font(15),fill=(108,153,174))
    manifest=[]
    for n,tile in enumerate(tiles):
        x=(n%cols)*tw;y=header+(n//cols)*th
        draw.rounded_rectangle((x+5,y+4,x+tw-5,y+th-4),radius=6,fill=(23,34,44),outline=(48,66,79))
        with Image.open(tile['path']) as im:
            im=im.convert('RGBA');im.thumbnail((tw-14,th-80));canvas=Image.new('RGBA',(tw-14,th-80),(137,148,158,255) if ui else (17,27,35,255));canvas.alpha_composite(im,((canvas.width-im.width)//2,(canvas.height-im.height)//2));image.paste(canvas.convert('RGB'),(x+7,y+8))
        draw.text((x+12,y+th-67),tile['source'],font=font(14,True),fill=(88,211,225))
        name=tile['name'];
        if len(name)>39:name=name[:36]+'…'
        draw.text((x+12,y+th-46),name,font=font(14),fill=(222,231,234));draw.text((x+12,y+th-25),tile['metric'],font=font(12),fill=(155,175,187))
        manifest.append({'sourceId':tile['source'],'originalPath':tile['original'],'renderSha256':digest(Path(tile['path'])),'tile':n+1})
    draw.text((18,image.height-26),'No bloom • neutral source-map conversion • compare silhouette/details, not device performance',font=font(13),fill=(137,167,184))
    target=DOCS/'evidence'/filename;target.parent.mkdir(parents=True,exist_ok=True);image.save(target,optimize=True)
    return {'file':'evidence/'+filename,'sha256':digest(target),'tiles':manifest}

def sheets():
    captures=json.loads((WORK/'reports/render_manifest.json').read_text(encoding='utf-8'))['captures']
    metrics={}
    with (WORK/'reports/unity_models.csv').open(encoding='utf-8-sig',newline='') as f:
        for row in csv.DictReader(f):metrics[(row['sourceId'],row['originalPath'])]=row
    def tiles(category):
        picked=[];seen=set()
        # Prioritize coherent stack, then compare serious alternative sources.
        priority=['env-quaternius-megakit','env-molten-maps','env-creepycat-starter','env-sickhead-construction','env-quaternius-essentials']
        ordered=sorted(captures,key=lambda c:(priority.index(c['sourceId']) if c['sourceId'] in priority else 9,c['sourceId'],c['originalPath']))
        caps={};limit=8 if category=='ENVIRONMENT' else 4
        for capture in ordered:
            if capture['category']!=category:continue
            sid=capture['sourceId'];name=Path(capture['originalPath']).stem
            source_limit=12 if sid=='env-quaternius-megakit' and category=='ENVIRONMENT' else limit
            if (sid,name) in seen or caps.get(sid,0)>=source_limit:continue
            with Image.open(capture['renderPath']) as im:
                pixels=list(im.resize((60,45)).get_flattened_data());magenta=sum(r>200 and g<65 and b>200 for r,g,b in pixels)/len(pixels)
                if magenta>.01:raise ValueError('Invalid magenta render rejected: '+capture['renderPath'])
            seen.add((sid,name));caps[sid]=caps.get(sid,0)+1;m=metrics[(sid,capture['originalPath'])]
            picked.append({'path':capture['renderPath'],'source':sid,'name':name,'metric':f"{m['triangleCount']} tris • {m['materialCount']} mat • 4m plinth",'original':capture['originalPath']})
        return picked
    evidence=[]
    labels=[('ENVIRONMENT','environment_contact_sheet.png','Environment / floor, walls, gates'),('MACHINERY_HERO_PROPS','machinery_contact_sheet.png','Machinery / containers, generators, service props'),('PIPES_SERVICES','pipes_contact_sheet.png','Pipes / services and cable silhouettes'),('MECHS_ENEMIES','mechs_contact_sheet.png','Mechs / enemy candidates — G-0 stays approved bipedal'),('WEAPONS_TURRETS','weapons_contact_sheet.png','Weapons / assembled groups and donor parts')]
    for category,name,title in labels:
        items=tiles(category)
        for offset in range(0,len(items),24):
            page=offset//24+1;page_count=math.ceil(len(items)/24)
            page_name=name if page==1 else name.replace('.png',f'_{page:02}.png')
            page_title=title+(f' — {page}/{page_count}' if page_count>1 else '')
            evidence.append(make_sheet(page_name,page_title,'Elevated Unity 6000.3.0f1 / URP17.3.0 • longest dimension normalized to 4m',items[offset:offset+24]))
    inspected=json.loads((WORK/'reports/inspection.json').read_text(encoding='utf-8'));uitiles=[]
    for source in inspected:
        if source['sourceId'] not in {'ui-exe','ui-kenney','ui-tiago'}:continue
        imgs=[x for x in source['images'] if Path(x['path']).suffix.lower()=='.png' and not any(t in x['path'].lower() for t in ['upsized','cover','preview','sample'])]
        if source['sourceId']=='ui-kenney':
            picks=[]
            for token in ['panel','button','bar','crosshair']:
                hit=next((x for x in imgs if token in x['path'].lower()),None)
                if hit:picks.append(hit)
            imgs=picks
        for im in imgs[:9 if source['sourceId']=='ui-exe' else 4]:
            path=ROOT/source['contentRoot']/im['path'];name=Path(im['path']).parent.name+'/'+Path(im['path']).stem
            uitiles.append({'path':path,'source':source['sourceId'],'name':name,'metric':f"Native {im['width']}x{im['height']} • {'rights unresolved' if source['sourceId']=='ui-tiago' else 'slice/atlas required'}",'original':im['path']})
    evidence.append(make_sheet('ui_contact_sheet.png','UI / native-source comparison','Local PNG colors on neutral gray to reveal black frames • no finished HUD is implied',uitiles,ui=True))
    write_json(DOCS/'EVIDENCE_MANIFEST.json',{'schemaVersion':1,'captureCount':len(captures),'sheets':evidence,'vfx':'Static captures intentionally omitted; static YAML inventory only, animation/overdraw not runtime validated.'})
    return [(x['file'],len(x['tiles'])) for x in evidence]

if __name__=='__main__':
    print('INVENTORY',inventory());print('CONTACT_SHEETS',sheets())
