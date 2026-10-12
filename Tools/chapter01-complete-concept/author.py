"""Six original installations, their decks and a connected industrial service spine.
Metres throughout; open fighting pockets and actual service endpoints precede detail.
Editable, unconsolidated Blender objects are saved before runtime mesh batching.
"""
import sys, math, json, bpy, bmesh
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
import meshlib as m
from meshlib import box,prism,lathe,cyl,arc,tube,bolts,grate
ROOT=m.ROOT;OUT=m.OUT;SOURCE=m.SOURCE;PASS=m.PASS
for prefix,root,names in [('CS',ROOT/'Assets/_Game/Content/ConnectedConceptSectors',['HeatSteel','PressPaint','FabricationDeck']),('CH',OUT,['ServicePaint','ArmorShell','Scorched'])]:
    for name in names:
        mat=bpy.data.materials.new(prefix+'_'+name);mat.use_nodes=True
        node=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
        tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(root/'Textures'/f'{prefix}_{name}_BaseColor.png'))
        mat.node_tree.links.new(tex.outputs['Color'],node.inputs['Base Color']);m.MATS[name]=mat

def group(kind):m.GROUP=kind
def loft(name,xy,layers,mat='Shell'):
    verts=[]
    for z,w,d in layers:
        c=min(w,d)*.18
        outline=[(-w/2+c,-d/2),(w/2-c,-d/2),(w/2,-d/2+c),(w/2,d/2-c),(w/2-c,d/2),(-w/2+c,d/2),(-w/2,d/2-c),(-w/2,-d/2+c)]
        verts.extend([(x+xy[0],y+xy[1],z) for x,y in outline])
    faces=[tuple(range(7,-1,-1)),tuple(range((len(layers)-1)*8,len(layers)*8))]
    for k in range(len(layers)-1):
        for j in range(8):faces.append((k*8+j,k*8+(j+1)%8,(k+1)*8+(j+1)%8,(k+1)*8+j))
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update()
    obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj);return m.finish(obj,name,mat,.04)
def axle(name,p,r,length,axis='X',mat='Steel',sides=32):
    obj=cyl(name,p,r,length,mat,sides)
    if axis=='X':obj.rotation_euler.y=math.pi/2
    if axis=='Y':obj.rotation_euler.x=math.pi/2
    return obj
def segment(name,a,b,width,depth,mat='Steel'):
    from mathutils import Vector
    av,bv=Vector(a),Vector(b);obj=box(name,(av+bv)/2,(width,depth,(bv-av).length),mat,.035)
    obj.rotation_euler=(bv-av).to_track_quat('Z','Y').to_euler();return obj
def cabinet(x,y,h=3.6,mat='ServicePaint',energy='Cold',width=1.8):
    loft('sealed diagnostic equipment',(x,y),[(.06,width+ .3,1.5),(.32,width+ .3,1.55),(.6,width,1.25),(h-.3,width,1.25),(h,width-.35,1.15)],mat)
    box('recessed service cassette',(x,y-.65,h*.57),(width*.75,.13,h*.57),'Graphite',.025)
    for z in [h*.37,h*.58]:
        box('instrument display backing',(x,y-.74,z),(width*.55,.09,.3),'Steel',.018)
        box('instrument readable display',(x,y-.80,z),(width*.42,.035,.15),energy,.009)
    for xx in [-1,1]:
        box('painted cassette rail',(x+xx*width*.36,y-.78,h*.57),(.1,.11,h*.6),mat,.012)
    for i in range(4):box('air rejection louvre',(x,y-.72,h*.82+i*.07),(width*.54,.1,.035),'Steel',.009)
    tube('floor service supply',[(x,y,-.5),(x,y+.9,-.5),(x,y+.9,1.8)],.10)
def trench(x,y,w,d,mat='Deck'):
    box('recessed utility bed',(x,y,-.73),(w,d,.16),'Graphite',.015)
    tube('protected primary utility',[(x,y-d/2,-.48),(x,y+d/2,-.48)],.13,'Coil')
    grate(x,y,w,d)
