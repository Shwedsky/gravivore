"""Editable original hard-surface machinery, authored in Blender.

Custom pressure profiles, chamfered plate extrusions, swept cages and cable
paths. Parts remain separate in .blend; exported atlas mesh has one material.
"""
import bpy, math, json, sys
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/_Game/Content/SurfaceHeroV46/Models';OUT.mkdir(parents=True,exist_ok=True)
SRC=ROOT/'art/surface-hero-v46';SRC.mkdir(parents=True,exist_ok=True)
DOC=ROOT/'docs/history/implementation-passes/chapter01-visual-replacement-v3/v46/verification';DOC.mkdir(parents=True,exist_ok=True)
PALETTE=[(32,43,49),(99,122,136),(154,169,173),(64,86,103),(143,104,48),(172,32,12),(195,103,20),(20,141,166),(17,24,28),(85,65,46),(115,139,148),(132,148,159),(46,95,109),(159,113,44),(121,139,144),(23,33,39)]
metrics={};parts=[];mats=[]
def reset():
    global parts,mats
    bpy.ops.wm.read_factory_settings(use_empty=True);parts=[];mats=[]
    for i,c in enumerate(PALETTE):
        m=bpy.data.materials.new('SurfaceTile_%02d'%i);m.diffuse_color=(*(x/255 for x in c),1);mats.append(m)
    bpy.context.scene.unit_settings.system='METRIC'
def active(o):
    bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o
def mesh(name,verts,faces,tile,bevel=0,smooth=False):
    data=bpy.data.meshes.new(name);data.from_pydata(verts,[],faces);data.update()
    o=bpy.data.objects.new(name,data);bpy.context.collection.objects.link(o)
    for m in mats:data.materials.append(m)
    for f in data.polygons:f.material_index=tile;f.use_smooth=smooth
    if bevel:
        active(o);b=o.modifiers.new('Manufactured edge chamfer','BEVEL');b.width=bevel;b.segments=2;b.affect='EDGES';b.material=2
        bpy.ops.object.modifier_apply(modifier=b.name)
        n=o.modifiers.new('Weighted panel normals','WEIGHTED_NORMAL');n.keep_sharp=True;n.weight=40;bpy.ops.object.modifier_apply(modifier=n.name)
    parts.append(o);return o
def plate(name,p,size,tile=1,ch=.06,bevel=.015):
    x,y,z=p;a,b,c=(s/2 for s in size);d=min(ch,a*.3,b*.3)
    # Eight-point chamfered plan; perimeter catches light from every camera.
    ring=[(-a+d,-b), (a-d,-b),(a,-b+d),(a,b-d),(a-d,b),(-a+d,b),(-a,b-d),(-a,-b+d)]
    verts=[(x+u,y+v,z+w) for w in (-c,c) for u,v in ring]
    faces=[tuple(reversed(range(8))),tuple(range(8,16))]+[(i,(i+1)%8,(i+1)%8+8,i+8) for i in range(8)]
    return mesh(name,verts,faces,tile,bevel)
def lathe(name,profile,p=(0,0,0),tile=1,n=32):
    x,y,z=p;verts=[(x+r*math.cos(i*2*math.pi/n),y+r*math.sin(i*2*math.pi/n),z+h) for r,h in profile for i in range(n)]
    faces=[(j*n+i,j*n+(i+1)%n,(j+1)*n+(i+1)%n,(j+1)*n+i) for j in range(len(profile)-1) for i in range(n)]
    o=mesh(name,verts,faces,tile,smooth=True)
    uv=o.data.uv_layers.new(name='Atlas')
    for j,f in enumerate(o.data.polygons):
        level=j//n;i=j%n
        coords=[(i/n,level/(len(profile)-1)),((i+1)/n,level/(len(profile)-1)),((i+1)/n,(level+1)/(len(profile)-1)),(i/n,(level+1)/(len(profile)-1))]
        for li,co in zip(f.loop_indices,coords):uv.data[li].uv=co
    o['cylindrical_uv']=True;return o
def tube(name,path,r=.04,tile=8,n=8):
    pts=[Vector(p) for p in path];verts=[]
    for i,p in enumerate(pts):
        tangent=(pts[min(i+1,len(pts)-1)]-pts[max(0,i-1)]).normalized()
        axis=Vector((0,0,1)) if abs(tangent.z)<.9 else Vector((0,1,0))
        a=tangent.cross(axis).normalized();b=tangent.cross(a).normalized()
        verts.extend(p+r*(a*math.cos(j*2*math.pi/n)+b*math.sin(j*2*math.pi/n)) for j in range(n))
    faces=[(i*n+j,i*n+(j+1)%n,(i+1)*n+(j+1)%n,(i+1)*n+j) for i in range(len(pts)-1) for j in range(n)]
    faces+=[tuple(reversed(range(n))),tuple(range((len(pts)-1)*n,len(pts)*n))]
    return mesh(name,verts,faces,tile,smooth=True)
