"""Original deterministic mechanical art. Blender 5.2, no downloaded geometry.
Authoring meters, Z up, -Y forward. Unity FBX export Y up, +Z gameplay forward.
"""
import bpy, bmesh, math, json
from pathlib import Path
from mathutils import Vector, Matrix

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Assets/_Game/Content/VisualSlice'
SOURCE = ROOT / 'art/first-visual-slice'
for p in [OUT/'Models', OUT/'Textures', SOURCE]: p.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
palette = [(.055,.071,.086),(.30,.39,.44),(.64,.74,.77),(.13,.18,.21),
           (.46,.29,.10),(.70,.028,.014),(1,.30,.025),(.015,.52,.67),(.025,.035,.043)]
names = ['graphite','machined steel','pale armor','floor','hazard ochre','hostile red','elite amber','service cyan','recess']
mats=[]
for i,c in enumerate(palette):
    m=bpy.data.materials.new(names[i]); m.diffuse_color=(*c,1); mats.append(m)
# Shared palette atlas, deliberately broad finish variation, no fine noise.
N=256
for label in ['BaseColor','Emission','MetallicSmoothness']:
    im=bpy.data.images.new('Slice_'+label,width=N,height=N,alpha=True)
    px=[]
    for y in range(N):
        for x in range(N):
            i=min(8,x//28); c=palette[i]
            if label=='BaseColor':
                grain=1+.025*math.sin(y*.13)*math.sin(x*.31)
                v=tuple(max(0,min(1,q*grain)) for q in c)+(1,)
            elif label=='Emission': v=(*c,1) if i in (5,6,7) else (0,0,0,1)
            else: v=(.68 if i in (1,2) else .28,0,0,.38 if i in (1,2) else .22)
            px.extend(v)
    im.pixels.foreach_set(px); im.filepath_raw=str(OUT/'Textures'/('Slice_'+label+'.png')); im.file_format='PNG'; im.save()
atlas=bpy.data.materials.new('Slice_IndustrialAtlas'); atlas.use_nodes=True
shader=atlas.node_tree.nodes.get('Principled BSDF')
for label,slot in [('BaseColor','Base Color'),('Emission','Emission Color')]:
    node=atlas.node_tree.nodes.new('ShaderNodeTexImage');node.image=bpy.data.images['Slice_'+label]
    atlas.node_tree.links.new(node.outputs['Color'],shader.inputs[slot])
shader.inputs['Metallic'].default_value=.45;shader.inputs['Roughness'].default_value=.68
shader.inputs['Emission Strength'].default_value=1.1
parts=[];bones={}; metrics={}

def active(o):
    bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o

def finish(o,name,mat,bone='BODY',bevel=.018):
    o.name=name;active(o);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel:
        b=o.modifiers.new('Manufactured chamfer','BEVEL');b.width=bevel;b.segments=1
        bpy.ops.object.modifier_apply(modifier=b.name)
    for f in o.data.polygons:f.use_smooth=True
    o.data.set_sharp_from_angle(angle=math.radians(38))
    n=o.modifiers.new('Weighted hard surface normals','WEIGHTED_NORMAL');n.keep_sharp=True
    bpy.ops.object.modifier_apply(modifier=n.name)
    # Bake coordinates so rigid weights and all meshes share one space.
    o.data.transform(o.matrix_world);o.matrix_world=Matrix.Identity(4)
    o.data.materials.append(mats[mat]);g=o.vertex_groups.new(name=bone);g.add(list(range(len(o.data.vertices))),1,'REPLACE')
    parts.append(o);return o

def box(name,c,s,mat=0,bone='BODY',angle=0,bevel=.018):
    bpy.ops.mesh.primitive_cube_add(size=1,location=c);o=bpy.context.object;o.scale=s;o.rotation_euler.z=angle
    return finish(o,name,mat,bone,bevel)

def rod(name,a,b,r,mat=1,bone='BODY',vertices=12):
    a=Vector(a);b=Vector(b);d=b-a
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=r,depth=d.length,location=(a+b)/2)
    o=bpy.context.object;o.rotation_euler=d.to_track_quat('Z','Y').to_euler()
    return finish(o,name,mat,bone,min(r*.12,.018))