def deck(w,d,mat='Deck',style='panel'):
    group('foundation')
    prism('chamfered deep bearing raft',[(-w/2,-d/2+.6),(-w/2+.6,-d/2),(w/2-.6,-d/2),(w/2,-d/2+.6),(w/2,d/2-.6),(w/2-.6,d/2),(-w/2+.6,d/2),(-w/2,d/2-.6)],-.95,-.7,'Graphite',.05)
    group('floor')
    # Long plates are oriented by the process, not a repeated square floor grid.
    for side in [-1,1]:
        prism('process alignment plate',[(side*.03,-d/2+.25),(side*(w/2-.3),-d/2+.25),(side*(w/2-.3),d/2-.3),(side*.03,d/2-.3)],-.28,-.015,mat,.025)
    for x in [-w/2+1,w/2-1]:trench(x,0,.9,d-1)
    for y in [-d/2+1.1,d/2-1.1]:
        box('flush load spreader',(0,y,-.055),(w-2,.22,.10),'Steel',.016)
    for side in [-1,1]:
        for y in [-d/2+2,d/2-2]:
            box('service hatch',(side*(w/2-2.4),y,.01),(1.3,1.7,.05),'Graphite',.025)
            box('flush hatch grip',(side*(w/2-2.4),y-.2,.05),(.35,.12,.04),'Steel',.01)

def repair():
    deck(22,20,'Deck')
    group('service_platform')
    # Walkable annular platform: all top surfaces remain at the collision plane.
    lathe('central maintenance plinth',(0,0,0),[(-.3,3.8),(-.08,3.8),(-.025,3.55)],'Graphite',96)
    for i in range(16):
        a=i*22.5
        arc('replaceable circular work plate',(0,0,0),3.5,.75,-.055,-.005,a,20,'ArmorShell')
        if i%2==0:arc('service guidance inset',(0,0,0),3.18,.035,.002,.024,a+2,13,'Cold')
    for x in [-2.2,2.2]:box('floor diagnostic conduit',(x,2.9,-.025),(.13,6.3,.045),'Steel',.009)
    group('maintenance_bays')
    for s in [-1,1]:
        for y,h in [(-5.6,1.4),(-1,3.2),(4.8,4.7)]:
            loft('service bay load bearing shell',(s*7.5,y),[(.1,3.5,3.7),(.6,3.8,4),(1.0,3.1,3.6),(h,2.85,3.2)],'ServicePaint')
            box('bay shell upper cap',(s*7.5,y,h+.05),(2.95,3.25,.16),'Steel',.04)
            box('service removable body panel',(s*7.5,y-1.83,h*.54),(2.35,.1,h*.53),'ArmorShell',.025)
            for xx in [-.8,.8]:box('diagnostic stripe',(s*7.5+xx,y-1.9,h*.54),(.08,.045,max(.35,h*.4)),'Cold',.009)
        cabinet(s*4.8,6.0,4.4,'ServicePaint','Cold',2.0)
        group('repair_mechanisms')
        lathe('repair manipulator anchored pivot',(s*4.75,1.0,0),[(.05,.75),(.25,.85),(.4,.65),(1.55,.54),(1.8,.67)],'Graphite',48)
        axle('repair arm shoulder',(s*4.75,1,1.9),.39,.72,'Y')
        segment('repair load arm',(s*4.7,1,1.9),(s*3.55,.8,3.15),.42,.55,'ServicePaint')
        axle('repair elbow',(s*3.55,.8,3.15),.28,.60,'Y')
        segment('diagnostic articulated forearm',(s*3.55,.8,3.15),(s*2.9,1.0,2.3),.25,.32,'Steel')
        box('diagnostic tool head',(s*2.9,1,2.3),(.50,.45,.24),'Graphite',.06)
        box('diagnostic sensor window',(s*2.9,.75,2.28),(.30,.035,.10),'Cold',.008)
        tube('repair hydraulic service',[(s*4.75,1,.4),(s*4.75,1,1.9),(s*3.55,.95,3.15),(s*2.9,1.12,2.3)],.065,'Coil')
        group('maintenance_bays')
        tube('service coolant connection',[(s*7.0,5,-.5),(s*5,5,-.5),(s*5,6,2.5)],.17)
    group('exit_frame')
    for s in [-1,1]:
        loft('protected exit diagnostic jamb',(s*3.5,8),[(.0,1.25,1.4),(1,1.1,1.2),(3.6,.8,1.0),(4.35,1.2,1.4)],'Graphite')
        box('exit cyan service lamp',(s*3.5,7.26,2.1),(.15,.08,2.2),'Cold',.015)
    segment('rear exit structural header',(-3.5,8.6,4.2),(3.5,8.6,4.2),.32,.42,'ServicePaint')
    group('floor');box('exit flexible technical threshold',(0,8.5,-.025),(5.2,1.1,.045),'ArmorShell',.025)

