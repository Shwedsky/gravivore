"""Author silhouette geometry in metres; build one node and one connector.

Unity instantiates the reusable node/connector four times. Stock machinery is
referenced from current runtime prefabs, never duplicated inside these FBXs.
"""
import bpy,bmesh,math,json,shutil
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/_Game/ArtProof/EnergyChamber'
PASS=ROOT/'docs/history/implementation-passes/energy-chamber-hybrid-proof-v1'
SOURCE=ROOT/'art/energy-chamber-hybrid-proof-v1'
for folder in [OUT/'Models',OUT/'Donors',OUT/'Licenses',SOURCE]:folder.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
MATS={};PARTS=[];GROUP='Foundation'
for name in ['Graphite','Shell','Steel','Deck','Ceramic','Warning','Coil','Cold']:
    mat=bpy.data.materials.new('EC_'+name);mat.diffuse_color={'Graphite':(.095,.13,.17,1),'Shell':(.30,.39,.44,1),'Steel':(.54,.60,.64,1),'Deck':(.16,.21,.25,1),'Ceramic':(.20,.15,.085,1),'Warning':(.55,.30,.06,1),'Coil':(.045,.16,.21,1),'Cold':(.10,.43,.54,1)}[name];MATS[name]=mat

def finish(obj,name,mat='Graphite',bevel=0,smooth=False):
    obj.name=GROUP+' / '+name;bpy.context.view_layer.objects.active=obj;obj.select_set(True)
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    bm=bmesh.new();bm.from_mesh(obj.data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(obj.data);bm.free()
    obj.data.materials.clear();obj.data.materials.append(MATS[mat])
    if mat not in ['Steel','Cold','Coil']:obj.data.materials.append(MATS['Steel'])
    if bevel:
        mod=obj.modifiers.new('Machined bevel','BEVEL');mod.width=bevel;mod.segments=3;mod.material=min(1,len(obj.data.materials)-1);bpy.ops.object.modifier_apply(modifier=mod.name)
    uv=obj.data.uv_layers.active or obj.data.uv_layers.new(name='UVMap');uv.name='UVMap'
    for face in obj.data.polygons:
        axes=sorted(range(3),key=lambda i:abs(face.normal[i]))[:2]
        for loop in face.loop_indices:
            co=obj.data.vertices[obj.data.loops[loop].vertex_index].co;uv.data[loop].uv=(co[axes[0]],co[axes[1]])
        face.use_smooth=smooth and abs(face.normal.z)<.8
    if not smooth:
        mod=obj.modifiers.new('Weighted normals','WEIGHTED_NORMAL');mod.keep_sharp=True;bpy.ops.object.modifier_apply(modifier=mod.name)
    obj.select_set(False);PARTS.append(obj);return obj

def box(name,pos,size,mat='Graphite',bevel=.035,yaw=0):
    bpy.ops.mesh.primitive_cube_add(size=1,location=pos);obj=bpy.context.object;obj.dimensions=size;obj.rotation_euler.z=math.radians(yaw);return finish(obj,name,mat,bevel)
def prism(name,outline,low,high,mat='Deck',bevel=.035):
    n=len(outline);verts=[(x,y,z) for z in [low,high] for x,y in outline];faces=[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update();obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj);return finish(obj,name,mat,bevel)
def polygon(r,sides=8,angle=22.5):return [(r*math.cos(math.radians(angle)+i*math.tau/sides),r*math.sin(math.radians(angle)+i*math.tau/sides)) for i in range(sides)]
def arc(name,r,thickness,low,high,start=0,span=360,mat='Shell'):
    count=max(6,int(span/5));n=count+1;verts=[]
    for z in [low,high]:
        for radius in [r-thickness,r]:
            for i in range(n):a=math.radians(start+span*i/count);verts.append((radius*math.cos(a),radius*math.sin(a),z))
    faces=[]
    for i in range(count):faces.extend([(i,i+1,2*n+i+1,2*n+i),(n+i,3*n+i,3*n+i+1,n+i+1),(i,n+i,n+i+1,i+1),(2*n+i,2*n+i+1,3*n+i+1,3*n+i)])
    faces.extend([(0,2*n,3*n,n),(count,n+count,3*n+count,2*n+count)])
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update();obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj);return finish(obj,name,mat,.018)
def lathe(name,profile,mat='Graphite',sides=64,pos=(0,0,0)):
    verts=[(r*math.cos(i*math.tau/sides),r*math.sin(i*math.tau/sides),z) for z,r in profile for i in range(sides)]
    faces=[(j*sides+i,j*sides+(i+1)%sides,(j+1)*sides+(i+1)%sides,(j+1)*sides+i) for j in range(len(profile)-1) for i in range(sides)]
    faces+=[tuple(range(sides-1,-1,-1)),tuple(range((len(profile)-1)*sides,len(profile)*sides))]
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update();obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj);obj.location=pos;return finish(obj,name,mat,smooth=True)
def tube(name,points,r=.085,mat='Steel'):
    curve=bpy.data.curves.new(name,'CURVE');curve.dimensions='3D';curve.resolution_u=1;curve.bevel_depth=r;curve.bevel_resolution=2
    spline=curve.splines.new('POLY');spline.points.add(len(points)-1)
    for pt,co in zip(spline.points,points):pt.co=(*co,1)
    obj=bpy.data.objects.new(name,curve);bpy.context.collection.objects.link(obj);bpy.context.view_layer.objects.active=obj;obj.select_set(True);bpy.ops.object.convert(target='MESH');return finish(obj,name,mat,smooth=True)
