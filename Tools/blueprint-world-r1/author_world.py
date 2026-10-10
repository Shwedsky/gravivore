"""Author eight industrial sectors from inspected CC0 parts and bespoke geometry.

Coordinates in this source are Blender x/y/z; Unity uses x/z/y. No actors, UI,
combat data, colliders, or imported vendor scripts are authored here.
"""
import bpy, bmesh, math, json, random
from pathlib import Path
from mathutils import Vector, Matrix
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/_Game/Content/BlueprintWorldR1/Models'
SOURCE=ROOT/'art/blueprint-world-r1'
OUT.mkdir(parents=True,exist_ok=True);SOURCE.mkdir(parents=True,exist_ok=True)
LAYOUT=json.loads((Path(__file__).parent/'layout.json').read_text())
random.seed(48)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
for material in list(bpy.data.materials): bpy.data.materials.remove(material)
MATERIALS={}
for name in ['R1_Trim1','R1_Trim2','R1_Trim3','R1_Industrial']:
    mat=bpy.data.materials.new(name);mat.use_nodes=True
    node=mat.node_tree.nodes.new('ShaderNodeTexImage')
    image_path=ROOT/'Assets/_Game/Content/BlueprintWorldR1/Textures'/f'{name}_BaseColor.png'
    node.image=bpy.data.images.load(str(image_path))
    shader=mat.node_tree.nodes.get('Principled BSDF') or mat.node_tree.nodes.new('ShaderNodeBsdfPrincipled')
    output=mat.node_tree.nodes.get('Material Output') or mat.node_tree.nodes.new('ShaderNodeOutputMaterial')
    mat.node_tree.links.new(shader.outputs['BSDF'],output.inputs['Surface'])
    mat.node_tree.links.new(node.outputs['Color'],shader.inputs['Base Color'])
    shader.inputs['Metallic'].default_value=.7;shader.inputs['Roughness'].default_value=.4
    MATERIALS[name]=mat
DONORS={}
PALETTE=bpy.data.images.load(str(ROOT/'ExternalAssetIntake/Current/molten-maps-scifi/inspection/Gradient Texture Atlas by Imphenzia.png'))
palette_pixels=list(PALETTE.pixels)

def bounds(objects):
    points=[o.matrix_world@v.co for o in objects for v in o.data.vertices]
    return Vector([min(v[i] for v in points) for i in range(3)]),Vector([max(v[i] for v in points) for i in range(3)])

