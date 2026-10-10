"""Reopen delivered sources and verify geometry, skin, clips and portable maps."""
import bpy, json, hashlib, math, numpy as np
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
E=ROOT/'docs/history/implementation-passes/chapter01-actor-production-v2'
records=[]
for actor in ['Scout','Cutter','Warden','ArcDrone','Carrier','Magnetar','Custodian']:
 path=ROOT/'art/chapter01-actor-production-v2'/(actor+'_V2.blend')
 bpy.ops.wm.open_mainfile(filepath=str(path),load_ui=False)
 rig=bpy.data.objects['RIG']; skins=sorted([o for o in bpy.data.objects if o.name.startswith(actor+'_LOD')],key=lambda o:o.name)
 assert len(skins)==3 and len(rig.data.bones)>3
 record={'actor':actor,'source_sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'lods':[],'clips':[],'maps':[],'motion_min_z':{}}
 for skin in skins:
  me=skin.data; me.calc_loop_triangles()
  assert len(me.materials)==1 and len(me.uv_layers)==1, (actor,skin.name,'material/UV counts',len(me.materials),len(me.uv_layers))
  assert all(len(v.groups)==1 and abs(v.groups[0].weight-1)<1e-5 for v in me.vertices)
  assert all(all(math.isfinite(c) for c in v.co) for v in me.vertices)
  zero=sum(((me.vertices[t.vertices[1]].co-me.vertices[t.vertices[0]].co).cross(me.vertices[t.vertices[2]].co-me.vertices[t.vertices[0]].co)).length_squared<1e-24 for t in me.loop_triangles)
  assert zero==0, (actor,skin.name,'zero area triangles',zero)
  assert skin.parent==rig and next(m for m in skin.modifiers if m.type=='ARMATURE').object==rig
  record['lods'].append({'name':skin.name,'triangles':len(me.loop_triangles),'vertices':len(me.vertices),'zero_area_triangles':zero,'one_rigid_weight_per_vertex':True})
 assert record['lods'][0]['triangles']>record['lods'][1]['triangles']>record['lods'][2]['triangles']
 for image in bpy.data.images:
  if image.source=='FILE':
   assert image.packed_file is not None, (actor,image.name,'not packed')
   assert image.filepath.replace('\\','/').startswith('//../../Assets/_Game/ArtReview/ActorProductionV2/Textures/'), (actor,image.name,image.filepath)
   record['maps'].append(image.name)
 assert len(record['maps'])>=4
 for action in bpy.data.actions:
  if not action.name.startswith(actor+'|'): continue
  clip=action.name.split('|')[-1]; rig.animation_data.action=action
  root_positions=[]; minima=[]
  start,end=map(int,action.frame_range)
  for frame in range(start,end+1):
   bpy.context.scene.frame_set(frame); bpy.context.view_layer.update()
   root_positions.append(list(rig.pose.bones['ROOT'].matrix.translation))
   evaluated=skins[0].evaluated_get(bpy.context.evaluated_depsgraph_get()); mesh=evaluated.to_mesh()
   coordinates=np.empty(len(mesh.vertices)*3,dtype=np.float32); mesh.vertices.foreach_get('co',coordinates)
   matrix=np.asarray(skins[0].matrix_world)
   minima.append(float((coordinates.reshape(-1,3)@matrix[2,:3]+matrix[2,3]).min())); evaluated.to_mesh_clear()
  assert all(Vector(p).length<1e-6 for p in root_positions), (actor,clip,'root travels')
  record['clips'].append({'name':clip,'frames':list(action.frame_range),'root_in_place':True})
  record['motion_min_z'][clip]=min(minima)
  assert min(minima)>.01, (actor,clip,'posed mesh penetrates floor',min(minima))
 for required in ['Idle','Move','Attack','Hit','Death']: assert any(x['name']==required for x in record['clips'])
 records.append(record)
(E/'SOURCE_VALIDATION.json').write_text(json.dumps({'blender':bpy.app.version_string,'portable_packed_sources':True,'actors':records},indent=2))
print('ACTOR_SOURCE_VALIDATION_PASS',flush=True)