def relay():
    deck(24,18,'Deck')
    group('deployment_dock')
    for s in [-1,1]:
        loft('light deployment bay structural jamb',(s*4.8,5.9),[(.02,1.6,2.7),(.35,1.7,2.8),(1.0,.9,2.2),(4.15,.8,2.0)],'ServicePaint')
        box('deployment rack upright',(s*4.8,6.3,4.2),(.95,2.8,.22),'Steel',.025)
        cabinet(s*7.7,3.9,3.6,'ServicePaint','Hot')
        box('dock release bumper',(s*3.7,3.7,.46),(.6,1.8,.9),'Graphite',.04)
    box('open deployment rear frame',(0,7.3,3.8),(10.7,.6,.95),'Graphite',.07)
    box('dock roof equipment tray',(0,6.9,4.4),(10.4,1.7,.23),'ServicePaint',.03)
    for x in [-3.6,-1.8,0,1.8,3.6]:
        box('deployable electronics cartridge',(x,7.1,4.75),(1.2,.95,.48),'ArmorShell',.05)
        box('rack identification aperture',(x,6.59,4.73),(.8,.04,.08),'Hot',.008)
    for x in [-2.1,2.1]:
        box('deployment charging rail',(x,5.4,.12),(.15,3.8,.26),'Steel',.02)
        for y in [4.1,5.2,6.3]:box('charging shoe',(x,y,.31),(.33,.45,.22),'Graphite',.035)
    group('assembly_line')
    for y in [-5,-1,3]:
        loft('light assembly pedestal',(-7.0,y),[(.05,2.9,2.0),(.4,2.7,1.9),(1.15,2.35,1.65)],'Graphite')
        box('fixture interchangeable plate',(-7,y,1.22),(2.7,1.8,.18),'Steel',.035)
        for x in [-7.6,-6.4]:box('assembly locating clamp',(x,y,1.45),(.22,1.1,.38),'ServicePaint',.025)
        for sy in [-.55,.55]:box('small assembled shell',(-7,y+sy,1.54),(1.35,.45,.42),'ArmorShell',.055)
    tube('deployment electronics line',[(-7,-5,-.5),(-7,6,-.5),(-4.8,6,-.5)],.12,'Coil')
    group('communications')
    loft('relay mast service base',(8.5,-.5),[(.05,2.5,2.2),(.45,2.6,2.3),(1.2,1.3,1.1),(4.6,.62,.65)],'Graphite')
    for z in [2.2,3.4,4.6]:
        box('horizontal relay panel',(8.5,-.5,z),(2.9,.22,.65),'ArmorShell',.04)
        for x in [7.6,8.5,9.4]:box('contained antenna array',(x,-.64,z),(.32,.06,.34),'Cold',.014)
    tube('comms trunk line',[(8.5,-.5,4.6),(8.5,1,4.6),(8.5,1,-.5),(7.7,4,-.5)],.09)
    group('floor')
    for x in [-2.25,2.25]:box('deployment lane flush inlay',(x,.5,.008),(.15,10,.035),'Steel',.009)
    for i in range(8):box('deployment mouth caution',(-2.4+i*.68,3.7,.015),(.35,.25,.035),'Warning',.01,yaw=25)

