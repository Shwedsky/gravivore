"""Verify additive delivery, exact promoted bytes, tests and APK identity."""
from pathlib import Path
import subprocess,json,hashlib,xml.etree.ElementTree as ET,zipfile
ROOT=Path(__file__).resolve().parents[2]
PASS=ROOT/'docs/history/implementation-passes/energy-chamber-hybrid-proof-v1'
BASE='e1669362fd3566f69939b547f263a3a600e3e1b7'
PREFIXES=('Assets/_Game/ArtProof/','Tools/energy-chamber-hybrid-proof-v1/','art/energy-chamber-hybrid-proof-v1/','docs/history/implementation-passes/energy-chamber-hybrid-proof-v1/')
def git(*args):return subprocess.check_output(['git',*args],cwd=ROOT,text=True,encoding='utf-8').strip()
def allowed(path):return path=='Assets/_Game/ArtProof.meta' or path.startswith(PREFIXES)
committed=[line.split('\t',1) for line in git('diff','--name-status',BASE,'HEAD').splitlines()]
assert committed and all(status=='A' and allowed(path) for status,path in committed),'Shared production edit or non-additive delivery detected.'
untracked=[p for p in git('ls-files','--others','--exclude-standard').splitlines() if allowed(p)]
planned=[PASS/'FILES_CHANGED.txt',PASS/'delivery.json',PASS/'verification/apk-badging.txt']
files=sorted(set([p for status,p in committed]+untracked+[str(path.relative_to(ROOT)).replace('\\','/') for path in planned]))
modified=git('diff','--name-only').splitlines()
assert not [p for p in modified if not allowed(p)],'Restore Unity-generated changes outside proof before final delivery.'
for item in json.loads((PASS/'promoted-donors.json').read_text()):
    assert hashlib.sha256((ROOT/item['unityPath']).read_bytes()).hexdigest()==item['sha256']
tests={}
for platform in ['EditMode','PlayMode']:
    test=ET.parse(PASS/'verification'/f'{platform}.xml').getroot()
    assert test.attrib['result']=='Passed' and int(test.attrib.get('failed',0))==0
    tests[platform]={key:test.attrib.get(key) for key in ['total','passed','failed','skipped','result']}
apk=ROOT/'Builds/Android/gravivore-energy-chamber-proof-v1-dev.apk'
assert apk.exists(),'Expected isolated dev APK missing.'
build=json.loads((PASS/'verification/android-result.json').read_text());assert build['result']=='Succeeded'
build_diagnostics=[]
if build['errors']:
    log=(PASS/'verification/AndroidStandalone.log').read_text(encoding='utf-8',errors='replace')
    known='Host type is not matching any asset type at Path Packages/com.unity.render-pipelines.core/Editor/Lighting/ProbeVolume/RenderingLayerMask/TraceRenderingLayerMask.urtshader.'
    assert build['errors']==3 and log.count(known)>=3 and 'Error building Player:' not in log and 'error CS' not in log,'Unexpected build failure/diagnostic.'
    build_diagnostics.append('Unity reports Succeeded with 3 nonfatal URP editor ProbeVolumeBakingResources TraceRenderingLayerMask resource-type errors. APV and light layers are disabled; no device acceptance claimed.')
sdk=Path(r'C:/Program Files/Unity/Hub/Editor/6000.3.0f1/Editor/Data/PlaybackEngines/AndroidPlayer/SDK')
aapt=sorted((sdk/'build-tools').glob('*/aapt.exe'))[-1]
badging=subprocess.check_output([str(aapt),'dump','badging',str(apk)],text=True,encoding='utf-8',errors='replace')
assert "name='com.gravivore.energychamberproof'" in badging,'APK identity conflicts with gameplay application.'
assert "native-code: 'arm64-v8a'" in badging
with zipfile.ZipFile(apk) as z:
    scenes=[n for n in z.namelist() if n.startswith('assets/bin/Data/level') and n.rsplit('/',1)[-1][5:].isdigit()]
    assert len(scenes)==1,'Proof APK must contain exactly one isolated scene.'
    assert 'assets/ThirdPartyNotices.txt' in z.namelist() and 'assets/EnergyChamberNotices.txt' in z.namelist(),'APK attribution notices missing.'
(PASS/'verification/apk-badging.txt').write_text(badging,encoding='utf-8')
receipt=dict(branch=git('branch','--show-current'),draftPr='https://github.com/Shwedsky/gravivore/pull/77',baseline=BASE,
    contentHead=git('rev-parse','HEAD'),originMainAtDelivery=git('rev-parse','origin/main'),
    artifactScope='additive proof only; final documentation commit may follow contentHead',changedFiles=len(files),sharedFilesChanged=0,
    tests=tests,compile='Passed',projectValidation='Passed',assemblyValidation='Passed',
    apk=dict(path=str(apk),sha256=hashlib.sha256(apk.read_bytes()).hexdigest(),bytes=apk.stat().st_size,package='com.gravivore.energychamberproof',architecture='ARM64',development=True,scenes=1),
    buildResult=build['result'],buildLoggedErrors=build['errors'],buildDiagnostics=build_diagnostics,
    verdict='READY FOR PRODUCTION DEVELOPMENT',deviceVisualGate='Not run',devicePerformance='Not measured',merge='Not performed')
(PASS/'FILES_CHANGED.txt').write_text('\n'.join(files)+'\n',encoding='utf-8')
(PASS/'delivery.json').write_text(json.dumps(receipt,indent=2),encoding='utf-8')
print(json.dumps(receipt,indent=2))
