"""Move the second physical scrap bin out of the energy-to-armor entry lane."""
import bpy,bmesh,math,json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];SOURCE=ROOT/'art/chapter01-complete-concept/Chapter01Facilities.blend'
bpy.ops.wm.open_mainfile(filepath=str(SOURCE))
parts=[o for o in bpy.data.objects if o.type=='MESH' and o.get('installation')=='ShieldDump']
for obj in parts:
    if not obj.name.startswith('sort_controls /'):continue
    ys=[(obj.matrix_world@v.co).y for v in obj.data.vertices];mid=(min(ys)+max(ys))/2
    if -1.2<mid<.3:obj.location.x-=11.3;obj.location.y-=5.5
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE))
groups={}
for obj in parts:
    points=[obj.matrix_world@v.co for v in obj.data.vertices];mx=(min(v.x for v in points)+max(v.x for v in points))/2;my=(min(v.y for v in points)+max(v.y for v in points))/2
    groups.setdefault((obj.name.split(' / ')[0],math.floor(mx/8),math.floor(my/4)),[]).append(obj)
merged=[]
for (kind,x,y),items in groups.items():
    bpy.ops.object.select_all(action='DESELECT')
    for obj in items:obj.hide_set(False);obj.select_set(True)
    bpy.context.view_layer.objects.active=items[0]
    if len(items)>1:bpy.ops.object.join()
    obj=bpy.context.object;obj.name=f'CH_ShieldDump_{kind}_{x}_{y}';bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    bm=bmesh.new();bm.from_mesh(obj.data);bmesh.ops.triangulate(bm,faces=list(bm.faces));bm.to_mesh(obj.data);bm.free();merged.append(obj)
bpy.ops.object.select_all(action='DESELECT')
for obj in merged:obj.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(ROOT/'Assets/_Game/Content/Chapter01CompleteConcept/Models/ShieldDump.fbx'),use_selection=True,object_types={'MESH'},apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',bake_anim=False,add_leaf_bones=False,path_mode='STRIP',mesh_smooth_type='FACE',use_tspace=True)
print('PHYSICAL_BIN_MOVED_OUT_OF_ENTRY_AND_STRONG_SPOT_LANE')