def atlas_uv(obj,swatch,polygons=None):
    uv=obj.data.uv_layers.active or obj.data.uv_layers.new(name='UVMap')
    for face in (obj.data.polygons if polygons is None else polygons):
        verts=[obj.data.vertices[obj.data.loops[i].vertex_index].co for i in face.loop_indices]
        axes=sorted(range(3),key=lambda i:abs(face.normal[i]))[:2]
        lows=[min(v[i] for v in verts) for i in axes];highs=[max(v[i] for v in verts) for i in axes]
        for loop,vert in zip(face.loop_indices,verts):
            # A mapped 2D panel field preserves surface grain and recessed seams.
            u=(vert[axes[0]]-lows[0])/max(.01,highs[0]-lows[0])
            v=(vert[axes[1]]-lows[1])/max(.01,highs[1]-lows[1])
            uv.data[loop].uv=((swatch%4+(u*.90+.05))/4,(1-swatch//4+(v*.90+.05))/4+.5)
            # Texture has two used rows at the TOP of a 4x4 image.
    return obj

def custom_material(obj,swatch):
    obj.data.materials.clear();obj.data.materials.append(MATERIALS['R1_Industrial'])
    atlas_uv(obj,swatch)
    return obj

def classify(face,obj,emissive=False):
    uv=obj.data.uv_layers.active
    if uv:
        co=sum((uv.data[i].uv for i in face.loop_indices),Vector((0,0)))/len(face.loop_indices)
        x=max(0,min(PALETTE.size[0]-1,int(co.x*PALETTE.size[0])));y=max(0,min(PALETTE.size[1]-1,int(co.y*PALETTE.size[1])))
        index=(y*PALETTE.size[0]+x)*4;r,g,b=palette_pixels[index:index+3]
    else:r,g,b=.2,.2,.2
    if emissive:return 5 if b>r*1.15 else 6 if g>r*.28 else 7
    if max(r,g,b)<.15:return 0
    if r>g*1.3 and r>b*1.4:return 3
    if max(r,g,b)>.62:return 4
    return 1 if max(r,g,b)>.32 else 2

for p in sorted((ROOT/'ExternalAssetIntake/Current').glob('*/inspection/*.fbx')):
    before=set(bpy.context.scene.objects)
    bpy.ops.import_scene.fbx(filepath=str(p))
    imported=[o for o in bpy.context.scene.objects if o not in before]
    meshes=[o for o in imported if o.type=='MESH']
    for obj in meshes:
        world=obj.matrix_world.copy();obj.parent=None;obj.matrix_world=world
        bpy.context.view_layer.objects.active=obj;obj.select_set(True)
        bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
        obj.select_set(False)
        if 'molten' in p.parents[1].name:
            swatches=[classify(f,obj,'Emissive' in obj.data.materials[f.material_index].name) for f in obj.data.polygons]
            obj.data.materials.clear();obj.data.materials.append(MATERIALS['R1_Industrial'])
            for face,swatch in zip(obj.data.polygons,swatches):
                face.material_index=0;atlas_uv(obj,swatch,[face])
        else:
            for index,material in enumerate(list(obj.data.materials)):
                name=material.name if material else ''
                target='R1_Trim1' if 'Trim_01' in name else 'R1_Trim2' if 'Trim_02' in name else 'R1_Trim3' if 'Trim_03' in name else 'R1_Industrial'
                obj.data.materials[index]=MATERIALS[target]
                if target=='R1_Industrial':
                    atlas_uv(obj,5 if 'Screen' in name else 7 if 'Light' in name else 0,[f for f in obj.data.polygons if f.material_index==index])
    lo,hi=bounds(meshes)
    centre=Vector(((lo.x+hi.x)/2,(lo.y+hi.y)/2,lo.z))
    for obj in meshes:
        obj.location-=centre;obj.hide_render=True;obj.hide_set(True)
    DONORS[p.stem]=(meshes,hi-lo)
    for obj in imported:
        if obj.type!='MESH':bpy.data.objects.remove(obj,do_unlink=True)

parts=[];category='architecture'
def register(obj,name):
    # Localized Blender primitives may call their UV layer "UVКарта" while
    # donor FBXs use "UVMap". Joining by different names creates empty UV0
    # for half the geometry, so canonicalize before any mesh consolidation.
    uv=obj.data.uv_layers.active
    if uv is None:raise ValueError('Missing authored UVs: '+name)
    for other in list(obj.data.uv_layers):
        if other!=uv:obj.data.uv_layers.remove(other)
    uv.name='UVMap';uv.active_render=True
    obj.name=category+' / '+name;parts.append(obj);return obj

def donor(name,position,size=None,yaw=0):
    meshes,original=DONORS[name]
    scale=Vector((1,1,1)) if size is None else Vector([size[i]/max(original[i],.05) for i in range(3)])
    rot=Matrix.Rotation(math.radians(yaw),4,'Z')
    for source in meshes:
        obj=source.copy();obj.data=source.data.copy();bpy.context.collection.objects.link(obj)
        obj.hide_render=False;obj.hide_set(False)
        obj.matrix_world=Matrix.Translation(Vector(position))@rot@Matrix.Diagonal((*scale,1))@source.matrix_world
        register(obj,name)

def box(name,position,size,swatch=1,bevel=.08,yaw=0):
    bpy.ops.mesh.primitive_cube_add(size=1,location=position)
    obj=bpy.context.object;obj.dimensions=size;obj.rotation_euler.z=math.radians(yaw)
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel:
        mod=obj.modifiers.new('Machined edge radii','BEVEL');mod.width=bevel;mod.segments=2
        bpy.ops.object.modifier_apply(modifier=mod.name)
    obj.data.update();custom_material(obj,swatch);return register(obj,name)

def cylinder(name,position,radius,depth,swatch=1,vertices=48):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=radius,depth=depth,location=position)
    obj=bpy.context.object;mod=obj.modifiers.new('Machined shoulder','BEVEL');mod.width=min(.09,depth/5);mod.segments=2
    bpy.ops.object.modifier_apply(modifier=mod.name);obj.data.update();custom_material(obj,swatch)
    for face in obj.data.polygons:face.use_smooth=abs(face.normal.z)<.8
    return register(obj,name)

def chamber_shell(name,position,inner,outer,height,start,span,swatch=1):
    # Curved pressure-shell sectors have an actual wall thickness and a
    # machined edge radius. Gaps between shells expose the contained core.
    steps=12;verts=[]
    for z in [-height*.5,height*.5]:
        for radius in [inner,outer]:
            for i in range(steps+1):
                angle=math.radians(start+span*i/steps)
                verts.append((radius*math.cos(angle),radius*math.sin(angle),z))
    n=steps+1;faces=[]
    for i in range(steps):
        faces.extend([(i,i+1,2*n+i+1,2*n+i),
                      (n+i,3*n+i,3*n+i+1,n+i+1),
                      (i,n+i,n+i+1,i+1),
                      (2*n+i,2*n+i+1,3*n+i+1,3*n+i)])
    faces.extend([(0,2*n,3*n,n),(steps,n+steps,3*n+steps,2*n+steps)])
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update()
    obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj)
    obj.location=position;bpy.context.view_layer.objects.active=obj
    mod=obj.modifiers.new('Pressure-shell machined lips','BEVEL');mod.width=.065;mod.segments=2
    bpy.ops.object.modifier_apply(modifier=mod.name);obj.data.update();custom_material(obj,swatch)
    return register(obj,name)

