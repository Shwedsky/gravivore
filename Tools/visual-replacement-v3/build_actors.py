"""Original role-specific production shells around the accepted mechanical rigs.

Reuses project-owned baseline construction and preserves mesh/clip/bone names.
CC0 Essentials is a mechanism reference; no Asset Store geometry is imported.
"""
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
script=(ROOT/'Tools/visual-replacement-v3/build_assets.py').read_text()
exec(script[:script.index('entries=json.loads')])
def skin_mesh(name,rig=None,lod=False):
 meshes=uv_merge(name,lod)
 for q in meshes:
  if rig:
   mod=q.modifiers.new('Rigid mechanical skin','ARMATURE');mod.object=rig;q.parent=rig
  else:q.vertex_groups.clear()
 metrics[name]['bones']=len(bones) if rig else 0
 return meshes
SOURCE=ROOT/'art/visual-replacement-v3'
manifest=json.loads((DOCS/'ingestion_manifest.json').read_text())
def donor_plating(role,center,size):
 entry=next(e for e in manifest if e['role']==role)
 path=ROOT/entry['sourcePath'];assert hashlib.sha256(path.read_bytes()).hexdigest()==entry['sourceSha256']
 previous=set(bpy.data.objects);bpy.ops.import_scene.fbx(filepath=str(path))
 imported=[o for o in bpy.data.objects if o not in previous]
 selected=[]
 for o in imported:
  if o.type!='MESH':continue
  world=o.matrix_world.copy();o.parent=None;o.matrix_world=Matrix.Identity(4);o.data.transform(world)
  bm=bmesh.new();bm.from_mesh(o.data);bm.normal_update()
  zmin=min(v.co.z for v in bm.verts);zmax=max(v.co.z for v in bm.verts)
  discard=[f for f in bm.faces if f.normal.z<.30 or f.calc_center_median().z<zmin+(zmax-zmin)*.55]
  bmesh.ops.delete(bm,geom=discard,context='FACES');bm.to_mesh(o.data);bm.free()
  if not o.data.polygons:bpy.data.objects.remove(o,do_unlink=True);continue
  selected.append(o)
 for o in imported:
  if o not in selected and o.name in bpy.data.objects:bpy.data.objects.remove(o,do_unlink=True)
 if not selected:raise RuntimeError('Donor has no suitable independent upper plating')
 bpy.ops.object.select_all(action='DESELECT')
 for o in selected:o.select_set(True)
 bpy.context.view_layer.objects.active=selected[0];bpy.ops.object.join();o=bpy.context.object
 coords=[v.co for v in o.data.vertices];lo=Vector(tuple(min(v[i] for v in coords) for i in range(3)));hi=Vector(tuple(max(v[i] for v in coords) for i in range(3)))
 for v in o.data.vertices:
  for i in range(3):v.co[i]=(v.co[i]-(hi[i]+lo[i])*.5)*size[i]/max(.001,hi[i]-lo[i])+center[i]
 o.name='CC0 '+role+' selectively extracted shell';o.data.materials.clear();o.data.materials.append(mats[2]);o.vertex_groups.clear()
 group=o.vertex_groups.new(name='BODY');group.add(list(range(len(o.data.vertices))),1,'REPLACE');parts.append(o)
first=(ROOT/'Tools/first-visual-slice/build_assets.py').read_text()
for name,start,end in [('Scout_V1','reset();legs(6,.91',"rig_and_export('Scout_V1')"),('Cutter_V1','reset();legs(4,1.08',"rig_and_export('Cutter_V1')"),('Magnetar_V1','reset();legs(6,1.72',"rig_and_export('Magnetar_V1')")]:
 OUT=ROOT/'Assets/_Game/Content/VisualSlice'
 exec(first[first.index(start):first.index(end)])
 if name=='Scout_V1':
  donor_plating('Enemy_QuadShell',(0,.12,.74),(.46,.58,.10))
  for s in (-1,1):
   poly('Independent swept recon shell',[(s*.09,-.33),(s*.34,-.13),(s*.31,.35),(s*.10,.48)],.69,.09,2)
   rod('Exposed sensor suspension',(s*.12,-.16,.50),(s*.23,-.43,.60),.025,1,vertices=8)
  ring('Top visible hostile focal core',(0,-.15,.71),.092,.024,0,'CORE')
  dome('Recessed hostile top optic',(0,-.15,.73),(.064,.064,.033),5,'CORE')
 elif name=='Cutter_V1':
  for s,label in [(-1,'R'),(1,'L')]:
   B=label+'_BLADE'
   poly('Large independent forward blade',[(s*.77,-.57),(s*1.12,-.85),(s*1.00,-1.67),(s*.63,-2.02),(s*.78,-1.18)],.48,.18,2,B,bevel=.017)
   shell('Reinforced cutting guard',(s*.66,-.32,.61),.43,.54,.27,0,B)
   ring('Blade drive visible rotor',(s*.64,-.32,.76),.15,.025,1,B)
   rod('Attack source blade channel',(s*.84,-.96,.59),(s*.77,-1.50,.59),.027,5,B,vertices=8)
  shell('Forward split armor breast',(0,-.37,.92),.78,.64,.14,1)
 elif name=='Magnetar_V1':
  # Broad electromagnetic engine, low-six-support stance and independent banks.
  for s,label in [(-1,'R'),(1,'L')]:
   B=label+'_CLAW'
   shell('Magnetic containment side bank',(s*.96,.09,1.38),.65,1.38,.29,0)
   poly('Layered elite side glacis',[(s*.65,-.34),(s*1.32,-.14),(s*1.25,.80),(s*.74,.94)],1.57,.16,2)
   for j in range(4):
    box('Dense magnetic lamination',(s*1.24,-.20+j*.27,1.36),(.15,.065,.23),1,bevel=.008)
    ring('Contained magnetic coil',(s*.73,-.10+j*.25,1.63),.13,.024,1)
   rod('Elite heavy stabilizer piston',(s*.96,-.13,.90),(s*1.56,-.83,.40),.079,1,B,vertices=12)
   hose('Contained power service',[(s*.24,-.43,1.34),(s*.49,-.10,1.51),(s*.85,.15,1.51)],.030,8)
  ring('Dorsal amber containment',(0,-.30,1.56),.27,.067,0,'CORE')
  dome('Obvious contained amber reactor',(0,-.30,1.57),(.18,.18,.075),6,'CORE')
  shell('Rear magnetic bridge',(0,.67,1.34),.96,.48,.28,1)
 rig_and_export(name)
