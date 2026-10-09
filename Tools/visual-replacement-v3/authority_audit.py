"""Compare gameplay sources/configuration and scene authority against the task baseline."""
import json,re,subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
BASE='9516026da7ff88dd982c43527cab04d4ca8a222b'
def git(*args):
    return subprocess.run(['git',*args],cwd=ROOT,check=True,stdout=subprocess.PIPE).stdout
def normalized(data):
    return data.replace(b'\r\n',b'\n').rstrip()+b'\n'
protected=['Assets/_Game/Runtime/'+name for name in ('Core','Gameplay','Persistence','Platform')]
protected+=['Assets/_Game/Runtime/Presentation/Camera','Assets/_Game/Content/Definitions']
paths=git('ls-tree','-r','--name-only',BASE,'--',*protected).decode().splitlines()
verified=[]
for path in paths:
    before=normalized(git('show',BASE+':'+path))
    after=normalized((ROOT/path).read_bytes())
    if path=='Assets/_Game/Content/Definitions/Chapter01_VisualIntegration.asset':
        after=b'\n'.join(line for line in after.split(b'\n') if not line.startswith(b'  _uiSkin:'))
    assert before==after, 'Gameplay/configuration authority changed: '+path
    verified.append(path)
scene='Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity'
def blocks(data,authority):
    text=normalized(data).decode()
    result={}
    for match in re.finditer(r'^--- !u!(\d+) &(\d+).*\n([\s\S]*?)(?=^--- !u!|\Z)',text,re.M):
        if int(match[1]) in authority:result[(int(match[1]),int(match[2]))]=match[3].rstrip()
    return result
baselineScene=git('show',BASE+':'+scene);currentScene=(ROOT/scene).read_bytes()
# Unity class IDs: camera, rigidbody, colliders, character controller, MonoBehaviours.
before=blocks(baselineScene,{20,54,64,65,114,135,136,143});after=blocks(currentScene,{20,54,64,65,114,135,136,143})
assert before==after, 'Serialized gameplay/camera/collision scene components changed'
originalTransforms=blocks(baselineScene,{4});currentTransforms=blocks(currentScene,{4})
for key,body in originalTransforms.items():
    assert key in currentTransforms, 'Existing transform removed: '+str(key)
    for field in ('m_LocalPosition','m_LocalRotation','m_LocalScale','m_Father'):
        pattern=r'^  '+field+r':.*$'
        assert re.findall(pattern,body,re.M)==re.findall(pattern,currentTransforms[key],re.M), 'Existing transform authority changed: '+str(key)+' '+field
evidence={'validated':True,'baseline':BASE,'unchangedSourceAndConfigurationFiles':len(verified),
          'verifiedPaths':verified,'identicalSerializedAuthorityComponents':len(before),
          'unchangedExistingSceneTransforms':len(originalTransforms),
          'scene':scene,'allowedDefinitionChange':'Injected production UI skin reference only'}
(ROOT/'docs/visual-replacement-v3/verification/authority_audit.json').write_text(json.dumps(evidence,indent=2))
print('GAMEPLAY_CAMERA_COLLISION_BASELINE_PASS: '+str(len(verified))+' files; '+str(len(before))+' scene components')
