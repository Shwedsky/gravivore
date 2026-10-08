"""Generate auditable source resolution and provenance; manual evidence overrides explicit."""
import csv
import json
from pathlib import Path
from intake_v2 import ROOT, WORK, write_json, digest
from prepare_zoo import identify

DOCS=ROOT/'docs/free-asset-intake-v2'
STORE_URLS={
    'mechs-medium-striker':'https://assetstore.unity.com/packages/3d/characters/robots/medium-mech-striker-124342',
    'weapons-rts-assets':'https://assetstore.unity.com/packages/3d/environments/sci-fi/rts-sci-fi-game-assets-v1-112251'}

def resolve():
    manifest=json.loads((ROOT/'docs/free-asset-intake-v1/SOURCE_MANIFEST.json').read_text(encoding='utf-8'))['sources']
    inspected=json.loads((WORK/'reports/inspection.json').read_text(encoding='utf-8'))
    rows=[]; expected=[]; discovery=[]; code=[]
    for item in inspected:
        sid=identify(item); item['sourceId']=sid
        source=next((s for s in manifest if s['id']==sid),None)
        if source is None:raise ValueError('Unmapped discovered payload: '+sid)
        metadata=item.get('storeMetadata',{})
        files=item['extensionCounts']
        signals=[]
        if metadata:
            signals=[f"Unity gzip FEXTRA id={metadata['id']}; publisher={metadata['publisher']['label']}; title={metadata['title']}; version={metadata['version']}",f"Reconstructed Unity Assets paths, {files.get('.fbx',0)} FBX / {files.get('.prefab',0)} prefabs"]
        else:
            signals=[f"Archive identity: {Path(item['path']).name}",f"Members: {', '.join(item['models'][:3])}" if item['models'] else f"Content: {files}"]
            signals.extend('Included '+x['path'] for x in item['licensesAndReadmes'] if 'license' in x['path'].lower() or 'credit' in x['path'].lower())
        if sid=='reference-concept-board':signals=['Owner supplied image at 00_reference/01_gravivore_concept_board','Image visually inspected: named GRAVIVORE, labelled enemies, environment modules and top-down gameplay frame; bipedal G-0 override applies']
        if sid=='pipes-techlab':signals.append('Indexed author listing names TechLab/TooManyDemons and a 20-piece set; sole TechPipes_Scene.fbx plus matching TechLab trimsheets')
        if sid=='ui-tiago':signals.append('Official publisher page names exact UI Sci-Fi Files.zip (3.7MB), two PSD dimensions match local 2000x730 / 3175x2082')
        row={'sourceId':sid,'payloadId':item['payloadId'],'state':'FOUND','detectedDisplayName':metadata.get('title',source['displayName']),'originalLocalPath':item['path'],'matchedManifestSource':sid,'matchingEvidence':signals,'matchingConfidence':'HIGH' if sid!='pipes-techlab' else 'MEDIUM','sha256':item['sha256'],'bytes':item['bytes'],'storeMetadata':metadata,'embeddedArchives':[]}
        if item['kind']!='LOOSE_ART':
            base=ROOT/item['contentRoot']
            row['embeddedArchives']=[{'path':p.relative_to(base).as_posix(),'state':'DUPLICATE','notes':'Same source bundled pipeline/source variant; not imported or executed','sha256':digest(p)} for p in base.rglob('*') if p.is_file() and p.suffix.lower() in {'.unitypackage','.rar','.zip','.7z'}]
        discovery.append(row)
        publisher=metadata.get('publisher',{}).get('label',source['publisher'])
        platform='UNITY_ASSET_STORE' if metadata else source['sourcePlatform']
        url=STORE_URLS.get(sid,source['sourceUrl'])
        license_class='UNKNOWN_REQUIRES_REVIEW'; evidence=''; allowed=False; redistribution=False; attr=None; attr_text=''; notes=''
        if metadata:
            license_class='UNITY_ASSET_STORE_EULA'; allowed=True; attr=False
            evidence=f"Local Unity header content id {metadata['id']}, publisher {publisher}; official product listing Standard Unity Asset Store EULA checked 2026-10-08; https://unity.com/legal/as-terms"
            notes='Commercial compiled-game use under owner acquisition/EULA; raw/modified source redistribution prohibited. Local version, not latest marketplace version, was inspected. No source files enter the public repo. Public V3 must use private donor artifacts or reproducible owner-local intake.'
            if sid=='mechs-medium-striker':notes+=' V1 Sketchfab mapping/CC BY expectation superseded: actual payload is MSGDI Unity id 124342, v1.1.'
            if sid=='weapons-rts-assets':notes+=' V1 unresolved entry resolved to free v1 id112251, not paid v3. Publisher header Vdr0id id15286 matches current Dmitrii Kutsenko.'
        elif sid.startswith(('env-quaternius','mechs-quaternius','weapons-quaternius','env-kenney','ui-kenney','env-molten')):
            local=next((x for x in item['licensesAndReadmes'] if 'CC0' in x['text']),None)
            if local is None:raise ValueError('Missing local CC0 evidence: '+sid)
            license_class='CC0'; allowed=redistribution=True; attr=False
            evidence='Included '+local['path']+'; file SHA256 '+digest(ROOT/item['contentRoot']/local['path'])+'; publisher URL '+url
            notes='Local license explicitly covers personal/commercial use; no raw assets committed even where redistribution is allowed.'
            if sid=='ui-kenney':notes+=' Included license confirms UI Pack: Sci-fi 2.0 despite archive filename space-expansion. 742 PNGs include colors/states/sample sheets; not 742 distinct HUD components.'
        elif sid=='ui-exe':
            license_class='CC_BY_4_0'; allowed=redistribution=True; attr=True
            evidence='Included crediting_guide.pdf identifies Catherine Laserna and CC BY 4.0; official https://cjlaserna.itch.io/exe confirms attribution license. Fonts Nunito / Zen Dots carry separate OFL.txt.'
            attr_text='EXE - Mini SciFi UI Pack by Catherine Laserna (cjlaserna), https://cjlaserna.itch.io/exe, CC BY 4.0 https://creativecommons.org/licenses/by/4.0/. Changes: none in V2; adaptation must be listed in V3.'
            notes='Nunito and Zen Dots are SIL OFL 1.1, not CC BY; preserve their copyright and OFL notices if bundled. No fonts imported.'
        elif sid=='ui-tiago':
            license_class='CUSTOM_FREE_COMMERCIAL'; attr=False
            evidence='Official https://thiff.itch.io/free-ui-sci-fi confirms personal/commercial use for creator artwork and matches exact ZIP/PSD dimensions.'
            allowed=False
            notes='FRAME-ONLY donor permitted after layered export excludes Icons8 icons and example text. Whole pack not cleared: official page names Icons8 icons (separate permission/attribution unresolved) and Audiowide font (not bundled). Raw redistribution not explicitly licensed.'
        elif sid=='pipes-techlab':
            attr=True
            evidence='Official Sketchfab indexed listing confirms TooManyDemons / TechLab and CC Attribution; direct page/API returned 403/inaccessible; exact license version not independently retrieved from payload (no bundled license).'
            attr_text='PROVISIONAL: TechLab: Modular Scifi Pipes by TooManyDemons, '+url+'. Confirm exact CC Attribution version before production and record modifications.'
            notes='Do not promote to production USE until exact license link/version is confirmed. Archive has conflicting duplicate texture members preserved separately. Geometry identity is established; material mapping needs reconstruction.'
        else:
            evidence='Owner reference image; visual authority only, not a production asset license.'
            notes='Do not redistribute full concept board or extract game assets from it. Reference has been inspected locally.'
        rows.append(dict(sourceId=sid,publisher=publisher,sourcePlatform=platform,sourceUrl=url,license=license_class,licenseEvidence=evidence,attributionRequired=attr,attributionText=attr_text,rawRedistributionAllowed=redistribution,productionUseAllowed=allowed,notes=notes))
        code.append({'sourceId':sid,'quarantinedCode':item['quarantinedCode'],'customDependencies':item['customDependencies'],'embeddedArchivesNotImported':row['embeddedArchives'],'shaderAssetsNotImported':files.get('.shader',0)+files.get('.shadergraph',0)+files.get('.shadersubgraph',0),'sourcePrefabsNotImported':files.get('.prefab',0)})
    for s in manifest:
        found=[r for r in discovery if r['sourceId']==s['id']]
        state='SKIPPED_SUBSCRIPTION' if s['sourcePlatform']=='MESHY' else 'FOUND' if found else 'MISSING'
        expected.append({'sourceId':s['id'],'displayName':s['displayName'],'state':state,'payloadIds':[r['payloadId'] for r in found],'reason':'Owner explicitly skipped subscription-only Meshy downloads' if state=='SKIPPED_SUBSCRIPTION' else 'No matching local payload' if state=='MISSING' else ''})
    write_json(DOCS/'SOURCE_RESOLUTION.json',{'schemaVersion':2,'baseline':'a6541ef0c78f1489354bb4a5777de5e5b8847ce5','originalAssetPayloadCount':26,'ownerReferenceCount':1,'discoveredPayloads':discovery,'expectedSources':expected})
    write_json(DOCS/'CODE_SAFETY_AUDIT.json',{'importPolicy':'art-only explicit allowlist; zero third-party executable imports/execution','sources':code})
    with (DOCS/'ASSET_PROVENANCE_RESOLVED.csv').open('w',newline='',encoding='utf-8') as f:
        w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
    report=['# Source discovery report','','Baseline: merged PR #66, `a6541ef0c78f1489354bb4a5777de5e5b8847ce5`. Same V1 worktree; V2 branch `art/free-asset-intake-v2`.','',
            'Discovered **26 asset archives/packages plus one owner concept board**, 5,000,627,772 input bytes. Original snapshot includes SHA-256 for all 27 files. The planned intake contains placeholders; actual downloads are in worktree-root `98_unclassified/` and `00_reference/`. Both roots are included in discovery and explicitly ignored. No sorting is required from the owner.','',
            'Concept image is PRESENT and visually inspected, including its top-down gameplay panel. G-0 stays the approved bipedal mech. Enemy text authority: `docs/visual-production-v2/ENEMY_VISUAL_TARGETS_APPROVED_V1.md`. Scoring is a technical-art recommendation, not owner acceptance or device performance.','',
            '26 downloaded art sources are identified; the reference is also FOUND. `pipes-armored` is the single missing non-Meshy source. All 11 expected Meshy sources are SKIPPED_SUBSCRIPTION. No duplicate owner downloads found; embedded pipeline/source archives are recorded as same-source DUPLICATE variants and excluded from Unity.','',
            'Important corrections: Striker is MSGDI Asset Store id124342, not the V1 Sketchfab license. RTS is the free v1 id112251; header publisher Vdr0id has id15286, matching current Dmitrii Kutsenko. Kenney UI archive name differs, but included license identifies Sci-fi 2.0. TechLab identity is matched, while exact attribution-license version and duplicate texture mapping remain open. Tiago whole-pack clearance is limited by separately sourced Icons8 icons.','']
    for r in discovery:
        report += [f"## {r['sourceId']} — FOUND",f"- Payload: `{r['originalLocalPath']}`",f"- Match confidence: {r['matchingConfidence']}; SHA256 `{r['sha256']}`",*['- '+x for x in r['matchingEvidence']],'']
    report+=['## Expected sources without payloads','']+[f"- `{s['sourceId']}`: **{s['state']}** — {s['reason']}" for s in expected if s['state']!='FOUND']
    (DOCS/'SOURCE_DISCOVERY_REPORT.md').write_text('\n'.join(report)+'\n',encoding='utf-8')
    (DOCS/'ATTRIBUTION_DRAFT.md').write_text('# Attribution draft\n\nNo production assets were integrated in V2. Required V3 credits must be included with any actual imports.\n\n'+ '\n\n'.join('## '+r['sourceId']+'\n\n'+r['attributionText'] for r in rows if r['attributionText'])+'\n\nNunito: Copyright 2014 The Nunito Project Authors. Zen Dots: Copyright 2021 The Dots Project Authors. Keep included SIL OFL 1.1 notices if these fonts are bundled.\n\nOptional credits: Quaternius; Kenney; Moltenbolt / Molten Maps. Store assets remain under each source publisher and owner EULA acquisition; no source redistribution permission is implied. Tiago frames must exclude unresolved Icons8 material and separately clear any font use.\n',encoding='utf-8')
    print('SOURCE_RESOLUTION_READY',len(discovery),'FOUND;',len([s for s in expected if s['state']=='MISSING']),'MISSING;',len([s for s in expected if s['state']=='SKIPPED_SUBSCRIPTION']),'MESHY_SKIPPED')

if __name__=='__main__':resolve()