def ring(name,c,r,thick,mat=1,bone='BODY',axis=(0,0,1)):
    bpy.ops.mesh.primitive_torus_add(major_segments=20,minor_segments=6,location=c,major_radius=r,minor_radius=thick)
    o=bpy.context.object;o.rotation_euler=Vector(axis).to_track_quat('Z','Y').to_euler()
    return finish(o,name,mat,bone,0)

def shell(name,c,w,d,h,mat=2,bone='BODY',nose=.6):
    # Octagonal swept shell with broad supported shoulder bevels; forward is -Y.
    x,y,z=c;outline=[(-w*.32,-d*.5),(w*.32,-d*.5),(w*.5,-d*.22),(w*.5,d*.24),
                       (w*.29,d*.5),(-w*.29,d*.5),(-w*.5,d*.24),(-w*.5,-d*.22)]
    v=[(x+a,y+b,z-h*.5) for a,b in outline]+[(x+a*.80,y+b*.86,z+h*.5+(nose if b<0 else 0)*h*.12) for a,b in outline]
    f=[tuple(range(7,-1,-1)),tuple(range(8,16))]+[(i,(i+1)%8,(i+1)%8+8,i+8) for i in range(8)]
    me=bpy.data.meshes.new(name);me.from_pydata(v,[],f);me.update();o=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(o)
    bm=bmesh.new();bm.from_mesh(me);bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bm.to_mesh(me);bm.free()
    return finish(o,name,mat,bone,.015)

