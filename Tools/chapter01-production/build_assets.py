"""Original Chapter 01 production meshes; uses the accepted kit's chamfer/rig authoring methods.
No external geometry. Metres, Z up, -Y facing; exported to Unity Y up.
"""
from pathlib import Path
import json
ROOT=Path(__file__).resolve().parents[2]
helper=(ROOT/'Tools/first-visual-slice/build_assets.py').read_text()
helper=helper.split('reset();legs(6,.91')[0]
helper=helper.replace("ROOT = Path(__file__).resolve().parents[2]", "ROOT = Path(__file__).resolve().parents[2]")
helper=helper.replace("'Assets/_Game/Content/VisualSlice'","'Assets/_Game/Content/Chapter01Production'")
helper=helper.replace("'art/first-visual-slice'","'art/chapter01-production'")
helper=helper.replace("(.34,.065,.034)]","(.34,.065,.034),(.09,.31,.19),(.23,.13,.32),(.17,.32,.42),(.56,.49,.24),(.21,.25,.28),(.09,.12,.14)]")
helper=helper.replace("'rust coating']","'rust coating','shield green','hauler violet','relay blue','aged hazard','brushed graphite','deck inset']")
helper=helper.replace("i=9 if x>=249 else min(8,x//28)","i=min(15,x//16)")
helper=helper.replace("(.985 if idx==9 else (idx*28+14)/N)","((idx*16+8)/N)")
helper=helper.replace("u=.985 if idx==9 else (idx*28+14)/N","u=(idx*16+8)/N")
helper=helper.replace("(.004 if idx==9 else .016 if idx==8 else .022)",".014")
helper=helper.replace("pb.location.y=abs(q)*.027", "pb.location.z=abs(q)*.027")
helper=helper.replace("pb.location.y=-u*.27", "pb.location.z=-u*.27")
helper=helper.replace("elif 'HIP' in pb.name:","elif 'WHEEL' in pb.name:\n                    if state=='Run':pb.rotation_euler.y=u*math.tau\n                elif 'ROTOR' in pb.name:\n                    if state in ('Idle','Run'):pb.rotation_euler.z=u*math.tau\n                elif 'HIP' in pb.name:")
exec(compile(helper, str(__file__), 'exec'))

def export_static(name):
    mesh=skin_mesh(name)[0];active(mesh)
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/(name+'.blend')))
    bpy.ops.export_scene.fbx(filepath=str(OUT/'Models'/(name+'.fbx')),use_selection=True,object_types={'MESH'},
        global_scale=1,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',mesh_smooth_type='FACE',use_tspace=True,
        axis_forward='-Z',axis_up='Y',bake_anim=False,path_mode='STRIP')

def grille(c,width,depth,count=6,bone='BODY'):
    x,y,z=c
    for i in range(count): box('Recessed cooling louvre',(x-width/2+width*(i+.5)/count,y,z),(width/count*.5,depth,.035),8,bone,bevel=.003)

# Arc Drone: airborne cruciform electrical machine with four enclosed lift turbines.
reset()
shell('Suspended arc chassis',(0,0,1.03),.66,1.10,.40,0)
shell('Split pale arc crown',(0,.13,1.28),.48,.83,.18,2)
bone('CORE',(0,-.37,1.05))
ring('Arc containment bezel',(0,-.45,1.05),.19,.041,1,'CORE',(0,-1,0))
rod('Hostile arc optic',(0,-.44,1.05),(0,-.51,1.05),.13,5,'CORE',16)
for i,(x,y) in enumerate([(-.68,-.43),(.68,-.43),(-.63,.52),(.63,.52)]):
    B='ROTOR_'+str(i);bone(B,(x,y,.97))
    rod('Turbine outrigger',(x*.25,y*.4,1.02),(x,y,.97),.068,1)
    ring('Opaque duct housing',(x,y,.97),.25,.072,0)
    ring('Machined turbine rim',(x,y,1.015),.245,.018,1)
    for a in range(4):
        t=a*math.pi/2
        rod('Lift rotor blade',(x,y,.97),(x+.18*math.cos(t),y+.18*math.sin(t),.97),.026,14,B,8)
    shell('Protective turbine shoulder',(x,y+.13,1.1),.32,.25,.09,2)
    rod('Insulated arc electrode',(x,y,1.12),(x*1.16,y,1.43),.034,1)
