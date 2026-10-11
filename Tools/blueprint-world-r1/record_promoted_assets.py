"""Record only donor families actually used by the authored runtime world."""
import ast,hashlib,json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
PASS=ROOT/'docs/history/implementation-passes/chapter01-blueprint-world-r1'
tree=ast.parse((Path(__file__).parent/'author_world.py').read_text())
used=set()
for node in ast.walk(tree):
    if isinstance(node,ast.Call) and isinstance(node.func,ast.Name) and node.func.id=='donor':
        for value in ast.walk(node.args[0]):
            if isinstance(value,ast.Constant) and isinstance(value.value,str):used.add(value.value)
intake=json.loads((PASS/'asset-intake.json').read_text())
families=[]
for family,url in [
 ('quaternius-modular-scifi-megakit-standard','https://quaternius.com/packs/modularscifimegakit.html'),
 ('molten-maps-scifi','https://moltenmaps.itch.io/molten-maps-scifi-pack')]:
    inputs=[r for r in intake if r['family']==family and Path(r['entry']).suffix.lower()=='.fbx' and Path(r['entry']).stem in used]
    families.append(dict(family=family,officialSource=url,license='CC0-1.0',
        licenseEvidence=f'Assets/_Game/Content/BlueprintWorldR1/Licenses/{family}.txt',
        usedModelFamilies=sorted(Path(r['entry']).stem for r in inputs),sourceEntries=inputs))
def record(file):return dict(path=str(file.relative_to(ROOT)).replace('\\','/'),bytes=file.stat().st_size,sha256=hashlib.sha256(file.read_bytes()).hexdigest())
assets=[record(p) for p in sorted((ROOT/'Assets/_Game/Content/BlueprintWorldR1/Models').glob('*.fbx'))]
source=record(ROOT/'art/blueprint-world-r1/Chapter01_BlueprintWorld_R1.blend')
data=dict(families=families,runtimeSectorAndServiceModels=assets,packedEditableSource=source,
    method='Actual donor calls in author_world.py matched to hashed canonical archive entries. Selected but unused intake models are excluded.',
    customAuthorship='Sector foundations, fitted deck fields, press/ram, storage shells, freight carcasses, crane, curved pressure chambers, flux yokes and cross-sector services are project-authored derivatives.')
(PASS/'promoted-assets.json').write_text(json.dumps(data,indent=2)+'\n')
print('PROMOTED_FAMILIES',[(r['family'],len(r['usedModelFamilies'])) for r in families])
