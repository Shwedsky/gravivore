"""Record executed Unity outcomes and verify the art-only runtime scope."""
import hashlib, json, subprocess, xml.etree.ElementTree as ET
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
DOC=ROOT/'docs/visual-production-v2/g0-bipedal-v2'
results={}
for platform in ['EditMode','PlayMode']:
    path=DOC/'verification'/f'{platform}.xml'; r=ET.parse(path).getroot()
    assert int(r.get('failed'))==0, platform
    results[platform]={'result':r.get('result'),'total':int(r.get('total')),
        'passed':int(r.get('passed')),'failed':int(r.get('failed')),'skipped':int(r.get('skipped')),
        'sha256':hashlib.sha256(path.read_bytes()).hexdigest()}
validator=(DOC/'verification/project-validation-final.log').read_text(encoding='utf-8',errors='replace')
assert 'terminate with return code 0' in validator
assert 'error CS' not in validator
buildlog=(DOC/'verification/AndroidBuild.log').read_text(encoding='utf-8',errors='replace')
assert 'terminate with return code 0' in buildlog
apk=ROOT/'Builds/Android/gravivore-dev-0.1.0+2.apk'
meta=json.loads((DOC/'verification/AndroidBuildMetadata.json').read_text())
assert meta['targetArchitecture']=='ARM64' and meta['applicationId']=='com.gravivore.mobile.dev'
subprocess.run(['git','diff','--quiet','origin/main','--','Assets','Packages','ProjectSettings'],cwd=ROOT,check=True)
report={'status':'PASS','unity':'6000.3.0f1','compile':'Completed; no C# compiler errors',
        'project_validator':{'entrypoint':'Gravivore.Editor.ProjectValidator.ValidateOrThrow','exit':0},
        'tests':results,'playmode_skip':'Optional structural capture export requires GRAVIVORE_VISUAL_INTEGRATION_QA',
        'android':{'status':'PASS','exit':0,'manifest_verification':'PASS via aapt',
          'apk_path':str(apk),'bytes':apk.stat().st_size,'sha256':hashlib.sha256(apk.read_bytes()).hexdigest(),'metadata':meta},
        'runtime_scope':'Assets, Packages and ProjectSettings exactly match origin/main after generated serialization cleanup',
        'runtime_base_sha':subprocess.check_output(['git','rev-parse','origin/main'],cwd=ROOT,text=True).strip(),
        'g0_v2_in_apk':False,'device_test':'Not run; this is a regression build, not an integrated G-0 preview'}
(DOC/'data/unity_verification.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print('UNITY_VERIFICATION_RECORDED',json.dumps({k:v for k,v in report.items() if k!='android'}))