def shield():
    deck(20,20,'FabricationDeck')
    group('armor_storage')
    for y in [-5.4,-1.6,2.2,6.4]:
        loft('heavy armor sorting cell',(-7.3,y),[(.02,3.3,3.3),(.5,3.4,3.5),(1.1,2.7,2.8),(3.65,2.7,2.8),(4.1,3.0,3.0)],'Graphite')
        for i in range(3):
            p=loft('stored layered shield blank',(-7.3,y-.8+i*.63),[(.8,2.6,.18),(3.1,2.3,.18),(3.5,1.7,.18)],'ArmorShell')
            box('shield face reinforcing rib',(-7.3,y-.96+i*.63,2.0),(.18,.08,1.8),'Steel',.02)
        box('storage cell protected corner',(-8.75,y,2.1),(.33,2.7,3.8),'Shell',.04)
    group('armor_transfer')
    # Broad processing mouth and suspended carriage remain behind the fight lane.
    for x in [-4.25,4.25]:
        loft('armored treatment enclosure side',(x,6.9),[(.05,2.1,4.6),(.45,2.4,4.8),(1.3,1.9,4.2),(4.6,1.85,4.2),(5.15,2.0,4.5)],'ArmorShell')
        box('armored door hardened guide',(x,4.7,2.7),(.55,.17,3.4),'Graphite',.025)
        box('sorting status slit',(x,4.58,2.9),(.12,.05,2.0),'Cold',.01)
    box('armor transfer crane bridge',(0,7.0,5.0),(10.8,1.7,.65),'Graphite',.06)
    for y in [6.4,7.6]:box('carriage rolling beam flange',(0,y,5.4),(10.6,.23,.17),'Steel',.02)
    box('powered sorting trolley',(-.8,7,5.45),(2.2,1.6,.8),'PressPaint',.07)
    tube('sorting hoist chain',[(-.8,7,5.25),(-.8,7,2.4)],.08)
    box('shield lifting clamp',(-.8,7,2.35),(1.45,.4,.32),'Steel',.045)
    loft('armor transfer cradle',(0,6.8),[(.1,5.1,2.7),(.5,5.4,3.0),(.8,4.6,2.5)],'Graphite')
    loft('processed protective hull',(0,6.8),[(.85,3.7,1.7),(2.5,3.1,1.6),(3.2,2.3,1.3)],'ArmorShell')
    for y in [3.8,5.2,6.6,8.0]:
        box('armor cell transverse load rail',(0,y,.03),(5.7,.18,.09),'Steel',.015)
    group('sort_controls');cabinet(7.1,-3.3,3.5,'ArmorShell','Hot');cabinet(7.2,9.0,3.8,'ArmorShell','Cold')
    for x,y in [(7.3,-5.7),(-4,-6)]:
        loft('sorted scrap armored bin',(x,y),[(.1,2.5,2.6),(.5,2.7,2.8),(1.5,2.8,2.7)],'Graphite')
        for i in range(3):box('rejected bent shield plate',(x-.2+i*.23,y,1.65+i*.1),(2.0,1.4,.15),'Scorched',.035,yaw=-18+i*17)

def wreck(x,y,yaw=0,damage=False):
    start=len(m.PARTS)
    loft('transport bearing chassis',(x,y),[(.18,2.9,5.6),(.6,3.2,5.7),(.85,3.0,5.3)],'Graphite')
    # The ruptured shell has a genuinely open missing side and exposed interior.
    for s in [-1,1]:
        if not(damage and s==1):
            prism('enclosed cargo hull cheek',[(x+s*1.0,y-2.5),(x+s*1.55,y-2.1),(x+s*1.55,y+2.1),(x+s*1.0,y+2.5)],.9,2.65,'Scorched',.055)
        for sy in [-1.7,0,1.7]:
            axle('transport wheel housing',(x+s*1.4,y+sy,.62),.51,.4,'X','Graphite',40)
            axle('wheel exposed hub',(x+s*1.66,y+sy,.62),.23,.08,'X')
    loft('damaged cargo cockpit',(x,y-2.1),[(.8,2.9,1.4),(1.45,2.8,1.4),(2.15,2.15,1.25)],'ArmorShell')
    box('blackened transport canopy',(x,y-2.75,1.65),(1.65,.08,.56),'Graphite',.055)
    if not damage:loft('closed cargo lid',(x,y+.25),[(2.55,2.9,3.7),(2.85,2.6,3.4)],'Scorched')
    else:
        for yy in [-.8,.4,1.6]:
            segment('broken exposed shell cross rib',(x-1.1,y+yy,2.6),(x+.3,y+yy,2.35),.15,.15)
        tube('severed haul power cable',[(x+.9,y-1,.9),(x+1.4,y-1,1.2),(x+1.6,y-.3,.55)],.11,'Coil')
        for i in range(3):box('displaced hull plate',(x+1.4+i*.25,y+.7+i*.35,1.1+i*.1),(1.1,1.3,.18),'Scorched',.025,yaw=24+i*15)
    if yaw:
        from mathutils import Matrix,Vector
        matrix=Matrix.Translation(Vector((x,y,0)))@Matrix.Rotation(math.radians(yaw),4,'Z')@Matrix.Translation(Vector((-x,-y,0)))
        for obj in m.PARTS[start:]:obj.matrix_world=matrix@obj.matrix_world
