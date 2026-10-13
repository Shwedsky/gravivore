"""Add fitted overlay detail. Never export or change the accepted primary FBXs."""
import bpy,bmesh,math,json,hashlib,zipfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
base=ROOT/'Tools/energy-chamber-hybrid-proof-v1/author.py'
# Reuse only the accepted mesh helpers, not its structure-authoring/export code.
ns={'__file__':str(base)}
exec(base.read_text().split('# Height zones')[0],ns)
globals().update({k:v for k,v in ns.items() if not k.startswith('__')})
PASS=ROOT/'docs/history/implementation-passes/energy-chamber-visual-polish-v1'
SOURCE=ROOT/'art/energy-chamber-visual-polish-v1';SOURCE.mkdir(parents=True,exist_ok=True)
for name,color in [('Copper',(.42,.26,.13,1)),('Gasket',(.04,.04,.04,1)),('Plate',(.24,.28,.25,1)),('Markings',(.28,.3,.27,1))]:
    m=bpy.data.materials.new('EC_'+name);m.diffuse_color=color;ns['MATS'][name]=m
def group(name):ns['GROUP']=name
def radial(a,r,z):return (r*math.cos(math.radians(a)),r*math.sin(math.radians(a)),z)
def rbox(name,a,r,z,size,mat='Plate',bevel=.015):return ns['box'](name,radial(a,r,z),size,mat,bevel,a)
def stud(pos,axis='Z'):
    o=ns['lathe']('flush hex fastener',[(-.012,.032),(.020,.032)],'Steel',6,pos)
    if axis=='X':o.rotation_euler.y=math.pi/2
def plaque(name,pos,size,yaw=0):
    o=ns['box'](name,pos,size,'Markings',.004,yaw)
    # Every face gets a complete atlas; visible top / vertical plate carries text.
    uv=o.data.uv_layers.active
    for p in o.data.polygons:
        axes=sorted(range(3),key=lambda i:abs(p.normal[i]))[:2]
        coords=[o.data.vertices[o.data.loops[l].vertex_index].co for l in p.loop_indices]
        lo=[min(v[a] for v in coords) for a in axes];hi=[max(v[a] for v in coords) for a in axes]
        for l in p.loop_indices:
            v=o.data.vertices[o.data.loops[l].vertex_index].co
            uv.data[l].uv=[(v[a]-lo[j])/max(.001,hi[j]-lo[j]) for j,a in enumerate(axes)]
    return o

def sloped_sector(name,r0,z0,r1,z1,start,span,mat):
    # A fitted cover follows the accepted crown's taper, rather than sitting
    # inside its opaque volume. The surface offset is only 25 mm.
    count=16;verts=[]
    for dz in [0,.026]:
        for r,z in [(r0,z0),(r1,z1)]:
            for i in range(count+1):verts.append(radial(start+span*i/count,r,z+dz))
    n=count+1;faces=[]
    for i in range(count):faces.extend([(i,i+1,n+i+1,n+i),(2*n+i,3*n+i,3*n+i+1,2*n+i+1),(i,2*n+i,2*n+i+1,i+1),(n+i,n+i+1,3*n+i+1,3*n+i)])
    faces.extend([(0,n,3*n,2*n),(count,2*n+count,3*n+count,n+count)])
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update()
    o=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(o);return ns['finish'](o,name,mat,.006)

# Bespoke mounting and mating interfaces require the accepted dimensions.
# Reinspect canonical intake inventory before choosing supporting parts.
intake=ROOT.parents[1]/'ExternalAssetIntake/Current'
candidates=[]
for package,filename in [('quaternius-modular-scifi-megakit-standard','Modular_SciFi_MegaKit_Standard.zip'),('molten-maps-scifi','Molten Maps SciFi Asset Pack.zip')]:
    archive=intake/package/filename
    with zipfile.ZipFile(archive) as z:
        candidates+= [{'archive':str(archive),'entry':n} for n in z.namelist() if n.lower().endswith('.fbx') and any(k in n.lower() for k in ['cable','pipeholder','generator','rail'])]
(PASS/'support-donor-review.json').write_text(json.dumps(dict(candidates=candidates,decision='Keep accepted imported stairs, rails, generators and pipe holders. The inspected Prop_Cable_3 is a 5.73 m hanging run, unsuitable for the short mating interfaces. Author only dimension-specific fittings, cable routes, fasteners and containment detail; no new stock machinery, stairs or generic rails.'),indent=2))

