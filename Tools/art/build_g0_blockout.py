"""Author G-0 V1 from explicit Blender mesh contours; no Unity or donor geometry.

This is a reproducible hard-surface blockout, not a runtime geometry generator.
Primary shells, limbs and mandibles have designed vertex contours and thickness.
Blender coordinates: Z up, -Y forward. Export axis conversion is deferred.
"""
import bpy
import bmesh
import json
import math
import sys
from pathlib import Path
from mathutils import Vector, Matrix

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT/'art/visual-production-v2/g0/G0_Blockout_V1.blend'
EVIDENCE = ROOT/'docs/visual-production-v2/modeling/g0-evidence'
SOURCE.parent.mkdir(parents=True, exist_ok=True)
EVIDENCE.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.unit_settings.system = 'METRIC'
scene.unit_settings.scale_length = 1
model_collection = bpy.data.collections.new('G0_Blockout_V1_AUTHORED')
scene.collection.children.link(model_collection)
stage_collection = bpy.data.collections.new('REVIEW_STAGE_NOT_EXPORT')
scene.collection.children.link(stage_collection)

def material(name, color, metal, roughness, emission=0):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color,1)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Base Color'].default_value = (*color,1)
    bsdf.inputs['Metallic'].default_value = metal
    bsdf.inputs['Roughness'].default_value = roughness
    if emission:
        bsdf.inputs['Emission Color'].default_value = (*color,1)
        bsdf.inputs['Emission Strength'].default_value = emission
    return mat

ARMOR = material('GV_G0_PaleArmor_BLOCKOUT',(.43,.51,.56),.62,.36)
DARK = material('GV_G0_GraphiteStructure_BLOCKOUT',(.032,.043,.054),.72,.43)
METAL = material('GV_G0_ExposedSteel_BLOCKOUT',(.14,.19,.23),.85,.30)
ENERGY = material('GV_G0_CyanCore_BLOCKOUT',(.002,.28,.80),.0,.27,1.3)
FLOOR = material('REVIEW_Ground',(.055,.066,.078),.25,.64)

def empty(name, location=(0,0,0), parent=None):
    obj = bpy.data.objects.new(name,None)
    model_collection.objects.link(obj)
    obj.location=location
    if parent:
        bpy.context.view_layer.update()
        mw=obj.matrix_world.copy()
        obj.parent=parent
        obj.matrix_world=mw
    obj.empty_display_type='PLAIN_AXES'
    obj.empty_display_size=.12
    return obj

root=empty('GV_Player_G0_T0_ROOT')
root['maturity']='Authored silhouette/proportion blockout only'
root['donor_parts_used']=0
root['forward_axis']='Blender -Y; Unity +Z conversion deferred'

def parent_keep(obj, parent):
    bpy.context.view_layer.update()
    world=obj.matrix_world.copy()
    obj.parent=parent
    obj.matrix_world=world

def mesh(name, vertices, faces, mat, bevel=.015, parent=root):
    data=bpy.data.meshes.new(name+'_Mesh')
    data.from_pydata(vertices,[],faces)
    data.update()
    bm=bmesh.new()
    bm.from_mesh(data)
    bmesh.ops.recalc_face_normals(bm,faces=bm.faces)
    bm.to_mesh(data)
    bm.free()
    obj=bpy.data.objects.new('GV_Player_G0_T0_'+name,data)
    model_collection.objects.link(obj)
    data.materials.append(mat)
    if bevel:
        mod=obj.modifiers.new('Authored edge chamfer','BEVEL')
        mod.width=bevel
        mod.segments=2
        mod.affect='EDGES'
        bpy.context.view_layer.objects.active=obj
        obj.select_set(True)
        bpy.ops.object.modifier_apply(modifier=mod.name)
        obj.select_set(False)
    if parent:
        parent_keep(obj,parent)
    obj['authored']='Original GRAVIVORE Blender blockout mesh'
    return obj

