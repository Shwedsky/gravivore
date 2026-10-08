"""Original V3 hard-surface assets. Reuses project-owned geometry helpers and the V2 atlas."""
import bpy, bmesh, math, json, random
from pathlib import Path
from mathutils import Vector, Matrix
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/_Game/Content/Chapter01V3'
SOURCE=ROOT/'art/chapter01-v3'
for path in (OUT/'Models',SOURCE,ROOT/'docs/chapter01-v3'):path.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.context.scene.unit_settings.system='METRIC';bpy.context.scene.unit_settings.scale_length=1
palette=[(.075,.105,.13),(.36,.46,.51),(.66,.72,.70),(.13,.19,.22),(.75,.48,.11),(1,.12,.045),(1,.52,.055),(.08,.86,.98),(.022,.030,.036),(.18,.105,.065),(.22,.72,.38),(.42,.18,.66),(.16,.30,.34),(.54,.36,.12),(.28,.32,.33),(.065,.09,.105)]
mats=[]
for i,color in enumerate(palette):
 m=bpy.data.materials.new('Family '+str(i));m.diffuse_color=(*color,1);mats.append(m)
atlas=bpy.data.materials.new('Fidelity_IndustrialAtlas');atlas.use_nodes=True
shader=atlas.node_tree.nodes.get('Principled BSDF')
for label,slot in [('BaseColor','Base Color'),('Emission','Emission Color')]:
 node=atlas.node_tree.nodes.new('ShaderNodeTexImage');node.image=bpy.data.images.load(str(ROOT/'Assets/_Game/Content/ConceptFidelityV2/Textures'/('Fidelity_'+label+'.png')));atlas.node_tree.links.new(node.outputs['Color'],shader.inputs[slot])
shader.inputs['Metallic'].default_value=.6;shader.inputs['Roughness'].default_value=.4;shader.inputs['Emission Strength'].default_value=3
parts=[];bones={};metrics={}
helper=(ROOT/'Tools/first-visual-slice/build_assets.py').read_text()
exec(helper[helper.index('def active('):helper.index('reset();legs(6,.91')])
fidelity=(ROOT/'Tools/concept-fidelity-v2/build_assets.py').read_text()
exec(fidelity[fidelity.index('def poly('):fidelity.index('# Custodian V2:')])
# Preserve the proven animated reactor shell, then add a six-support silhouette,
# elevated rear command armor, shoulder radiators and nested weapon ports.
boss=fidelity[fidelity.index('reset();bone(\'BODY\''):fidelity.index("export('Custodian_V2',True)")]
exec(boss)
for s,label in [(-1,'R'),(1,'L')]:
 B='SUPPORT_'+('4' if s<0 else '5');bone(B,(s*.88,1.35,1.05))
 rod('Rear hexapod hip',(s*.8,1.25,1.12),(s*1.72,1.83,.9),.17,0,B,12)
 rod('Rear hexapod shin',(s*1.72,1.83,.9),(s*2.43,2.21,.25),.13,1,B,12)
 shell('Rear swept ankle armor',(s*2.02,2.06,.53),.51,.95,.34,14,B)
 shell('Rear pointed stabilizer',(s*2.4,2.30,.18),.64,1.22,.23,2,B)
 hose('Powered rear actuator',[(s*.96,1.34,1.24),(s*1.8,2.04,1.02),(s*2.35,2.28,.35)],.04,5,B)
 shell('Raised command shoulder',(s*.91,.76,2.27),.81,1.24,.48,0,label+'_SHUTTER')
 shell('Overlapping swept dorsal armor',(s*.79,.73,2.6),.74,1.08,.32,2,label+'_SHUTTER')
 rod('Dorsal armored heat chimney',(s*.8,1.08,2.44),(s*.62,1.07,3.16),.16,0,vertices=12)
 ring('Recessed dorsal power seat',(s*.62,1.07,3.12),.14,.025,1)
 rod('Dorsal internal warning',(s*.62,1.07,3.08),(s*.62,1.07,3.115),.09,5,vertices=12)
 for j in range(3):box('Radiator lamella',(s*1.24,.62+j*.23,2.47),(.16,.11,.32),1,angle=s*.18,bevel=.009)
 # Sharp blade fins use separate swept surfaces, with an inset red channel.
 poly('Forward swept shoulder blade',[(s*1.48,-.32),(s*1.88,-.5),(s*1.98,-1.14),(s*1.6,-.88)],2.16,.15,2,label+'_SHUTTER',.018)
 rod('Shoulder red recessed strip',(s*1.63,-.45,2.25),(s*1.84,-.91,2.25),.019,5,label+'_SHUTTER',6)
