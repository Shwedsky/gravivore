"""Heavy cutting/press line and its electrical-to-fabrication transfer gallery.
Hero geometry is original. Shared CF PBR vocabulary is retained, not its shapes.
"""
import sys, math, json, bpy, bmesh
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
import meshlib as m
from meshlib import box,prism,lathe,cyl,tube,bolts,grate
ROOT=m.ROOT;OUT=m.OUT;PASS=m.PASS;SOURCE=m.SOURCE
for name in ['HeatSteel','PressPaint','FabricationDeck']:
    mat=bpy.data.materials.new('CS_'+name);mat.use_nodes=True
    node=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(OUT/'Textures'/f'CS_{name}_BaseColor.png'));mat.node_tree.links.new(tex.outputs['Color'],node.inputs['Base Color']);m.MATS[name]=mat
def group(name):m.GROUP=name
def loft(name,xy,layers,mat='PressPaint'):
    verts=[]
    for z,w,d in layers:
        cham=min(w,d)*.17
        outline=[(-w/2+cham,-d/2),(w/2-cham,-d/2),(w/2,-d/2+cham),(w/2,d/2-cham),(w/2-cham,d/2),(-w/2+cham,d/2),(-w/2,d/2-cham),(-w/2,-d/2+cham)]
        verts.extend([(x+xy[0],y+xy[1],z) for x,y in outline])
    faces=[tuple(range(7,-1,-1)),tuple(range((len(layers)-1)*8,len(layers)*8))]
    for k in range(len(layers)-1):
        for j in range(8):faces.append((k*8+j,k*8+(j+1)%8,(k+1)*8+(j+1)%8,(k+1)*8+j))
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update();obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj);return m.finish(obj,name,mat,.035)
def axle(name,pos,r,length,axis='X',mat='Steel'):
    obj=cyl(name,pos,r,length,mat,40);obj.rotation_euler.y=math.pi/2 if axis=='X' else 0;obj.rotation_euler.x=math.pi/2 if axis=='Y' else 0;return obj
