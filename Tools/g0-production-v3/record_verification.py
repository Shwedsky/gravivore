"""Record executed Unity/XML/source checks and the actual regression APK."""
import json,hashlib,subprocess,shutil,xml.etree.ElementTree as ET
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];DOC=ROOT/'docs/g0-production-v3';V=DOC/'verification';D=DOC/'data'
def tests(name):
    r=ET.parse(V/(name+'.xml')).getroot()
    result={k:r.attrib.get(k) for k in ['result','total','passed','failed','skipped','duration']}
    result['ignored']=[dict(name=t.attrib['fullname'],reason=''.join(t.find('reason').itertext()).strip()) for t in r.iter('test-case') if t.attrib.get('result')=='Skipped']
    assert int(result['failed'])==0
    assert int(result['passed'])>0
    return result
edit=tests('EditMode');play=tests('PlayMode')
for name in ['Capture','Validate','Build']:
    log=(V/(name+'.log')).read_text(encoding='utf-8',errors='replace')
    assert 'Application will terminate with return code 0' in log,name+' execution not verified'
    assert 'Scripts have compiler errors.' not in log,name+' compile failure'
    if name=='Capture':assert 'G0_V3_CAMERA_REST_SCALE_PASS' in log and 'G0_V3_CAPTURE_AND_IMPORT_PASS' in log
asset=json.loads((D/'reopened_asset_validation.json').read_text());fbx=json.loads((D/'fbx_roundtrip_validation.json').read_text())
assert all(asset['checks'].values()) and fbx['pass']
evidence=json.loads((D/'evidence_manifest.json').read_text());assert evidence['status']=='PASS'
apk=ROOT/'Builds/Android/gravivore-dev-0.1.0+2.apk'
assert apk.is_file() and apk.stat().st_size>1000000
metadata=apk.with_suffix('.build.json');assert metadata.is_file()
meta=json.loads(metadata.read_text(encoding='utf-8-sig'))
shutil.copyfile(metadata,V/'AndroidBuildMetadata.json')
aapt=Path('C:/Program Files/Unity/Hub/Editor/6000.3.0f1/Editor/Data/PlaybackEngines/AndroidPlayer/SDK/build-tools/36.0.0/aapt.exe')
badging=subprocess.run([str(aapt),'dump','badging',str(apk)],check=True,capture_output=True,text=True).stdout
(V/'AndroidBadging.txt').write_text(badging,encoding='utf-8')
assert "name='com.gravivore.mobile.dev'" in badging
assert "versionCode='2'" in badging and "versionName='0.1.0'" in badging
assert "native-code: 'arm64-v8a'" in badging
head=meta['gitCommitSha']
report=dict(status='PASS',verified_code_checkpoint=head,unity='6000.3.0f1',compile='PASS',edit_mode=edit,play_mode=play,
            project_validation='PASS (includes VisualIntegrationValidator)',source_validation='PASS',fbx_roundtrip='PASS',unity_import_and_camera='PASS',
            required_evidence_count=len(evidence['required_captures']),android=dict(status='PASS',path=str(apk),bytes=apk.stat().st_size,
            sha256=hashlib.sha256(apk.read_bytes()).hexdigest(),metadata=meta,manifest_verified=True,scope='Existing runtime regression only; V3 art review is excluded from build scenes/canonical dependencies'),
            source_sha256=asset['source_sha256'],fbx_sha256=fbx['fbx_sha256'],device_performance='NOT MEASURED',live_integration='NOT PERFORMED')
(D/'verification_summary.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps({'status':report['status'],'EditMode':edit,'PlayMode':play,'APK':str(apk),'bytes':apk.stat().st_size},indent=2))
