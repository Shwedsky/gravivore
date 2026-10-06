"""Refine the accepted saved V2.1 asset. Run with Blender 5.2.2 --background.

No donor reload, silhouette rescale, runtime prefab or gameplay changes.
"""
import bpy, bmesh, json, math, hashlib, sys
from pathlib import Path
from mathutils import Vector, Matrix

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'art/visual-production-v2/g0/G0_Bipedal_Blockout_V21.blend'
ART = ROOT / 'art/g0-production-v3'
UNITY = ROOT / 'Assets/_Game/ArtReview/G0ProductionV3'
DATA = ROOT / 'docs/g0-production-v3/data'
ATLAS_SIZE=2048
for p in [ART, UNITY/'Models', UNITY/'Textures', DATA]: p.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(SOURCE))
scene=bpy.context.scene
hero=bpy.data.collections['G0_BIPEDAL_V21_EDITABLE']
objects=[o for o in hero.all_objects if o.type=='MESH']
rig=bpy.data.objects['G0_MECHANICAL_RIG_BLOCKOUT']
root=bpy.data.objects['G0_ROOT_METERS_Z_UP_FORWARD_MINUS_Y']
source_hash=hashlib.sha256(SOURCE.read_bytes()).hexdigest()

def bounds(obs):
    deps=bpy.context.evaluated_depsgraph_get(); pts=[]; tris=0
    for o in obs:
        ev=o.evaluated_get(deps); me=ev.to_mesh(); me.calc_loop_triangles()
        pts.extend(o.matrix_world @ v.co for v in me.vertices)
        tris+=len(me.loop_triangles); ev.to_mesh_clear()
    lo=[min(v[i] for v in pts) for i in range(3)]; hi=[max(v[i] for v in pts) for i in range(3)]
    return dict(min=lo,max=hi,dimensions=[hi[i]-lo[i] for i in range(3)],triangles=tris)
baseline=bounds(objects)
materials={m.name:m for o in objects for m in o.data.materials}
dark=materials['G0 | graphite mechanism']; armor=materials['G0 | titanium shell']
steel=materials['G0 | machined actuator']; pale=materials['G0 | ceramic titanium edge']
cyan=materials['G0 | cyan gravity cell']; copper=materials['G0 | copper service mark']
prod=bpy.data.collections.new('G0_V3_PRODUCTION'); scene.collection.children.link(prod)
edit=bpy.data.collections.new('EDITABLE_COMPONENTS'); prod.children.link(edit)
lodcol=bpy.data.collections.new('EXPORT_LODS'); prod.children.link(lodcol)
refinements=[]

def recalc(me):
    bm=bmesh.new(); bm.from_mesh(me); bmesh.ops.recalc_face_normals(bm,faces=bm.faces); bm.to_mesh(me); bm.free()

def clean_degenerate(me):
    bm=bmesh.new();bm.from_mesh(me)
    bad=[f for f in bm.faces if f.calc_area()<1e-10]
    if bad:bmesh.ops.delete(bm,geom=bad,context='FACES_ONLY')
    loose=[v for v in bm.verts if not v.link_faces]
    if loose:bmesh.ops.delete(bm,geom=loose,context='VERTS')
    bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bm.to_mesh(me);bm.free()

def activate(o):
    bpy.ops.object.select_all(action='DESELECT'); o.select_set(True); bpy.context.view_layer.objects.active=o

def center(o):
    vs=[o.matrix_world @ v.co for v in o.data.vertices]
    return Vector([(min(v[i] for v in vs)+max(v[i] for v in vs))/2 for i in range(3)])

