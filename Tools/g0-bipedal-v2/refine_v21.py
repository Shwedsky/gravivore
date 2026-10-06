"""Revise the saved V2 source, normalize authoring transforms and export review-only FBX.

Blender 5.2.2 LTS --background --python Tools/g0-bipedal-v2/refine_v21.py
No production prefab, final rig, UV, texture or animation work.
"""
import bpy, bmesh, json, sys, math, hashlib
from pathlib import Path
from mathutils import Vector, Matrix
ROOT = Path(__file__).resolve().parents[2]
DATA = ROOT/'docs/visual-production-v2/g0-bipedal-v2/data-v21'
DATA.mkdir(parents=True, exist_ok=True)
SOURCE = ROOT/'art/visual-production-v2/g0/G0_Bipedal_Blockout_V2.blend'
OUTPUT = SOURCE.with_name('G0_Bipedal_Blockout_V21.blend')
bpy.ops.wm.open_mainfile(filepath=str(SOURCE))
hero = bpy.data.collections['G0_BIPEDAL_V2_EDITABLE']
meshes = [o for o in hero.all_objects if o.type == 'MESH']
root = bpy.data.objects['G0_ROOT_METERS_Z_UP_FORWARD_MINUS_Y']
rig = bpy.data.objects['G0_MECHANICAL_RIG_BLOCKOUT']
bpy.context.view_layer.update()
def bounds(objects):
    deps = bpy.context.evaluated_depsgraph_get(); pts=[]; triangles=0
    for o in objects:
        ev=o.evaluated_get(deps); me=ev.to_mesh(); me.calc_loop_triangles()
        pts += [o.matrix_world@v.co for v in me.vertices]
        triangles += len(me.loop_triangles); ev.to_mesh_clear()
    lo=[min(p[i] for p in pts) for i in range(3)]
    hi=[max(p[i] for p in pts) for i in range(3)]
    return {'min':lo,'max':hi,'dimensions':[hi[i]-lo[i] for i in range(3)],'triangles':triangles,'mesh_objects':len(objects)}
before = bounds(meshes)
HIP_HEIGHT=2.075
def shorten(p):
    p=p.copy(); p.z = p.z*.875 if p.z<=HIP_HEIGHT else p.z-HIP_HEIGHT*.125
    return p
def revise(p, name):
    p=p.copy()
    if name.startswith(('RETAINED_PELVIS','Hip swept','Pelvis ventral')): p.x*=1.12
    if name.startswith(('L ','R ','RETAINED_L_','RETAINED_R_')):
        s=1 if name.startswith(('L ','RETAINED_L_')) else -1
        if 'thigh' in name or 'hip' in name:
            p.x=s*.35+(p.x-s*.35)*1.20
            p.y=-.08+(p.y+.08)*1.16
        elif 'shin' in name:
            p.x=s*.40+(p.x-s*.40)*1.20
            p.y=-.08+(p.y+.08)*1.18
        elif 'hock cover' in name:
            p.x=s*.42+(p.x-s*.42)*1.12
            p.y=.08+(p.y-.08)*1.12
        elif 'toe' in name or 'foot' in name:
            p.x=s*.43+(p.x-s*.43)*1.08
            p.y=-.08+(p.y+.08)*1.10
        # Move only lower-body mechanisms; shoulders and forearms are preserved.
        if any(t in name for t in ['thigh','hip','knee','shin','hock','ankle','foot','toe']) or name.startswith('RETAINED_'):
            p.x+=s*.02
    if name.startswith('Sensor'):
        p.x*=1.12; p.y=.04+(p.y-.04)*1.10; p.z=3.445+(p.z-3.445)*1.08
    return shorten(p)

# Capture base vertices in world space, then rebuild normalized local transforms.
world={o.name:[revise(o.matrix_world@v.co,o.name) for v in o.data.vertices] for o in meshes}
positions={o.name:shorten(o.matrix_world.translation) for o in hero.all_objects}
parents={o.name:o.parent for o in hero.all_objects}
for o in meshes:
    for m in list(o.modifiers):
        if m.type=='ARMATURE': o.modifiers.remove(m)