def arc(name,radius,zcenter,start,end,width=.18,depth=.3,tile=1,y=.15):
    verts=[];segments=40
    for i in range(segments+1):
        a=math.radians(start+(end-start)*i/segments)
        for rr,yy in [(radius-width/2,y-depth/2),(radius+width/2,y-depth/2),(radius+width/2,y+depth/2),(radius-width/2,y+depth/2)]:
            verts.append((rr*math.cos(a),yy,zcenter+rr*math.sin(a)))
    faces=[(i*4+j,i*4+(j+1)%4,(i+1)*4+(j+1)%4,(i+1)*4+j) for i in range(segments) for j in range(4)]
    faces+=[(3,2,1,0),tuple(range(segments*4,segments*4+4))]
    return mesh(name,verts,faces,tile,.012)
def bolt(p,tile=2,r=.032):lathe('Captive structural fastener',[(0,-.014),(r,-.014),(r,.012),(r*.8,.020),(0,.020)],p,tile,n=6)
def panel(p,size,tile=3):
    plate('Sealed service recess',p,size,15,.08,.012)
    q=Vector(p)+Vector((0,-size[1]/2-.014,0))
    plate('Inset removable service plate',q,(size[0]*.85,.028,size[2]*.78),tile,.08,.012)
    for s in (-1,1):
        for t in (-1,1):bolt((p[0]+s*size[0]*.32,p[1],p[2]+t*size[2]*.32))
def pressure():
    # Closed hemispherical shoulder profile with pressure shell, not stacked cups.
    lathe('Welded sealed capsule pressure shell',[(0,.28),(.36,.28),(.53,.36),(.64,.54),(.68,.78),(.68,2.03),(.64,2.27),(.53,2.44),(.34,2.53),(0,2.55)],tile=1)
    for z in (.72,1.92):lathe('Forged clamping collar',[(.666,z-.065),(.705,z-.065),(.727,z-.035),(.727,z+.035),(.705,z+.065),(.666,z+.065)],tile=10)
    lathe('Bolted inspection crown',[(0,2.54),(.27,2.54),(.3,2.57),(.3,2.64),(.27,2.67),(0,2.67)],tile=3)
    for i in range(8):
        a=i*math.pi/4;bolt((.23*math.cos(a),.23*math.sin(a),2.675))
    for s in (-1,1):
        plate('Pressure frame foot',(s*.57,0,.11),(.47,1.18,.18),0,.09,.025)
        plate('Attached saddle leg',(s*.61,0,.37),(.16,.82,.42),10,.035,.016)
        plate('Vertical support bracket',(s*.72,.34,1.25),(.12,.25,1.5),3,.04,.012)
    panel((0,-.667,1.24),(.59,.16,.64))
    plate('Pressure status display',(0,-.763,1.36),(.18,.016,.12),7,.02,.006)
    tube('Welded outlet elbow',[(.44,0,2.48),(.78,0,2.52),(.92,0,2.42),(.92,0,1.88)],.09,10,12)
    lathe('Flanged pipe coupling',[(.06,-.05),(.15,-.05),(.15,.05),(.06,.05)],(.92,0,1.85),2,16)
    tube('Return service pipe',[(-.48,.37,.49),(-.80,.37,.48),(-.85,.37,.65),(-.85,.37,1.62)],.07,9,12)
    plate('Valve actuator',(-.85,.37,1.7),(.24,.28,.14),4,.04,.015)
    for i in range(8):
        a=i*math.pi/4;bolt((.704*math.cos(a),.704*math.sin(a),1.98))