def blade(name,side,bone):
    # Large curved chopping jaw: 1.45m long, explicit swept cutting edge.
    pts=[(.52,-.15),(.88,-.38),(1.03,-.89),(.92,-1.46),(.62,-1.93),(.70,-1.20),(.54,-.70)]
    v=[(side*x,y,z) for z in (.35,.51) for x,y in pts];n=len(pts)
    f=[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    me=bpy.data.meshes.new(name);me.from_pydata(v,[],f);me.update();o=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(o)
    bm=bmesh.new();bm.from_mesh(me);bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bm.to_mesh(me);bm.free()
    finish(o,name,2,bone,.012)
    rod(name+' cutting energy channel',(side*.91,-.86,.515),(side*.82,-1.39,.515),.024,5,bone,8)

def skin_mesh(name,rig=None,lod=False):
    # UV coordinates address a broad palette strip, reused intentionally across parts.
    for o in parts:
        uv=o.data.uv_layers.new(name='IndustrialPalette')
        idx=mats.index(o.data.materials[0]);u=(idx*28+14)/N
        for f in o.data.polygons:
            for loop in f.loop_indices:
                p=o.data.vertices[o.data.loops[loop].vertex_index].co
                uv.data[loop].uv=(u+.022*math.sin(p.x*2.7),.12+.75*(.5+.5*math.sin(p.z*1.3+p.y*.7)))
        o.data.materials.clear();o.data.materials.append(atlas)
    bpy.ops.object.select_all(action='DESELECT')
    for o in parts:o.select_set(True)
    bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name=name+'_LOD0' if lod else name
    tr=o.modifiers.new('Deterministic triangles','TRIANGULATE');bpy.ops.object.modifier_apply(modifier=tr.name)
    meshes=[o]
    if lod:
        for level,ratio in [(1,.56),(2,.29)]:
            q=o.copy();q.data=o.data.copy();bpy.context.collection.objects.link(q);q.name=name+'_LOD'+str(level)
            active(q);mod=q.modifiers.new('Mobile LOD','DECIMATE');mod.ratio=ratio;mod.use_collapse_triangulate=True;bpy.ops.object.modifier_apply(modifier=mod.name);meshes.append(q)
    for q in meshes:
        if rig:
            for v in q.data.vertices:
                if v.groups:
                    g=max(v.groups,key=lambda a:a.weight).group
                    for vg in q.vertex_groups:vg.remove([v.index])
                    q.vertex_groups[g].add([v.index],1,'REPLACE')
            a=q.modifiers.new('Rigid mechanical articulation','ARMATURE');a.object=rig;q.parent=rig
        else:q.vertex_groups.clear()
    metrics[name]={'triangles':[len(q.data.polygons) for q in meshes], 'parts_before_merge':len(parts),'materials':1,'rig_bones':len(bones) if rig else 0}
    return meshes

def bone(name,c,parent='BODY'):
    bones[name]=(c,parent)

def rig_and_export(name):
    arm=bpy.data.armatures.new(name+'_MechanicalRig');rig=bpy.data.objects.new(name+'_Rig',arm);bpy.context.collection.objects.link(rig)
    active(rig);bpy.ops.object.mode_set(mode='EDIT')
    for key,(p,parent) in bones.items():
        b=arm.edit_bones.new(key);b.head=p;b.tail=Vector(p)+Vector((0,0,.18))
        if parent:b.parent=arm.edit_bones[parent]
    bpy.ops.object.mode_set(mode='OBJECT');meshes=skin_mesh(name,rig,True)
    rig.animation_data_create();bpy.context.scene.render.fps=30
    for state,end in [('Idle',61),('Run',25),('Attack',22),('Hit',13),('Death',31)]:
        act=bpy.data.actions.new(state);act.use_fake_user=True;rig.animation_data.action=act
        for f in range(1,end+1):
            u=(f-1)/(end-1);q=math.sin(u*math.tau)
            for i,pb in enumerate(rig.pose.bones):
                pb.rotation_mode='XYZ';pb.rotation_euler=(0,0,0);pb.location=(0,0,0)
                if pb.name=='BODY':
                    if state=='Idle':pb.rotation_euler.x=q*.008
                    if state=='Run':pb.location.y=abs(q)*.027;pb.rotation_euler.x=-.045
                    if state=='Attack':pb.rotation_euler.x=math.sin(u*math.pi)*-.20;pb.location.y=math.sin(u*math.pi)*-.05
                    if state=='Hit':pb.rotation_euler.x=math.sin(u*math.pi)*.17
                    if state=='Death':pb.rotation_euler.x=u*.30;pb.rotation_euler.z=u*.14;pb.location.y=-u*.27
                elif 'HIP' in pb.name:
                    phase=u*math.tau+(math.pi if i%2 else 0)
                    if state=='Run':pb.rotation_euler.x=math.sin(phase)*.19;pb.rotation_euler.z=math.cos(phase)*.11
                    if state=='Death':pb.rotation_euler.x=u*.26
                elif 'KNEE' in pb.name:
                    if state=='Run':pb.rotation_euler.x=max(0,math.sin(u*math.tau+i*math.pi))*.28
                    if state=='Death':pb.rotation_euler.x=-u*.45
                elif 'BLADE' in pb.name or 'CLAW' in pb.name:
                    if state=='Attack':pb.rotation_euler.z=math.sin(u*math.pi)*(.43 if pb.name.startswith('L') else -.43)
                    if state=='Idle':pb.rotation_euler.z=q*.008
                    if state=='Death':pb.rotation_euler.x=u*.35
                elif pb.name=='CORE' and state=='Attack':pb.rotation_euler.z=u*.7
                pb.keyframe_insert('rotation_euler',frame=f);pb.keyframe_insert('location',frame=f)
    rig.animation_data.action=bpy.data.actions.get('Idle');bpy.context.scene.frame_set(1)
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/(name+'.blend')))
    bpy.ops.object.select_all(action='DESELECT');rig.select_set(True)
    for q in meshes:q.select_set(True)
    bpy.ops.export_scene.fbx(filepath=str(OUT/'Models'/(name+'.fbx')),use_selection=True,object_types={'MESH','ARMATURE'},
        add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,
        bake_anim_simplify_factor=0,path_mode='STRIP')

def reset():
    global parts,bones
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
    for a in list(bpy.data.actions):bpy.data.actions.remove(a)
    parts=[];bones={};bone('ROOT',(0,0,0),None);bone('BODY',(0,0,.5),'ROOT')