def export(name,parts):
    groups={}
    for obj in parts:
        points=[obj.matrix_world@v.co for v in obj.data.vertices];mid=(min(v.y for v in points)+max(v.y for v in points))/2
        groups.setdefault((obj.name.split(' / ')[0],math.floor(mid/4)),[]).append(obj)
    merged=[]
    for (kind,band),items in groups.items():
        bpy.ops.object.select_all(action='DESELECT')
        for obj in items:obj.hide_set(False);obj.select_set(True)
        bpy.context.view_layer.objects.active=items[0];bpy.ops.object.join();obj=bpy.context.object;obj.name=f'CS_{kind}_{band}';bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
        bm=bmesh.new();bm.from_mesh(obj.data);bmesh.ops.triangulate(bm,faces=list(bm.faces));bm.to_mesh(obj.data);bm.free();merged.append(obj)
    bpy.ops.object.select_all(action='DESELECT')
    for obj in merged:obj.select_set(True)
    bpy.ops.export_scene.fbx(filepath=str(OUT/'Models'/f'{name}.fbx'),use_selection=True,object_types={'MESH'},apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',bake_anim=False,add_leaf_bones=False,path_mode='STRIP',mesh_smooth_type='FACE',use_tspace=True)
    result=dict(model=name,renderers=len(merged),triangles=sum(len(o.data.polygons) for o in merged))
    for obj in merged:obj.hide_set(True)
    return result

group('fabrication_floor')
prism('load bearing fabrication foundation',[(-10,-10),(10,-10),(10,10),(-10,10)],-1.2,-.85,'Graphite',.12)
for side in [-1,1]:
    outline=[(side*.08,-8.8),(side*3.0,-8.8),(side*3.0,-6),(side*3.1,-2),(side*3.1,3.9),(side*.08,3.9)]
    if side<0:outline.reverse()
    prism('long feed alignment load plate',outline,-.3,0,'FabricationDeck',.04)
    box('flush feed rail',(side*2.55,-2.25,.018),(.17,13.1,.045),'Steel',.012)
    for y in [-7.3,-3.7,.2,3.4]:
        box('load cassette transverse joint',(side*1.5,y,.006),(2.75,.045,.014),'Graphite',.003)
        for x in [.35,2.8]:box('cassette captive service latch',(side*x,y,.016),(.19,.33,.03),'Steel',.012)
    box('chip extraction trough',(side*3.75,-2.4,-.76),(1.3,12.5,.18),'Graphite',.025)
    for y in [-7,-3,1,3]:
        tube('waste evacuation side manifold',[(side*3.75,y,-.6),(side*3.75,y+1,-.6),(side*4.35,y+1,-.5)],.14,'HeatSteel')
    grate(side*3.75,-2.4,1.25,12.4)
    box('operator inspection deck',(side*5.35,-2.2,-.18),(1.8,13.2,.36),'FabricationDeck',.06)
    for y in [-6.5,-2,2]:
        box('removable waste chamber bridge',(side*3.75,y,-.06),(1.4,1.1,.12),'FabricationDeck',.03)
        for i in range(5):box('pinch warning over chip pit',(side*(3.21+i*.22),y-.46,.025),(.13,.14,.025),'Warning',.004)
prism('west transfer entry load apron',[(-10,-2.7),(-5,-2.7),(-3.3,-1.8),(-3.3,1.8),(-5,2.7),(-10,2.7)],-.28,0,'FabricationDeck',.04)
box('rear press reinforced raft',(0,6.7,-.23),(19.6,6.6,.46),'Graphite',.11)
for x in [-7.6,-3.2,3.2,7.6]:box('rear maintenance deck plate',(x,8.9,.04),(3.1,1.2,.08),'FabricationDeck',.035)

group('press_frame')
# Four cast pillars support a broad bridge. Their flared feet and stepped
# shoulders carry the crosshead; no stacked vessel vocabulary is reused.
for x in [-4.05,4.05]:
    for y in [4.9,8.5]:
        loft('cast portal press upright',(x,y),[(.1,1.85,1.7),(.5,1.75,1.65),(1.1,1.32,1.25),(4.9,1.20,1.08),(5.7,1.65,1.4),(6.25,1.9,1.55)])
        box('exposed pillar guide face',(x,y-.63,3.35),(.55,.09,3.9),'Steel',.025)
        for z in [1.5,4.7]:
            box('guide mounting collar',(x,y-.67,z),(.85,.14,.24),'Graphite',.035)
            for sx in [-.3,.3]:axle('pillar collar bolt',(x+sx,y-.78,z),.07,.08,'Y')
    box('deep structural longitudinal web',(x,6.7,5.95),(1.7,5.5,.85),'Graphite',.10)
    box('upper structural flange',(x,6.7,6.4),(1.85,5.7,.22),'Steel',.035)
loft('forged bridge crosshead',(0,6.6),[(5.45,10.1,2.65),(5.85,10.2,2.8),(6.25,9.6,2.5),(6.65,8.7,2.1)])
box('crosshead front flange',(0,5.16,5.75),(9.8,.23,.3),'Steel',.045)
box('bridge recessed load web',(0,5.35,6.1),(6.5,.12,.38),'Graphite',.035)
for x in [-3,-2,-1,0,1,2,3]:box('bridge service inspection recess',(x,5.26,6.1),(.48,.07,.17),'HeatSteel',.016)
for x in [-2.15,2.15]:
    lathe('hydraulic ram cylinder',(x,6.6,0),[(4.1,.59),(4.22,.70),(4.38,.70),(4.42,.59),(5.9,.59),(6.05,.78),(6.22,.78)],'Graphite')
    cyl('polished hydraulic plunger',(x,6.6,3.6),.28,1.7,'Steel',48)
    bolts(x,6.6,6.24,.63,8)
    tube('pressure inlet',[(x,6.6,5.6),(x,7.4,5.6),(4.4,7.4,5.6),(6.3,7.4,2.7)],.105,'Steel')
loft('cross-slide blade carrier',(0,6.6),[(2.9,5.9,1.1),(3.1,6.1,1.4),(3.65,5.9,1.4),(3.82,5.2,1.05)],'Graphite')
box('blade carrier guide flange',(0,5.88,3.34),(5.9,.17,.46),'Steel',.035)
verts=[(x,y,z) for y in [6.04,6.30] for x,z in [(-2.55,3.12),(2.55,3.12),(2.55,2.03),(-2.55,2.64)]]
faces=[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)]
mesh=bpy.data.meshes.new('sloping shear blade');mesh.from_pydata(verts,[],faces);mesh.update();obj=bpy.data.objects.new('sloping shear blade',mesh);bpy.context.collection.objects.link(obj);m.finish(obj,'sloping replaceable cutting edge','Steel',.035)
tube('contained hot cutting edge',[(-2.5,6.005,2.63),(2.5,6.005,2.025)],.036,'Hot')

group('work_center')
loft('massive die bolster',(0,5.9),[(.12,6.25,3.55),(.45,6.5,3.7),(.87,5.9,3.2),(1.08,5.9,3.2)],'Graphite')
box('replaceable die working plate',(0,5.9,1.12),(5.5,3.0,.16),'HeatSteel',.055)
box('hot workpiece',(0,5.75,1.24),(3.8,2.05,.10),'Steel',.025)
for x in [-2.25,2.25]:
    box('blank hold down clamp',(x,5.8,1.38),(.48,2.25,.34),'PressPaint',.06)
    for y in [5.1,6.5]:cyl('clamp captive fastener',(x,y,1.59),.105,.14,'Steel',8)