def pipe(name,points,radius=.12,swatch=0):
    curve=bpy.data.curves.new(name,'CURVE');curve.dimensions='3D';curve.resolution_u=1
    curve.bevel_depth=radius;curve.bevel_resolution=2;curve.resolution_u=8
    spline=curve.splines.new('POLY');spline.points.add(len(points)-1)
    for p,co in zip(spline.points,points):p.co=(*co,1)
    obj=bpy.data.objects.new(name,curve);bpy.context.collection.objects.link(obj)
    bpy.context.view_layer.objects.active=obj;obj.select_set(True);bpy.ops.object.convert(target='MESH');obj.select_set(False)
    obj.data.update();custom_material(obj,swatch);return register(obj,name)

def deck(name,width,depth,swatch=2):
    global category
    category='floor'
    # Deep chamfered foundation, inset broad plates, two service strips and
    # recessed grates. The proportions vary by sector, no world tile loop.
    box(name+' foundation',(0,0,-.48),(width,depth,.9),0,.35)
    box(name+' deck substrate',(0,0,-.045),(width-.25,depth-.25,.08),2,.15)
    for x in [-width*.37,width*.37]:
        box('recessed utility trench',(x,0,-.013),(1.25,depth-.8,.035),0,.025)
        for y in range(int(-depth/2+1),int(depth/2),2):
            box('inset grate bridge',(x,y,.012),(1.05,.9,.025),1,.012)
    # Large unequal fitted panels with broad seams, stable flush combat plane.
    for x,y,w,d in [(-width*.18,-depth*.22,width*.32,depth*.43),(width*.18,-depth*.22,width*.32,depth*.43),(-width*.18,depth*.24,width*.32,depth*.44),(width*.18,depth*.24,width*.32,depth*.44)]:
        box('large flush fitted plate',(x,y,-.01),(w-.06,d-.06,.035),swatch,.035)
    for x in [-width*.47,width*.47]:
        box('structural deck rim',(x,0,-.14),(.22,depth-.6,.35),1,.05)
    category='architecture'

def wall_run(x,y,count,yaw=0,broken=False,height=4.5):
    for i in range(count):
        a=math.radians(yaw);p=(x-math.sin(a)*i*4,y+math.cos(a)*i*4,0)
        donor('WallAstra_Straight_Broken' if broken and i==count-1 else 'WallAstra_Straight',p,(1.4,4,height),yaw)
        donor('TopCables_Straight',(p[0],p[1],height),(1.25,4,1.35),yaw)
        donor('Column_Pipes',(p[0],p[1]-1.9,0),(1.1,1.1,height+1.15))

