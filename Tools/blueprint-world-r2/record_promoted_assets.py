"""Retain exact R1 donor provenance; record the current R2 derivatives."""
import hashlib,json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
PASS=ROOT/'docs/history/implementation-passes/chapter01-blueprint-world-r2'
original=json.loads((ROOT/'docs/history/implementation-passes/chapter01-blueprint-world-r1/promoted-assets.json').read_text())
def record(p):return dict(path=str(p.relative_to(ROOT)),bytes=p.stat().st_size,sha256=hashlib.sha256(p.read_bytes()).hexdigest())
data=dict(families=original['families'],
    runtimeModels=[record(p) for p in sorted((ROOT/'Assets/_Game/Content/BlueprintWorldR1/Models').glob('*.fbx'))],
    surfaceMaps=[record(p) for p in sorted((ROOT/'Assets/_Game/Content/BlueprintWorldR1/Textures').glob('*.png'))],
    editableSource=record(ROOT/'art/blueprint-world-r2/Chapter01_BlueprintWorld_R2.blend'),
    method='Same selectively promoted CC0 donor families and exact canonical archive provenance as R1. R2 refines geometry, native trim colors, controlled PBR atlas, contained energy and camera composition in place; raw sources unchanged.')
(PASS/'promoted-assets.json').write_text(json.dumps(data,indent=2)+'\n');print('R2_PROMOTED',[(f['family'],len(f['usedModelFamilies'])) for f in data['families']])
