"""Original Blender production assets, metres/Z-up/-Y-forward. No external geometry.

Authored swept shells, cast support profiles, damaged deck polygons and curved
services are exported offline. Unity never constructs these visible meshes.
"""
import bpy, bmesh, math, json, random
from pathlib import Path
from mathutils import Vector, Matrix

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Assets/_Game/Content/ConceptFidelityV2'
SOURCE = ROOT / 'art/concept-fidelity-v2'
for p in (OUT/'Models', OUT/'Textures', SOURCE): p.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.context.scene.unit_settings.system = 'METRIC'
bpy.context.scene.unit_settings.scale_length = 1
palette = [(.075,.105,.13),(.36,.46,.51),(.66,.72,.70),(.13,.19,.22),
           (.75,.48,.11),(1,.12,.045),(1,.52,.055),(.08,.86,.98),
           (.022,.030,.036),(.18,.105,.065),(.22,.72,.38),(.42,.18,.66),
           (.16,.30,.34),(.54,.36,.12),(.28,.32,.33),(.065,.09,.105)]
names = ['graphite enamel','brushed steel','worn pale armor','structural deck',
         'service paint','contained red','contained amber','contained cyan',
         'rubber recess','burned metal','infrastructure green','violet field',
         'cool painted housing','worn hazard','damaged alloy','deck cavity']
mats = []
for name,c in zip(names,palette):
    m=bpy.data.materials.new(name); m.diffuse_color=(*c,1); mats.append(m)
