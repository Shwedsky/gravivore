"""Original V47 actor bodies; retain existing rest rigs, actions and runtime IDs.

Source: tracked project-owned art only. Profiled armor shells, open joints,
load-bearing actuators and role-specific tooling carry the gameplay-scale read.
The V46 4x4 PBR atlas remains the surface source; no raw donor cache is read.
"""
import bpy, bmesh, math, json, hashlib, sys
from pathlib import Path
from mathutils import Vector, Matrix
ROOT=Path(__file__).resolve().parents[2]
helper=(ROOT/'Tools/surface-hero-v46/build_machinery.py').read_text()
exec(helper[helper.index('PALETTE='):helper.index('def finish(')])
SOURCE=ROOT/'art/concept-convergence-v47';SOURCE.mkdir(parents=True,exist_ok=True)
DOC=ROOT/'docs/history/implementation-passes/chapter01-visual-replacement-v3/v47'
metrics={};current_bone='BODY'
base_mesh=mesh
def mesh(name,verts,faces,tile,bevel=0,smooth=False):
    o=base_mesh(name,verts,faces,tile,bevel,smooth)
    bm=bmesh.new();bm.from_mesh(o.data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(o.data);bm.free()
    g=o.vertex_groups.new(name=current_bone);g.add(list(range(len(o.data.vertices))),1,'REPLACE')
    return o
def weight(b):
    global current_bone
    current_bone=b
def profile(name,p,size,tile=2):
    # Authored compound-curvature armor: deep skirt, inset shoulder, domed crown.
    x,y,z=p;w,d,h=size
    plan=[(-.28,-.5),(.28,-.5),(.44,-.36),(.5,-.10),(.48,.25),(.31,.47),(.12,.5),(-.12,.5),(-.31,.47),(-.48,.25),(-.5,-.10),(-.44,-.36)]
    levels=[(-.5,1),(-.30,1),(.16,.93),(.40,.76),(.50,.44)]
    verts=[(x+a*w*s,y+b*d*s,z+k*h) for k,s in levels for a,b in plan];n=len(plan)
    faces=[tuple(reversed(range(n))),tuple(range((len(levels)-1)*n,len(levels)*n))]
    faces += [(j*n+i,j*n+(i+1)%n,(j+1)*n+(i+1)%n,(j+1)*n+i) for j in range(len(levels)-1) for i in range(n)]
    return mesh(name,verts,faces,tile,.012)
def fin(name,points,z,h,tile=2):
    n=len(points);verts=[(x,y,z+k*h) for k in (-.5,.5) for x,y in points]
    faces=[tuple(reversed(range(n))),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    return mesh(name,verts,faces,tile,.010)
def link(name,a,b,r,tile=10):
    a,b=Vector(a),Vector(b);t=(b-a).normalized();mid=(a+b)/2
    # Telescoping, attached bushings and visible polished piston, not a bare rod.
    tube(name+' machined actuator body',[a,a.lerp(b,.62)],r,tile,12)
    tube(name+' exposed piston',[a.lerp(b,.55),b],r*.50,2,12)
    for p in (a,b):
        tube(name+' bearing hub',[p-t*r*.6,p+t*r*.6],r*1.30,0,12)
def core(p,r,tile=5):
    weight('CORE')
    lathe('Forged core containment',[(0,-r*.16),(r*.96,-r*.16),(r*1.22,0),(r*1.2,r*.16),(r*.84,r*.22)],p,0,24)
    lathe('Inset energy lens',[(0,0),(r*.68,0),(r*.78,r*.10),(r*.63,r*.30),(0,r*.43)],p,tile,24)
    weight('BODY')
def legs(rig,thickness):
    for b in rig.data.bones:
        if not b.name.startswith('HIP_'):continue
        k=rig.data.bones['KNEE_'+b.name.split('_')[1]]
        hip=Vector(b.head_local);knee=Vector(k.head_local)
        toe=Vector((knee.x*1.25,knee.y*1.12,.075))
        weight(b.name);link('Upper support',hip,knee,thickness)
        profile('Tapered joint cover',(knee.x,knee.y,knee.z+.07),(thickness*2.7,thickness*3,.14),1)
        weight(k.name);link('Load bearing tibia',knee,toe,thickness*.75)
        direction=Vector((toe.x,toe.y,0)).normalized()
        p=toe+direction*.07
        fin('Ground contact talon',[(p.x-.075,p.y+.10),(p.x+.075,p.y+.10),(p.x+direction.x*.19,p.y+direction.y*.19)],.065,.10,0)
        if thickness>.10:profile('Load bearing foot shoe',(toe.x,toe.y,.12),(.28,.40,.20),1)
    weight('BODY')
def scout(rig):
    legs(rig,.055)
    profile('Recon exposed chassis',(0,.05,.51),(.62,.80,.20),0)
    for s in (-1,1):
        profile('Split swept recon carapace',(s*.19,.10,.66),(.30,.72,.24),2)
        fin('Pointed reconnaissance sensor',[(s*.12,-.26),(s*.29,-.34),(s*.24,-.76),(s*.12,-.52)],.55,.12,2)
        link('Forward sensor gimbal',(s*.16,-.12,.48),(s*.19,-.45,.54),.033)
    core((0,-.22,.64),.105)
    for y in (.25,.37,.49):plate('Rear heat sink',(0,y,.68),(.16,.035,.075),0,.01,.004)
def cutter(rig):
    legs(rig,.085)
    profile('Low predator engine keel',(0,.06,.58),(1.06,1.40,.34),0)
    for s,label in [(-1,'R'),(1,'L')]:
        profile('Forward split predator armor',(s*.31,-.18,.79),(.63,1.06,.40),2)
        weight(label+'_BLADE')
        link('Blade power linkage',(s*.52,.02,.55),(s*.71,-.60,.49),.075)
        lathe('Cutting drive bearing',[(0,-.07),(.17,-.07),(.20,0),(.16,.10),(0,.10)],(s*.61,-.24,.53),10,20)
        pts=[(s*.57,-.43),(s*.84,-.50),(s*1.07,-.91),(s*.95,-1.43),(s*.58,-1.95),(s*.74,-1.35),(s*.77,-.91)]
        fin('Forged swept cutting jaw',pts,.46,.20,1)
        fin('Independent polished cutting edge',[(s*.96,-.87),(s*1.05,-.91),(s*.93,-1.43),(s*.58,-1.95),(s*.72,-1.55)],.49,.025,2)
        tube('Blade heated working channel',[(s*.79,-.67,.58),(s*.91,-1.02,.58),(s*.84,-1.38,.56)],.021,5,8)
        weight('BODY')
        for y in (.45,.56,.67):plate('Predator exhaust recess',(s*.25,y,.84),(.28,.052,.085),15,.016,.005)
    core((0,-.32,.82),.135)
def warden(rig):
    legs(rig,.105)
    profile('Defensive spine',(0,.13,.78),(1.04,1.50,.50),0)
    for s in (-1,1):
        for y,z in [(-.43,1.10),(.02,1.21),(.47,1.11)]:
            profile('Overlapping bastion carapace',(s*.39,y,z),(.71,.68,.42),2)
        # Shield frontal coverage defines mitigation direction at phone scale.
        profile('Thick frontal shield cheek',(s*.43,-.77,.79),(.58,.34,.93),1)
        link('Shield lower support',(s*.32,-.20,.52),(s*.45,-.80,.45),.075)
        plate('Shield recessed warning insert',(s*.46,-.957,.94),(.14,.02,.30),4,.02,.005)
    core((0,-.80,1.04),.145,6)
def drone(rig):
    profile('Suspended arc drive hull',(0,0,1.10),(.65,.80,.36),0)
    for s in (-1,1):
        fin('Long swept arc electrode',[(s*.16,-.25),(s*.78,-.66),(s*.97,-.37),(s*.63,.19),(s*.24,.35)],1.13,.16,2)
        tube('Recessed electrode conductor',[(s*.28,-.36,1.23),(s*.64,-.50,1.23),(s*.80,-.36,1.23)],.026,5,10)
    fin('Rear arc stabilizer',[(-.22,.23),(.22,.23),(0,.81)],1.15,.18,1)
    for b in rig.data.bones:
        if not b.name.startswith('ROTOR_'):continue
        weight(b.name);p=Vector(b.head_local)
        lathe('Ducted levitation drive',[(0,-.09),(.13,-.09),(.20,-.04),(.21,.03),(.14,.08),(0,.08)],p,0,20)
        lathe('Recessed rotor hub',[(0,0),(.10,0),(.11,.06),(0,.09)],p,10,16)
    weight('BODY');core((0,-.25,1.29),.16)
def carrier(rig):
    profile('Hauler heavy lower hull',(0,0,.58),(1.42,1.90,.55),0)
    for b in rig.data.bones:
        if not b.name.startswith('WHEEL_'):continue
        weight(b.name);p=Vector(b.head_local)
        o=lathe('Armored traction wheel',[(0,-.13),(.24,-.13),(.29,-.08),(.30,.08),(.24,.13),(0,.13)],(0,0,0),0,20)
        rot=Matrix.Rotation(math.pi/2,4,'Y');o.data.transform(rot);o.data.transform(Matrix.Translation(p))
        tube('Supported axle',[p+Vector((-.15,0,0)),p+Vector((.15,0,0))],.11,10,12)
    weight('BODY')
    for s in (-1,1):
        profile('Swept haulage fender',(s*.62,.06,.75),(.36,1.90,.34),1)
        lathe('Contained industrial load capsule',[(0,.87),(.18,.87),(.25,1.02),(.25,1.43),(.21,1.58),(0,1.66)],(s*.28,.42,0),4,24)
        link('Load restraint',(s*.56,.07,.81),(s*.56,.73,1.28),.055)
    profile('Frontal impact ram',(0,-.92,.61),(1.14,.43,.53),2)
    fin('Ram attack edge',[(-.53,-.96),(.53,-.96),(.39,-1.43),(-.39,-1.43)],.53,.22,1)
    core((0,-.69,.92),.17,6)
def magnetar(rig):
    legs(rig,.15)
    profile('Wide magnetic engine chassis',(0,.10,.88),(2.1,1.9,.55),0)
    for s,label in [(-1,'R'),(1,'L')]:
        profile('Massive separated induction shoulder',(s*.70,.25,1.27),(1.03,1.61,.63),2)
        profile('Elite rear cascading armor',(s*.69,.83,1.35),(.84,.57,.36),1)
        for y in (-.05,.17,.39,.61):
            plate('Induction bank heat exchanger',(s*1.16,y,1.32),(.14,.11,.32),0,.025,.008)
        weight(label+'_CLAW')
        link('Elite crushing arm',(s*.93,-.25,.91),(s*1.38,-1.12,.42),.15)
        profile('Heavy articulated forearm shield',(s*1.15,-.73,.76),(.60,.81,.37),2)
        fin('Magnetic anchoring claw',[(s*1.21,-.92),(s*1.48,-1.06),(s*1.46,-1.62),(s*1.09,-1.89),(s*1.25,-1.41)],.29,.22,10)
        weight('BODY')
        tube('Attached containment power service',[(s*.24,-.30,1.18),(s*.36,.0,1.45),(s*.64,.38,1.52)],.040,9,10)
    core((0,-.32,1.39),.285,6)
    profile('Rear magnetic bridge',(0,.61,1.18),(.50,.59,.30),0)
def boss(rig):
    profile('Arena machine pressure chassis',(0,.34,1.15),(2.75,2.90,.91),0)
    for b in rig.data.bones:
        if not b.name.startswith('SUPPORT_'):continue
        weight(b.name);p=Vector(b.head_local);s=1 if p.x>0 else -1
        knee=p+Vector((s*.77,.15,-.23));toe=knee+Vector((s*.47,.23,-.67));toe.z=.15
        link('Boss upper load strut',p,knee,.18);link('Boss ground actuator',knee,toe,.13)
        profile('Heavy joint plated housing',tuple(knee+Vector((0,0,.12))),(.48,.68,.39),2)
        profile('Arena load foot',tuple(toe),(.48,.75,.25),1)
    weight('BODY')
    for s,label in [(-1,'R'),(1,'L')]:
        weight(label+'_SHUTTER')
        profile('Curved moving reactor shutter',(s*.88,.46,2.05),(1.12,1.83,1.06),2)
        for y in (.18,.42,.66,.90):plate('Reactor heat vent',(s*1.34,y,2.19),(.14,.14,.42),15,.025,.006)
        profile('Independent upper reactor cap',(s*.91,.64,2.62),(.88,1.13,.37),1)
        weight(label+'_WEAPON')
        p=Vector(rig.data.bones[label+'_WEAPON'].head_local)
        end=Vector((s*1.95,-1.58,.69));link('Boss sweep weapon drive',p,end,.16)
        fin('Charged sweep cutter',[(s*1.44,-.75),(s*2.09,-1.16),(s*2.15,-1.78),(s*1.72,-2.16),(s*1.81,-1.52)],.67,.31,1)
        tube('Weapon active edge',[(s*1.78,-1.05,.85),(s*1.97,-1.48,.85),(s*1.83,-1.91,.80)],.045,5,10)
    weight('BODY')
    profile('Offset rear command tower',(.83,1.10,2.20),(.77,.89,.90),0)
    profile('Command tower armored canopy',(.83,1.14,2.63),(.91,1.02,.30),2)
    plate('Command tower sensor slit',(.83,.65,2.55),(.40,.034,.105),5,.02,.008)
    # Dorsal containment remains visible from the fixed portrait camera even
    # when the authored rear command housing faces the player during reset.
    core((0,.80,2.37),.48)
    weight('ROTOR_CAGE')
    for i in range(6):
        a=i*math.tau/6;x,y=.57*math.cos(a),.80+.57*math.sin(a)
        tube('Attached reactor cage vane',[(x,y,2.08),(x*1.1,(y-.80)*1.1+.80,2.33),(x,y,2.57)],.066,10,12)
    weight('BODY')

def export(name,folder,rig):
    # Per-part planar atlas coordinates have gutters and retain the authored PBR.
    for o in parts:
        uv=o.data.uv_layers.active or o.data.uv_layers.new(name='Atlas')
        lo=[min(v.co[i] for v in o.data.vertices) for i in range(3)];hi=[max(v.co[i] for v in o.data.vertices) for i in range(3)]
        for f in o.data.polygons:
            tile=f.material_index;axis=max(range(3),key=lambda k:abs(f.normal[k]));axes=[k for k in range(3) if k!=axis]
            for li in f.loop_indices:
                v=o.data.vertices[o.data.loops[li].vertex_index].co
                u=(v[axes[0]]-lo[axes[0]])/max(.001,hi[axes[0]]-lo[axes[0]]);w=(v[axes[1]]-lo[axes[1]])/max(.001,hi[axes[1]]-lo[axes[1]])
                uv.data[li].uv=((tile%4+.018+.964*u)/4,(tile//4+.018+.964*w)/4)
            f.material_index=0
        o.data.materials.clear()
    atlas=bpy.data.materials.new('V47_AuthoredActorSurface');atlas.use_nodes=True
    nodes=atlas.node_tree.nodes;links=atlas.node_tree.links;bs=next(n for n in nodes if n.type=='BSDF_PRINCIPLED')
    for suffix,slot in [('BaseColor','Base Color'),('EmissionRed' if name!='Magnetar_V1' else 'Emission','Emission Color')]:
        t=nodes.new('ShaderNodeTexImage');t.image=bpy.data.images.load(str(ROOT/'Assets/_Game/Content/SurfaceHeroV46/Textures'/('V46_'+suffix+'.png')));links.new(t.outputs['Color'],bs.inputs[slot])
    bs.inputs['Metallic'].default_value=.75;bs.inputs['Roughness'].default_value=.42;bs.inputs['Emission Strength'].default_value=1.4
    for o in parts:o.data.materials.append(atlas)
    # Editable parts are kept; joined LODs are the reviewed runtime output.
    source=bpy.data.collections.new('Editable manufactured parts');bpy.context.scene.collection.children.link(source)
    for o in parts:
        q=o.copy();q.data=o.data.copy();source.objects.link(q);q.hide_render=True;q.hide_set(True)
    bpy.ops.object.select_all(action='DESELECT')
    for o in parts:o.select_set(True)
    bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name=name+'_LOD0'
    active(o);m=o.modifiers.new('Runtime triangles','TRIANGULATE');bpy.ops.object.modifier_apply(modifier=m.name)
    meshes=[o]
    for level,ratio in [(1,.52),(2,.23)]:
        q=o.copy();q.data=o.data.copy();bpy.context.collection.objects.link(q);q.name=name+'_LOD'+str(level);active(q)
        m=q.modifiers.new('Camera-distance simplification','DECIMATE');m.ratio=ratio;m.use_collapse_triangulate=True;bpy.ops.object.modifier_apply(modifier=m.name);meshes.append(q)
    for q in meshes:
        m=q.modifiers.new('Existing rigid mechanical skin','ARMATURE');m.object=rig;q.parent=rig
    bpy.context.scene.frame_set(1)
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/(name+'.blend')))
    bpy.ops.object.select_all(action='DESELECT');rig.select_set(True)
    for q in meshes:q.select_set(True)
    target=ROOT/'Assets/_Game/Content'/folder/'Models'/(name+'.fbx')
    runtime_target=target
    if '--staging' in sys.argv:
        target=ROOT/'.utmp/v47/actors'/folder/(name+'.fbx');target.parent.mkdir(parents=True,exist_ok=True)
    bpy.ops.export_scene.fbx(filepath=str(target),use_selection=True,object_types={'MESH','ARMATURE'},global_scale=1,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',mesh_smooth_type='FACE',use_tspace=True,add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,path_mode='STRIP')
    metrics[name]={'triangles':[len(q.data.polygons) for q in meshes],'editableParts':len(parts),'rigBones':len(rig.data.bones),'runtimeModel':runtime_target.relative_to(ROOT).as_posix(),'runtimeSha256':hashlib.sha256(target.read_bytes()).hexdigest(),'source':f'art/concept-convergence-v47/{name}.blend','preservedRigAndActions':True,'atlas':'V46 4x4 authored PBR','license':'Original project-owned geometry'}

for name,folder,fn in [('Scout_V1','VisualSlice',scout),('Cutter_V1','VisualSlice',cutter),('Warden_V1','Chapter01Production',warden),('ArcDrone_V1','Chapter01Production',drone),('Carrier_V1','Chapter01Production',carrier),('Magnetar_V1','VisualSlice',magnetar),('Custodian_V3','Chapter01V3',boss)]:
    bpy.ops.wm.open_mainfile(filepath=str(ROOT/'art/visual-replacement-v3'/(name+'.blend')))
    rig=next(o for o in bpy.data.objects if o.type=='ARMATURE')
    old_bones={b.name:tuple(b.head_local) for b in rig.data.bones}
    for o in list(bpy.data.objects):
        if o.type=='MESH':bpy.data.objects.remove(o,do_unlink=True)
    parts=[];mats=[];weight('BODY')
    for i,c in enumerate(PALETTE):
        m=bpy.data.materials.new('V47SurfaceTile_%02d'%i);m.diffuse_color=(*(x/255 for x in c),1);mats.append(m)
    fn(rig);assert old_bones=={b.name:tuple(b.head_local) for b in rig.data.bones}
    export(name,folder,rig);print('V47_ACTOR',name,metrics[name]['triangles'])
(DOC/'actor_authoring.json').write_text(json.dumps(metrics,indent=2))

