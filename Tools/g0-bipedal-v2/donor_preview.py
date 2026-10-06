import bpy, sys
from pathlib import Path
from mathutils import Matrix, Vector
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(Path(__file__).parent))
from studio import setup_studio, render
bpy.ops.wm.open_mainfile(filepath=str(ROOT/'.local-g0-v2/catfish_imported.blend'))
meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
points=[o.matrix_world@Vector(c) for o in meshes for c in o.bound_box]
zmin=min(p.z for p in points); zmax=max(p.z for p in points)
root=bpy.data.objects.new('Donor_normalization',None); bpy.context.scene.collection.objects.link(root)
for o in list(bpy.context.scene.objects):
    if o!=root and o.parent is None:
        mw=o.matrix_world.copy(); o.parent=root; o.matrix_world=mw
factor=3.65/(zmax-zmin)
root.scale=(factor,)*3; root.location.z=-zmin*factor
setup_studio()
render(ROOT/'.local-g0-v2/donor_preview.png',(5,-8,4.6))