def service_wall(side,width,depth,energy=5):
    x=side*(width*.5-1.7)
    wall_run(x,-depth*.5+2,int((depth-1)/4),0,height=4.7)
    for y in [-depth*.26,depth*.26]:
        donor('Cryo Tube ON',(x-side*.25,y,.05),(1.65,1.65,4.8))
        box('service vessel crown',(x-side*.25,y,5.0),(2,2,.35),0,.12)
    pipe('high service header',[(x,-depth*.43,4.9),(x,depth*.42,4.9)],.20,1)

def bay(x,y,width=7,energy=6,yaw=0):
    # A broad deployment/fabrication cell with a real open mouth, roof mass,
    # rear services and an apron. Support objects live inside one footprint.
    donor('Door_Frame_A',(x,y,0),(width,1.4,5.3),yaw)
    box('deployment cell roof',(x,y+1.15,5.0),(width+1.1,3.5,.85),0,.24,yaw)
    box('stepped armor fascia',(x,y+.2,4.5),(width-.9,.9,.65),1,.12,yaw)
    box('energized inspection panel',(x,y-.48,3.9),(width*.53,.14,.16),energy,.03,yaw)
    for side in [-1,1]:
        box('cell armored jamb',(x+side*(width*.5-.3),y,2.1),(.8,1.55,4.2),1,.15)
        donor('Prop_Vent_Wide',(x+side*(width*.5-.3),y-.8,2.4),(1.0,.15,.35),90)
    donor('Wall Command',(x,y+2.1,.3),(width-.9,.35,3.6))
    for side in [-1,1]:
        pipe('exposed service returns',[(x+side*width*.38,y+1.8,.2),(x+side*width*.38,y+1.8,4.6),(x,y+1.8,4.6)],.12,0)

def vessel(x,y,height=5.4,energy=5):
    donor('Cryo Tube ON',(x,y,0),(2.1,2.1,height))
    # Imported vessel is reworked with containment fins, strapped returns and
    # a heavy crown, avoiding a naked stock-donor silhouette.
    for a in [45,135,225,315]:
        r=math.radians(a)
        box('containment armor fin',(x+1.03*math.cos(r),y+1.03*math.sin(r),height*.55),(.26,.7,height*.72),1,.07,a)
    cylinder('pressure crown',(x,y,height+.13),1.28,.30,0)
    for side in [-1,1]:
        pipe('pressure return',[(x+side*1.1,y,.45),(x+side*1.35,y,.7),(x+side*1.35,y,height-.5),(x+side*1.05,y,height-.1)],.08,energy)

def repair():
    deck('semicircular maintenance base',22,20)
    service_wall(-1,22,20);service_wall(1,22,20)
    category_set('machinery')
    donor('Platform_Round1',(0,0,.012),(8,8,.08))
    for x in [-4.6,4.6]:
        donor('Command Console',(x,2,.06),(3.6,2.2,1.6),-25 if x<0 else 25)
        donor('Generator Pile Small',(x,-4,.05),(1.5,1.5,2.3))
    donor('Wall Command',(0,-8.0,.1),(13,.5,3.4),180)
    for x in [-5,0,5]:vessel(x,-8,4.9,5)
    pipe('repair hydraulic network',[(-8,5,3.8),(-8,-6,3.8),(0,-7,3.8),(8,-6,3.8),(8,5,3.8)],.17,1)

def relay():
    deck('assembly loading yard',24,18)
    service_wall(-1,24,18,6);service_wall(1,24,18,6)
    category_set('machinery')
    bay(-8,4.7,5.3,6);bay(8,4.7,5.3,6)
    for side in [-1,1]:
        donor('Generator Pile Small',(side*8,-4,.1),(2,2,3))
        donor('Command Console',(side*8,-.8,.03),(3,2,1.5),side*90)
        # Loading conveyor goes into the facility, kept behind collision.
        box('continuous assembly conveyor',(side*9,0,.35),(2,12,.6),0,.1)
        for y in [-5,-2,1,4]:box('conveyor segmented carrier',(side*9,y,.7),(1.7,2.8,.12),1,.04)
    pipe('communications backbone',[(-10,-6,4.6),(-10,6,4.6),(10,6,4.6),(10,-6,4.6)],.12,1)
    # Deployment originates from a building across the north service mouth;
    # the opening stays clear through to the first paired-sector junction.
    donor('Door_Frame_SquareTall',(0,5.3,0),(12.2,.8,4.2))
    for side in [-1,1]:
        donor('Column_Astra',(side*6.1,5.3,0),(1.3,1.5,4.5))
        donor('Wall Command',(side*6.1,5.2,.4),(1.15,.35,2.9))
        donor('Prop_Vent_Wide',(side*6.1,4.75,2.7),(1.1,.25,.35),90)
        box('assembly cell signal',(side*6.1,4.7,3.2),(.12,.16,.7),6,.02)
    box('relay deployment building roof',(0,6.0,4.2),(13.1,2.6,.65),1,.18)
    box('relay continuous control fascia',(0,4.75,3.75),(10.0,.25,.45),0,.08)
    for x in [-4.5,-1.5,1.5,4.5]:
        box('deployment sequence indicator',(x,4.58,3.75),(.75,.08,.13),6,.015)