def fasteners(radius,z,count=16):
    for i in range(count):
        a=i*math.tau/count;lathe('recessed captive stud',[(z-.025,.045),(z+.025,.045)],'Steel',6,(radius*math.cos(a),radius*math.sin(a),0))

# Height zones 0.52 / 1.32 / 2.08. Deck is an open annulus with cast
# support below it; cardinal interruptions explain the distribution routes.
prism('chamfered octagonal foundation',polygon(2.95),.015,.52,'Graphite',.095)
prism('foundation inset wear cap',polygon(2.83),.52,.60,'Deck',.035)
arc('service deck structural annulus',2.69,.91,1.12,1.32,mat='Deck')
arc('lower exposed steel bearing lip',2.72,.11,1.08,1.16,mat='Steel')
lathe('pressure seat stepped mass',[(.56,1.93),(.69,1.93),(.84,1.70),(1.35,1.70),(1.48,1.51),(1.84,1.51),(1.98,1.68),(2.08,1.68)])
arc('pressure seat bolted flange',1.73,.22,1.94,2.09,mat='Steel');fasteners(1.59,2.11)
for a in range(0,360,45):
    ang=math.radians(a);c,s=math.cos(ang),math.sin(ang)
    box('radial cast load rib',(2.23*c,2.23*s,.85),(1.03,.27,.74),'Graphite',.055,a)
    box('foundation access door',(2.72*c,2.72*s,.34),(.47,.055,.25),'Shell',.024,a-90)
    box('pressure seat cooling recess',(1.73*c,1.73*s,1.05),(.54,.08,.37),'Graphite',.023,a-90)
    for k in range(5):box('pressure seat exposed cooling louvre',((1.80)*c-.17*s+k*.085*s,1.80*s+.17*c-k*.085*c,1.05),(.035,.08,.27),'Steel',.006,a)
for a in [45,135,225,315]:
    arc('removable service deck cassette',2.62,.71,1.32,1.37,a-28,56,'Shell')
    arc('service safety yellow edge',2.64,.065,1.37,1.388,a-24,48,'Warning')

GROUP='CentralChamber'
lathe('contained induction sleeve',[(2.08,.91),(2.19,1.02),(4.95,1.02),(5.12,.89)],'Coil')
lathe('ribbed lower vessel shoulder',[(2.08,1.24),(2.18,1.34),(2.39,1.34),(2.51,1.17)],'Graphite')
arc('machined lower structural ring',1.43,.22,2.34,2.51,mat='Steel')
arc('upper load transfer ring',1.43,.24,4.88,5.12,mat='Steel')
for a in range(0,360,60):
    ang=math.radians(a);c,s=math.cos(ang),math.sin(ang)
    arc('segmented opaque vessel armor',1.20,.12,2.57,4.82,a+8,34,'Shell')
    tube('frame preloaded steel tie rod',[(1.34*c,1.34*s,2.10),(1.47*c,1.47*s,2.74),(1.47*c,1.47*s,4.56),(1.30*c,1.30*s,5.38)],.095,'Graphite')
    box('tie rod machined mid joint',(1.47*c,1.47*s,3.67),(.24,.24,.46),'Steel',.04,a)
    box('contained inspection energy slit',(1.037*c,1.037*s,3.67),(.055,.065,2.08),'Cold',.012,a)
    box('upper frame locking jaw',(1.20*c,1.20*s,5.24),(.36,.42,.44),'Shell',.045,a-90)
    tube('crown cooling line',[(1.21*c,1.21*s,5.12),(1.54*c,1.54*s,5.42),(1.06*c,1.06*s,5.63)],.055)
for z in [2.87,3.66,4.45]:arc('pressure circumferential belt',1.26,.085,z,z+.10,mat='Graphite')
lathe('stepped containment crown',[(5.08,1.12),(5.16,1.47),(5.33,1.47),(5.40,1.24),(5.56,1.24),(5.65,.84),(5.79,.84),(5.86,.57)],'Graphite')
arc('crown exposed bright metal band',1.48,.12,5.20,5.29,mat='Steel');fasteners(1.09,5.60,12)
lathe('crown ceramic terminal',[(5.86,.34),(6.04,.34),(6.11,.22)],'Ceramic',32)
for a in range(0,360,90):arc('crown isolated status aperture',.70,.07,5.78,5.805,a+10,32,'Cold')