atlas=bpy.data.materials.new('Fidelity_IndustrialAtlas'); atlas.use_nodes=True
shader=atlas.node_tree.nodes.get('Principled BSDF')
N=512
# Broad scratches, oxidised weld regions and brushed response remain visible at
# mobile distance. Each family has its own metal/smoothness range and emission.
metal=[.65,.92,.52,.72,.25,0,0,0,0,.15,0,0,.5,.3,.78,.55]
smooth=[.33,.65,.35,.28,.31,.8,.8,.8,.12,.08,.6,.6,.3,.16,.19,.12]
for label in ('BaseColor','Emission','MetallicSmoothness'):
    im=bpy.data.images.new('Fidelity_'+label,width=N,height=N,alpha=True); px=[]
    for y in range(N):
        for x in range(N):
            idx=(x//128)+(y//128)*4; u=(x%128)/127; v=(y%128)/127; c=palette[idx]
            wave=math.sin(u*11+v*4)*math.cos(v*8-u*3)
            scratch=abs(math.sin(v*46+u*6))<.055 and math.sin(u*29+v*5)>.5
            stain=max(0,math.sin(u*7+v*9)-.55)*.38
            edge=min(u,1-u,v,1-v)<.028
            if label=='BaseColor':
                k=1+.10*wave-stain
                if scratch and idx not in (5,6,7,10,11): k+=.22
                if edge and idx in (0,2,3,12,14): k+=.16
                px.extend((*[min(1,max(0,q*k)) for q in c],1))
            elif label=='Emission':
                k=(.7+.3*math.sin(v*math.pi)) if idx in (5,6,7,10,11) else 0
                px.extend((*[q*k for q in c],1))
            else:
                px.extend((metal[idx],0,0,max(.04,min(.9,smooth[idx]+.09*wave-stain))))
    im.pixels.foreach_set(px); im.filepath_raw=str(OUT/'Textures'/('Fidelity_'+label+'.png')); im.file_format='PNG'; im.save()
    if label in ('BaseColor','Emission'):
        node=atlas.node_tree.nodes.new('ShaderNodeTexImage'); node.image=im
        atlas.node_tree.links.new(node.outputs['Color'],shader.inputs['Base Color' if label=='BaseColor' else 'Emission Color'])
shader.inputs['Metallic'].default_value=.6; shader.inputs['Roughness'].default_value=.42
shader.inputs['Emission Strength'].default_value=3
parts=[];bones={};metrics={}
# Reuse established export conventions, not existing geometry.
helper=(ROOT/'Tools/first-visual-slice/build_assets.py').read_text()
exec(helper[helper.index('def active('):helper.index('reset();legs(6,.91')])

def poly(name,outline,z,h,mat=3,bone='BODY',bevel=.025):
    n=len(outline);verts=[(x,y,z-h/2) for x,y in outline]+[(x,y,z+h/2) for x,y in outline]
    faces=[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    me=bpy.data.meshes.new(name);me.from_pydata(verts,[],faces);me.update()
    ob=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(ob)
    bm=bmesh.new();bm.from_mesh(me);bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bm.to_mesh(me);bm.free()
    return finish(ob,name,mat,bone,bevel)

def arc(name,c,rx,ry,inner,start,end,h,mat=2,bone='BODY',steps=18):
    # Open curved armor segment: a real nested shell, never a filled cylinder.
    x,y,z=c;pts=[]
    for i in range(steps+1):
        a=start+(end-start)*i/steps;pts.append((x+rx*math.cos(a),y+ry*math.sin(a)))
    for i in range(steps,-1,-1):
        a=start+(end-start)*i/steps;pts.append((x+(rx-inner)*math.cos(a),y+(ry-inner)*math.sin(a)))
    return poly(name,pts,z,h,mat,bone,min(.035,h*.15))

def hose(name,points,r=.06,mat=8,bone='BODY'):
    cu=bpy.data.curves.new(name,'CURVE');cu.dimensions='3D';cu.resolution_u=6;cu.bevel_depth=r;cu.bevel_resolution=1
    sp=cu.splines.new('BEZIER');sp.bezier_points.add(len(points)-1)
    for p,c in zip(sp.bezier_points,points):p.co=c;p.handle_left_type='AUTO';p.handle_right_type='AUTO'
    ob=bpy.data.objects.new(name,cu);bpy.context.collection.objects.link(ob);active(ob);bpy.ops.object.convert(target='MESH')
    return finish(bpy.context.object,name,mat,bone,0)

def dome(name,c,s,mat=2,bone='BODY'):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=24,ring_count=12,location=c)
    ob=bpy.context.object;ob.scale=s
    return finish(ob,name,mat,bone,0)

def skin_mesh(name,rig=None,lod=False):
    for ob in parts:
        idx=mats.index(ob.data.materials[0]); origin=Vector((0,0,0))
        for layer in list(ob.data.uv_layers):ob.data.uv_layers.remove(layer)
        uv=ob.data.uv_layers.new(name='FamilyAtlas')
        coords=[v.co for v in ob.data.vertices]
        mins=[min(p[a] for p in coords) for a in range(3)];maxs=[max(p[a] for p in coords) for a in range(3)]
        for f in ob.data.polygons:
            major=max(range(3),key=lambda a:abs(f.normal[a]));axes=[a for a in range(3) if a!=major]
            for loop in f.loop_indices:
                p=ob.data.vertices[ob.data.loops[loop].vertex_index].co
                a,b=axes;u=(p[a]-mins[a])/max(.001,maxs[a]-mins[a]);v=(p[b]-mins[b])/max(.001,maxs[b]-mins[b])
                uv.data[loop].uv=((idx%4+.07+.86*u)/4,(idx//4+.07+.86*v)/4)
        ob.data.materials.clear();ob.data.materials.append(atlas)
    bpy.ops.object.select_all(action='DESELECT')
    for ob in parts:ob.select_set(True)
    bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();ob=bpy.context.object;ob.name=name+'_LOD0' if lod else name
    active(ob);mod=ob.modifiers.new('Export triangles','TRIANGULATE');bpy.ops.object.modifier_apply(modifier=mod.name)
    meshes=[ob]
    if lod:
        for level,ratio in ((1,.52),(2,.24)):
            q=ob.copy();q.data=ob.data.copy();bpy.context.collection.objects.link(q);q.name=name+'_LOD'+str(level);active(q)
            dec=q.modifiers.new('Mobile silhouette LOD','DECIMATE');dec.ratio=ratio;dec.use_collapse_triangulate=True;bpy.ops.object.modifier_apply(modifier=dec.name);meshes.append(q)
    for q in meshes:
        if rig:
            mod=q.modifiers.new('Mechanical rigid skin','ARMATURE');mod.object=rig;q.parent=rig
        else:q.vertex_groups.clear()
    metrics[name]={'triangles':[len(q.data.polygons) for q in meshes],'materials':1,'authored_parts':len(parts),'bones':len(bones) if rig else 0}
    return meshes

def export(name,animated=False,states=None):
    rig=None
    if animated:
        arm=bpy.data.armatures.new(name+'_MechanicalRig');rig=bpy.data.objects.new(name+'_Rig',arm);bpy.context.collection.objects.link(rig);active(rig);bpy.ops.object.mode_set(mode='EDIT')
        for key,(p,parent) in bones.items():
            b=arm.edit_bones.new(key);b.head=p;b.tail=Vector(p)+Vector((0,0,.2))
            if parent:b.parent=arm.edit_bones[parent]
        bpy.ops.object.mode_set(mode='OBJECT')
    meshes=skin_mesh(name,rig,animated)
    if animated:
        rig.animation_data_create();bpy.context.scene.render.fps=30
        for state,end in (states or [('Idle',91),('Run',31),('Attack',31),('Windup',31),('Release',16),('Special',31),('Hit',13),('Death',61)]):
            act=bpy.data.actions.new(state);act.use_fake_user=True;rig.animation_data.action=act
            for f in range(1,end+1):
                u=(f-1)/(end-1);q=math.sin(u*math.tau);pulse=math.sin(u*math.pi)
                for i,pb in enumerate(rig.pose.bones):
                    pb.rotation_mode='XYZ';pb.rotation_euler=(0,0,0);pb.location=(0,0,0);pb.scale=(1,1,1)
                    if pb.name=='BODY':
                        if state=='Idle':pb.location.z=q*.012
                        if state=='Run':pb.location.z=abs(q)*.035;pb.rotation_euler.y=q*.015
                        if state in ('Attack','Windup'):pb.rotation_euler.x=-u*.065;pb.location.z=u*.06
                        if state=='Release':pb.rotation_euler.x=pulse*.10;pb.location.z=-pulse*.10
                        if state=='Special':pb.location.z=pulse*.14
                        if state=='Hit':pb.rotation_euler.y=pulse*.045
                        if state=='Death':pb.location.z=-u*.42;pb.rotation_euler.x=u*.13;pb.rotation_euler.y=u*.09
                    elif 'ROTOR' in pb.name:
                        pb.rotation_euler.z=u*math.tau if state in ('Idle','Run','Special') else pulse*1.2
                    elif 'SHUTTER' in pb.name:
                        s=1 if pb.name.startswith('L') else -1
                        if state in ('Windup','Attack'):pb.rotation_euler.y=s*u*.32
                        if state=='Release':pb.rotation_euler.y=s*(1-u)*.32
                        if state=='Special':pb.rotation_euler.y=s*pulse*.42
                        if state=='Death':pb.rotation_euler.y=s*u*.62
                    elif 'WEAPON' in pb.name:
                        s=1 if pb.name.startswith('L') else -1
                        if state in ('Windup','Attack'):pb.rotation_euler.z=s*u*.20;pb.rotation_euler.x=-u*.14
                        if state=='Release':pb.rotation_euler.x=-.14+pulse*.32
                        if state=='Special':pb.rotation_euler.z=s*pulse*.44
                        if state=='Hit':pb.rotation_euler.x=pulse*.07
                        if state=='Death':pb.rotation_euler.x=u*.65
                    elif 'SUPPORT' in pb.name:
                        if state=='Run':pb.rotation_euler.y=math.sin(u*math.tau+i*math.pi)*.06
                        if state=='Death':pb.rotation_euler.x=u*.21
                    elif pb.name=='CORE':
                        if state in ('Idle','Special'):pb.scale=(1+q*.025,1+q*.025,1+q*.025)
                        if state=='Windup':pb.scale=(1+u*.08,)*3
                        if state=='Death':pb.scale=(max(.04,1-u),)*3
                    pb.keyframe_insert('rotation_euler',frame=f);pb.keyframe_insert('location',frame=f);pb.keyframe_insert('scale',frame=f)
        rig.animation_data.action=bpy.data.actions.get('Idle');bpy.context.scene.frame_set(1)
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/(name+'.blend')),compress=True)
    bpy.ops.object.select_all(action='DESELECT')
    for ob in meshes:ob.select_set(True)
    if rig:rig.select_set(True)
    bpy.context.view_layer.objects.active=rig or meshes[0]
    bpy.ops.export_scene.fbx(filepath=str(OUT/'Models'/(name+'.fbx')),use_selection=True,object_types={'MESH','ARMATURE'} if rig else {'MESH'},
        global_scale=1,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',mesh_smooth_type='FACE',use_tspace=True,
        add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=animated,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,path_mode='STRIP')

def deck_cells(seeds):
    """Clip irregular large maintenance plates; no uniform rectangular lattice."""
    cells=[]
    for sx,sy in seeds:
        points=[(-3,-3),(3,-3),(3,3),(-3,3)]
        for tx,ty in seeds:
            if(sx,sy)==(tx,ty):continue
            nx,ny=tx-sx,ty-sy;limit=(tx*tx+ty*ty-sx*sx-sy*sy)/2
            output=[]
            for a,b in zip(points,points[1:]+points[:1]):
                da=a[0]*nx+a[1]*ny-limit;db=b[0]*nx+b[1]*ny-limit
                if da<=0:output.append(a)
                if (da<=0)!=(db<=0):
                    u=da/(da-db);output.append((a[0]+u*(b[0]-a[0]),a[1]+u*(b[1]-a[1])))
            points=output
            if not points:break
        if points:
            cx=sum(x for x,y in points)/len(points);cy=sum(y for x,y in points)/len(points)
            cells.append([(cx+(x-cx)*.993,cy+(y-cy)*.993) for x,y in points])
    return cells

def bolts(cx,cy,z,r,number=8,bone='BODY'):
    for i in range(number):
        a=i*math.tau/number;rod('Recessed flange fastener',(cx+r*math.cos(a),cy+r*math.sin(a),z),(cx+r*math.cos(a),cy+r*math.sin(a),z+.035),.045,1,bone,6)

# Custodian V2: split curved containment cradle, four broad cast rocker supports,
# asymmetrical clamp and pressure-lance. Top aperture reads at gameplay camera.
reset();bone('BODY',(0,0,1.05),'ROOT');bone('CORE',(0,-.20,1.93));bone('ROTOR_CAGE',(0,-.20,1.93))
shell('Armored mobile reactor keel',(0,.25,.73),2.8,2.45,.63,0)
arc('Cast horseshoe cradle',(0,.12,1.28),1.65,1.43,.33,math.radians(-35),math.radians(225),.45,14)
for s,label in ((-1,'R'),(1,'L')):
    B=label+'_SHUTTER';bone(B,(s*1.0,.27,1.50))
    start,end=(-.70,.95) if s==1 else (math.pi-.95,math.pi+.70)
    arc('Overlapping curved pale containment shell',(0,.18,1.82),1.66,1.5,.50,start,end,.58,2,B)
    arc('Raised machined armor lip',(0,.18,2.12),1.64,1.48,.14,start+.05,end-.08,.11,1,B)
    arc('Dark separation beneath shell',(0,.18,1.50),1.76,1.57,.22,start+.1,end-.05,.13,0,B)
    for a in (start+.2,start+.6,end-.25):
        x=1.64*math.cos(a);y=.18+1.48*math.sin(a)
        shell('Individual scarred shell latch',(x,y,2.09),.30,.38,.15,14,B)
    hose('Exposed curved shutter hydraulic',[(s*.52,.9,1.36),(s*1.18,.78,1.43),(s*1.42,.27,1.57)],.07,8,B)
ring('Deep reactor aperture mechanical bezel',(0,-.20,1.77),.89,.12,0)
ring('Aperture machined inner seat',(0,-.20,1.81),.68,.05,1)
rod('Power located in recessed cavity',(0,-.20,1.40),(0,-.20,1.79),.57,5,'CORE',24)
dome('Contained bright reactor heart',(0,-.20,1.82),(.43,.43,.25),5,'CORE')
ring('Internal rotating energy cage',(0,-.20,1.95),.62,.037,1,'ROTOR_CAGE')
for i in range(8):
    a=i*math.tau/8;rod('Opaque reactor shielding rib',(.76*math.cos(a),-.2+.76*math.sin(a),1.64),(.52*math.cos(a),-.2+.52*math.sin(a),2.15),.044,0,'ROTOR_CAGE')
shell('Rear pressure manifold',(0,1.09,1.74),1.72,.77,.61,0)
for x in (-.56,0,.56):
    rod('Rear heat exchanger',(x,1.20,1.45),(x,1.20,2.47),.17,1)
    for z in (1.59,1.82,2.06,2.29):ring('Heat exchanger cast ribs',(x,1.20,z),.20,.035,0)
    dome('Pressure end cap',(x,1.20,2.48),(.18,.18,.12),2)
for i,(s,y) in enumerate(((-1,-.63),(1,-.53),(-1,.87),(1,1.02))):
    B='SUPPORT_'+str(i);bone(B,(s*1.02,y,1.03))
    p=[(s*1.0,y-.22),(s*1.35,y-.34),(s*2.19,y-.18),(s*2.45,y+.27),(s*2.19,y+.60),(s*1.78,y+.35),(s*1.37,y+.21)]
    if s<0:p.reverse()
    poly('Swept cast rocker support',p,.72,.37,14,B,.04)
    rod('Massive rotary support bearing',(s*1.17,y,.88),(s*1.17,y,1.16),.30,0,B,24)
    ring('Support bearing steel rim',(s*1.17,y,1.15),.24,.047,1,B)
    rod('Support polished hydraulic',(s*1.33,y,1.00),(s*2.09,y+.22,.43),.085,1,B)
    rod('Support piston sleeve',(s*1.65,y+.10,.77),(s*2.11,y+.23,.40),.14,0,B)
    shell('Broad load spreading foot',(s*2.15,y+.25,.24),.81,.97,.29,2,B)
    hose('Rocker rubber service loop',[(s*1.13,y+.14,1.16),(s*1.60,y+.46,1.04),(s*2.12,y+.37,.46)],.055,8,B)
bone('L_WEAPON',(1.35,-.58,1.31));bone('R_WEAPON',(-1.33,-.57,1.31))
# One side is a long pressure cannon, the other a crescent industrial clamp.
rod('Lance rotator drive',(1.22,-.59,1.34),(1.57,-.59,1.34),.35,0,'L_WEAPON',24)
ring('Lance rotary flange',(1.59,-.59,1.34),.29,.054,1,'L_WEAPON',(1,0,0))
rod('Contained pressure lance',(1.74,-.76,1.36),(1.74,-2.01,1.36),.27,1,'L_WEAPON',20)
rod('Lance dark muzzle shroud',(1.74,-1.70,1.36),(1.74,-2.12,1.36),.32,0,'L_WEAPON',20)
ring('Recessed weapon muzzle',(1.74,-2.14,1.36),.23,.052,1,'L_WEAPON',(0,1,0))
rod('Lance internal red source',(1.74,-2.10,1.36),(1.74,-2.13,1.36),.17,5,'L_WEAPON',20)
shell('Pressure lance sloped armor',(1.74,-1.11,1.63),.67,1.12,.23,2,'L_WEAPON')
rod('Clamp articulated servo',(-1.2,-.59,1.34),(-1.62,-1.08,1.13),.25,0,'R_WEAPON',20)
arc('Industrial crescent restraint claw',(-1.71,-1.4,1.18),.60,.76,.19,math.radians(40),math.radians(310),.30,2,'R_WEAPON')
rod('Exposed clamp actuator',(-1.31,-.53,1.64),(-1.81,-1.22,1.47),.083,1,'R_WEAPON')
hose('Heavy asymmetric supply trunk',[(-.42,1.03,1.59),(-1.5,1.22,1.75),(-1.76,.1,1.46),(-1.64,-.78,1.35)],.11,8)
for x in (-.42,.13):shell('Unequal rear maintenance plate',(x,1.34,2.0),.43,.55,.11,13 if x<0 else 2)
export('Custodian_V2',True)

# Repair machine components have native metre-length pivots for behavior-driven
# articulation. No meshes or materials are constructed at runtime.
reset();arc('Service dais rounded shell',(0,0,.04),2.55,2.35,.28,0,math.tau,.13,2,steps=32)
for start,end in ((.1,1.37),(1.61,2.87),(3.15,4.45),(4.73,6.15)):
    arc('Recessed cyan service ring',(0,0,.075),2.16,1.96,.065,start,end,.035,7)
poly('Asymmetric dock plating',[(-1.57,-1.58),(.64,-1.81),(1.63,-.68),(1.21,1.54),(-.51,1.77),(-1.75,.58)],.04,.1,3)
for x in (-.72,.64):
    for y in (-1.3,-.9,-.5,.7,1.1):box('Service drainage slot',(x,y,.11),(.31,.085,.025),8,bevel=.006)
shell('Rear asymmetric power housing',(-1.14,1.83,.53),1.34,.73,.84,12)
rod('Repair condenser',(-1.14,1.80,.66),(-1.14,1.80,1.31),.22,1)
ring('Condenser cyan inset',(-1.14,1.80,1.17),.23,.036,7)
hose('Dock bundled supply',[(-1.24,1.8,.4),(-2.3,1.4,.18),(-2.45,.25,.18)],.09,8)
export('Repair_Platform_V2')
reset();rod('Cast shoulder pedestal',(0,0,.08),(0,0,.63),.34,0,vertices=24)
shell('Angled shoulder riser',(0,.035,.82),.48,.53,1.20,2)
ring('Actual rotary servo',(0,0,1.23),.25,.08,1,axis=(1,0,0))
rod('Shoulder motor casing',(-.28,0,1.23),(.28,0,1.23),.20,0,vertices=20)
hose('Shoulder flexible service',[(.22,.24,.25),(.35,.34,.7),(.21,.24,1.21)],.045,8)
export('Repair_Pedestal_V2')
for name,width in (('Repair_Upper_V2',.24),('Repair_Forearm_V2',.17)):
    reset();shell('Tapered cast arm',(0,0,.5),width,.25,1,2)
    rod('Exposed polished linear actuator',(-width*.57,-.09,.08),(-width*.57,-.09,.93),.035,1)
    rod('Protected hydraulic sleeve',(-width*.57,-.09,.09),(-width*.57,-.09,.5),.051,0)
    hose('Flexible arm hose',[(width*.58,.09,.09),(width*.9,.15,.4),(width*.64,.1,.91)],.028,8)
    for z in (.14,.81):ring('Cast arm servo seat',(0,0,z),width*.52,.027,1,axis=(1,0,0))
    export(name)
reset();rod('Elbow rotary motor',(-.19,0,0),(.19,0,0),.17,0,vertices=24)
ring('Elbow machined joint',(0,0,0),.18,.04,1,axis=(1,0,0));export('Repair_Joint_V2')
reset();shell('Tool protective shell',(0,.06,0),.29,.42,.25,12)
ring('Tool optical barrel',(0,-.16,0),.12,.043,0,axis=(0,1,0))
rod('Recessed bright tool source',(0,-.15,0),(0,-.19,0),.085,7,vertices=16)
for x in (-.13,.13):rod('Welding electrode',(x,-.10,-.08),(x,-.32,-.10),.023,1)
export('Repair_Tool_V2')
reset();arc('Contained scanner aperture',(0,0,0),.82,.82,.025,0,math.tau,.022,7,steps=32);export('Repair_Scanner_V2')

# Three deck variants: broken chamfered polygons, irregular seams, removable
# service plates, patch welds and a curved utility trunk. One opaque mesh each.
for variant in range(3):
    reset();rng=random.Random(810+variant)
    poly('Base structural deck',[(-3,-3),(3,-3),(3,3),(-3,3)],-.035,.09,15,bevel=.008)
    seeds=[(-2.35,-1.75),(.85,-2.38),(2.48,-.38),(-1.52,.27),(.49,.39),(-2.15,2.46),(1.16,2.19)]
    seeds=[(x+rng.uniform(-.34,.34),y+rng.uniform(-.37,.37)) for x,y in seeds]
    for index,pts in enumerate(deck_cells(seeds)):
        poly('Irregular fitted structural plating',pts,.025,.08,3 if index%4 else 14,bevel=.009)
    if variant!=1:
        poly('Welded overlapping maintenance patch',[(-.53,-.41),(.49,-.46),(.64,.31),(-.42,.57)],.079,.025,14,bevel=.007)
        for a in (-.38,.41):rod('Visible patch weld',(a,-.30,.101),(a+.04,.29,.101),.012,1,vertices=6)
    if variant==0:
        box('Recessed service cavity',(1.94,-1.2,.076),(.48,1.29,.027),8,angle=.21,bevel=.009)
        for j in range(6):box('Flush drainage cover',(1.94,-1.73+j*.20,.091),(.43,.045,.014),14,angle=.21,bevel=.004)
        for j in range(3):box('Local service hazard marking',(2.22-j*.20,-2.3,.082),(.11,.48,.008),13,angle=-.35,bevel=0)
    if variant==1:
        hose('Exposed recessed deck services',[(-2.6,1.22,.071),(-1.65,1.31,.074),(-.75,1.10,.076),(.2,1.52,.076),(1.65,1.68,.073)],.022,8)
        poly('Irregular scorched maintenance area',[(1.32,-2.18),(1.92,-2.34),(2.32,-1.86),(2.10,-1.35),(1.55,-1.51)],.079,.004,9,bevel=0)
    if variant==2:
        poly('Partly removed access hatch',[(-2.27,.98),(-1.21,1.03),(-1.06,2.15),(-2.30,2.06)],.079,.021,8,bevel=.007)
        hose('Conduit inside missing hatch',[(-2.09,1.12,.095),(-1.48,1.32,.092),(-1.24,1.88,.094)],.027,1)
        poly('Displaced hatch cover',[(-2.75,2.38),(-1.74,2.27),(-1.51,2.94),(-2.83,2.96)],.09,.019,14,bevel=.006)
    export('Deck_V2_'+str(variant))

reset()
for j in range(3):
    hose('Swept wall floor pressure line',[(-2.3,-.4+j*.19,.15),(-1.5,-.5+j*.20,.23),(-.6,-.15+j*.18,.65),(.3,.20+j*.15,1.45),(1.3,.18+j*.18,1.72),(2.4,-.12+j*.17,1.6)],.09 if j==0 else .045,1 if j==0 else 8)
    ring('Bent conduit coupling',(.3,.20+j*.15,1.45),.11 if j==0 else .06,.018,0,axis=(1,0,1))
shell('Service channel armor',(-1.7,0,.18),1.1,.8,.25,12)
box('Inset active infrastructure',(-1.71,-.35,.19),(.58,.055,.05),7)
export('Curved_Services_V2')

reset();poly('Chamfered bulkhead foot',[(-2.9,-.65),(2.9,-.65),(3.08,.11),(2.7,.53),(-2.62,.5),(-3.05,-.05)],.14,.28,0)
for x in (-1.9,0,1.85):
    shell('Sloped nested wall shell',(x,.13,1.28),1.65,.61,1.97,12 if x==0 else 2)
    rod('Pale cast structural rib',(x-.70,-.22,.18),(x-.54,-.10,2.68),.13,1)
    shell('Separate overlapping damaged armor',(x,-.25,.87),1.49,.16,.55,14)
hose('Curved exposed bulkhead service',[(-2.5,-.36,2.18),(-1.3,-.48,2.06),(.1,-.43,2.32),(1.8,-.47,2.19),(2.55,-.28,1.20)],.085,8)
box('Recessed wall lamp',(.58,-.38,1.65),(.63,.07,.19),8)
box('Contained wall amber light',(.58,-.43,1.65),(.41,.02,.075),6)
export('Bulkhead_V2')

def pressure_machine(name,energy=6,turbine=False):
    reset();bone('ROTOR_PRESSURE',(0,0,1.6))
    shell('Asymmetric reactor foundation',(0,0,.20),3.1,3.4,.4,14)
    arc('Curved protective vessel cradle',(0,0,1.15),1.10,1.15,.27,-.77,4.34,1.28,0)
    rod('Internal recessed energy column',(0,0,.55),(0,0,2.30),.52,energy,vertices=24)
    arc('Split curved pressure crown',(0,0,2.51),1.01,1.01,.30,-.40,3.82,.34,2)
    ring('Protected crown aperture',(0,0,2.56),.80,.10,1)
    dome('Recessed luminous pressure source',(0,0,2.22),(.42,.42,.17),energy)
    for x in (-.29,.29):
        rod('Opaque crown shielding cage',(x,-.53,2.1),(x,.53,2.56),.027,0)
    for z in (.55,1.08,1.93,2.27):ring('Industrial vessel armor hoop',(0,0,z),.92,.09,1)
    for i in range(6):
        a=i*math.tau/6;x=.90*math.cos(a);y=.90*math.sin(a)
        rod('Outer containment mechanical rib',(x,y,.6),(x,y,2.32),.09,0)
    for s in (-1,1):
        hose('Curved high pressure bypass',[(s*.47,.65,2.51),(s*1.18,.89,2.20),(s*1.5,.41,1.1),(s*1.73,-.65,.18)],.11,1)
        shell('Bolted bypass foot',(s*1.62,-.56,.24),.63,.76,.40,12)
    shell('Offset control enclosure',(-1.12,-.77,.87),.86,.70,1.14,12)
    shell('Removed machinery access cover',(1.34,.94,.30),.94,1.17,.15,2)
    if turbine:
        for i in range(6):
            a=i*math.tau/6
            poly('Curved turbine paddle',[(.19*math.cos(a),.19*math.sin(a)),(.65*math.cos(a+.12),.65*math.sin(a+.12)),(.79*math.cos(a+.38),.79*math.sin(a+.38)),(.31*math.cos(a+.45),.31*math.sin(a+.45))],2.7,.075,1,'ROTOR_PRESSURE',.012)
    else:
        arc('Rotating opaque diagnostic ring',(0,0,2.02),.68,.68,.04,0,math.tau, .08,1,'ROTOR_PRESSURE')
    bolts(0,0,2.67,.72)
    export(name,True, [('Idle',121)])
pressure_machine('Reactor_V2',6)
pressure_machine('Turbine_V2',7,True)
pressure_machine('Containment_Vessel_V2',5)

reset();shell('Arena pressure-frame footing',(0,0,.25),2.1,1.9,.5,0)
for s in (-1,1):
    rod('Slanted load-bearing cast column',(s*.58,0,.34),(s*.35,0,3.98),.22,1,vertices=16)
    shell('Overlapping column armor',(s*.51,-.14,1.35),.53,.47,1.57,2)
    hose('Heavy bowed containment services',[(s*.8,.13,.3),(s*1.03,.29,1.5),(s*.83,.24,2.77),(s*.34,.10,3.92)],.12,8)
shell('Containment-frame upper crosshead',(0,0,4.0),1.88,1.03,.38,14)
ring('Recessed high voltage junction',(0,-.55,3.24),.39,.10,0,axis=(0,1,0))
rod('Contained warning source',(0,-.53,3.24),(0,-.57,3.24),.24,5,vertices=20)
export('Containment_Frame_V2')

reset();arc('Broken curved transport pressure shell',(0,0,1.12),1.29,1.84,.26,-.33,3.72,1.44,2)
for y in (-1.31,0,1.30):
    ring('Freight drum reinforcement',(0,y,1.07),.78,.10,1,axis=(0,1,0))
    for s in (-1,1):rod('Freight bogie',(s*.87,y,.2),(s*1.1,y,.2),.25,0,vertices=16)
hose('Hanging displaced supply',[(-1.2,.6,1.6),(-1.82,.3,.84),(-1.66,-1.27,.15),(-.74,-1.9,.10)],.075,8)
shell('Detached armor patch',(1.24,-1.25,.13),1.24,1.14,.17,14)
export('Pressure_Wreck_V2')

(ROOT/'docs/concept-fidelity-v2').mkdir(parents=True,exist_ok=True)
(ROOT/'docs/concept-fidelity-v2/asset_metrics.json').write_text(json.dumps(metrics,indent=2))
print('CONCEPT_FIDELITY_BLENDER_ASSETS_COMPLETE',json.dumps(metrics))