def plate(name, points, thickness, mat=ARMOR, bevel=.025, parent=root):
    """Closed plate with individually positioned top contour, real thickness."""
    n=len(points)
    verts=points+[(x,y,z-thickness) for x,y,z in points]
    faces=[tuple(range(n)),tuple(range(2*n-1,n-1,-1))]
    faces += [(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    return mesh(name,verts,faces,mat,bevel,parent)

def beam(name, start, end, sections, mat, parent=root):
    """Tapered octagonal mechanical/armor segment, explicit longitudinal profile."""
    a,b=Vector(start),Vector(end)
    axis=(b-a).normalized()
    u=axis.cross(Vector((0,0,1)))
    if u.length<.01:
        u=Vector((1,0,0))
    u.normalize()
    v=axis.cross(u).normalized()
    verts=[]
    shape=[(-.72,-1),(.72,-1),(1,-.60),(1,.60),(.72,1),(-.72,1),(-1,.60),(-1,-.60)]
    for t,w,h in sections:
        c=a.lerp(b,t)
        verts += [tuple(c+u*x*w+v*y*h) for x,y in shape]
    faces=[tuple(range(7,-1,-1)),tuple(range((len(sections)-1)*8,len(sections)*8))]
    for j in range(len(sections)-1):
        for i in range(8):
            faces.append((j*8+i,j*8+(i+1)%8,(j+1)*8+(i+1)%8,(j+1)*8+i))
    return mesh(name,verts,faces,mat,.018,parent)

def cylinder(name,a,b,radius,mat,vertices=16,parent=root):
    a,b=Vector(a),Vector(b)
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=radius,depth=(b-a).length,location=(a+b)/2)
    obj=bpy.context.object
    obj.name='GV_Player_G0_T0_'+name
    obj.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler()
    for col in list(obj.users_collection):
        col.objects.unlink(obj)
    model_collection.objects.link(obj)
    obj.data.materials.append(mat)
    bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
    parent_keep(obj,parent)
    obj['authored']='Original low-identity cylindrical joint/actuator'
    return obj

# The internal keel is irregular/tapered; the outer shell never becomes a box hull.
beam('VentralCradle',(0,-.30,.74),(0,.95,.84),[(0,.28,.14),(.16,.58,.19),(.55,.64,.22),(.86,.49,.19),(1,.25,.11)],DARK)
beam('DorsalStructuralSpine',(0,.05,1.18),(0,1.04,1.03),[(0,.21,.12),(.30,.32,.16),(.72,.27,.14),(1,.16,.10)],METAL)

# Real angled cavity: recessed lens, deep annular housing and a separate pale rim.
core_center=Vector((0,-.60,1.19))
normal=Vector((0,-.83,.56)).normalized()
cu=Vector((1,0,0))
cv=normal.cross(cu).normalized()
def annulus(name, profile, mat):
    verts=[]
    segments=40
    for radius,depth in profile:
        c=core_center+normal*depth
        verts += [tuple(c+radius*(cu*math.cos(i*math.tau/segments)+cv*math.sin(i*math.tau/segments))) for i in range(segments)]
    faces=[]
    for j in range(len(profile)):
        k=(j+1)%len(profile)
        faces += [(j*segments+i,j*segments+(i+1)%segments,k*segments+(i+1)%segments,k*segments+i) for i in range(segments)]
    return mesh(name,verts,faces,mat,0)

annulus('CoreCavity_Housing',[(.59,-.19),(.59,.005),(.45,.018),(.43,-.18)],DARK)
annulus('CoreCavity_ArmorRim',[(.60,-.045),(.61,.022),(.575,.063),(.50,.068),(.477,.026),(.493,-.055)],ARMOR)
annulus('CoreCavity_InnerBearing',[(.452,-.135),(.451,-.060),(.421,-.057),(.420,-.134)],METAL)
# Close the rear of the cavity so the cyan lens never reads as a rear-mounted lamp.
cap_verts=[]
for depth in (-.187,-.24):
    c=core_center+normal*depth
    cap_verts += [tuple(c+.447*(cu*math.cos(i*math.tau/40)+cv*math.sin(i*math.tau/40))) for i in range(40)]
cap_faces=[tuple(range(40)),tuple(range(79,39,-1))]
cap_faces += [(i,(i+1)%40,(i+1)%40+40,i+40) for i in range(40)]
mesh('CoreCavity_RearContainment',cap_verts,cap_faces,DARK,0)
verts=[]
segments=40
for radius,depth in [(0.02,.004),(.18,-.01),(.32,-.05),(.402,-.105)]:
    c=core_center+normal*depth
    verts += [tuple(c+radius*(cu*math.cos(i*math.tau/segments)+cv*math.sin(i*math.tau/segments))) for i in range(segments)]
faces=[tuple(range(segments-1,-1,-1))]
for j in range(3):
    faces += [(j*segments+i,j*segments+(i+1)%segments,(j+1)*segments+(i+1)%segments,(j+1)*segments+i) for i in range(segments)]
faces.append(tuple(range(120,160)))
# Close the rear at a recessed cap; the exposed lens is actual convex geometry.
mesh('GravityCore_Lens',verts,faces,ENERGY,0)

# Two-layer shell panels follow the core and open outward into mechanical roots.
for sign,label in [(-1,'L'),(1,'R')]:
    def mirrored(points):
        return [(sign*x,y,z) for x,y,z in points]
    plate('FlankUnderplate_'+label,mirrored([(.30,-.10,1.34),(.62,-.42,1.34),(.95,-.12,1.18),(.94,.45,1.17),(.61,.83,1.27),(.35,.48,1.40)]),.10,METAL)
    plate('MantleForward_'+label,mirrored([(.34,-.20,1.57),(.58,-.50,1.46),(.88,-.25,1.40),(1.00,.04,1.27),(.79,.31,1.44),(.50,.20,1.62)]),.12)
    plate('MantleRear_'+label,mirrored([(.34,.22,1.50),(.72,.27,1.42),(.84,.60,1.31),(.49,1.01,1.23),(.26,.82,1.46)]),.105)
    plate('LateralGuard_'+label,mirrored([(.79,-.17,1.20),(1.03,-.02,1.10),(1.05,.35,.95),(.86,.67,1.05),(.72,.28,1.28)]),.08)
    # Rear paired compact heat exchanger, deliberately below the top mantle.
    cylinder('RearAccumulator_'+label,(sign*.36,.62,.95),(sign*.36,1.12,.85),.14,DARK)
    cylinder('RearAccumulatorCap_'+label,(sign*.36,1.10,.85),(sign*.36,1.18,.84),.155,METAL)
    for index,y in enumerate([.65,.77,.89]):
        plate('RearCoolingLamella_'+label+'_'+str(index),mirrored([(.21,y,1.12),(.52,y,1.12),(.50,y+.045,1.18),(.24,y+.045,1.24)]),.035,METAL,.009)

# Four load-bearing chains, with named mechanical axes and real ground contact.
for sign,side in [(-1,'L'),(1,'R')]:
    for front in (True,False):
        code=('F' if front else 'B')+side
        hip=Vector((sign*.80,-.13 if front else .64,1.02))
        knee=Vector((sign*1.36,-.51 if front else 1.03,.82))
        ankle=Vector((sign*1.45,-1.08 if front else 1.42,.19))
        hip_axis=empty('PIV_Support_'+code+'_Root',hip,root)
        knee_axis=empty('PIV_Support_'+code+'_Knee',knee,hip_axis)
        foot_axis=empty('PIV_Support_'+code+'_Foot',ankle,knee_axis)
        cylinder('HipBearing_'+code,hip-Vector((.10,0,0)),hip+Vector((.10,0,0)),.20,METAL,parent=hip_axis)
        cylinder('HipBearingInner_'+code,hip-Vector((.112,0,0)),hip+Vector((.112,0,0)),.107,DARK,parent=hip_axis)
        beam('UpperLink_'+code,hip,knee,[(0,.14,.11),(.22,.15,.12),(.78,.115,.095),(1,.09,.08)],DARK,hip_axis)
        # Designed armor section widens at root and tapers toward exposed knee.
        start=hip.lerp(knee,.14)+Vector((0,0,.10))
        end=hip.lerp(knee,.85)+Vector((0,0,.09))
        beam('UpperShell_'+code,start,end,[(0,.20,.12),(.20,.25,.14),(.65,.20,.115),(1,.115,.075)],ARMOR,hip_axis)
        cylinder('KneeAxle_'+code,knee-Vector((.105,0,0)),knee+Vector((.105,0,0)),.158,METAL,parent=knee_axis)
        cylinder('KneeCap_'+code,knee+Vector((sign*.11,0,0)),knee+Vector((sign*.135,0,0)),.10,DARK,parent=knee_axis)
        beam('LowerLink_'+code,knee,ankle,[(0,.095,.07),(.45,.09,.07),(1,.06,.05)],DARK,knee_axis)
        beam('TibiaGuard_'+code,knee.lerp(ankle,.17)+Vector((sign*.075,0,.03)),knee.lerp(ankle,.87)+Vector((sign*.065,0,.025)),[(0,.13,.075),(.20,.15,.08),(.65,.10,.055),(1,.048,.035)],ARMOR,knee_axis)
        p1=hip.lerp(knee,.55)+Vector((0,.10,.04))
        p2=knee.lerp(ankle,.62)+Vector((0,.10,.05))
        mid=p1.lerp(p2,.57)
        cylinder('PistonSleeve_'+code,p1,mid,.050,METAL,12,knee_axis)
        cylinder('PistonRod_'+code,mid,p2,.029,METAL,12,knee_axis)
        x,y,z=ankle
        plate('FootBridge_'+code,[(x-.12,y+.06,.18),(x+.12,y+.06,.18),(x+.16,y-.10,.12),(x+.085,y-.29,.08),(x-.10,y-.28,.085),(x-.15,y-.08,.13)],.045,DARK,.015,foot_axis)
        for index,dx in enumerate([-.078,.078]):
            beam('GroundClaw_'+code+'_'+str(index),(x+dx,y-.09,.115),(x+dx*1.5,y-.38,.015),[(0,.045,.044),(.65,.041,.026),(1,.015,.012)],METAL,foot_axis)

# Paired attack appendages are separate from the four supporting feet.
for sign,label in [(-1,'L'),(1,'R')]:
    pivot=Vector((sign*.61,-.59,.85))
    end=Vector((sign*.94,-1.10,.64))
    hinge=empty('PIV_Mandible_'+label,pivot,root)
    cylinder('MandibleBearing_'+label,pivot-Vector((.07,0,0)),pivot+Vector((.07,0,0)),.14,METAL,parent=hinge)
    beam('MandibleRoot_'+label,pivot,end,[(0,.10,.085),(.45,.145,.105),(1,.09,.075)],DARK,hinge)
    def m(points):
        return [(sign*x,y,z) for x,y,z in points]
    plate('MandibleArmor_'+label,m([(.68,-.76,.93),(1.00,-.99,.80),(1.06,-1.33,.67),(.95,-1.62,.53),(.72,-1.37,.61),(.77,-1.08,.82)]),.10,ARMOR,.022,hinge)
    plate('MandibleCuttingForm_'+label,m([(.89,-1.27,.65),(1.00,-1.39,.54),(.75,-1.95,.30),(.78,-1.57,.50),(.66,-1.37,.60)]),.055,METAL,.012,hinge)
    # A restrained channel, secondary to the central lens.
    beam('MandiblePowerChannel_'+label,(sign*.86,-1.20,.708),(sign*.80,-1.59,.504),[(0,.025,.016),(1,.014,.011)],ENERGY,hinge)

for name,position in [('SCK_Core',core_center),('SCK_Attack_Left',(-.75,-1.94,.30)),('SCK_Attack_Right',(.75,-1.94,.30)),('SCK_VFX_Hit',(0,0,1.1)),('SCK_UI_Anchor',(0,.10,1.90))]:
    empty(name,position,root)

# Review stage is clearly separated from exportable authored geometry.
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.012))
ground=bpy.context.object
ground.name='REVIEW_Ground_NOT_EXPORT'
for col in list(ground.users_collection):
    col.objects.unlink(ground)