grille((0,.23,1.38),.40,.33)
rig_and_export('ArcDrone_V1')

# Carrier: industrial cargo crawler; tandem wheel bogies, lift forks, protected cargo spine.
reset()
shell('Loadbearing cargo skid',(0,0,.43),1.43,2.02,.30,0)
shell('Cargo armored cab',(0,-.63,.78),1.32,.74,.57,2)
shell('Recessed cargo bed',(0,.44,.68),1.18,1.32,.18,14)
bone('CORE',(0,-.9,.79));rod('Cab hostile optic',(0,-1.00,.83),(0,-1.05,.83),.12,5,'CORE')
for s in (-1,1):
    for i,y in enumerate([-.68,0,.68]):
        B='WHEEL_'+str(s)+'_'+str(i);bone(B,(s*.78,y,.28))
        rod('Wide segmented wheel',(s*.66,y,.28),(s*.94,y,.28),.28,0,B,16)
        ring('Wheel bearing',(s*.95,y,.28),.13,.037,1,B,(1,0,0))
    shell('Continuous track guard',(s*.80,0,.61),.34,2.07,.15,1)
    rod('Lift hydraulic piston',(s*.45,-.60,.60),(s*.45,-1.08,.42),.059,1)
    shell('Tapered salvage fork',(s*.45,-1.20,.25),.21,.78,.12,1,nose=1)
    box('Cargo retention cage',(s*.56,.43,1.02),(.09,1.26,.66),0)
for y in [.04,.42,.80]:
    rod('Cargo pressure capsule',(0,y,.73),(0,y,1.20),.24,1,vertices=16)
    ring('Capsule insulated rim',(0,y,1.12),.24,.03,0)
    box('Cargo pale retention strap',(0,y,1.28),(.88,.075,.12),2)
grille((0,-.51,1.10),.74,.21)
rig_and_export('Carrier_V1')

# Warden: low fortress walker with broad layered shield, exposed four hydraulic legs.
reset();legs(4,1.10,.55,.10)
shell('Warden armored chassis',(0,.12,.60),1.45,1.17,.48,0)
shell('Forward convex defensive shell',(0,-.43,.88),1.67,.65,.66,2,nose=1)
shell('Nested graphite shield',(0,-.77,.86),1.32,.14,.44,14,nose=1)
for s in [-1,1]:
    shell('Pale reinforced shield shoulder',(s*.65,-.49,1.18),.36,.68,.19,2)
    rod('Shield support piston',(s*.55,.15,.75),(s*.56,-.49,1.0),.070,1)
    shell('Rear armored haunch',(s*.47,.52,.83),.45,.42,.23,1)
    grille((s*.40,.43,.97),.32,.25,4)
bone('CORE',(0,-.80,.98));rod('Narrow hostile visor',(-.19,-.87,1.04),(.19,-.87,1.04),.034,5,'CORE')
ring('Rear shield coupling',(0,.65,.63),.20,.051,1,axis=(0,1,0))
rig_and_export('Warden_V1')

