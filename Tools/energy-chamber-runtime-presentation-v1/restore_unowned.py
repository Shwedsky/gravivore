"""Restore Unity-generated tracked changes outside the explicitly owned pass."""
import json,subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
assert ROOT.name=='chapter01-architecture-production-v1'
assert subprocess.check_output(['git','branch','--show-current'],cwd=ROOT).decode().strip()=='integration/chapter01-architecture-production-v1'
prefixes=('Assets/_Game/Content/Chapter01Architecture/EnergyChamber/',
 'Assets/_Game/Editor/VisualIntegration/EnergyChamberRuntimePresentation.cs',
 'Assets/_Game/Tests/EditMode/EnergyChamberRuntimePresentationAssetTests.cs',
 'Assets/_Game/Tests/PlayMode/EnergyChamberRuntimePresentationTests.cs',
 'Tools/energy-chamber-runtime-presentation-v1/',
 'docs/history/implementation-passes/energy-chamber-runtime-presentation-v1/')
exact={'docs/CURRENT_VISUAL_TARGET.md','docs/ASSET_SOURCE_OF_TRUTH.md','art/ENERGY_CHAMBER_SOURCE_CUSTODY.md'}
paths=subprocess.check_output(['git','-c','core.safecrlf=false','diff','--name-only'],cwd=ROOT,stderr=subprocess.DEVNULL).decode().splitlines()
status=subprocess.check_output(['git','status','--porcelain=v1','-z'],cwd=ROOT).decode().split('\0')
paths=sorted(set(paths+[entry[3:] for entry in status if entry[:2] in (' M','MM')]))
restore=[p for p in paths if p not in exact and not p.startswith(prefixes)]
for path in restore:assert (ROOT/path).resolve().is_relative_to(ROOT)
recovery=ROOT/'Builds/Logs/runtime-presentation-generated.patch'
if restore:
    recovery.parent.mkdir(parents=True,exist_ok=True)
    recovery.write_bytes(subprocess.check_output(['git','diff','--binary','--',*restore],cwd=ROOT,stderr=subprocess.DEVNULL))
    subprocess.check_call(['git','restore','--worktree','--',*restore],cwd=ROOT)
out=ROOT/'docs/history/implementation-passes/energy-chamber-runtime-presentation-v1/verification'
(out/'generated-change-restoration.json').write_text(json.dumps(dict(restoredPaths=restore,count=len(restore),recovery=str(recovery.relative_to(ROOT)),originalUserCheckoutModified=False,untrackedFilesRemoved=False),indent=2)+'\n')
print('Restored',len(restore),'generated tracked files outside this pass; local recovery patch retained.')
