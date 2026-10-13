"""Delivery/preservation checks; screenshots still require visual judgement."""
from pathlib import Path
import hashlib,json,subprocess,zipfile,xml.etree.ElementTree as ET
import numpy as np
from PIL import Image

root=Path(__file__).resolve().parents[2]
evidence=root/'docs/history/implementation-passes/energy-chamber-final-polish-v1'
baseline='535c99c1703bff88aaac3cfcd4e8b9833e3507d2'
def git(*args):return subprocess.check_output(['git','-c','core.safecrlf=false',*args],cwd=root)
def sha(data):return hashlib.sha256(data).hexdigest()
def same_source(p):
    assert (root/p).read_bytes().replace(b'\r\n',b'\n')==git('show',f'{baseline}:{p}').replace(b'\r\n',b'\n'),p

preserved=[]
for p in sorted((root/'Assets/_Game/ArtProof/EnergyChamber/Models').glob('*.fbx')):
    path=p.relative_to(root).as_posix()
    data=p.read_bytes();assert data==git('show',f'{baseline}:{path}'),path
    preserved.append(dict(path=path,sha256=sha(data)))
assert len(preserved)==11
for p in ['Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity',
          'Assets/_Game/Content/VisualSlice/Prefabs/G0_V3_Live_Tier0.prefab',
          'Assets/_Game/Content/Definitions/S01_CameraFollowSettings.asset']:
    same_source(p)
for folder in ['hybrid-proof','visual-polish','vertical-nodes']:
    for p in (root/f'art/energy-chamber-{folder}-v1').glob('*.blend'):
        path=p.relative_to(root).as_posix();assert p.read_bytes()==git('show',f'{baseline}:{path}'),path

allowed=('Assets/_Game/ArtProof/EnergyChamber/','Tools/energy-chamber-hybrid-proof-v1/',
         'Tools/energy-chamber-final-polish-v1/','docs/history/implementation-passes/energy-chamber-final-polish-v1/')
files=git('diff','--name-only',baseline).decode().splitlines()
assert all(p.startswith(allowed) for p in files),[p for p in files if not p.startswith(allowed)]
previous=json.loads((root/'docs/history/implementation-passes/energy-chamber-vertical-nodes-v1/component-bounds.json').read_text())
current=json.loads((evidence/'component-bounds.json').read_text())
assert len(previous['items'])==len(current['items'])==45
for a,b in zip(previous['items'],current['items']):
    assert a['name']==b['name']
    for field in ['position','size','center']:
        assert all(abs(a[field][k]-b[field][k])<.0001 for k in ['x','y','z']),(a['name'],field)
camera=json.loads((evidence/'camera-and-scale.json').read_text())
assert all(abs(camera['installationBounds'][k]-v)<.02 for k,v in [('x',11.48),('y',6.095),('z',11.48)])
context=json.loads((evidence/'verification/chapter-context.json').read_text())
assert context['sourceGeometryMaterialsTransformsPreserved'] and context['raisedMeshIntersections']==0
# Unity and Git may switch the unchanged production YAML between LF and CRLF.
# same_source above checks every serialized byte after line-ending normalization.
source_lf=(root/context['source']).read_bytes().replace(b'\r\n',b'\n')
assert context['sourceSha256'] in {sha(source_lf),sha(source_lf.replace(b'\n',b'\r\n'))}
context['sourceLfSha256']=sha(source_lf)

tests={}
for name in ['EditMode','PlayMode']:
    tests[name]=ET.parse(evidence/f'verification/{name}.xml').getroot().attrib
    assert tests[name]['result']=='Passed' and tests[name]['failed']=='0'
for name in ['01_low_technical','02_high_three_quarter','03_gameplay_camera','04_gameplay_G0',
             '05_emission_disabled','07_close_three_quarter','08_vertical_node_detail',
             '09_chapter01_gameplay','10_chapter01_gameplay_G0','11_chapter01_high_three_quarter',
             '12_chapter01_close_three_quarter','13_chapter01_emission_disabled','14_chapter01_emission_disabled_detail']:
    assert (evidence/f'screenshots/{name}.png').exists()
normal=np.asarray(Image.open(evidence/'screenshots/10_chapter01_gameplay_G0.png'),dtype=np.int16)
off=np.asarray(Image.open(evidence/'screenshots/13_chapter01_emission_disabled.png'),dtype=np.int16)
delta=np.abs(normal-off)
emission=dict(changedPixels=int(np.count_nonzero(delta.sum(2))),meanChannelDelta=float(delta.mean()),
              comparison='Same exact player camera/G-0/Chapter lighting/grading; installation emission only disabled')
assert emission['changedPixels']>3000,'Shader emission did not survive reload'
(evidence/'verification/emission-visibility.json').write_text(json.dumps(emission,indent=2))

apk=root/'Builds/Android/gravivore-energy-chamber-final-polish-v1-dev.apk'
receipt=json.loads((evidence/'verification/android-result.json').read_text());assert receipt['result']=='Succeeded'
with zipfile.ZipFile(apk) as z:
    for name in ['lib/arm64-v8a/libil2cpp.so','assets/ThirdPartyNotices.txt','assets/EnergyChamberNotices.txt']:
        assert name in z.namelist()
    level='assets/bin/Data/level0'
    parts=[n for n in z.namelist() if n.startswith(level+'.split')]
    if parts:
        parts.sort(key=lambda n:int(n.split('.split')[-1]))
        assert [int(n.split('.split')[-1]) for n in parts]==list(range(len(parts)))
    else:parts=[level]
    scene_binary=b''.join(z.read(n) for n in parts)
    for label in [b'Energy Chamber / hybrid proof V1',b'G-0 / unchanged current Tier 0 scale reference',b'Chapter 01 Blueprint World R2']:
        assert label in scene_binary,label
    assert z.testzip() is None
sdk=Path('C:/Program Files/Unity/Hub/Editor/6000.3.0f1/Editor/Data/PlaybackEngines/AndroidPlayer/SDK')
aapt=sorted((sdk/'build-tools').glob('*/aapt.exe'))[-1]
badging=subprocess.check_output([str(aapt),'dump','badging',str(apk)],text=True,encoding='utf-8',errors='replace')
assert "package: name='com.gravivore.energychamberproof'" in badging and "native-code: 'arm64-v8a'" in badging
(evidence/'verification/apk-badging.txt').write_text(badging,encoding='utf-8')
scene=context['scene'];assert (root/scene).read_bytes()==(root.parents[1]/'.utmp/ec77'/scene).read_bytes()
record=dict(acceptedBaseline=baseline,preservedMeshes=preserved,assemblyTransformsAndBoundsPreserved=True,
            camera=camera,chapter=context,emission=emission,tests=tests,
            apk=dict(path=str(apk),bytes=apk.stat().st_size,sha256=sha(apk.read_bytes()),sceneSha256=sha((root/scene).read_bytes()),sceneBinaryParts=len(parts),sceneBinarySha256=sha(scene_binary),receipt=receipt),files=files)
(evidence/'FILES_CHANGED.txt').write_text('\n'.join(sorted(files))+'\n')
(evidence/'verification/preservation-and-delivery.json').write_text(json.dumps(record,indent=2),encoding='utf-8')
print('FINAL_PROOF_VERIFIED: 11 unchanged meshes; 45 unchanged renderer bounds/transforms; isolated Chapter context; tests and Android build passed')
