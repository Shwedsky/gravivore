"""Keep pressure-lock side architecture clear of the shared camera's player ray."""
import sys,math,json,bpy,bmesh
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
import meshlib as m
ROOT=m.ROOT;SOURCE=m.SOURCE;OUT=m.OUT;PASS=m.PASS
bpy.ops.wm.open_mainfile(filepath=str(SOURCE/'Chapter01Facilities.blend'))
m.MATS['Graphite']=bpy.data.materials['CF_Graphite'];m.MATS['Steel']=bpy.data.materials['CF_Steel']
target='ChapterServiceNetwork'
for obj in list(bpy.data.objects):
    if obj.get('installation')==target and ('rear pressure-lock frame' in obj.name or 'pressure-lock side structural outrigger' in obj.name):bpy.data.objects.remove(obj,do_unlink=True)
m.GROUP='gate_architecture';start=len(m.PARTS)
for y,h in [(44,5),(64,6.5)]:
    for s in [-1,1]:m.box('pressure-lock side structural outrigger',(s*4.85,y+1,h),(1.5,.52,.30),'Graphite',.025)
for obj in m.PARTS[start:]:obj['installation']=target
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'Chapter01Facilities.blend'))
groups={}
for obj in [o for o in bpy.data.objects if o.type=='MESH' and o.get('installation')==target]:
    points=[obj.matrix_world@v.co for v in obj.data.vertices];mx=(min(v.x for v in points)+max(v.x for v in points))/2;my=(min(v.y for v in points)+max(v.y for v in points))/2
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
bpy.ops.export_scene.fbx(filepath=str(OUT/'Models'/f'{target}.fbx'),use_selection=True,object_types={'MESH'},apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',bake_anim=False,add_leaf_bones=False,path_mode='STRIP',mesh_smooth_type='FACE',use_tspace=True)
stats=json.loads((PASS/'verification/geometry.json').read_text(encoding='utf-8'))
stats=[s for s in stats if s['model']!=target]+[dict(model=target,renderers=len(merged),triangles=sum(len(o.data.polygons) for o in merged))]
(PASS/'verification/geometry.json').write_text(json.dumps(stats,indent=2),encoding='utf-8')
print('GATE_SIDE_FRAMES_KEEP_CENTRAL_CAMERA_RAY_CLEAR')