def hauler():
    deck(20,20,'FabricationDeck')
    group('transport_wrecks');wreck(-6.9,3.8,-9,True);wreck(1.0,7.1,80,True)
    group('loading_frame')
    for y in [-4.6,6.8]:
        loft('loading crane splayed outrigger',(7.4,y),[(.05,3.6,2.8),(.5,3.8,3.0),(1.2,1.5,1.5),(5.1,1.25,1.3),(5.6,1.8,1.6)],'Graphite')
        box('crane longitudinal runner',(7.4,y,5.7),(2.3,2.0,.35),'Steel',.03)
    box('travel crane trussed beam',(7.4,1.1,5.7),(1.5,14,.72),'Scorched',.06)
    for y in [-3,-.5,2,4.5]:
        segment('crane diagonal web',(6.65,y,5.3),(6.65,y+2,6.05),.16,.2)
    box('hauler crane carriage',(7.4,3.3,6.3),(2.6,2.5,.75),'PressPaint',.06)
    box('loading outreach beam',(4.4,3.3,6.5),(6.5,.6,.48),'Steel',.03)
    for x in [2.1,3.5]:tube('cargo suspended lifting tether',[(x,3.3,6.4),(x,3.3,3.5)],.055)
    box('cargo spreader bar',(2.8,3.3,3.45),(2.8,.8,.32),'Scorched',.055)
    group('loading_services');cabinet(7.5,-1.2,3.0,'PressPaint','Hot')
    for y in [-5,2]:
        box('loading dock rail',(5.9,y,.035),(.22,5,.12),'Steel',.018)
        box('deformed dock bumper',(6.4,y, .7),(.5,1.1,1.35),'Graphite',.04)
    group('broken_platform')
    for i in range(4):
        prism('fractured peripheral load plate',[(-8.8+i*.6,-8),(-7.8+i*.65,-7.8),(-8+i*.6,-6.8),(-9+i*.65,-6.3)],.02,.14,'Scorched',.025)
    for x,y,yaw in [(-6,-4,-24),(-7,-5.2,13),(6,-7,28)]:
        box('fallen transport armor',(x,y,.21),(1.5,2.1,.36),'Scorched',.065,yaw)
        tube('ruptured logistics conduit',[(x-.6,y,.5),(x-.6,y+1,.5),(x+.4,y+1,.12)],.1)