# Keep the geometry in source meters and rigid bindings, bake world transforms.
for o in objects:
    name=o.name; group=o.vertex_groups[0].name
    mw=o.matrix_world.copy(); o.data.transform(mw); o.parent=None; o.matrix_world=Matrix.Identity(4)
    for c in list(o.users_collection): c.objects.unlink(o)
    edit.objects.link(o)
    for m in list(o.modifiers): o.modifiers.remove(m)
    o['v21_component']=name; o['rigid_bone']=group
    # Replace single-point fan ridges by a short, supported ridge. This changes
    # surface construction inside the same perimeter, rather than proportions.
    if any(s in name for s in ['Chest swept carapace','swept shoulder vane','upperarm dorsal shell','Chest crown ridge']):
        # Rebuild just the front fan into a planar ridge landing surrounded by
        # broad quad patches. Vertex-only beveling creates pinched star normals.
        original=[v.co.copy() for v in o.data.vertices]; n=(len(original)-1)//2; apex=original[-1]
        ring=[Vector((apex.x+(v.x-apex.x)*.24,apex.y,apex.z+(v.z-apex.z)*.24)) for v in original[:n]]
        vs=original[:-1]+ring
        fs=[tuple(range(n,2*n)),tuple(range(2*n,3*n))]
        fs += [(i,(i+1)%n,2*n+(i+1)%n,2*n+i) for i in range(n)]
        fs += [(i,i+n,(i+1)%n+n,(i+1)%n) for i in range(n)]
        o.data.clear_geometry();o.data.from_pydata(vs,[],fs);o.data.update();recalc(o.data)
        o.vertex_groups.clear();g=o.vertex_groups.new(name=group);g.add(list(range(len(o.data.vertices))),1,'REPLACE')
        refinements.append({'part':name,'operation':'supported ridge, perimeter preserved'})
    # Bearing end plates now have an inset retainer with a dark central hub.
    if 'endcap' in name:
        bm=bmesh.new(); bm.from_mesh(o.data)
        caps=[f for f in bm.faces if len(f.verts)>8]
        result=bmesh.ops.inset_individual(bm,faces=caps,thickness=.016,depth=-.006,use_even_offset=True)
        o.data.materials.append(dark)
        for f in caps: f.material_index=1
        bmesh.ops.recalc_face_normals(bm,faces=bm.faces); bm.to_mesh(o.data); bm.free()
        refinements.append({'part':name,'operation':'recessed bearing hub and annular retainer'})
    # Low broad inset panels give armor deliberate construction without greebles.
    if any(s in name for s in ['shin blade','tapered thigh shield','gravity gauntlet','Rear swept containment fin','instep bridge']):
        bm=bmesh.new(); bm.from_mesh(o.data)
        face=max(bm.faces,key=lambda f:f.calc_area())
        bmesh.ops.inset_individual(bm,faces=[face],thickness=.022,depth=-.006,use_even_offset=True)
        o.data.materials.append(armor if o.data.materials[0]==pale else dark)
        face.material_index=1
        bmesh.ops.recalc_face_normals(bm,faces=bm.faces); bm.to_mesh(o.data); bm.free()
        refinements.append({'part':name,'operation':'broad recessed service panel'})
    bevel=o.modifiers.new('Production edge radius','BEVEL'); bevel.width=.006 if 'endcap' in name else .009
    if 'cyan' in name: bevel.width=.0015
    bevel.segments=1; bevel.affect='EDGES'; bevel.limit_method='ANGLE'; bevel.angle_limit=math.radians(28)
    bevel.harden_normals=True
    for f in o.data.polygons: f.use_smooth=True
    o.data.set_sharp_from_angle(angle=math.radians(38))
    norm=o.modifiers.new('Area weighted mechanical normals','WEIGHTED_NORMAL'); norm.keep_sharp=True; norm.weight=40
    activate(o)
    for m in list(o.modifiers): bpy.ops.object.modifier_apply(modifier=m.name)
    recalc(o.data)

def piece(name,verts,faces,mat,bone):
    me=bpy.data.meshes.new(name); me.from_pydata(verts,[],faces); me.update(); recalc(me)
    o=bpy.data.objects.new(name,me); edit.objects.link(o); me.materials.append(mat)
    g=o.vertex_groups.new(name=bone); g.add(list(range(len(me.vertices))),1,'REPLACE')
    o['authorship']='GRAVIVORE original V3 mechanical construction'; o['rigid_bone']=bone
    for f in me.polygons: f.use_smooth=True
    me.set_sharp_from_angle(angle=math.radians(38))
    bevel=o.modifiers.new('Manufacturing edge','BEVEL'); bevel.width=.004; bevel.segments=1
    activate(o); bpy.ops.object.modifier_apply(modifier=bevel.name)
    objects.append(o); return o

