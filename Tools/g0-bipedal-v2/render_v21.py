import bpy, sys, json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(Path(__file__).parent))
from studio import render
bpy.ops.wm.open_mainfile(filepath=str(ROOT/'art/visual-production-v2/g0/G0_Bipedal_Blockout_V21.blend'))
OUT=ROOT/'docs/visual-production-v2/g0-bipedal-v2/evidence-v21/blender'; OUT.mkdir(parents=True,exist_ok=True)
s=bpy.context.scene; s.camera=bpy.data.objects['Review_camera']; s.cycles.samples=32
manifest=[]
for name,pos in [('01_G0_V21_front.png',(0,-9,1.75)),('02_G0_V21_side.png',(9,0,1.75)),('03_G0_V21_back.png',(0,9,1.75)),('04_G0_V21_threequarter.png',(5,-8,4.4))]:
    render(OUT/name,pos,(0,0,1.7),4.4,(1200,1400)); manifest.append({'file':name,'position':pos,'target':[0,0,1.7],'resolution':[1200,1400]})
black=bpy.data.materials.new('V21_unlit_black'); black.use_nodes=True; black.node_tree.nodes.clear()
n=black.node_tree.nodes.new('ShaderNodeEmission'); n.inputs[0].default_value=(0,0,0,1)
o=black.node_tree.nodes.new('ShaderNodeOutputMaterial'); black.node_tree.links.new(n.outputs[0],o.inputs[0])
s.view_layers[0].material_override=black; bpy.data.objects['Studio_floor'].hide_render=True
bg=next(n for n in s.world.node_tree.nodes if n.type=='BACKGROUND'); bg.inputs[0].default_value=(1,1,1,1); bg.inputs[1].default_value=1
s.view_settings.view_transform='Standard'
render(OUT/'05_G0_V21_black_silhouette.png',(3.5,-9,3.4),(0,0,1.7),4.4,(1200,1400))
manifest.append({'file':'05_G0_V21_black_silhouette.png','unlit_black':True,'resolution':[1200,1400]})
(ROOT/'docs/visual-production-v2/g0-bipedal-v2/data-v21/blender_render_manifest.json').write_text(json.dumps(manifest,indent=2))
print('FIVE_V21_BLENDER_CAPTURES_COMPLETE')
