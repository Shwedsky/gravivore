"""Project-owned capacitor installation. Blender metres x/y/z -> Unity x/z/y.

No R2 deck recipe or global nonuniform fit. Main shells, foundations, manifolds,
walkways and power distribution are authored here. Donor details keep native UVs.
"""
import bpy, bmesh, math, json
from pathlib import Path
from mathutils import Vector, Matrix

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Assets/_Game/Content/CapacitorFieldProof'
SOURCE = ROOT / 'art/capacitor-field-proof'
PASS = ROOT / 'docs/history/implementation-passes/capacitor-field-proof'
for folder in [OUT / 'Models', SOURCE, PASS]: folder.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
MATS = {}
for name in ['Graphite','Shell','Steel','Deck','Ceramic','Warning','Coil','Cold','Hot']:
    mat = bpy.data.materials.new('CF_' + name); mat.use_nodes = True
    shader = next((n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED'), None) or mat.node_tree.nodes.new('ShaderNodeBsdfPrincipled')
    tex = mat.node_tree.nodes.new('ShaderNodeTexImage')
    tex.image = bpy.data.images.load(str(OUT / 'Textures' / f'CF_{name}_BaseColor.png'))
    mat.node_tree.links.new(tex.outputs['Color'], shader.inputs['Base Color'])
    shader.inputs['Metallic'].default_value = .75; shader.inputs['Roughness'].default_value = .42
    MATS[name] = mat
PARTS = []; GROUP = 'deck'

def finish(obj, name, material, bevel=0, smooth=False):
    obj.name = GROUP + ' / ' + name
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    bm=bmesh.new();bm.from_mesh(obj.data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(obj.data);bm.free()
    obj.data.materials.clear();obj.data.materials.append(MATS[material])
    if material not in ['Steel','Cold','Hot','Coil']:
        obj.data.materials.append(MATS['Steel'])
    if bevel:
        mod = obj.modifiers.new('Machined edge radius', 'BEVEL'); mod.width = bevel; mod.segments = 3
        mod.material = 1 if len(obj.data.materials)>1 else 0
        bpy.ops.object.modifier_apply(modifier=mod.name)
    obj.data.update()
    uv = obj.data.uv_layers.active or obj.data.uv_layers.new(name='UVMap'); uv.name = 'UVMap'
    # Planar physical coordinates in metres. Faces neither normalize to bounds
    # nor repeat an entire atlas cell. Curved forms get a circumference mapping.
    for face in obj.data.polygons:
        axes = sorted(range(3), key=lambda i: abs(face.normal[i]))[:2]
        for loop in face.loop_indices:
            co = obj.data.vertices[obj.data.loops[loop].vertex_index].co
            uv.data[loop].uv = (co[axes[0]], co[axes[1]])
        face.use_smooth = smooth and abs(face.normal.z) < .8
    if not smooth:
        mod = obj.modifiers.new('Area weighted hard surface normals', 'WEIGHTED_NORMAL'); mod.keep_sharp = True
        bpy.ops.object.modifier_apply(modifier=mod.name)
    obj.select_set(False); PARTS.append(obj); return obj

def box(name, pos, size, mat='Graphite', bevel=.05, yaw=0):
    bpy.ops.mesh.primitive_cube_add(size=1, location=pos)
    obj = bpy.context.object; obj.dimensions = size; obj.rotation_euler.z = math.radians(yaw)
    return finish(obj,name,mat,bevel)

def prism(name, outline, low, high, mat='Deck', bevel=.025):
    n=len(outline); verts=[(x,y,z) for z in [low,high] for x,y in outline]
    faces=[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update()
    obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj)
    return finish(obj,name,mat,bevel)

def lathe(name, center, profile, mat='Steel', sides=64):
    verts=[]
    for z,r in profile:
        for i in range(sides):
            a=i*math.tau/sides;verts.append((r*math.cos(a),r*math.sin(a),z))
    faces=[]
    for j in range(len(profile)-1):
        for i in range(sides): faces.append((j*sides+i,j*sides+(i+1)%sides,(j+1)*sides+(i+1)%sides,(j+1)*sides+i))
    faces += [tuple(range(sides-1,-1,-1)),tuple(range((len(profile)-1)*sides,len(profile)*sides))]
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update()
    obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj);obj.location=center
    finish(obj,name,mat,smooth=True)
    uv=obj.data.uv_layers.active
    for face in obj.data.polygons[:-2]:
        indices=list(face.vertices); angles=[math.atan2(mesh.vertices[i].co.y,mesh.vertices[i].co.x)/math.tau for i in indices]
        if max(angles)-min(angles)>.5: angles=[a+1 if a<0 else a for a in angles]
        for loop,i,a in zip(face.loop_indices,indices,angles): uv.data[loop].uv=(a*math.tau*max(r for z,r in profile),mesh.vertices[i].co.z)
    return obj

def cyl(name,pos,r,h,mat='Steel',sides=32):
    return lathe(name,pos,[(-h/2,r*.93),(-h/2+.04,r),(h/2-.04,r),(h/2,r*.93)],mat,sides)

def arc(name,pos,r,thickness,low,high,start,span,mat='Shell'):
    count=max(8,int(span/6));verts=[]
    for z in [low,high]:
        for radius in [r-thickness,r]:
            for i in range(count+1):
                a=math.radians(start+span*i/count);verts.append((radius*math.cos(a),radius*math.sin(a),z))
    n=count+1;faces=[]
    for i in range(count): faces.extend([(i,i+1,2*n+i+1,2*n+i),(n+i,3*n+i,3*n+i+1,n+i+1),(i,n+i,n+i+1,i+1),(2*n+i,2*n+i+1,3*n+i+1,3*n+i)])
    faces.extend([(0,2*n,3*n,n),(count,n+count,3*n+count,2*n+count)])
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update()
    obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj);obj.location=pos
    return finish(obj,name,mat,.028)

def tube(name,points,r=.10,mat='Steel'):
    curve=bpy.data.curves.new(name,'CURVE');curve.dimensions='3D';curve.resolution_u=1
    curve.bevel_depth=r;curve.bevel_resolution=2
    spline=curve.splines.new('POLY');spline.points.add(len(points)-1)
    for point,co in zip(spline.points,points):point.co=(*co,1)
    obj=bpy.data.objects.new(name,curve);bpy.context.collection.objects.link(obj)
    bpy.context.view_layer.objects.active=obj;obj.select_set(True);bpy.ops.object.convert(target='MESH')
    return finish(obj,name,mat,smooth=True)

def bolts(x,y,z,r,count=12):
    for i in range(count):
        a=i*math.tau/count;cyl('pressure coupling captive stud',(x+r*math.cos(a),y+r*math.sin(a),z),.065,.10,'Steel',6)

def grate(x,y,w,d,z=0):
    # The slots reveal pipes, ribs and an actual 0.8 m recessed service volume.
    box('trench cast side beam',(x-w/2,y,z-.2),(.14,d,.4),'Graphite')
    box('trench cast side beam',(x+w/2,y,z-.2),(.14,d,.4),'Graphite')
    count=int(d/.19)
    for i in range(count):box('load bearing open grate',(x,y-d/2+(i+.5)*d/count,z-.018),(w,.075,.055),'Steel',.012)
    for sx in [-1,1]:box('grate removable cassette lip',(x+sx*(w/2-.1),y,z),(.10,d,.06),'Shell',.012)

# Irregular uninterrupted combat slabs; not the R2 three-by-three deck.
prism('reinforced undercroft',[(-10,-10),(10,-10),(10,10),(-10,10)],-1.2,-.9,'Graphite',.10)
for sign in [-1,1]:
    outline=[(sign*.055,-4.55),(sign*3.88,-4.55),(sign*3.65,-2.1),(sign*3.65,3.9),(sign*.055,4.8)]
    if sign<0:outline.reverse()
    prism('grounded main work slab',outline,-.28,0,'Deck')
    outline=[(sign*.055,-8.9),(sign*3.45,-8.9),(sign*3.83,-6.4),(sign*.055,-6.4)]
    if sign<0:outline.reverse()
    prism('separate approach load slab',outline,-.28,0,'Deck')
    # Real plate seams and recessed mounting sockets at human scale.
    for y in [-7.5,-3.3,0,3.2]:
        box('slab mechanical joint',(sign*1.9,y,.009),(3.48,.026,.014),'Graphite',.004)
        for x in [.3,3.3]:box('flush insulated floor latch',(sign*x,y,.014),(.16,.25,.028),'Steel',.012)
    box('peripheral thick maintenance deck',(sign*7.75,-2.5,-.25),(3.45,14.8,.50),'Deck',.10)
    box('undercroft cable tray',(sign*4.75,-1.2,-.66),(1.95,13.6,.13),'Graphite')
    for offset in [-.48,-.19,.10,.40]:
        tube('protected high voltage cable',[(sign*4.75+offset,-7.9,-.56),(sign*4.75+offset,4.0,-.56),(sign*4.75+offset,5.2,-.10)],.085,'Ceramic')
    grate(sign*4.75,-1.2,1.88,13.3)
    for y in [-5.8,-1.8,2.2]:
        box('service pit cross support',(sign*4.75,y,-.46),(2.35,.22,.70),'Shell')
    # Short contained route markings explain actual crossings.
    for y in [-6.3,0,3.5]:
        box('functional cable crossing',(sign*4.9,y,-.10),(2.65,1.5,.2),'Deck')
        for i in range(5):box('electrical service hazard marking',(sign*(3.85+i*.38),y-.61,.012),(.22,.13,.019),'Warning',.003)
prism('east access throat',[(3.5,-2.7),(10,-2.7),(10,2.7),(3.5,2.7)],-.25,0,'Deck')
# A transverse distribution chamber physically cuts the former long floor field.
# Open grating is traversable at y=0 and shows the lower copper/cable layer.
box('transverse power distribution pit',(0,-5.5,-.70),(7.9,1.8,.16),'Graphite')
for y in [-5.98,-5.58,-5.18]:tube('cross bus recessed conductor',[(-3.8,y,-.49),(3.8,y,-.49)],.11,'Ceramic')
grate(0,-5.5,7.7,1.75)
for x in [-3,-1.5,0,1.5,3]:box('distribution pit grounding support',(x,-5.5,-.40),(.13,1.8,.75),'Shell')
prism('reactor bank elevated service plinth',[(-9.6,4.9),(-6.8,4.2),(-2,4.8),(2,4.8),(6.8,4.2),(9.6,4.9),(9.6,9.7),(-9.6,9.7)],-.7,.26,'Graphite',.1)
for x in [-7.5,-2.2,2.2,7.5]:
    box('inset reactor service plate',(x,8.9,.28),(2.6,1.1,.08),'Deck')
    grate(x,8.9,2.2,.8,.35)
# Angular floor branch collectors terminate in pressure vessel bases.
for x in [-4.65,0,4.65]:
    tube('grounded lower bus route',[(x,5.5,-.36),(x,2.6,-.36),(0,1.6,-.36),(0,-8.8,-.36)],.17,'Steel')
box('central power route recessed cover',(0,-3,.012),(.42,12,.032),'Graphite',.013)
for y in [-7.5,-3.7,.2,3.9]:box('power route inspection indicator',(0,y,.033),(.10,.27,.018),'Cold',.007)

GROUP='pressure vessels'
def chamber(x,y,height,r=1.32):
    lathe('load bearing ribbed pressure pedestal',(x,y,0),[(.26,1.60),(.38,1.60),(.55,1.44),(.75,1.44),(1.08,1.30),(1.26,1.45),(1.42,1.45)],'Graphite')
    lathe('machined base coupling',(x,y,0),[(1.26,1.47),(1.33,1.52),(1.48,1.52),(1.55,1.32)],'Steel')
    bolts(x,y,1.52,1.38)
    for a in range(0,360,45):
        ang=math.radians(a)
        box('base replaceable access shell',(x+1.4*math.cos(ang),y+1.4*math.sin(ang),.88),(.52,.20,.52),'Shell',.065,a-90)
        box('pedestal deep cooling intake',(x+1.51*math.cos(ang),y+1.51*math.sin(ang),.87),(.31,.035,.23),'Graphite',.02,a-90)
    # Coil is mostly dark. Only the narrow axial electrode and slot traces glow.
    cyl('induction containment sleeve',(x,y,(1.65+height-.65)/2),.85,height-2.30,'Coil',64)
    cyl('contained axial discharge',(x,y,(1.65+height-.65)/2),.14,height-2.30,'Cold',32)
    for a in [20,110,200]:arc('curved pressure armor sector',(x,y,0),r,.13,1.62,height-.53,a,62,'Shell')
    for a in [15,105,195,285]:
        ang=math.radians(a)
        tube('preloaded chamber tie rod',[(x+1.12*math.cos(ang),y+1.12*math.sin(ang),1.40),(x+1.12*math.cos(ang),y+1.12*math.sin(ang),height-.40)],.065,'Steel')
        # Thin contained blue slots remain subordinate to opaque construction.
        box('sealed energy inspection aperture',(x+.91*math.cos(ang),y+.91*math.sin(ang),(height+.8)/2),(.075,.085,height-2.1),'Cold',.018,a)
    for z in [1.7,height-1.0]:
        lathe('rolled pressure retaining collar',(x,y,z),[(-.15,1.15),(-.12,1.38),(.05,1.38),(.12,1.20)],'Graphite')
        bolts(x,y,z+.12,1.24,10)
    lathe('nested reactor crown',(x,y,0),[(height-.55,1.22),(height-.38,1.48),(height-.21,1.48),(height-.12,1.32),(height,1.32),(height+.11,.98),(height+.19,.98)],'Graphite')
    lathe('exposed crown annulus',(x,y,0),[(height-.33,1.5),(height-.26,1.5),(height-.22,1.36)],'Steel')
    bolts(x,y,height+.04,1.11)
    for a in range(0,360,60):
        ang=math.radians(a);box('crown removable armored jaw',(x+1.23*math.cos(ang),y+1.23*math.sin(ang),height-.06),(.42,.34,.38),'Shell',.04,a-90)
    cyl('crown ceramic terminal',(x,y,height+.34),.35,.28,'Ceramic')
    tube('pressure cooling return',[(x,y+.9,1.1),(x,y+1.4,1.1),(x,y+1.8,.65),(x,y+2.3,.65)],.16,'Steel')
for x,y,h in [(-4.65,6.1,5.65),(0,7.1,7.15),(4.65,6.1,5.65)]: chamber(x,y,h)

# One interlocked pressure-bank chassis makes the triad a connected installation.
# The stepped saddle exposes each chamber base and leaves the approach open.
prism('interlocked pressure bank saddle',[(-6.8,5.0),(-5.7,4.4),(-3.3,4.4),(-2.3,5.0),(-1.4,4.25),(1.4,4.25),(2.3,5.0),(3.3,4.4),(5.7,4.4),(6.8,5.0),(6.8,8.9),(-6.8,8.9)],.27,.83,'Graphite',.08)
for x in [-4.65,0,4.65]:
    box('saddle removable front armor',(x,4.34, .84),(2.30,.24,.88),'Shell',.065)
    box('saddle deep pressure inspection',(x,4.19,.86),(1.76,.08,.38),'Graphite',.02)
    for k in range(5):box('saddle pressure cooling louvre',(x-.60+k*.30,4.12,.85),(.12,.055,.32),'Steel',.018)
    for dx in [-.97,.97]:box('saddle captive armored locking dog',(x+dx,4.12,.85),(.14,.16,.62),'Graphite',.025)
    box('pressure bank warning edge',(x,4.15,.36),(1.7,.06,.13),'Warning',.01)
    tube('visible chamber feed manifold',[(x+1.17,5.90,1.07),(x+1.65,5.55,1.07),(x+1.65,4.73,.72),(x+.4,4.73,.72)],.105,'Steel')
    cyl('service pressure takeoff',(x+1.65,5.55,1.23),.15,.28,'Graphite',24)
    for dx in [-1.1,1.1]:cyl('exposed foundation anchor',(x+dx,4.61,.48),.09,.15,'Steel',6)

GROUP='distribution'
# Structural beam and properly insulated conductors connect all three chambers.
for x in [-8.8,8.8]:
    box('load bearing power gantry pier',(x,9.2,2.9),(.7,.85,5.3),'Graphite',.12)
    box('gantry pier foundation',(x,9.2,.55),(1.25,1.25,1.1),'Shell',.08)
box('power bus steel carrier',(0,9.2,5.6),(17.9,.70,.55),'Graphite',.08)
for y,z in [(8.85,5.9),(9.2,6.12),(9.55,5.9)]:
    tube('insulated phase bus',[(-8.5,y,z),(8.5,y,z)],.12,'Steel')
    for x in [-7,-3,0,3,7]:
        for k in range(4):cyl('ceramic bus support disc',(x,y,z-.24-k*.095),.19,.045,'Ceramic',24)
for x,y,h in [(-4.65,6.1,5.65),(0,7.1,7.15),(4.65,6.1,5.65)]:
    tube('chamber terminal grounded conductor',[(x,y,h+.45),(x,y, h+.78),(x,8.3,h+.78),(x,8.9,6.12)],.105,'Steel')
    tube('thick ground return',[(x+1.2,y,.78),(x+1.9,y,.78),(x+1.9,8.3,.78),(0,8.3,.78)],.19,'Graphite')
# Open collars, louvres and tapered housings shape two side transformers.
for x,y in [(-6.0,-3.3),(-6.0,1.5),(6.0,-4.2),(6.0,3.0)]:
    box('transformer footing',(x,y,.24),(2.6,2.95,.48),'Graphite',.1)
    lathe('transformer cast enclosure',(x,y,0),[(.48,.95),(.65,1.08),(2.3,1.08),(2.75,.80),(2.9,.80)],'Graphite',8)
    for side in [-1,1]:
        box('transformer nested shell',(x+side*.88,y,1.60),(.26,2.1,2.1),'Shell',.08)
        for k in range(8):box('deep heat exchanger fins',(x+side*1.14,y-.76+k*.22,1.60),(.17,.07,1.60),'Steel',.015)
    prism('angled transformer crown',[(x-1.12,y-.90),(x-.77,y-1.10),(x+.77,y-1.10),(x+1.12,y-.90),(x+1.12,y+.85),(x-1.12,y+.85)],2.54,2.82,'Shell',.065)
    box('replaceable control cassette',(x,y-1.0,1.1),(1.45,.32,1.30),'Shell',.07)
    box('control recessed aperture',(x,y-1.18,1.25),(1.04,.045,.68),'Graphite',.02)
    box('hostile control status',(x,y-1.21,1.41),(.22,.02,.48),'Hot',.005)
    for dx in [-.58,.58]:
        cyl('bushing mounting foot',(x+dx,y,2.94),.32,.22,'Steel')
        for k in range(5):cyl('transformer ceramic insulator',(x+dx,y,3.13+k*.115),.24,.045,'Ceramic',24)
        cyl('insulator terminal bolt',(x+dx,y,3.71),.085,.17,'Steel',16)
    tube('transformer protected feed',[(x,y+.8,.55),(x,y+1.35,.55),(x*.62,y+1.35,-.48)],.15,'Graphite')

GROUP='service structure'
for x in [-9.35,9.35]:
    for y in [-8.4,-4.2,0,4.2]:
        box('maintenance platform deep rib',(x,y,-.32),(.3,.35,1.2),'Steel')
    # Thick angled chassis, no broad smooth wall closing the camera view.
    box('plant perimeter structural sill',(x,-.8,.32),(.55,16,.55),'Graphite',.10)
    tube('plant handrail',[(x,-8.8,1.18),(x,7.6,1.18)],.045,'Steel')
    for y in [-8,-4,0,4,7]:tube('handrail upright',[(x,y,.4),(x,y,1.18)],.043,'Steel')
for x in [-3.0,3.0]:
    # Elevated servicing is presentation only; the combat plane stays flat.
    for k in range(5):box('reactor access stair',(x,4.30+k*.21,.04+k*.055),(1.2,.30,.10),'Steel',.018)
for y in [5.0,8.9]:
    tube('bank service safety rail',[(-8.6,y,1.1),(-6.6,y,1.1)],.045)
    tube('bank service safety rail',[(6.6,y,1.1),(8.6,y,1.1)],.045)
# Grounding node has a useful visual and electrical endpoint.
box('ground switch angled cabinet',(-8.0,7.6,1.70),(1.55,1.4,3.1),'Shell',.15)
box('ground switch open mechanism',(-8.0,6.86,1.65),(1.12,.12,2.1),'Graphite',.03)
for z in [1.0,1.55,2.10]:
    tube('ground switch contact', [(-8.35,6.72,z),(-7.70,6.72,z)],.065,'Steel')
box('ground switch danger indicator',(-8.54,6.69,1.65),(.08,.05,1.65),'Hot',.01)

# Selected donor details: preserve UV and source maps. Uniform scale only.
GROUP='donor service'
donor_log=[]
def donor(name,pos,scale=1,yaw=0):
    source=next((ROOT/'ExternalAssetIntake/Current').glob('*/inspection/'+name+'.fbx'))
    before=set(bpy.context.scene.objects);bpy.ops.import_scene.fbx(filepath=str(source))
    objects=[o for o in bpy.context.scene.objects if o not in before]
    meshes=[o for o in objects if o.type=='MESH']
    points=[o.matrix_world@v.co for o in meshes for v in o.data.vertices]
    lo=Vector([min(v[i] for v in points) for i in range(3)]);hi=Vector([max(v[i] for v in points) for i in range(3)])
    center=Vector(((lo.x+hi.x)/2,(lo.y+hi.y)/2,lo.z))
    transform=Matrix.Translation(Vector(pos))@Matrix.Rotation(math.radians(yaw),4,'Z')@Matrix.Diagonal((scale,scale,scale,1))@Matrix.Translation(-center)
    for obj in meshes:
        world=obj.matrix_world.copy();obj.parent=None;obj.matrix_world=transform@world;obj.name=GROUP+' / '+name
        # Quaternius trim identity kept, Molten palette retained without remap.
        for i,mat in enumerate(obj.data.materials):
            key='R1_Trim1' if 'Trim_01' in mat.name else 'R1_Trim2' if 'Trim_02' in mat.name else 'R1_Trim3' if 'Trim_03' in mat.name else 'CF_DonorPalette'
            if key not in MATS: MATS[key]=bpy.data.materials.new(key)
            obj.data.materials[i]=MATS[key]
        uv=obj.data.uv_layers.active
        if uv:uv.name='UVMap'
        PARTS.append(obj)
    for obj in objects:
        if obj.type!='MESH':bpy.data.objects.remove(obj,do_unlink=True)
    donor_log.append(dict(name=name,scale=scale,source=str(source.relative_to(ROOT)),nativeUV=True))
donor('Command Console',(7.0,6.1,.28),.80,180)
donor('Column_Pipes',(-9.05,7.5,0),.65)
donor('Prop_PipeHolder',(-8.0,5.2,.4),1.0)
donor('Prop_AccessPoint',(8.6,-7.3,.3),.75,180)
# Molten's palette image is copied exactly from canonical ZIP for those UVs.
import zipfile
archive=next((ROOT.parents[1]/'ExternalAssetIntake/Current/molten-maps-scifi').glob('*.zip'))
with zipfile.ZipFile(archive) as z:
    entry=next(i for i in z.infolist() if Path(i.filename).name=='Gradient Texture Atlas by Imphenzia.png')
    (OUT/'Textures/CF_DonorPalette_BaseColor.png').write_bytes(z.read(entry))

# Consolidate logical machinery into longitudinal culling bands. Editable source
# is saved BEFORE merging so each modeled part remains independently editable.
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'CapacitorField.blend'))
groups={}
for obj in PARTS:
    points=[obj.matrix_world@v.co for v in obj.data.vertices]
    mid=(min(v.y for v in points)+max(v.y for v in points))/2
    groups.setdefault((obj.name.split(' / ')[0],math.floor(mid/4)),[]).append(obj)
merged=[]
for (kind,band),objects in groups.items():
    bpy.ops.object.select_all(action='DESELECT')
    for obj in objects:obj.select_set(True)
    bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();obj=bpy.context.object
    obj.name=f'CF_{kind}_{band}';bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    bm=bmesh.new();bm.from_mesh(obj.data);bmesh.ops.triangulate(bm,faces=list(bm.faces));bm.to_mesh(obj.data);bm.free()
    merged.append(obj)
bpy.ops.object.select_all(action='DESELECT')
for obj in merged:obj.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(OUT/'Models/CapacitorField.fbx'),use_selection=True,object_types={'MESH'},apply_unit_scale=True,
    apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',bake_anim=False,add_leaf_bones=False,path_mode='STRIP',mesh_smooth_type='FACE',use_tspace=True)
(PASS/'authored.json').write_text(json.dumps(dict(renderers=len(merged),triangles=sum(len(o.data.polygons) for o in merged),donors=donor_log,
    texelConvention='custom UV one repeat per metre; donor native UV retained',nonuniformHeroFit=False),indent=2),encoding='utf-8')
print('CAPACITOR_FIELD_AUTHORED',len(merged))