def capacitor():
    deck('capacitor distribution platform',20,20,1)
    service_wall(-1,20,20,5)
    category_set('machinery')
    for x,y,h in [(-7,-5,5.8),(-7,3,6.4),(-2.6,8,5.7),(3.5,8,5.7)]:vessel(x,y,h,5)
    donor('Generator',(0,8,.05),(5,3.6,3.2))
    pipe('contained power collector',[(-7,-5,5.9),(-7,6,5.9),(-2.6,8,5.9),(3.5,8,5.9)],.18,5)
    donor('Catwalk',(-5.5,0,2.5),(1.1,12,.8))
    for x in [-4,3]:box('distribution switching cabinet',(x,8.2,1.5),(2.2,2.5,3),0,.2)
    for x in [-4,3]:box('hostile switch status',(x,6.9,1.6),(1.1,.08,.22),7,.025)

def cutting():
    deck('heavy fabrication deck',20,20)
    service_wall(1,20,20,6)
    category_set('machinery')
    bay(0,8,10,7)
    # Heavy cutting frame with broad suspended ram and a heated inset workcell.
    for x in [-4.6,4.6]:
        box('press loadbearing cheek',(x,8,2.4),(1.3,2.8,4.8),0,.22)
        cylinder('press hydraulic piston',(x,6.9,3.2),.28,3.5,4)
    box('cutting press shoulder',(0,8,5.2),(11,3.2,1.1),1,.23)
    box('press ram housing',(0,7.5,3.9),(6,2.1,1.25),0,.17)
    box('heated cutting jaw',(0,6.6,3.3),(5.7,.25,.25),7,.04)
    donor('Generator Pile Large',(7.2,-4.5,.05),(3.4,3.4,4.3))
    for x in [6.3,7.8]:pipe('press hot return',[(x,-6,.7),(x,-6,4.8),(x,7,4.8)],.16,1)
    for x in [-4,4]:box('inset production rail',(x,0,.017),(.16,13,.035),4,.01)

def shield():
    deck('armor sorting platform',20,20,0)
    service_wall(-1,20,20)
    category_set('machinery')
    bay(0,8,10,6)
    for x in [-4,0,4]:
        box('armored shield storage shell',(x,8,2.2),(3.4,3.2,4.4),1,.32)
        box('recessed armor cell',(x,6.3,2.2),(2.45,.4,3.3),0,.16)
        box('shield inspection strip',(x,6.05,2.1),(.12,.08,2.3),5,.02)
        box('inclined sorted armor slab',(x,6.1,.7),(2.7,1.25,.3),4,.08,8)
    donor('Centrifuge',(-7,0,.02),(3.6,3.6,4))
    pipe('shield press feed',[(-7,-6,3.9),(-7,5,3.9),(4,8,3.9)],.18,1)