# Custodian: six-legged containment engine with articulated industrial clamping arms.
reset();legs(6,2.10,1.06,.21)
shell('Custodian containment lower hull',(0,.17,1.01),2.90,2.73,.67,0)
shell('Sloped containment glacis',(0,-.41,1.42),2.63,2.05,.48,2)
rod('Central armored pressure drum',(0,.30,1.33),(0,.30,2.63),.66,14,vertices=24)
for z in [1.49,1.95,2.48]: ring('Pressure drum containment hoop',(0,.30,z),.74,.096,1)
shell('Containment crown',(0,.30,2.79),1.93,1.71,.24,2)
bone('CORE',(0,-.80,1.82))
ring('Boss recessed containment aperture',(0,-.91,1.91),.46,.11,0,'CORE',(0,-1,0))
ring('Boss aperture steel inner',(0,-1.03,1.91),.33,.032,1,'CORE',(0,-1,0))
rod('Controlled hostile reactor',(0,-1.01,1.91),(0,-1.07,1.91),.27,5,'CORE',24)
for s,label in [(-1,'R'),(1,'L')]:
    B=label+'_CLAW';bone(B,(s*1.16,-.24,1.56))
    ring('Primary clamp rotary joint',(s*1.24,-.23,1.61),.32,.095,1,B,axis=(1,0,0))
    rod('Dual actuator forearm',(s*1.3,-.24,1.6),(s*1.83,-.93,1.12),.19,0,B)
    rod('Exposed clamp hydraulic',(s*1.2,-.29,1.84),(s*1.80,-.85,1.33),.075,1,B)
    shell('Long industrial restraint claw',(s*1.80,-1.14,1.06),.69,1.33,.43,2,B,nose=1)
    shell('Restraint jaw inner',(s*1.55,-1.65,.88),.24,.70,.27,1,B)
    for j in range(3):box('Clamp warning inlay',(s*1.81,-.89-j*.20,1.30),(.31,.064,.013),4,B,angle=.42)
    for j in range(5): shell('Rear articulated exhaust armor',(s*.88,.58+j*.19,1.69-j*.07),.61,.35,.15,1)
    rod('Containment energy trunk',(s*.75,.35,1.40),(s*.75,.35,2.50),.069,0)
    grille((s*.81,-.15,1.91),.44,.63,7)
rig_and_export('Custodian_V1')

# Relay mast: supported communications structure, panel antennae and cable couplings.
reset()
shell('Relay octagonal footing',(0,0,.22),1.8,1.8,.44,0)
rod('Relay load mast',(0,0,.35),(0,0,4.25),.18,1)
for s in [-1,1]:
    rod('Triangulated relay brace',(s*.70,0,.35),(0,0,2.25),.08,0)
    shell('Relay antenna shield',(s*.63,0,3.23),.46,.72,1.76,2)
    rod('Antenna cross beam',(0,0,3.5),(s*.65,0,3.5),.07,1)
    for z in [2.71,3.1,3.49]:box('Signal vane',(s*.63,-.37,z),(.25,.027,.15),12)
ring('Signal director ring',(0,0,4.34),.63,.051,1)
rod('Mast signal pin',(0,0,4.24),(0,0,5.2),.027,1)
shell('Junction cabinet',(0,-.40,.90),1.2,.72,1.18,14)
grille((0,-.31,1.52),.78,.36)
box('Cyan signal status',(0,-.78,1.10),(.44,.015,.052),7)
export_static('Relay_Mast')

reset()
shell('Routing cabinet plinth',(0,0,.14),1.52,1.72,.28,0)
shell('Relay armored junction',(0,0,1.0),1.32,1.42,1.46,1)
for s in [-1,1]:
    for z in [.63,1.07,1.43]:
        rod('Cable grommet',(s*.65,0,z),(s*.9,0,z),.11,0)
        ring('Cable coupling',(s*.88,0,z),.14,.028,1,axis=(1,0,0))
grille((0,0,1.79),.91,.80)
box('Routing status screen',(0,-.73,1.23),(.48,.022,.21),8)
box('Contained signal status',(0,-.75,1.23),(.38,.014,.037),7)
export_static('Relay_Junction')

# Shield stacks have thickness, nested frames, separation and recognisable plate profiles.
reset()
shell('Armor sorting skid',(0,0,.12),2.23,2.24,.24,0)
for i in range(5):
    shell('Salvaged armor laminate',(0,-i*.08,.34+i*.21),1.97-i*.07,1.77,.18,2 if i%2==0 else 1,nose=1)
    box('Muted shield ID stripe',(0,-.90-i*.08,.35+i*.21),(.68,.028,.07),10)
for s in [-1,1]:rod('Stack retention post',(s*.91,.8,.21),(s*.91,.8,1.50),.072,0)
export_static('Shield_Stack')
reset()
shell('Containment shell lower',(0,0,.20),2.80,2.48,.40,0)
for s in [-1,1]:
    shell('Damaged shielding side',(s*.97,0,1.0),.52,2.45,1.40,2)
    rod('Exposed damaged defensive brace',(s*.89,-.8,.45),(s*.62,.68,2.12),.12,1)
    box('Industrial shield paint',(s*1.26,0,1.04),(.032,.59,.40),10)
