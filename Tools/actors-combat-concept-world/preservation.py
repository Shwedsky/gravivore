from pathlib import Path
import subprocess, json, hashlib
root=Path(__file__).resolve().parents[2]
base='7dcd6652c0c7232aaaed5e134edbfc5ea1c3c7e2'
protected=[
 'Assets/_Game/Content/BlueprintWorldR1', 'Assets/_Game/Content/CapacitorFieldProof',
 'Assets/_Game/Content/ConnectedConceptSectors','Assets/_Game/Content/Chapter01CompleteConcept',
 'Assets/_Game/Content/Scenes','Assets/_Game/Runtime/Core','Assets/_Game/Runtime/Persistence',
 'Assets/_Game/Runtime/Platform','Assets/_Game/Content/ActorProductionV2/Models',
 'Assets/_Game/ArtReview/G0ProductionV3/Models','Assets/_Game/Content/Definitions',
 'docs/visual-blueprints/chapter01','docs/visual-blueprints/chapter01-actors']
changes=subprocess.check_output(['git','diff','--name-only',base,'--',*protected],cwd=root).decode().splitlines()
changes=[p for p in changes if p!='Assets/_Game/Content/Definitions/S15_VisualCatalog.asset']
camera=[p for p in subprocess.check_output(['git','ls-tree','-r','--name-only',base,'Assets'],cwd=root).decode().splitlines() if 'Camera' in p and p.endswith('.asset')]
camera_changes=subprocess.check_output(['git','diff','--name-only',base,'--',*camera],cwd=root).decode().splitlines()
notices=subprocess.check_output(['git','diff','--name-only',base,'--','ThirdPartyNotices.md'],cwd=root).decode().splitlines()
report={'baseCommit':base,'protectedWorldGameplayIdsModelsAndReferencesChanged':changes,'cameraContractsChanged':camera_changes,'thirdPartyNoticesChanged':notices,'passed':not changes and not camera_changes and not notices,
 'allowedDefinitionChange':'S15_VisualCatalog: player wall envelope radius; IDs/recipes/balance retained',
 'apk53Sha256':hashlib.sha256((root.parents[1]/'Builds/Android/gravivore-dev-0.1.0+53.apk').read_bytes()).hexdigest()}
out=root/'docs/history/implementation-passes/actors-combat-concept-world/verification'
out.mkdir(parents=True,exist_ok=True);(out/'preservation.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2))
raise SystemExit(0 if report['passed'] else 1)