for x in [-1.8,-.9,0,.9,1.8]:box('die bed heat rejection slot',(x,4.02,.75),(.44,.12,.26),'Graphite',.025)
for x in [-1.65,-.85,0,.85,1.65]:box('internal heated die throat',(x,4.035,.74),(.19,.045,.12),'Hot',.01)
for i in range(12):box('work cell pinch boundary',(-2.9+i*.52,3.8,.045),(.32,.26,.05),'Warning',.007,yaw=30)
# Lateral stock feed ends at the die, with individual rollers and bearing housings.
for y in [5.0,6.6]:box('feed conveyor side bearer',(5.6,y,1.02),(6.0,.28,.5),'Graphite',.06)
for x in [3.2,3.8,4.4,5,5.6,6.2,6.8,7.4,8]:
    axle('stock feeder roller',(x,5.8,1.29),.14,1.55,'Y')
    for y in [4.98,6.62]:box('roller bearing captive block',(x,y,1.25),(.25,.24,.31),'PressPaint',.035)
for x in [4.0,6.7,8.1]:loft('conveyor anchored leg',(x,5.8),[(.1,.62,1.7),(.9,.4,1.3),(1.05,.65,1.8)],'Graphite')
box('loaded raw blank',(6.2,5.8,1.48),(3.8,1.2,.16),'HeatSteel',.025)
axle('roller drive gearbox',(8.45,5.8,.91),.40,.7,'X','Graphite')
for y in [4.9,6.7]:tube('feeder service return',[(8.6,y,.5),(4,y,.5),(4,7.8,.5)],.11,'Steel')

group('auxiliary_machinery')
for x,y,w,d,h in [(7.9,-.7,2.9,4.0,4.8),(-6.85,-4,2.55,3.2,3.25),(-6.85,3,2.4,2.9,3.7)]:
    loft('production service cabinet',(x,y),[(.08,w+.28,d+.28),(.42,w,d),(h-.35,w,d),(h,w-.35,d-.3)],'PressPaint')
    box('machine inset mechanism face',(x,y-d/2-.035,h*.55),(w*.68,.16,h*.57),'Graphite',.05)
    for i in range(7):box('cooling extraction blade',(x,y-d/2-.13,.8+i*.29),(w*.60,.16,.085),'Steel',.013)
    for sx in [-1,1]:
        tube('hydraulic return manifold',[(x+sx*w*.32,y-d/2-.1,.35),(x+sx*w*.32,y-d/2-.1,h*.8),(x+sx*w*.32,y,h*.8)],.09,'HeatSteel')
    box('limited work chamber warning',(x,y-d/2-.14,h*.75),(w*.39,.04,.14),'Hot',.009)
    box('isolated service control',(x-.3,y-d/2-.15,h*.35),(.34,.06,.18),'Cold',.011)
for x in [-8.5,-7.2,7.1,8.5]:
    box('raised rear service deck',(x,9.1,.64),(1.1,1.55,.22),'FabricationDeck',.055)
    for y in [8.55,9.65]:box('service deck support',(x,y,.27),(.24,.24,.6),'Graphite')
for i in range(5):box('press inspection stair',(-5.55,5.5+i*.4,.06+i*.13),(1.1,.48,.12),'Steel',.025)
for x in [-8.9,8.9]:tube('coolant riser',[(x,-8,-.4),(x,8,-.4),(x,8,2.4),(4.9,8,2.4)],.18,'HeatSteel')
box('rear insulated hydraulic distributor',(0,8.8,2.0),(5.8,.9,.9),'Graphite',.10)
for x in [-2,-1,0,1,2]:axle('pressure accumulator valve',(x,8.3,2.0),.23,.18,'Y','Steel')
cutting=list(m.PARTS)

group('transfer_gallery')
# Different infrastructure at each end, with a clear four-way service junction.
prism('transfer gallery substrate',[(-9,-3.95),(9,-3.95),(9,3.95),(-9,3.95)],-.95,-.7,'Graphite',.11)
for x in [-7.2,-4.8,-2.4,0,2.4,4.8,7.2]:
    box('load spreading transfer slab',(x,0,-.16),(2.33,4.6,.34),'Deck',.045)
    for y in [-1.95,1.95]:box('transfer slab bolted lip',(x,y,.022),(1.85,.075,.035),'Steel',.009)
