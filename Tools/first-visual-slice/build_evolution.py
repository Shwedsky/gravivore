"""Original G-0 evolution armor, rigidly bound to the existing production rig.
Run in Blender 5.2. Uses the slice's shared industrial atlas and authored
chamfered shells/containment rails; never modifies the accepted base asset.
"""
from pathlib import Path
import bpy, json
ROOT = Path(__file__).resolve().parents[2]
# Reuse the established hard-surface, rigid-skin and palette authoring helpers.
helper = (ROOT/'Tools/first-visual-slice/build_assets.py').read_text(encoding='utf-8')
exec(compile(helper.split('\ndef bone(')[0], 'slice_authoring_helpers', 'exec'))
reports = {}
for tier in (1, 2):
    for o in list(bpy.data.objects): bpy.data.objects.remove(o, do_unlink=True)
    with bpy.data.libraries.load(str(ROOT/'art/g0-production-v3/G0_Production_V3.blend')) as (src, dst):
        dst.objects = ['G0_V3_RIG']
    rig = dst.objects[0]; bpy.context.collection.objects.link(rig)
    rig.animation_data_clear()
    for b in rig.pose.bones:
        b.rotation_euler = (0, 0, 0); b.location = (0, 0, 0); b.scale = (1, 1, 1)
    parts.clear()
    for s, label in ((-1, 'R'), (1, 'L')):
        shoulder = label+'_SHOULDER'
        shell(label+' layered shoulder carapace', (s*.76, .03, 2.96), .62, .73, .34, 2, shoulder)
        shell(label+' recessed shoulder saddle', (s*.76, .035, 2.79), .54, .68, .16, 0, shoulder)
        rod(label+' shoulder bearing', (s*.53,.045,2.86), (s*.96,.045,2.86), .105, 1, shoulder)
        rod(label+' shoulder energy seam', (s*.77,-.35,2.98), (s*.77,-.35,3.09), .019, 7, shoulder, 8)
        shell(label+' lateral core containment', (s*.39, -.19, 2.64), .19, .49, .64, 1, 'TORSO')
        rod(label+' core rail', (s*.22,-.36,2.50), (s*.29,-.36,2.94), .038, 2, 'TORSO')
        rod(label+' rail energy inset', (s*.23,-.397,2.52), (s*.285,-.397,2.88), .012, 7, 'TORSO', 8)
        shell(label+' waist stabilizer', (s*.42,.02,1.90), .32, .55, .26, 0, 'PELVIS')
        if tier == 2:
            # Broad armor silhouette and two swept containment blades, no spikes.
            shell(label+' heavy shoulder cap', (s*.93,.04,3.04), .49, .87, .39, 2, shoulder)
            shell(label+' dorsal containment blade', (s*.44,.41,2.92), .20, .72, .75, 1, 'TORSO')
            rod(label+' dorsal inset', (s*.44,.66,2.76), (s*.44,.67,3.19), .018, 7, 'TORSO', 8)
            shell(label+' tool armor blade', (s*.90,-.30,2.08), .22, .89, .30, 2, label+'_TOOL')
            rod(label+' tool energy rail', (s*.91,-.69,2.16), (s*.91,-.36,2.25), .020, 7, label+'_TOOL', 8)
            shell(label+' knee containment armor', (s*.43,-.26,1.25), .38, .28, .37, 2, label+'_KNEE')
    meshes = skin_mesh('G0_Tier'+str(tier)+'Armor', rig, True)
    for q in meshes: q.hide_set(False)
    bpy.ops.object.select_all(action='DESELECT')
    rig.select_set(True)
    for q in meshes: q.select_set(True)
    bpy.context.view_layer.objects.active = rig
    path = OUT/'Models'/('G0_Tier'+str(tier)+'Armor.fbx')
    bpy.ops.export_scene.fbx(filepath=str(path),use_selection=True,object_types={'MESH','ARMATURE'},
        apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',
        add_leaf_bones=False,bake_anim=False,use_armature_deform_only=True)
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/('G0_Tier'+str(tier)+'Armor.blend')))
    reports[str(tier)] = {'triangles':[len(q.data.polygons) for q in meshes], 'bones':18,
        'materials':1, 'source':'Original project-owned armor on G0 production rig', 'forward':'Blender -Y / imported Unity -Z'}
(ROOT/'docs/device-correction').mkdir(exist_ok=True)
(ROOT/'docs/device-correction/evolution_geometry.json').write_text(json.dumps(reports,indent=2),encoding='utf-8')