def magnetar():
    deck(50,20,'FabricationDeck')
    group('magnetic_installation')
    x,y=-5,5.9
    lathe('magnetic induction bearing foundation',(x,y,0),[(.0,4.2),(.35,4.4),(.75,3.8),(1.05,3.7)],'Graphite',80)
    for i in range(8):
        a=i*45;rad=math.radians(a)
        arc('laminated induction coil segment',(x,y,0),3.2,.8,1.1,2.15,a+3,39,'Coil')
        arc('magnetic return pole shoe',(x,y,0),3.55,.28,2.05,2.45,a+3,39,'Steel')
        xx,yy=x+3.9*math.cos(rad),y+3.9*math.sin(rad)
        loft('induction radial retaining buttress',(xx,yy),[(.25,1.0,1.4),(1.3,.9,1.2),(2.6,.7,.8)],'PressPaint')
    # An exposed pole gap, not another set of capacitor pressure vessels.
    lathe('central magnetic pole',(x,y,0),[(1.0,1.8),(1.25,2.0),(2.7,1.8),(3,2.0),(4.3,1.35),(5.0,1.45)],'Graphite',64)
    for z in [1.5,2.05,2.6,3.3,3.85]:arc('contained induction winding',(x,y,0),1.85,.14,z,z+.18,0,360,'Hot')
    for s in [-1,1]:
        segment('magnetic arch lower leg',(x+s*3.25,y,2.4),(x+s*3.1,y,5.4),.72,1.15,'Graphite')
        segment('arched flux return path',(x+s*3.1,y,5.4),(x+s*1.75,y,7.1),.82,1.2,'ArmorShell')
        for z in [3,3.55,4.1,4.65]:box('laminated magnetic armature',(x+s*3.25,y,z),(.93,1.4,.23),'Steel',.025)
    box('magnetic return arch cap',(x,y,7.0),(4.0,1.35,.8),'ArmorShell',.08)
    box('elite apparatus core aperture',(x,y-.73,6.9),(1.4,.08,.3),'Hot',.02)
    for xx in [-14,8]:
        group('power_conversion');cabinet(xx,6.5,4.8,'PressPaint','Hot',2.6)
        tube('induction high current circuit',[(xx,6,1.1),(xx,4.5,1.1),(x,4.5,1.1),(x,y,1.1)],.23,'Coil')
    group('induction_architecture')
    for xx in [-22,22]:
        for yy in [-6,0,6]:
            loft('magnetic complex structural shell',(xx,yy),[(.0,3.5,4.6),(.6,3.7,4.8),(1.1,2.3,3.2),(5.5,2.0,2.9)],'Graphite')
    for xx in [12,19]:
        box('elite deployment service ledge',(xx,7.0,.65),(5.0,3.7,1.3),'Graphite',.05)
        box('elite deployment reinforced cell',(xx,8.0,2.0),(4.6,1.5,2.7),'ArmorShell',.07)
        box('elite cell warning aperture',(xx,7.2,2.2),(2.8,.06,.26),'Hot',.013)
    group('floor')
    # Broad radial sectors unify the reactor with its open elite combat field.
    for a in [200,240,280,320]:
        r=math.radians(a)
        segment('flush magnetic alignment inlay',(x+4.5*math.cos(r),y+4.5*math.sin(r),.01),(x+8*math.cos(r),y+8*math.sin(r),.01),.07,.07,'Steel')
    for yy in [-5,0,5]:box('elite deck lateral load rail',(9,yy,.008),(18,.18,.035),'Steel',.014)