group('FoundationPolish')
# Deep lower seam + segmented compression shoes keep the existing step outline.
ns['arc']('recessed load seam',2.73,.05,.43,.47,mat='Gasket')
for a in range(0,360,45):
    ns['arc']('lower compression shoe',2.72,.10,.08,.21,a-15,30,'Plate')
    rbox('gusset shoulder',a,2.25,.94,(.79,.17,.33),'Plate',.025)
    rbox('upper deck bearing pad',a,2.3,1.095,(.73,.31,.085),'Steel',.012)
    for shift in [-.18,.18]:
        p=radial(a,2.48,.57);stud((p[0]-math.sin(math.radians(a))*shift,p[1]+math.cos(math.radians(a))*shift,p[2]))
    # Paired service-panel locks repeat the clamp language at human scale.
    for shift in [-.13,.13]:
        p=radial(a,2.755,.34)
        o=ns['box']('paired access hatch latch',(p[0]-math.sin(math.radians(a))*shift,p[1]+math.cos(math.radians(a))*shift,.34),(.045,.024,.12),'Copper',.007,a-90)
    ns['arc']('deck gasket seam',2.60,.018,1.378,1.385,a+18,9,'Gasket')
for a in [0,90,180,270]:
    rbox('pressure manifold junction',a,1.59,1.70,(.33,.65,.32),'Plate',.025)
    ns['arc']('mounting transition ring',1.67,.08,1.83,1.91,a-16,32,'Copper')
# Front maintenance plate sits flush on the existing annular deck.
plaque('base maintenance identification',radial(-45,2.12,1.395),(.72,.18,.012),-45)

group('ChamberPolish')
# Collar, gasket and recessed crown segments add termination inside old height.
for z,r in [(2.52,1.19),(4.85,1.19),(5.30,1.39)]:
    ns['arc']('sealed compression interface',r,.065,z,z+.025,mat='Gasket')
for a in range(0,360,60):
    sloped_sector('crown segmented service cap',.89,5.647,1.22,5.583,a+6,43,'Shell')
    rbox('crown radial lock',a,1.07,5.605,(.38,.14,.08),'Copper',.015)
    stud(radial(a+14,1.20,5.59))
    # Keep vessel windows open. Copper inductor coils and paired narrow arcs
    # communicate a contained mechanism without transparent glow volumes.
    for z in [2.76,3.16,3.56,3.96,4.36,4.72]:
        ns['arc']('window recessed induction turn',1.073,.030,z,z+.075,a-15,26,'Copper')
    for z in [3.05,4.07]:ns['arc']('internal energy junction',1.083,.030,z,z+.032,a-6,12,'Cold')
    for z in [2.62,4.73]:
        rbox('tie rod terminal ferrule',a,1.47,z,(.23,.25,.17),'Copper',.025)
    # Ring tie shoes attach the continuous frame to the existing vessel bands.
    for z in [2.95,4.52]:rbox('pressure belt tie shoe',a,1.35,z,(.28,.19,.18),'Plate',.02)
    rbox('containment plate fastener',a+24,1.205,3.76,(.06,.065,.09),'Steel',.008)
ns['arc']('ceramic terminal compression ferrule',.38,.08,5.91,6.025,mat='Steel')
for a in range(0,360,90):
    ns['arc']('terminal inset energy status',.74,.065,5.84,5.86,a+5,38,'Cold')
    sloped_sector('terminal compression chevron',.585,5.87,.83,5.813,a+51,23,'Copper')
# Containment plaques are seated on an armor segment, not floating decals.
for a in [25,205]:plaque('containment identification',radial(a,1.222,3.24),(.035,.62,.17),a)

group('ConnectorPolish')
# Mating shoes visibly transfer carrier load into base and node plinth.
for x,z,w in [(2.10,.81,.74),(3.12,.79,.63),(4.14,.76,.73)]:
    ns['box']('carrier compression splice',(x,0,z),(.23,w,.13),'Plate',.02)
    for y in [-w*.36,w*.36]:stud((x,y,z+.082))
for x,z in [(1.57,2.13),(2.70,1.70),(3.71,1.18)]:
    for y in [-.21,.21]:
        o=ns['lathe']('paired keyed pipe collar',[(-.028,.103),(.028,.103)],'Copper',32,(x,y,z));o.rotation_euler.y=math.pi/2
