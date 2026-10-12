"""Open actual source meshes, measure native materials/UVs, render geometry."""
import bpy,json,math
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
PASS=ROOT/'docs/history/implementation-passes/energy-chamber-hybrid-proof-v1'
OUT=PASS/'donor-inspection';OUT.mkdir(parents=True,exist_ok=True)
rows=[]
for source in sorted((ROOT/'ExternalAssetIntake/Current/energy-chamber-inspection').glob('*/*.fbx')):
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
    bpy.ops.import_scene.fbx(filepath=str(source))
    meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
    points=[o.matrix_world@v.co for o in meshes for v in o.data.vertices]
    lo=Vector([min(v[i] for v in points) for i in range(3)]);hi=Vector([max(v[i] for v in points) for i in range(3)])
    mats=[]
    for obj in meshes:
        for mat in obj.data.materials:
            if mat and mat.use_nodes:mats.append(dict(name=mat.name,images=[n.image.filepath for n in mat.node_tree.nodes if n.type=='TEX_IMAGE' and n.image]))
    rows.append(dict(name=source.stem,source=str(source.relative_to(ROOT)),dimensions=list(hi-lo),triangles=sum(len(f.vertices)-2 for o in meshes for f in o.data.polygons),materials=mats,uvs=[list(o.data.uv_layers.keys()) for o in meshes]))
    center=(hi+lo)/2;size=max(hi-lo)
    bpy.ops.object.camera_add(location=center+Vector((1.25,-1.5,.95))*size)
    camera=bpy.context.object;camera.rotation_euler=(center-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=size*1.6
    scene=bpy.context.scene;scene.camera=camera;scene.render.engine='BLENDER_WORKBENCH'
    scene.display.shading.light='STUDIO';scene.display.shading.color_type='MATERIAL';scene.display.shading.show_shadows=True;scene.display.shading.show_cavity=True
    scene.display.shading.cavity_type='BOTH';scene.display.shading.background_type='WORLD';scene.world.color=(.065,.08,.10)
    scene.render.resolution_x=800;scene.render.resolution_y=800;scene.render.resolution_percentage=100
    scene.render.filepath=str(OUT/(source.stem+'.png'));bpy.ops.render.render(write_still=True)
(PASS/'donor-geometry.json').write_text(json.dumps(rows,indent=2),encoding='utf-8')
print('SOURCE_GEOMETRY_INSPECTED',len(rows))