def legs(count,reach,height,thickness):
    for i in range(count):
        a=math.tau*i/count+math.pi/6;s=1 if math.cos(a)>0 else -1
        hip=(math.cos(a)*reach*.34,math.sin(a)*reach*.35,height)
        knee=(math.cos(a)*reach*.79,math.sin(a)*reach*.86,height*.85)
        toe=(math.cos(a)*reach,math.sin(a)*reach,.055)
        H='HIP_'+str(i);K='KNEE_'+str(i);bone(H,hip);bone(K,knee,H)
        rod('Shoulder drive '+str(i),hip,knee,thickness,0,H)
        rod('Tibial actuator '+str(i),knee,toe,thickness*.58,1,K)
        ring('Exposed knee bearing '+str(i),knee,thickness*1.5,thickness*.31,1,H,(1,0,0))
        shell('Upper limb guard '+str(i),((hip[0]+knee[0])/2,(hip[1]+knee[1])/2,height+.065),thickness*2.5,thickness*4,.07,2,H)
        rod('Ground claw '+str(i),toe,(toe[0]+s*.09,toe[1]-.14,.025),thickness*.55,0,K)
        if count==6 and reach>1:
            rod('Heavy support piston '+str(i),(hip[0],hip[1],height-.18),(knee[0],knee[1],height*.67),thickness*.40,1,H)
            shell('Segmented armored foot '+str(i),(toe[0],toe[1],.13),.36,.55,.20,2,K)

reset();legs(6,.91,.48,.053)
shell('Recon tapered chassis',(0,.04,.51),.63,.81,.20,0)
shell('Narrow dorsal carapace',(0,.11,.64),.48,.58,.12,2)
bone('CORE',(0,-.22,.58));ring('Recessed hostile optic',(0,-.28,.58),.11,.035,1,'CORE',(0,-1,0))
rod('Small hostile core',(0,-.30,.58),(0,-.33,.58),.075,5,'CORE')
for s in (-1,1):
    shell('Forward swept sensor '+str(s),(s*.20,-.40,.51),.13,.46,.09,2,nose=1)
    rod('Fine articulated antenna '+str(s),(s*.12,.25,.64),(s*.20,.51,.83),.014,1)
rig_and_export('Scout_V1')

reset();legs(4,1.08,.55,.105)
shell('Predator mechanical chassis',(0,.05,.54),1.14,1.33,.30,0)
shell('Frontal layered armor',(0,-.10,.77),1.22,1.05,.21,2)
shell('Rear heat armor',(0,.57,.65),.76,.55,.19,1)
for s,label in [(-1,'R'),(1,'L')]:
    B=label+'_BLADE';bone(B,(s*.57,-.20,.46));blade('Major cutting jaw '+label,s,B)
    ring('Cutting jaw rotary drive '+label,(s*.60,-.22,.48),.16,.047,1,B)
    rod('Forward blade actuator '+label,(s*.41,.08,.58),(s*.73,-.60,.48),.055,1,B)
    for i in range(4):box('Rear exhaust louvre '+label+str(i),(s*.29,.46+i*.09,.785),(.20,.034,.038),8)
bone('CORE',(0,-.32,.76));rod('Contained red core',(0,-.33,.76),(0,-.39,.76),.11,5,'CORE')
ring('Core inset steel bezel',(0,-.39,.76),.15,.039,0,'CORE',(0,1,0))
rig_and_export('Cutter_V1')

reset();legs(6,1.72,.88,.16)
shell('Broad elite lower chassis',(0,0,.82),2.17,1.87,.43,0)
shell('Heavy layered glacis',(0,-.30,1.04),2.11,1.44,.30,2)
shell('Upper containment crown',(0,.20,1.28),1.43,1.15,.23,1)
bone('CORE',(0,-.32,1.19))
ring('Central energy containment outer',(0,-.55,1.15),.41,.092,0,'CORE',(0,-.45,1))
ring('Central energy containment rim',(0,-.55,1.15),.30,.034,1,'CORE',(0,-.45,1))
rod('Contained amber energy',(0,-.56,1.10),(0,-.58,1.22),.27,6,'CORE',20)
for s,label in [(-1,'R'),(1,'L')]:
    B=label+'_CLAW';bone(B,(s*.93,-.25,.91))
    shell('Heavy forearm shield '+label,(s*1.02,-.53,1.03),.67,.91,.24,2,B)
    shell('Glacis shoulder tier '+label,(s*.85,.30,1.18),.74,1.09,.25,2)
    rod('Broad crushing mandible '+label,(s*1.13,-.42,.63),(s*1.36,-1.05,.39),.13,0,B)
    for j in range(3):shell('Rear armor cascade '+label+str(j),(s*.61,.58+j*.15,1.17-j*.07),.49,.26,.13,1)
    for j in range(4):box('Containment cooling fin '+label+str(j),(s*(.51+j*.105),-.35,1.35),(.043,.42,.16),0)