stage_collection.objects.link(ground)
ground.data.materials.append(FLOOR)
world=bpy.data.worlds.new('REVIEW_NeutralWorld')
world.use_nodes=True
world.node_tree.nodes['Background'].inputs[0].default_value=(.11,.14,.18,1)
world.node_tree.nodes['Background'].inputs[1].default_value=.42
scene.world=world

def light(name, location, power, size, color):
    data=bpy.data.lights.new(name,'AREA')
    data.energy=power
    data.shape='DISK'
    data.size=size
    data.color=color
    obj=bpy.data.objects.new(name,data)
    stage_collection.objects.link(obj)
    obj.location=location
    obj.rotation_euler=(Vector((0,0,.8))-obj.location).to_track_quat('-Z','Y').to_euler()

light('REVIEW_Key',(1,-4,6),700,5,(.86,.93,1))
light('REVIEW_Fill',(-4,-1,3),500,4,(.72,.83,1))
light('REVIEW_Rim',(2,4,5),850,3,(1,.85,.69))
camera_data=bpy.data.cameras.new('REVIEW_Camera')
camera=bpy.data.objects.new('REVIEW_Camera',camera_data)
stage_collection.objects.link(camera)
scene.camera=camera
camera_data.type='ORTHO'
scene.render.engine='CYCLES'
scene.cycles.samples=48
scene.cycles.use_denoising=True
scene.render.image_settings.file_format='PNG'
scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX'