ring('Exposed dorsal reactor dark seat',(0,-.22,2.63),.48,.06,0)
dome('Exposed dorsal reactor heart',(0,-.22,2.64),(.35,.35,.17),5,'CORE')
for s in (-1,1):
 hose('Dorsal internal red supply',[(s*.38,-.16,2.56),(s*.55,.39,2.68),(s*.79,.74,2.79)],.032,5)
shell('Rear command bridge',(0,1.09,2.62),1.21,.99,.57,14)
shell('Command head layered armor',(0,.94,2.91),1.10,.86,.25,2)
box('Recessed scanner slit',(0,.48,2.89),(.71,.10,.12),8)
box('Red command scanner inside slit',(0,.415,2.89),(.50,.025,.045),5,bevel=0)
export('Custodian_V3',True)
# Right-manipulator equipment. Its muzzle is authored in the Unity prefab.
reset();shell('Emitter hardpoint mount',(0,.15,0),.24,.32,.27,0)
rod('Emitter recessed cyan chamber',(0,.05,.02),(0,-.24,.02),.14,7,vertices=16)
for s in (-1,1):shell('Split emitter armor',(s*.14,-.12,.02),.12,.57,.27,2)
rod('Emitter barrel shroud',(0,-.27,.02),(0,-.53,.02),.17,0,vertices=16)
ring('Machined emitter muzzle',(0,-.54,.02),.13,.035,1,axis=(0,1,0))
rod('Cyan source behind aperture',(0,-.48,.02),(0,-.52,.02),.09,7,vertices=16)
hose('Emitter service cable',[(.13,.16,-.08),(.2,-.03,-.11),(.13,-.29,-.07)],.022,8)
export('Emitter_M0')
# Service trenches and damaged deck edges. Opaque parts reveal bounded cavities;
# all geometry is presentation only and stays clear of continuous traversal.
for variant in range(2):
 reset();rng=random.Random(480+variant)
 poly('Dark underdeck depth',[(-1.8,-3),(1.8,-3),(1.8,3),(-1.8,3)],-.33,.12,8,bevel=.02)
 for j in range(5):
  y=-2.4+j*1.15
  points=[(-1.8,y-.48),(.3+rng.uniform(-.3,.2),y-.54),(.75,y-.16),(.26,y+.04),(.70,y+.31),(-1.8,y+.54)]
  poly('Jagged torn steel deck',points,.035,.11,3 if j%2 else 14,bevel=.012)
  rod('Broken hanging reinforcing rib',(-.2,y,.0),(.93,y+.23,-.29),.035,1,vertices=6)
 for j in range(3):
  x=.78+j*.25
  hose('Exposed powered trench conduit',[(x,-2.8,-.18),(x+.14,-1.2,-.22),(x-.15,.4,-.13),(x+.1,2.8,-.20)],.034,7 if variant==0 else 6)
 for y in (-2.2,1.65):
  shell('Displaced deck repair cover',(1.05,y,.08),.73,.92,.07,14)
  box('Warning stripe',(1.05,y,.122),(.42,.11,.008),13,angle=.4,bevel=0)
 export('Broken_Edge_V3_'+str(variant))
reset()
shell('Service trench inset frame',(0,0,-.16),1.44,4.8,.22,0)
for j in range(3):hose('Inner luminous service wire',[(-.3+j*.29,-2.2,-.05),(-.4+j*.29,-.5,-.06),(-.22+j*.29,1.3,-.04),(-.3+j*.29,2.2,-.05)],.025,7 if j!=1 else 6)
for y in (-1.9,-.85,.3,1.35):shell('Unequal angled trench cover',(0,y,.055),1.22,.73,.1,14)
for y in (-2.24,2.25):box('Trench bolted retaining seam',(0,y,.063),(1.28,.13,.08),1)
export('Service_Trench_V3')
reset();arc('Collapsed bowed hull',(0,0,.85),1.53,1.12,.21,-.5,3.8,1.09,14)
hose('Torn powered wreck cable',[(-.82,.21,.8),(-1.54,-.38,.27),(-.48,-1.17,.12),(.85,-1.04,.16)],.05,6)
for i in range(4):shell('Detached scorched armor',(i*.42-.85,-.69,.08+i*.018),.61,.86,.07,9 if i%2 else 2)
export('Collapsed_Hull_V3')
(ROOT/'docs/chapter01-v3/asset_metrics.json').write_text(json.dumps(metrics,indent=2))
print('CHAPTER01_V3_ASSETS_COMPLETE',json.dumps(metrics))
