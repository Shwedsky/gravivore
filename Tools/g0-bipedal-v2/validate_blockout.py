"""Reopen saved artifact and check topology, provenance, dependencies, rigid binding.

Outputs measured geometry metrics. This validates an art artifact, not locomotion.
"""
import bpy, bmesh, json, math, hashlib
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
FILE=ROOT/'art/visual-production-v2/g0/G0_Bipedal_Blockout_V2.blend'
DATA=ROOT/'docs/visual-production-v2/g0-bipedal-v2/data'
bpy.ops.wm.open_mainfile(filepath=str(FILE))
assert bpy.app.version==(5,2,2)
scene=bpy.context.scene; col=bpy.data.collections['G0_BIPEDAL_V2_EDITABLE']
meshes=[o for o in col.all_objects if o.type=='MESH']
rig=bpy.data.objects['G0_MECHANICAL_RIG_BLOCKOUT']
deps=bpy.context.evaluated_depsgraph_get(); rows=[]; allpts=[]
for o in meshes:
    assert o.data.vertices, o.name
    assert all(math.isfinite(c) for v in o.data.vertices for c in v.co), o.name
    assert all(len(v.groups)==1 and abs(v.groups[0].weight-1)<1e-6 for v in o.data.vertices), o.name
    assert len(o.vertex_groups)==1, o.name
    assert o.vertex_groups[0].name in rig.data.bones, o.name
    assert any(m.type=='ARMATURE' and m.object==rig for m in o.modifiers), o.name
    assert not o.data.shape_keys, o.name
    raw=o.data; raw.calc_loop_triangles()
    ev=o.evaluated_get(deps); me=ev.to_mesh(); me.calc_loop_triangles()
    bm=bmesh.new(); bm.from_mesh(raw)
    row={'name':o.name,'authorship':o['authorship'],'raw_triangles':len(raw.loop_triangles),
         'evaluated_triangles':len(me.loop_triangles),'vertices':len(raw.vertices),
         'uv_layers':[u.name for u in raw.uv_layers],
         'materials':[m.name for m in raw.materials],
         'boundary_edges':sum(e.is_boundary for e in bm.edges),
         'degenerate_faces':sum(f.calc_area()<1e-10 for f in bm.faces)}
    bm.free()
    allpts.extend([o.matrix_world@v.co for v in me.vertices])
    ev.to_mesh_clear(); rows.append(row)
retained=[r for r in rows if r['name'].startswith('RETAINED_')]
custom=[r for r in rows if not r['name'].startswith('RETAINED_')]
assert len(retained)==17 and sum(r['raw_triangles'] for r in retained)==1499
assert not list(bpy.data.actions), 'Unexpected donor animation in blockout'
assert bpy.data.collections['CATFISH_REFERENCE_CC_BY_4_0'].hide_render
assert 'CC BY 4.0' in bpy.data.texts['READ_ME_G0_V2_AND_ATTRIBUTION'].as_string()
used_images={n.image for m in bpy.data.materials if m.node_tree for n in m.node_tree.nodes if n.type=='TEX_IMAGE' and n.image}
assert all(i.packed_file for i in used_images), 'External image dependency'
assert all(max(i.size)<=1024 for i in used_images), 'Reference texture not downsampled'

# One actual pose smoke check: a knee rotation must move rigid leg vertices, then reset.
shin=bpy.data.objects['L shin blade']; pbone=rig.pose.bones['L_KNEE']
before=shin.evaluated_get(deps).to_mesh(); initial=[v.co.copy() for v in before.vertices]
shin.evaluated_get(deps).to_mesh_clear()
pbone.rotation_mode='XYZ'; pbone.rotation_euler.x=math.radians(10)
bpy.context.view_layer.update(); deps=bpy.context.evaluated_depsgraph_get()
after=shin.evaluated_get(deps).to_mesh()
moved=max((v.co-initial[i]).length for i,v in enumerate(after.vertices))
shin.evaluated_get(deps).to_mesh_clear(); pbone.rotation_euler.x=0; bpy.context.view_layer.update()
assert moved>.01, moved
mins=[min(p[i] for p in allpts) for i in range(3)]; maxs=[max(p[i] for p in allpts) for i in range(3)]
report={'status':'PASS','blender':bpy.app.version_string,'blend_sha256':hashlib.sha256(FILE.read_bytes()).hexdigest(),
        'blend_bytes':FILE.stat().st_size,'hero_mesh_objects':len(meshes),'hero_total_objects':len(col.all_objects),
        'hero_raw_triangles':sum(r['raw_triangles'] for r in rows),
        'hero_evaluated_triangles':sum(r['evaluated_triangles'] for r in rows),
        'donor_mesh_objects':len(retained),'donor_triangles':1499,'custom_mesh_objects':len(custom),
        'custom_raw_triangles':sum(r['raw_triangles'] for r in custom),
        'custom_evaluated_triangles':sum(r['evaluated_triangles'] for r in custom),
        'hero_materials':sorted({m for r in rows for m in r['materials']}),
        'bones':len(rig.data.bones),'bone_names':[b.name for b in rig.data.bones],
        'world_bounds':{'min':mins,'max':maxs},'hero_height_m':maxs[2]-mins[2],
        'boundary_edges':sum(r['boundary_edges'] for r in rows),
        'degenerate_faces':sum(r['degenerate_faces'] for r in rows),
        'rig_pose_smoke_max_vertex_displacement_m':moved,'packed_reference_images':len(used_images),
        'reference_meshes':len([o for o in bpy.data.collections['CATFISH_REFERENCE_CC_BY_4_0'].objects if o.type=='MESH']),
        'whole_scene_objects':len(scene.objects),'checks':['Saved blend reopened','17 retained components / 1499 triangles',
        'Finite geometry','Exactly one rigid bone weight per vertex','Knee pose moves geometry','No donor shape keys/actions',
        'Attribution embedded','Reference hidden by default','All used images packed, max 1024'], 'objects':rows}
(DATA/'blockout_validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print('G0_VALIDATION_PASS',json.dumps({k:v for k,v in report.items() if k not in ['objects','bone_names','checks']}))