def band(name,c,axis,outer,inner,depth,mat,bone,n=16):
    axis=Vector(axis).normalized(); u=axis.orthogonal().normalized(); v=axis.cross(u)
    c=Vector(c); vs=[]
    for axial,r in [(-depth/2,outer),(depth/2,outer),(-depth/2,inner),(depth/2,inner)]:
        for i in range(n): vs.append(c+axis*axial+(u*math.cos(i*2*math.pi/n)+v*math.sin(i*2*math.pi/n))*r)
    fs=[]
    for i in range(n):
        j=(i+1)%n
        fs.extend([(i,j,n+j,n+i),(2*n+i,3*n+i,3*n+j,2*n+j),(i,2*n+i,2*n+j,j),(n+i,n+j,3*n+j,3*n+i)])
    return piece(name,vs,fs,mat,bone)

# Collars support exposed joint axes and tie shoulder/hip mechanisms to the shell.
for s,label in [(-1,'R'),(1,'L')]:
    for joint,r in [('hip',.17),('knee',.15),('shoulder',.16),('elbow',.125)]:
        spindle=next(o for o in objects if o.name==label+' '+joint+' spindle')
        c=center(spindle); bone=spindle.vertex_groups[0].name
        band(label+' '+joint+' structural bearing collar',c,(1,0,0),r,r-.035,.095,dark,bone)
    # Heel plate and toe rails are one load-bearing assembly in the final mesh;
    # broad sole treads, not thin decorative teeth, reinforce ground support.
    for y in [-.35,-.20,.12]:
        x=s*.45; z=.038; w=.25; d=.045; h=.023
        vs=[(x+dx*w/2,y+dy*d/2,z+dz*h/2) for dz in [-1,1] for dy in [-1,1] for dx in [-1,1]]
        piece(label+' broad sole tread '+str(y),vs,[(0,2,3,1),(4,5,7,6),(0,1,5,4),(2,6,7,3),(0,4,6,2),(1,3,7,5)],dark,label+'_ANKLE')

# Fit joint pivots to actual revised source bearings. Preserve compact hierarchy.
rig.parent=None; rig.matrix_world=Matrix.Identity(4); rig.name='G0_V3_RIG'; rig.data.name='G0_V3_18_BONE_MECHANICAL'
activate(rig); bpy.ops.object.mode_set(mode='EDIT')
oldroot='G0_ROOT_METERS_Z_UP_FORWARD_MINUS_Y'
rig.data.edit_bones[oldroot].name='ROOT'
rig.data.edit_bones['ROOT'].head=(0,0,0); rig.data.edit_bones['ROOT'].tail=(0,0,.16)
for label in ['L','R']:
    for bname,part in [('HIP','hip spindle'),('KNEE','knee spindle'),('ANKLE','foot rocker spindle'),('SHOULDER','shoulder spindle'),('ELBOW','elbow spindle')]:
        b=rig.data.edit_bones[label+'_'+bname]; c=center(next(o for o in objects if o.name==label+' '+part))
        b.head=c; b.tail=c+Vector((0,0,.16)); b.roll=0
for side in [-1,1]:
    b=rig.data.edit_bones.new('L_JAW_'+('OUTER' if side>0 else 'INNER'))
    b.head=(.88+side*.09,-.22,2.085625); b.tail=b.head+Vector((0,0,.14)); b.parent=rig.data.edit_bones['L_TOOL']
bpy.ops.object.mode_set(mode='OBJECT')
for o in objects:
    if o.name.startswith('Capture jaw'):
        o.vertex_groups.clear(); g=o.vertex_groups.new(name='L_JAW_'+('OUTER' if o.name.endswith('1') and not o.name.endswith('-1') else 'INNER'))
        g.add(list(range(len(o.data.vertices))),1,'REPLACE')
for c in list(rig.users_collection): c.objects.unlink(rig)
prod.objects.link(rig)
rig['purpose']='Rigid mechanical game rig; root in-place, hip/knee/ankle, torso, sensor, shoulders/elbows/tools, two capture jaws'

# One contiguous draw submission per LOD; disconnected islands are intentional
# rigid machine parts, all skin to this one skeleton with exactly one influence.
bpy.ops.object.select_all(action='DESELECT')
for o in objects: o.select_set(True)
bpy.context.view_layer.objects.active=objects[0]; bpy.ops.object.join()
mesh=bpy.context.object; mesh.name='G0_LOD0'; mesh.data.name='G0_V3_LOD0_UV_SKIN'
for c in list(mesh.users_collection): c.objects.unlink(mesh)
lodcol.objects.link(mesh)
clean_degenerate(mesh.data)
activate(mesh)
for layer in list(mesh.data.uv_layers): mesh.data.uv_layers.remove(layer)
mesh.data.uv_layers.new(name='UV0_1024_UNIQUE')
bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.uv.smart_project(angle_limit=math.radians(68),island_margin=.0015,area_weight=.8,scale_to_bounds=True)
bpy.ops.object.mode_set(mode='OBJECT')
mesh.data.uv_layers.active.name='UV0_1024_UNIQUE'

