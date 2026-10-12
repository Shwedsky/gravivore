"""Align added chassis/access parts to the two rotated transport shells."""
import bpy,bmesh,math,json
from pathlib import Path
from mathutils import Matrix,Vector
ROOT=Path(__file__).resolve().parents[2];SOURCE=ROOT/'art/chapter01-complete-concept/Chapter01Facilities.blend'
bpy.ops.wm.open_mainfile(filepath=str(SOURCE))
parts=[o for o in bpy.data.objects if o.type=='MESH' and o.get('installation')=='HaulerGraveyard']
for obj in parts:
    if not obj.get('camera_refinement') or obj.get('wreck_alignment') or not obj.name.startswith('rupture_and_loading_details /'):continue
    points=[obj.matrix_world@v.co for v in obj.data.vertices];mx=(min(v.x for v in points)+max(v.x for v in points))/2
    if mx>=4:continue
    x,y,yaw=(-6.9,3.8,-9) if mx<0 else (1.0,7.1,80)
    transform=Matrix.Translation(Vector((x,y,0)))@Matrix.Rotation(math.radians(yaw),4,'Z')@Matrix.Translation(Vector((-x,-y,0)))
    obj.matrix_world=transform@obj.matrix_world;obj['wreck_alignment']=True
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
    obj=bpy.context.object;obj.name=f'CH_HaulerGraveyard_{kind}_{x}_{y}';bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    bm=bmesh.new();bm.from_mesh(obj.data);bmesh.ops.triangulate(bm,faces=list(bm.faces));bm.to_mesh(obj.data);bm.free();merged.append(obj)
bpy.ops.object.select_all(action='DESELECT')
for obj in merged:obj.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(ROOT/'Assets/_Game/Content/Chapter01CompleteConcept/Models/HaulerGraveyard.fbx'),use_selection=True,object_types={'MESH'},apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',bake_anim=False,add_leaf_bones=False,path_mode='STRIP',mesh_smooth_type='FACE',use_tspace=True)
print('WRECK_DETAILS_FOLLOW_THE_AUTHORED_SHELL_AXES')
