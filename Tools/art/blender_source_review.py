"""Blender source audit and neutral inspection renders (no source redistribution)."""
import bpy
import bmesh
import json
import math
import sys
import xml.etree.ElementTree as ET
from pathlib import Path
from mathutils import Vector, Matrix

ROOT = Path(__file__).resolve().parents[2]
SCRATCH = ROOT / '.asset-intake-tmp'
OUT = ROOT / 'docs/visual-production-v2/modeling'
DATA = OUT / 'data'
IMAGES = OUT / 'source-review'
DATA.mkdir(parents=True, exist_ok=True)
IMAGES.mkdir(parents=True, exist_ok=True)

def import_dae(path):
    """Read static COLLADA geometry/scene matrices; never claim rig conversion."""
    tree = ET.parse(path)
    ns = {'c': 'http://www.collada.org/2005/11/COLLADASchema'}
    geometries = {}
    for geo in tree.findall('.//c:library_geometries/c:geometry', ns):
        sources = {}
        for src in geo.findall('c:mesh/c:source', ns):
            array = src.find('c:float_array', ns)
            access = src.find('c:technique_common/c:accessor', ns)
            if array is not None:
                stride = int(access.get('stride', '3'))
                numbers = list(map(float, array.text.split()))
                sources[src.get('id')] = [numbers[i:i+stride] for i in range(0, len(numbers), stride)]
        vmap = {v.get('id'): v.find('c:input', ns).get('source')[1:]
                for v in geo.findall('c:mesh/c:vertices', ns)}
        vertices, faces = [], []
        for tag in ['triangles', 'polylist', 'polygons']:
            for prim in geo.findall('c:mesh/c:' + tag, ns):
                inputs = prim.findall('c:input', ns)
                stride = max(int(i.get('offset', '0')) for i in inputs) + 1
                inp = next(i for i in inputs if i.get('semantic') == 'VERTEX')
                source = sources[vmap[inp.get('source')[1:]]]
                offset = int(inp.get('offset', '0'))
                base = len(vertices)
                vertices.extend([v[:3] for v in source])
                for p in prim.findall('c:p', ns):
                    inds = list(map(int, p.text.split()))[offset::stride]
                    vc = prim.find('c:vcount', ns)
                    counts = list(map(int, vc.text.split())) if vc is not None else ([3] * (len(inds)//3) if tag == 'triangles' else [len(inds)])
                    start = 0
                    for count in counts:
                        faces.append([base + j for j in inds[start:start+count]])
                        start += count
        mesh = bpy.data.meshes.new(geo.get('name', geo.get('id')))
        mesh.from_pydata(vertices, [], faces)
        mesh.update()
        geometries[geo.get('id')] = mesh
    def node_visit(node, parent=None):
        obj = bpy.data.objects.new(node.get('name', node.get('id', 'Node')), None)
        bpy.context.collection.objects.link(obj)
        obj.parent = parent
        transform = Matrix.Identity(4)
        for item in node:
            tag = item.tag.split('}')[-1]
            if tag == 'matrix':
                nums = list(map(float, item.text.split()))
                transform = transform @ Matrix([nums[i:i+4] for i in range(0, 16, 4)])
            elif tag == 'translate':
                transform = transform @ Matrix.Translation(Vector(map(float, item.text.split())))
            elif tag == 'rotate':
                nums = list(map(float, item.text.split()))
                transform = transform @ Matrix.Rotation(math.radians(nums[3]), 4, Vector(nums[:3]))
            elif tag == 'scale':
                nums = list(map(float, item.text.split()))
                transform = transform @ Matrix.Diagonal(Vector((*nums, 1)))
        obj.matrix_basis = transform
        for instance in node.findall('c:instance_geometry', ns):
            mesh = geometries[instance.get('url')[1:]]
            part = bpy.data.objects.new(mesh.name, mesh)
            bpy.context.collection.objects.link(part)
            part.parent = obj
        for child in node.findall('c:node', ns):
            node_visit(child, obj)
    for node in tree.findall('.//c:library_visual_scenes/c:visual_scene/c:node', ns):
        node_visit(node)
    # Source is Y-up, Blender uses Z-up. Units are recorded, not normalized in audit.
    up = tree.find('.//c:asset/c:up_axis', ns)
    roots = [o for o in bpy.context.scene.objects if o.parent is None]
    if up is not None and up.text == 'Y_UP':
        for obj in roots:
            obj.matrix_world = Matrix.Rotation(math.pi/2, 4, 'X') @ obj.matrix_world
    return dict(method='static XML geometry/scene import; UV/material/skin not converted',
                source_geometry_count=len(geometries),
                source_materials=[dict(id=m.get('id'),name=m.get('name')) for m in tree.findall('.//c:library_materials/c:material',ns)],
                source_images=[dict(id=i.get('id'),path=i.findtext('c:init_from',default='',namespaces=ns)) for i in tree.findall('.//c:library_images/c:image',ns)],
                source_controller_count=len(tree.findall('.//c:controller', ns)),
                source_animation_count=len(tree.findall('.//c:library_animations/c:animation', ns)),
                source_uv_inputs=len(tree.findall('.//c:input[@semantic="TEXCOORD"]', ns)))

def load(path):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    details = {}
    if path.suffix == '.blend':
        bpy.ops.wm.open_mainfile(filepath=str(path), load_ui=False, use_scripts=False)
    elif path.suffix in ('.glb', '.gltf'):
        bpy.ops.import_scene.gltf(filepath=str(path))
    elif path.suffix == '.fbx':
        bpy.ops.wm.fbx_import(filepath=str(path), use_anim=True)
    elif path.suffix == '.dae':
        try:
            bpy.ops.wm.collada_import.get_rna_type()
        except KeyError:
            details = import_dae(path)
        else:
            bpy.ops.wm.collada_import(filepath=str(path))
    else:
        raise ValueError(path)
    bpy.context.scene.frame_set(0)
    bpy.context.view_layer.update()
    return details

def audit(path, details):
    meshes = []
    for obj in bpy.context.scene.objects:
        if obj.type != 'MESH':
            continue
        mesh = obj.data
        mesh.calc_loop_triangles()
        bm = bmesh.new()
        bm.from_mesh(mesh)
        topo = dict(boundary_edges=sum(e.is_boundary for e in bm.edges),
                    non_manifold_edges=sum(not e.is_manifold and not e.is_boundary for e in bm.edges),
                    loose_vertices=sum(not v.link_edges for v in bm.verts),
                    zero_area_faces=sum(f.calc_area() < 1e-12 for f in bm.faces))
        bm.free()
        evaluated = obj.evaluated_get(bpy.context.evaluated_depsgraph_get())
        em = evaluated.to_mesh()
        em.calc_loop_triangles()
        eval_tris = len(em.loop_triangles)
        evaluated.to_mesh_clear()
        meshes.append(dict(name=obj.name, vertices=len(mesh.vertices), triangles=len(mesh.loop_triangles),
                           evaluated_triangles=eval_tris, uv_layers=[u.name for u in mesh.uv_layers],
                           material_slots=[m.name if m else None for m in mesh.materials],
                           modifiers=[dict(name=m.name, type=m.type) for m in obj.modifiers],
                           vertex_groups=len(obj.vertex_groups), dimensions=list(obj.dimensions),
                           negative_scale=any(s < 0 for s in obj.scale), topology=topo))
    textures = [dict(name=i.name, path=i.filepath, size=list(i.size), packed=bool(i.packed_file))
                for i in bpy.data.images if i.source == 'FILE']
    return dict(path=str(path.relative_to(ROOT)).replace('\\', '/'), import_details=details,
                meshes=meshes, triangles=sum(m['triangles'] for m in meshes),
                evaluated_triangles=sum(m['evaluated_triangles'] for m in meshes),
                materials=[m.name for m in bpy.data.materials], textures=textures,
                armatures=[dict(name=o.name, bones=len(o.data.bones), bone_names=[b.name for b in o.data.bones])
                           for o in bpy.context.scene.objects if o.type == 'ARMATURE'],
                actions=[dict(name=a.name, frames=list(a.frame_range), slots=len(a.slots)) for a in bpy.data.actions],
                hierarchy=[dict(name=o.name, type=o.type, parent=o.parent.name if o.parent else None,
                                hide_render=o.hide_render) for o in bpy.context.scene.objects])

def render_neutral(filename, angle=(6, -8, 5), size=480):
    scene = bpy.context.scene
    for obj in list(scene.objects):
        if obj.type in ('CAMERA', 'LIGHT'):
            bpy.data.objects.remove(obj, do_unlink=True)
        else:
            obj.hide_render = False
    objects = [o for o in scene.objects if o.type == 'MESH']
    points = [o.matrix_world @ Vector(v) for o in objects for v in o.bound_box]
    lo = Vector([min(p[i] for p in points) for i in range(3)])
    hi = Vector([max(p[i] for p in points) for i in range(3)])
    center = (lo + hi)/2
    span = max(hi-lo)
    cam_data = bpy.data.cameras.new('Inspection_Camera')
    camera = bpy.data.objects.new('Inspection_Camera', cam_data)
    scene.collection.objects.link(camera)
    camera.location = center + Vector(angle).normalized() * span * 3
    camera.rotation_euler = (center-camera.location).to_track_quat('-Z', 'Y').to_euler()
    cam_data.type = 'ORTHO'
    cam_data.ortho_scale = span * 1.45
    cam_data.clip_end = max(1000, span*10)
    cam_data.clip_start = max(0.00001, span/10000)
    scene.camera = camera
    scene.render.engine = 'BLENDER_WORKBENCH'
    shading = scene.display.shading
    shading.light = 'STUDIO'
    shading.studiolight_rotate_z = 0.3
    shading.color_type = 'SINGLE'
    shading.single_color = (0.48, 0.52, 0.57)
    shading.show_shadows = True
    shading.show_cavity = True
    shading.cavity_type = 'BOTH'
    shading.background_type = 'WORLD'
    if scene.world is None:
        scene.world = bpy.data.worlds.new('Inspection_World')
    scene.world.color = (0.12, 0.12, 0.12)
    scene.render.resolution_x = size
    scene.render.resolution_y = size
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = 'PNG'
    scene.render.filepath = str(filename)
    scene.view_settings.view_transform = 'Standard'
    bpy.ops.render.render(write_still=True)

def main():
    mode = sys.argv[sys.argv.index('--')+1] if '--' in sys.argv else 'candidates'
    reports = []
    if mode == 'environment':
        paths = sorted(p for p in (SCRATCH/'environment').rglob('*.fbx') if 'FBX' in p.parts)
    else:
        paths = [p for cat in ('scout','cutter','magnetar') for p in (SCRATCH/cat).rglob('*')
                 if p.suffix.lower() in ('.blend','.fbx','.glb','.gltf','.dae')]
    for path in paths:
        details = load(path)
        record = audit(path, details)
        record['id'] = path.relative_to(SCRATCH).parts[1]
        reports.append(record)
        name = path.stem if mode == 'environment' else record['id']
        render_neutral(IMAGES/(name+'_threequarter.png'), size=300 if mode == 'environment' else 700)
        if mode != 'environment':
            render_neutral(IMAGES/(name+'_front.png'), angle=(0,-9,3), size=700)
        (DATA/(mode+'_audit.json')).write_text(json.dumps(reports, indent=2), encoding='utf-8')
        print('AUDIT', mode, name, record['triangles'], 'meshes', len(record['meshes']), flush=True)

if __name__ == '__main__':
    main()