# Connector authored along +X. Twin return/feed conduits cross an opaque
# carrier bridge and terminate in physical flange manifolds at both ends.
GROUP='Connector'
prism('carrier bridge chamfered structure',[(2.02,-.46),(4.26,-.46),(4.44,-.25),(4.44,.25),(4.26,.46),(2.02,.46)],.61,.88,'Graphite',.065)
for y in [-.21,.21]:
    tube('continuous coolant feed and return',[(1.31,y,2.13),(1.86,y,2.13),(2.12,y,1.70),(2.85,y,1.70),(3.26,y,1.18),(4.42,y,1.18)],.092)
    for x,z in [(1.85,2.13),(2.60,1.70),(3.52,1.18),(4.23,1.18)]:
        obj=lathe('bolted in-line pipe union',[(-.07,.145),(-.045,.16),(.045,.16),(.07,.145)],'Graphite',32,(x,y,z));obj.rotation_euler.y=math.pi/2
for x in [2.16,3.20,4.13]:
    box('conduit support saddle',(x,0,1.07),(.20,.76,.45),'Shell',.04)
box('visible insulated cable channel',(3.28,0,.955),(2.12,.16,.115),'Ceramic',.025)
for x in [2.36,3.26,4.10]:box('channel inspection cap',(x,0,1.02),(.13,.23,.04),'Steel',.014)

GROUP='SideNode'
prism('distribution node load plinth',[(-1.14,-1.17),(1.14,-1.17),(1.34,-.91),(1.34,.92),(1.08,1.16),(-1.08,1.16),(-1.34,.92),(-1.34,-.91)],.015,.37,'Graphite',.07)
prism('node raised machine mounting plate',[(-1.15,-.98),(1.15,-.98),(1.15,.98),(-1.15,.98)],.37,.49,'Deck',.035)
for x in [-1.1,1.1]:
    box('node service perimeter steel ledge',(x,0,.51),(.13,1.92,.08),'Steel',.022)
    for y in [-.82,.82]:box('node foundation locking dog',(x,y,.26),(.28,.29,.30),'Shell',.035)
# Donor generator remains the functional primary node form. A manifold makes
# its inbound route terminate in machinery, rather than at an arbitrary prop.
box('node incoming manifold',(-1.10,0,.99),(.40,.72,.88),'Graphite',.06)
for y in [-.21,.21]:tube('node manifold to generator feed',[(-1.34,y,1.18),(-.74,y,1.18),(-.57,y,.78)],.092)
box('node grounded control recess',(-1.33,0,.75),(.035,.27,.24),'Shell',.012)

# Retain individually editable project geometry. License attribution is
# embedded even though CC0 does not require it; donor models are separate.
text=bpy.data.texts.new('AUTHORSHIP');text.write('GRAVIVORE project-owned Energy Chamber silhouette geometry. Secondary donors are listed in the separate origin record. No stock donor bytes are embedded in this custom source.')
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'EnergyChamber_HybridProof.blend'))
summary=[]
for group in ['Foundation','CentralChamber','Connector','SideNode']:
    objs=[o for o in bpy.context.scene.objects if o.type=='MESH' and o.name.startswith(group+' / ')];bpy.ops.object.select_all(action='DESELECT')
    for o in objs:o.select_set(True)
    bpy.context.view_layer.objects.active=objs[0];bpy.ops.object.join();obj=bpy.context.object;obj.name='EC_'+group
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    bm=bmesh.new();bm.from_mesh(obj.data);bmesh.ops.triangulate(bm,faces=list(bm.faces));bm.to_mesh(obj.data);bm.free()
    bpy.ops.export_scene.fbx(filepath=str(OUT/'Models'/('EC_'+group+'.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',bake_anim=False,add_leaf_bones=False,path_mode='STRIP',use_tspace=True)
    summary.append(dict(group=group,triangles=len(obj.data.polygons),sourceParts=len(objs),path='Assets/_Game/ArtProof/EnergyChamber/Models/EC_'+group+'.fbx'))

# Selected new service donors only. Existing generator/centrifuge/pipe-holder
# imports are referenced by the Unity builder, never re-imported here.
selection=['Platform_Stairs_4','Prop_Rail_Round_Big']
intake=json.loads((PASS/'intake-candidates.json').read_text())
promoted=[]
for name in selection:
    row=next(r for r in intake if Path(r['entry']).stem==name and not r['licenseEvidence'])
    src=ROOT/row['inspectionPath'];dst=OUT/'Donors'/(name+'.fbx');shutil.copyfile(src,dst)
    promoted.append(dict(row,origin='EXTERNAL DONOR',unityPath=str(dst.relative_to(ROOT)),usage='service access stair' if name.startswith('Platform') else 'quarter arc service safety rail'))
for row in intake:
    if row['licenseEvidence']:shutil.copyfile(ROOT/row['inspectionPath'],OUT/'Licenses'/(row['package']+'.txt'))
(PASS/'promoted-donors.json').write_text(json.dumps(promoted,indent=2),encoding='utf-8')
(PASS/'custom-geometry.json').write_text(json.dumps(summary,indent=2),encoding='utf-8')
print('ENERGY_CHAMBER_GEOMETRY_AUTHORED',summary)
