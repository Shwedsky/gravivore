import bpy, json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
rows=[]
for p in sorted((ROOT/'ExternalAssetIntake/Current').glob('*/inspection/*.fbx')):
    bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
    bpy.ops.import_scene.fbx(filepath=str(p))
    meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
    coords=[o.matrix_world@v.co for o in meshes for v in o.data.vertices]
    lo=[min(v[i] for v in coords) for i in range(3)]; hi=[max(v[i] for v in coords) for i in range(3)]
    row=dict(family=p.parents[1].name,name=p.stem,bounds=[lo,hi],triangles=sum(len(f.vertices)-2 for o in meshes for f in o.data.polygons),meshes=[dict(name=o.name,materials=[m.name if m else None for m in o.data.materials]) for o in meshes])
    rows.append(row)
    print('DONOR',p.stem, 'size', [round(hi[i]-lo[i],2) for i in range(3)], 'materials',set(m.name for o in meshes for m in o.data.materials if m))
(ROOT/'docs/history/implementation-passes/chapter01-blueprint-world-r1/donor-measurements.json').write_text(json.dumps(rows,indent=2),encoding='utf-8')
