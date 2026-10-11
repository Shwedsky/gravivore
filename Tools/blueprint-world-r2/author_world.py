"""Presentation-only refinement over the frozen R1 layout and donor pipeline."""
from pathlib import Path
import json

ROOT=Path(__file__).resolve().parents[2]
PASS=ROOT/'docs/history/implementation-passes/chapter01-blueprint-world-r2'
SOURCE=ROOT/'art/blueprint-world-r2'
PASS.mkdir(parents=True,exist_ok=True);SOURCE.mkdir(parents=True,exist_ok=True)
baseline=(ROOT/'Tools/blueprint-world-r1/author_world.py').read_text()
# Reuse the inspected, licensed import/export machinery, without modifying
# R1 authoring instructions, physical layout or encounter coordinates.
baseline=baseline.replace("SOURCE=ROOT/'art/blueprint-world-r1'","SOURCE=ROOT/'art/blueprint-world-r2'")
baseline=baseline.replace("Path(__file__).parent/'layout.json'","ROOT/'Tools/blueprint-world-r1/layout.json'")
baseline=baseline.replace("Chapter01_BlueprintWorld_R1.blend","Chapter01_BlueprintWorld_R2.blend")
baseline=baseline.replace("chapter01-blueprint-world-r1/authored-sector-meshes.json","chapter01-blueprint-world-r2/authored-sector-meshes.json")
cut=baseline.index('manifest=[]')
exec(compile(baseline[:cut],str(__file__)+' / donor pipeline','exec'),globals())
for name,mat in MATERIALS.items():
    nodes=mat.node_tree.nodes;links=mat.node_tree.links
    shader=[node for node in nodes if node.type=='BSDF_PRINCIPLED'][-1]
    for suffix,input_name in [('Normal','Normal'),('Emission','Emission Color')]:
        path=ROOT/'Assets/_Game/Content/BlueprintWorldR1/Textures'/f'{name}_{suffix}.png'
        if not path.exists():continue
        texture=nodes.new('ShaderNodeTexImage');texture.image=bpy.data.images.load(str(path))
        if suffix=='Normal':
            texture.image.colorspace_settings.name='Non-Color';normal_node=nodes.new('ShaderNodeNormalMap')
            links.new(texture.outputs['Color'],normal_node.inputs['Color']);links.new(normal_node.outputs['Normal'],shader.inputs['Normal'])
        else:links.new(texture.outputs['Color'],shader.inputs[input_name]);shader.inputs['Emission Strength'].default_value=1.55
CURRENT='repair-hub'
original_donor=donor;original_vessel=vessel

