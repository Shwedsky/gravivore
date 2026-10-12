"""Re-export one installation from its unconsolidated chapter source.
The outer boss service decks share the gameplay ground plane at Y=0.
"""
import bpy,bmesh,math,json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/'art/chapter01-complete-concept/Chapter01Facilities.blend'
bpy.ops.wm.open_mainfile(filepath=str(SOURCE))
target='ShieldDump'
parts=[o for o in bpy.data.objects if o.type=='MESH' and o.get('installation')==target]
for o in parts:
    if o.name.startswith('sort_controls /'):
        ys=[(o.matrix_world@v.co).y for v in o.data.vertices]
        if (min(ys)+max(ys))/2>2.8 and (min(ys)+max(ys))/2<6:o.location.y+=5.1
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE))
groups={}
for obj in parts:
    points=[obj.matrix_world@v.co for v in obj.data.vertices]
    mx=(min(v.x for v in points)+max(v.x for v in points))/2;my=(min(v.y for v in points)+max(v.y for v in points))/2
    groups.setdefault((obj.name.split(' / ')[0],math.floor(mx/8),math.floor(my/4)),[]).append(obj)
merged=[]
for (kind,x,y),items in groups.items():
    bpy.ops.object.select_all(action='DESELECT')
    for obj in items:obj.hide_set(False);obj.select_set(True)
    bpy.context.view_layer.objects.active=items[0]
    if len(items)>1:bpy.ops.object.join()
    obj=bpy.context.object;obj.name=f'CH_{target}_{kind}_{x}_{y}';bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    bm=bmesh.new();bm.from_mesh(obj.data);bmesh.ops.triangulate(bm,faces=list(bm.faces));bm.to_mesh(obj.data);bm.free();merged.append(obj)
bpy.ops.object.select_all(action='DESELECT')
for obj in merged:obj.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(ROOT/'Assets/_Game/Content/Chapter01CompleteConcept/Models'/f'{target}.fbx'),use_selection=True,object_types={'MESH'},apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',bake_anim=False,add_leaf_bones=False,path_mode='STRIP',mesh_smooth_type='FACE',use_tspace=True)
print('SHIELD_NORTH_SERVICE_MOVED_OFF_REQUIRED_STRONG_SPOT_ROUTE')
