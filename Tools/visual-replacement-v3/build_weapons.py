"""M-0 mech emitter ranks. CC0 barrel/body donors, no grips or stocks."""
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
script=(ROOT/'Tools/visual-replacement-v3/build_assets.py').read_text()
exec(script[:script.index('entries=json.loads')])
entries=json.loads((DOCS/'ingestion_manifest.json').read_text())
def donor(key,c,size):
 entry=next(e for e in entries if e['outputName']==key)
 assert entry['license']=='CC0';path=ROOT/entry['sourcePath'];assert hashlib.sha256(path.read_bytes()).hexdigest()==entry['sourceSha256']
 before=set(bpy.context.scene.objects);bpy.ops.import_scene.fbx(filepath=str(path))
 obs=[o for o in bpy.context.scene.objects if o not in before and o.type=='MESH']
 coords=[]
 for o in obs:
  world=o.matrix_world.copy();o.parent=None;o.matrix_world=Matrix.Identity(4)
  o.data.transform(world)
  coords.extend(v.co for v in o.data.vertices)
 lo=Vector(tuple(min(p[i] for p in coords) for i in range(3)));hi=Vector(tuple(max(p[i] for p in coords) for i in range(3)))
 center=(lo+hi)*.5;scale=Vector(tuple(size[i]/max(.001,hi[i]-lo[i]) for i in range(3)))
 for o in obs:
  for v in o.data.vertices:v.co=Vector(tuple((v.co[i]-center[i])*scale[i]+c[i] for i in range(3)))
  o.data.materials.clear();o.data.materials.append(mats[1]);o.vertex_groups.clear();parts.append(o)
for rank in range(1,6):
 reset();length=.98+(rank-1)*.16
 shell('Loadbearing mech hardpoint',(0,.12,0),.34,.42,.30,0)
 shell('Angled external emitter housing',(0,-.22,.03),.46,.78,.34,2)
 donor('VR3_bodyar1',(0,-.22,.09),(.35,.54,.26))
 donor('VR3_barrelar1' if rank<3 else 'VR3_barrelsniper1',(0,-length+.20,.02),(.30,.62,.30))
 rod('Cyan contained chamber',(0,-.23,.025),(0,-length+.15,.025),.099,7,vertices=16)
 for s in (-1,1):
  shell('Supported split barrel armor',(s*.19,-length*.58,.055),.15,length*.68,.26,0)
  rod('Machined pressure rail',(s*.21,-.35,.17),(s*.21,-length+.12,.17),.025,1,vertices=8)
 for j in range(rank+1):
  y=-.4-j*(length-.44)/(rank+1)
  ring('Emitter containment coil',(0,y,.025),.16,.025,1,axis=(0,1,0))
 ring('Muzzle armoured crown',(0,-length,.025),.17,.04,0,axis=(0,1,0))
 ring('Visible cyan emission aperture',(0,-length-.005,.025),.095,.018,7,axis=(0,1,0))
 if rank>=2:
  for s in (-1,1):shell('Secondary capacitor',(s*.27,-.11,.08),.16,.45,.25,1)
 if rank>=3:
  shell('Top energy conditioning module',(0,-.12,.30),.33,.42,.15,0)
  for j in range(4):box('Cooling lamella',(0,-.25+j*.08,.395),(.25,.035,.025),1,bevel=.003)
 if rank>=4:
  for s in (-1,1):
   rod('External stabilized barrel strut',(s*.32,.05,.15),(s*.29,-length+.13,.15),.045,0,vertices=8)
   hose('Shielded power service',[(s*.27,.19,-.04),(s*.37,-.12,-.09),(s*.30,-length+.24,.02)],.025,8)
 if rank==5:
  for s in (-1,1):poly('Premium swept radiator',[(s*.27,-.05),(s*.48,-.18),(s*.41,-.76),(s*.29,-.93)],.24,.10,2,bevel=.012)
  ring('Forward compression stage',(0,-length+.24,.025),.24,.034,0,axis=(0,1,0))
 output('M0_Rank'+str(rank))
(DOCS/'weapon_metrics.json').write_text(json.dumps(metrics,indent=2))
print('M0_RANK_FAMILY_EXPORT_PASS')