rig_and_export('Magnetar_V1')

# Environment kit: each FBX is one opaque atlas mesh, no runtime procedural primitives.
def export_static(name):
    mesh=skin_mesh(name)[0];active(mesh)
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/(name+'.blend')))
    bpy.ops.export_scene.fbx(filepath=str(OUT/'Models'/(name+'.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_anim=False,path_mode='STRIP')

reset()
box('Recessed deck foundation',(0,0,-.035),(3.96,3.96,.10),8,bevel=.006)
for s in (-1,1):
    shell('Broad chamfered deck '+str(s),(s*.95,0,.02),1.84,3.82,.04,3,nose=0)
    for j in range(3):box('Deck seam '+str(s)+str(j),(s*1.10,-1.15+j*1.15,.05),(1.45,.021,.012),1,bevel=0)
for j in range(10):box('Recessed service grate '+str(j),(0,-1.65+j*.36,.02),(.16,.21,.04),1,bevel=.003)
export_static('Deck_Module')

reset()
box('Wall loadbearing spine',(0,0,1.30),(3.96,.45,2.60),0)
for s in (-1,1):
    shell('Angled bulkhead armor '+str(s),(s*.95,-.28,1.12),1.78,.32,1.70,1)
    box('Raised wall cap '+str(s),(s*.95,-.25,2.16),(1.75,.52,.20),2)
for x in (-1.70,0,1.70):
    rod('Visible structural brace '+str(x),(x,-.42,.10),(x+.18,-.43,2.38),.083,0)
    box('Brace seat '+str(x),(x,-.35,.27),(.30,.52,.42),1)
rod('Wall service pipe',(-1.95,-.34,1.78),(1.95,-.34,1.78),.085,0)
box('Restrained service light',(0,-.49,1.93),(.76,.034,.05),7,.0 if False else 'BODY')
export_static('Bulkhead_Module')

reset()
shell('Reactor octagonal foundation',(0,0,.25),2.5,2.5,.5,0)
rod('Containment vessel',(0,0,.45),(0,0,3.3),.59,0,vertices=20)
rod('Industrial energy column',(0,0,.75),(0,0,2.83),.38,6,vertices=16)
for z in (.64,1.27,2.25,3.03):ring('Containment steel hoop '+str(z),(0,0,z),.72,.10,1)
for i in range(6):
    a=i*math.tau/6;x=.70*math.cos(a);y=.70*math.sin(a)
    rod('Reactor load spine '+str(i),(x,y,.42),(x,y,3.27),.105,1)
    shell('Radial reactor shield '+str(i),(x*1.09,y*1.09,1.58),.43,.36,1.11,2)
shell('Reactor crown',(0,0,3.41),1.8,1.8,.25,1)
for s in (-1,1):rod('Industrial feed coupling '+str(s),(s*.52,0,.55),(s*1.23,0,.55),.19,0)
export_static('Hero_Reactor')

reset()
shell('Power bank lower skid',(0,0,.15),1.25,2.8,.30,0)
for y in (-.83,0,.83):
    rod('Capacitor pressure vessel '+str(y),(0,y,.30),(0,y,1.46),.34,1,vertices=16)
    ring('Capacitor insulated collar '+str(y),(0,y,1.22),.35,.055,0)
    box('Amber status window '+str(y),(0,y-.345,1.0),(.14,.02,.12),6)
shell('Armored power canopy',(0,0,1.51),1.22,2.9,.20,2)
export_static('Power_Bank')

reset()
shell('Freight protective housing',(0,0,.64),1.45,1.65,1.2,1)
for x in (-.51,.51):box('Freight clamp '+str(x),(x,0,.70),(.13,1.78,1.3),0)
box('Industrial freight ID plate',(0,-.84,.87),(.55,.023,.22),4)
for i in range(3):box('Freight chevron '+str(i),(-.18+i*.18,-.857,.87),(.064,.02,.16),8,angle=-.40)
export_static('Freight_Container')

reset()
shell('Pump cast base',(0,0,.12),1.25,1.80,.24,0)
rod('Machinery pump motor',(0,-.45,.69),(0,.52,.69),.40,1,vertices=16)
for y in (-.32,-.12,.08,.28):ring('Motor cooling fins '+str(y),(0,y,.69),.43,.035,0,axis=(0,1,0))
shell('Pump terminal housing',(.42,.32,1.03),.40,.52,.31,2)
rod('Output riser',(0,.57,.69),(0,.57,1.72),.13,0)
rod('Feed manifold',(0,.57,1.72),(.53,.57,1.72),.13,1)
export_static('Coolant_Pump')

reset()
for z in (.72,1.09,1.46):
    rod('Conduit pressure line '+str(z),(-1.9,0,z),(1.9,0,z),.11,0)
    for x in (-1.40,1.40):ring('Pipeline flange '+str(z)+str(x),(x,0,z),.16,.038,1,axis=(1,0,0))
for x in (-1.65,1.65):box('Manifold structural support '+str(x),(x,0,.79),(.16,.50,1.57),1)
export_static('Conduit_Rack')

reset()
shell('Service station base',(0,0,.13),1.1,1.2,.26,0)
shell('Maintenance cabinet',(0,.12,.85),.93,.70,1.3,1)
box('Recessed maintenance screen',(0,-.25,1.18),(.48,.037,.28),8)
box('Restrained cyan display',(0,-.272,1.18),(.34,.014,.055),7)
rod('Robot service arm',(.36,.10,1.48),(.62,-.36,1.92),.063,1)
rod('Robot service tool',(.62,-.36,1.92),(.30,-.63,1.65),.056,0)
export_static('Maintenance_Station')

reset()
shell('Support plinth',(0,0,.22),.90,.90,.44,0)
rod('Load support column',(0,0,.39),(0,0,2.72),.16,1)
shell('Structural capital',(0,0,2.75),.74,.74,.25,2)
rod('Diagonal load brace',(0,0,1.54),(.53,0,2.52),.082,0)
box('Support warning insert',(0,-.20,1.01),(.19,.03,.42),4)
export_static('Structural_Support')

reset()
shell('Armored barrier shoe',(0,0,.18),3.8,.74,.36,0)
shell('Angled armored barricade',(0,0,.73),3.6,.55,.85,1)
box('Barrier pale cap',(0,-.01,1.19),(3.5,.34,.12),2)
for x in (-1.28,-.85,-.42,0,.42,.85,1.28):box('Barrier warning slash '+str(x),(x,-.291,.85),(.18,.023,.24),4,angle=.43)
export_static('Barrier_Module')

reset()
# Central gate leaf exactly occupies the existing 5m x 2.5m authority barrier.
for s in (-1,1):
    shell('Gate heavy sliding leaf '+str(s),(s*1.23,0,1.20),2.46,.36,2.36,1)
    for z in (.38,1.1,1.87):box('Gate reinforcing rail '+str(s)+str(z),(s*1.23,-.24,z),(2.18,.18,.16),0)
    rod('Gate hydraulic lock '+str(s),(s*1.91,-.25,.32),(s*1.91,-.25,2.14),.072,1)
    box('Containment seam marker '+str(s),(s*.14,-.26,1.21),(.055,.03,1.55),6)
export_static('Containment_Gate')
(ROOT/'docs/first-visual-slice').mkdir(parents=True,exist_ok=True)
(ROOT/'docs/first-visual-slice/asset_metrics.json').write_text(json.dumps(metrics,indent=2))
print('VISUAL_SLICE_ASSETS_COMPLETE',json.dumps(metrics))
