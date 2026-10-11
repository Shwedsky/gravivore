"""Authored open deployment structures and recessed industrial deck panels.

Grounded masses align with the accepted rear-console and rack proxies. The
overhead service bridge remains behind all four gameplay spawn/attack anchors.
"""
import bpy, math, json, random
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
helper=(ROOT/'Tools/surface-hero-v46/build_machinery.py').read_text()
exec(helper[helper.index('PALETTE='):helper.index('def finish(')])
OUT=ROOT/'Assets/_Game/Content/ConceptConvergenceV47/Models';OUT.mkdir(parents=True,exist_ok=True)
SRC=ROOT/'art/concept-convergence-v47';SRC.mkdir(parents=True,exist_ok=True)
DOC=ROOT/'docs/history/implementation-passes/chapter01-visual-replacement-v3/v47'
metrics={}
def structure(kind):
    # Separate load-bearing jambs occupy the existing +/-3.5m utility-rack feet.
    for s in (-1,1):
        plate('Deep planted structural shoe',(s*3.5,.1,.13),(.59,.66,.26),0,.08,.018)
        plate('Recessed service upright',(s*3.5,.1,2.12),(.44,.57,4.05),0,.075,.018)
        plate('Independent armor spine',(s*3.5,-.21,2.2),(.36,.13,3.35),1,.06,.018)
        for z in (.61,2.0,3.46):plate('Jamb load collar',(s*3.5,.1,z),(.59,.65,.16),10,.05,.012)
        plate('Localized maintenance indicator',(s*3.5,-.287,2.45),(.09,.02,.34),5,.02,.005)
        tube('Attached upright coolant return',[(s*3.67,.2,.32),(s*3.67,.2,3.89),(s*3.3,.2,4.2)],.042,9,10)
    plate('Manufactured overhead machine bridge',(0,.13,4.23),(7.48,.73,.45),1,.14,.025)
    plate('Inset overhead cable raceway',(0,.35,4.53),(6.83,.31,.19),0,.055,.012)
    for x in (-2.85,-1.7,0,1.7,2.85):plate('Captive overhead cassette',(x,-.26,4.23),(.23,.08,.32),10,.035,.009)
    # Rear center occupies the exact accepted 1.7 x 1.3m console footprint.
    plate('Rear machine foundation',(0,0,.16),(1.62,1.20,.32),0,.12,.021)
    plate('Deep central machinery recess',(0,.21,1.96),(1.35,.70,3.59),15,.12,.018)
    for s in (-1,1):
        plate('Central armored side cheek',(s*.66,.0,1.79),(.22,1.10,3.12),1,.06,.018)
        tube('Connected rear power bus',[(s*.49,.5,.38),(s*.49,.5,3.50),(s*2.3,.4,3.92),(s*3.5,.28,3.85)],.068,10,12)
    if kind=='FabricationBay':
        lathe('Vertical fabrication pressure chamber',[(0,.38),(.35,.38),(.52,.53),(.53,2.94),(.44,3.17),(0,3.29)],(0,.03,0),10,32)
        for z in (.68,2.73):lathe('Fabricator clamping flange',[(.5,z-.06),(.59,z-.05),(.60,z+.05),(.51,z+.07)],tile=1,n=24)
        for s in (-1,1):
            tube('Attached fabrication arm',[(s*.68,-.20,3.27),(s*1.7,-.42,3.42),(s*2.24,-.90,3.13)],.12,0,12)
            plate('Manufactured fabrication tool head',(s*2.24,-.89,3.0),(.39,.48,.49),1,.07,.019)
            plate('Contained welding source',(s*2.24,-1.14,2.98),(.10,.023,.18),5,.015,.005)
        plate('Sealed fabrication control head',(0,-.55,1.75),(.63,.17,.82),3,.09,.016)
    elif kind=='InductionStation':
        lathe('Induction transformer lower housing',[(0,.42),(.48,.42),(.56,.57),(.56,1.08),(.39,1.25),(0,1.25)],tile=0,n=28)
        lathe('Contained induction winding',[(0,1.18),(.30,1.18),(.37,1.32),(.37,2.68),(.26,2.85),(0,2.85)],tile=5,n=28)
        for i in range(4):
            a=i*math.tau/4+.4;x,y=.49*math.cos(a),.49*math.sin(a)
            tube('Visible induction containment cage',[(x,y,1.02),(x*1.25,y*1.25,1.32),(x*1.25,y*1.25,2.69),(x,y,3.04)],.065,1,10)
        lathe('Induction upper containment cap',[(0,2.98),(.43,2.98),(.58,3.1),(.54,3.39),(0,3.48)],tile=1,n=28)
        for s in (-1,1):
            plate('Suspended electrode carriage',(s*1.66,.03,3.82),(.73,.59,.36),0,.08,.02)
            tube('Industrial electrode',[(s*1.66,-.02,3.7),(s*1.66,-.43,3.17)],.09,10,12)
    else:
        for z in (.75,1.42,2.09,2.76):
            plate('Angled deployment magazine',(0,-.08,z),(1.13,.93,.39),3,.1,.022)
            plate('Recessed maintenance slot',(0,-.55,z),(.66,.034,.11),15,.03,.006)
        plate('Recon sensor crown',(0,-.05,3.33),(1.15,.87,.44),1,.10,.02)
        plate('Recon optical strip',(0,-.499,3.35),(.37,.023,.09),5,.02,.004)
        tube('Articulated sensor bridge',[(0,.15,3.50),(0,.15,3.85),(0,-.42,4.05)],.11,0,12)
    for s in (-1,1):
        plate('Rear endpoint service manifold',(s*2.88,.20,.24),(.46,.45,.23),0,.04,.012)
        tube('Terminated facility deck supply',[(s*.62,.3,.22),(s*2.9,.3,.22),(s*3.48,.25,.28)],.055,8,10)

