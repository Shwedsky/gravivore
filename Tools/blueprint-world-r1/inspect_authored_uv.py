import bpy,json
from pathlib import Path
root=Path(__file__).resolve().parents[2]
bpy.ops.wm.open_mainfile(filepath=str(root/'art/blueprint-world-r1/Chapter01_BlueprintWorld_R1.blend'))
report=[]
for obj in bpy.context.scene.objects:
    if obj.type!='MESH':continue
    layers=[dict(name=u.name,zero=sum(1 for loop in u.data if loop.uv.length<.0001),loops=len(u.data)) for u in obj.data.uv_layers]
    if len(layers)>1:report.append(dict(mesh=obj.name,layers=layers))
(root/'docs/history/implementation-passes/chapter01-blueprint-world-r1/verification/uv-diagnostic.json').write_text(json.dumps(report,indent=2))
print('MULTI_UV_MESHES',len(report),report[:2])
if report:raise SystemExit(1)