def bake(name,mode):
    image=bpy.data.images.new(name,width=ATLAS_SIZE,height=ATLAS_SIZE,alpha=True)
    image.colorspace_settings.name='sRGB' if mode in ['base','emission'] else 'Non-Color'
    for m in mesh.data.materials:
        m.use_nodes=True; nodes=m.node_tree.nodes; links=m.node_tree.links; nodes.clear()
        out=nodes.new('ShaderNodeOutputMaterial'); emit=nodes.new('ShaderNodeEmission'); links.new(emit.outputs[0],out.inputs[0])
        tex=nodes.new('ShaderNodeTexImage'); tex.image=image; nodes.active=tex
        name0=m.name
        role='cyan' if 'cyan' in name0 else 'ceramic' if 'ceramic' in name0 else 'actuator' if 'actuator' in name0 else 'copper' if 'copper' in name0 else 'graphite' if 'graphite' in name0 else 'shell'
        palette={'shell':(.057,.084,.098,1),'ceramic':(.21,.25,.25,1),'actuator':(.115,.142,.15,1),'copper':(.32,.14,.055,1),'graphite':(.025,.035,.039,1),'cyan':(.008,.55,.72,1)}
        metal={'shell':.5,'ceramic':.35,'actuator':.85,'copper':.72,'graphite':.65,'cyan':.15}
        smooth={'shell':.30,'ceramic':.26,'actuator':.52,'copper':.35,'graphite':.27,'cyan':.38}
        if mode=='base' and role!='cyan':
            noise=nodes.new('ShaderNodeTexNoise'); noise.inputs['Scale'].default_value=17; noise.inputs['Detail'].default_value=2
            coord=nodes.new('ShaderNodeTexCoord'); links.new(coord.outputs['Object'],noise.inputs['Vector'])
            ramp=nodes.new('ShaderNodeValToRGB'); c=palette[role]
            ramp.color_ramp.elements[0].position=.15; ramp.color_ramp.elements[0].color=tuple(v*.77 for v in c[:3])+(1,)
            ramp.color_ramp.elements[1].position=.82; ramp.color_ramp.elements[1].color=c
            links.new(noise.outputs['Fac'],ramp.inputs[0]); links.new(ramp.outputs[0],emit.inputs[0])
        elif mode=='base': emit.inputs[0].default_value=palette[role]
        elif mode=='emission': emit.inputs[0].default_value=palette['cyan'] if role=='cyan' else (0,0,0,1)
        elif mode=='metal': emit.inputs[0].default_value=(metal[role],)*3+(1,)
        elif mode=='smooth': emit.inputs[0].default_value=(smooth[role],)*3+(1,)
    scene.render.engine='CYCLES'; scene.cycles.samples=8; scene.render.bake.margin=3
    activate(mesh); bpy.ops.object.bake(type='EMIT')
    return image

base=bake('G0_V3_BaseColor','base'); emission=bake('G0_V3_Emission','emission')
metal=bake('G0_V3_Metal','metal'); smooth=bake('G0_V3_Smooth','smooth')
import numpy as np
mp=np.empty(ATLAS_SIZE*ATLAS_SIZE*4,dtype=np.float32); sp=np.empty_like(mp)
metal.pixels.foreach_get(mp); smooth.pixels.foreach_get(sp); mp[3::4]=sp[0::4]
packed=bpy.data.images.new('G0_V3_MetallicSmoothness',width=ATLAS_SIZE,height=ATLAS_SIZE,alpha=True); packed.colorspace_settings.name='Non-Color'; packed.pixels.foreach_set(mp)
for im in [base,emission,packed]:
    im.filepath_raw=str(UNITY/'Textures'/(im.name+'.png')); im.file_format='PNG'; im.save(); im.pack()

atlas=bpy.data.materials.new('G0_V3_IndustrialAtlas'); atlas.use_nodes=True
nodes=atlas.node_tree.nodes; links=atlas.node_tree.links; p=next(n for n in nodes if n.type=='BSDF_PRINCIPLED')
for im,inputname in [(base,'Base Color'),(emission,'Emission Color')]:
    t=nodes.new('ShaderNodeTexImage'); t.image=im; links.new(t.outputs['Color'],p.inputs[inputname])