def deck(seed):
    rng=random.Random(seed)
    plate('Opaque recessed industrial subdeck',(0,0,-.03),(3.98,3.98,.12),15,.04,0)
    for ix in (-1,1):
        for iy in (-1,1):
            x,y=ix*.995,iy*.995;a=.95;b=.95;cut=rng.uniform(.12,.28)
            plan=[(x-a,y-b),(x+a-cut,y-b),(x+a,y-b+cut),(x+a,y+b),(x-a+cut,y+b),(x-a,y+b-cut)]
            n=len(plan);verts=[(u,v,z) for z in (.025,.077) for u,v in plan]
            faces=[tuple(reversed(range(n))),tuple(range(n,n*2))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
            mesh('Worn fitted machinery deck plate',verts,faces,3 if (ix==iy) else 0,0)
    # Inset linear service cut is flush/walkable and breaks the decorative-floor read.
    plate('Recessed servicing trench',(0,0,.079),(.31,3.75,.01),15,.02,0)
    for y in [-1.7+i*.22 for i in range(16)]:plate('Flush servicing grate',(0,y,.091),(.27,.041,.014),10,.006,0)
    for s in (-1,1):plate('Deck working edge',(s*.18,0,.083),(.029,3.8,.014),1,.006,0)

def output(name):
    for o in parts:
        uv=o.data.uv_layers.active or o.data.uv_layers.new(name='Atlas')
        lo=[min(v.co[i] for v in o.data.vertices) for i in range(3)];hi=[max(v.co[i] for v in o.data.vertices) for i in range(3)]
        for f in o.data.polygons:
            tile=f.material_index;axis=max(range(3),key=lambda k:abs(f.normal[k]));axes=[k for k in range(3) if k!=axis]
            for li in f.loop_indices:
                v=o.data.vertices[o.data.loops[li].vertex_index].co;u=(v[axes[0]]-lo[axes[0]])/max(.001,hi[axes[0]]-lo[axes[0]]);w=(v[axes[1]]-lo[axes[1]])/max(.001,hi[axes[1]]-lo[axes[1]])
                uv.data[li].uv=((tile%4+.015+.97*u)/4,(tile//4+.015+.97*w)/4)
            f.material_index=0
        o.data.materials.clear()
    atlas=bpy.data.materials.new('V47_PreservedPbr');atlas.use_nodes=True
    bs=next(n for n in atlas.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
    t=atlas.node_tree.nodes.new('ShaderNodeTexImage');t.image=bpy.data.images.load(str(ROOT/'Assets/_Game/Content/SurfaceHeroV46/Textures/V46_BaseColor.png'));atlas.node_tree.links.new(t.outputs['Color'],bs.inputs['Base Color']);bs.inputs['Metallic'].default_value=.7;bs.inputs['Roughness'].default_value=.45
    for o in parts:o.data.materials.append(atlas)
    bpy.ops.wm.save_as_mainfile(filepath=str(SRC/(name+'.blend')))
    bpy.ops.object.select_all(action='DESELECT')
    for o in parts:o.select_set(True)
    bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name='V47_'+name
    active(o);m=o.modifiers.new('Runtime triangles','TRIANGULATE');bpy.ops.object.modifier_apply(modifier=m.name)
    bpy.ops.export_scene.fbx(filepath=str(OUT/('V47_'+name+'.fbx')),use_selection=True,object_types={'MESH'},global_scale=1,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',bake_anim=False,path_mode='STRIP',mesh_smooth_type='FACE',use_tspace=True)
    metrics[name]={'triangles':len(o.data.polygons),'materials':1,'source':f'art/concept-convergence-v47/{name}.blend','originalProjectArt':True}
for name in ['DeploymentBay','FabricationBay','InductionStation']:
    reset();structure(name);output(name)
for i in range(3):
    reset();deck(4700+i);output('Deck'+str(i))
(DOC/'facility_authoring.json').write_text(json.dumps(metrics,indent=2))
print(json.dumps(metrics))

