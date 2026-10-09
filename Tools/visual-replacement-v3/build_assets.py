"""Adapt verified CC0 donors to one worn-metal atlas, plus original machinery.

Blender batch authoring. Originals remain ignored; exported public geometry is
restricted to ingestion_manifest CC0 sources and project-owned construction.
"""
import bpy,bmesh,math,json,hashlib,random
from pathlib import Path
from mathutils import Vector,Matrix
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/_Game/Content/VisualReplacementV3'
DOCS=ROOT/'docs/visual-replacement-v3'
for p in (OUT/'Models',OUT/'Textures',ROOT/'art/visual-replacement-v3'):p.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.context.scene.unit_settings.system='METRIC'
bpy.context.scene.unit_settings.scale_length=1.0
palette=[(.055,.078,.092),(.27,.34,.38),(.49,.56,.57),(.14,.18,.20),(.35,.23,.10),(.85,.035,.012),(1,.28,.027),(.035,.54,.65),(.014,.022,.029),(.12,.071,.043),(.21,.27,.29),(.34,.39,.40),(.10,.17,.20),(.42,.30,.12),(.20,.25,.27),(.032,.049,.061)]
mats=[]
for i,c in enumerate(palette):
 m=bpy.data.materials.new('VR3_Family_'+str(i));m.diffuse_color=(*c,1);mats.append(m)
atlas=bpy.data.materials.new('VR3_WornIndustrialAtlas');atlas.use_nodes=True
trim=bpy.data.materials.new('VR3_IndustrialTrimAtlas')
shader=atlas.node_tree.nodes.get('Principled BSDF');shader.inputs['Metallic'].default_value=.68;shader.inputs['Roughness'].default_value=.59
parts=[];bones={};metrics={};N=1024
helper=(ROOT/'Tools/first-visual-slice/build_assets.py').read_text()
exec(helper[helper.index('def active('):helper.index('reset();legs(6,.91')])
fidelity=(ROOT/'Tools/concept-fidelity-v2/build_assets.py').read_text()
exec(fidelity[fidelity.index('def poly('):fidelity.index('# Custodian V2:')])
def uv_merge(name,lod=False):
 for o in parts:
  for layer in list(o.data.uv_layers):o.data.uv_layers.remove(layer)
  uv=o.data.uv_layers.new(name='WornIndustrial')
  for f in o.data.polygons:
   mi=mats.index(o.data.materials[f.material_index])
   n=f.normal;axis=max(range(3),key=lambda i:abs(n[i]));axes=[i for i in range(3) if i!=axis]
   for li in f.loop_indices:
    v=o.data.vertices[o.data.loops[li].vertex_index].co
    # Broad spatial mapping reveals local scratches/grime; protected tile gutters.
    u=(v[axes[0]]*.31)%1;w=(v[axes[1]]*.31)%1
    uv.data[li].uv=((mi*64+5+u*54)/1024,(5+w*502)/512)
   f.material_index=0
  o.data.materials.clear();o.data.materials.append(atlas)
 bpy.ops.object.select_all(action='DESELECT')
 for o in parts:o.select_set(True)
 bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name=name+'_LOD0' if lod else name
 active(o);tr=o.modifiers.new('Triangulate','TRIANGULATE');bpy.ops.object.modifier_apply(modifier=tr.name)
 meshes=[o]
 if lod:
  for level,ratio in [(1,.55),(2,.25)]:
   q=o.copy();q.data=o.data.copy();bpy.context.collection.objects.link(q);q.name=name+'_LOD'+str(level);active(q)
   d=q.modifiers.new('Mobile LOD','DECIMATE');d.ratio=ratio;d.use_collapse_triangulate=True;bpy.ops.object.modifier_apply(modifier=d.name);meshes.append(q)
 metrics[name]={'triangles':[len(q.data.polygons) for q in meshes],'materials':1,'source':'CC0 donor adaptation / original project authoring'}
 return meshes