def atlas_uv(obj,swatch,polygons=None):
    uv=obj.data.uv_layers.active or obj.data.uv_layers.new(name='UVMap')
    for face in (obj.data.polygons if polygons is None else polygons):
        vertices=[obj.data.vertices[obj.data.loops[i].vertex_index].co for i in face.loop_indices]
        axes=sorted(range(3),key=lambda i:abs(face.normal[i]))[:2]
        lows=[min(v[i] for v in vertices) for i in axes];highs=[max(v[i] for v in vertices) for i in axes]
        for loop,vert in zip(face.loop_indices,vertices):
            u=(vert[axes[0]]-lows[0])/max(.01,highs[0]-lows[0]);v=(vert[axes[1]]-lows[1])/max(.01,highs[1]-lows[1])
            uv.data[loop].uv=((swatch%4+.06+u*.88)/4,1-(swatch//4+1-(.06+v*.88))/4)
    return obj

def donor(name,position,size=None,yaw=0):
    start=len(parts);original_donor(name,position,size,yaw)
    energy=5 if CURRENT in ['repair-hub','capacitor-field'] else 6 if CURRENT in ['relay-yard','shield-dump','hauler-graveyard','elite-arena'] else 7
    for obj in parts[start:]:
        for face in obj.data.polygons:
            material=obj.data.materials[face.material_index]
            if material!=MATERIALS['R1_Industrial']:continue
            uv=obj.data.uv_layers.active
            centre=sum((uv.data[i].uv for i in face.loop_indices),Vector((0,0)))/len(face.loop_indices)
            slot=min(3,int(centre.x*4))+4*min(3,int((1-centre.y)*4))
            if slot in [5,6,7]:atlas_uv(obj,energy,[face])

def armor_panel(name,position,width,depth,height,swatch=0,yaw=0):
    # A stepped/chamfered enclosure profile, with sloped shoulder planes.
    w=width/2;d=depth/2;c=min(.065,min(width,depth)*.045) if height<.06 else min(width,depth)*.17
    outline=[(-w+c,-d),(w-c,-d),(w,-d+c),(w,d-c),(w-c,d),(-w+c,d),(-w,d-c),(-w,-d+c)]
    verts=[(x,y,z) for z in [-height/2,height/2] for x,y in outline]
    faces=[tuple(range(7,-1,-1)),tuple(range(8,16))]+[(i,(i+1)%8,(i+1)%8+8,i+8) for i in range(8)]
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update()
    obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj);obj.location=position;obj.rotation_euler.z=math.radians(yaw)
    custom_material(obj,swatch);return register(obj,name)

def grate(x,y,width,depth,angle=0):
    box('flush recessed service cassette',(x,y,-.004),(width,depth,.026),14,.015,angle)
    box('perforated access grate',(x,y,.014),(width-.09,depth-.09,.021),13,.015,angle)
    for side in [-1,1]:
        box('grate loadbearing frame',(x+side*(width/2-.06),y,.022),(.065,depth,.023),1,.009)

def floor_plate(x,y,w,d,slot,angle=0):
    armor_panel('fitted sector deck cassette',(x,y,-.006),w,d,.028,0,angle)
    # Each large load field is an assembly of replaceable irregular inspection
    # plates; seam scale follows real hardware rather than a stretched hatch.
    across=max(1,math.ceil(w/2.1));along=max(1,math.ceil(d/2.5))
    rot=Matrix.Rotation(math.radians(angle),4,'Z')
    for ix in range(across):
        for iy in range(along):
            offset=rot@Vector((-w/2+(ix+.5)*w/across,-d/2+(iy+.5)*d/along,0))
            tone=slot if (ix+iy)%3 else 1 if slot in [11,15] else 2 if slot in [9,12] else 0
            armor_panel('removable deck inspection panel',(x+offset.x,y+offset.y,.015),w/across-.045,d/along-.055,.022,tone,angle)
    # Hardware follows plate corners and seams, never random distribution.
    for sx,sy in [(-1,-1),(1,1)]:
        box('recessed deck lock',(x+sx*(w*.5-.19),y+sy*(d*.5-.18),.025),(.10,.20,.013),4,.008,angle)

def deck(name,width,depth,swatch=2):
    category_set('floor')
    box(name+' foundation',(0,0,-.48),(width,depth,.9),0,.25)
    box(name+' graphite underdeck',(0,0,-.025),(width-.15,depth-.15,.055),14,.04)
    sectors={'repair-hub':(15,8),'relay-yard':(8,2),'capacitor-field':(8,0),'cutting-floor':(10,2),
             'shield-dump':(11,0),'hauler-graveyard':(9,2),'elite-arena':(0,12),'boss-arena':(10,0)}
    primary,secondary=sectors[CURRENT]
    # Unequal plate fields form inspection cells and relationships to machinery.
    for x in [-width*.29,0,width*.29]:
        for j,y in enumerate([-depth*.32,-depth*.02,depth*.29]):
            floor_plate(x,y,width*.28-.08,depth*(.26 if j==1 else .29)-.08,primary if j!=1 else secondary)
    for x in [-4.2,4.2]:
        for y in [-depth*.32,-depth*.02,depth*.29]:grate(x,y,.8,depth*.23)
    # Recesses are service construction, not empty black stripes between tiles.
    for y in [-depth*.17,depth*.13]:
        grate(0,y,min(width*.90,18),.68)
        for x in [-3.8,3.8]:
            pipe('embedded hydraulic route',[(x,y-.16,-.035),(x,y+.16,-.035)],.10,4)
    if CURRENT=='repair-hub':
        cylinder('clean graphite maintenance dais',(0,0,.012),3.8,.028,8)
        for a in range(0,360,30):
            r=math.radians(a);armor_panel('radial diagnostic dock plate',(3.25*math.cos(r),3.25*math.sin(r),.04),1.42,.50,.026,15,a+90)
    elif CURRENT=='capacitor-field':
        for x in [-2.7,2.7]:
            box('grounded electrical routing duct',(x,0,.015),(.42,depth-.8,.027),0,.018)
            for y in [-6,-1,4]:box('cyan insulated junction',(x,y,.035),(.23,.50,.012),5,.014)
    elif CURRENT in ['cutting-floor','relay-yard']:
        for y in [-5.0,3.0]:box('marked machinery work envelope',(0,y,.031),(7.2,.46,.022),3,.018)
        for x in [-2.9,2.9]:box('embedded production carrier rail',(x,0,.035),(.12,depth-1,.026),4,.01)
    elif CURRENT=='shield-dump':
        for x in [-2.8,2.8]:
            for y in [-5,0,5]:armor_panel('armored deck inspection socket',(x,y,.029),1.7,1.7,.023,0,45)
    elif CURRENT=='hauler-graveyard':
        for x,y,a in [(-2,-5,12),(2,1,-9),(-1,5,17)]:
            armor_panel('patched freight abrasion plate',(x,y,.03),3.1,1.8,.022,2,a)
            box('worn freight rail repair',(x,y,.035),(.14,2.8,.024),4,.009,a)
    else:
        radius=5.4 if CURRENT=='elite-arena' else 7.1
        for a in range(0,360,20):
            r=math.radians(a);x=radius*math.cos(r);y=radius*math.sin(r)
            armor_panel('reactor inspection perimeter',(x,y,.027),1.75,.60,.022,12 if CURRENT=='elite-arena' else 3,a+90)
    category_set('architecture')

def bay(x,y,width=7,energy=6,yaw=0):
    # Open roof structure and tiered end caps expose machinery at the camera.
    for side in [-1,1]:
        armor_panel('bay structural cheek',(x+side*(width/2-.35),y,2.0),.92,2.5,4.0,0)
        armor_panel('bay ribbed pressure endcap',(x+side*(width/2-.35),y-.48,2.8),.75,.8,1.1,1)
        for z in [1.0,1.65,2.3]:box('jamb cooling louvre',(x+side*(width/2-.35),y-.83,z),(.58,.18,.11),14,.02)
        pipe('exposed bay actuator',[(x+side*(width/2-.35),y-.70,.4),(x+side*(width/2-.35),y-.70,3.5)],.07,4)
        armor_panel('sloped bay roof segment',(x+side*width*.29,y+.75,4.1),width*.38,2.3,.45,8 if CURRENT=='relay-yard' else 0)
    box('open roof rear load beam',(x,y+1.8,4.15),(width+.7,.65,.70),1,.11)
    box('machinery cell header',(x,y+.05,3.6),(width-.9,.40,.55),0,.08)
    for dx in [-width*.3,0,width*.3]:
        box('local work cell luminaire',(x+dx,y-.19,3.48),(.7,.10,.13),energy,.018)
    donor('Wall Command',(x,y+2.0,.3),(width-.9,.35,3.0))

def vessel(x,y,height=5.4,energy=5):
    original_vessel(x,y,height,energy)
    # Broad contained energy aperture is surrounded by opaque pressure armor.
    cylinder('contained sector energy spine',(x,y,height*.5),.43,height*.64,energy,32)
    for z in [.8,height*.50,height-.45]:
        cylinder('segmented pressure coupling',(x,y,z),1.18,.18,0,40)
    for side in [-1,1]:
        box('energy vessel cheek',(x+side*.68,y-.55,height*.5),(.32,.37,height*.60),0,.065)
        box('pressure aperture armor',(x+side*.62,y-.75,height*.5),(.10,.20,height*.48),4,.035)

old_functions=[repair,relay,capacitor,cutting,shield,hauler,magnetar,custodian]

def peripheral_workcell(x,y,energy,kind='diagnostic'):
    armor_panel(kind+' service body',(x,y,1.12),1.55,1.70,2.24,0)
    armor_panel(kind+' steel shoulder',(x,y,2.12),1.72,1.82,.42,1)
    box(kind+' deep service cavity',(x,y-.87,1.18),(.96,.12,1.4),14,.08)
    for dx in [-.44,.44]:box(kind+' energy aperture',(x+dx,y-.95,1.24),(.13,.05,1.0),energy,.02)
    for z in [.6,1.05,1.5]:box(kind+' inspection louvre',(x,y-.96,z),(.60,.06,.11),1,.015)
    pipe(kind+' protected return',[(x+.62,y+.6,.2),(x+.62,y+.6,2.65),(x,y+.6,2.65)],.08,4)

def suspended_edge_services(index):
    if index>=6:return
    primary=5 if index in [0,2] else 7 if index==3 else 6
    for side in [-1,1]:
        for y in [-6.0,-1.0,4.0]:
            pipe('supported overhead service cantilever',[(side*7.6,y,2.8),(side*7.6,y,4.6),(side*4.15,y,4.6)],.16,1)
            armor_panel('overhead integrated machinery shoulder',(side*4.25,y,3.8),1.55,1.9,.85,8 if index in [0,2] else 9 if index==5 else 0)
            for dx in [-.48,.48]:
                box('overhead service armor seam',(side*4.25+dx,y-.8,3.85),(.14,.14,.6),4,.026)
            for z in [3.60,3.86,4.10]:
                box('overhead heat exchanger louvre',(side*4.25,y-1.00,z),(1.15,.17,.13),14,.025)
            box('localized downward work luminaire',(side*4.25,y,3.35),(.7,.55,.10),primary,.025)
            if index==2:
                for dx in [-.3,.3]:cylinder('suspended insulated electrical bushing',(side*4.25+dx,y,4.5),.17,.55,5,24)
            if index==5:
                pipe('damaged freight gantry hose',[(side*4.25,y,3.4),(side*4.1,y+.4,2.9),(side*4.3,y+.6,2.7)],.06,4)

def refine_sector(index):
    global CURRENT
    CURRENT=LAYOUT['sectors'][index]['id'];old_functions[index]()
    category_set('machinery')
    suspended_edge_services(index)
    if index==0:
        for side in [-1,1]:
            peripheral_workcell(side*7.55,-2,5)
            # Existing side-wall footprint contains overhead repair tooling.
            pipe('articulated diagnostic gantry',[(side*7.5,1,3.8),(side*6.5,1,4.9),(side*5.2,1,4.3)],.20,1)
            armor_panel('diagnostic scanning head',(side*5.2,1,4.2),1.1,.65,.65,8)
            box('cyan diagnostic aperture',(side*5.2,.63,4.15),(.72,.06,.14),5,.018)
    elif index==1:
        # Remove the full-width smooth roof; retain a readable open bay crown.
        for obj in list(parts):
            if any(n in obj.name for n in ['relay deployment building roof','relay continuous control fascia']):
                parts.remove(obj);bpy.data.objects.remove(obj,do_unlink=True)
        for x in [-4.8,4.8]:
            armor_panel('relay roof comms cabinet',(x,5.8,3.7),2.0,2.6,.65,8)
            cylinder('relay antenna pedestal',(x,5.8,4.25),.38,.35,1)
            pipe('folded relay aerial',[(x,5.8,4.3),(x,5.8,5.1),(x-.45,5.8,5.1)],.055,4)
        box('relay open skeletal roof crown',(0,6.6,4.2),(11.4,.48,.45),0,.09)
        for side in [-1,1]:peripheral_workcell(side*7.6,-2.4,6,'deployment')
    elif index==2:
        for side in [-1,1]:
            peripheral_workcell(side*7.45,-2.8,5,'grounded switch')
            pipe('contained energy supply',[(-7,-4,.2),(-4.8,-4,.2),(-4.8,5,.2),(-2.6,8,.2)],.16,0)
        for x in [-2.6,3.5]:
            box('cyan capacitor aperture',(x,7.1,3.25),(.7,.12,3.2),5,.09)
    elif index==3:
        for obj in list(parts):
            if 'cutting press shoulder' in obj.name:
                parts.remove(obj);bpy.data.objects.remove(obj,do_unlink=True)
        for x in [-3.6,0,3.6]:
            armor_panel('stepped loadbearing press crown',(x,8,5.2),3.38,2.9,1.08,12 if x==0 else 0)
            for dx in [-.9,0,.9]:box('press crown frontal service slot',(x+dx,6.5,5.2),(.28,.25,.35),14,.035)
            cylinder('press crown tension anchor',(x,8,5.85),.28,.25,4,24)
        for side in [-1,1]:
            peripheral_workcell(side*7.5,-2.2,7,'hot hydraulic')
            for z in [1.0,2.1,3.1]:
                box('furnace armored shutter',(side*5.3,7.8,z),(.7,.7,.70),0,.10)
                box('furnace deep hot aperture',(side*5.3,7.36,z),(.42,.12,.38),7,.04)
        for x in [-3,-1.5,0,1.5,3]:
            armor_panel('press removable inspection armor',(x,7.25,4.5),1.34,.35,.65,12)
    elif index==4:
        for side in [-1,1]:peripheral_workcell(side*7.5,-2.0,6,'armored recovery')
        for x in [-4,0,4]:
            for z in [1.2,2.4,3.7]:
                armor_panel('replaceable armored shield cassette',(x,5.91,z),2.45,.45,.75,11)
                for dx in [-.8,.8]:box('shield locking dog',(x+dx,5.61,z),(.18,.16,.4),4,.04)
    elif index==5:
        for obj in list(parts):
            if 'damaged freight armored body' in obj.name:
                x,y,z=obj.location;w,d,h=obj.dimensions;a=math.degrees(obj.rotation_euler.z)
                parts.remove(obj);bpy.data.objects.remove(obj,do_unlink=True)
                for side in [-1,1]:
                    armor_panel('torn transport hull side',(x+side*(w/2-.10),y,z),.23,d,h,9,a)
                armor_panel('transport shell rear bulkhead',(x,y+d/2-.10,z),w,.22,h,8,a)
                for dx in [-w*.30,0,w*.30]:
                    pipe('exposed transport internal frame',[(x+dx,y-d/2,z-h/2),(x+dx,y-d/2,z+h*.32),(x+dx,y+d/2,z+h*.32)],.09,1)
                armor_panel('empty cargo bay deep cavity',(x,y,z-h/2+.15),w-.3,d-.3,.15,14,a)
            elif 'freight lid split' in obj.name:
                obj.rotation_euler.x=math.radians(22 if obj.location.x<0 else -12)
        for x,y,w,a in [(-3,8,6,-7),(5,8,3.8,11)]:
            armor_panel('exposed damaged transport belly',(x,y-1.9,1.45),w,.45,1.45,9,a)
            for dx in [-w*.28,0,w*.28]:
                box('broken transport internal crossmember',(x+dx,y-2.15,1.5),(.17,.22,1.35),0,.02,a)
            pipe('severed transport hydraulic',[(x-w*.3,y-2,.35),(x-w*.3,y-2,1.75),(x-w*.3+.65,y-2,1.92)],.07,4)
        peripheral_workcell(7.4,-1.3,6,'freight handling')
    elif index==6:
        for y in [-4,0,4]:
            # Existing reactor footprint, not the clear centre, owns density.
            if y==4:peripheral_workcell(-8.2,y,6,'magnetic shunt')
        for x in [-2.6,-7.4]:
            pipe('laminated copper induction winding',[(x,3.7,1),(x,3.7,4.5),(x+.45,3.7,4.5)],.16,12)
    else:
        for side in [-1,1]:
            # The existing reactor footprint contains these boss-grade cheeks.
            armor_panel('containment monolithic stepped cheek',(side*3.9,9.2,2.6),1.0,2.4,5.2,0)
            for z in [1.2,2.4,3.6]:
                armor_panel('containment interlocked armor',(side*3.9,7.9,z),1.18,.6,.85,1)
                box('red containment interlock',(side*3.9,7.56,z),(.12,.07,.52),7,.02)
        for x in [-2.4,0,2.4]:
            box('reactor crown heat exchanger',(x,10.2,6.2),(1.8,1.3,.6),0,.1)

repair=lambda:refine_sector(0);relay=lambda:refine_sector(1);capacitor=lambda:refine_sector(2);cutting=lambda:refine_sector(3)
shield=lambda:refine_sector(4);hauler=lambda:refine_sector(5);magnetar=lambda:refine_sector(6);custodian=lambda:refine_sector(7)

# Lower and compress the tall chambers within the SAME existing footprints;
# expose their mechanical head/shell hierarchy at the fixed portrait camera.
old_induction=induction_machine
def induction_machine(x,y,scale=1,energy=6):
    start=len(parts);old_induction(x,y,scale,energy)
    sx=.69 if CURRENT=='elite-arena' else .72;sy=.65 if CURRENT=='elite-arena' else .54;sz=.67 if CURRENT=='elite-arena' else .52
    anchor=Vector((x,y,0));new_anchor=anchor+Vector((1.3,-.65,0)) if CURRENT=='elite-arena' else anchor+Vector((0,-.4,0))
    fit=Matrix.Diagonal((sx,sy,sz,1))
    for obj in parts[start:]:obj.matrix_world=Matrix.Translation(new_anchor)@fit@Matrix.Translation(-anchor)@obj.matrix_world

def connection_floor():
    # R1's continuous top plates covered the sector-specific decks. Keep the
    # same union but draw connecting bands only outside sector foundations.
    category_set('floor')
    for a,b in [(-20,-19),(-1,0)]:
        if b<=a:continue
        box('connecting spine foundation',(0,(a+b)/2,-.25),(8,b-a,.5),0,.08)
        floor_plate(0,(a+b)/2,7.75,b-a-.05,8)
    for a,b in [(0,6),(14,28),(36,44),(64,63)]:
        if b<=a:continue
        for y in range(a,b,2):
            floor_plate(0,y+1,7.75,1.94,8 if y<20 else 2)
            for x in [-3.1,3.1]:grate(x,y+1,.75,1.65)
    for y in [10,32]:
        floor_plate(0,y,9.85,7.8,0)
        for x in [-3.2,3.2]:grate(x,y,1.2,6.8)

tail=baseline[cut:]
start=tail.index("for y,length in [(-20,3),(0,94)]:")
end=tail.index("category_set('architecture')",start)
tail=tail[:start]+"connection_floor()\n"+tail[end:]
exec(compile(tail,str(__file__)+' / frozen-layout export','exec'),globals())