def energy():
    lathe('Armored lower coolant manifold',[(0,.12),(.53,.12),(.76,.22),(.76,.44),(.62,.60),(0,.60)],tile=0)
    lathe('Upper mechanical containment housing',[(0,2.64),(.58,2.64),(.75,2.78),(.75,3.03),(.58,3.16),(0,3.16)],tile=3)
    lathe('Contained luminous energy volume',[(0,.58),(.28,.58),(.38,.77),(.43,1.05),(.43,2.05),(.37,2.43),(.25,2.63),(0,2.63)],tile=6, n=40)
    for i in range(6):
        a=i*math.pi/3;ca,sa=math.cos(a),math.sin(a)
        tube('Bowed containment cage rail',[(ca*.61,sa*.61,.43),(ca*.79,sa*.79,.75),(ca*.82,sa*.82,2.36),(ca*.62,sa*.62,2.80)],.066,10,10)
        plate('Cage foot attachment',(ca*.64,sa*.64,.41),(.22,.22,.25),1,.04,.015)
        bolt((ca*.64,sa*.64,3.065))
    for z in (.62,2.58):lathe('Recessed core isolator',[(.40,z-.06),(.61,z-.06),(.65,z),(.61,z+.06),(.40,z+.06)],tile=2)
    for s in (-1,1):
        panel((s*.61,.08,.78),(.31,.35,.60),4)
        tube('Coolant return braid',[(s*.5,.5,.43),(s*.7,.55,.74),(s*.67,.52,2.45),(s*.4,.45,2.8)],.05,8,8)
    plate('Energy diagnostic head',(0,0,3.21),(.54,.45,.15),1,.06,.018)
    plate('Localized status lens',(0,-.24,3.21),(.19,.018,.06),6,.015,.004)
def arcmachine():
    plate('Articulated focus-machine plinth',(0,.2,.14),(2.85,1.6,.28),0,.18,.026)
    arc('Forged partial induction ring',1.22,1.24,-22,202,.27,.39,1)
    arc('Recessed inner winding',1.01,1.24,-16,196,.10,.26,9,y=.13)
    for s in (-1,1):
        plate('Articulated support shoe',(s*1.06,.15,.38),(.51,.89,.43),3,.1,.022)
        plate('Attached ring bracket',(s*1.13,.15,.70),(.24,.61,.53),10,.05,.014)
    lathe('Focus rotor lower housing',[(0,.37),(.43,.37),(.55,.48),(.55,.64),(.4,.76),(0,.76)],(0,.12,0),0)
    lathe('Recessed focus volume',[(0,.72),(.27,.72),(.34,.89),(.28,1.08),(0,1.14)],(0,.12,0),6)
    for i in range(9):
        a=math.radians(4+i*21);x=1.25*math.cos(a);z=1.24+1.25*math.sin(a)
        plate('Segmented ring service cassette',(x,.15,z),(.22,.48,.15),3 if i%2 else 10,.035,.009)
    panel((.81,-.3,.83),(.55,.35,.77),4)
    tube('Asymmetric braided power feed',[(-.8,.37,.41),(-.93,.5,.8),(-.73,.49,1.58),(-.35,.39,1.2)],.055,8)
    tube('Copper bus return',[(.3,.57,.43),(.67,.61,.76),(.87,.57,1.07)],.046,9)
def gate():
    # Layered industrial wall with chamfered center and actual separated ribs.
    plate('Structural gate sill',(0,0,.15),(3.1,.8,.3),0,.14,.025)
    plate('Deep recessed wall cavity',(0,.18,1.42),(2.6,.20,2.36),15,.12,.022)
    for s in (-1,1):
        plate('Load bearing layered jamb',(s*1.26,.03,1.38),(.45,.72,2.45),1,.1,.026)
        plate('Jamb warning inset',(s*1.26,-.342,1.56),(.17,.018,.59),13,.028,.005)
        for z in (.57,1.21,1.93):plate('Gate support rib',(s*1.24,-.39,z),(.52,.15,.13),10,.035,.012)
    plate('Chamfered overhead frame',(0,.02,2.65),(3,.75,.36),1,.13,.027)
    for s in (-1,1):plate('Inset segmented shield panel',(s*.48,-.008,1.47),(.88,.18,2.03),3 if s<0 else 11,.11,.024)
    panel((.73,-.44,.86),(.39,.16,.47),4)
    plate('Gate narrow diagnostic source',(-.4,-.38,2.62),(.32,.025,.07),6,.015,.004)
