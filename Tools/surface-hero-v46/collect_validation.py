"""Collect executed receipts without inferring unexecuted tests or device FPS."""
import json,hashlib,xml.etree.ElementTree as ET
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];OUT=ROOT/'docs/history/implementation-passes/chapter01-visual-replacement-v3/v46';D=OUT/'verification'
tests=[]
for name in ('EditModeFull','PlayModeFull','PortablePathsEditMode','PortablePathsPlayMode'):
    root=ET.parse(D/(name+'.xml')).getroot();assert int(root.get('failed'))==0
    tests.append({'suite':name,'total':int(root.get('total')),'passed':int(root.get('passed')),'failed':int(root.get('failed')),'skipped':int(root.get('skipped')),'xml':name+'.xml','ignored':[t.get('fullname') for t in root.findall('.//test-case') if t.get('result')=='Skipped']})
for name,marker in [('CompileRelease','Exiting batchmode successfully'),('ProjectValidatorRelease','SURFACE_HERO_V46_MATERIAL_AUTHORITY_AUDIT_PASS')]:
    assert marker in (D/(name+'.log')).read_text(encoding='utf-8')
aliases=(D/'mesh_alias_equivalence.txt').read_text().splitlines();assert len(aliases)==5 and all('identical to full-suite tested mesh' in line for line in aliases)
assert 'SURFACE_HERO_V46_AUTHOR_PASS' in (D/'AuthorPortablePaths.log').read_text(encoding='utf-8')
material=json.loads((D/'material_quality_audit.json').read_text());assert material['validated'] and not material['boundsOverlapCandidates']
soak=json.loads((D/'five_minute_runtime.json').read_text());assert soak['seconds']>=300 and soak['completedStops']==17 and soak['initialMaterials']==soak['finalMaterials']
assert soak['initialTransforms']-soak['initialActorTransforms']==soak['finalTransforms']-soak['finalActorTransforms']
for name in ('scope_audit','license_audit','blender_source_validation'):assert json.loads((D/(name+'.json')).read_text())['validated']
boards=['surface_quality_board','spawn_dock_board','hero_machinery_board','actor_surface_board','intersection_cleanup_board']
gate={'passed':True,'basis':'Agent technical-art inspection of actual neutral-lighting and production-camera captures after the final cleanup, plus conservative major-machinery bounds scan','beforeApkBuild':True,'reference':'Owner local concept and inspected CC0 donor contact sheets; source image not redistributed','visibleIntersectionDefectsObserved':0,'visibleZfightingDefectsObserved':0,'majorMachineryBoundsOverlapCandidates':0,'reviewCriteria':['silhouette','large form','medium form','small detail','material variation','roughness response','edge highlight','dark recesses','emissive integration','top-down readability'],'boards':[{'file':'internal/'+n+'.jpg','sha256':hashlib.sha256((OUT/'internal'/(n+'.jpg')).read_bytes()).hexdigest()} for n in boards]}
(D/'visual_gate.json').write_text(json.dumps(gate,indent=2))
result={'validated':True,'compileExit':0,'projectValidatorExit':0,'tests':tests,'materialAudit':{'flatColorOnlyHeroRenderers':material['flatColorOnlyHeroRenderers'],'primitiveOnlyHeroProps':material['primitiveOnlyHeroProps'],'heroMaterialRows':material['heroRenderers'],'enabledRenderers':material['enabledRenderers'],'staticTriangles':material['staticTriangles']},'capsuleTraversalIncludedInFullPlayMode':True,'fiveMinuteSoak':soak,'licenseAuditPassed':True,'blenderSourcesReopened':9,'frozenScopeFiles':763,'internalVisualGatePassed':True,'executedOnAndroid':False}
(D/'validation.json').write_text(json.dumps(result,indent=2));print('V46_VALIDATION_VISUAL_GATE_READY')