ns['box']('base docking transition',(2.03,0,.98),(.38,.86,.17),'Plate',.025)
ns['box']('node docking transition',(3.29,0,.64),(.48,.91,.15),'Plate',.025)
# Protected electrical run beside, below and parallel to the twin fluid lines.
for y in [.34,.39,.44]:
    ns['tube']('fitted feed cable',[(1.67,y,1.78),(2.05,y,1.55),(2.78,y,1.49),(3.16,y,1.04),(3.55,y,1.00)],.019,'Gasket')
for x,z in [(2.48,1.50),(3.44,1.01)]:ns['box']('cable bundle retention',(x,.39,z),(.08,.17,.08),'Steel',.008)

group('NodePolish')
# The accepted donor's tank axis/centre were measured from the actual FBX.
# These covers fit its existing barrel; the donor mesh and placement stay intact.
barrel=(.1395,.10,1.1496)
cover=ns['arc']('generator fitted containment saddle',.478,.024,-.43,.43,35,110,'Plate')
cover.rotation_euler.x=math.pi/2;cover.location=barrel
for y in [-.49,.49]:
    collar=ns['arc']('generator paired compression belt',.485,.028,-.036,.036,10,160,'Copper')
    collar.rotation_euler.x=math.pi/2;collar.location=(barrel[0],barrel[1]+y,barrel[2])
plaque('node top containment marking',(.1395,.10,1.637),(.19,.55,.014),90)
ns['box']('node inbound docking flange',(-1.29,0,1.03),(.085,.80,.67),'Plate',.02)
for y in [-.34,.34]:
    for z in [.80,1.26]:stud((-1.35,y,z),'X')
ns['box']('control face inset',(-1.386,0,.89),(.022,.31,.19),'Gasket',.012)
for y in [-.085,.085]:ns['box']('paired node status lamp',(-1.405,y,.93),(.017,.04,.055),'Cold',.004)
for y in [-.73,.73]:
    ns['box']('generator bed transition rail',(0,y,.55),(1.82,.14,.11),'Plate',.016)
    for x in [-.66,.66]:
        ns['box']('generator isolation pad',(x,y,.63),(.22,.22,.095),'Gasket',.018)
        stud((x,y,.70))
ns['box']('node service hatch',(.48,-1.015,.405),(.68,.055,.34),'Shell',.018)
plaque('node service classification',(.48,-1.048,.425),(.54,.014,.14))
for x in [-.91,.91]:
    for y in [-.85,.85]:stud((x,y,.57))
for y in [.35,.40]:ns['tube']('electrical gland to machine',[(-1.34,y,.99),(-1.02,y,.96),(-.77,y,.66),(-.25,y,.64)],.02,'Gasket')

group('NodeVariant')
# Used on opposite two nodes only. Same design, modest service variation.
ns['box']('auxiliary service pod',(.55,.98,.65),(.62,.26,.30),'Plate',.025)
for x in [.40,.64]:ns['box']('pod paired latch',(x,1.12,.68),(.065,.025,.11),'Copper',.009)
for x in [.36,.43,.50]:ns['tube']('service pod cable',[(x,1.03,.51),(x,.78,.51),(x,.63,.62)],.018,'Gasket')

text=bpy.data.texts.new('POLISH_SCOPE');text.write('GRAVIVORE project-owned overlay meshes. The four accepted primary models and composition are unchanged. Existing donor machinery is referenced by Unity, not embedded here.')
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'EnergyChamber_VisualPolishV1.blend'))
summary=[]
for name in ['FoundationPolish','ChamberPolish','ConnectorPolish','NodePolish','NodeVariant']:
    objs=[o for o in bpy.context.scene.objects if o.type=='MESH' and o.name.startswith(name+' / ')]
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs:o.select_set(True)
    bpy.context.view_layer.objects.active=objs[0];bpy.ops.object.join();o=bpy.context.object;o.name='EC_'+name
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    bm=bmesh.new();bm.from_mesh(o.data);bmesh.ops.triangulate(bm,faces=list(bm.faces));bm.to_mesh(o.data);bm.free()
    bpy.ops.export_scene.fbx(filepath=str(OUT/'Models'/f'EC_{name}.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',bake_anim=False,add_leaf_bones=False,path_mode='STRIP',use_tspace=True)
    summary.append(dict(group=name,triangles=len(o.data.polygons),editableParts=len(objs)))
(PASS/'polish-geometry.json').write_text(json.dumps(summary,indent=2))
print('POLISH_OVERLAYS_AUTHORED',summary)
