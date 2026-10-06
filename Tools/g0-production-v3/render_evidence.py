"""Unaltered Cycles renders of the reopened production blend."""
import bpy, json, sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'Tools/g0-bipedal-v2'))
from studio import render
bpy.ops.wm.open_mainfile(filepath=str(ROOT/'art/g0-production-v3/G0_Production_V3.blend'))
OUT=ROOT/'docs/g0-production-v3/evidence/blender'; OUT.mkdir(parents=True,exist_ok=True)
s=bpy.context.scene; s.camera=bpy.data.objects['Review_camera']; s.cycles.samples=32
rig=bpy.data.objects['G0_V3_RIG']; rig.animation_data.action=bpy.data.actions['Idle']; s.frame_set(1)
manifest=[]
for name,pos in [('01_front.png',(0,-9,1.75)),('02_side.png',(9,0,1.75)),('03_rear.png',(0,9,1.75)),('04_threequarter.png',(5,-8,4.4))]:
    render(OUT/name,pos,(0,0,1.7),4.4,(1200,1400)); manifest.append(dict(file=name,position=pos,frame=1,clip='Idle'))
black=bpy.data.materials.new('V3_unlit_black'); black.use_nodes=True; black.node_tree.nodes.clear()
n=black.node_tree.nodes.new('ShaderNodeEmission'); n.inputs[0].default_value=(0,0,0,1)
o=black.node_tree.nodes.new('ShaderNodeOutputMaterial'); black.node_tree.links.new(n.outputs[0],o.inputs[0])
s.view_layers[0].material_override=black; bpy.data.objects['Studio_floor'].hide_render=True
bg=next(n for n in s.world.node_tree.nodes if n.type=='BACKGROUND'); bg.inputs[0].default_value=(1,1,1,1); bg.inputs[1].default_value=1
s.view_settings.view_transform='Standard'
render(OUT/'05_black_silhouette.png',(3.5,-9,3.4),(0,0,1.7),4.4,(1200,1400))
manifest.append(dict(file='05_black_silhouette.png',unlit_black=True))
(ROOT/'docs/g0-production-v3/data/blender_capture_manifest.json').write_text(json.dumps(manifest,indent=2))
print('G0_V3_FIVE_REOPENED_SOURCE_CAPTURES_COMPLETE')
