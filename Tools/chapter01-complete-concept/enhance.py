"""Camera review refinement: functional floor channels and exposed mechanical layers.
Edits the individual source forms; never paints over gameplay evidence.
"""
import sys,math,json,bpy,bmesh
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
import meshlib as m
from meshlib import box,prism,lathe,cyl,arc,tube,bolts,grate
ROOT=m.ROOT;OUT=m.OUT;SOURCE=m.SOURCE;PASS=m.PASS
bpy.ops.wm.open_mainfile(filepath=str(SOURCE/'Chapter01Facilities.blend'))
for key,prefix in [('Graphite','CF'),('Shell','CF'),('Steel','CF'),('Deck','CF'),('Warning','CF'),('Coil','CF'),('Cold','CF'),('Hot','CF'),('ServicePaint','CH'),('ArmorShell','CH'),('Scorched','CH'),('PressPaint','CS'),('FabricationDeck','CS')]:m.MATS[key]=bpy.data.materials[prefix+'_'+key]
models=['RepairHub','RelayYard','ShieldDump','HaulerGraveyard','MagnetarComplex','CustodianCore']
# Isolate each installation in the dependency graph during editing. Thousands
# of unrelated source parts need not be recomputed for every machining operation.
collections={}
for name in models+['ChapterServiceNetwork']:
    col=bpy.data.collections.get('Facility '+name) or bpy.data.collections.new('Facility '+name)
    if col.name not in bpy.context.scene.collection.children:bpy.context.scene.collection.children.link(col)
    collections[name]=col
for obj in list(bpy.data.objects):
    name=obj.get('installation')
    if name not in collections:continue
    col=collections[name]
    if obj.name not in col.objects:col.objects.link(obj)
    for prior in list(obj.users_collection):
        if prior!=col:prior.objects.unlink(obj)
def activate(name=None):
    for key,col in collections.items():bpy.context.view_layer.layer_collection.children[col.name].exclude=name is not None and key!=name
def group(name):m.GROUP=name
def axle(name,p,r,length,axis='X',mat='Steel'):
    obj=cyl(name,p,r,length,mat,32)
    if axis=='X':obj.rotation_euler.y=math.pi/2
    if axis=='Y':obj.rotation_euler.x=math.pi/2
    return obj
def brace(name,a,b,w=.16,mat='Steel'):
    from mathutils import Vector
    aa,bb=Vector(a),Vector(b);obj=box(name,(aa+bb)/2,(w,w,(bb-aa).length),mat,.022);obj.rotation_euler=(bb-aa).to_track_quat('Z','Y').to_euler()
def panel(x,y,z,w,h,mat='ArmorShell',energy=None):
    box('recessed bolted access frame',(x,y,z),(w+.18,.12,h+.18),'Graphite',.025)
    box('removable protective access panel',(x,y-.075,z),(w,.08,h),mat,.025)
    for s in [-1,1]:
        for t in [-1,1]:axle('access corner captive stud',(x+s*(w/2-.08),y-.13,z+t*(h/2-.08)),.05,.045,'Y')
    if energy:box('equipment status aperture',(x,y-.124,z-h*.25),(w*.56,.03,.08),energy,.007)
def vent(x,y,z,w,h):
    box('deep ventilation opening',(x,y,z),(w,.11,h),'Graphite',.02)
    for i in range(max(3,int(h/.16))):box('formed ventilation louvre',(x,y-.08,z-h/2+.08+i*.16),(w-.12,.18,.045),'Steel',.009)
def floor(w,d,mat):
    group('refined_floor')
    for s in [-1,1]:
        # Physically open service channels, split bearing plates and removable lids.
        prism('central process bearing plate',[(s*.045,-d/2+.3),(s*3.18,-d/2+.3),(s*3.18,d/2-.3),(s*.045,d/2-.3)],-.27,-.015,mat,.02)
        prism('peripheral service plate',[(s*4.43,-d/2+.3),(s*(w/2-.3),-d/2+.3),(s*(w/2-.3),d/2-.3),(s*4.43,d/2-.3)],-.27,-.015,mat,.02)
        box('actual longitudinal utility trench',(s*3.8,-2.1,-.8),(1.18,d-5,.14),'Graphite',.018)
        tube('protected process trunk',[(s*3.8,-d/2+.55,-.51),(s*3.8,d/2-4.7,-.51)],.15,'Coil')
        grate(s*3.8,-2.1,1.10,d-5)
        for y in [-d/2+1.4,-2.4,1.5]:
            box('removable trench maintenance crossing',(s*3.8,y,-.035),(1.3,.85,.07),mat,.025)
            box('flush equipment access cassette',(s*2.35,y,.004),(1.1,1.5,.045),'Graphite',.025)
            box('service cassette raised edge',(s*2.35,y,.030),(.93,1.33,.014),'Steel',.015)
            box('service cassette centre panel',(s*2.35,y,.042),(.78,1.15,.012),mat,.008)
            for t in [-1,1]:box('cassette recessed handle',(s*2.35+t*.34,y,.055),(.06,.35,.018),'Graphite',.005)
        for y in [-d/2+3,-1.3,2.9]:
            box('transverse bearing plate joint',(s*1.6,y,.005),(3.0,.045,.015),'Graphite',.005)
            for x in [.45,2.85]:cyl('flush deck locating bolt',(s*x,y,.025),.045,.03,'Steel',8)
    # Clear thresholds link the different process surfaces and maintain Y=0 ground.
    for y in [-d/2+.7,d/2-.7]:box('process transition load sill',(0,y,-.025),(8.8,.65,.05),'Steel',.02)