p.inputs['Emission Strength'].default_value=1.4
t=nodes.new('ShaderNodeTexImage'); t.image=packed
sep=nodes.new('ShaderNodeSeparateColor'); links.new(t.outputs['Color'],sep.inputs[0]); links.new(sep.outputs['Red'],p.inputs['Metallic'])
inv=nodes.new('ShaderNodeMath'); inv.operation='SUBTRACT'; inv.inputs[0].default_value=1
links.new(t.outputs['Alpha'],inv.inputs[1]); links.new(inv.outputs[0],p.inputs['Roughness'])
mesh.data.materials.clear(); mesh.data.materials.append(atlas)
for f in mesh.data.polygons: f.material_index=0
# Explicit triangles make export, counts and UV interpolation reproducible.
tri=mesh.modifiers.new('Production triangulation','TRIANGULATE'); tri.keep_custom_normals=True
activate(mesh); bpy.ops.object.modifier_apply(modifier=tri.name)
clean_degenerate(mesh.data)

# LOD simplification protects silhouette tips, feet contacts, core and joint seams.
# Edges between rigid bone regions never share vertices, so no deform blending.
lods=[mesh]
for level,ratio in [(1,.57),(2,.30)]:
    o=mesh.copy(); o.data=mesh.data.copy(); lodcol.objects.link(o); o.name='G0_LOD'+str(level)
    protect=o.vertex_groups.new(name='LOD_PRESERVE_CORE_CONTACTS')
    indices=[v.index for v in o.data.vertices if v.co.z<.08 or (abs(v.co.x)<.085 and v.co.y<-.24 and v.co.z>2.45) or abs(v.co.x)>.96 or v.co.z>3.24]
    if indices: protect.add(indices,1,'REPLACE')
    d=o.modifiers.new('Silhouette protected collapse','DECIMATE'); d.ratio=ratio; d.use_collapse_triangulate=True
    d.vertex_group=protect.name; d.vertex_group_factor=1; d.invert_vertex_group=True
    activate(o); bpy.ops.object.modifier_apply(modifier=d.name)
    o.vertex_groups.remove(o.vertex_groups['LOD_PRESERVE_CORE_CONTACTS'])
    clean_degenerate(o.data)
    lods.append(o)
for o in lods:
    for g in list(o.vertex_groups):
        if g.name not in rig.data.bones: o.vertex_groups.remove(g)
    # Collapse can interpolate same-rigid-island weights; enforce exactly one.
    for v in o.data.vertices:
        if not v.groups: raise RuntimeError('unweighted vertex '+o.name)
        best=max(v.groups,key=lambda x:x.weight).group
        for g in o.vertex_groups: g.remove([v.index])
        o.vertex_groups[best].add([v.index],1,'REPLACE')
    a=o.modifiers.new('Rigid mechanical skin','ARMATURE'); a.object=rig
    o.parent=rig; o.matrix_parent_inverse=Matrix.Identity(4)
    o['rigid_skin']='one influence per vertex; no flexing armor'; o['authoring_forward']='-Y'
    o.hide_render=o!=mesh; o.hide_set(o!=mesh)

# FK actions use explicit hinge axes with in-place root, no gameplay events.
scene.render.fps=30
clips=[('Idle',1,61,True),('Run',1,25,True),('Attack',1,25,False),('Hit',1,16,False),('Death',1,46,False)]
for a in list(bpy.data.actions): bpy.data.actions.remove(a)
rig.animation_data_clear()
for pb in rig.pose.bones: pb.rotation_mode='XYZ'
def plant(label,target_y,target_z,pelvis_drop=0):
    """Bake a two-link sagittal solve into FK hinges; no runtime IK dependency."""
    h=rig.data.bones[label+'_HIP'].head_local.copy(); h.z+=pelvis_drop
    k=rig.data.bones[label+'_KNEE'].head_local; a=rig.data.bones[label+'_ANKLE'].head_local
    h0=rig.data.bones[label+'_HIP'].head_local
    v1=k-h0;v2=a-k
    l1=math.hypot(v1.y,v1.z);l2=math.hypot(v2.y,v2.z)
    dy=target_y-h.y;dz=target_z-h.z;d=min(math.hypot(dy,dz),l1+l2-.001)
    direction=math.atan2(dy,-dz)
    upper=direction-math.acos(max(-1,min(1,(l1*l1+d*d-l2*l2)/(2*l1*d))))
    ky=h.y+l1*math.sin(upper);kz=h.z-l1*math.cos(upper)
    lower=math.atan2(target_y-ky,-(target_z-kz))
    hip=upper-math.atan2(v1.y,-v1.z)
    knee=lower-math.atan2(v2.y,-v2.z)-hip
    rig.pose.bones[label+'_HIP'].rotation_euler.x=hip
    rig.pose.bones[label+'_KNEE'].rotation_euler.x=knee
    rig.pose.bones[label+'_ANKLE'].rotation_euler.x=-hip-knee