def output(name):
 meshes=uv_merge(name)
 active(meshes[0]);bpy.ops.export_scene.fbx(filepath=str(OUT/'Models'/(name+'.fbx')),use_selection=True,object_types={'MESH'},global_scale=1,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',bake_anim=False,path_mode='STRIP',mesh_smooth_type='FACE',use_tspace=True)
def trim_output(name):
 bpy.ops.object.select_all(action='DESELECT')
 for o in parts:o.select_set(True)
 bpy.context.view_layer.objects.active=parts[0]
 if len(parts)>1:bpy.ops.object.join()
 o=bpy.context.object;o.name=name;active(o)
 mod=o.modifiers.new('Triangles','TRIANGULATE');bpy.ops.object.modifier_apply(modifier=mod.name)
 metrics[name]={'triangles':[len(o.data.polygons)],'materials':1,'source':'Quaternius CC0; retained authored trim UVs, atlas and coordinate adaptation'}
 bpy.ops.export_scene.fbx(filepath=str(OUT/'Models'/(name+'.fbx')),use_selection=True,object_types={'MESH'},global_scale=1,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',bake_anim=False,path_mode='STRIP',mesh_smooth_type='FACE',use_tspace=True)
def classify(m,role):
 name=m.name.lower();c=m.diffuse_color
 if any(t in name for t in ('emit','glow','light')):return 7 if 'command' in role.lower() else 6
 if any(t in name for t in ('black','dark','rubber')):return 0
 if any(t in name for t in ('white','silver','grey','gray')):return 1
 if max(c[:3])-min(c[:3])>.15:return 4
 return 1 if sum(c[:3])>1.2 else 0
entries=json.loads((DOCS/'ingestion_manifest.json').read_text(encoding='utf-8'))
for entry in entries:
 if not entry['sourcePath'].endswith('.fbx'):continue
 assert entry['license']=='CC0'
 p=ROOT/entry['sourcePath'];assert hashlib.sha256(p.read_bytes()).hexdigest()==entry['sourceSha256']
 reset();bpy.ops.import_scene.fbx(filepath=str(p))
 imported=[o for o in bpy.context.scene.objects if o.type=='MESH']
 # Enemy sources are donor libraries only. Export selected upper shell surfaces,
 # excluding stock legs/rigs; custom role shells are authored in the actor pass.
 if 'enemy' in entry['outputName']:continue
 for o in imported:
  active(o)
  for mod in list(o.modifiers):
   if mod.type=='ARMATURE':o.modifiers.remove(mod)
  world=o.matrix_world.copy();o.parent=None;o.matrix_world=Matrix.Identity(4)
  o.data.transform(world)
  original=list(o.data.materials);mapping=[classify(m,entry['role']) if m else 0 for m in original]
  if entry['sourceId']=='env-quaternius-megakit':
   uv=o.data.uv_layers.active
   for f in o.data.polygons:
    mn=original[f.material_index].name;tile=0 if '01' in mn else 1 if '02' in mn else 2 if '03' in mn else 3
    for li in f.loop_indices:
     a,b=uv.data[li].uv
     # Keep the original surface motifs; atlas quadrants have 4px guard margins.
     a=max(0,min(1,a));b=max(0,min(1,b))
     uv.data[li].uv=((tile%2+.004+.992*a)/2,(tile//2+.004+.992*b)/2)
    f.material_index=0
   o.data.materials.clear();o.data.materials.append(trim);parts.append(o);continue
  o.data.materials.clear()
  for m in mats:o.data.materials.append(m)
  for f in o.data.polygons:f.material_index=mapping[f.material_index] if mapping else 1
  o.vertex_groups.clear();parts.append(o)
 # All delivered modules have a ground-centered pivot in authoring metres.
 coords=[v.co for o in parts for v in o.data.vertices]
 lo=Vector(tuple(min(v[i] for v in coords) for i in range(3)));hi=Vector(tuple(max(v[i] for v in coords) for i in range(3)))
 offset=Vector(((lo.x+hi.x)/2,(lo.y+hi.y)/2,lo.z))
 for o in parts:
  for v in o.data.vertices:v.co-=offset
 if entry['sourceId']=='env-quaternius-megakit':trim_output(entry['outputName'])
 else:output(entry['outputName'])
 print('ADAPTED',entry['outputName'],metrics[entry['outputName']])
# Connected four-metre service route with endpoint flanges and load brackets.
reset()
for j in range(3):
 x=(j-1)*.24
 rod('Pressure service',(x,-2,.12),(x,2,.12),.061 if j!=1 else .084,0,vertices=10)
 for y in (-1.95,1.95):ring('Bolted endpoint',(x,y,.12),.10,.018,1,axis=(0,1,0))
for y in (-1.6,0,1.6):
 box('Mounted saddle',(0,y,.03),(.94,.19,.06),1,bevel=.006)
 for x in (-.43,.43):rod('Hex fastener',(x,y,.055),(x,y,.09),.034,2,vertices=6)
box('Inset service identifier',(0,-.25,.21),(.035,.42,.012),7,bevel=0)
output('VR3_ServiceRun')
# Original grated service well: opaque recess and strong frame thickness.
reset();box('Dark service well',(0,0,-.15),(1.3,4,.3),8)
for s in (-1,1):box('Frame lip',(s*.62,0,.02),(.12,4,.10),1,bevel=.012)
for i in range(21):box('Grate slat',(0,-1.9+i*.19,.015),(1.12,.042,.07),10,bevel=.004)
for x in (-.33,.33):rod('Undergrate service',(x,-1.94,-.12),(x,1.94,-.12),.052,0,vertices=8)
output('VR3_ServiceGrate')
# Damaged floor layer with irregular panel seams and visible reinforcement.
reset();rng=random.Random(4403)
box('Thick dark subdeck',(0,0,-.055),(6,6,.1),8,bevel=.008)
for iy in range(3):
 for ix in range(3):
  x=-2+ix*2;y=-2+iy*2;cut=rng.uniform(.18,.43)
  poly('Fractured steel plate',[(x-.97,y-.97),(x+.78,y-.97),(x+.97,y-.64),(x+.97,y+.67),(x+.64,y+.97),(x-.97,y+.97),(x-.97,y+.15),(x-.97+cut,y-.05)],.006,.075,3 if (ix+iy)%2 else 14,bevel=.012)
  for s in (-1,1):rod('Countersunk panel lock',(x+s*.75,y-.77,.051),(x+s*.75,y-.77,.062),.029,1,vertices=6)
  rod('Scraped seam lip',(x-.75,y+.91,.054),(x+.55,y+.91,.054),.010,1,vertices=6)
output('VR3_FracturedDeck')
(DOCS/'mesh_metrics.json').write_text(json.dumps(metrics,indent=2),encoding='utf-8')
print('VISUAL_REPLACEMENT_V3_STATIC_EXPORT_PASS',len(metrics))
