"""Render actual Blender evidence from saved .blend, with exact camera manifest."""
import bpy, sys, json, math
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(Path(__file__).parent))
from studio import render, aim
OUT=ROOT/'docs/visual-production-v2/g0-bipedal-v2/evidence'
OUT.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(ROOT/'art/visual-production-v2/g0/G0_Bipedal_Blockout_V2.blend'))
scene=bpy.context.scene
root=bpy.data.objects['G0_ROOT_METERS_Z_UP_FORWARD_MINUS_Y']
manifest=[]
def capture(name,pos,target=(0,0,1.9),scale=4.65,res=(1200,1400),note='Authoring-scale orthographic studio render'):
    render(OUT/name,pos,target,scale,res)
    c=scene.camera
    manifest.append({'file':name,'camera_position':list(c.location),'target':list(target),
                     'camera_type':c.data.type,'ortho_scale':scale,'resolution':res,'note':note})

args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if args==['preview']:
    capture('04_G0_Bipedal_threequarter.png',(5,-8,4.7))
    raise SystemExit(0)
capture('01_G0_Bipedal_front.png',(0,-9,2))
capture('02_G0_Bipedal_side.png',(9,0,2))
capture('03_G0_Bipedal_back.png',(0,9,2))
capture('04_G0_Bipedal_threequarter.png',(5,-8,4.7))

# Existing S20 camera mapped Unity (X,Y,Z) -> Blender (X,-Z,Y).
# A 0.5 authoring scale is explicitly a presentation assumption (~1.80 m hero).
scene.camera=bpy.data.objects['CAM_05_GAMEPLAY']
root.scale=(.5,)*3; root.location.z=-.0325; root.rotation_euler.z=math.pi
capture('05_G0_Bipedal_gameplay_angle.png',(0,11.2,15.7),(0,0,.9),4.65,(900,1600),
        'S20 offset (0,14.8,-11.2), look-at 0.9, vertical FOV 46 degrees; 0.5 authoring root scale, facing camera. Blender studio stage, not Unity/device evidence.')
root.scale=(1,)*3; root.location.z=-.065; root.rotation_euler.z=0
scene.camera=bpy.data.objects['Review_camera']

# Pure unlit black contour on white: no emission/light/material advantage.
black=bpy.data.materials.new('REVIEW_unlit_black_silhouette'); black.use_nodes=True
black.node_tree.nodes.clear()
n=black.node_tree.nodes.new('ShaderNodeEmission'); n.inputs[0].default_value=(0,0,0,1)
out=black.node_tree.nodes.new('ShaderNodeOutputMaterial'); black.node_tree.links.new(n.outputs[0],out.inputs[0])
scene.view_layers[0].material_override=black
ground=bpy.data.objects['Studio_floor']; ground.hide_render=True
bg=next(n for n in scene.world.node_tree.nodes if n.type=='BACKGROUND')
old_color=bg.inputs[0].default_value[:]; old_strength=bg.inputs[1].default_value
bg.inputs[0].default_value=(1,1,1,1); bg.inputs[1].default_value=1
scene.view_settings.view_transform='Standard'
capture('06_G0_Bipedal_black_silhouette.png',(3.5,-9,3.7),note='Unlit pure black geometry on white; no floor or emission')
scene.view_layers[0].material_override=None; ground.hide_render=False
bg.inputs[0].default_value=old_color; bg.inputs[1].default_value=old_strength
scene.view_settings.view_transform='AgX'

reference=bpy.data.collections['CATFISH_REFERENCE_CC_BY_4_0']
reference.hide_render=False; reference.hide_viewport=False
donor_root=bpy.data.objects['CATFISH_COMPARISON_ROOT']
root.location.x=1.25; donor_root.location.x=-1.25
root.rotation_euler.z=math.radians(-10); donor_root.rotation_euler.z=math.radians(-10)
c=scene.camera; c.location=(0,-10,4.7); aim(c,(0,0,1.9))
def label(name,body,x,y,size):
    d=bpy.data.curves.new(name,'FONT'); d.body=body; d.align_x='CENTER'; d.size=size
    o=bpy.data.objects.new(name,d); scene.collection.objects.link(o)
    o.parent=c; o.location=(x,y,-5)
    m=bpy.data.materials.new(name+'_white'); m.use_nodes=True; m.node_tree.nodes.clear()
    n=m.node_tree.nodes.new('ShaderNodeEmission'); n.inputs[0].default_value=(.63,.75,.82,1)
    out=m.node_tree.nodes.new('ShaderNodeOutputMaterial'); m.node_tree.links.new(n.outputs[0],out.inputs[0]); d.materials.append(m)
    return o
labels=[label('LEFT_ORIGINAL','ORIGINAL CATFISH',-1.25,2.05,.13),
        label('RIGHT_CUSTOM','G-0  /  BIPEDAL V2',1.25,2.05,.13),
        label('COMPARISON_CREDIT','Catfish: Jungle Jim / CC BY 4.0   |   equal height, rest pose   |   G-0: custom shells + 17 adapted mechanisms',0,-2.12,.069)]
capture('07_G0_Bipedal_vs_Catfish_donor.png',(0,-10,4.7),(0,0,1.9),4.8,(1800,1400),
        'LEFT original Catfish REST silhouette/materials, RIGHT new G-0, equal height; both yaw -10 degrees; no geometry change to reference')
(OUT.parent/'data/render_manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print('SEVEN_BLENDER_RENDERS_COMPLETE')
