"""Restore only generated tracked changes outside this task in its isolated checkout."""
import json, subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
assert ROOT.name=='chapter01-architecture-production-v1'
assert subprocess.check_output(['git','branch','--show-current'],cwd=ROOT).decode().strip()=='integration/chapter01-architecture-production-v1'
exact={
'Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity',
'Assets/_Game/Content/BlueprintWorldR1/Chapter01WorldLayout.asset',
'Assets/_Game/Runtime/Presentation/World/Chapter01WorldPresenter.cs',
'Assets/_Game/Runtime/Presentation/World/ChapterVisualEnvironment.cs',
'docs/CURRENT_VISUAL_TARGET.md','docs/ASSET_SOURCE_OF_TRUTH.md',
'docs/CHAPTER01_ARCHITECTURE_LANGUAGE_V1.md','docs/CHAPTER01_ARCHITECTURE_AUDIT_V1.md',
'ThirdPartyNotices.md','art/ENERGY_CHAMBER_SOURCE_CUSTODY.md'}
prefixes=(
'Assets/_Game/Content/Chapter01Architecture',
'Assets/_Game/Editor/VisualIntegration/Chapter01ArchitectureProduction.cs',
'Assets/_Game/Editor/VisualIntegration/ArchitectureAndroidBuild.cs',
'Assets/_Game/Tests/EditMode/Chapter01ArchitectureProductionTests.cs',
'Assets/_Game/Tests/PlayMode/Chapter01ArchitectureProductionSmokeTests.cs',
'Tools/chapter01-architecture-production-v1/',
'docs/history/implementation-passes/chapter01-architecture-production-v1/',
'art/energy-chamber-hybrid-proof-v1/','art/energy-chamber-visual-polish-v1/','art/energy-chamber-vertical-nodes-v1/')
paths=subprocess.check_output(['git','diff','--name-only'],cwd=ROOT).decode().splitlines()
restore=[p for p in paths if p not in exact and not p.startswith(prefixes)]
for p in restore: assert (ROOT/p).resolve().is_relative_to(ROOT)
if restore:
    recovery=ROOT/'Builds/Logs/unrelated-unity-generated.patch';recovery.parent.mkdir(parents=True,exist_ok=True)
    recovery.write_bytes(subprocess.check_output(['git','diff','--binary','--',*restore],cwd=ROOT))
    subprocess.check_call(['git','restore','--worktree','--',*restore],cwd=ROOT)
out=ROOT/'docs/history/implementation-passes/chapter01-architecture-production-v1/verification'
(out/'generated-change-restoration.json').write_text(json.dumps(dict(restoredPaths=restore,count=len(restore),untrackedFilesRemoved=False,originalUserCheckoutModified=False,recoveryPatch='Builds/Logs/unrelated-unity-generated.patch'),indent=2)+'\n')
print('Restored',len(restore),'unrelated generated tracked files in the isolated checkout; recovery patch retained.')
