"""Assemble the executed verification and APK evidence; fail on incomplete gates."""
import hashlib,json,subprocess,xml.etree.ElementTree as ET,zipfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
PASS=ROOT/'docs/history/implementation-passes/chapter01-blueprint-world-r1'
VERIFY=PASS/'verification'
def read(name):return json.loads((VERIFY/name).read_text())
def test(name):
    file=VERIFY/(name+'.xml');r=ET.parse(file).getroot()
    data={k:int(r.attrib[k]) for k in ['total','passed','failed','skipped']}
    if data['failed'] or data['skipped'] or data['total']!=data['passed']:raise RuntimeError('Incomplete test gate: '+name)
    data.update(xml=str(file.relative_to(ROOT)),sha256=hashlib.sha256(file.read_bytes()).hexdigest())
    return data
def play_mode_gate():
    # Preserve the original full-run XML, including its two fixture failures.
    # A focused rerun may supersede only matching cases after fixture-only fixes.
    full=VERIFY/'playmode-full-final.xml';retry=VERIFY/'playmode-fixture-retest.xml'
    root=ET.parse(full).getroot();rerun=ET.parse(retry).getroot()
    cases={c.attrib['fullname']:c.attrib['result'] for c in root.iter('test-case')}
    replaced=[]
    for case in rerun.iter('test-case'):
        name=case.attrib['fullname']
        if name not in cases:raise RuntimeError('Focused test absent from the full suite: '+name)
        if case.attrib['result']!='Passed':raise RuntimeError('Focused fixture rerun failed: '+name)
        cases[name]=case.attrib['result'];replaced.append(name)
    unresolved=[name for name,result in cases.items() if result!='Passed']
    if unresolved or len(replaced)!=2:raise RuntimeError('Unresolved PlayMode gate: '+str(unresolved))
    return dict(totalUniqueCases=len(cases),passedUniqueCases=len(cases),unresolvedCases=unresolved,
        fullRun=dict(total=int(root.attrib['total']),passed=int(root.attrib['passed']),failed=int(root.attrib['failed']),skipped=int(root.attrib['skipped']),xml=str(full.relative_to(ROOT))),
        focusedFixtureRerun=dict(total=int(rerun.attrib['total']),passed=int(rerun.attrib['passed']),cases=replaced,xml=str(retry.relative_to(ROOT))),
        method='Full 142-case run plus focused rerun of two test-fixture corrections. No runtime, asset or layout changes after the full run; original XML results retained.')
world=read('world-audit.json');scope=read('locked-scope.json');soak=read('five-minute-soak.json');owner=read('owner-preservation.json');build=read('build-evidence.json')
if not world['validated'] or not scope['validated'] or not owner['verifiedAfterDelivery']:raise RuntimeError('Incomplete world/scope/owner gate.')
if not build['validated']:raise RuntimeError('Incomplete build/shader gate.')
if not soak['completed'] or soak['seconds']<300 or soak['reachedWaypoints']<14:raise RuntimeError('Incomplete real-time soak.')
apk=ROOT/'Builds/Android/gravivore-dev-0.1.0+48.apk'
metadata=json.loads(apk.with_suffix('.build.json').read_text())
if metadata['versionCode']!=48 or metadata['flavor']!='Dev' or metadata['targetArchitecture']!='ARM64':raise RuntimeError('Wrong build flavor/version/architecture.')
if not metadata['developmentBuild']:raise RuntimeError('Development build flag missing.')
apk_sha=hashlib.sha256(apk.read_bytes()).hexdigest()
delivered=Path(owner['ownerRoot'])/'Builds/Android'/apk.name
if not delivered.is_file() or hashlib.sha256(delivered.read_bytes()).hexdigest()!=apk_sha:raise RuntimeError('Owner delivery is missing or differs from verified APK.')
if build['apkSha256']!=apk_sha:raise RuntimeError('Build evidence belongs to another APK.')
with zipfile.ZipFile(apk) as z:
    libraries=sorted(n for n in z.namelist() if n.startswith('lib/'))
    if 'lib/arm64-v8a/libil2cpp.so' not in libraries:raise RuntimeError('Missing IL2CPP ARM64 runtime.')
    if any(not n.startswith('lib/arm64-v8a/') for n in libraries):raise RuntimeError('Unexpected non-ARM64 native library.')
captures=[]
for file in sorted((PASS/'internal').glob('*.png')):
    cost=json.loads(file.with_name(file.stem+'_cost.json').read_text())
    captures.append(dict(path=str(file.relative_to(ROOT)).replace('\\','/'),sha256=hashlib.sha256(file.read_bytes()).hexdigest(),inventory=cost))
if len(captures)!=11:raise RuntimeError('Missing final sector/map/facility captures.')
data=dict(synchronizedMainSHA=scope['synchronizedMainSHA'],preImplementationMergedBranchSHA=scope['preImplementationMergedBranchSHA'],
    branch='art/chapter01-blueprint-rebuild-v1',draftPR='https://github.com/Shwedsky/gravivore/pull/71',
    productionScene='Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity',
    apk=dict(path=str(apk.relative_to(ROOT)).replace('\\','/'),deliveredPath=str(delivered),bytes=apk.stat().st_size,sha256=apk_sha,
        metadata=metadata,nativeLibraries=libraries),
    tests=dict(editMode=test('editmode-delivery-final'),playMode=play_mode_gate()),buildEvidence=build,
    worldAudit={k:v for k,v in world.items() if k!='dependencies'},protectedFiles=scope['protectedFiles'],
    fiveMinuteSoak=soak,fiveMinuteRealLocomotion=read('five-minute-real-locomotion.json'),ownerOriginalsPreserved=owner['verifiedAfterDelivery'],finalCaptures=captures,
    performanceLimitations='Editor batch-mode realtime observation and frustum inventories are not Android GPU/FPS measurements. Frustum inventories may include occluded, inactive LOD and combined static-mesh geometry. Device 60 FPS and human concept-fidelity acceptance are pending.',
    assumptions='Approved blueprint macro-layout with explicit new collision/encounter coordinates; accepted V47 actors, M0, UI and gameplay source preserved against the merged pre-implementation branch.')
(VERIFY/'delivery.json').write_text(json.dumps(data,indent=2)+'\n')
print('DELIVERY_EVIDENCE',data['tests'],'APK',data['apk']['bytes'],data['apk']['sha256'])