def hauler():
    deck('damaged freight loading apron',20,20)
    category_set('architecture')
    wall_run(8,-6,4,broken=True,height=4)
    category_set('machinery')
    # Asymmetric transport carcasses with tilted armored shells and exposed
    # ribbing; all large debris remains outside the central combat pocket.
    for x,y,w,d,a in [(-3,8,7,3.4,-7),(5,8,4.7,3.6,11),(8,-4,2.5,5.8,8)]:
        box('freight hull underframe',(x,y,.4),(w,d,.7),0,.2,a)
        box('damaged freight armored body',(x,y,1.9),(w-.25,d-.2,2.5),1,.32,a)
        box('freight lid split',(x+.25,y,3.2),(w-.4,d-.4,.4),0,.1,a+6)
        for offset in [-.32,0,.32]:
            box('freight external structural rib',(x+w*offset,y,2.0),(.18,d+.1,2.7),0,.04,a)
    for y in [-6,6]:
        box('loading crane column',(8,y,3.2),(.9,1.4,6.4),0,.17)
        donor('Column_Pipes',(8,y,.1),(1.05,1.05,6.5))
    box('loading bridge beam',(8,0,6.4),(1.4,14,.9),1,.18)
    donor('Catwalk',(7.2,0,4.7),(1,14,.7))
    pipe('damaged hydraulic service',[(8,-6,5.6),(8,0,5.6),(7.3,1.3,4.7),(6.9,1.4,3.2)],.14,1)
    for x in [-4,4]:box('flush freight guide rail',(x,0,.025),(.18,13,.04),4,.01)

def induction_machine(x,y,scale=1,energy=6):
    # A complete pressure chamber replaces the ordinary-sector generator:
    # load-bearing skirt, six curved shielding shells, exposed contained
    # energy, upper pressure head and paired laminated magnetic return yokes.
    cylinder('reactor loadbearing skirt',(x,y,.35*scale),3.65*scale,.7*scale,0)
    cylinder('lower pressure bearing',(x,y,.92*scale),3.25*scale,.48*scale,1)
    cylinder('contained reactor energy',(x,y,3.25*scale),1.4*scale,4.5*scale,energy)
    for angle in range(0,360,60):
        chamber_shell('curved reactor shielding',(x,y,3.35*scale),2.25*scale,2.95*scale,4.05*scale,angle+8,44,0)
        r=math.radians(angle+30)
        px=x+3.02*scale*math.cos(r);py=y+3.02*scale*math.sin(r)
        box('reactor inspection spine',(px,py,3.35*scale),(.26*scale,.48*scale,3.2*scale),1,.06,angle+30)
        pipe('pressure instrumentation',[(px,py,1.6*scale),(px,py,5.1*scale)],.055*scale,energy)
    cylinder('upper reactor pressure head',(x,y,5.7*scale),3.3*scale,.58*scale,1)
    cylinder('reactor crown gasket',(x,y,6.04*scale),2.85*scale,.18*scale,0)
    donor('Centrifuge',(x,y,6.15*scale),(3.6*scale,3.6*scale,1.1*scale))
    for side in [-1,1]:
        box('flux yoke foundation',(x+side*3.2*scale,y,.45*scale),(2.2*scale,4.4*scale,.9*scale),0,.2)
        box('articulated induction shoulder',(x+side*3.2*scale,y,4*scale),(1.1*scale,3.4*scale,5.4*scale),1,.17)
        for offset in [-1.25,-.55,.15,.85]:
            box('laminated flux comb',(x+side*2.7*scale,y+offset*scale,3.8*scale),(.45*scale,.32*scale,3.8*scale),0,.05)
        pipe('broad induction return',[(x+side*3.1*scale,y-1.9*scale,.8),(x+side*3.1*scale,y-1.9*scale,6.4*scale),(x+side*.8*scale,y-1.9*scale,6.4*scale)],.18*scale,energy)

def magnetar():
    deck('magnetic induction hall',50,20)
    category_set('architecture')
    wall_run(-23,-8,5,height=5.5);wall_run(23,-8,5,broken=True,height=5.5)
    for side in [-1,1]:
        # Strong ordinary occupants retain clear side commissioning aprons.
        bay(side*17,8,8,6)
        donor('Catwalk',(side*22,0,3.3),(1.6,15,1))
    category_set('machinery')
    induction_machine(-5,5,1,6)
    donor('Generator Pile Large',(-8.5,7,.05),(3.4,3.4,4.6))
    pipe('magnetic facility feed',[(-8.5,7,4.7),(-6,8.5,4.7),(-5,8.5,4.7),(-5,5,4.7)],.2,1)
    for x in [-5,5]:box('magnetic protected service strip',(x,-3,.016),(.45,11,.025),3,.01)