def custodian():
    deck(50,26,'Deck')
    group('containment_core')
    x,y=0,8.6
    lathe('octagonal core containment plinth',(x,y,0),[(.05,4.6),(.5,4.7),(1.0,4.2),(1.6,3.9),(1.85,3.5)],'Graphite',16)
    lathe('central reactor working chamber',(x,y,0),[(1.7,2.05),(2.0,2.25),(2.3,2.2),(6.9,2.2),(7.3,2.5),(7.7,2.4),(8.6,1.6)],'Graphite',64)
    for i in range(6):
        a=math.radians(i*60)
        xx,yy=x+2.23*math.cos(a),y+2.23*math.sin(a)
        tube('contained reactor luminous channel',[(xx,yy,2.5),(xx,yy,6.8)],.15,'Hot')
        # Deep shielding louvres expose strips of core while giving structural depth.
        for z in [3,4,5,6]:arc('sacrificial containment baffle',(x,y,0),2.7,.55,z,z+.24,i*60+6,48,'ArmorShell')
        bx,by=x+3.75*math.cos(a),y+3.75*math.sin(a)
        loft('reactor radial monolithic buttress',(bx,by),[(.1,1.7,2.2),(.7,1.8,2.4),(1.6,1.4,1.8),(5.8,1.1,1.4),(7.5,1.4,1.8)],'Graphite')
        segment('reactor crown restraint',(bx,by,7.3),(x+2.2*math.cos(a),y+2.2*math.sin(a),8.3),.35,.55,'Steel')
    arc('crown maintenance rim',(0,y,0),3.9,.5,7.3,7.7,0,360,'Steel')
    lathe('governing reactor crown',(0,y,0),[(8.25,2.3),(8.5,2.5),(8.9,2.1),(9.65,1.4)],'ArmorShell',32)
    cyl('restrained central output beam',(0,y,6.1),.44,6.1,'Hot',40)
    group('containment_architecture')
    # Stepped monoliths frame the boss and never roof its telegraph area.
    for s in [-1,1]:
        for yy,h in [(-8,2.2),(-1.5,4.4),(5,6.4),(10.0,7.7)]:
            xx=s*(14 if yy==5 else 11 if yy<7 else 13)
            loft('containment monolithic wall pier',(xx,yy),[(.1,3.8,3.0),(.7,4.0,3.3),(1.7,2.6,2.6),(h,2.4,2.3)],'Graphite')
            box('containment shielding face',(xx,yy-1.28,h*.57),(1.75,.15,h*.65),'ArmorShell',.05)
            box('containment energy recess',(xx,yy-1.39,h*.57),(.22,.08,h*.46),'Hot',.012)
            tube('core service conduit',[(xx,yy,-.5),(xx,yy+2,-.5),(xx,yy+2,h*.6)],.17)
        group('core_service')
        cabinet(s*7.7,7.4,4.4,'ArmorShell','Cold',2.0)
        for yy in [-4,2,8]:
            box('outer containment service deck',(s*19,yy,-.15),(8.7,4.7,.3),'Graphite',.045)
            cabinet(s*22,yy+1.5,3.6,'PressPaint','Hot',2.5)
        group('containment_architecture')
    for x in [-8,8]:
        segment('reactor rear service gantry',(x,11.5,5.7),(0,11.5,5.7),.34,.52,'Steel')
    group('floor')
    for i in range(12):
        a=i*30
        arc('boss containment segmented battle apron',(0,0,0),8.6,1.8,-.02,.008,a+1,28,'ArmorShell')
        arc('telegraph pocket boundary',(0,0,0),6.5,.045,.01,.032,a+3,20,'Steel')
    for s in [-1,1]:
        trench(s*9.0,0,.9,13)
        for yy in [-5,0,5]:box('containment service crossing',(s*9,yy,-.03),(1.6,1.0,.06),'Deck',.025)

def network():
    group('spine_floor')
    # Exact gaps between sector plates; original energy/fabrication gallery stays.
    for lo,hi in [(-20,-19),(-1,6),(14,28),(36,44),(64,65)]:
        mid=(lo+hi)/2;d=hi-lo
        box('continuous service bridge bearing',(0,mid,-.5),(8,d,.9),'Graphite',.035)
        for x in [-1.1,1.1]:box('north bound replaceable access slab',(x,mid,-.08),(2.1,d,.16),'Deck',.02)
        for x in [-2.85,2.85]:
            trench(x,mid,1.25,max(.7,d-.2))
            tube('continuous side distribution',[(x,lo,-.45),(x,hi,-.45)],.13,'Coil')
        for yy in [lo+.35,hi-.35]:box('bridge expansion threshold',(0,yy,.009),(7.8,.14,.035),'Steel',.01)
    # The second crossing is a load transfer route with longitudinal rails.
    for s in [-1,1]:
        box('armor logistics transfer foundation',(s*3.1,32,-.4),(6.1,8,.8),'Graphite',.03)
        for yy in [29.2,34.8]:
            box('cargo transfer guide',(s*3.1,yy,.03),(6.1,.23,.08),'Steel',.012)
        box('cargo transfer access plate',(s*3.1,32,-.07),(6.1,4.8,.14),'FabricationDeck',.02)
    group('spine_services')
    for yy in [-.2,20,39.5,65]:
        for s in [-1,1]:
            h=2.5 if yy!=65 else 4.0
            loft('service spine distribution pier',(s*3.35,yy),[(.02,.65,1.1),(.35,.75,1.2),(h-.2,.55,.75),(h,.8,.95)],'Graphite')
            box('route technical guidance',(s*3.35,yy-.51,h*.6),(.13,.04,.6),'Cold',.01)
            tube('bridge riser supply',[(s*3.35,yy,-.4),(s*3.35,yy,h-.2)],.07)
    for yy in [23,26]:
        box('bridge side thermal service case',(-3.3,yy,1.0),(.7,1.65,2.0),'Scorched',.04)
    # Architectural gate cheeks align with unchanged authoritative gate proxies.
    group('gate_architecture')
    for yy,height in [(44,5.0),(64,6.5)]:
        for s in [-1,1]:
            loft('progression pressure-lock gate cheek',(s*5.5,yy),[(.05,3,2.6),(.65,3.2,2.8),(1.6,2.2,2.0),(height,2.0,1.7)],'Graphite')
            box('pressure-lock armored face',(s*5.5,yy-1.0,height*.55),(1.4,.19,height*.65),'ArmorShell',.035)
            box('gate lock technical indicator',(s*5.5,yy-1.12,height*.55),(.16,.05,height*.45),'Hot',.01)
        segment('rear pressure-lock frame',(-5.5,yy+1,height),(5.5,yy+1,height),.30,.52,'Graphite')