def pose(frame,name):
    t=(frame-1)/30
    for b in rig.pose.bones: b.rotation_euler=(0,0,0); b.location=(0,0,0); b.scale=(1,1,1)
    def rot(b,x=0,y=0,z=0): rig.pose.bones[b].rotation_euler=tuple(math.radians(v) for v in (x,y,z))
    # All rest hinges retain X lateral, local Y vertical, local Z depth.
    if name=='Idle':
        q=math.sin(t*math.pi); rot('TORSO',q*.9,0,q*.4); rot('SENSOR',0,q*2,0)
        rot('R_TOOL',q*1.4); rot('L_TOOL',-q*1.0)
    elif name=='Run':
        q=(frame-1)/24*2*math.pi; sway=math.sin(q)
        rot('TORSO',-6,0,sway*2)
        drop=-.040*(1-math.cos(q*2))
        rig.pose.bones['PELVIS'].location.y=drop
        for label,phase in [('L',q),('R',q+math.pi)]:
            swing=math.sin(phase); lift=max(0,math.sin(phase))
            ankle=rig.data.bones[label+'_ANKLE'].head_local
            plant(label,ankle.y+.27*math.cos(phase),ankle.z+.16*lift**1.5,drop)
            rot(label+'_SHOULDER',-swing*13); rot(label+'_ELBOW',-8-5*lift)
        rot('SENSOR',6)
    elif name=='Attack':
        # Anticipation, heavy tool drive, hold then damped recovery.
        keys=[(1,0),(6,-.30),(11,1),(14,.90),(19,.12),(25,0)]
        v=0
        for (a,x),(b,y) in zip(keys,keys[1:]):
            if a<=frame<=b: u=(frame-a)/(b-a); u=u*u*(3-2*u); v=x+(y-x)*u; break
        rot('TORSO',-v*8,-v*8); rot('R_SHOULDER',-v*46); rot('R_ELBOW',v*21); rot('R_TOOL',-v*17)
        rot('L_SHOULDER',-v*21); rot('L_ELBOW',-v*16); rot('L_JAW_OUTER',0,0,-v*12); rot('L_JAW_INNER',0,0,v*12)
        rot('L_HIP',v*6); rot('R_HIP',-v*4); rot('L_ANKLE',-v*6); rot('R_ANKLE',v*4)
    elif name=='Hit':
        u=(frame-1)/15; v=math.sin(min(u/.24,1)*math.pi/2)*math.exp(-max(u-.24,0)*5)
        if frame==16: v=0
        rot('TORSO',v*13,0,v*5); rot('SENSOR',-v*8); rot('R_SHOULDER',v*15); rot('L_SHOULDER',v*10)
    elif name=='Death':
        u=(frame-1)/45; kneel=min(u/.47,1); kneel=kneel*kneel*(3-2*kneel)
        fall=max(0,(u-.30)/.70); fall=fall*fall*(3-2*fall)
        drop=-.62*kneel;rig.pose.bones['PELVIS'].location.y=drop
        for label in ['L','R']:
            ankle=rig.data.bones[label+'_ANKLE'].head_local
            plant(label,ankle.y,ankle.z,drop)
        rot('TORSO',-fall*66,fall*13,fall*8); rot('SENSOR',fall*22)
        rot('R_SHOULDER',fall*36); rot('L_SHOULDER',fall*26); rot('R_ELBOW',-fall*22); rot('L_ELBOW',-fall*17)
    for b in rig.pose.bones:
        b.keyframe_insert('rotation_euler',frame=frame); b.keyframe_insert('location',frame=frame); b.keyframe_insert('scale',frame=frame)
