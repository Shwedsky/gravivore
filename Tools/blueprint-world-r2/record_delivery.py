"""Fail closed on missing executed R2 tests, build, packing or owner delivery."""
import hashlib,json,subprocess,xml.etree.ElementTree as ET,zipfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];PASS=ROOT/'docs/history/implementation-passes/chapter01-blueprint-world-r2';VERIFY=PASS/'verification'
SDK=Path('C:/Program Files/Unity/Hub/Editor/6000.3.0f1/Editor/Data/PlaybackEngines/AndroidPlayer/SDK')
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def read(n):return json.loads((VERIFY/n).read_text())
def suite(n):
    file=VERIFY/(n+'.xml');r=ET.parse(file).getroot();data={k:int(r.attrib[k]) for k in ['total','passed','failed','skipped']}
    if data['failed'] or data['skipped'] or data['passed']!=data['total']:raise RuntimeError('Incomplete suite: '+n)
    return dict(**data,path=str(file.relative_to(ROOT)),sha256=sha(file))
world=read('world-audit.json');preservation=read('preservation.json');owner=read('owner-preservation.json');soak=read('five-minute-soak.json');locomotion=read('five-minute-real-locomotion.json')
if not all([world['validated'],preservation['validated'],owner['verifiedAfterDelivery'],soak['completed'],soak['seconds']>=300,locomotion['seconds']>=300,locomotion['completedStops']==14]):raise RuntimeError('Incomplete preservation/world/soak gate.')
apk=ROOT/'Builds/Android/gravivore-dev-0.1.0+49.apk';meta=json.loads(apk.with_suffix('.build.json').read_text())
if meta['versionCode']!=49 or meta['flavor']!='Dev' or meta['targetArchitecture']!='ARM64' or not meta['developmentBuild']:raise RuntimeError('Wrong APK configuration.')
delivered=Path(owner['ownerRoot'])/'Builds/Android'/apk.name;digest=sha(apk)
if not delivered.exists() or sha(delivered)!=digest:raise RuntimeError('Owner copy differs from verified APK.')
if (VERIFY/'apk-sha256.txt').read_text().strip()!=digest:raise RuntimeError('Packing callback hash mismatch.')
with zipfile.ZipFile(apk) as z:
    libraries=sorted(n for n in z.namelist() if n.startswith('lib/'))
    if 'lib/arm64-v8a/libil2cpp.so' not in libraries or any(not n.startswith('lib/arm64-v8a/') for n in libraries):raise RuntimeError('Native libraries are not ARM64 IL2CPP only.')
badging=subprocess.check_output([str(SDK/'build-tools/36.0.0/aapt2.exe'),'dump','badging',str(apk)],text=True,encoding='utf-8')
if "versionCode='49'" not in badging or "native-code: 'arm64-v8a'" not in badging or 'application-debuggable' not in badging:raise RuntimeError('APK manifest mismatch.')
essential='\n'.join(l for l in badging.splitlines() if l.startswith(('package:','minSdkVersion:','targetSdkVersion:','native-code:','application-debuggable')))
(VERIFY/'apk-manifest.txt').write_text(essential+'\n')
executed={}
builds=[p for p in (ROOT/'Builds/Logs').glob('android-dev-*.log') if 'gravivore-dev-0.1.0+49.apk' in p.read_text(errors='replace')]
if not builds:raise RuntimeError('No executed APK 49 build log.')
build_log=max(builds,key=lambda p:p.stat().st_mtime)
for name,path in [('compile',VERIFY/'compile-final.log'),('validator',VERIFY/'validator-final.log'),('androidBuild',build_log)]:
    body=path.read_text(errors='replace')
    if 'Application will terminate with return code 0' not in body:raise RuntimeError('Unity gate failed: '+name)
    if name=='androidBuild' and 'Build Finished, Result: Success.' not in body:raise RuntimeError('Android build failed.')
    executed[name]=dict(passed=True,exitCode=0,log=str(path.relative_to(ROOT)),sha256=sha(path))
for name in ['compiled_shader_variants.txt','apk-packed-assets.txt']:
    p=VERIFY/name;p.write_text('\n'.join(l.rstrip() for l in p.read_text().splitlines())+'\n')
variants=(VERIFY/'compiled_shader_variants.txt').read_text()
for token in ['Universal Render Pipeline/Lit','GLES3x','Vulkan','_NORMALMAP','_METALLICSPECGLOSSMAP','_OCCLUSIONMAP','_EMISSION','UI/Default']:
    if token not in variants:raise RuntimeError('Compiled shader evidence missing: '+token)
if not read('render_dependency_report.json')['validated']:raise RuntimeError('Render dependency gate failed.')
if read('apk-post-device-combat.json')['apkSha256']!=digest:raise RuntimeError('Combat packing belongs to another APK.')
serialized=(VERIFY/'apk-world-serialized-sectors.txt').read_text().splitlines()
if len(serialized)!=10 or 'Chapter 01 Blueprint World R2' not in serialized:raise RuntimeError('Actual APK does not contain all R2 sector objects.')
captures=[]
for directory,count in [(PASS/'internal',11),(PASS/'internal/no-ui',10)]:
    files=sorted(directory.glob('*.png'))
    if len(files)!=count:raise RuntimeError('Missing final portrait captures: '+str(directory))
    captures.extend(dict(path=str(f.relative_to(ROOT)),sha256=sha(f)) for f in files)
devices=subprocess.check_output([str(SDK/'platform-tools/adb.exe'),'devices'],text=True)
data=dict(branch='art/chapter01-blueprint-rebuild-v1',draftPR='https://github.com/Shwedsky/gravivore/pull/71',
    acceptedR1TechnicalBaseline=preservation['r1AcceptedTechnicalBaseline'],worldPresentationOnly=True,
    apk=dict(path=str(apk.relative_to(ROOT)),deliveredPath=str(delivered),bytes=apk.stat().st_size,sha256=digest,metadata=meta,nativeLibraries=libraries,manifest=essential),
    tests=dict(editMode=suite('editmode-final'),playMode=suite('playmode-final')),executedGates=executed,
    worldAudit={k:v for k,v in world.items() if k!='dependencies'},preservation=preservation,ownerOriginalsPreserved=True,
    finalPortraitCaptures=captures,fiveMinuteSoak=soak,fiveMinuteRealLocomotion=locomotion,adbDevices=[l for l in devices.splitlines()[1:] if l.strip()],
    shaderPackingValidated=True,actualSerializedWorldEntries=serialized,
    performanceLimitations='Editor batch realtime and frustum inventories do not establish Android GPU/FPS or allocation guarantees. Static local lights have no shadows and bounded ranges. Device 60 FPS and owner visual acceptance remain pending.',
    assumptions='VersionCode 49 is the next development candidate. R1 physical topology, positions, collision, minimap and gameplay are frozen; presentation changes retain the real production camera.')
(VERIFY/'delivery.json').write_text(json.dumps(data,indent=2)+'\n')
print('R2_DELIVERY_EVIDENCE',data['tests'],'APK',data['apk']['bytes'],digest)