specs=[('RepairHub',repair),('RelayYard',relay),('ShieldDump',shield),('HaulerGraveyard',hauler),('MagnetarComplex',magnetar),('CustodianCore',custodian),('ChapterServiceNetwork',network)]
requested=set(sys.argv[sys.argv.index('--only')+1].split(',')) if '--only' in sys.argv else set()
if requested:specs=[s for s in specs if s[0] in requested]
chunks=[]
for name,build in specs:
    print('AUTHORING_INSTALLATION',name,flush=True)
    start=len(m.PARTS);build();parts=m.PARTS[start:]
    for obj in parts:obj['installation']=name
    chunks.append((name,parts))
# Editable file keeps individual forms, process-specific names and local metres.
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/('Chapter01Facilities.blend' if not requested else 'Revised_'+ '_'.join(sorted(requested))+'.blend')))
stats=[]
for name,parts in chunks:
    groups={}
    for obj in parts:
        points=[obj.matrix_world@v.co for v in obj.data.vertices]
        mx=(min(v.x for v in points)+max(v.x for v in points))/2
        my=(min(v.y for v in points)+max(v.y for v in points))/2
        groups.setdefault((obj.name.split(' / ')[0],math.floor(mx/8),math.floor(my/4)),[]).append(obj)
    merged=[]
    for (kind,x,y),items in groups.items():
        bpy.ops.object.select_all(action='DESELECT')
        for obj in items:obj.hide_set(False);obj.select_set(True)
        bpy.context.view_layer.objects.active=items[0];bpy.ops.object.join();obj=bpy.context.object
        obj.name=f'CH_{name}_{kind}_{x}_{y}';bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
        bm=bmesh.new();bm.from_mesh(obj.data);bmesh.ops.triangulate(bm,faces=list(bm.faces));bm.to_mesh(obj.data);bm.free();merged.append(obj)
    bpy.ops.object.select_all(action='DESELECT')
    for obj in merged:obj.select_set(True)
    bpy.ops.export_scene.fbx(filepath=str(OUT/'Models'/f'{name}.fbx'),use_selection=True,object_types={'MESH'},apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',bake_anim=False,add_leaf_bones=False,path_mode='STRIP',mesh_smooth_type='FACE',use_tspace=True)
    stats.append(dict(model=name,renderers=len(merged),triangles=sum(len(o.data.polygons) for o in merged)))
    for obj in merged:obj.hide_set(True)
prior=json.loads((PASS/'verification/geometry.json').read_text(encoding='utf-8')) if requested and (PASS/'verification/geometry.json').exists() else []
stats=[s for s in prior if s['model'] not in requested]+stats
(PASS/'verification/geometry.json').write_text(json.dumps(stats,indent=2),encoding='utf-8')
print('CHAPTER_FACILITIES_EXPORTED',json.dumps(stats))