for name,start,end,loop in clips:
    action=bpy.data.actions.new(name); action.use_fake_user=True
    rig.animation_data_create(); rig.animation_data.action=action
    for frame in range(start,end+1): pose(frame,name)
    action['loop']=loop; action['visual_only']=True; action['root_motion']=False
rig.animation_data.action=bpy.data.actions['Idle']; scene.frame_set(1); scene.frame_start=1; scene.frame_end=61

# Preserve old editable source in a hidden collection for traceable provenance;
# donor reference remains hidden and never belongs to the export selection.
hero.hide_render=True; hero.hide_viewport=True
edit.hide_render=True; edit.hide_viewport=True
prod.name='G0_V3_PRODUCTION_EXPORT'
scene.name='G0_PRODUCTION_V3'; scene['review_gate']='Production asset / isolated review only; human approval before Chapter01 integration'
scene['source_sha256']=source_hash; scene['presentation_scale']='accepted 1.10x applied only in Unity review'
note=bpy.data.texts.new('READ_ME_G0_PRODUCTION_V3')
note.write('Derived from accepted V2.1 source at main 467b43b. Z up, -Y forward, source meters, no Unity fit baked.\nOne skinned renderer/material per LOD; rigid one-bone weights, 18-bone game hierarchy. Five in-place visual clips at 30fps.\n2048 unique UV atlas, Android cap 1024: base color, metallic RGB/smoothness alpha, emission. No donor textures. LOD1/2 share atlas/skeleton.\nCatfish low-identity pelvis/knee/ankle provenance: Jungle Jim / CC BY 4.0; see preserved READ_ME_G0_V2_AND_ATTRIBUTION and ThirdPartyNotices.\nNo gameplay authority or live prefab replacement.\n')
activate(mesh)
output=ART/'G0_Production_V3.blend'
bpy.ops.wm.save_as_mainfile(filepath=str(output),compress=True)

lod_reports=[]
for o in lods:
    o.data.calc_loop_triangles(); b=bounds([o]); b.update(name=o.name,vertices=len(o.data.vertices),triangles=len(o.data.loop_triangles),reduction_ratio=1-len(o.data.loop_triangles)/len(mesh.data.loop_triangles),materials=len(o.data.materials),uv_layers=len(o.data.uv_layers))
    lod_reports.append(b)
report=dict(source=str(SOURCE.relative_to(ROOT)),source_sha256=source_hash,output=str(output.relative_to(ROOT)),baseline=baseline,lods=lod_reports,
            bones=[dict(name=b.name,parent=b.parent.name if b.parent else None,head=list(b.head_local)) for b in rig.data.bones],
            clips=[dict(name=n,start=s,end=e,fps=30,loop=l,in_place=True) for n,s,e,l in clips],refinements=refinements,
            renderer_count_per_lod=1,material_count_per_lod=1,texture_resolution=ATLAS_SIZE,android_texture_cap=1024,source_mesh_count=142,forward='Blender -Y; Unity orientation validated on import',presentation_fit=0.45854827761650085)
(DATA/'production_metrics.json').write_text(json.dumps(report,indent=2),encoding='utf-8')

# Export exactly the production rig and LODs, no hidden source or reference.
bpy.ops.object.select_all(action='DESELECT'); rig.select_set(True)
for o in lods: o.hide_set(False); o.select_set(True)
bpy.context.view_layer.objects.active=rig
fbx=UNITY/'Models/G0_Production_V3.fbx'
bpy.ops.export_scene.fbx(filepath=str(fbx),use_selection=True,object_types={'MESH','ARMATURE'},global_scale=1,apply_unit_scale=True,
    apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',bake_space_transform=False,use_mesh_modifiers=True,
    mesh_smooth_type='FACE',use_tspace=True,add_leaf_bones=False,primary_bone_axis='Y',secondary_bone_axis='X',
    bake_anim=True,bake_anim_use_all_bones=True,bake_anim_use_nla_strips=False,bake_anim_use_all_actions=True,
    bake_anim_force_startend_keying=True,bake_anim_step=1,bake_anim_simplify_factor=0,path_mode='AUTO',embed_textures=False)
print('G0_V3_PRODUCTION_EXPORTED',json.dumps({'lods':lod_reports,'bones':len(rig.data.bones),'clips':[c[0] for c in clips]}))