def dock(heavy=False,special=False):
    height=2.5 if heavy else 2.1
    plate('Framed open floor cradle',(0,-.38,.10),(2.8,2.48,.20),0,.17,.023)
    plate('Recessed robot parking bed',(0,-.45,.185),(1.65,1.99,.045),15,.12,.009)
    for s in (-1,1):
        plate('Walk-in floor guide',(s*.79,-.66,.24),(.14,1.4,.12),10,.032,.012)
        plate('Floor stop coupling',(s*.55,.39,.28),(.34,.24,.21),3,.05,.015)
    plate('Armored service backplate',(0,.77,height*.50+.18),(2.76,.38,height),0,.13,.023)
    for s in (-1,1):
        plate('Separated backplate panel',(s*.53,.553,height*.56),(.90,.075,height*.65),11 if s<0 else 3,.09,.018)
        plate('Dock charger side support',(s*1.12,-.04,height*.47),(.49,1.01,height*.85),1,.10,.025)
        panel((s*1.12,-.58,height*.47),(.39,.11,height*.56),4 if heavy else 3)
        plate('Contact induction pad',(s*.864,-.2,height*.47),(.045,.47,.45),6 if special else 5,.025,.008)
        tube('Terminated charger power conduit',[(s*.65,.90,.26),(s*1.1,.87,.35),(s*1.18,.53,.56),(s*1.18,.24,height*.73)],.055,8)
        for z in (.37,height*.75):plate('Support cooling shroud',(s*1.12,-.015,z),(.56,1.08,.13),10,.055,.012)
    plate('Service power feed spine',(0,.81,height*.57),(.29,.34,height*.85),10,.05,.016)
    plate('Recessed central diagnostic',(0,.547,height*.59),(.21,.025,.42),6 if special else 5,.025,.006)
    panel((0,.528,.50),(.67,.1,.47),4)
    for i in range(5):plate('Backplate recessed vent fin',(-.5+i*.25,.54,height*.91),(.10,.025,.18),15,.012,.004)
    if heavy:
        plate('Heavy lifting cross-head',(0,.56,height+.18),(2.9,.80,.26),10,.10,.025)
        for s in (-1,1):tube('Attached service lift arm',[(s*.9,.17,height*.85),(s*.81,-.10,height*.93),(s*.72,-.55,height*.80)],.075,1,10)
    if special:
        lathe('Industrial coolant header',[(0,0),(.24,0),(.3,.13),(.3,.37),(.24,.48),(0,.48)],(.81,.87,height-.21),9,20)
    # Front edge remains physically and visually open to the patrol apron.
    for s in (-1,1):plate('Short cradle warning plate',(s*1.04,-1.55,.22),(.39,.14,.018),13,.025,.004)
def apron():
    for s in (-1,1):
        for y in (-1.5,1.5):
            plate('Flush framed robot berth',(s*1.5,y,.08),(1.25,1.2,.10),0,.08,.008)
            plate('Recessed berth bed',(s*1.5,y,.135),(1.03,.98,.014),15,.05,.004)
            for t in (-1,1):plate('Berth locating shoe',(s*1.5+t*.48,y,.15),(.10,.92,.04),10,.015,.006)
            plate('Localized charge indicator',(s*1.5,y+.48,.151),(.14,.08,.016),5,.01,.003)
            tube('Berth power feed',[(s*1.5,y+.6,.11),(s*2.3,y+.85,.11),(s*2.3,3.35,.11),(s*.45,3.7,.11)],.037,8)
def gantry():
    for s in (-1,1):
        plate('Gantry planted shoe',(s*3.4,0,.25),(.64,.69,.49),0,.06,.023)
        plate('Chamfered structural upright',(s*3.4,0,1.97),(.29,.44,3.44),1,.04,.018)
        for z in (.56,1.57,2.65):plate('Attached upright service collar',(s*3.4,0,z),(.38,.53,.12),10,.04,.012)
    plate('Layered suspended girder',(0,0,3.7),(7.37,.88,.34),1,.09,.022)
    plate('Recessed cable tray',(0,0,3.96),(6.78,.58,.15),0,.07,.015)
    for x in (-2.6,-1.3,0,1.3,2.6):plate('Girder captive service rib',(x,-.03,3.72),(.10,.94,.48),10,.02,.009)
    plate('Recessed inspection emitter',(0,-.457,3.67),(.37,.019,.052),6,.008,.004)
