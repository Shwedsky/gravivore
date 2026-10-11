"""Verify integration against pinned World R2; no inference from old implementation rules."""
import hashlib, json, subprocess
from pathlib import Path
ROOT = Path(__file__).resolve().parents[2]
BASE = "1f8084daf1b9a42a9bd5115d4d9580833f57867b"
ACTOR = "a3b5c279f48a932127a1d3cebe63069bedc7f6f5"
OUT = ROOT / "docs/history/implementation-passes/chapter01-r2-actors-v2-integration/verification"
def git(*args): return subprocess.check_output(["git", *args], cwd=ROOT)
def normalized(data): return data.replace(b"\r\n", b"\n")
paths=git("ls-tree","-r","--name-only",BASE).decode().splitlines()
protected=[p for p in paths if p.startswith(("Assets/_Game/Runtime/Gameplay/", "Assets/_Game/Runtime/Core/", "Assets/_Game/Runtime/Persistence/",
    "Assets/_Game/Runtime/Platform/", "Assets/_Game/Content/BlueprintWorldR1/", "Assets/_Game/Content/VisualSlice/Prefabs/G0",
    "Assets/_Game/Content/VisualSlice/Models/G0", "Assets/_Game/ArtReview/G0", "art/chapter01-blueprint-world-r2/"))
    or p in ("Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity", "Assets/_Game/Content/Scenes/Bootstrap.unity",
    "Tools/blueprint-world-r1/layout.json")
    or p.startswith(("Assets/_Game/Runtime/Presentation/World/","Assets/_Game/Runtime/Presentation/Map/"))
    or p.startswith("Assets/") and "G0" in Path(p).name
    or (p.startswith("Assets/_Game/Content/Definitions/") and p not in (
    "Assets/_Game/Content/Definitions/S15_VisualCatalog.asset","Assets/_Game/Content/Definitions/Chapter01_VisualIntegration.asset"))]
changed=[]
for p in protected:
    if normalized(git("show",BASE+":"+p)) != normalized((ROOT/p).read_bytes()): changed.append(p)
actors=["Scout","Cutter","Warden","ArcDrone","Carrier","Magnetar","Custodian"]
source=[]
for actor in actors:
    relative="Assets/_Game/Content/ActorProductionV2/Models/"+actor+"_V2.fbx"
    old="Assets/_Game/ArtReview/ActorProductionV2/Models/"+actor+"_V2.fbx"
    original=git("show",ACTOR+":"+old); current=(ROOT/relative).read_bytes()
    source.append(dict(actor=actor,modelSourceIdentical=original==current,sha256=hashlib.sha256(current).hexdigest()))
catalog="Assets/_Game/Content/Definitions/S15_VisualCatalog.asset"
original=normalized(git("show",BASE+":"+catalog)).decode();current=normalized((ROOT/catalog).read_bytes()).decode()
catalog_preserved=(original.split("  _enemies:")[0]==current.split("  _enemies:")[0] and
    original.split("  _landmarks:")[1].rstrip()==current.split("  _landmarks:")[1].split("  _hostilePresentation:")[0].rstrip())
payloads=[]
for p in git("ls-tree","-r","--name-only",ACTOR).decode().splitlines():
    if p.startswith("Assets/_Game/ArtReview/ActorProductionV2/") and (p.endswith(".controller") or p.endswith(".png")):
        destination=p.replace("Assets/_Game/ArtReview/ActorProductionV2/","Assets/_Game/Content/ActorProductionV2/")
        before=git("show",ACTOR+":"+p);after=(ROOT/destination).read_bytes()
        payloads.append(dict(path=destination,sourceIdentical=normalized(before)==normalized(after) if p.endswith(".controller") else before==after))
    elif p.startswith("art/chapter01-actor-production-v2/") and p.endswith(".blend"):
        payloads.append(dict(path=p,sourceIdentical=git("show",ACTOR+":"+p)==(ROOT/p).read_bytes()))
board=ROOT/"docs/visual-blueprints/chapter01-actors/CHAPTER01_ACTOR_VISUAL_TARGETS_V2.png"
report=dict(validated=not changed and catalog_preserved and all(x["modelSourceIdentical"] for x in source) and all(x["sourceIdentical"] for x in payloads),worldSource=BASE,actorSource=ACTOR,
    protectedFileCount=len(protected),changedProtectedFiles=changed,sourceModels=source,
    actorAuthoritySha256=hashlib.sha256(board.read_bytes()).hexdigest(),
    playerAndLandmarkCatalogSectionsPreserved=catalog_preserved,sourceControllersTexturesAndEditableArt=payloads,
    method="LF-normalized tracked source comparisons against pinned World R2. Gameplay, data definitions except the two visual binding catalogs, world meshes/materials/lights/layout, scenes, G-0 and persistence are protected; actor model bytes compared against pinned Actor V2.")
OUT.mkdir(parents=True,exist_ok=True);(OUT/"preservation.json").write_text(json.dumps(report,indent=2)+"\n")
print("PROTECTED",len(protected),"CHANGED",changed,"ACTOR_SOURCE_IDENTICAL",all(x["modelSourceIdentical"] for x in source))
if not report["validated"]: raise SystemExit(1)
