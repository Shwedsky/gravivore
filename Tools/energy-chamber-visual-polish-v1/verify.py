"""Check isolation, accepted mesh bytes, captures, tests and optional APK receipt."""
from pathlib import Path
import subprocess,hashlib,json,xml.etree.ElementTree as ET,zipfile
ROOT=Path(__file__).resolve().parents[2]
PASS=ROOT/'docs/history/implementation-passes/energy-chamber-visual-polish-v1'
BASE='5d1c981a4e5796335e5d0e8a2059d28200d45a4e'
def git(*args):return subprocess.check_output(['git',*args],cwd=ROOT)
def sha(data):return hashlib.sha256(data).hexdigest()
preserved=[]
for name in ['Foundation','CentralChamber','Connector','SideNode']:
    path=f'Assets/_Game/ArtProof/EnergyChamber/Models/EC_{name}.fbx'
    accepted=git('show',f'{BASE}:{path}');current=(ROOT/path).read_bytes()
    assert accepted==current,f'Accepted primary geometry changed: {path}'
    preserved.append(dict(path=path,sha256=sha(current)))
for path in ['Assets/_Game/Content/VisualSlice/Prefabs/G0_V3_Live_Tier0.prefab','Assets/_Game/Content/Definitions/S01_CameraFollowSettings.asset','art/energy-chamber-hybrid-proof-v1/EnergyChamber_HybridProof.blend']:
    current=(ROOT/path).read_bytes();accepted=git('show',f'{BASE}:{path}')
    if path.endswith(('.prefab','.asset')):current=current.replace(b'\r\n',b'\n');accepted=accepted.replace(b'\r\n',b'\n')
    assert current==accepted,f'Accepted reference changed: {path}'
changed=git('diff','--name-only',BASE).decode().splitlines()
allowed=('Assets/_Game/ArtProof/EnergyChamber/','Tools/energy-chamber-hybrid-proof-v1/','Tools/energy-chamber-visual-polish-v1/','art/energy-chamber-visual-polish-v1/','docs/history/implementation-passes/energy-chamber-visual-polish-v1/')
outside=[p for p in changed if not p.startswith(allowed)]
assert not outside,f'Changes outside polish scope: {outside}'
captures=[]
for path in sorted((PASS/'screenshots').glob('*.png')):
    from PIL import Image
    with Image.open(path) as im: size=im.size
    captures.append(dict(path=str(path.relative_to(ROOT)),size=size,sha256=sha(path.read_bytes())))
required=['03_gameplay_camera','04_gameplay_G0','02_high_three_quarter','05_emission_disabled','07_close_three_quarter']
assert all((PASS/'screenshots'/f'{n}.png').exists() for n in required)
tests={}
for name in ['EditMode','PlayMode']:
    path=PASS/f'verification/{name}.xml'
    if path.exists():
        node=ET.parse(path).getroot();tests[name]=node.attrib
        assert node.attrib.get('failed','0')=='0',f'{name} tests failed'
apk=ROOT/'Builds/Android/gravivore-energy-chamber-polish-v1-dev.apk'
record=dict(acceptedBaseline=BASE,preservedPrimaryMeshes=preserved,changedTrackedFiles=changed,captures=captures,tests=tests)
if apk.exists():record['apk']=dict(path=str(apk),bytes=apk.stat().st_size,sha256=sha(apk.read_bytes()))
receipt=PASS/'verification/android-result.json'
if receipt.exists():record['android']=json.loads(receipt.read_text());assert record['android']['result']=='Succeeded'
if apk.exists():
    with zipfile.ZipFile(apk) as z:
        entries=z.namelist()
        assert 'assets/bin/Data/level0' in entries
        assert 'lib/arm64-v8a/libil2cpp.so' in entries
        assert 'assets/ThirdPartyNotices.txt' in entries and 'assets/EnergyChamberNotices.txt' in entries
        record['apk']['archiveVerified']=['level0','ARM64 IL2CPP','third-party notices','Energy Chamber notices']
    stage=ROOT.parents[1]/'.utmp/ec77'
    scene='Assets/_Game/ArtProof/EnergyChamber/Scenes/EnergyChamber_HybridProof.unity'
    assert (stage/scene).read_bytes()==(ROOT/scene).read_bytes(),'Device scene differs from the reviewed saved scene'
    record['apk']['sourceSceneSha256']=sha((ROOT/scene).read_bytes())
    sdk=Path('C:/Program Files/Unity/Hub/Editor/6000.3.0f1/Editor/Data/PlaybackEngines/AndroidPlayer/SDK')
    aapt=sorted((sdk/'build-tools').glob('*/aapt.exe'))[-1]
    badging=subprocess.check_output([str(aapt),'dump','badging',str(apk)],text=True,encoding='utf-8',errors='replace')
    assert "package: name='com.gravivore.energychamberproof'" in badging
    assert "native-code: 'arm64-v8a'" in badging
    assert "sdkVersion:'26'" in badging
    (PASS/'verification/apk-badging.txt').write_text(badging,encoding='utf-8')
    record['apk']['package']='com.gravivore.energychamberproof'
files=git('diff','--name-only',BASE).decode().splitlines()
for name in ['REPORT.md','FILES_CHANGED.txt']:
    path=f'docs/history/implementation-passes/energy-chamber-visual-polish-v1/{name}'
    if (ROOT/path).exists() and path not in files:files.append(path)
(PASS/'FILES_CHANGED.txt').write_text('\n'.join(sorted(files))+'\n')
(PASS/'verification/preservation-and-delivery.json').write_text(json.dumps(record,indent=2),encoding='utf-8')
print('PRESERVED_BASELINE_AND_SCOPE',len(preserved),'primary models;',len(captures),'captures;',tests.keys())
