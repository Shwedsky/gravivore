import bpy, json
from pathlib import Path
root = Path(__file__).resolve().parents[2]
source = root / 'art/chapter01-actor-production-v2/Warden_V2.blend'
bpy.ops.wm.open_mainfile(filepath=str(source), load_ui=False, use_scripts=False)
record = []
for obj in bpy.data.objects:
    if obj.type == 'MESH' and ('LOD' in obj.name or 'shield' in obj.name.lower()):
        uv = obj.data.uv_layers.active
        record.append(dict(name=obj.name, vertices=len(obj.data.vertices), uv_name=uv.name if uv else None,
                           uv_bounds=[min(d.uv[i] for d in uv.data) for i in range(2)] + [max(d.uv[i] for d in uv.data) for i in range(2)] if uv else None,
                           binding=obj.get('binding'), trim=obj.get('trim_region')))
out = root / 'docs/history/implementation-passes/actors-combat-concept-world/verification'
out.mkdir(parents=True, exist_ok=True)
(out / 'source-uv-diagnosis.json').write_text(json.dumps(record, indent=2))
