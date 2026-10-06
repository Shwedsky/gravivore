"""Independent reopened-source and exported-FBX structural validation."""
import bpy, json, math, hashlib
from pathlib import Path
from mathutils import Vector
import numpy as np
ROOT=Path(__file__).resolve().parents[2];DATA=ROOT/'docs/g0-production-v3/data'
source=ROOT/'art/g0-production-v3/G0_Production_V3.blend'
bpy.ops.wm.open_mainfile(filepath=str(source))
rig=bpy.data.objects['G0_V3_RIG'];lods=[bpy.data.objects['G0_LOD'+str(i)] for i in range(3)]
checks={};details={}
def check(key,value,detail=None):
    checks[key]=bool(value)
    if detail is not None: details[key]=detail
check('18_bones',len(rig.data.bones)==18)
check('root_origin',rig.data.bones['ROOT'].head_local.length<1e-6)
check('unit_object_transforms',all(abs(x-1)<1e-6 for o in [rig]+lods for x in o.scale))
check('five_named_actions',all(n in bpy.data.actions for n in ['Idle','Run','Attack','Hit','Death']))
check('strict_lod_reductions',len(lods[0].data.polygons)>len(lods[1].data.polygons)>len(lods[2].data.polygons))
for o in lods:
    me=o.data;me.calc_loop_triangles();uv=me.uv_layers.active
    check(o.name+'_unique_UV0',len(me.uv_layers)==1 and all(-1e-5<=v<=1.00001 for p in uv.data for v in p.uv))
    check(o.name+'_single_material',len(me.materials)==1)
    check(o.name+'_rigid_weights',all(len(v.groups)==1 and abs(v.groups[0].weight-1)<1e-6 and o.vertex_groups[v.groups[0].group].name in rig.data.bones for v in me.vertices))
    check(o.name+'_finite_geometry',all(math.isfinite(c) for v in me.vertices for c in v.co))
    # Exact duplicate triangles are distinct from legitimate hidden overlaps of
    # layered machinery, which are not automatically destructive topology errors.
    faces=[tuple(sorted(tuple(round(c,6) for c in me.vertices[i].co) for i in t.vertices)) for t in me.loop_triangles]
    check(o.name+'_no_duplicate_triangles',len(faces)==len(set(faces)),len(faces)-len(set(faces)))
    degenerate=sum(t.area<1e-10 for t in me.loop_triangles)
    check(o.name+'_no_zero_area_triangles',degenerate==0,degenerate)
    # Strict-interior raster overlap on UV0. Shared edges do not count as overlap.
    coverage=np.zeros((512,512),dtype=np.uint16);area=0
    for t in me.loop_triangles:
        p=np.array([uv.data[i].uv[:] for i in t.loops])*512
        lo=np.maximum(np.floor(p.min(axis=0)).astype(int),0);hi=np.minimum(np.ceil(p.max(axis=0)).astype(int),511)
        x0,y0=lo;x1,y1=hi
        if x1<x0 or y1<y0:continue
        xs,ys=np.meshgrid(np.arange(x0,x1+1)+.5,np.arange(y0,y1+1)+.5)
        a,b,c=p;den=(b[1]-c[1])*(a[0]-c[0])+(c[0]-b[0])*(a[1]-c[1])
        if abs(den)<1e-10:continue
        u=((b[1]-c[1])*(xs-c[0])+(c[0]-b[0])*(ys-c[1]))/den
        v=((c[1]-a[1])*(xs-c[0])+(a[0]-c[0])*(ys-c[1]))/den
        mask=(u>1e-5)&(v>1e-5)&((1-u-v)>1e-5)
        coverage[y0:y1+1,x0:x1+1]+=mask.astype(np.uint16)
        area+=abs(den)/2/(512*512)
    overlaps=int(np.count_nonzero(coverage>1))
    # Decimated UV triangles inherit the atlas; tiny overlaps from edge collapse
    # are recorded separately rather than disguising them as a fresh unwrap.
    check(o.name+'_uv_overlap',overlaps==0,dict(overlap_pixels=overlaps,occupied_pixels=int(np.count_nonzero(coverage)),uv_area=area,resolution=512))

anim={}
o=lods[0]
for name in ['Idle','Run','Attack','Hit','Death']:
    action=bpy.data.actions[name];rig.animation_data.action=action
    frames=range(int(action.frame_range[0]),int(action.frame_range[1])+1);foot=[];root_positions=[]
    for frame in frames:
        bpy.context.scene.frame_set(frame);bpy.context.view_layer.update()
        root_positions.append(list(rig.pose.bones['ROOT'].matrix.translation))
        ev=o.evaluated_get(bpy.context.evaluated_depsgraph_get());me=ev.to_mesh()
        heights={}
        for label in ['L','R']:
            index=o.vertex_groups[label+'_ANKLE'].index
            vertices=[i for i,v in enumerate(o.data.vertices) if v.groups[0].group==index]
            heights[label]=min(me.vertices[i].co.z for i in vertices)
        foot.append(dict(frame=frame,**heights));ev.to_mesh_clear()
    anim[name]=dict(frames=len(foot),min_feet_z=min(min(f['L'],f['R']) for f in foot),feet=foot)
    check(name+'_stationary_root',all(Vector(p).length<1e-6 for p in root_positions))
    check(name+'_ground_clearance',anim[name]['min_feet_z']>-.025,anim[name]['min_feet_z'])
rig.animation_data.action=bpy.data.actions['Idle'];bpy.context.scene.frame_set(1)
report=dict(checks=checks,details=details,animations=anim,source_sha256=hashlib.sha256(source.read_bytes()).hexdigest())
(DATA/'reopened_asset_validation.json').write_text(json.dumps(report,indent=2))

# Independent FBX importer verifies exported skins/takes rather than assuming
# Blender's export success proves the hierarchy is useful to downstream tools.
bpy.ops.wm.read_factory_settings(use_empty=True)
fbx=ROOT/'Assets/_Game/ArtReview/G0ProductionV3/Models/G0_Production_V3.fbx'
bpy.ops.import_scene.fbx(filepath=str(fbx),use_anim=True)
meshes=[o for o in bpy.context.scene.objects if o.type=='MESH'];arms=[o for o in bpy.context.scene.objects if o.type=='ARMATURE']
roundtrip=dict(meshes=[dict(name=o.name,vertices=len(o.data.vertices),polygons=len(o.data.polygons),uv_layers=len(o.data.uv_layers)) for o in meshes],
              rigs=[dict(name=o.name,bones=[b.name for b in o.data.bones]) for o in arms],actions=[a.name for a in bpy.data.actions],fbx_sha256=hashlib.sha256(fbx.read_bytes()).hexdigest())
roundtrip['pass']=len(meshes)==3 and len(arms)==1 and len(arms[0].data.bones)==18 and len(bpy.data.actions)==5
(DATA/'fbx_roundtrip_validation.json').write_text(json.dumps(roundtrip,indent=2))
print('REOPEN_CHECKS',json.dumps(checks));print('FBX_ROUNDTRIP',json.dumps(roundtrip))
if not roundtrip['pass'] or not all(checks.values()):raise RuntimeError('Asset checks require repair; inspect validation JSON')
