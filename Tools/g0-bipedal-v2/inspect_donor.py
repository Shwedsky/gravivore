"""Run with Blender 5.2.2 LTS --background --python this_file.

Reads the user-provided ZIP; safe extraction stays in ignored local storage.
Produces measured source inspection and an untouched imported donor reference.
"""
import bpy, bmesh, json, hashlib, zipfile, math
from pathlib import Path
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[2]
LOCAL = ROOT / '.local-g0-v2'
DATA = ROOT / 'docs/visual-production-v2/g0-bipedal-v2/data'
ARCHIVE = Path.home() / 'Documents/GRAVIVORE_ASSET_INTAKE/g0/catfish-mech-low-poly-animated.zip'
assert bpy.app.version == (5, 2, 2), bpy.app.version_string
LOCAL.mkdir(exist_ok=True)
DATA.mkdir(parents=True, exist_ok=True)
with zipfile.ZipFile(ARCHIVE) as z:
    entries = [{'path': e.filename, 'bytes': e.file_size} for e in z.infolist()]
    for e in z.infolist():
        target = (LOCAL / 'donor' / e.filename).resolve()
        assert target.is_relative_to((LOCAL / 'donor').resolve()), e.filename
    z.extractall(LOCAL / 'donor')
fbxs = list((LOCAL / 'donor').rglob('*.fbx'))
assert len(fbxs) == 1, fbxs
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.fbx(filepath=str(fbxs[0]), use_anim=True)
bpy.context.view_layer.update()

def rounded(seq): return [round(float(v), 6) for v in seq]

objects = []
for o in bpy.context.scene.objects:
    row = {'name': o.name, 'type': o.type, 'parent': o.parent.name if o.parent else None,
           'location': rounded(o.location), 'rotation_degrees': rounded([math.degrees(v) for v in o.rotation_euler]),
           'scale': rounded(o.scale), 'dimensions': rounded(o.dimensions),
           'modifiers': [{'name': m.name, 'type': m.type} for m in o.modifiers]}
    if o.type == 'MESH':
        me = o.data
        me.calc_loop_triangles()
        corners = [o.matrix_world @ Vector(c) for c in o.bound_box]
        row['world_bounds'] = {'min': [min(c[i] for c in corners) for i in range(3)],
                               'max': [max(c[i] for c in corners) for i in range(3)]}
        row['shape_keys'] = [k.name for k in me.shape_keys.key_blocks] if me.shape_keys else []
        row['weighted_groups'] = sorted({o.vertex_groups[g.group].name for v in me.vertices for g in v.groups if g.weight > .01})
        bm = bmesh.new(); bm.from_mesh(me)
        row.update(vertices=len(me.vertices), triangles=len(me.loop_triangles), polygons=len(me.polygons),
                   materials=[m.name if m else None for m in me.materials],
                   uv_layers=[u.name for u in me.uv_layers], vertex_groups=[g.name for g in o.vertex_groups],
                   boundary_edges=sum(e.is_boundary for e in bm.edges),
                   nonmanifold_edges=sum(not e.is_manifold for e in bm.edges),
                   loose_vertices=sum(not v.link_edges for v in bm.verts),
                   degenerate_faces=sum(f.calc_area() < 1e-12 for f in bm.faces))
        bm.free()
    if o.type == 'ARMATURE':
        row['bones'] = [{'name': b.name, 'parent': b.parent.name if b.parent else None,
                         'head_local': rounded(b.head_local), 'tail_local': rounded(b.tail_local),
                         'deform': b.use_deform} for b in o.data.bones]
    objects.append(row)
actions = [{'name': a.name, 'frames': rounded(a.frame_range), 'slots': [s.identifier for s in a.slots],
            'layers': len(a.layers)} for a in bpy.data.actions]
materials = []
for m in bpy.data.materials:
    materials.append({'name': m.name, 'nodes': [{'name': n.name, 'type': n.type,
        'image': n.image.filepath if n.type == 'TEX_IMAGE' and n.image else None} for n in m.node_tree.nodes] if m.use_nodes else []})
meshes = [o for o in bpy.context.scene.objects if o.type == 'MESH']
points = [o.matrix_world @ Vector(c) for o in meshes for c in o.bound_box]
mins = [min(p[i] for p in points) for i in range(3)]
maxs = [max(p[i] for p in points) for i in range(3)]
images = [{'name': i.name, 'path': i.filepath, 'size': list(i.size), 'packed': bool(i.packed_file)} for i in bpy.data.images]
report = {'blender': bpy.app.version_string, 'archive': str(ARCHIVE),
          'sha256': hashlib.sha256(ARCHIVE.read_bytes()).hexdigest(), 'entries': entries,
          'source': str(fbxs[0].relative_to(LOCAL / 'donor')), 'objects': objects,
          'object_count': len(objects), 'mesh_count': len(meshes),
          'triangles': sum(r.get('triangles', 0) for r in objects),
          'vertices': sum(r.get('vertices', 0) for r in objects), 'actions': actions,
          'materials': materials, 'images': images, 'world_bounds': {'min': mins, 'max': maxs},
          'scene_units': bpy.context.scene.unit_settings.system, 'unit_scale': bpy.context.scene.unit_settings.scale_length,
          'fps': bpy.context.scene.render.fps}
(DATA / 'catfish_inspection.json').write_text(json.dumps(report, indent=2), encoding='utf-8')

# Remap actual images without guessing internal source filenames.
texture_paths = {p.stem.lower(): p for p in (LOCAL / 'donor/textures').iterdir()}
for m in bpy.data.materials:
    if not m.node_tree: continue
    for n in m.node_tree.nodes:
        if n.type != 'TEX_IMAGE' or not n.image: continue
        basename = Path(n.image.filepath.replace('\\', '/')).stem.lower()
        if basename in texture_paths:
            n.image = bpy.data.images.load(str(texture_paths[basename]), check_existing=True)
            if 'normal' in basename: n.image.colorspace_settings.name='Non-Color'
for o in bpy.context.scene.objects:
    if o.animation_data: o.animation_data.action = None
    if o.type == 'ARMATURE': o.data.pose_position = 'REST'
bpy.context.scene.frame_set(1)
bpy.ops.wm.save_as_mainfile(filepath=str(LOCAL / 'catfish_imported.blend'))
print('CATFISH_INSPECTION_COMPLETE', json.dumps({k: report[k] for k in ['object_count','mesh_count','triangles','vertices','world_bounds']}))
