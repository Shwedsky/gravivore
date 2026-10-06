"""Editable G-0 blockout, explicit donor adaptation, portable comparison and cameras.

Run after inspect_donor.py with Blender 5.2.2 LTS. No Unity assets are touched.
"""
import bpy, bmesh, sys, math, json
from pathlib import Path
from mathutils import Vector, Matrix
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(Path(__file__).parent))
from studio import material, setup_studio, aim
OUT=ROOT/'art/visual-production-v2/g0'
DATA=ROOT/'docs/visual-production-v2/g0-bipedal-v2/data'
OUT.mkdir(parents=True,exist_ok=True)
assert bpy.app.version==(5,2,2)
bpy.ops.wm.open_mainfile(filepath=str(ROOT/'.local-g0-v2/catfish_imported.blend'))

# Bake only the unaltered donor REST shape for reference and component extraction.
deps=bpy.context.evaluated_depsgraph_get()
donor=[]
for o in list(bpy.context.scene.objects):
    if o.type!='MESH': continue
    me=bpy.data.meshes.new_from_object(o.evaluated_get(deps),preserve_all_data_layers=True,depsgraph=deps)
    me.transform(o.matrix_world)
    donor.append((o.name,me))
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
for a in list(bpy.data.actions): bpy.data.actions.remove(a)
for arm in list(bpy.data.armatures):
    if arm.users==0: bpy.data.armatures.remove(arm)
scene=bpy.context.scene; scene.name='G0_BIPEDAL_V2_REVIEW'
scene.unit_settings.system='METRIC'; scene.unit_settings.scale_length=1
hero=bpy.data.collections.new('G0_BIPEDAL_V2_EDITABLE'); scene.collection.children.link(hero)
reference=bpy.data.collections.new('CATFISH_REFERENCE_CC_BY_4_0'); scene.collection.children.link(reference)
rigcol=bpy.data.collections.new('G0_MECHANICAL_PIVOTS'); hero.children.link(rigcol)
shellcol=bpy.data.collections.new('CUSTOM_ARMOR_AND_IDENTITY'); hero.children.link(shellcol)
structure=bpy.data.collections.new('CUSTOM_STRUCTURE_AND_TOOLS'); hero.children.link(structure)
retained=bpy.data.collections.new('DONOR_PELVIS_KNEE_ANKLE_ONLY'); hero.children.link(retained)

armor=material('G0 | titanium shell',(.075,.12,.155),.8,.28)
pale=material('G0 | ceramic titanium edge',(.29,.38,.41),.7,.26)
dark=material('G0 | graphite mechanism',(.038,.054,.068),.72,.32)
steel=material('G0 | machined actuator',(.16,.21,.24),.88,.23)
cyan=material('G0 | cyan gravity cell',(.008,.65,.85),.3,.23,2.2)
copper=material('G0 | copper service mark',(.45,.19,.075),.72,.34)

def link(o,col):
    for c in list(o.users_collection): c.objects.unlink(o)
    col.objects.link(o)

def pivot(name,loc,parent=None):
    o=bpy.data.objects.new(name,None); rigcol.objects.link(o)
    o.empty_display_type='PLAIN_AXES'; o.empty_display_size=.1
    o.location=loc; bpy.context.view_layer.update()
    if parent:
        mw=o.matrix_world.copy(); o.parent=parent; o.matrix_world=mw
    return o
root=pivot('G0_ROOT_METERS_Z_UP_FORWARD_MINUS_Y',(0,0,0))
pelvis=pivot('PIVOT_PELVIS',(0,0,2.1),root)
chest=pivot('PIVOT_TORSO',(0,0,2.72),pelvis)

def finish(o,name,mat,col=structure,parent=root,bevel=.012):
    o.name=name; link(o,col); o.data.materials.clear(); o.data.materials.append(mat)
    o['authorship']='GRAVIVORE original blockout geometry'
    if bevel:
        m=o.modifiers.new('Editable edge chamfer','BEVEL'); m.width=bevel; m.segments=2
        m=o.modifiers.new('Weighted panel normals','WEIGHTED_NORMAL'); m.keep_sharp=True; m.weight=40
    bpy.context.view_layer.update()
    if parent:
        mw=o.matrix_world.copy(); o.parent=parent; o.matrix_world=mw
    return o