def finish(name):
    # Assign tile-aware UVs before joining; no shader or geometry created at runtime.
    for o in parts:
        uv=o.data.uv_layers.active or o.data.uv_layers.new(name='Atlas')
        coords=[v.co for v in o.data.vertices]
        lo=[min(v[i] for v in coords) for i in range(3)];hi=[max(v[i] for v in coords) for i in range(3)]
        for f in o.data.polygons:
            mi=f.material_index
            for li in f.loop_indices:
                if not o.get('cylindrical_uv'):
                    normal=f.normal;axis=max(range(3),key=lambda k:abs(normal[k]));axes=[k for k in range(3) if k!=axis]
                    v=o.data.vertices[o.data.loops[li].vertex_index].co
                    u=(v[axes[0]]-lo[axes[0]])/max(.001,hi[axes[0]]-lo[axes[0]]);w=(v[axes[1]]-lo[axes[1]])/max(.001,hi[axes[1]]-lo[axes[1]])
                else:u,w=uv.data[li].uv
                uv.data[li].uv=((mi%4+.012+.976*u)/4,(mi//4+.012+.976*w)/4)
    # Materials in the editable source use the same maps as the final Unity mesh.
    atlas=bpy.data.materials.new('V46_SharedProductionAtlas');atlas.use_nodes=True
    nodes=atlas.node_tree.nodes;links=atlas.node_tree.links;bs=nodes.get('Principled BSDF')
    for kind,socket in [('BaseColor','Base Color'),('Emission','Emission Color')]:
        tex=nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(OUT.parent/'Textures'/('V46_'+kind+'.png')));links.new(tex.outputs['Color'],bs.inputs[socket])
    tex=nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(OUT.parent/'Textures/V46_Normal.png'));tex.image.colorspace_settings.name='Non-Color'
    norm=nodes.new('ShaderNodeNormalMap');links.new(tex.outputs['Color'],norm.inputs['Color']);links.new(norm.outputs['Normal'],bs.inputs['Normal'])
    tex=nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(OUT.parent/'Textures/V46_MetallicSmoothness.png'));tex.image.colorspace_settings.name='Non-Color'
    sep=nodes.new('ShaderNodeSeparateColor');links.new(tex.outputs['Color'],sep.inputs['Color']);links.new(sep.outputs['Red'],bs.inputs['Metallic'])
    inv=nodes.new('ShaderNodeMath');inv.operation='SUBTRACT';inv.inputs[0].default_value=1;links.new(tex.outputs['Alpha'],inv.inputs[1]);links.new(inv.outputs[0],bs.inputs['Roughness']);bs.inputs['Emission Strength'].default_value=2
    for o in parts:
        for f in o.data.polygons:f.material_index=0
        o.data.materials.clear();o.data.materials.append(atlas)
    for im in bpy.data.images:
        if im.source=='FILE':im.filepath=bpy.path.relpath(im.filepath,start=str(SRC))
    bpy.ops.wm.save_as_mainfile(filepath=str(SRC/(name+'.blend')))
    bpy.ops.object.select_all(action='DESELECT')
    for o in parts:o.select_set(True)
    bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name='V46_'+name
    active(o);tri=o.modifiers.new('Mobile triangles','TRIANGULATE');bpy.ops.object.modifier_apply(modifier=tri.name)
    metrics[name]={'triangles':len(o.data.polygons),'materials':1,'source':'Original editable Blender hard-surface authoring','blend':'art/surface-hero-v46/'+name+'.blend','parts':len(parts),'primitiveOnly':False,'size':[round(s,4) for s in o.dimensions]}
    bpy.ops.export_scene.fbx(filepath=str(OUT/('V46_'+name+'.fbx')),use_selection=True,object_types={'MESH'},apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',bake_anim=False,path_mode='STRIP',mesh_smooth_type='FACE',use_tspace=True)
if '--validate-sources' in sys.argv:
    evidence=[]
    for name,e in json.loads((DOC/'machinery_authoring.json').read_text()).items():
        bpy.ops.wm.open_mainfile(filepath=str(ROOT/e['blend']))
        meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
        assert len(meshes)==e['parts'] and len(meshes)>10
        assert all(o.data.uv_layers.active is not None for o in meshes)
        assert all(im.filepath.startswith('//') and Path(bpy.path.abspath(im.filepath)).exists() for im in bpy.data.images if im.source=='FILE')
        evidence.append({'family':name,'editableMeshParts':len(meshes),'relativeTexturePathsResolve':True,'uvsRetained':True})
    (DOC/'blender_source_validation.json').write_text(json.dumps({'validated':True,'blender':bpy.app.version_string,'sources':evidence},indent=2))
    print('V46_BLENDER_EDITABLE_SOURCES_REOPEN_PASS');sys.exit(0)
for name,build in [('PressureVessel',pressure),('EnergyCylinder',energy),('ArcMachine',arcmachine),('GateModule',gate),('LightDock',lambda:dock()),('HeavyDock',lambda:dock(True)),('SpecialDock',lambda:dock(True,True)),('DockApron',apron),('ServiceGantry',gantry)]:
    reset();build();finish(name)
(DOC/'machinery_authoring.json').write_text(json.dumps(metrics,indent=2))
print('V46_EDITABLE_MACHINERY_READY')
