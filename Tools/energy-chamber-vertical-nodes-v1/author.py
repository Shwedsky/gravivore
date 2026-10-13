"""Vertical distribution module and core insert; accepted assembly stays intact."""
import bpy,bmesh,math,json,zipfile
from pathlib import Path
root=Path(__file__).resolve().parents[2]
base=root/'Tools/energy-chamber-hybrid-proof-v1/author.py'
ns={'__file__':str(base)}
# Reuse mesh helpers only; do not run the original structural authoring.
# Single bevel cuts on these small fittings preserve crisp surfaces efficiently.
exec(base.read_text().split('# Height zones')[0].replace('mod.segments=3','mod.segments=1'),ns)
out=root/'Assets/_Game/ArtProof/EnergyChamber'
evidence=root/'docs/history/implementation-passes/energy-chamber-vertical-nodes-v1'
source=root/'art/energy-chamber-vertical-nodes-v1';source.mkdir(parents=True,exist_ok=True)
for name,color in [('CoreWarm',(1,.25,.02,1)),('CoreInnerWarm',(.65,.14,.015,1)),('NodeCool',(.04,.47,.7,1)),('Copper',(.42,.26,.13,1)),('Gasket',(.04,.04,.04,1)),('Plate',(.24,.28,.25,1))]:
    m=bpy.data.materials.new('EC_'+name);m.diffuse_color=color;ns['MATS'][name]=m
def group(name):ns['GROUP']=name
def box(*a,**k):return ns['box'](*a,**k)
def lathe(*a,**k):return ns['lathe'](*a,**k)
def arc(*a,**k):return ns['arc'](*a,**k)
def tube(*a,**k):return ns['tube'](*a,**k)
def stud(x,y,z):return lathe('captive flange fastener',[(z-.018,.03),(z+.018,.03)],'Steel',6,(x,y,0))
cx,cy=.04,.10 # Blender +Y corresponds to Unity -Z, matching retained reservoir.

# Direct inventory/geometry review: retain the accepted compressor, centrifuge
# and holder. The source Cryo Tube is a low-detail closed/domed capsule, so a
# fitted open-frame cylindrical stabilizer is authored as the node's new primary.
intake=root.parents[1]/'ExternalAssetIntake/Current'
candidates=[]
for package,archive in [('molten-maps-scifi','Molten Maps SciFi Asset Pack.zip'),('quaternius-modular-scifi-megakit-standard','Modular_SciFi_MegaKit_Standard.zip')]:
    p=intake/package/archive
    with zipfile.ZipFile(p) as z:candidates += [dict(archive=str(p),entry=n) for n in z.namelist() if n.lower().endswith('.fbx') and any(t in n.lower() for t in ['cryo','column_pipe','generator'])]
(evidence/'source-review.json').write_text(json.dumps(dict(candidates=candidates,decision='No new external import. Existing horizontal reservoir machinery remains. Custom open-frame stabilizer is a silhouette-defining primary element, fitted to the retained node/platform/manifold. Actual source capsule and generator geometry were reviewed; the former is not sufficient as the final cylindrical energy presentation.'),indent=2))

group('VerticalDistribution')
# Two clear load paths straddle the reservoir, leaving its useful machinery and
# original transformation intact. Cross beams clear the 1.881 m donor top.
for x in [-.84,.89]:
    box('gantry deck bolted foot',(x,cy,.56),(.36,.40,.14),'Plate',.025)
    box('gantry load-bearing web',(x,cy,1.32),(.12,.22,1.38),'Graphite',.018)
    for y in [cy-.105,cy+.105]:box('gantry exposed steel flange',(x,y,1.32),(.23,.05,1.42),'Steel',.012)
    box('gantry bearing head',(x,cy,2.025),(.32,.43,.14),'Plate',.024)
    for y in [cy-.13,cy+.13]:stud(x,y,.65)
for y in [cy-.19,cy+.19]:box('paired stabilizer bearing bridge',(cx,y,2.05),(1.98,.13,.17),'Plate',.022)
lathe('receiver lower transfer seat',[(2.135,.48),(2.20,.55),(2.29,.55),(2.36,.41)],'Graphite',64,(cx,cy,0))
ring=arc('receiver machined locking flange',.575,.09,2.205,2.27,mat='Steel');ring.location=(cx,cy,0)
ring=arc('receiver pressure gasket',.43,.045,2.365,2.395,mat='Gasket');ring.location=(cx,cy,0)
for a in range(0,360,45):
    t=math.radians(a);stud(cx+.50*math.cos(t),cy+.50*math.sin(t),2.295)

lathe('visible internal cylindrical stabilizer',[(2.37,.26),(2.45,.315),(3.76,.315),(3.85,.25)],'NodeCool',64,(cx,cy,0))
for z in [2.56,2.96,3.36,3.73]:
    ring=arc('internal induction compression turn',.343,.023,z,z+.040,mat='Copper');ring.location=(cx,cy,0)