production=(ROOT/'Tools/chapter01-production/build_assets.py').read_text()
exec(production[production.index('def grille('):production.index('# Arc Drone:')])
for name,start,end in [('ArcDrone_V1','# Arc Drone:',"rig_and_export('ArcDrone_V1')"),('Carrier_V1','# Carrier:',"rig_and_export('Carrier_V1')"),('Warden_V1','# Warden:',"rig_and_export('Warden_V1')")]:
 OUT=ROOT/'Assets/_Game/Content/Chapter01Production'
 exec(production[production.index(start):production.index(end)])
 if name=='ArcDrone_V1':
  for s in (-1,1):
   poly('Original angular arc fin',[(s*.25,-.53),(s*.73,-.76),(s*1.0,-.15),(s*.50,.13)],1.23,.10,2,bevel=.010)
   rod('Arc emitter electrode',(s*.19,-.49,1.15),(s*.19,-.79,1.18),.037,1,vertices=8)
  ring('Dorsal arc energy core',(0,-.16,1.44),.13,.035,0,'CORE');dome('Arc hostile heart',(0,-.16,1.45),(.09,.09,.05),5,'CORE')
 elif name=='Warden_V1':
  donor_plating('Enemy_Trilobite',(0,.30,1.43),(1.17,.95,.13))
  for s in (-1,1):
   for j in range(3):shell('Overlapping defensive carapace',(s*.58,-.22+j*.34,1.20-j*.08),.56,.45,.17,2)
   rod('Top shield tensioner',(s*.64,-.55,1.18),(s*.54,.37,1.03),.046,0,vertices=10)
  shell('Broad bastion top plate',(0,-.25,1.31),.88,.55,.15,1)
 else:
  for s in (-1,1):
   rod('Supported cargo gantry',(s*.58,.06,1.30),(s*.58,.88,1.30),.070,0,vertices=10)
   shell('Machine load side radiator',(s*.52,.49,1.49),.20,.99,.24,1)
   for j in range(5):box('Cargo radiator lamella',(s*.52,.12+j*.17,1.62),(.26,.045,.08),2,bevel=.008)
  shell('Loaded pressure machinery',(0,.50,1.55),.76,.78,.26,0)
  ring('Load hazard cap',(0,.50,1.70),.22,.037,1);dome('Industrial hostile load energy',(0,.50,1.71),(.15,.15,.05),6)
 rig_and_export(name)
# Preserve Custodian's imported phase articulation, shutters and exact socket family.
v3=(ROOT/'Tools/chapter01-v3/build_assets.py').read_text()
boss=fidelity[fidelity.index("reset();bone('BODY'"):fidelity.index("export('Custodian_V2',True)")]
OUT=ROOT/'Assets/_Game/Content/Chapter01V3';exec(boss)
exec(v3[v3.index('for s,label in [(-1,\'R\'),(1,\'L\')]:'):v3.index("export('Custodian_V3',True)")])
for s,label in [(-1,'R'),(1,'L')]:
 B=label+'_SHUTTER'
 shell('Independent industrial reactor shoulder',(s*1.05,.60,2.71),.83,1.32,.30,0,B)
 poly('Swept boss heavy containment cap',[(s*.70,-.16),(s*1.30,-.08),(s*1.49,.91),(s*.88,1.25)],2.91,.17,2,B)
 for j in range(5):box('Reactor heat exchange rack',(s*1.36,.02+j*.23,2.72),(.15,.065,.34),1,B,bevel=.008)
 hose('Boss pressure service',[(s*.36,.15,2.37),(s*.69,.53,2.77),(s*1.0,.84,2.78)],.046,8,B)
 for j in range(3):rod('Pressure bank exposed piston',(s*1.12,-.14+j*.20,2.31),(s*1.44,-.14+j*.20,2.25),.065,1,B,vertices=10)
rod('Asymmetric command exhaust',(1.10,1.15,2.42),(1.10,1.15,3.24),.13,0,vertices=12)
ring('Exhaust castellated head',(1.1,1.15,3.22),.17,.035,1)
export('Custodian_V3',True)
(DOCS/'actor_metrics.json').write_text(json.dumps(metrics,indent=2))
print('ROLE_SHELL_EXPORT_PASS')
