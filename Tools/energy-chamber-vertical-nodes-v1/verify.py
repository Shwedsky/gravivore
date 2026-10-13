from pathlib import Path
import subprocess,hashlib,json,zipfile,xml.etree.ElementTree as ET
root=Path(__file__).resolve().parents[2]
evidence=root/'docs/history/implementation-passes/energy-chamber-vertical-nodes-v1'
baseline='bd886ef6e96aab172b9b469877f382dfc83406fd'
def git(*args):return subprocess.check_output(['git',*args],cwd=root)
def sha(data):return hashlib.sha256(data).hexdigest()
preserved=[]
for name in ['Foundation','CentralChamber','Connector','SideNode','FoundationPolish','ChamberPolish','ConnectorPolish','NodePolish','NodeVariant']:
    p=f'Assets/_Game/ArtProof/EnergyChamber/Models/EC_{name}.fbx'
    data=(root/p).read_bytes();assert data==git('show',f'{baseline}:{p}'),f'Accepted mesh changed: {p}'
    preserved.append(dict(path=p,sha256=sha(data)))
for p in ['Assets/_Game/Content/VisualSlice/Prefabs/G0_V3_Live_Tier0.prefab','Assets/_Game/Content/Definitions/S01_CameraFollowSettings.asset']:
    assert (root/p).read_bytes().replace(b'\r\n',b'\n')==git('show',f'{baseline}:{p}').replace(b'\r\n',b'\n')
allowed=('Assets/_Game/ArtProof/EnergyChamber/','Tools/energy-chamber-hybrid-proof-v1/','Tools/energy-chamber-vertical-nodes-v1/','art/energy-chamber-vertical-nodes-v1/','docs/history/implementation-passes/energy-chamber-vertical-nodes-v1/')
files=git('diff','--name-only',baseline).decode().splitlines()
assert all(p.startswith(allowed) for p in files),f'Change outside correction scope: {[p for p in files if not p.startswith(allowed)]}'
for name in ['03_gameplay_camera','04_gameplay_G0','02_high_three_quarter','05_emission_disabled','07_close_three_quarter','08_vertical_node_detail']:
    assert (evidence/f'screenshots/{name}.png').exists()
camera=json.loads((evidence/'camera-and-scale.json').read_text())
assert all(abs(camera['installationBounds'][k]-v)<.02 for k,v in [('x',11.48),('y',6.095),('z',11.48)])
tests={}
for name in ['EditMode','PlayMode']:
    p=evidence/f'verification/{name}.xml'
    if p.exists():
        tests[name]=ET.parse(p).getroot().attrib;assert tests[name]['result']=='Passed' and tests[name]['failed']=='0'
record=dict(acceptedBaseline=baseline,preservedMeshes=preserved,files=files,tests=tests,camera=camera)
from PIL import Image
import numpy as np
normal=np.asarray(Image.open(evidence/'screenshots/04_gameplay_G0.png'),dtype=np.int16)
diagnostic=np.asarray(Image.open(evidence/'screenshots/05_emission_disabled.png'),dtype=np.int16)
delta=np.abs(normal-diagnostic)
visibility=dict(changedPixels=int(np.count_nonzero(delta.sum(2))),meanChannelDelta=float(delta.mean()),maxChannelDelta=int(delta.max()),comparison='Same camera/G-0/lighting; shader emission disabled in diagnostic')
assert visibility['changedPixels']>3000,'Emission no longer survives the rendered scene reload'
record['emissionVisibility']=visibility
(evidence/'verification/emission-visibility.json').write_text(json.dumps(visibility,indent=2))
apk=root/'Builds/Android/gravivore-energy-chamber-vertical-nodes-v1-dev.apk'
if apk.exists():
    receipt=json.loads((evidence/'verification/android-result.json').read_text());assert receipt['result']=='Succeeded'
    with zipfile.ZipFile(apk) as z:
        for name in ['assets/bin/Data/level0','lib/arm64-v8a/libil2cpp.so','assets/ThirdPartyNotices.txt','assets/EnergyChamberNotices.txt']:assert name in z.namelist()
    sdk=Path('C:/Program Files/Unity/Hub/Editor/6000.3.0f1/Editor/Data/PlaybackEngines/AndroidPlayer/SDK')
    aapt=sorted((sdk/'build-tools').glob('*/aapt.exe'))[-1]
    badging=subprocess.check_output([str(aapt),'dump','badging',str(apk)],text=True,encoding='utf-8',errors='replace')
    assert "package: name='com.gravivore.energychamberproof'" in badging and "native-code: 'arm64-v8a'" in badging
    (evidence/'verification/apk-badging.txt').write_text(badging,encoding='utf-8')
    scene='Assets/_Game/ArtProof/EnergyChamber/Scenes/EnergyChamber_HybridProof.unity'
    assert (root/scene).read_bytes()==(root.parents[1]/'.utmp/ec77'/scene).read_bytes()
    record['apk']=dict(path=str(apk),bytes=apk.stat().st_size,sha256=sha(apk.read_bytes()),sceneSha256=sha((root/scene).read_bytes()),receipt=receipt)
for name in ['REPORT.md','FILES_CHANGED.txt']:
    p=f'docs/history/implementation-passes/energy-chamber-vertical-nodes-v1/{name}'
    if (root/p).exists() and p not in files:files.append(p)
(evidence/'FILES_CHANGED.txt').write_text('\n'.join(sorted(files))+'\n')
(evidence/'verification/preservation-and-delivery.json').write_text(json.dumps(record,indent=2),encoding='utf-8')
print('VERTICAL_NODE_CORRECTION_VERIFIED',len(preserved),'unchanged accepted meshes;',tests.keys())