for a in range(0,360,60):
    t=math.radians(a);c,s=math.cos(t),math.sin(t)
    tube('formed containment load brace',[(cx+.45*c,cy+.45*s,2.28),(cx+.51*c,cy+.51*s,2.51),(cx+.51*c,cy+.51*s,3.70),(cx+.44*c,cy+.44*s,3.93)],.047,'Graphite')
    for z in [2.48,3.73]:box('containment locking collar',(cx+.50*c,cy+.50*s,z),(.13,.14,.15),'Steel',.014,a)
    # Narrow armor sectors keep the cylindrical core visible between the braces.
    shell=arc('segmented outer containment shield',.445,.025,2.61,3.56,a+17,15,'Shell');shell.location=(cx,cy,0)
    for z in [2.66,3.51]:stud(cx+.435*math.cos(t+.38),cy+.435*math.sin(t+.38),z)
for z in [2.61,3.14,3.60]:
    ring=arc('circumferential structural restraint',.485,.045,z,z+.062,mat='Plate');ring.location=(cx,cy,0)
lathe('stabilizer crown pressure termination',[(3.83,.39),(3.89,.54),(4.00,.54),(4.055,.41),(4.12,.24)],'Graphite',64,(cx,cy,0))
ring=arc('crown machined band',.545,.075,3.915,3.97,mat='Steel');ring.location=(cx,cy,0)
for a in range(0,360,90):
    ring=arc('crown cool status aperture',.375,.075,4.083,4.103,a+8,54,'NodeCool');ring.location=(cx,cy,0)
    t=math.radians(a);box('crown radial clamp',(cx+.40*math.cos(t),cy+.40*math.sin(t),4.027),(.20,.09,.07),'Copper',.01,a)

# Twin existing fluid routes branch through a keyed junction, then rise into the
# stabilizer's collector seat. The old reservoir feed remains physically useful.
for y in [-.21,.21]:
    box('manifold branch interface',(-.94,y,1.18),(.19,.22,.20),'Plate',.022)
    tube('manifold to stabilizer feed',[(-1.32,y,1.18),(-.84,y,1.18),(-.84,y,1.70),(-.65,y,2.04),(-.47,y,2.28)],.059,'Steel')
    for z in [1.42,1.76]:
        collar=lathe('keyed vertical feed coupling',[(z-.03,.072),(z+.03,.072)],'Copper',24,(-.84,y,0))
    box('riser retained clamp',(-.84,y,1.62),(.17,.17,.12),'Graphite',.016)
tube('stabilizer reservoir equalization return',[(.55,.31,2.28),(.83,.31,1.99),(.83,.31,1.46),(.55,.31,1.35)],.055,'Steel')
box('reservoir return flange',(.55,.31,1.35),(.14,.17,.17),'Plate',.02)
for y in [.38,.42]:tube('protected stabilizer telemetry cable',[(-1.1,y,1.04),(-.97,y,1.24),(-.94,y,1.81),(-.52,y,2.18)],.018,'Gasket')

group('CoreHeart')
# Same central vessel envelope. This surface sits just outside the accepted dark
# sleeve, visible through its armor windows. No shell, frame or ring is moved.
lathe('contained central orange energy surface',[(2.58,1.033),(2.66,1.047),(4.75,1.047),(4.82,1.033)],'CoreInnerWarm',96)

text=bpy.data.texts.new('SCOPE');text.write('Project-owned vertical stabilizer and central energy insert. Four accepted primary meshes and all existing donor source meshes remain unchanged. No stock source geometry is embedded in this Blender file.')
bpy.ops.wm.save_as_mainfile(filepath=str(source/'EnergyChamber_VerticalNodesV1.blend'))
rows=[]
for name in ['VerticalDistribution','CoreHeart']:
    objs=[o for o in bpy.context.scene.objects if o.type=='MESH' and o.name.startswith(name+' / ')]
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs:o.select_set(True)
    bpy.context.view_layer.objects.active=objs[0]
    if len(objs)>1:bpy.ops.object.join()
    o=bpy.context.object;o.name='EC_'+name
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    bm=bmesh.new();bm.from_mesh(o.data);bmesh.ops.triangulate(bm,faces=list(bm.faces));bm.to_mesh(o.data);bm.free()
    bpy.ops.export_scene.fbx(filepath=str(out/'Models'/f'EC_{name}.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',bake_anim=False,add_leaf_bones=False,path_mode='STRIP',use_tspace=True)
    rows.append(dict(group=name,triangles=len(o.data.polygons),parts=len(objs)))
(evidence/'geometry.json').write_text(json.dumps(rows,indent=2))
print('VERTICAL_DISTRIBUTION_MODULE_AUTHORED',rows)
