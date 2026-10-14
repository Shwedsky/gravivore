"""Keep the scene patch additive and preserve baseline serialized object records."""
import json, re, subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
BASE='e1669362fd3566f69939b547f263a3a600e3e1b7'
SCENE='Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity'
def git(*a): return subprocess.check_output(['git',*a],cwd=ROOT)
def docs(text):
    return {m[0]:m[1] for m in re.findall(r'(--- !u!\d+ &\d+[^\n]*\n)(.*?)(?=--- !u!|\Z)',text,re.S)}
base=git('show',BASE+':'+SCENE).decode().replace('\r\n','\n')
current=(ROOT/SCENE).read_text().replace('\r\n','\n')
b,c=docs(base),docs(current)
root=next(k for k in b if k.startswith('--- !u!1660057539 '))
assert set(b)<=set(c), 'Unity deleted existing scene documents'
added=[k for k in c if k not in b]
assert added and len(added)<=6, added
old_roots=re.findall(r'fileID: (\d+)',b[root])
new_roots=re.findall(r'fileID: (\d+)',c[root])
assert all(i in new_roots for i in old_roots)
extra=[i for i in new_roots if i not in old_roots]
assert len(extra)==1,extra
result=base.replace(root+b[root],root+b[root].rstrip()+'\n  - {fileID: '+extra[0]+'}\n')
bindings=[(k,re.search(r'  _architecturalCollisionRoots:\n  - \{fileID: \d+\}\n',c[k])) for k in b if '_fallbackRoot:' in b[k]]
bindings=[(k,m[0]) for k,m in bindings if m]
for k,binding in bindings:
    result=result.replace(k+b[k],k+b[k].rstrip()+'\n'+binding)
result=result.replace(root, ''.join(k+c[k] for k in added)+root,1)
(ROOT/SCENE).write_text(result)
OUT=ROOT/'docs/history/implementation-passes/chapter01-architecture-production-v1/verification'
OUT.mkdir(parents=True,exist_ok=True)
refs=git('for-each-ref','--format=%(refname:short)','refs/remotes/origin').decode().splitlines()
branches=[]
for ref in refs:
    if ref in ('origin','origin/main','origin/integration/chapter01-architecture-production-v1'): continue
    ahead=int(git('rev-list','--count',BASE+'..'+ref))
    if ahead:
        changes=git('diff','--name-only',BASE+'...'+ref,'--',SCENE).decode().splitlines()
        branches.append(dict(branch=ref,head=git('rev-parse',ref).decode().strip(),aheadOfMain=ahead,productionSceneChanged=bool(changes)))
(OUT/'scene-preservation-and-conflicts.json').write_text(json.dumps(dict(base=BASE,existingSceneDocuments=len(b),existingFieldsPreserved=True,addedDocuments=len(added),addedRoots=extra,explicitArchitecturalCollisionBindings=len(bindings),divergentRemoteBranches=branches,gameplayDomainCodeChanged=False,method='Restore baseline serialized fields verbatim; append production instance/root and one external collision-root binding.'),indent=2)+'\n')
print('Preserved',len(b),'baseline scene records; appended',len(added),'new records and one root')
