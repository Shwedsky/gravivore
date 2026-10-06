"""Reopen the delivered source and validate actual geometry/hierarchy, not a mock."""
import bpy
import bmesh
import json
from pathlib import Path
from mathutils import Vector

ROOT=Path(__file__).resolve().parents[2]
path=ROOT/'art/visual-production-v2/g0/G0_Blockout_V1.blend'
bpy.ops.wm.open_mainfile(filepath=str(path),load_ui=False,use_scripts=False)
collection=bpy.data.collections['G0_Blockout_V1_AUTHORED']
root=bpy.data.objects['GV_Player_G0_T0_ROOT']
assert root.location.length<1e-6 and root.rotation_euler.to_matrix().is_identity
assert all(abs(x-1)<1e-6 for x in root.scale)
meshes=[o for o in collection.objects if o.type=='MESH']
triangles=0
topology=[]
for obj in meshes:
    assert obj['authored'].startswith('Original')
    assert not obj.modifiers, obj.name
    assert all(x>0 for x in obj.scale), obj.name
    assert len(obj.data.materials)==1, obj.name
    obj.data.calc_loop_triangles()
    triangles+=len(obj.data.loop_triangles)
    bm=bmesh.new()
    bm.from_mesh(obj.data)
    record=dict(name=obj.name,uv_layers=[u.name for u in obj.data.uv_layers],non_manifold_edges=sum(not e.is_manifold for e in bm.edges),
        zero_area_faces=sum(f.calc_area()<1e-12 for f in bm.faces),loose_vertices=sum(not v.link_edges for v in bm.verts))
    bm.free()
    topology.append(record)
    assert not record['non_manifold_edges'], record
    assert not record['zero_area_faces'], record
    assert not record['loose_vertices'], record
foot_checks=[]
for code in ('FL','FR','BL','BR'):
    foot=bpy.data.objects['GV_Player_G0_T0_FootBridge_'+code]
    points=[foot.matrix_world@Vector(v) for v in foot.bound_box]
    minz=min(p.z for p in points)
    assert 0<minz<.12, (code,minz)
    claws=[bpy.data.objects['GV_Player_G0_T0_GroundClaw_'+code+'_'+str(i)] for i in (0,1)]
    contact=min((o.matrix_world@Vector(v)).z for o in claws for v in o.bound_box)
    assert -.005<contact<.012, (code,contact)
    pivot=bpy.data.objects['PIV_Support_'+code+'_Foot']
    assert pivot.matrix_world.translation.length>.5, 'Foot pivot unexpectedly at origin'
    foot_checks.append(dict(code=code,foot_bridge_min_z=minz,ground_claw_contact_z=contact,pivot=list(pivot.matrix_world.translation)))
for name,expected in [('SCK_Core',(0,-.60,1.19)),('SCK_Attack_Left',(-.75,-1.94,.30)),('SCK_Attack_Right',(.75,-1.94,.30))]:
    assert (bpy.data.objects[name].matrix_world.translation-Vector(expected)).length<1e-5, name
report=json.loads((ROOT/'docs/visual-production-v2/modeling/data/g0_blockout.json').read_text())
assert triangles==report['triangles']
assert len(meshes)==report['mesh_objects']
assert len(collection.objects)==report['total_objects']
assert root['donor_parts_used']==0
assert not [o for o in collection.objects if o.type=='ARMATURE']
assert not bpy.data.actions
assert not [i for i in bpy.data.images if i.source=='FILE']
result=dict(status='passed',blend_reopened=True,triangles=triangles,mesh_objects=len(meshes),
    model_objects=len(collection.objects),scene_objects=len(bpy.context.scene.objects),
    unique_model_materials=len({m.name for o in meshes for m in o.data.materials}),
    closed_manifold_meshes=len(meshes),zero_area_faces=0,donor_parts=0,
    support_contact_checks=foot_checks,topology=topology,
    limits='Static neutral pose only; inter-object intersections, locomotion clearance and animation remain unverified. Cylinder default UVs are incidental; no unified authored unwrap is delivered.')
(ROOT/'docs/visual-production-v2/modeling/data/g0_blend_verification.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
print('PASS: reopened authored blend; all meshes closed/manifold; four support contacts and core/attack pivots verified.')