def custodian():
    deck('containment reactor court',50,26,1)
    category_set('architecture')
    wall_run(-23,-10,6,height=7);wall_run(23,-10,6,height=7)
    wall_run(-23,13,12,-90,height=6)
    for side in [-1,1]:
        bay(side*18,10,8,7)
        donor('Catwalk',(side*22,0,4.0),(1.5,21,1))
        for y in [-8,2,10]:
            donor('Column_Pipes',(side*23,y,0),(1.4,1.4,7.5))
    category_set('machinery')
    induction_machine(0,9,1.25,7)
    for x in [-7.5,7.5]:vessel(x,11,6.3,7)
    pipe('containment redundant coolant',[(-7.5,11,5),(-10,10,5),(-10,3,3),(-14,2,3)],.2,1)
    pipe('containment hot output',[(7.5,11,5),(10,10,5),(10,3,3),(14,2,3)],.2,7)
    # A broad segmented circular service apron frames the boss space; nothing
    # protrudes into the line/cone/circle telegraph surface.
    for a in range(0,360,30):
        r=math.radians(a);x=9*math.cos(r);y=9*math.sin(r)
        box('containment service inspection plate',(x,y,.021),(3.2,.72,.035),3,.018,a+90)

def category_set(value):
    global category
    category=value

def export(name,objects):
    bpy.ops.object.select_all(action='DESELECT')
    for o in objects:
        bm=bmesh.new();bm.from_mesh(o.data)
        bmesh.ops.triangulate(bm,faces=[f for f in bm.faces if len(f.verts)>3])
        bm.to_mesh(o.data);bm.free();o.data.update();o.select_set(True)
    bpy.context.view_layer.objects.active=objects[0]
    bpy.ops.export_scene.fbx(filepath=str(OUT/f'R1_{name}.fbx'),use_selection=True,object_types={'MESH'},apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',bake_anim=False,add_leaf_bones=False,use_mesh_modifiers=True,path_mode='STRIP',mesh_smooth_type='FACE',use_tspace=True)

manifest=[]
for sector,build in zip(LAYOUT['sectors'],[repair,relay,capacitor,cutting,shield,hauler,magnetar,custodian]):
    parts=[];build()
    # Fit architecture to the unchanged portrait camera's combat court.
    # Foundation dimensions stay authoritative; heavy assemblies move closer
    # to the court, with matching explicit footprints in layout.json.
    profiles={'repair-hub':(.70,.90,.85),'relay-yard':(.75,.85,.85),
              'capacitor-field':(.77,.74,.85),'cutting-floor':(.77,.74,.85),
              'shield-dump':(.77,.74,.85),'hauler-graveyard':(.77,.74,.85)}
    if sector['id'] in profiles:
        sx,sy,sz=profiles[sector['id']]
        fit=Matrix.Diagonal((sx,sy,sz,1))
        for obj in parts:
            if not obj.name.startswith('floor / '):obj.matrix_world=fit@obj.matrix_world
    # Keep culling at machinery/building/deck scale. Consolidate each category
    # in 5m longitudinal bands; shared materials do not flatten source UVs.
    groups={}
    for obj in parts:
        lo,hi=bounds([obj]);band=math.floor((lo.y+hi.y)*.1)
        groups.setdefault((obj.name.split(' / ')[0],band),[]).append(obj)
    merged=[]
    for (kind,band),objects in groups.items():
        bpy.ops.object.select_all(action='DESELECT')
        for obj in objects:obj.hide_set(False);obj.select_set(True)
        bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join()
        obj=bpy.context.object;obj.name=f'{sector["model"]}_{kind}_{band}'
        bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
        merged.append(obj)
    export(sector['model'],merged)
    # Authoritative editable source contains the whole eight-sector world.
    origin=sector['center']
    for obj in merged:obj.location+=Vector((origin['x'],origin['z'],0))
    manifest.append(dict(id=sector['id'],model=sector['model'],renderers=len(merged),triangles=sum(len(f.vertices)-2 for o in merged for f in o.data.polygons)))
    for obj in merged:obj.hide_set(True)

# Dedicated bridges, service islands and permanent gate frames. They are new
# geometry rather than reuse of the previous global repeated deck.
parts=[];category_set('floor')
for y,length in [(-20,3),(0,94)]:
    if length==94:
        for a,b in [(-32,-2),(-2,30),(30,62)]:
            box('continuous spine substrate',(0,(a+b)*.5,-.4),(8,b-a,.8),0,.12)
        for a,b in [(-20,-2),(-2,20),(20,42),(42,64)]:
            box('long spine service plate',(0,(a+b)*.5,-.02),(7.4,b-a-.08,.04),2,.025)
        for x in [-3.35,3.35]:
            for a,b in [(-30.5,-2),(-2,30),(30,60.5)]:
                box('recessed spine services',(x,(a+b)*.5,-.01),(.34,b-a,.04),0,.02)
            for a,b in [(-28,-2),(-2,30),(30,60)]:
                pipe('protected spine cable',[(x,a,-.04),(x,b,-.04)],.08,1)
for y in [10,32]:
    box('sector crossing foundation',(0,y,-.25),(50,8,.5),0,.18)
    box('sector crossing flush deck',(0,y,-.02),(50,7.8,.04),2,.02)
box('containment bridge substrate',(0,65,-.3),(8,10,.6),0,.13)
box('containment approach deck',(0,65,-.02),(7.8,10,.04),1,.02)
category_set('architecture')
for side in [-1,1]:
    wall_run(side*7,17,3,broken=side==1,height=4)
    donor('Generator Pile Small',(side*7,21,.1),(2,2,4.1))
for y in [44,64]:
    donor('Door_Frame_A',(0,y,0),(9.1,.75,3.8))
    for x in [-4.9,4.9]:
        donor('Column_Pipes',(x,y,0),(1.4,1.4,4.1))
        box('transition gate heavy shoulder',(x,y,1.75),(1.5,1.2,3.5),0,.18)
    box('transition gate crest',(0,y,3.8),(11,.75,.3),1,.10)
for volume in LAYOUT['blockers']:
    if not volume['id'].startswith('platform-edge/'):continue
    p=volume['center'];s=volume['size'];length=max(s['x'],s['z'])
    axis='x' if s['x']<s['z'] else 'y'
    dims=(.13,length,.13) if axis=='x' else (length,.13,.13)
    box('major platform safety rail',(p['x'],p['z'],.85),dims,1,.03)
    count=max(1,int(length/3.5))
    for i in range(count+1):
        a=-length*.5+length*i/count
        pos=(p['x'],p['z']+a,.43) if axis=='x' else (p['x']+a,p['z'],.43)
        box('rail structural stanchion',pos,(.16,.16,.86),0,.03)
groups={}
for obj in parts:
    lo,hi=bounds([obj]);band=math.floor((lo.y+hi.y)*.1)
    groups.setdefault((obj.name.split(' / ')[0],band),[]).append(obj)
consolidated=[]
for (kind,band),objects in groups.items():
    bpy.ops.object.select_all(action='DESELECT')
    for obj in objects:obj.hide_set(False);obj.select_set(True)
    bpy.context.view_layer.objects.active=objects[0]
    if len(objects)>1:bpy.ops.object.join()
    obj=bpy.context.object;obj.name=f'ServiceNetwork_{kind}_{band}'
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    consolidated.append(obj)
parts=consolidated
export('ServiceNetwork',parts)
for obj in parts:obj.hide_set(True)
parts=[];category_set('gate')
for x in [-2.8,0,2.8]:
    box('retractable containment blade',(x,0,1.8),(2.45,.35,3.6),0,.11)
    box('containment warning band',(x,-.2,1.8),(1.8,.05,.12),6,.015)
export('GateBarrier',parts)
for obj in parts:bpy.data.objects.remove(obj,do_unlink=True)
for mesh,_ in DONORS.values():
    for obj in mesh:bpy.data.objects.remove(obj,do_unlink=True)
for obj in bpy.context.scene.objects:obj.hide_set(False)
bpy.data.orphans_purge(do_recursive=True)
for image in bpy.data.images:
    if image.source=='FILE' and image.filepath and not image.packed_file:image.pack()
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'Chapter01_BlueprintWorld_R1.blend'),compress=True)
(ROOT/'docs/history/implementation-passes/chapter01-blueprint-world-r1/authored-sector-meshes.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print('BLUEPRINT_WORLD_SECTORS_AUTHORED',manifest)