for model in models:
    activate(model)
    print('REFINING',model,flush=True)
    original=[o for o in bpy.data.objects if o.type=='MESH' and o.get('installation')==model]
    # Safe reruns replace only the additions from this pass.
    for o in original:
        if o.get('camera_refinement'):bpy.data.objects.remove(o,do_unlink=True)
    original=[o for o in bpy.data.objects if o.type=='MESH' and o.get('installation')==model]
    for o in original:
        if 'process alignment plate' in o.name:bpy.data.objects.remove(o,do_unlink=True)
    original=[o for o in bpy.data.objects if o.type=='MESH' and o.get('installation')==model]
    start=len(m.PARTS)
    dims={'RepairHub':(22,20,'Deck'),'RelayYard':(24,18,'Deck'),'ShieldDump':(20,20,'FabricationDeck'),'HaulerGraveyard':(20,20,'FabricationDeck'),'MagnetarComplex':(50,20,'FabricationDeck'),'CustodianCore':(50,26,'Deck')}
    floor(*dims[model])
    if model=='RepairHub':
        group('diagnostic_mechanics')
        for s in [-1,1]:
            for y,h in [(-5.6,1.4),(-1,3.2),(4.8,4.7)]:
                panel(s*7.5,y-1.9,h*.51,2.1,max(.7,h*.52),'ArmorShell','Cold')
                vent(s*7.5,y-1.97,h*.80,1.3,max(.3,h*.18))
                for z in [.45,h-.1]:box('maintenance shell reinforcement',(s*7.5,y-1.94,z),(2.65,.16,.14),'Steel',.025)
            for z in [1.1,2.2,3.3]:panel(s*4.8,5.28,z,1.48,.66,'ServicePaint','Cold')
            tube('diagnostic diagnostic power loom',[(s*4.8,6.4,.4),(s*4.8,6.4,4.4),(s*6.0,6.4,4.4),(s*7.5,5.8,3.5)],.085,'Coil')
            # A repair head with differentiated clamps, not a single cylinder arm.
            for t in [-1,1]:box('repair manipulator tool finger',(s*2.9+t*.18,.79,2.12),(.10,.62,.16),'Steel',.014)
            axle('repair actuator exposed piston',(s*4.25,.73,2.25),.10,1.1,'X')
        for a in range(0,360,30):
            arc('service turntable cable collection rim',(0,0,0),3.78,.15,-.2,-.055,a+2,26,'Steel')
            r=math.radians(a);box('annular plinth removable radial latch',(3.3*math.cos(r),3.3*math.sin(r),.045),(.18,.30,.05),'Graphite',.01,yaw=a)
    elif model=='RelayYard':
        group('dock_mechanics')
        for s in [-1,1]:
            panel(s*4.8,4.77,2.2,.60,2.1,'ArmorShell','Hot')
            box('dock column channel extrusion',(s*4.8,5.05,2.2),(.22,.18,2.5),'Steel',.025)
            for y in [-5,-1,3]:
                vent(-7,y-1.00,.75,1.7,.42)
                axle('assembly robot shoulder',(-7,y,1.95),.18,.5,'Y')
                brace('assembly tool upright',(-7,y,1.3),(-7.0,y,2.4),.22,'ServicePaint')
                brace('assembly tool outreach',(-7,y,2.4),(-6.2,y,2.4),.18)
                box('assembly gripper head',(-6.2,y,2.22),(.35,.36,.25),'Graphite',.035)
            for z in [1.2,2.3,3.2]:panel(s*7.7,3.19,z,1.15,.65,'ArmorShell','Hot')
        for x in [-3.6,-1.8,0,1.8,3.6]:vent(x,6.48,4.76,.8,.28)
        tube('deployment charging distribution',[(-4.8,5.8,3.5),(0,5.8,3.5),(4.8,5.8,3.5)],.065,'Coil')
        for y in [4.0,5.1,6.2]:
            for x in [-1.5,-.5,.5,1.5]:box('dock conveyor roller',(x,y,.13),(.55,.15,.20),'Steel',.024)
    elif model=='ShieldDump':
        group('armor_processing_details')
        # Move foreground diagnostics away from the production-camera entry ray.
        for o in [o for o in original if o.name.startswith('sort_controls /')]:
            ys=[(o.matrix_world@v.co).y for v in o.data.vertices];mid=(min(ys)+max(ys))/2
            if -4.2<mid<-2.6:o.location.x-=1.3;o.location.y-=4.5
        for s in [-1,1]:
            for z in [1.2,2.9,4.1]:panel(s*4.25,4.55,z,1.2,.65,'ArmorShell','Cold' if z==2.9 else None)
            for yy in [5.1,7.5]:
                tube('sorting lift exposed actuator',[(s*4.25,yy,1.1),(s*4.25,yy,4.2)],.16,'Graphite')
                tube('sorting lift polished piston',[(s*4.25,yy,2.8),(s*4.25,yy,4.65)],.075)
        for y in [-5.4,-1.6,2.2,6.4]:
            panel(-7.3,y-1.80,1.0,2.2,.48,'Scorched')
            for x in [-8.1,-6.5]:tube('armor cassette moving chain',[(x,y-1.5,.7),(x,y-1.5,3.6)],.075,'Coil')
            for z in [1.25,2.2,3.0]:box('stored shield exposed laminated edge',(-7.3,y-1.05,z),(2.05,.1,.10),'Steel',.012)
        panel(-.8,6.13,5.5,1.6,.43,'PressPaint','Hot')
        vent(0,5.43,1.8,1.8,.65)
        for x in [-1.15,1.15]:
            box('processed shield flanged edge',(x,5.94,2.1),(.16,.15,1.6),'Steel',.02)
            panel(x,5.90,2.1,.6,.6,'ArmorShell')
    elif model=='HaulerGraveyard':
        group('rupture_and_loading_details')
        for x,y,yaw in [(-6.9,3.8,-9),(1.0,7.1,80)]:
            wreck_start=len(m.PARTS)
            for sy in [-1.3,.1,1.5]:
                box('transport chassis exposed cross member',(x,y+sy,.95),(2.3,.16,.22),'Steel',.025,yaw)
            for z in [1.25,1.6,1.95]:box('transport load restraint rib',(x-1.48,y,z),(.13,3.6,.12),'Steel',.018,yaw)
            tube('wreck severed hydraulic line',[(x+1.15,y-.4,1.2),(x+1.8,y+.4,.8),(x+2.0,y+1.1,.1)],.09,'Coil')
            panel(x,y-2.90,1.02,1.8,.35,'Scorched','Hot')
            from mathutils import Matrix,Vector
            rotation=Matrix.Translation(Vector((x,y,0)))@Matrix.Rotation(math.radians(yaw),4,'Z')@Matrix.Translation(Vector((-x,-y,0)))
            for obj in m.PARTS[wreck_start:]:obj.matrix_world=rotation@obj.matrix_world;obj['wreck_alignment']=True
        for y in [-4.6,6.8]:
            panel(7.4,y-.85,3.3,.85,2.1,'Scorched','Hot')
            for z in [1.2,2.7,4.5]:box('loader guide collar',(7.4,y,z),(1.7,1.75,.20),'Steel',.03)
        tube('powered trolley travel supply',[(7.4,-5.3,5.4),(7.4,3.3,5.4),(7.4,3.3,6.8)],.095,'Coil')
        vent(7.4,1.99,6.3,1.65,.42)
        for i in range(6):box('loader floor hazard boundary',(5.7,-5+i*1.7,.035),(.14,.6,.035),'Warning',.007)
    elif model=='MagnetarComplex':
        group('induction_cooling_and_laminations')
        x,y=-5,5.9
        for a in range(0,360,45):
            angle=math.radians(a);xx=x+3.95*math.cos(angle);yy=y+3.95*math.sin(angle)
            bolts(xx,yy,.85,.30,6)
            arc('induction ring dielectric support',(x,y,0),3.56,.27,1.0,1.42,a+10,26,'Shell')
            for z in [1.24,1.50,1.76]:arc('coil visible winding turns',(x,y,0),3.16,.06,z,z+.06,a+4,37,'Steel')
        for s in [-1,1]:
            for z in [3.2,4.3,5.3]:panel(x+s*3.1,y-.73,z,.58,.6,'ArmorShell')
            tube('induction arch cooling jacket',[(x+s*3.7,y-.55,1),(x+s*3.7,y-.55,5.2),(x+s*2.2,y-.55,6.8)],.14,'Coil')
            tube('arch contained return lead',[(x+s*3.7,y-.5,1),(x+s*3.7,y-.5,2.2)],.045,'Hot')
        for xx in [-14,8]:
            for z in [1.1,2.6,4]:panel(xx,5.81,z,1.9,.70,'PressPaint','Hot')
            vent(xx,5.76,3.3,1.6,.44)
        group('refined_floor')
        for yy in [-5.5,-1.5,2.5]:
            box('elite apparatus armored access plate',(7,yy,.012),(6.6,2.1,.045),'ArmorShell',.035)
            for xx in [4.1,9.9]:box('elite deck plate captive locking shoe',(xx,yy,.044),(.18,1.4,.035),'Graphite',.01)
    elif model=='CustodianCore':
        group('containment_service_layers')
        for i in range(6):
            a=math.radians(i*60);x=3.75*math.cos(a);y=8.6+3.75*math.sin(a)
            for z in [1.2,3.0,5.1,6.9]:
                box('containment pier armored collar',(x,y,z),(1.5,1.65,.19),'Steel',.026)
            tube('containment cooling standpipe',[(x,y,.9),(x,y,6.9),(2.4*math.cos(a),8.6+2.4*math.sin(a),7.9)],.10,'Coil')
        for s in [-1,1]:
            for yy,h in [(-8,2.2),(-1.5,4.4),(5,6.4),(10,7.7)]:
                xx=s*(14 if yy==5 else 11 if yy<7 else 13)
                for z in [.9,h*.58,h-.4]:panel(xx,yy-1.44,z,1.5,.55,'ArmorShell','Hot' if z==h*.58 else None)
                vent(xx,yy-1.47,h*.78,1.25,max(.3,h*.14))
            for z in [1.0,2.35,3.6]:panel(s*7.7,6.69,z,1.48,.65,'ArmorShell','Cold')
            tube('reactor service pressure manifold',[(s*7.7,7.4,2.3),(s*6.0,7.4,2.3),(s*5.3,8.6,2.3),(s*4.4,8.6,2.3)],.18,'Steel')
        for a in range(0,360,30):
            angle=math.radians(a);bolts(4.28*math.cos(angle),8.6+4.28*math.sin(angle),.60,.16,4)
        group('refined_floor')
        for a in range(0,360,30):
            angle=math.radians(a)
            x,y=7.9*math.cos(angle),7.9*math.sin(angle)
            box('containment radial technical hatch',(x,y,.025),(.9,1.25,.05),'Graphite',.025,yaw=a)
    for obj in m.PARTS[start:]:
        obj['installation']=model;obj['camera_refinement']=True
        col=collections[model];col.objects.link(obj)
        for prior in list(obj.users_collection):
            if prior!=col:prior.objects.unlink(obj)
    print('REFINED',model,len(m.PARTS)-start,flush=True)