for o in hero.all_objects: o.parent=None
root.location=(0,0,0); root.rotation_euler=(0,0,0); root.scale=(1,1,1)
for o in hero.all_objects:
    if o==root: continue
    o.matrix_world=Matrix.Translation(positions[o.name] if o.type=='EMPTY' else Vector((0,0,0)))
    o.rotation_euler=(0,0,0); o.scale=(1,1,1)
for o in hero.all_objects:
    if o==root: continue
    mw=o.matrix_world.copy(); o.parent=parents[o.name]; o.matrix_parent_inverse=Matrix.Identity(4); o.matrix_world=mw
bpy.context.view_layer.update()
for o in meshes:
    inv=o.matrix_world.inverted()
    for v,p in zip(o.data.vertices,world[o.name]): v.co=inv@p
    bm=bmesh.new(); bm.from_mesh(o.data); bmesh.ops.recalc_face_normals(bm,faces=bm.faces); bm.to_mesh(o.data); bm.free()
    o['revision']='V2.1 proportion review; base topology retained'

# Existing blockout skeleton follows revised rest joints; no new bones/animation.
bpy.context.view_layer.objects.active=rig; rig.select_set(True)
bpy.ops.object.mode_set(mode='EDIT')
for b in rig.data.edit_bones:
    b.head=shorten(b.head+Vector((0,0,-.065)))
    b.tail=shorten(b.tail+Vector((0,0,-.065)))
bpy.ops.object.mode_set(mode='OBJECT'); rig.select_set(False)
for o in meshes:
    m=o.modifiers.new('Existing rigid blockout binding','ARMATURE'); m.object=rig

# Purposeful combat feet: split toe retained, heel stabilizer, instep bridge and sole.
col=bpy.data.collections['CUSTOM_STRUCTURE_AND_TOOLS']
dark=bpy.data.materials['G0 | graphite mechanism']; armor=bpy.data.materials['G0 | titanium shell']
steel=bpy.data.materials['G0 | machined actuator']
def foot_piece(name,s,rings,mat):
    vs=[]
    for z,y,w,d in rings:
        for px,py in [(-.7,-1),(.7,-1),(1,-.6),(1,.6),(.7,1),(-.7,1),(-1,.6),(-1,-.6)]:
            vs.append((s*.45+px*w/2,y+py*d/2,z))
    fs=[tuple(range(7,-1,-1)),tuple(range((len(rings)-1)*8,len(rings)*8))]
    for j in range(len(rings)-1):
        for i in range(8): fs.append((j*8+i,j*8+(i+1)%8,(j+1)*8+(i+1)%8,(j+1)*8+i))
    me=bpy.data.meshes.new(name); me.from_pydata(vs,[],fs); me.update()
    bm=bmesh.new(); bm.from_mesh(me); bmesh.ops.recalc_face_normals(bm,faces=bm.faces); bm.to_mesh(me); bm.free()
    o=bpy.data.objects.new(name,me); col.objects.link(o); o.parent=root
    me.materials.append(mat); o['authorship']='GRAVIVORE original V2.1 combat-foot blockout'
    b=o.modifiers.new('Editable edge chamfer','BEVEL'); b.width=.006; b.segments=2
    o.modifiers.new('Weighted panel normals','WEIGHTED_NORMAL')
    bone=('L' if s>0 else 'R')+'_ANKLE'
    g=o.vertex_groups.new(name=bone); g.add(list(range(len(me.vertices))),1,'REPLACE')
    a=o.modifiers.new('Existing rigid blockout binding','ARMATURE'); a.object=rig
    meshes.append(o)
