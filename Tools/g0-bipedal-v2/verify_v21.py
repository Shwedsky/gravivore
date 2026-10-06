"""Independently reopen V2.1 and check normalization, evaluated export and provenance."""
import bpy, json, hashlib, math, bmesh
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
FILE=ROOT/'art/visual-production-v2/g0/G0_Bipedal_Blockout_V21.blend'
bpy.ops.wm.open_mainfile(filepath=str(FILE))
hero=bpy.data.collections['G0_BIPEDAL_V21_EDITABLE']; root=bpy.data.objects['G0_ROOT_METERS_Z_UP_FORWARD_MINUS_Y']
assert root.location.length<1e-6
assert all(abs(v-1)<1e-6 for o in hero.all_objects for v in o.scale)
assert all(abs(v)<1e-6 for o in hero.all_objects for v in o.rotation_euler)
assert bpy.context.scene.unit_settings.scale_length==1
assert bpy.data.collections['CATFISH_REFERENCE_CC_BY_4_0'].hide_render
assert 'CC BY 4.0' in bpy.data.texts['READ_ME_G0_V2_AND_ATTRIBUTION'].as_string()
pts=[]; tris=0; degenerate=0; retained=0
deps=bpy.context.evaluated_depsgraph_get()
for o in hero.all_objects:
    if o.type!='MESH': continue
    assert all(math.isfinite(c) for v in o.data.vertices for c in v.co)
    assert all(len(v.groups)==1 and abs(v.groups[0].weight-1)<1e-6 for v in o.data.vertices)
    bm=bmesh.new();bm.from_mesh(o.data);degenerate+=sum(f.calc_area()<1e-10 for f in bm.faces);bm.free()
    ev=o.evaluated_get(deps);me=ev.to_mesh();me.calc_loop_triangles();tris+=len(me.loop_triangles)
    pts.extend(o.matrix_world@v.co for v in me.vertices);ev.to_mesh_clear()
    if o.name.startswith('RETAINED_'):retained+=1
assert abs(min(p.z for p in pts))<1e-5
assert degenerate==0
assert retained==17
report={'status':'PASS','saved_file_reopened':True,'root_origin':True,'unit_positive_scales':True,'zero_object_rotations':True,
 'feet_grounded':True,'no_degenerate_faces':True,'retained_donor_objects':retained,'existing_bones':len(bpy.data.objects['G0_MECHANICAL_RIG_BLOCKOUT'].data.bones),
 'triangles':tris,'blend_sha256':hashlib.sha256(FILE.read_bytes()).hexdigest()}
(ROOT/'docs/visual-production-v2/g0-bipedal-v2/data-v21/blend_reopen_validation.json').write_text(json.dumps(report,indent=2))
print('V21_REOPEN_PASS',json.dumps(report))
