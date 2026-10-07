"""Original opaque industrial floor paint for the existing service route.
No collision, navigation logic or additional props. Blender 5.2.
"""
from pathlib import Path
import bpy
ROOT=Path(__file__).resolve().parents[2]
helper=(ROOT/'Tools/first-visual-slice/build_assets.py').read_text(encoding='utf-8')
exec(compile(helper.split('\ndef bone(')[0],'slice_authoring_helpers','exec'))
for s in (-1,1):
    box('Technical service rail '+str(s),(s*.90,0,0),(.035,7.4,.004),7,bevel=0)
    for i in range(4):
        box('Yellow service landing '+str(s)+str(i),(s*(1.12+i*.22),3.12,0),(.11,.40,.004),4,angle=.42,bevel=0)
    box('Rust maintenance boundary '+str(s),(s*1.68,-3.23,0),(.28,.14,.004),9,bevel=0)
mesh=skin_mesh('Deck_ServiceMarkings')[0];active(mesh)
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'Deck_ServiceMarkings.blend'))
bpy.ops.export_scene.fbx(filepath=str(OUT/'Models/Deck_ServiceMarkings.fbx'),use_selection=True,object_types={'MESH'},
    apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',bake_anim=False)