for s,label in [(-1,'R'),(1,'L')]:
    foot_piece(label+' V21 heel stabilizer',s,[(.0,.11,.25,.23),(.065,.11,.28,.26),(.12,.075,.22,.19)],dark)
    foot_piece(label+' V21 instep bridge',s,[(.08,-.06,.27,.29),(.18,-.04,.21,.23),(.23,.00,.15,.16)],armor)
    foot_piece(label+' V21 forward sole brace',s,[(.012,-.26,.30,.33),(.045,-.26,.32,.36),(.075,-.25,.29,.30)],steel)
    foot_piece(label+' V21 heel counterplate',s,[(.07,.15,.19,.12),(.16,.11,.19,.13),(.195,.055,.15,.11)],armor)

# Correct evaluated ground contact exactly; never bake Unity presentation scale.
bpy.context.view_layer.update(); ground=bounds(meshes)['min'][2]
for o in meshes:
    delta=o.matrix_world.inverted().to_3x3()@Vector((0,0,-ground))
    for v in o.data.vertices: v.co+=delta
hero.name='G0_BIPEDAL_V21_EDITABLE'; bpy.context.scene.name='G0_BIPEDAL_V21_PROPORTION_REVIEW'
bpy.context.scene['review_gate']='V2.1 proportions and Unity visual scale only; STOP for human decision'
note=bpy.data.texts.new('READ_ME_G0_V21')
note.write('Derived from saved V2. Z up, -Y forward, meters (unit scale 1). Root origin, positive unit object scales, zero object rotations.\nLeg rest-height compression 12.5%; pelvis +12% width, thigh +20% width/+16% depth, shin +20% width/+18% depth, sensor +12% width/+10% depth/+8% height. Existing 16-bone blockout only, no final rigging.\nReview FBX is static evaluated geometry, exported at authoring size. Unity nominal presentation fit is separate. CC BY 4.0 donor attribution retained in READ_ME_G0_V2_AND_ATTRIBUTION.\n')
bpy.ops.wm.save_as_mainfile(filepath=str(OUTPUT),compress=True)
after=bounds(meshes)
assert abs(after['min'][2])<1e-5
assert all(abs(v-1)<1e-6 for o in hero.all_objects for v in o.scale)
assert all(abs(v)<1e-6 for o in hero.all_objects for v in o.rotation_euler)
report={'status':'PASS','source':str(SOURCE),'source_sha256':hashlib.sha256(SOURCE.read_bytes()).hexdigest(),'output':str(OUTPUT),
 'before':before,'after':after,'total_hero_objects':len(hero.all_objects),'bones':len(rig.data.bones),
 'hip_height_before':HIP_HEIGHT,'hip_height_after':HIP_HEIGHT*.875,'leg_length_change_percent':-12.5,
 'ground_correction':ground,'root_position':list(root.location),'root_scale':list(root.scale),
 'forward':'Blender -Y; imported Unity -Z (verified chest-center probe)','up':'Blender Z; Unity Y','unit_scale':1,
 'source_not_scaled_for_unity':True,'zero_rotations_unit_positive_scales':True}
(DATA/'proportion_metrics.json').write_text(json.dumps(report,indent=2),encoding='utf-8')

# Static export duplicate: evaluated bevels, no armature/animation/cameras/reference.
deps=bpy.context.evaluated_depsgraph_get(); export=[]
for o in meshes:
    me=bpy.data.meshes.new_from_object(o.evaluated_get(deps),depsgraph=deps)
    me.transform(o.matrix_world)
    e=bpy.data.objects.new(o.name,me); bpy.context.scene.collection.objects.link(e); export.append(e)
bpy.ops.object.select_all(action='DESELECT')
for e in export: e.select_set(True)
fbx=ROOT/'Assets/_Game/ArtReview/G0V21/Models/G0_Bipedal_V21_Review.fbx'; fbx.parent.mkdir(parents=True,exist_ok=True)
bpy.ops.export_scene.fbx(filepath=str(fbx),use_selection=True,object_types={'MESH'},global_scale=1,
 apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',
 bake_space_transform=True,bake_anim=False,use_mesh_modifiers=True,add_leaf_bones=False)
print('V21_PROPORTIONS_PASS',json.dumps(report))
