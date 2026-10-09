"""Task-scope release gate: immutable inputs, private art, provenance and evidence."""
import csv
import json
import re
import subprocess
from collections import Counter
from pathlib import Path
from intake_v2 import ROOT, WORK, digest, verify_originals, write_json

BASE='a6541ef0c78f1489354bb4a5777de5e5b8847ce5'
DOCS=ROOT/'docs/free-asset-intake-v2'
REQUIRED=['SOURCE_DISCOVERY_REPORT.md','SOURCE_RESOLUTION.json','ASSET_PROVENANCE_RESOLVED.csv','ATTRIBUTION_DRAFT.md','TECHNICAL_ASSET_INVENTORY.csv','ASSET_SELECTION_MATRIX.md','ASSET_ZOO_REPORT.md','RECOMMENDED_ASSET_STACK.md','CHAPTER01_REPLACEMENT_MATRIX.md','UNFILLED_ART_GAPS.md']

def git(*args):
    return subprocess.check_output(['git',*args],cwd=ROOT,text=True,encoding='utf-8').strip()

def validate():
    checks=[]
    def gate(name,condition):
        if not condition:raise ValueError('FAIL '+name)
        checks.append(name);print('PASS',name,flush=True)
    verify_originals();checks.append('27 original owner files rehashed and byte-identical')
    gate('all required deliverables exist',all((DOCS/name).is_file() for name in REQUIRED))
    gate('merge baseline is an ancestor',subprocess.run(['git','merge-base','--is-ancestor',BASE,'HEAD'],cwd=ROOT).returncode==0)
    changed=set(git('diff','--name-only',BASE).splitlines())|set(git('ls-files','--others','--exclude-standard').splitlines())
    gate('production and gameplay unchanged',all(p=='.gitignore' or p.startswith(('Tools/free-asset-intake/','docs/free-asset-intake-v2/')) for p in changed))
    def owned(path):
        ext=Path(path).suffix.lower()
        return path=='.gitignore' or ext in {'.py','.ps1','.cs','.json','.csv','.md'} or (ext=='.png' and path.startswith('docs/free-asset-intake-v2/evidence/')) or (path.startswith('Tools/free-asset-intake/urp-baseline/') and ext in {'.asset','.meta'})
    gate('changed files contain only owned tools/docs/annotated evidence',all(owned(p) for p in changed))
    gate('raw intake and scratch remain ignored',all(git('check-ignore',path) for path in ['98_unclassified/Animated Mech Pack - March 2021-20261008T180817Z-1-001.zip','00_reference/01_gravivore_concept_board/Концепт GRAVIVORE_ Мехи и окружение.png','ExternalAssetIntake/FreeAssetIntakeV1/_work_v2/unity_zoo/catalog.json']))
    resolution=json.loads((DOCS/'SOURCE_RESOLUTION.json').read_text(encoding='utf-8'));expected=resolution['expectedSources']
    gate('every discovered payload has a resolution',len(resolution['discoveredPayloads'])==27 and all(x['state'] in {'FOUND','UNRESOLVED','DUPLICATE'} for x in resolution['discoveredPayloads']))
    meshy=[x for x in expected if x['sourceId'].startswith('meshy-')]
    gate('all 11 Meshy entries skipped_subscription',len(meshy)==11 and all(x['state']=='SKIPPED_SUBSCRIPTION' for x in meshy))
    gate('one real missing source only',[x['sourceId'] for x in expected if x['state']=='MISSING']==['pipes-armored'])
    with (DOCS/'ASSET_PROVENANCE_RESOLVED.csv').open(encoding='utf-8',newline='') as f:provenance={x['sourceId']:x for x in csv.DictReader(f)}
    selection=json.loads((DOCS/'SELECTION_SUMMARY.json').read_text(encoding='utf-8'))
    gate('all 26 discovered art sources selected once',len(selection['sources'])==26 and {s['sourceId'] for s in selection['sources']}=={x['sourceId'] for x in resolution['discoveredPayloads'] if x['sourceId']!='reference-concept-board'})
    gate('all USE licenses permit production',all(provenance[x['sourceId']]['productionUseAllowed']=='True' and provenance[x['sourceId']]['license']!='UNKNOWN_REQUIRES_REVIEW' for x in selection['sources'] if x['decision']=='USE'))
    gate('weighted scores correct',all(abs(x['weightedScore']-round(sum(a*b for a,b in zip(x['scores'],[.35,.20,.15,.15,.10,.05])),2))<1e-8 for x in selection['sources']))
    matrix=(DOCS/'CHAPTER01_REPLACEMENT_MATRIX.md').read_text(encoding='utf-8')
    blocks=re.findall(r'^## \d\d\. .+?(?=^## |\Z)',matrix,re.M|re.S)
    gate('matrix covers all 24 roles and six fields',len(blocks)==24 and all(all(field+':' in block for field in ['CURRENT','PROBLEM','RECOMMENDED SOURCE','USE / DONOR / CUSTOM','EXPECTED VISUAL GAIN','TECH RISK']) for block in blocks))
    stack=(DOCS/'RECOMMENDED_ASSET_STACK.md').read_text(encoding='utf-8')
    gate('one concrete stack covers nine roles',all(name in stack for name in ['## Environment backbone','## Floor / wall / damage detail','## Machinery / hero props','## Pipes / services','## Enemy donors','## Elite / boss donors','## Weapon / equipment donors','## VFX','## UI']))
    imported=list((WORK/'unity_zoo/Assets/Intake').rglob('*'))
    gate('source art allowlist, no payload code or prefab execution',all(p.suffix.lower() in {'.fbx','.png','.mat','.meta'} for p in imported if p.is_file()))
    normalization=json.loads((WORK/'reports/normalization_audit.json').read_text(encoding='utf-8'));by_source={s['sourceId']:s for s in json.loads((WORK/'reports/inspection.json').read_text(encoding='utf-8'))}
    from prepare_zoo import preview_material
    binary=0;upgraded=[]
    for row in normalization:
        if 'scratchPath' not in row or Path(row['originalPath']).suffix.lower()!='.mat':continue
        original=ROOT/by_source[row['sourceId']]['contentRoot']/row['originalPath'];raw=original.read_bytes()
        if not raw.startswith((b'%YAML',b'\xef\xbb\xbf%YAML')):
            binary+=1
            if digest(original)!=row['sha256Original'] or preview_material(raw)!=raw:raise ValueError('Binary material source/copy function modified: '+row['originalPath'])
            scratch=(WORK/'unity_zoo'/row['scratchPath']).read_bytes()
            if scratch!=raw:
                if not scratch.startswith((b'%YAML',b'\xef\xbb\xbf%YAML')):raise ValueError('Unexpected binary scratch alteration: '+row['originalPath'])
                upgraded.append({'sourceId':row['sourceId'],'originalPath':row['originalPath'],'scratchSha256':digest(WORK/'unity_zoo'/row['scratchPath'])})
    gate('binary material sources intact; copy function preserves bytes',binary>0)
    print('UNITY_SCRATCH_BINARY_TO_YAML_UPGRADES',len(upgraded),flush=True)
    captures=json.loads((WORK/'reports/render_manifest.json').read_text(encoding='utf-8'))['captures']
    gate('representative model capture count',len(captures)==120)
    from PIL import Image
    for c in captures:
        with Image.open(c['renderPath']) as im:
            pixels=list(im.convert('RGB').resize((60,45)).get_flattened_data())
            if sum(r>200 and g<65 and b>200 for r,g,b in pixels)/len(pixels)>.01:raise ValueError('Magenta frame: '+c['renderPath'])
    gate('all 120 selected frames pass magenta screening',True)
    evidence=json.loads((DOCS/'EVIDENCE_MANIFEST.json').read_text(encoding='utf-8'))
    valid={(c['sourceId'],c['originalPath']):digest(Path(c['renderPath'])) for c in captures}
    for source in by_source.values():
        if source['sourceId'].startswith('ui-'):
            for im in source['images']:valid[(source['sourceId'],im['path'])]=digest(ROOT/source['contentRoot']/im['path'])
    for sheet in evidence['sheets']:
        gate('sheet and tile identity '+sheet['file'],digest(DOCS/sheet['file'])==sheet['sha256'] and all(valid[(t['sourceId'],t['originalPath'])]==t['renderSha256'] for t in sheet['tiles']))
    with (DOCS/'TECHNICAL_ASSET_INVENTORY.csv').open(encoding='utf-8',newline='') as f:inventory=list(csv.DictReader(f))
    gate('2727 inventory rows and 1171 measured models',len(inventory)==2727 and sum(x['assetType']=='model' for x in inventory)==1171)
    gate('every inventoried item has a decision; all item USE licenses confirmed',all(x['itemDecision'] in {'USE','DONOR','REJECT','UNRESOLVED'} and (x['itemDecision']!='USE' or provenance[x['sourceId']]['productionUseAllowed']=='True') for x in inventory))
    gate('all selected MegaKit captures use declared source map bindings',all('declared source glTF' in c['materialMode'] for c in captures if c['sourceId']=='env-quaternius-megakit'))
    log=(WORK/'reports/Unity_Inspect.log').read_text(encoding='utf-8',errors='replace')
    gate('scratch compilation and inspection completed',not re.search(r'error CS\d+',log) and 'INTAKE_ZOO_COMPLETE models=1171 captures=120' in log and 'return code 0' in log)
    summary={'schemaVersion':1,'date':'2026-10-09','baseline':BASE,'status':'PASS_TASK_SCOPE_WITH_DOCUMENTED_RENDER_RESOURCE_LIMITS','checks':checks,'originalFiles':27,'originalBytes':sum(x['bytes'] for x in json.loads((WORK/'reports/original_snapshot.json').read_text(encoding='utf-8'))),'binaryMaterialSourcesIntact':binary,'unityScratchSerializationUpgrades':upgraded,'expectedSourceStates':dict(Counter(x['state'] for x in expected)),'selectionCounts':selection['counts'],'inventoryRows':len(inventory),'measuredModels':1171,'captures':len(captures),'contactSheets':len(evidence['sheets']),'evidenceBytes':sum((DOCS/s['file']).stat().st_size for s in evidence['sheets']),'unityLogSha256':digest(WORK/'reports/Unity_Inspect.log'),'compile':'Owned scratch editor code compiled as part of Inspect; no separate production compile','unityInspectExitCode':0,'vfxRuntime':'NOT_RUN_STATIC_AUDIT_ONLY','productionValidator':'NOT_RUN_PRODUCTION_OUTSIDE_TASK_SCOPE','androidBuild':'NOT_REQUIRED_BY_EXPLICIT_V2_TASK'}
    write_json(DOCS/'VALIDATION_SUMMARY.json',summary)
    print('V2_REVIEW_VALIDATION_READY',summary['selectionCounts'],summary['evidenceBytes'])

if __name__=='__main__':validate()