def render(name, location, scale=4.8, portrait=False):
    target=Vector((0,-.16,.80))
    camera.location=location
    camera.rotation_euler=(target-camera.location).to_track_quat('-Z','Y').to_euler()
    camera_data.ortho_scale=scale
    scene.render.resolution_x=1080 if portrait else 1200
    scene.render.resolution_y=1440 if portrait else 1000
    scene.render.filepath=str(EVIDENCE/name)
    bpy.ops.render.render(write_still=True)

# Save editable source with fully applied mesh bevels, organized pivots and review stage.
camera.location=(5,-7,4)
camera.rotation_euler=(Vector((0,-.16,.8))-camera.location).to_track_quat('-Z','Y').to_euler()
camera_data.ortho_scale=4.8
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE),compress=True)
records=[]
for obj in model_collection.objects:
    if obj.type=='MESH':
        obj.data.calc_loop_triangles()
        records.append(dict(name=obj.name,triangles=len(obj.data.loop_triangles),
            vertices=len(obj.data.vertices),material=obj.data.materials[0].name,
            parent=obj.parent.name if obj.parent else None))
points=[o.matrix_world@Vector(v) for o in model_collection.objects if o.type=='MESH' for v in o.bound_box]
minimum=[min(p[i] for p in points) for i in range(3)]
maximum=[max(p[i] for p in points) for i in range(3)]
report=dict(source=str(SOURCE.relative_to(ROOT)).replace('\\','/'),
    mesh_objects=len(records),total_objects=len(model_collection.objects),
    triangles=sum(r['triangles'] for r in records),materials=sorted({r['material'] for r in records}),
    donor_parts_used=0,armatures=0,animations=0,uv_status='Deferred for blockout; no final UV/texture handoff',
    bounds_min=minimum,bounds_max=maximum,dimensions=[b-a for a,b in zip(minimum,maximum)],
    source_inspection_gate='72547ed',objects=records,
    camera_note='Blender portrait orthographic preview; 14.8/-11.2 gameplay camera angle (~53 degrees). Facing staged toward camera; no Unity capture.')
