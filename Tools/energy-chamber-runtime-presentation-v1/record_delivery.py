"""Record executed gates, source freeze, costs and (optionally) the real APK56."""
import argparse,hashlib,json,shutil,subprocess,xml.etree.ElementTree as ET,zipfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'docs/history/implementation-passes/energy-chamber-runtime-presentation-v1'
V=OUT/'verification'
parser=argparse.ArgumentParser();parser.add_argument('--apk',action='store_true');args=parser.parse_args()
def test(name):
    root=ET.parse(V/name).getroot()
    assert root.get('result')=='Passed' and int(root.get('failed'))==0,name
    return dict(file=name,passed=int(root.get('passed')),failed=0,skipped=int(root.get('skipped')),tests=[c.get('fullname')for c in root.iter('test-case')])
def successful_log(name):
    text=(V/name).read_text(errors='replace')
    assert 'Application will terminate with return code 0' in text,name
    return dict(file=name,exitCode=0,sha256=hashlib.sha256((V/name).read_bytes()).hexdigest())
checks=dict(compile=successful_log('FinalCompile.log'),projectValidation=successful_log('FinalProjectValidation.log'),editMode=test('FinalEditMode.xml'),playMode=test('FinalPlayMode.xml'))
assert checks['editMode']['passed']==533 and checks['playMode']['passed']==9
copies={
'docs/history/implementation-passes/chapter01-architecture-production-v1/verification/g0-traversal.json':'final-g0-traversal.json',
'docs/history/implementation-passes/chapter01-architecture-production-v1/verification/g0-locomotion-envelope.json':'final-g0-locomotion-envelope.json',
'docs/history/implementation-passes/chapter01-r2-actors-v2-integration/verification/actor-traversal.txt':'final-actor-traversal.txt',
'docs/history/implementation-passes/chapter01-r2-actors-v2-integration/verification/route-arena-clearance.json':'final-actor-route-arena-clearance.json',
'docs/history/implementation-passes/chapter01-r2-actors-v2-integration/verification/death-respawn.txt':'final-actor-death-respawn.txt',
'docs/history/implementation-passes/chapter01-r2-actors-v2-integration/full-chapter/verification/traversal.txt':'final-chapter-traversal.txt'}
# Copy fresh route receipts only before restoration; existing copied receipts are
# the immutable source for subsequent APK recording.
for source,target in copies.items():
    if not (V/target).exists():shutil.copyfile(ROOT/source,V/target)
inventory=json.loads((V/'lookdev/renderer-inventory.json').read_text())
baseline=json.loads((V/'diagnosis/renderer-inventory.json').read_text())
assert len(inventory['items'])==45
checks['performance']=dict(levels=inventory['levels'],baselineLevels=baseline['levels'],realtimeLightsAdded=0,renderersAdded=0,trianglesAdded=0,materialAssetsAdded=2,materialAssetsChanged=20,newPbrMaps=32,mapSize=512,androidTextureFormat='ASTC 6x6 with mips',newShaders=1,newRuntimePresenters=0,newParticles=0,newTransparentLayers=0,newFullScreenEffects=0,
    perFrameCpuUpdateCalls=0,perFrameCpuManagedAllocationsIntroduced=0,estimatedAstcMapBytesWithMips=sum(((max(1,512>>m)+5)//6)**2*16 for m in range(10))*32,
    risk='Retaining secondary assemblies increases distant triangle/slot cost versus APK55. Local opaque shader adds a bounded analytic spill/indicator and slow sine evaluation per shaded fragment. GPU/device FPS, thermals and driver-specific output require human device review; no Android GPU timing is claimed.')
checks['frozenArchitecture']=json.loads((V/'architecture-freeze-verified.json').read_text())
previous=ROOT/'Builds/Android/gravivore-dev-0.1.0+55.apk'
oldhash=hashlib.sha256(previous.read_bytes()).hexdigest()
assert oldhash=='0b9ee3dc31185f6084bc8c961637e7624375404e013f0dd16c5f6cfa81593ffa'
checks['previousApk55']=dict(path=str(previous),size=previous.stat().st_size,sha256=oldhash,preserved=True)
checks['humanAcceptance']='Pending owner review of the new device candidate; accepted architecture remains frozen.'
if args.apk:
    apk=ROOT/'Builds/Android/gravivore-dev-0.1.0+56.apk'
    metadata=json.loads(apk.with_suffix('.build.json').read_text())
    assert metadata['versionCode']==56 and metadata['version']=='0.1.0' and metadata['targetArchitecture']=='ARM64' and metadata['developmentBuild']
    build=successful_log('Android56.log');buildtext=(V/'Android56.log').read_text(errors='replace')
    assert 'Android Dev build written to Builds/Android/gravivore-dev-0.1.0+56.apk' in buildtext
    markers={m:[]for m in ['GRAVIVORE/EnergyChamberRuntimeLit','EC_Runtime_R1_Trim1','EC_Runtime_R1_Trim2','Energy Chamber / production','Architectural collision','EC_NodeCool','EC_CoreWarm']}
    with zipfile.ZipFile(apk) as archive:
        assert archive.testzip() is None
        libs=[n for n in archive.namelist() if n.startswith('lib/')]
        assert 'lib/arm64-v8a/libil2cpp.so' in libs and 'lib/arm64-v8a/libunity.so' in libs
        assert not [n for n in libs if not n.startswith('lib/arm64-v8a/')]
        for name in archive.namelist():
            if not name.startswith('assets/bin/Data/'):continue
            data=archive.read(name)
            for marker in markers:
                if marker.encode() in data:markers[marker].append(name)
    assert all(markers.values()),markers
    checks['android']=dict(**build,path=str(apk),size=apk.stat().st_size,sha256=hashlib.sha256(apk.read_bytes()).hexdigest(),metadata=metadata,zipIntegrity=True,nativeLibraries=libs,productionPacking=markers)
    sdk=json.loads((ROOT/'docs/history/implementation-passes/chapter01-architecture-production-v1/verification/sdk-preference-restoration.json').read_text())
    assert sdk['before']==sdk['after'],sdk
    (V/'sdk-preference-restoration.json').write_text(json.dumps(sdk,indent=2)+'\n')
    checks['status']='ENERGY CHAMBER FINAL RUNTIME PRESENTATION — READY FOR HUMAN DEVICE REVIEW'
else:checks['status']='Checkpoint C runtime implementation validated; APK56 build pending.'
(V/'delivery-checks.json').write_text(json.dumps(checks,indent=2)+'\n')
print(checks['status'])
if args.apk:print('APK56:',checks['android']['size'],'bytes; SHA256',checks['android']['sha256'])