ring('Empty shield generator socket',(0,0,.52),.68,.15,1)
export_static('Shield_Shell')

# Capacitors: contained storage vessels, ceramic isolators and functional exposed bus bars.
reset()
shell('Capacitor bank skid',(0,0,.18),2.61,3.05,.36,0)
for x in [-.72,.72]:
    for y in [-.82,.82]:
        rod('Energy vessel',(x,y,.35),(x,y,2.11),.38,1,vertices=16)
        for z in [.55,1.61,2.0]:ring('Vessel protective collar',(x,y,z),.39,.057,0)
        rod('Ceramic terminal',(x,y,2.1),(x,y,2.45),.095,2)
        for z in [2.20,2.31,2.42]:ring('Insulator fins',(x,y,z),.14,.025,2)
        box('Vessel identification',(x,y-.39,1.3),(.25,.02,.30),13)
        box('Small energy window',(x,y-.40,1.0),(.11,.01,.09),6)
    rod('Protected copper bus',(x,-.82,2.47),(x,.82,2.47),.065,4)
for s in [-1,1]:rod('Diagonal vessel support',(s*1.08,1.20,.30),(s*.76,1.20,2.0),.078,0)
export_static('Capacitor_Bank')
reset()
shell('Transformer foundation',(0,0,.21),2.60,2.26,.42,0)
shell('Transformer main housing',(0,0,1.17),1.73,1.73,1.50,14)
for s in [-1,1]:
    for y in [-.65,-.35,-.05,.25,.55]:box('Radiator fin',(s*1.05,y,1.10),(.38,.055,1.41),1)
for x in [-.55,0,.55]:
    rod('Transformer isolator',(x,0,1.95),(x,0,2.81),.10,2)
    for z in [2.05,2.2,2.35,2.5]:ring('Terminal ceramic shed',(x,0,z),.19,.032,2)
rod('Amber high voltage bus',(-.55,0,2.84),(.55,0,2.84),.053,4)
box('Electrical hazard face',(0,-.88,1.21),(.43,.028,.32),4)
export_static('Transformer')

# Hauler wreck: complete industrial wheel chassis, gutted cargo frame and damaged cab.
reset()
shell('Heavy hauler belly',(0,0,.50),2.60,4.30,.60,0)
shell('Wrecked hauler cab',(0,-1.26,1.41),2.31,1.51,1.42,2,nose=1)
shell('Recessed cab glazing',(0,-1.89,1.66),1.55,.12,.58,8)
box('Violet fleet identification',(0,-2,1.18),(.67,.022,.10),11)
for s in [-1,1]:
    for y in [-1.26,.08,1.41]:
        rod('Massive transport wheel',(s*1.04,y,.51),(s*1.61,y,.51),.49,0,vertices=16)
        ring('Wheel cast axle',(s*1.63,y,.51),.24,.071,1,axis=(1,0,0))
    for y in [0,.95,1.84]:
        rod('Broken cargo arch',(s*1.09,y,.81),(s*.91,y,2.44 if y!=.95 else 1.77),.11,1)
    rod('Transport cargo upper beam',(s*.91,0,2.44),(s*.91,1.84,2.44),.11,1)
    grille((s*.64,-1.18,2.19),.40,.60)
for y in [.2,.8,1.4]:box('Heavy loading deck seam',(0,y,.86),(1.96,.18,.04),14)
export_static('Hauler_Wreck')
reset()
shell('Loader mounting skid',(0,0,.2),2.4,2.4,.4,0)
rod('Loading crane support',(0,0,.4),(0,0,3.1),.20,1)
rod('Canted loading boom',(0,0,3.0),(1.30,-.65,3.85),.18,0)
rod('Loader exposed ram',(.12,0,1.95),(1.0,-.5,3.6),.08,1)
rod('Damaged salvage clamp',(1.30,-.65,3.85),(1.30,-.65,2.31),.07,1)
for s in [-1,1]:rod('Salvage clamp fork',(1.30,-.65,2.31),(1.30+s*.32,-.65,1.91),.08,0)
shell('Loader drive housing',(-.5,.2,.92),.91,1.28,.76,14)
box('Loader fleet paint',(-.95,.2,.96),(.025,.61,.31),11)
export_static('Loading_Crane')