(EVIDENCE.parent/'data/g0_blockout.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
mode=sys.argv[sys.argv.index('--')+1] if '--' in sys.argv else 'all'
if mode=='preview':
    render('04_G0_threequarter.png',(5,-7,4))
else:
    render('01_G0_front.png',(0,-8,3.1))
    render('02_G0_side.png',(8,0,2.5))
    render('03_G0_back.png',(0,8,3.2))
    render('04_G0_threequarter.png',(5,-7,4))
    # Source gameplay offset ratio, shown with front staged toward the camera.
    render('05_G0_top_gameplay_angle.png',(0,-11.2,14.8),8.0,True)
    ground.hide_render=True
    scene.render.engine='BLENDER_WORKBENCH'
    shade=scene.display.shading
    shade.light='FLAT'
    shade.color_type='SINGLE'
    shade.single_color=(0,0,0)
    shade.show_shadows=False
    shade.show_cavity=False
    shade.show_specular_highlight=False
    shade.background_type='VIEWPORT'
    shade.background_color=(1,1,1)
    scene.view_settings.view_transform='Standard'
    render('06_G0_black_silhouette.png',(0,-11.2,14.8),4.7)
print('G0_BLOCKOUT',report['triangles'],'tris /',report['mesh_objects'],'mesh objects /',report['total_objects'],'objects',flush=True)
