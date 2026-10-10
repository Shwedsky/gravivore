"""Read exact canonical donor bytes without saving or copying restricted art.

blender --background --factory-startup --disable-autoexec --python this.py --
    --intake <Current/_actor-audit-v1> --output <evidence directory>
"""
import bpy, json, hashlib, argparse, sys
from pathlib import Path
from mathutils import Vector

p=argparse.ArgumentParser(); p.add_argument('--intake',type=Path,required=True); p.add_argument('--output',type=Path,required=True)
a=p.parse_args(sys.argv[sys.argv.index('--')+1:]); a.output.mkdir(parents=True,exist_ok=True)
sources=[
 ('Scout','quaternius-animated-mech-pack','Animated Mech Pack - March 2021/Textured/Blends/George.blend','55e24fbae28736f87684129d2f3e27e0e3ba246cc8e91668822d0b3faf155b0c'),
 ('Warden','msgdi-medium-mech-striker','Assets/MediumMechStriker/FBX/MediumMechStriker/MediumMechStriker.fbx','b0cea6976225126369264bb4ace54f52b4d3f46f2ebaec9b997703ccafc17e91'),
 ('Carrier','unityfan-scifi-vehicle-012','source/sci-fi_vehicle_013_2.blend','c3b6164b80876271a2569f41e2661c7fc030effc4b6d8558819dd40dee9393c6'),
 ('Cutter','quaternius-scifi-essentials','FBX (Unity)/Enemy_QuadShell.fbx','823ff8d076f4f99be412cf6854e8e85763efb06cc8e82f4bca8fd93b8a7407a0'),
 ('ArcDrone','quaternius-scifi-essentials','FBX (Unity)/Enemy_EyeDrone.fbx','2ed79c0e1bf99052cbc769942033a637d63afb89496c1a46ff3031f3ef4916f2'),
 ('Magnetar','quaternius-scifi-essentials','FBX (Unity)/Enemy_Trilobite.fbx','015e1424f20afacd7fcd57a93b95418fd0745f849ee81790ea3853facc19f5e8'),
]
rows=[]
for actor,source,member,expected in sources:
 path=a.intake/source/member; digest=hashlib.sha256(path.read_bytes()).hexdigest()
 if digest!=expected: raise RuntimeError('Source hash changed: '+str(path))
 bpy.ops.wm.read_factory_settings(use_empty=True)
 if path.suffix=='.blend': bpy.ops.wm.open_mainfile(filepath=str(path),load_ui=False,use_scripts=False)
 else: bpy.ops.import_scene.fbx(filepath=str(path))
 meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']; deps=bpy.context.evaluated_depsgraph_get(); pts=[]; triangles=0
 for o in meshes:
  ev=o.evaluated_get(deps); me=ev.to_mesh(); me.calc_loop_triangles(); triangles+=len(me.loop_triangles)
  pts += [o.matrix_world@v.co for v in me.vertices]; ev.to_mesh_clear()
 rigs=[{'name':o.name,'bones':[{'name':b.name,'parent':b.parent.name if b.parent else None,'head':list(b.head_local),'tail':list(b.tail_local)} for b in o.data.bones]} for o in bpy.context.scene.objects if o.type=='ARMATURE']
 rows.append({'actor':actor,'source_id':source,'member':member,'sha256':digest,'evaluated_triangles':triangles,'dimensions':[max(v[i] for v in pts)-min(v[i] for v in pts) for i in range(3)],'rigs':rigs,'actions':[{'name':x.name,'frames':list(x.frame_range)} for x in bpy.data.actions],'reuse':'Inspection only. No donor vertices, skin weights, textures, bone definitions or keyframes copied.'})
 print('INSPECTED',actor,triangles,flush=True)
(a.output/'DONOR_REINSPECTION.json').write_text(json.dumps({'blender':bpy.app.version_string,'records':rows,'custodian':'Custom architecture; inspected heavy joints from the Magnetar component reference, no full boss donor.'},indent=2))