def mesh(name,verts,faces,mat,col=shellcol,parent=root,bevel=.015):
    me=bpy.data.meshes.new(name+' editable mesh'); me.from_pydata(verts,[],faces); me.update()
    o=bpy.data.objects.new(name,me); col.objects.link(o)
    bm=bmesh.new(); bm.from_mesh(me); bmesh.ops.recalc_face_normals(bm,faces=bm.faces); bm.to_mesh(me); bm.free()
    return finish(o,name,mat,col,parent,bevel)

def plate(name,outline,yfront,yback,mat,parent=root,bevel=.015):
    # Purpose-shaped X/Z contour, front plane and tapered rear contour.
    n=len(outline); cx=sum(x for x,z in outline)/n; cz=sum(z for x,z in outline)/n
    vs=[(x,yfront,z) for x,z in outline]+[(cx+(x-cx)*.88,yback,cz+(z-cz)*.92) for x,z in outline]
    faces=[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]
    faces += [(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    return mesh(name,vs,faces,mat,parent=parent,bevel=bevel)

def carapace(name,outline,depths,apex,mat,parent=root):
    # An actual sculpted panel ridge, rather than a single flat front ngon.
    n=len(outline); cx=sum(x for x,z in outline)/n; cz=sum(z for x,z in outline)/n
    vs=[(x,y,z) for (x,z),y in zip(outline,depths)]
    vs += [(cx+(x-cx)*.82,.10,cz+(z-cz)*.93) for x,z in outline]
    vs.append(apex)
    fs=[(i,(i+1)%n,2*n) for i in range(n)]
    fs.append(tuple(range(n,2*n)))
    fs += [(i,i+n,(i+1)%n+n,(i+1)%n) for i in range(n)]
    return mesh(name,vs,fs,mat,parent=parent,bevel=.011)

def loft(name,rings,mat,parent=root,col=shellcol,bevel=.015):
    # Octagonal section; variable profile avoids cuboid limb armor.
    vs=[]
    for z,x,y,w,d in rings:
        for px,py in [(-.68,-1),(.68,-1),(1,-.6),(1,.6),(.68,1),(-.68,1),(-1,.6),(-1,-.6)]:
            vs.append((x+px*w/2,y+py*d/2,z))
    fs=[tuple(range(7,-1,-1)),tuple(range((len(rings)-1)*8,len(rings)*8))]
    for j in range(len(rings)-1):
        for i in range(8): fs.append((j*8+i,j*8+(i+1)%8,(j+1)*8+(i+1)%8,(j+1)*8+i))
    return mesh(name,vs,fs,mat,col,parent,bevel)

def beam(name,a,b,r,mat=steel,parent=root,vertices=12):
    av,bv=Vector(a),Vector(b); delta=bv-av
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=r,depth=delta.length,location=(av+bv)/2)
    o=bpy.context.object; o.rotation_euler=delta.to_track_quat('Z','Y').to_euler()
    for p in o.data.polygons: p.use_smooth=len(p.vertices)==4
    return finish(o,name,mat,structure,parent,.006)

def axle(name,loc,r,length,parent=root):
    x,y,z=loc
    beam(name+' spindle',(x-length/2,y,z),(x+length/2,y,z),r,dark,parent,20)
    for sign in [-1,1]:
        beam(name+f' endcap {sign}',(x+sign*length/2,y,z),(x+sign*(length/2+.025),y,z),r*.78,steel,parent,20)

# Reframe retained mechanisms; world-baked meshes preserve exact source topology/UV.
source={n:me for n,me in donor}
adaptation=[]
def retain_group(suffixes,target,factor,name,parent):
    selected=[(f'mesh_rep_0_ori_repair_{s}',source[f'mesh_rep_0_ori_repair_{s}']) for s in suffixes]
    pts=[v.co for _,me in selected for v in me.vertices]
    center=Vector([(min(v[i] for v in pts)+max(v[i] for v in pts))/2 for i in range(3)])
    for original,me in selected:
        copy=me.copy(); copy.transform(Matrix.Translation(Vector(target)-center*factor)@Matrix.Scale(factor,4))
        bm=bmesh.new(); bm.from_mesh(copy)
        weights=bm.verts.layers.deform.active
        if weights:
            for v in bm.verts: v[weights].clear()
        bm.to_mesh(copy); bm.free()
        o=bpy.data.objects.new('RETAINED_'+name+'_'+original.rsplit('_',1)[-1],copy); retained.objects.link(o)
        finish(o,o.name,steel,retained,parent,0)
        o['authorship']='Adapted Catfish Mech by Jungle Jim / CC BY 4.0'
        o['donor_object']=original
        o['modifications']='REST bake, world transform, mechanism regroup, project material, original weights discarded'
        me.calc_loop_triangles()
        adaptation.append({'object':o.name,'source_object':original,'source_triangles':len(me.loop_triangles),'scale':factor,'target':target})
retain_group([134],(0,.05,2.13),3.1,'PELVIS',pelvis)

# Split mechanical legs with exposed rails and custom armor, two load-bearing feet.
for s,label in [(-1,'R'),(1,'L')]:
    hip=(s*.29,.04,2.14); knee=(s*.40,-.18,1.47); hock=(s*.40,.18,.58); ankle=(s*.43,-.025,.29)
    ph=pivot('PIVOT_'+label+'_HIP',hip,pelvis); pk=pivot('PIVOT_'+label+'_KNEE',knee,ph)
    pa=pivot('PIVOT_'+label+'_ANKLE',ankle,pk)
    retain_group([135,136,137,138] if s<0 else [139,140,152,153],(s*.4,-.13,1.47),2.7,label+'_KNEE',pk)
    retain_group([143,144,147,148] if s<0 else [141,142,145,146],(s*.43,.08,.43),1.7,label+'_ANKLE',pa)
    axle(label+' hip',hip,.15,.31,ph)
    axle(label+' knee',knee,.135,.31,pk)
    axle(label+' hock',hock,.105,.26,pk)
    beam(label+' thigh load rail',hip,knee,.105,dark,ph)
    for side in [-1,1]:
        off=s*.1*side
        beam(label+f' shin rail {side}',(knee[0]+off,knee[1],knee[2]-.05),(hock[0]+off,hock[1],hock[2]+.04),.055,steel,pk)
    beam(label+' ankle shaft',hock,ankle,.08,dark,pa)
    beam(label+' rear thigh actuator',(s*.32,.16,2.07),(s*.42,.01,1.55),.045,steel,ph)
    loft(label+' tapered thigh shield',[(1.61,s*.39,-.19,.19,.18),(1.91,s*.34,-.12,.32,.27),(2.14,s*.30,-.03,.26,.24)],armor,ph)
    # Long narrow shin blade leaves the posterior twin rails visible in side/back.
    loft(label+' shin blade',[(.66,s*.4,.075,.13,.13),(.95,s*.4,-.06,.21,.17),(1.38,s*.4,-.22,.26,.21)],pale,pk)
    plate(label+' knee prow',[(s*.4-.14,1.49),(s*.4-.08,1.64),(s*.4+.1,1.63),(s*.4+.15,1.48),(s*.4,1.36)],-.35,-.23,armor,pk)
    loft(label+' hock cover',[(.29,s*.43,.00,.19,.17),(.5,s*.41,.09,.22,.20),(.65,s*.4,.18,.16,.18)],armor,pa)
    # Compact separated toe rails, not source Catfish splayed feet.
    for side in [-1,1]:
        tx=s*.43+side*.09
        loft(label+f' toe {side}',[(.065,tx,-.17,.145,.58),(.14,tx,-.17,.15,.58),(.23,tx,-.015,.12,.31)],dark,pa,bevel=.015)
        loft(label+f' toe cap {side}',[(.145,tx,-.30,.14,.31),(.19,tx,-.25,.13,.27)],armor,pa,bevel=.008)
    beam(label+' foot cross tie',(s*.43-.07,-.24,.21),(s*.43+.07,-.24,.21),.012,steel,pa,8)

# Skeletal waist with offset load cylinders and a compact pelvis prow.
beam('Central spinal axis',(0,.06,2.12),(0,.08,2.97),.115,dark,pelvis,16)
for s in [-1,1]:
    beam('Waist actuator '+str(s),(s*.17,.10,2.24),(s*.31,.11,2.85),.055,steel,chest)
    plate('Hip swept fairing '+str(s),[(s*.15,2.28),(s*.41,2.22),(s*.48,2.04),(s*.27,1.95),(s*.18,2.07)],-.16,.1,armor,pelvis)
loft('Pelvis ventral prow',[(2.03,0,-.19,.2,.22),(2.22,0,-.1,.36,.24),(2.35,0,-.035,.26,.2)],dark,pelvis)
loft('Torso internal load frame',[(2.65,0,.04,.43,.37),(2.95,0,.06,.65,.47),(3.30,0,.07,.76,.48),(3.44,0,.07,.50,.35)],dark,chest)

# Swept split carapace; central recessed energy slit is physically behind armor lips.
for s in [-1,1]:
    outline=[(s*.065,3.32),(s*.29,3.46),(s*.56,3.28),(s*.49,3.03),(s*.27,2.70),(s*.095,2.91)]
    carapace('Chest swept carapace '+str(s),outline,[-.24,-.13,-.10,-.21,-.20,-.285],(s*.30,-.40,3.14),armor,chest)
    carapace('Chest crown ridge '+str(s),[(s*.09,3.32),(s*.29,3.445),(s*.54,3.275),(s*.31,3.34)],[-.25,-.15,-.14,-.28],(s*.3,-.30,3.375),pale,chest)
    plate('Ventral rib '+str(s),[(s*.10,2.89),(s*.25,2.71),(s*.18,2.52),(s*.075,2.63)],-.19,-.02,armor,chest,.009)
plate('Recessed gravity cell black socket',[(-.12,3.18),(0,3.32),(.12,3.18),(.08,2.92),(0,2.85),(-.08,2.92)],-.282,-.12,dark,chest,.009)
plate('Integrated cyan diamond slit',[(-.052,3.15),(0,3.23),(.052,3.15),(.028,2.97),(0,2.94),(-.028,2.97)],-.292,-.28,cyan,chest,.003)
plate('Core containment keel',[(-.05,2.94),(0,2.86),(.05,2.94),(.032,2.8),(-.032,2.8)],-.315,-.22,steel,chest,.006)

# Sensor blade has no human face/visor proportions and no circular eye.
sensor=pivot('PIVOT_SENSOR',(0,.04,3.51),chest)
beam('Sensor neck carriage',(0,.08,3.40),(0,.08,3.62),.085,steel,sensor)
loft('Sensor swept wedge',[(3.50,0,-.07,.17,.39),(3.59,0,-.13,.22,.54),(3.67,0,.01,.105,.28)],armor,sensor,bevel=.01)
plate('Sensor front graphite aperture',[(-.095,3.565),(-.07,3.61),(.07,3.61),(.095,3.565),(.035,3.54),(-.035,3.54)],-.411,-.389,dark,sensor,.002)
beam('Sensor cyan horizontal slit',(-.066,-.418,3.578),(.066,-.418,3.578),.009,cyan,sensor,8)

# Open arm linkage, sharp swept shoulder vanes, unequal non-ballistic gravity tools.
for s,label in [(-1,'R'),(1,'L')]:
    sh=(s*.63,.035,3.20); el=(s*.78,-.01,2.73); wrist=(s*.86,-.25,2.35)
    ps=pivot('PIVOT_'+label+'_SHOULDER',sh,chest); pe=pivot('PIVOT_'+label+'_ELBOW',el,ps)
    pw=pivot('PIVOT_'+label+'_TOOL',wrist,pe)
    axle(label+' shoulder',sh,.14,.27,ps); axle(label+' elbow',el,.11,.27,pe)
    beam(label+' humeral load bar',sh,el,.082,dark,ps)
    beam(label+' upperarm exposed piston',(s*.72,.12,3.13),(s*.86,.10,2.75),.037,steel,ps)
    carapace(label+' swept shoulder vane',[(s*.53,3.28),(s*.65,3.44),(s*.89,3.22),(s*.93,3.01),(s*.73,3.12)],[-.12,-.07,-.12,-.13,-.20],(s*.72,-.27,3.25),pale,ps)
    carapace(label+' upperarm dorsal shell',[(s*.64,3.07),(s*.76,3.09),(s*.87,2.83),(s*.78,2.75),(s*.70,2.91)],[-.02,.0,-.035,-.07,-.08],(s*.77,-.16,2.94),armor,ps)
    beam(label+' forearm spine',el,wrist,.065,steel,pe)
    loft(label+' gravity gauntlet',[(2.25,s*.87,-.31,.15,.24),(2.49,s*.83,-.12,.24,.31),(2.67,s*.78,-.015,.17,.23)],armor,pe,bevel=.011)
    # Forearm aperture intentionally left open between dual emitter jaws.
    if s<0:
        for side in [-1,1]:
            x=s*.88+side*.1
            loft('Lash emitter split blade '+str(side),[(2.12,x,-.89,.045,.15),(2.22,x,-.74,.075,.33),(2.41,x,-.44,.10,.42),(2.51,x,-.21,.08,.20)],pale,pw,bevel=.008)
            beam('Lash cyan rail '+str(side),(x,-.94,2.17),(x,-.62,2.34),.011,cyan,pw,8)
        beam('Lash recessed power spine',(s*.88,-.22,2.40),(s*.88,-.74,2.22),.035,dark,pw)
    else:
        for side in [-1,1]:
            x=s*.88+side*.09
            loft('Capture jaw '+str(side),[(2.02+side*.07,x,-.57,.045,.12),(2.22,x+side*.025,-.46,.075,.27),(2.41,x,-.22,.07,.24)],pale,pw,bevel=.008)
        plate('Capture cyan inset',[(s*.845,2.34),(s*.895,2.34),(s*.89,2.22),(s*.85,2.22)],-.38,-.36,cyan,pw,.002)
    beam(label+' shoulder service seam',(s*.67,-.208,3.34),(s*.79,-.25,3.22),.012,copper,ps,8)

# Back silhouette: integrated spine and paired swept containment vanes.
loft('Rear spinal containment',[(2.52,0,.28,.20,.20),(3.13,0,.32,.31,.22),(3.55,0,.26,.19,.18)],armor,chest,bevel=.02)
for s in [-1,1]:
    yz=[(.27,2.86),(.42,2.96),(.77,3.31),(.57,3.64),(.32,3.53)]
    vs=[(s*.30+dx,y,z) for dx in [-.065,.065] for y,z in yz]
    fs=[(4,3,2,1,0),(5,6,7,8,9)]+[(i,(i+1)%5,(i+1)%5+5,i+5) for i in range(5)]
    mesh('Rear swept containment fin '+str(s),vs,fs,armor,parent=chest,bevel=.012)
    beam('Rear fin crest rail '+str(s),(s*.30,.58,3.60),(s*.30,.77,3.31),.028,pale,chest,8)
    beam('Rear fin cyan slit '+str(s),(s*.30,.785,3.25),(s*.30,.63,3.48),.012,cyan,chest,8)
    beam('Rear containment brace '+str(s),(s*.19,.31,2.75),(s*.35,.37,3.15),.055,dark,chest)

# Compact original mechanical skeleton: explicit rigid weights, no donor face/hand skeleton.
rigdata=bpy.data.armatures.new('G0 original mechanical skeleton')
rig=bpy.data.objects.new('G0_MECHANICAL_RIG_BLOCKOUT',rigdata); rigcol.objects.link(rig)
rig.parent=root
rig.show_in_front=True
bpy.context.view_layer.objects.active=rig; rig.select_set(True)
bpy.ops.object.mode_set(mode='EDIT')
bone_map={}
for p in list(rigcol.objects):
    if p.type!='EMPTY': continue
    b=rigdata.edit_bones.new(p.name.removeprefix('PIVOT_'))
    b.head=p.matrix_world.translation; b.tail=b.head+Vector((0,0,.16))
    bone_map[p.name]=b.name
    if p.parent and p.parent.name in bone_map: b.parent=rigdata.edit_bones[bone_map[p.parent.name]]
bpy.ops.object.mode_set(mode='OBJECT'); rig.select_set(False)
for o in list(hero.all_objects):
    if o.type!='MESH': continue
    if o.parent and o.parent.name in bone_map:
        o.vertex_groups.clear()
        group=o.vertex_groups.new(name=bone_map[o.parent.name]); group.add(list(range(len(o.data.vertices))),1,'REPLACE')
        m=o.modifiers.new('Rigid mechanical bone binding','ARMATURE'); m.object=rig
rig['status']='Original blockout rigid skeleton; unanimated; no IK or locomotion certification'
root.location.z=-.065
bpy.context.view_layer.update()
hero_points=[o.matrix_world@v.co for o in hero.all_objects if o.type=='MESH' for v in o.data.vertices]
hero_height=max(v.z for v in hero_points)-min(v.z for v in hero_points)

# Static original donor, comparison only. Retains source silhouette/material/UV.
donor_root=bpy.data.objects.new('CATFISH_COMPARISON_ROOT',None); reference.objects.link(donor_root)
points=[v.co for _,me in donor for v in me.vertices]
zmin=min(v.z for v in points); zmax=max(v.z for v in points)
dfactor=hero_height/(zmax-zmin)
for name,me in donor:
    o=bpy.data.objects.new('CATFISH_'+name,me); reference.objects.link(o); o.parent=donor_root
    o['authorship']='Catfish Mech low-poly (animated) by Jungle Jim / CC BY 4.0; comparison only'
donor_root.scale=(dfactor,)*3; donor_root.location.z=-zmin*dfactor
reference.hide_render=True; reference.hide_viewport=True

# Pack only used, downsampled donor reference maps. Hero has no texture dependency.
used=set()
for o in reference.objects:
    if o.type=='MESH':
        for m in o.data.materials:
            if m and m.node_tree:
                for n in m.node_tree.nodes:
                    if n.type=='TEX_IMAGE' and n.image: used.add(n.image)
for i in used:
    if max(i.size)>1024: i.scale(1024,1024)
    i.pack()
for i in list(bpy.data.images):
    if i not in used: bpy.data.images.remove(i)

cam,ground=setup_studio()
cam.location=(5,-8,4.7); aim(cam,(0,0,1.9)); cam.data.ortho_scale=4.65
for name,pos,scale in [('01_FRONT',(0,-9,2.0),4.6),('02_SIDE',(9,0,2),4.6),('03_BACK',(0,9,2),4.6),('04_THREEQUARTER',(5,-8,4.7),4.65),('05_GAMEPLAY',(0,11.2,15.7),4.65)]:
    data=bpy.data.cameras.new(name); o=bpy.data.objects.new('CAM_'+name,data); scene.collection.objects.link(o)
    data.type='PERSP' if 'GAMEPLAY' in name else 'ORTHO'; data.ortho_scale=scale
    o.location=pos; aim(o,(0,0,.9) if 'GAMEPLAY' in name else (0,0,1.9))
    if 'GAMEPLAY' in name: data.lens=36/(2*math.tan(math.radians(46)/2)); data.sensor_fit='VERTICAL'; data.sensor_height=36

text=bpy.data.texts.new('READ_ME_G0_V2_AND_ATTRIBUTION')
text.write('G-0 BIPEDAL BLOCKOUT V2 | 2026-10-06\nZ up, -Y forward, meters. Exactly two supporting feet. Main editable collection: G0_BIPEDAL_V2_EDITABLE.\nCustom upper body, weapons, armor, feet, structural legs and original rigid skeleton. No polished animation/UV/LOD/runtime binding.\nDONOR: Catfish Mech low-poly (animated) by Jungle Jim (jungle_jim). CC BY 4.0.\nhttps://sketchfab.com/3d-models/catfish-mech-low-poly-animated-ad9bc16464744935b1ac9b7768a17474\nhttps://creativecommons.org/licenses/by/4.0/\n17 pelvis/knee/ankle objects adapted by REST bake, reframe and material replacement. Original static donor in hidden comparison collection; texture downsample only. No endorsement.\nSee repository inspection, design decision, review, per-object adaptation JSON and ThirdPartyNotices.\n')
scene['review_gate']='STOP: human art-direction approval required before further production'
scene['donor_attribution']='Catfish Mech low-poly (animated) by Jungle Jim, CC BY 4.0; 17 adapted mechanisms, original comparison only'
scene['hero_forward_axis']='-Y'; scene['hero_up_axis']='Z'

# Authoring viewport opens on the hero rather than donor or lighting objects.
bpy.ops.object.select_all(action='DESELECT')
for o in hero.all_objects:
    if o.type=='MESH': o.select_set(True)
bpy.context.view_layer.objects.active=next(o for o in hero.all_objects if o.name.startswith('Chest swept'))
for screen in bpy.data.screens:
    for area in screen.areas:
        if area.type=='VIEW_3D':
            area.spaces.active.region_3d.view_distance=6
            area.spaces.active.region_3d.view_location=(0,0,1.9)
            area.spaces.active.region_3d.view_rotation=cam.rotation_euler.to_quaternion()
            area.spaces.active.shading.type='MATERIAL'
scene.camera=cam
bpy.ops.outliner.orphans_purge(do_local_ids=True,do_linked_ids=True,do_recursive=True)
(DATA/'donor_adaptation.json').write_text(json.dumps(adaptation,indent=2),encoding='utf-8')
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'G0_Bipedal_Blockout_V2.blend'),compress=True)
print('G0_BLOCKOUT_SAVED',str(OUT/'G0_Bipedal_Blockout_V2.blend'))
