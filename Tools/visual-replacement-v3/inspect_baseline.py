import bpy,collections
from pathlib import Path
root=Path(__file__).resolve().parents[2]
bpy.ops.wm.read_factory_settings(use_empty=True)
for path in [root/'Assets/_Game/Content/ConceptFidelityV2/Models/Deck_V2_0.fbx',root/'Assets/_Game/Content/VisualReplacementV3/Models/VR3_platformmetal.fbx',next((root/'ExternalAssetIntake/FreeAssetIntakeV1/_work_v2/extracted/p15/content').rglob('Platform_Metal.fbx'))]:
 bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.fbx(filepath=str(path));bpy.context.view_layer.update()
 for o in bpy.context.scene.objects:
  if o.type!='MESH':continue
  coords=[o.matrix_world@v.co for v in o.data.vertices]
  print('BOUNDS',path.name,o.name,tuple(max(v[i] for v in coords)-min(v[i] for v in coords) for i in range(3)),o.matrix_world)
  for layer in o.data.uv_layers:
   print('UV',layer.name,collections.Counter((int(p.uv.x*4),int(p.uv.y*4)) for p in layer.data))
