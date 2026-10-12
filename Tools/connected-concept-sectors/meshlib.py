"""Project-owned capacitor installation. Blender metres x/y/z -> Unity x/z/y.

No R2 deck recipe or global nonuniform fit. Main shells, foundations, manifolds,
walkways and power distribution are authored here. Donor details keep native UVs.
"""
import bpy, bmesh, math, json
from pathlib import Path
from mathutils import Vector, Matrix

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Assets/_Game/Content/ConnectedConceptSectors'
SOURCE = ROOT / 'art/connected-concept-sectors'
PASS = ROOT / 'docs/history/implementation-passes/connected-concept-sectors'
for folder in [OUT / 'Models', SOURCE, PASS]: folder.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
MATS = {}
for name in ['Graphite','Shell','Steel','Deck','Ceramic','Warning','Coil','Cold','Hot']:
    mat = bpy.data.materials.new('CF_' + name); mat.use_nodes = True
    shader = next((n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED'), None) or mat.node_tree.nodes.new('ShaderNodeBsdfPrincipled')
    tex = mat.node_tree.nodes.new('ShaderNodeTexImage')
    tex.image = bpy.data.images.load(str(ROOT / 'Assets/_Game/Content/CapacitorFieldProof/Textures' / f'CF_{name}_BaseColor.png'))
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