for y in [-3.1,3.1]:
    box('gallery service trench',(0,y,-.62),(17.8,1.3,.12),'Graphite',.035)
    for offset in [-.36,0,.36]:tube('energy transfer protected conduit',[(-9,y+offset,-.45),(0,y+offset,-.45),(9,y+offset,-.45)],.10,'Ceramic' if y>0 else 'HeatSteel')
    for x in [-7.5,-5,-2.5,0,2.5,5,7.5]:
        grate(x,y,2.34,1.27)
for y in [-3.05,3.05]:box('north south junction crossing',(0,y,-.08),(7.6,1.38,.16),'Deck',.035)
for side in [-1,1]:
    x=side*7.15
    for y in [-3.4,3.4]:
        layers=[(0,1.4,1.45),(.35,1.25,1.35),(1.35,.9,.92),(1.65,1.15,1.15)] if y<0 else [(0,1.4,1.45),(.48,1.25,1.35),(3.9,.9,.92),(4.6,1.25,1.15)]
        loft('low technical threshold' if y<0 else 'transfer rear cast jamb',(x,y),layers,'Shell')
        box('portal recessed service channel',(x,y, .95 if y<0 else 2.5),(.45,.18,.85 if y<0 else 2.7),'Graphite',.025)
        box('portal status identity',(x-.23,y-.49,1.15 if y<0 else 3.45),(.19,.08,.3 if y<0 else .65),'Cold' if side<0 else 'Hot',.011)
        # Split upper shoulders preserve the visible actor lane under the portal.
        if y>0:box('open portal rear upper shoulder',(x,y*.74,4.43),(1.25,1.7,.35),'Steel',.06)
        for z in ([.45,.9,1.4] if y<0 else [.8,2.2,3.6]):box('jamb bolted service closure',(x+.51,y,z),(.06,.42,.15),'Steel',.011)
    for y in [-3.7,3.7]:
        h=1.3 if y<0 else 4.1
        tube('supported sector bus',[(side*9,y,h-.25),(side*7.1,y,h),(side*4.25,y,h)],.18,'Steel')
        for xx in [side*8.1,side*5.6]:
            box('bus ceramic support',(xx,y,h-.14),(.20,.30,.45),'Ceramic',.025)
    for i in range(8):box('technical transfer threshold',(x, -1.75+i*.5,.028),(.42,.23,.05),'Warning',.007)
for x in [-8.4,8.4]:
    box('threshold ground bonded foot',(x,0,-.035),(.37,4.4,.09),'Steel',.02)
box('crossing distribution service plate',(0,0,.025),(2.05,2.05,.05),'Graphite',.04)
for y in [-.75,.75]:box('distribution channel inspection lid',(0,y,.06),(1.5,.16,.04),'Steel',.015)
# The junction has a legible conversion/isolation station inside the normal
# portrait view, with equipment flanking the open north/south spine.
for x,y in [(-3.2,3.0),(3.2,-3.0)]:
    loft('junction isolation and pressure station',(x,y),[(.03,1.85,1.72),(.42,1.75,1.58),(2.75,1.55,1.40),(3.2,1.25,1.18)],'Graphite')
    box('station bolted armor face',(x,y-.73,1.65),(1.32,.14,1.85),'Shell',.055)
    box('station inset service face',(x,y-.82,1.68),(.75,.06,1.25),'Graphite',.027)
    for sx in [-.44,.44]:
        for z in [.84,2.55]:axle('station closure fastener',(x+sx,y-.85,z),.07,.055,'Y')
    for offset in [-.36,0,.36]:
        lathe('ceramic isolator or pressure port',(x+offset,y,0),[(3.2,.16),(3.3,.23),(3.43,.16),(3.55,.23),(3.68,.16)],'Ceramic')
        tube('station visible service termination',[(x+offset,y,3.7),(x+offset,y+.34,3.7),(x+offset,y+.34,-.44)],.072,'Steel')
    box('small station status window',(x,y-.855,1.9),(.48,.025,.16),'Cold' if x<0 else 'Hot',.008)
    for i in range(5):box('station cooling louvre',(x,y-.865,1.03+i*.12),(.52,.06,.05),'Steel',.008)
bridge=m.PARTS[len(cutting):]
# Keep both complete installations independently editable before consolidation.
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'CuttingFloorAndTransfer.blend'))
records=[export('CuttingFloor',cutting),export('IndustrialTransfer',bridge)]
(PASS/'authored.json').write_text(json.dumps(dict(models=records,texelConvention='one repeat per metre; shared CF PBR plus original fabrication maps',externalGeometry='none added',nonuniformFit=False),indent=2),encoding='utf-8')
print('CONNECTED_CONCEPT_MODELS_AUTHORED',records)
