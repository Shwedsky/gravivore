import bpy,bmesh,json
from pathlib import Path
from mathutils import Vector
root=Path(__file__).resolve().parents[2]
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
path=root/'Assets/_Game/Content/VisualReplacementV3/Models/VR3_generator.fbx'
bpy.ops.import_scene.fbx(filepath=str(path))
rows=[]
for o in bpy.context.scene.objects:
    if o.type!='MESH':continue
    bm=bmesh.new();bm.from_mesh(o.data);unvisited=set(bm.verts)
    while unvisited:
        component=set();pending=[next(iter(unvisited))]
        while pending:
            v=pending.pop()
            if v in component:continue
            component.add(v);pending.extend(e.other_vert(v) for e in v.link_edges)
        unvisited-=component
        if len(component)<20:continue
        p=[o.matrix_world@v.co for v in component]
        low=Vector([min(v[i] for v in p) for i in range(3)]);high=Vector([max(v[i] for v in p) for i in range(3)])
        rows.append(dict(vertices=len(component),center=list((low+high)/2),size=list(high-low)))
    bm.free()
result=dict(source=str(path.relative_to(root)),parts=sorted(rows,key=lambda r:-r['vertices'])[:20])
(root/'docs/history/implementation-passes/energy-chamber-visual-polish-v1/node-donor-geometry.json').write_text(json.dumps(result,indent=2))
print('GENERATOR_PARTS',json.dumps(result))