# Shared architecture: arched supports, functional trunks, floor plates and containment buttresses.
reset()
for s in [-1,1]:
    shell('Service arch cast footing',(s*3.4,0,.25),1.06,1.12,.5,0)
    rod('Service arch upright',(s*3.4,0,.4),(s*3.4,0,3.25),.17,1)
    rod('Angled arch shoulder',(s*3.4,0,3.25),(s*2.4,0,4.04),.17,1)
rod('Suspended utility truss',(-2.4,0,4.04),(2.4,0,4.04),.19,0)
for y in [-.16,.16]:rod('Raised cable trunk',(-3.0,y,3.82),(3.0,y,3.82),.06,1)
export_static('Service_Arch')
reset()
shell('Massive containment footing',(0,0,.3),1.83,2.68,.6,0)
shell('Sloped reinforced arena spine',(0,0,1.9),1.36,1.86,3.22,1)
shell('Containment spine pale crown',(0,.0,3.61),1.83,2.26,.31,2)
rod('Reinforcement diagonal',(-.5,-.8,.4),(.35,-.8,3.33),.14,0)
rod('Contained energy feed',(0,.87,.5),(0,.87,3.25),.105,0)
for z in [1.1,2.8]:ring('Reinforced feed flange',(0,.87,z),.145,.033,1)
box('Amber containment status',(0,-.96,2.37),(.21,.019,.31),6)
export_static('Containment_Buttress')
reset()
shell('Control chamber pedestal',(0,0,.33),3.55,3.03,.66,0)
rod('Containment energy core',(0,0,.61),(0,0,3.55),.78,14,vertices=24)
for z in [.88,1.53,2.80,3.42]:ring('Main containment pressure hoop',(0,0,z),.91,.12,1)
for i in range(8):
    a=i*math.tau/8
    x,y=math.cos(a)*.98,math.sin(a)*.98
    rod('Control vessel spine',(x,y,.66),(x,y,3.55),.13,1)
    shell('Control vessel armor',(x,y,1.91),.56,.42,1.24,2)
shell('Primary containment crown',(0,0,3.8),2.97,2.56,.42,2)
for s in [-1,1]:rod('Primary energy trunk',(s*.4,0,.72),(s*1.90,0,.72),.22,0)
box('Contained amber monitoring',(0,-1.0,2.33),(.15,.015,.55),6)
export_static('Primary_Containment')
reset()
box('Manufactured floor undertray',(0,0,-.015),(7.98,7.98,.045),15,bevel=.003)
for x in [-2,2]:
    for y in [-2,2]:
        shell('Graphite floor panel',(x,y,.021),3.94,3.94,.027,3,nose=0)
        for v in [-1.1,1.1]: box('Machined floor service seam',(x,y+v,.045),(3.25,.014,.007),14,bevel=0)
for y in [-3.6,-2.7,-1.8,-.9,0,.9,1.8,2.7,3.6]:box('Recessed floor drain',(0,y,.032),(.14,.61,.04),1,bevel=.002)
export_static('Facility_Deck')
reset()
for x in [-2.6,2.6]:
    box('Service route painted rail',(x,0,.01),(.065,7.74,.014),13,bevel=0)
    for y in [-3.1,0,3.1]:
        for s in [-1,1]:box('Service direction chevron',(x+s*.14,y,.02),(.055,.37,.014),2,angle=s*.65,bevel=0)
export_static('Service_Markings')

(ROOT/'docs/chapter01-production').mkdir(parents=True,exist_ok=True)
(ROOT/'docs/chapter01-production/asset_metrics.json').write_text(json.dumps(metrics,indent=2))
print('CHAPTER01_ORIGINAL_ASSETS_COMPLETE',json.dumps(metrics))