activate();bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'Chapter01Facilities.blend'))
stats=json.loads((PASS/'verification/geometry.json').read_text(encoding='utf-8'))
for name in models:
    activate(name)
    parts=[o for o in bpy.data.objects if o.type=='MESH' and o.get('installation')==name];groups={}
    for obj in parts:
        points=[obj.matrix_world@v.co for v in obj.data.vertices];mx=(min(v.x for v in points)+max(v.x for v in points))/2;my=(min(v.y for v in points)+max(v.y for v in points))/2
        groups.setdefault((obj.name.split(' / ')[0],math.floor(mx/8),math.floor(my/4)),[]).append(obj)
    merged=[]
    for (kind,x,y),items in groups.items():
        bpy.ops.object.select_all(action='DESELECT')
        for obj in items:obj.hide_set(False);obj.select_set(True)
        bpy.context.view_layer.objects.active=items[0]
        if len(items)>1:bpy.ops.object.join()
        obj=bpy.context.object;obj.name=f'CH_{name}_{kind}_{x}_{y}';bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
        bm=bmesh.new();bm.from_mesh(obj.data);bmesh.ops.triangulate(bm,faces=list(bm.faces));bm.to_mesh(obj.data);bm.free();merged.append(obj)
    bpy.ops.object.select_all(action='DESELECT')
    for obj in merged:obj.select_set(True)
    bpy.ops.export_scene.fbx(filepath=str(OUT/'Models'/f'{name}.fbx'),use_selection=True,object_types={'MESH'},apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',bake_anim=False,add_leaf_bones=False,path_mode='STRIP',mesh_smooth_type='FACE',use_tspace=True)
    stats=[s for s in stats if s['model']!=name]+[dict(model=name,renderers=len(merged),triangles=sum(len(o.data.polygons) for o in merged))]
    for obj in merged:obj.hide_set(True)
(PASS/'verification/geometry.json').write_text(json.dumps(stats,indent=2),encoding='utf-8')
print('CAMERA_REFINEMENT_EXPORTED',json.dumps(stats))
