"""Project-owned hard-surface sources. No donor file is loaded by this authoring tool.

Meters, Z up, forward -Y. Explicit editable construction and rigid articulated
skin; one skinned renderer and one PBR trim-atlas material per visible LOD.
Run with Blender 5.2 --background --factory-startup --disable-autoexec.
"""
import argparse, bpy, bmesh, math, json, sys, numpy as np
from pathlib import Path
from mathutils import Vector, Matrix
ROOT=Path(__file__).resolve().parents[2]
ART=ROOT/'art/chapter01-actor-production-v2'
IMPORT=ROOT/'Assets/_Game/ArtReview/ActorProductionV2'
EVID=ROOT/'docs/history/implementation-passes/chapter01-actor-production-v2'
for p in [ART,IMPORT/'Models',IMPORT/'Textures',EVID/'blender']: p.mkdir(parents=True,exist_ok=True)

def activate(o):
 bpy.ops.object.select_all(action='DESELECT'); o.select_set(True); bpy.context.view_layer.objects.active=o

def recalc(me):
 bm=bmesh.new(); bm.from_mesh(me); bmesh.ops.recalc_face_normals(bm,faces=bm.faces); bm.to_mesh(me); bm.free()

def clean_export_topology(me):
 # Deterministic triangulation avoids collinear fan triangles on machined
 # n-gon landings. Preserve UV/deform layers and remove only collapsed faces.
 bm=bmesh.new(); bm.from_mesh(me)
 bmesh.ops.triangulate(bm,faces=list(bm.faces),quad_method='BEAUTY',ngon_method='BEAUTY')
 collapsed=[face for face in bm.faces if face.calc_area()<1e-12 or (len(face.verts)==3 and ((face.verts[1].co-face.verts[0].co).cross(face.verts[2].co-face.verts[0].co)).length_squared<1e-24)]
 if collapsed: bmesh.ops.delete(bm,geom=collapsed,context='FACES_ONLY')
 loose=[vertex for vertex in bm.verts if not vertex.link_faces]
 if loose: bmesh.ops.delete(bm,geom=loose,context='VERTS')
 bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces)); bm.to_mesh(me); bm.free(); me.update()

PALETTE=[(.035,.045,.057),(.58,.64,.68),(.27,.34,.40),(.12,.155,.20),(.68,.73,.78),(.55,.22,.046),(.09,.028,.016),(.20,.245,.29)]
ENERGY={'Red':(1,.018,.006),'Amber':(1,.28,.014),'Blue':(.006,.32,1)}
RENDER_IMAGES=True

def atlas():
 """Authored industrial trim sheet, shared across the family; channel maps explicit."""
 n=1024; rng=np.random.default_rng(714); yy,xx=np.mgrid[0:256,0:256]/255
 base=np.zeros((n,n,4),np.float32); packed=base.copy(); normal=base.copy(); ao=base.copy(); emiss={k:base.copy() for k in ENERGY}
 for i,col in enumerate(PALETTE):
  row,colidx=divmod(i,4); sl=(slice(row*512,(row+1)*512),slice(colidx*256,(colidx+1)*256))
  # Vertical tiles are 512 pixels, same physical content with extra resolution.
  y,x=np.mgrid[0:512,0:256]; u=x/255; v=y/511
  noise=rng.normal(0,.015,(512,256)); brushed=.019*np.sin(v*920)+.013*np.sin(v*271)
  edge=np.minimum.reduce([u,1-u,v,1-v]); recess=(edge<.062).astype(float)
  scratches=np.zeros_like(u)
  for j in range(75):
   sx=rng.uniform(.05,.95); sy=rng.uniform(.05,.95); length=rng.uniform(.012,.13); slope=rng.uniform(-.16,.16)
   scratches+=((abs(u-sx-slope*(v-sy))<rng.uniform(.0006,.002))&(abs(v-sy)<length*.5)).astype(float)*rng.uniform(.1,.5)
  scratches=np.clip(scratches,0,1)
  wear=((edge>.063)&(edge<.092)&(np.sin(u*177+v*223)>.1)).astype(float)*.14
  grain=np.clip(1+noise+brushed-recess*.22+scratches*.27+wear,.65,1.2)
  tile=base[sl]; tile[:,:,:3]=np.asarray(col)[None,None,:]*grain[:,:,None]; tile[:,:,3]=1
  metal=[.62,.38,.98,.35,.20,.42,.65,.65][i]; smooth=[.31,.35,.68,.30,.40,.30,.63,.38][i]
  packed[sl][:,:,0]=metal; packed[sl][:,:,3]=np.clip(smooth+noise*2-recess*.09-scratches*.12,0,1)
  normal[sl][:,:,:3]=np.stack([.5+np.cos(v*271)*.023+scratches*.025,.5+np.sin(u*83)*.006,np.ones_like(u)],axis=-1); normal[sl][:,:,3]=1
  ao[sl][:,:,:3]=(1-recess*.2)[:,:,None]; ao[sl][:,:,3]=1
  for k,c in ENERGY.items():
   emiss[k][sl][:,:,3]=1
   if i==6:
    hot=np.exp(-((u-.5)**2+(v-.5)**2)/.004)
    emiss[k][sl][:,:,:3]=np.asarray(c)[None,None,:]*(.88+noise)[:,:,None]*(1-hot[:,:,None]) + np.ones((512,256,3))*hot[:,:,None]
 arrays={'BaseColor':base,'MetallicSmoothness':packed,'Normal':normal,'Occlusion':ao,**{'Emission'+k:v for k,v in emiss.items()}}
 for name,data in arrays.items():
  path=IMPORT/'Textures'/('HostileV2_'+name+'.png')
  img=bpy.data.images.new('HostileV2_'+name,width=n,height=n,alpha=True)
  if name not in ['BaseColor','EmissionRed','EmissionAmber','EmissionBlue']: img.colorspace_settings.name='Non-Color'
  img.pixels.foreach_set(data.ravel()); img.filepath_raw=str(path); img.file_format='PNG'; img.save(); bpy.data.images.remove(img)

def material(color):
 m=bpy.data.materials.new('HostileV2_Industrial_'+color); m.use_nodes=True; nodes=m.node_tree.nodes; links=m.node_tree.links; bs=nodes.get('Principled BSDF')
 def tex(name,space='sRGB'):
  im=bpy.data.images.load(str(IMPORT/'Textures'/('HostileV2_'+name+'.png')),check_existing=True); im.colorspace_settings.name=space
  t=nodes.new('ShaderNodeTexImage'); t.image=im; t.interpolation='Linear'; t.extension='EXTEND'; return t
 links.new(tex('BaseColor').outputs['Color'],bs.inputs['Base Color'])
 packed=tex('MetallicSmoothness','Non-Color'); sep=nodes.new('ShaderNodeSeparateColor'); links.new(packed.outputs['Color'],sep.inputs[0]); links.new(sep.outputs['Red'],bs.inputs['Metallic'])
 inv=nodes.new('ShaderNodeMath'); inv.operation='SUBTRACT'; inv.inputs[0].default_value=1; links.new(packed.outputs['Alpha'],inv.inputs[1]); links.new(inv.outputs[0],bs.inputs['Roughness'])
 nm=nodes.new('ShaderNodeNormalMap'); nm.inputs['Strength'].default_value=.3; links.new(tex('Normal','Non-Color').outputs['Color'],nm.inputs['Color']); links.new(nm.outputs[0],bs.inputs['Normal'])
 links.new(tex('Emission'+color).outputs['Color'],bs.inputs['Emission Color']); bs.inputs['Emission Strength'].default_value=1.8
 return m

class Actor:
 def __init__(self,name,color):
  bpy.ops.wm.read_factory_settings(use_empty=True); self.name=name; self.color=color; self.parts=[]; self.bones={}; self.sockets={}
  self.edit=bpy.data.collections.new(name+'_EDITABLE_AUTHORED_COMPONENTS'); bpy.context.scene.collection.children.link(self.edit)
  self.exports=bpy.data.collections.new(name+'_EXPORT_SKINS'); bpy.context.scene.collection.children.link(self.exports)
  self.mat=material(color); self.rig=None; self.bone('ROOT',(0,0,0),(0,0,.15),None)
 def bone(self,name,head,tail,parent='ROOT'):
  self.bones[name]=(Vector(head),Vector(tail),parent); return name
 def mesh(self,name,vs,fs,mat=1,bone='BODY',bevel=.008,smooth=False):
  me=bpy.data.meshes.new(name); me.from_pydata(vs,[],fs); me.update(); recalc(me)
  o=bpy.data.objects.new(name,me); self.edit.objects.link(o); o['binding']=bone; o['trim_region']=mat; self.parts.append(o)
  # Construction material slots encode UV trim regions, then consolidate.
  for i in range(8): me.materials.append(self.mat)
  for p in me.polygons: p.material_index=mat
  if bevel:
   mod=o.modifiers.new('Machined edge radius','BEVEL'); mod.width=bevel; mod.segments=3; mod.affect='EDGES'; mod.harden_normals=True; mod.material=2
  for poly in me.polygons: poly.use_smooth=smooth
  if bevel or smooth:
   mod=o.modifiers.new('Production corner normals','WEIGHTED_NORMAL'); mod.keep_sharp=True; mod.weight=40
  return o
 def loft(self,name,rings,mat=1,bone='BODY',bevel=.009):
  # Rings: center and cross-section XY radius on Z axis, chamfered octagonal shell.
  vs=[]; count=16
  for z,cx,cy,rx,ry in rings:
   for i in range(count):
    t=2*math.pi*i/count; vs.append((cx+rx*math.cos(t),cy+ry*math.sin(t),z))
  fs=[tuple(range(count-1,-1,-1))]
  for r in range(len(rings)-1):
   for i in range(count): fs.append((r*count+i,r*count+(i+1)%count,(r+1)*count+(i+1)%count,(r+1)*count+i))
  fs.append(tuple(range((len(rings)-1)*count,len(rings)*count)))
  return self.mesh(name,vs,fs,mat,bone,bevel,True)
 def panel(self,name,outline,y,depth=.035,mat=1,bone='BODY',ridge=.018,bevel=.006):
  # Authored planar landing and swept perimeter; no single-apex fan surfaces.
  n=len(outline); cx=sum(v[0] for v in outline)/n; cz=sum(v[1] for v in outline)/n
  vs=[(x,y,z) for x,z in outline]+[(x,y+depth,z) for x,z in outline]+[(cx+(x-cx)*.70,y-ridge,cz+(z-cz)*.70) for x,z in outline]
  fs=[tuple(range(n,2*n)),tuple(range(2*n,3*n))]
  for i in range(n):
   j=(i+1)%n; fs.extend([(i,j,2*n+j,2*n+i),(i,i+n,j+n,j)])
  o=self.mesh(name,vs,fs,mat,bone,bevel)
  # Recess the broad landing inside a machined rim. The actual geometry casts
  # occlusion; graphite gasket and face finish use separate trim regions.
  if mat not in [0,6]:
   bm=bmesh.new(); bm.from_mesh(o.data); bm.faces.ensure_lookup_table(); landing=bm.faces[1]
   result=bmesh.ops.inset_individual(bm,faces=[landing],thickness=min(.006,depth*.10),depth=0,use_even_offset=True)
   for f in result['faces']: f.material_index=0
   for v in landing.verts: v.co.y+=min(.006,depth*.18)
   landing.material_index=mat; bmesh.ops.recalc_face_normals(bm,faces=bm.faces); bm.to_mesh(o.data); bm.free()
  for i,p in enumerate(o.data.polygons):
   if i>=2 and p.normal.y>-.15: p.material_index=0
  return o
 def tube(self,name,a,b,r1,r2=None,mat=2,bone='BODY',steps=16):
  a=Vector(a); b=Vector(b); d=b-a; q=d.to_track_quat('Z','Y'); r2=r1 if r2 is None else r2
  vs=[]
  lip=min(.035,d.length*.2)
  for z,r in [(0,r1*.86),(lip,r1),(d.length-lip,r2),(d.length,r2*.86)]:
   vs += [a+q@Vector((r*math.cos(i*2*math.pi/steps),r*math.sin(i*2*math.pi/steps),z)) for i in range(steps)]
  fs=[tuple(range(steps-1,-1,-1))]
  for row in range(3):
   fs += [(row*steps+i,row*steps+(i+1)%steps,(row+1)*steps+(i+1)%steps,(row+1)*steps+i) for i in range(steps)]
  fs.append(tuple(range(3*steps,4*steps))); return self.mesh(name,vs,fs,mat,bone,0,True)
 def ring(self,name,center,radius,width,thickness,mat=2,bone='BODY',axis=(0,-1,0),segments=48):
  c=Vector(center); q=Vector(axis).to_track_quat('Z','Y'); vs=[]
  for z,r in [(-thickness/2,radius-width/2),(-thickness/2,radius+width/2),(thickness/2,radius+width/2),(thickness/2,radius-width/2)]:
   vs += [c+q@Vector((r*math.cos(i*2*math.pi/segments),r*math.sin(i*2*math.pi/segments),z)) for i in range(segments)]
  fs=[]
  for j in range(4):
   fs += [(j*segments+i,j*segments+(i+1)%segments,((j+1)%4)*segments+(i+1)%segments,((j+1)%4)*segments+i) for i in range(segments)]
  return self.mesh(name,vs,fs,mat,bone,.003,True)
 def joint(self,name,c,r,bone='BODY'):
  c=Vector(c); self.tube(name+' axle',c+Vector((-r*.9,0,0)),c+Vector((r*.9,0,0)),r,r,0,bone,24)
  for s in [-1,1]:
   self.ring(name+' bearing '+str(s),c+Vector((s*r*.96,0,0)),r*.76,r*.21,r*.13,2,bone,(1,0,0),24)
   self.tube(name+' hex retainer '+str(s),c+Vector((s*r*.98,0,0)),c+Vector((s*r*1.1,0,0)),r*.28,mat=7,bone=bone,steps=6)
 def armored_rail(self,name,a,b,width,bone,mat=1):
  a=Vector(a); b=Vector(b); d=b-a; q=d.to_track_quat('Z','Y'); length=d.length
  outline=[(-width*.65,length*.12),(-width,length*.26),(-width*.83,length*.73),(-width*.38,length*.89),(width*.35,length*.86),(width*.85,length*.63),(width*.91,length*.24),(width*.56,length*.11)]
  o=self.panel(name+' segmented armor landing',outline,-width*.7,width*.19,mat,bone,width*.11,width*.05)
  for v in o.data.vertices: v.co=a+q@v.co
  for f in [.31,.58]:
   pa=a+q@Vector((-width*.67,-width*.87,length*f)); pb=a+q@Vector((width*.64,-width*.87,length*f+.016))
   self.tube(name+' machined transverse interruption '+str(f),pa,pb,width*.035,mat=0,bone=bone,steps=8)
 def swept_beam(self,name,centers,widths,depth,bone,mat=3):
  vs=[]; profile=[(-.65,-1),(.65,-1),(1,-.55),(1,.55),(.65,1),(-.65,1),(-1,.55),(-1,-.55)]
  for i,c in enumerate(centers):
   c=Vector(c); prev=Vector(centers[max(0,i-1)]); nxt=Vector(centers[min(len(centers)-1,i+1)]); q=(nxt-prev).to_track_quat('Z','Y')
   vs += [c+q@Vector((x*widths[i],y*depth,0)) for x,y in profile]
  fs=[tuple(range(7,-1,-1)),tuple(range((len(centers)-1)*8,len(centers)*8))]
  for j in range(len(centers)-1): fs += [(j*8+i,j*8+(i+1)%8,(j+1)*8+(i+1)%8,(j+1)*8+i) for i in range(8)]
  self.mesh(name,vs,fs,mat,bone,.016,True)
 def sector(self,name,c,r,w,start,end,bone='BODY',mat=1):
  c=Vector(c); count=16; vs=[]
  for y,rad in [(-w*.20,r-w*.50),(-w*.20,r+w*.50),(w*.2,r+w*.50),(w*.2,r-w*.5)]:
   vs += [c+Vector((math.sin(start+(end-start)*i/count)*rad,y,math.cos(start+(end-start)*i/count)*rad)) for i in range(count+1)]
  n=count+1; fs=[]
  for j in range(4): fs += [(j*n+i,j*n+i+1,((j+1)%4)*n+i+1,((j+1)%4)*n+i) for i in range(count)]
  fs += [(0,n,2*n,3*n),(n-1,2*n-1,3*n-1,4*n-1)]
  self.mesh(name,vs,fs,mat,bone,.006,True)
 def actuator(self,name,a,b,r,bone):
  a=Vector(a); b=Vector(b); mid=a+(b-a)*.59
  self.tube(name+' cylinder',a,mid,r,mat=0,bone=bone); self.tube(name+' chrome ram',mid,b,r*.42,mat=2,bone=bone)
  self.tube(name+' gland',mid-(b-a).normalized()*r*.3,mid+(b-a).normalized()*r*.3,r*1.1,mat=7,bone=bone)
 def limb(self,name,a,b,width,bone,armor=1):
  a=Vector(a); b=Vector(b); d=b-a; q=d.to_track_quat('Z','Y')
  self.tube(name+' internal spar',a,b,width*.40,width*.32,0,bone)
  # Swept tapered longitudinal armor, curved on cross section with open joint ends.
  vs=[]; count=12
  for f,rx,ry in [(.12,width*.67,width*.56),(.30,width,width*.75),(.72,width*.78,width*.61),(.86,width*.48,width*.45)]:
   vs += [a+q@Vector((rx*math.cos(i*2*math.pi/count),ry*math.sin(i*2*math.pi/count),d.length*f)) for i in range(count)]
  fs=[tuple(range(count-1,-1,-1)),tuple(range(3*count,4*count))]
  for j in range(3): fs += [(j*count+i,j*count+(i+1)%count,(j+1)*count+(i+1)%count,(j+1)*count+i) for i in range(count)]
  self.mesh(name+' swept armor',vs,fs,armor,bone,.006,True)
  off=q@Vector((width*.52,-width*.34,0)); self.actuator(name+' external actuator',a+d*.16+off,b-d*.1+off,width*.17,bone)
 def blade(self,name,a,tip,width,bone,color=True):
  a=Vector(a); tip=Vector(tip); d=tip-a; q=d.to_track_quat('Z','Y'); l=d.length
  verts=[(-width*.48,0,0),(-width,0,l*.2),(-width*.5,0,l*.65),(0,0,l),(width*.18,0,l*.7),(width*.5,0,l*.15)]
  # Real knife section: central spine plus steel sides and sharp contact edge.
  vs=[a+q@Vector((x,y-.014,z)) for x,y,z in verts]+[a+q@Vector((x,y+.014,z)) for x,y,z in verts]
  fs=[tuple(range(5,-1,-1)),tuple(range(6,12))]+[(i,(i+1)%6,(i+1)%6+6,i+6) for i in range(6)]
  self.mesh(name+' forged blade',vs,fs,2,bone,.002)
  self.tube(name+' load bearing spine',a,a+d*.72,width*.20,width*.08,0,bone,8)
  if color:
   e1=a+q@Vector((-width*.87,-.017,l*.21)); e2=a+q@Vector((-width*.46,-.017,l*.64))
   self.tube(name+' energized cutting edge',e1,e2,width*.062,width*.046,6,bone,8)
   self.tube(name+' contact edge',e2,tip,width*.046,.001,6,bone,8)
 def core(self,name,c,r,bone='BODY',heavy=False):
  c=Vector(c); self.ring(name+' graphite containment',c,r,r*.27,r*.37,0,bone)
  self.ring(name+' machined aperture',c+Vector((0,-r*.18,0)),r*.91,r*.10,r*.16,2,bone)
  self.ring(name+' internal field coil',c+Vector((0,-r*.16,0)),r*.72,r*.11,r*.11,6,bone)
  bpy.ops.mesh.primitive_uv_sphere_add(segments=32,ring_count=16,radius=1,location=c)
  o=bpy.context.object; o.name=name+' contained lens'; o.scale=(r*.64,r*.52,r*.64); activate(o); bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
  for co in list(o.users_collection): co.objects.unlink(o)
  self.edit.objects.link(o); o['binding']=bone; o['trim_region']=6
  for i in range(8): o.data.materials.append(self.mat)
  self.parts.append(o)
  for poly in o.data.polygons: poly.use_smooth=True; poly.material_index=6
  count=8 if heavy else 6
  for i in range(count):
   t=i*2*math.pi/count; off=Vector((math.sin(t)*r,0,math.cos(t)*r))
   self.tube(name+' radial clamp '+str(i),c+off*.73+Vector((0,-r*.22,0)),c+off*1.17+Vector((0,-r*.10,0)),r*.071,mat=7,bone=bone,steps=8)
   self.tube(name+' clamp stud '+str(i),c+off*1.06+Vector((0,-r*.10,0)),c+off*1.06+Vector((0,-r*.23,0)),r*.047,mat=2,bone=bone,steps=6)
  if r>.10:
   for i in range(4):
    start=i*math.pi/2+.18; end=(i+1)*math.pi/2-.18
    self.sector(name+' segmented containment armor '+str(i),c+Vector((0,r*.02,0)),r*1.15,r*.19,start,end,bone,7 if heavy else 1)
   self.ring(name+' contained axial coil',c+Vector((0,-r*.31,0)),r*.43,r*.034,r*.025,2,bone)
   self.tube(name+' central field pole',c+Vector((0,-r*.34,0)),c+Vector((0,-r*.54,0)),r*.105,mat=6,bone=bone,steps=12)
   # Inboard conductors visibly terminate at the emitter. They remain opaque
   # geometry; the hot source is localized rather than a broad emissive shell.
   for i in range(8 if heavy else 6):
    t=i*2*math.pi/(8 if heavy else 6)+.12
    direction=Vector((math.sin(t),0,math.cos(t)))
    self.tube(name+' radial conductor '+str(i),c+direction*r*.29+Vector((0,-r*.55,0)),c+direction*r*.62+Vector((0,-r*.46,0)),r*.028,mat=2,bone=bone,steps=8)
    self.tube(name+' conductor retainer '+str(i),c+direction*r*.67+Vector((0,-r*.39,0)),c+direction*r*.67+Vector((0,-r*.49,0)),r*.047,mat=0,bone=bone,steps=8)
 def socket(self,name,position,parent='BODY'):
  self.sockets[name]=(Vector(position),parent)
 def finish(self):
  # Concept labels have an unspecified axis. Ground bipeds use height; the
  # low vehicle uses length. Bake the family fit into source coordinates,
  # keeping exported transforms at unit scale and G-0 completely unchanged.
  fit={'Scout':1.16,'Warden':1.16,'ArcDrone':1.14,'Carrier':1.125,'Magnetar':1.18,'Custodian':1.16}.get(self.name,1.)
  for obj in self.parts:
   for vertex in obj.data.vertices: vertex.co*=fit
  self.bones={name:(head*fit,tail*fit,parent) for name,(head,tail,parent) in self.bones.items()}
  self.sockets={name:(position*fit,bone) for name,(position,bone) in self.sockets.items()}
  rigdata=bpy.data.armatures.new(self.name+'_MECHANICAL_RIG'); rig=bpy.data.objects.new('RIG',rigdata); self.exports.objects.link(rig); self.rig=rig; activate(rig); bpy.ops.object.mode_set(mode='EDIT')
  for name,(head,tail,parent) in self.bones.items():
   b=rigdata.edit_bones.new(name); b.head=head; b.tail=tail; b.use_deform=True
   if parent: b.parent=rigdata.edit_bones[parent]
  bpy.ops.object.mode_set(mode='OBJECT'); rig.show_in_front=True
  for o in self.parts:
   activate(o)
   for mod in list(o.modifiers): bpy.ops.object.modifier_apply(modifier=mod.name)
   # Per-face trim projection avoids overlapping different surface categories.
   for old_uv in list(o.data.uv_layers): o.data.uv_layers.remove(old_uv)
   uv=o.data.uv_layers.new(name='IndustrialTrimUV')
   mesh_lo=[min(v.co[i] for v in o.data.vertices) for i in range(3)]; mesh_hi=[max(v.co[i] for v in o.data.vertices) for i in range(3)]
   for p in o.data.polygons:
    idx=p.material_index; row,col=divmod(idx,4)
    normal=p.normal; axis=max(range(3),key=lambda i:abs(normal[i])); axes=[i for i in range(3) if i!=axis]; verts=[o.data.vertices[o.data.loops[k].vertex_index].co for k in p.loop_indices]
    lo=[mesh_lo[i] for i in axes]; hi=[mesh_hi[i] for i in axes]
    for k,v in zip(p.loop_indices,verts):
     u=.055+.89*(v[axes[0]]-lo[0])/max(hi[0]-lo[0],.00001); w=.055+.89*(v[axes[1]]-lo[1])/max(hi[1]-lo[1],.00001)
     uv.data[k].uv=((col+u)/4,(row+w)/2)
   group=o.vertex_groups.new(name=o['binding']); group.add(list(range(len(o.data.vertices))),1,'REPLACE')
  # Editable parts survive separately. Export copy is consolidated, not hundreds
  # of runtime renderers. LODs use the same exact skin weights and trim UVs.
  copies=[]
  for src in self.parts:
   o=src.copy(); o.data=src.data.copy(); self.exports.objects.link(o); copies.append(o)
  bpy.ops.object.select_all(action='DESELECT')
  for o in copies: o.select_set(True)
  bpy.context.view_layer.objects.active=copies[0]; bpy.ops.object.join(); skin=copies[0]; skin.name=self.name+'_LOD0'
  # Joining duplicates a single material slot; remove slot indirection.
  for p in skin.data.polygons: p.material_index=0
  skin.data.materials.clear(); skin.data.materials.append(self.mat)
  clean_export_topology(skin.data)
  skins=[skin]
  for index,ratio in [(1,.57),(2,.29)]:
   o=skin.copy(); o.data=skin.data.copy(); self.exports.objects.link(o); o.name=self.name+'_LOD'+str(index); activate(o)
   mod=o.modifiers.new('Screen coverage LOD reduction','DECIMATE'); mod.ratio=ratio; mod.use_collapse_triangulate=True
   # Protect material seams and articulation weights where decimation permits.
   bpy.ops.object.modifier_apply(modifier=mod.name); clean_export_topology(o.data); skins.append(o)
  for o in skins:
   o.parent=rig; mod=o.modifiers.new('Rigid mechanism skin','ARMATURE'); mod.object=rig
  for name,(position,bone) in self.sockets.items():
   o=bpy.data.objects.new(name,None); self.exports.objects.link(o); o.empty_display_size=.025; o.parent=rig; o.parent_type='BONE'; o.parent_bone=bone
   bpy.context.view_layer.update(); o.matrix_world=Matrix.Translation(position)
  self.skins=skins; self.animate()
  self.edit.hide_render=True; self.edit.hide_viewport=True
  skins[1].hide_render=skins[2].hide_render=True
  bpy.context.scene.frame_set(1)
  self.report(skins)
  self.render(skins)
  # Keep all LODs visible for export; Unity LODGroup selects one skin.
  skins[1].hide_render=skins[2].hide_render=False
  bpy.ops.object.select_all(action='DESELECT'); rig.select_set(True)
  for o in self.exports.all_objects: o.select_set(True)
  bpy.context.view_layer.objects.active=rig
  bpy.ops.export_scene.fbx(filepath=str(IMPORT/'Models'/(self.name+'_V2.fbx')),use_selection=True,object_types={'ARMATURE','MESH','EMPTY'},global_scale=1,axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',bake_space_transform=False,use_space_transform=True,mesh_smooth_type='FACE',use_tspace=False,add_leaf_bones=False,primary_bone_axis='Y',secondary_bone_axis='X',bake_anim=True,bake_anim_use_all_bones=True,bake_anim_use_nla_strips=False,bake_anim_use_all_actions=True,bake_anim_force_startend_keying=True,bake_anim_step=1,bake_anim_simplify_factor=0,path_mode='RELATIVE',use_mesh_modifiers=True)
  skins[1].hide_render=skins[2].hide_render=True
  # Review cameras/lights stay in the editable source; no DCC helper exported.
  for img in bpy.data.images:
   if img.source=='FILE':
    filename=Path(img.filepath).name; img.pack(); img.filepath='//../../Assets/_Game/ArtReview/ActorProductionV2/Textures/'+filename
  bpy.ops.wm.save_as_mainfile(filepath=str(ART/(self.name+'_V2.blend')))
  print('ACTOR_EXPORTED',self.name,flush=True)
 def animate(self):
  rig=self.rig; scene=bpy.context.scene; scene.render.fps=30; rig.animation_data_create(); self.clips=[]
  extra={'Warden':['Block'],'Cutter':['Cut'],'ArcDrone':['Hover','Discharge'],'Carrier':['Bank'],'Magnetar':['Charge','Vent'],'Custodian':['Telegraph','AttackLine','AttackCircle','AttackCone']}.get(self.name,[])
  for clip in ['Idle','Move','Attack','Hit','Death']+extra:
   frames=61 if clip in ['Idle','Move','Hover'] else (46 if clip=='Death' else 31)
   action=bpy.data.actions.new(self.name+'|'+clip); action.use_fake_user=True; rig.animation_data.action=action
   for f in range(1,frames+1,3):
    scene.frame_set(f)
    t=(f-1)/(frames-1); wave=math.sin(t*math.pi*2); pulse=math.sin(t*math.pi)
    for b in rig.pose.bones: b.rotation_mode='XYZ'; b.rotation_euler=(0,0,0); b.location=(0,0,0)
    body=rig.pose.bones['BODY']; body.rotation_euler.x=.012*wave
    for side,phase in [('L',0),('R',math.pi)]:
     sign=1 if side=='L' else -1; s=math.sin(t*math.pi*4+phase)
     if clip=='Move' and self.name in ['Scout','Warden']:
      rig.pose.bones[side+'_HIP'].rotation_euler.x=s*(.28 if self.name=='Scout' else .16)
      rig.pose.bones[side+'_KNEE'].rotation_euler.x=max(0,-s)*.38
      rig.pose.bones[side+'_ANKLE'].rotation_euler.x=-s*.13
      rig.pose.bones[side+'_SHOULDER'].rotation_euler.x=-s*.16
      body.rotation_euler.x=.11 if self.name=='Scout' else .03
     if self.name in ['Scout','Warden'] and clip in ['Attack','Block']:
      rig.pose.bones[side+'_SHOULDER'].rotation_euler.x=-pulse*(.7 if clip=='Attack' else .35)
      rig.pose.bones[side+'_ELBOW'].rotation_euler.x=-pulse*.35
      rig.pose.bones[side+'_TOOL'].rotation_euler.y=pulse*.12*sign
    if self.name in ['Cutter','Magnetar','Custodian']:
     for i in range(4):
      if clip=='Move':
       rig.pose.bones['SUPPORT_'+str(i)].rotation_euler.y=math.sin(t*4*math.pi+(i%2)*math.pi)*.1
       rig.pose.bones['KNEE_'+str(i)].rotation_euler.x=math.sin(t*4*math.pi+(i%2)*math.pi)*.12
     if clip in ['Attack','Cut','Charge','Telegraph','AttackLine','AttackCircle','AttackCone','Vent']:
      for name in ['TOOL_L','TOOL_R','CAGE_L','CAGE_R','REACTOR','CROWN_L','CROWN_R']:
       if name in rig.pose.bones:
        rig.pose.bones[name].rotation_euler.x=pulse*(-.32 if name.startswith('TOOL') else .17)
        rig.pose.bones[name].rotation_euler.y=pulse*(.18 if name.endswith('L') else -.18)
      body.rotation_euler.x=-pulse*.08
    if self.name=='ArcDrone':
     body.location.y=.035*wave
     for side in ['L','R']:
      rig.pose.bones['WING_'+side].rotation_euler.y=wave*.035
      if clip in ['Attack','Discharge']: rig.pose.bones['WING_'+side].rotation_euler.x=-pulse*.18
     if clip in ['Attack','Discharge']: rig.pose.bones['EMITTER'].location.y=-pulse*.025
    if self.name=='Carrier':
     body.rotation_euler.y=wave*(.055 if clip in ['Move','Bank'] else .008)
     for side in ['L','R']: rig.pose.bones['PROPULSION_'+side].rotation_euler.x=wave*.025
     if clip=='Attack':
      body.rotation_euler.x=-pulse*.08
      for side in ['L','R']: rig.pose.bones['PROPULSION_'+side].rotation_euler.x=pulse*.13
    if self.name=='Magnetar' and clip in ['Charge','Vent']:
     for side,sign in [('L',1),('R',-1)]:
      rig.pose.bones['CAGE_'+side].rotation_euler.y=sign*pulse*(.26 if clip=='Charge' else .12)
      rig.pose.bones['TOOL_'+side].rotation_euler.x=-pulse*(.34 if clip=='Charge' else -.16)
     rig.pose.bones['REACTOR'].rotation_euler.z=wave*.055 if clip=='Vent' else 0
    if self.name=='Custodian' and clip in ['Telegraph','AttackLine','AttackCircle','AttackCone']:
     for side,sign in [('L',1),('R',-1)]:
      crown=rig.pose.bones['CROWN_'+side]; tool=rig.pose.bones['TOOL_'+side]
      crown.rotation_euler.y=sign*pulse*{'Telegraph':.20,'AttackLine':.045,'AttackCircle':.42,'AttackCone':.26}[clip]
      crown.rotation_euler.z=sign*pulse*(.23 if clip=='AttackCircle' else .06)
      tool.rotation_euler.x=-pulse*{'Telegraph':.18,'AttackLine':.48,'AttackCircle':.16,'AttackCone':.38}[clip]
      tool.rotation_euler.y=sign*pulse*(.46 if clip=='AttackCircle' else .10)
     rig.pose.bones['REACTOR'].rotation_euler.z=wave*.16 if clip=='AttackCircle' else pulse*.035
    if clip=='Hit': body.rotation_euler.x=-pulse*.15; body.rotation_euler.z=pulse*.10
    if clip=='Death':
     body.rotation_euler.x=t*.8; body.rotation_euler.z=t*.23; body.location.y=-t*(.35 if self.name!='Carrier' else .09)
     for b in rig.pose.bones:
      if b.name.startswith(('CAGE_','CROWN_','WING_')): b.rotation_euler.y=t*.30
      if b.name.startswith('KNEE_') or b.name.endswith('_KNEE'): b.rotation_euler.x=t*.58
      if b.name.endswith('_HIP'): b.rotation_euler.x=t*.36
    # Baked presentation correction only: project the lowest posed point onto
    # the floor, without moving ROOT or adding runtime foot/contact authority.
    # This covers strike, recoil and collapse as well as locomotion.
    bpy.context.view_layer.update()
    evaluated=self.skins[0].evaluated_get(bpy.context.evaluated_depsgraph_get()); posed=evaluated.to_mesh()
    coordinates=np.empty(len(posed.vertices)*3,dtype=np.float32); posed.vertices.foreach_get('co',coordinates)
    min_z=float(coordinates.reshape(-1,3)[:,2].min()); evaluated.to_mesh_clear()
    if min_z<.025:
     local_up=rig.data.bones['BODY'].matrix_local.to_3x3().inverted()@Vector((0,0,1))
     body.location+=local_up*(.025-min_z)
    for b in rig.pose.bones:
     b.keyframe_insert(data_path='rotation_euler',frame=f,group=b.name); b.keyframe_insert(data_path='location',frame=f,group=b.name)
   self.clips.append({'name':clip,'frames':[1,frames],'fps':30,'duration':(frames-1)/30,'root_motion':False})
  rig.animation_data.action=bpy.data.actions[self.name+'|Idle']; scene.frame_start=1; scene.frame_end=61; scene.frame_set(1)
 def report(self,skins):
  scene=bpy.context.scene; scene.frame_set(1); deps=bpy.context.evaluated_depsgraph_get(); lods=[]; bounds=[]
  for o in skins:
   me=o.evaluated_get(deps).to_mesh(); me.calc_loop_triangles(); vs=[o.matrix_world@v.co for v in me.vertices]
   lo=[min(v[i] for v in vs) for i in range(3)]; hi=[max(v[i] for v in vs) for i in range(3)]
   lods.append({'name':o.name,'vertices':len(me.vertices),'triangles':len(me.loop_triangles),'dimensions_xyz':[hi[i]-lo[i] for i in range(3)],'min_xyz':lo,'max_xyz':hi,'materials':1,'weights_per_vertex':1}); o.evaluated_get(deps).to_mesh_clear()
  self.bounds=(Vector(lods[0]['min_xyz']),Vector(lods[0]['max_xyz']))
  data={'actor':self.name,'authored_geometry_percent':100,'donor_data_reused':False,'coordinate_system':'meters, Z up, forward -Y; FBX -Z forward, Y up','energy':self.color,'editable_components':len(self.parts),'bones':len(self.bones),'bone_hierarchy':{n:p for n,(h,t,p) in self.bones.items()},'sockets':{n:{'position_xyz':list(p),'parent_bone':b} for n,(p,b) in self.sockets.items()},'lods':lods,'clips':self.clips,'maps':['BaseColor','MetallicSmoothness','Normal','Occlusion','Emission'+self.color],'trim_atlas_size':1024,'ready_for_integration':False,'acceptance':'Await matched Unity camera, animation/envelope validation and owner device gate.'}
  (EVID/(self.name+'_METRICS.json')).write_text(json.dumps(data,indent=2))
 def render(self,skins):
  scene=bpy.context.scene; scene.render.engine='CYCLES'; scene.cycles.samples=24; scene.cycles.use_denoising=True
  scene.render.resolution_x=900; scene.render.resolution_y=900; scene.render.resolution_percentage=100; scene.render.image_settings.file_format='PNG'
  scene.world=bpy.data.worlds.new('Neutral industrial studio'); scene.world.use_nodes=True; scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.14,.19,.25,1); scene.world.node_tree.nodes['Background'].inputs[1].default_value=.4
  scene.view_settings.view_transform='AgX'; scene.view_settings.look='AgX - Medium High Contrast'
  lights=[]
  for name,pos,power,size,color in [('Soft key',(3,-4,6),600,5,(.82,.91,1)),('Rim',(-3,1,4),800,3,(.65,.78,1)),('Warm bounce',(1,-1,2),70,2,(1,.54,.25))]:
   d=bpy.data.lights.new(name,'AREA'); d.energy=power; d.shape='DISK'; d.size=size; d.color=color; o=bpy.data.objects.new(name,d); scene.collection.objects.link(o); o.location=pos; o.rotation_euler=(Vector((0,0,1))-o.location).to_track_quat('-Z','Y').to_euler(); lights.append(o)
  d=bpy.data.cameras.new('Production diagnostic camera'); cam=bpy.data.objects.new('Production diagnostic camera',d); scene.collection.objects.link(cam); scene.camera=cam; d.type='ORTHO'
  lo,hi=self.bounds; center=(lo+hi)/2; span=max(hi.x-lo.x,hi.y-lo.y,hi.z-lo.z); d.ortho_scale=span*1.25
  for view,direction in [('front',(0,-1,.10)),('threequarter',(1,-1.8,.95)),('top',(0,-.001,1)),('silhouette',(0,-1,.1))]:
   cam.location=center+Vector(direction).normalized()*span*4; cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
   old=None
   if view=='silhouette':
    old=skins[0].data.materials[0]; m=bpy.data.materials.new('Diagnostic black silhouette'); m.diffuse_color=(0,0,0,1); m.use_nodes=True; m.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(0,0,0,1); skins[0].data.materials[0]=m
    scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.85,.85,.85,1)
   scene.render.filepath=str(EVID/'blender'/(self.name+'_'+view+'.png'))
   if RENDER_IMAGES: bpy.ops.render.render(write_still=True)
   if old: skins[0].data.materials[0]=old; scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.14,.19,.25,1)
  if self.name in ['Magnetar','Custodian']:
   self.rig.animation_data.action=bpy.data.actions[self.name+('|Charge' if self.name=='Magnetar' else '|Telegraph')]; scene.frame_set(16)
   cam.location=center+Vector((1,-1.8,.95)).normalized()*span*4; cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler(); scene.render.filepath=str(EVID/'blender'/(self.name+'_telegraph.png'))
   if RENDER_IMAGES: bpy.ops.render.render(write_still=True)
   self.rig.animation_data.action=bpy.data.actions[self.name+'|Idle']; scene.frame_set(1)

def common_sockets(a,z):
 for n,p in [('VisualCenter',(0,0,z*.55)),('HitCenter',(0,-.03,z*.55)),('Core',(0,-.12,z*.62)),('DamageNumberAnchor',(0,0,z+.10)),('HealthBarAnchor',(0,0,z+.16)),('GroundContact',(0,0,0)),('TelegraphOrigin',(0,0,0))]: a.socket(n,p,'ROOT' if n in ['GroundContact','TelegraphOrigin'] else 'BODY')

def biped(name,heavy=False):
 a=Actor(name,'Amber' if heavy else 'Red'); k=1.66 if heavy else 1.; width=.22 if heavy else .095
 a.bone('BODY',(0,0,.54*k),(0,-.025,.83*k)); a.bone('HEAD',(0,-.03,.9*k),(0,-.04,1.01*k),'BODY')
 a.loft('Pelvic load frame',[(.49*k,0,.025,width*.85,.085*k),(.57*k,0,0,width,.09*k),(.62*k,0,0,width*.7,.065*k)],0)
 a.loft('Torso inner pressure enclosure',[(.59*k,0,.015,width*.62,.065*k),(.69*k,0,-.015,width*1.1,.11*k),(.85*k,0,.015,width*1.6,.12*k),(.90*k,0,.025,width*1.3,.09*k)],0)
 for s in [-1,1]:
  side='L' if s==-1 else 'R'; hip=(s*.10*k,0,.53*k); knee=(s*.15*k,-.025,.29*k); ankle=(s*.17*k,.025,.07*k)
  a.bone(side+'_HIP',hip,knee,'BODY'); a.bone(side+'_KNEE',knee,ankle,side+'_HIP'); a.bone(side+'_ANKLE',ankle,(s*.17*k,-.1*k,.025),side+'_KNEE')
  a.joint(side+' hip',hip,.056*k,'BODY'); a.joint(side+' knee',knee,.05*k,side+'_HIP'); a.joint(side+' ankle',ankle,.036*k,side+'_KNEE')
  a.limb(side+' femoral shield',hip,knee,.055*k,side+'_HIP'); a.limb(side+' tibial shield',knee,ankle,.043*k,side+'_KNEE')
  a.loft(side+' grounded heel bearing',[(.018,s*.17*k,.028*k,.054*k,.055*k),(.048*k,s*.17*k,.02*k,.060*k,.062*k),(.08*k,s*.17*k,.02*k,.041*k,.040*k)],0,side+'_ANKLE',.003)
  for toe in [-1,1]:
   tx=s*.17*k+toe*.025*k
   a.loft(side+' split mechanical load toe '+str(toe),[(.018,tx,-.053*k,.023*k,.105*k),(.041*k,tx,-.068*k,.027*k,.095*k),(.065*k,tx,-.015*k,.020*k,.045*k)],7,side+'_ANKLE',.003)
  a.actuator(side+' Achilles foot damper',(s*.17*k,.072*k,.095*k),(s*.17*k,.069*k,.22*k),.013*k,side+'_KNEE')
  a.panel(side+' tibial layered front armor',[(s*.17*k-.034*k,.10*k),(s*.17*k-.041*k,.24*k),(s*.17*k,.27*k),(s*.17*k+.038*k,.23*k),(s*.17*k+.027*k,.11*k)],-.068*k,.026*k,1,side+'_KNEE',.009*k,.003)
  shoulder=(s*(.235 if heavy else .16)*k,-.025*k,.83*k); elbow=(s*(.31 if heavy else .23)*k,-.055*k,.64*k); tool=(s*(.355 if heavy else .29)*k,-.15*k,.55*k)
  a.bone(side+'_SHOULDER',shoulder,elbow,'BODY'); a.bone(side+'_ELBOW',elbow,tool,side+'_SHOULDER'); a.bone(side+'_TOOL',tool,(tool[0],tool[1]-.045,tool[2]-.08),side+'_ELBOW')
  a.joint(side+' shoulder exposed bearing',shoulder,.048*k,'BODY'); a.joint(side+' elbow exposed bearing',elbow,.035*k,side+'_SHOULDER')
  a.limb(side+' upper arm',shoulder,elbow,.047*k,side+'_SHOULDER'); a.limb(side+' lower arm',elbow,tool,.039*k,side+'_ELBOW',7)
  # Separate chest armor with narrow center aperture and tapered waist.
  out=[(s*.025*k,.72*k),(s*.06*k,.65*k),(s*(.155 if heavy else .112)*k,.74*k),(s*(.22 if heavy else .15)*k,.83*k),(s*(.19 if heavy else .14)*k,.87*k),(s*.035*k,.87*k)]
  a.panel(side+' swept chest armor',out,-.112*k,.045*k,1,'BODY',.027*k)
  a.panel(side+' upper pectoral layered collar',[(s*.052*k,.87*k),(s*.19*k,.88*k),(s*.205*k,.83*k),(s*.145*k,.814*k),(s*.045*k,.841*k)],-.142*k,.022*k,7,'BODY',.01*k,.003)
  a.tube(side+' pectoral titanium seam',(s*.055*k,-.149*k,.823*k),(s*.147*k,-.137*k,.797*k),.007*k,mat=2)
  a.tube(side+' thoracic optical status',(s*.078*k,-.146*k,.777*k),(s*.078*k,-.146*k,.81*k),.007*k,mat=6)
  a.panel(side+' scapular articulated armor',[(s*.16*k,.88*k),(s*.245*k,.89*k),(s*.27*k,.85*k),(s*.23*k,.795*k),(s*.17*k,.80*k)],-.039*k,.063*k,1,side+'_SHOULDER',.013*k,.004)
  for z in [.66,.69,.72]: a.tube(side+' exposed waist corrugation',(s*.042*k,-.069*k,z*k),(s*.060*k,-.05*k,z*k),.009*k,mat=2)
  a.panel(side+' hip armor',[(s*.04*k,.60*k),(s*.145*k,.61*k),(s*.18*k,.54*k),(s*.10*k,.49*k)],-.095*k,.025*k,1,'BODY')
  if heavy:
   # Grip and rear bracket are connected to lower arm/tool, never torso.
   c=(s*.40*k,-.21*k,.53*k); a.actuator(side+' shield control link',tool,c,.021*k,side+'_TOOL')
   a.tube(side+' physical shield grip',(s*.40*k,-.20*k,.49*k),(s*.40*k,-.20*k,.59*k),.025*k,mat=2,bone=side+'_TOOL')
   x=s*.40*k; lower=.20*k; upper=.76*k; w=.14*k
   outline=[(x-w*.68,lower),(x-w,lower+.06*k),(x-w,upper-.07*k),(x-w*.7,upper),(x+w*.65,upper),(x+w,upper-.07*k),(x+w,lower+.08*k),(x+w*.65,lower)]
   a.panel(side+' independent shield structural frame',outline,-.245*k,.067*k,5,side+'_TOOL',.005*k,.009*k)
   inner=[(x+(xx-x)*.83,(zz-(upper+lower)/2)*.91+(upper+lower)/2) for xx,zz in outline]
   a.panel(side+' shield layered pale armor',inner,-.257*k,.028*k,1,side+'_TOOL',.012*k,.006*k)
   a.panel(side+' shield recessed graphite service panel',[(x-w*.46,lower+.13*k),(x-w*.56,upper-.12*k),(x+w*.40,upper-.12*k),(x+w*.46,lower+.13*k)],-.281*k,.012*k,3,side+'_TOOL',.003*k,.003)
   for zz in [lower+.075*k,upper-.07*k]:
    for xx in [x-w*.61,x+w*.61]: a.tube(side+' shield fastener',(xx,-.285*k,zz),(xx,-.30*k,zz),.012*k,mat=2,bone=side+'_TOOL',steps=6)
   a.socket('ShieldPivot_'+side,c,side+'_TOOL'); a.socket('ShieldImpact_'+side,(x,-.29*k,.52*k),side+'_TOOL'); a.socket('Grip_'+side,tool,side+'_TOOL')
   for zz in [.37,.45,.53,.61]:
    a.panel(side+' shield inset drive cooling port '+str(zz),[(x-w*.31,zz*k),(x+w*.23,zz*k+.013*k),(x+w*.23,zz*k+.03*k),(x-w*.31,zz*k+.018*k)],-.303*k,.009*k,7,side+'_TOOL',.001,.002)
   a.tube(side+' shield field monitor',(x+w*.53,-.295*k,.51*k),(x+w*.53,-.295*k,.58*k),.007*k,mat=6,bone=side+'_TOOL')
  else:
   tip=(s*.57,-.16,.08); a.blade(side+' integrated blade arm',tool,tip,.047,side+'_TOOL'); a.socket('BladeTip_'+side,tip,side+'_TOOL'); a.socket('BladeRoot_'+side,tool,side+'_TOOL')
   a.panel(side+' dorsal shoulder blade',[(s*.12,.86),(s*.21,.89),(s*.26,.82),(s*.20,.77)],.045,.035,1,'BODY',.015)
  a.socket('AttackOrigin_'+side,tool,side+'_TOOL')
  # Rear cooling channels and longitudinal rails, not decorative antenna weapons.
  a.tube(side+' rear power route',(s*.09*k,.105*k,.65*k),(s*.13*k,.12*k,.84*k),.016*k,mat=7)
  for z in [.71,.75,.79]: a.tube(side+' rear vent', (s*.06*k,.133*k,z*k),(s*.14*k,.133*k,z*k),.009*k,mat=0)
 a.loft('Sensor internal housing',[(.90*k,0,-.03*k,.058*k,.054*k),(.955*k,0,-.045*k,.070*k,.069*k),(1.015*k,0,.008*k,.038*k,.026*k)],0,'HEAD',.004*k)
 a.panel('Swept armored sensor brow',[(-.085*k,.96*k),(-.06*k,1.017*k),(0,1.032*k),(.06*k,1.017*k),(.085*k,.96*k),(.046*k,.953*k),(-.046*k,.953*k)],-.096*k,.074*k,1,'HEAD',.017*k,.004)
 for s in [-1,1]:
  a.panel('Sensor cheek armor '+str(s),[(s*.041*k,.94*k),(s*.079*k,.949*k),(s*.085*k,.981*k),(s*.102*k,.957*k),(s*.078*k,.909*k),(s*.051*k,.915*k)],-.093*k,.046*k,7,'HEAD',.008*k,.003)
 a.panel('Recessed angular sensor visor',[(-.066*k,.955*k),(-.041*k,.924*k),(.046*k,.924*k),(.066*k,.955*k),(.045*k,.979*k),(-.045*k,.979*k)],-.105*k,.012*k,0,'HEAD',.004)
 a.tube('Single optical sensor',(-.027*k,-.120*k,.947*k),(.027*k,-.120*k,.947*k),.008*k,mat=6,bone='HEAD',steps=8)
 a.core('Contained thoracic cell',(0,-.122*k,.725*k),.044*k)
 if heavy:
  a.loft('Heavy dorsal upper armor',[(.78*k,0,.11*k,.19*k,.09*k),(.86*k,0,.08*k,.21*k,.11*k),(.92*k,0,.06*k,.16*k,.07*k)],1)
  a.panel('Ventral armored keel',[(-.10*k,.65*k),(0,.58*k),(.10*k,.65*k),(.10*k,.71*k),(-.10*k,.71*k)],-.10*k,.04*k,7,'BODY')
 common_sockets(a,1.025*k); a.finish()

def carrier():
 a=Actor('Carrier','Amber'); a.bone('BODY',(0,0,.34),(0,-.4,.34));
 def hull(name,sections,mat,bone='BODY'):
  # Longitudinal octagonal pressure hull with low roof and armored chines.
  vs=[]
  for y,w,low,high in sections:
   h=high-low
   vs += [(-w*.70,y,low),(-w,y,low+h*.22),(-w,y,low+h*.56),(-w*.65,y,high),(w*.65,y,high),(w,y,low+h*.56),(w,y,low+h*.22),(w*.70,y,low)]
  fs=[tuple(range(7,-1,-1)),tuple(range((len(sections)-1)*8,len(sections)*8))]
  for j in range(len(sections)-1): fs += [(j*8+i,j*8+(i+1)%8,(j+1)*8+(i+1)%8,(j+1)*8+i) for i in range(8)]
  a.mesh(name,vs,fs,mat,bone,.013)
 hull('Enclosed assault hull',[(-1.0,.16,.20,.32),(-.80,.31,.18,.43),(-.30,.39,.18,.59),(.25,.40,.18,.66),(.75,.33,.20,.57),(.92,.25,.22,.43)],3)
 hull('Armored swept roof',[(-.73,.25,.39,.43),(-.22,.31,.54,.61),(.22,.32,.61,.69),(.70,.26,.54,.62),(.80,.21,.42,.48)],7)
 hull('Wedge nose lower armor',[(-1.035,.16,.20,.27),(-.80,.29,.18,.34),(-.49,.33,.20,.39)],2)
 hull('Recessed forward sensor glazing',[(-.71,.17,.43,.435),(-.32,.24,.568,.574),(-.22,.21,.595,.60)],0)
 for s in [-1,1]:
  side='L' if s==-1 else 'R'; a.bone('PROPULSION_'+side,(s*.44,0,.22),(s*.44,-.5,.22),'BODY')
  # Pontoon ringed duct morphology remains integrated under the main hull.
  sections=[(-.78,.06,.13,.22),(-.55,.13,.08,.27),(.33,.15,.08,.31),(.73,.09,.14,.27)]
  before=len(a.parts); hull(side+' enclosed lateral propulsion nacelle',sections,7,'PROPULSION_'+side)
  o=a.parts[-1]
  for v in o.data.vertices: v.co.x+=s*.43
  for y in [-.47,-.02,.43]:
   a.ring(side+' underside propulsion recess',(s*.47,y,.095),.091,.018,.023,0,'PROPULSION_'+side,(0,0,-1),32)
   a.ring(side+' contained hover coil',(s*.47,y,.08),.068,.018,.012,6,'PROPULSION_'+side,(0,0,-1),32)
   a.tube(side+' pod mechanical brace',(s*.31,y,.28),(s*.44,y,.22),.03,mat=2,bone='PROPULSION_'+side)
  for y in [.37,.44,.51,.58,.65]:
   a.tube(side+' roof heat exchanger',(s*.07,y,.645),(s*.22,y,.645),.012,mat=0)
  # Independent swept cheek armor, access ribs and recessed drive coverings
  # break the long hull into believable serviceable construction.
  a.swept_beam(side+' swept roof armor rail',[(s*.17,-.74,.43),(s*.25,-.34,.60),(s*.26,.16,.69),(s*.23,.61,.61)],[.045,.055,.055,.039],.023,'BODY',3)
  a.panel(side+' aft lateral drive cover',[(s*.37,.30),(s*.40,.49),(s*.31,.57),(s*.24,.49),(s*.24,.32)],.54,.17,7,'BODY',.014,.005)
  for y in [-.39,-.06,.24]:
   a.tube(side+' propulsion nacelle seam',(s*.48,y,.27),(s*.57,y,.21),.009,mat=0,bone='PROPULSION_'+side)
  a.panel(side+' forward hull armor cheek',[(s*.13,.24),(s*.32,.30),(s*.31,.39),(s*.18,.36)],-.81,.07,3,'BODY',.01)
  a.swept_beam(side+' recessed forward chine',[(s*.10,-1.03,.27),(s*.21,-.83,.35),(s*.29,-.62,.46),(s*.34,-.36,.48)],[.024,.033,.041,.030],.018,'BODY',2)
  for y in [.28,.43,.58]:
   a.ring(side+' lateral drive access bearing',(s*.395,y,.40),.046,.008,.018,2,'BODY',(s,0,0),24)
   a.tube(side+' access core stud',(s*.4,y,.40),(s*.421,y,.40),.012,mat=0,bone='BODY',steps=8)
  a.tube(side+' front hostile position lamp',(s*.20,-.849,.305),(s*.20,-.849,.37),.01,mat=6)
  a.tube(side+' shoulder seam', (s*.34,-.35,.48),(s*.35,.32,.54),.006,mat=0)
  a.tube(side+' reinforced side cooling rail',(s*.395,-.45,.32),(s*.395,.55,.39),.019,mat=5)
  a.socket('PropulsionVFX_'+side,(s*.47,.47,.1),'PROPULSION_'+side)
 hull('Enclosed rear service spine',[(.30,.19,.63,.70),(.53,.19,.61,.70),(.70,.15,.55,.62)],3)
 for y in [.34,.41,.48]:
  a.tube('Rear coupling rib',(-.14,y,.711),(.14,y,.711),.013,mat=2)
 a.socket('AttackOrigin',(0,-.93,.3)); a.socket('ForwardReference',(0,-1,.22)); common_sockets(a,.72); a.finish()

def supports(a,heavy=False,boss=False):
 # Four support chains; architecture changes independently for elite/boss.
 if boss: coords=[(-.71,-.55),(.71,-.55),(-.77,.56),(.77,.56)]; z=1.8; spread=1.90; knee_z=1.0
 elif heavy: coords=[(-.42,-.35),(.42,-.35),(-.43,.40),(.43,.40)]; z=1.6; spread=1.30; knee_z=.87
 else: coords=[(-.31,-.28),(.31,-.28),(-.32,.28),(.32,.28)]; z=.57; spread=.77; knee_z=.31
 for i,(x,y) in enumerate(coords):
  s=1 if x>0 else -1; sy=1 if y>0 else -1; hip=(x,y,z); knee=(s*spread*.84,sy*(.67 if heavy or boss else .50),knee_z); foot=(s*spread,sy*(1.13 if boss else .84 if heavy else .72),.04)
  a.bone('SUPPORT_'+str(i),hip,knee,'BODY'); a.bone('KNEE_'+str(i),knee,foot,'SUPPORT_'+str(i)); a.bone('CONTACT_'+str(i),foot,(foot[0],foot[1]-.12,.02),'KNEE_'+str(i))
  w=.215 if boss else .152 if heavy else .072
  a.joint('Support '+str(i)+' root',hip,w*1.4,'BODY'); a.joint('Support '+str(i)+' knuckle',knee,w*1.2,'SUPPORT_'+str(i))
  a.limb('Support '+str(i)+' armored upper load path',hip,knee,w,'SUPPORT_'+str(i),3 if boss else 1)
  a.limb('Support '+str(i)+' tibial shield',knee,foot,w*.79,'KNEE_'+str(i),3 if boss else 1)
  if heavy or boss:
   a.armored_rail('Support '+str(i)+' heavy upper segmented armor',hip,knee,w*1.24,'SUPPORT_'+str(i),3 if boss else 1)
   a.armored_rail('Support '+str(i)+' heavy lower segmented armor',knee,foot,w*1.05,'KNEE_'+str(i),7 if boss else 1)
  a.actuator('Support '+str(i)+' weight cylinder',Vector(hip)+Vector((s*w,-w,0)),Vector(knee)+Vector((s*w,-w,w*.5)),w*.26,'SUPPORT_'+str(i))
  if heavy or boss:
   # Broad split contact shoe supplies mass and load spread, not a needle foot.
   a.loft('Support '+str(i)+' plated contact',[(.02,foot[0],foot[1]-.08,w*.85,w*1.1),(.09,foot[0],foot[1]-.05,w,w*1.2),(.20,foot[0],foot[1],w*.44,w*.60)],7,'CONTACT_'+str(i),.009)
   a.tube('Support '+str(i)+' field conduit',Vector(knee)+Vector((0,-w*.8,.03)),Vector(foot)+Vector((0,-w*.6,.21)),w*.085,mat=6,bone='KNEE_'+str(i))
   for position in [.31,.61]:
    point=Vector(hip).lerp(Vector(knee),position)
    a.ring('Support '+str(i)+' upper service bearing '+str(position),point+Vector((0,-w*.80,0)),w*.36,w*.06,w*.06,2,'SUPPORT_'+str(i),(0,-1,0),20)
   a.actuator('Support '+str(i)+' opposed suspension ram',Vector(knee)+Vector((s*w*.6,w*.5,.11)),Vector(foot)+Vector((s*w*.2,w*.5,.23)),w*.27,'KNEE_'+str(i))
  else:
   a.armored_rail('Support '+str(i)+' predator load armor',hip,knee,w*1.12,'SUPPORT_'+str(i),3)
   a.armored_rail('Support '+str(i)+' forward tibial armor',knee,foot,w,'KNEE_'+str(i),7)
   a.blade('Support '+str(i)+' ground spur',Vector(foot)+Vector((0,0,.20)),foot,w*.66,'KNEE_'+str(i),False)
  a.socket('Contact_'+str(i),foot,'CONTACT_'+str(i))

def cutter():
 a=Actor('Cutter','Red'); a.bone('BODY',(0,.06,.55),(0,-.35,.63)); supports(a)
 a.loft('Predator pressure shell',[(.46,0,.13,.32,.44),(.61,0,.08,.39,.47),(.76,0,.06,.31,.42),(.81,0,.08,.20,.28)],0)
 for s in [-1,1]:
  side='L' if s==-1 else 'R'
  a.panel(side+' forward split carapace',[(s*.04,.66),(s*.16,.51),(s*.34,.55),(s*.39,.71),(s*.29,.81),(s*.06,.80)],-.33,.15,3,'BODY',.035)
  a.bone('TOOL_'+side,(s*.32,-.29,.73),(s*.49,-.67,.52),'BODY')
  a.joint(side+' cutting drive',(s*.33,-.31,.73),.09,'TOOL_'+side)
  a.limb(side+' cutting drive housing',(s*.34,-.35,.73),(s*.5,-.68,.54),.09,'TOOL_'+side,3)
  a.armored_rail(side+' layered cutter drive',(s*.34,-.35,.73),(s*.5,-.68,.54),.105,'TOOL_'+side,3)
  a.blade(side+' dominant forward cutting assembly',(s*.5,-.67,.56),(s*.52,-1.18,.08),.12,'TOOL_'+side)
  a.actuator(side+' tool control piston',(s*.21,-.28,.65),(s*.45,-.61,.53),.036,'TOOL_'+side)
  a.socket('AttackOrigin_'+side,(s*.48,-.72,.47),'TOOL_'+side); a.socket('CutterTip_'+side,(s*.52,-1.18,.08),'TOOL_'+side)
  for y in [.13,.25,.37]:
   a.tube(side+' rear heat sink',(s*.10,y,.83),(s*.26,y,.78),.017,mat=7)
  a.panel(side+' layered dorsal predator shell',[(s*.04,.82),(s*.10,.88),(s*.25,.84),(s*.34,.76),(s*.28,.67),(s*.09,.73)],.06,.31,3,'BODY',.024,.005)
 a.core('Frontal cutting power cell',(0,-.385,.64),.11)
 a.panel('Top armored spine',[(-.09,.70),(-.08,.86),(.08,.86),(.09,.70)],.15,.32,3,'BODY',.02)
 common_sockets(a,.90); a.finish()

def arc():
 a=Actor('ArcDrone','Blue'); a.bone('BODY',(0,0,.90),(0,0,1.1)); a.bone('EMITTER',(0,-.12,.94),(0,-.22,.94),'BODY')
 a.loft('Aerial contained emitter body',[(.66,0,.01,.09,.10),(.76,0,.04,.24,.17),(.99,0,.04,.28,.19),(1.19,0,.04,.18,.12),(1.24,0,.02,.11,.08)],0)
 a.core('Central ranged emitter',(0,-.16,.96),.23,'EMITTER')
 for s in [-1,1]:
  side='L' if s==-1 else 'R'; a.bone('WING_'+side,(s*.20,.02,1.03),(s*.55,.08,1.13),'BODY')
  a.limb(side+' airborne outrigger',(s*.20,.02,1.03),(s*.56,.08,1.13),.08,'WING_'+side,1)
  a.ring(side+' vectored nozzle',(s*.50,-.01,1.09),.095,.025,.06,0,'WING_'+side,(0,-1,-.2),32)
  a.ring(side+' blue nozzle throat',(s*.50,-.035,1.085),.060,.014,.015,6,'WING_'+side,(0,-1,-.2),32)
  a.panel(side+' swept wing armor',[(s*.26,1.12),(s*.53,1.22),(s*.65,1.23),(s*.60,1.10),(s*.35,1.03)],.055,.09,1,'WING_'+side,ridge=.014)
  # Hover vanes terminate well clear of the floor; no joints or contact feet.
  a.bone('VANE_'+side,(s*.18,.02,.75),(s*.42,-.03,.38),'BODY')
  a.blade(side+' descending stabilizer vane',(s*.18,.02,.75),(s*.43,-.04,.32),.061,'VANE_'+side,False)
  a.tube(side+' vane blue inlay',(s*.24,-.016,.66),(s*.41,-.055,.38),.009,mat=6,bone='VANE_'+side)
  a.socket('FlightVFX_'+side,(s*.5,.06,1.03),'WING_'+side)
 a.bone('REAR_VANE',(0,.15,.75),(0,.38,.40),'BODY'); a.blade('Rear airborne stabilizer',(0,.15,.75),(0,.38,.40),.055,'REAR_VANE',False)
 a.ring('Upper induction crown',(0,.035,1.20),.11,.02,.035,2,'BODY',(0,0,1),32)
 a.tube('Upper antenna cell',(0,.035,1.20),(0,.035,1.31),.021,mat=6)
 a.socket('ProjectileSource',(0,-.34,.96),'EMITTER'); a.socket('AttackOrigin',(0,-.34,.96),'EMITTER'); common_sockets(a,1.31); a.finish()

def magnetar():
 a=Actor('Magnetar','Amber'); a.bone('BODY',(0,0,1.63),(0,0,2.1)); supports(a,True)
 a.bone('REACTOR',(0,-.10,1.80),(0,-.16,2.1),'BODY')
 a.loft('Magnetic containment vessel',[(1.32,0,.02,.26,.24),(1.51,0,.02,.46,.38),(1.95,0,.08,.48,.38),(2.26,0,.06,.32,.26),(2.42,0,.06,.22,.18)],0,'REACTOR',.012)
 a.core('Contained magnetic field aperture',(0,-.31,1.90),.38,'REACTOR',True)
 for s in [-1,1]:
  side='L' if s==-1 else 'R'; a.bone('CAGE_'+side,(s*.38,0,1.78),(s*.69,-.02,2.28),'BODY')
  # Twin containment buttresses carry the core; large architecture, not a ball on legs.
  a.panel(side+' load bearing reactor yoke',[(s*.26,1.49),(s*.39,1.41),(s*.65,1.54),(s*.74,2.13),(s*.47,2.38),(s*.27,2.29)],-.04,.22,1,'CAGE_'+side,.048,.013)
  a.panel(side+' yoke recessed armor landing',[(s*.44,1.68),(s*.62,1.73),(s*.63,2.02),(s*.45,2.17),(s*.39,2.11)],-.095,.024,7,'CAGE_'+side,.008)
  a.actuator(side+' containment tension ram',(s*.39,.07,1.42),(s*.59,.04,2.16),.056,'CAGE_'+side)
  a.tube(side+' core power feed',(s*.28,-.31,1.57),(s*.52,-.21,1.53),.038,mat=5,bone='REACTOR')
  a.bone('TOOL_'+side,(s*.69,-.10,2.02),(s*.91,-.36,1.44),'CAGE_'+side)
  a.limb(side+' magnetic actuator arm',(s*.69,-.10,2.02),(s*.91,-.36,1.44),.115,'TOOL_'+side,7)
  a.armored_rail(side+' magnetic manipulator shielding',(s*.69,-.10,2.02),(s*.91,-.36,1.44),.13,'TOOL_'+side,7)
  a.ring(side+' induction release terminal',(s*.92,-.40,1.43),.12,.034,.08,2,'TOOL_'+side,(0,-1,0),32)
  a.ring(side+' terminal field coil',(s*.92,-.44,1.43),.08,.02,.018,6,'TOOL_'+side,(0,-1,0),32)
  a.socket('FieldOrigin_'+side,(s*.92,-.46,1.43),'TOOL_'+side)
  for z in [1.72,1.84,1.96,2.08]: a.tube(side+' rear cooling rib',(s*.22,.41,z),(s*.39,.40,z),.025,mat=7,bone='REACTOR')
  a.panel(side+' independent upper containment manifold',[(s*.18,2.26),(s*.35,2.51),(s*.53,2.48),(s*.66,2.24),(s*.42,2.19)],.04,.22,7,'CAGE_'+side,.036,.009)
  a.swept_beam(side+' external magnetic buttress',[(s*.32,-.04,1.39),(s*.62,-.05,1.56),(s*.70,-.01,2.14),(s*.46,.03,2.48)],[.09,.125,.13,.085],.075,'CAGE_'+side,7)
  for z in [1.64,1.80,1.96,2.12]:
   a.panel(side+' yoke recessed cooling land '+str(z),[(s*.49,z),(s*.62,z+.03),(s*.62,z+.075),(s*.49,z+.048)],-.145,.03,0,'CAGE_'+side,.003,.003)
  a.actuator(side+' reactor restraint ram',(s*.25,.23,1.39),(s*.51,.22,2.31),.058,'CAGE_'+side)
 a.ring('Upper flux containment crown',(0,.055,2.42),.23,.038,.07,2,'REACTOR',(0,0,1),48)
 a.ring('Upper amber contained coil',(0,.055,2.46),.17,.024,.02,6,'REACTOR',(0,0,1),40)
 a.socket('MagneticCore',(0,-.38,1.90),'REACTOR'); a.socket('AttackOrigin',(0,-.38,1.90),'REACTOR'); common_sockets(a,2.48); a.finish()

def custodian():
 a=Actor('Custodian','Red'); a.bone('BODY',(0,0,1.81),(0,0,2.25)); supports(a,True,True)
 a.bone('REACTOR',(0,0,2.28),(0,0,3.0),'BODY')
 # Tall armored reactor vault differentiates boss from elite's exposed yoke.
 a.loft('Governing reactor pressure vault',[(1.46,0,.10,.41,.35),(1.75,0,.09,.64,.48),(2.43,0,.09,.66,.47),(2.82,0,.12,.43,.34),(3.17,0,.10,.25,.22)],0,'REACTOR',.017)
 a.core('Boss frontal reactor containment',(0,-.38,2.22),.49,'REACTOR',True)
 a.loft('Vertical reactor chimney',[(2.64,0,.02,.19,.19),(2.86,0,.04,.25,.21),(3.22,0,.04,.22,.19),(3.37,0,.04,.15,.14)],7,'REACTOR',.011)
 a.tube('Vertical red reactor window',(0,-.19,2.75),(0,-.17,3.28),.045,mat=6,bone='REACTOR')
 for s in [-1,1]:
  side='L' if s==-1 else 'R'; a.bone('CROWN_'+side,(s*.48,.02,2.42),(s*1.13,-.02,2.53),'BODY')
  # Independent bulky attack mantles with articulated ribs and real pivot axes.
  a.joint(side+' major mantle root',(s*.65,.05,2.43),.24,'BODY')
  a.panel(side+' boss articulated mantle',[(s*.55,2.21),(s*.89,2.06),(s*1.48,2.18),(s*1.62,2.60),(s*1.34,2.91),(s*.89,2.95),(s*.58,2.64)],-.03,.40,3,'CROWN_'+side,.085,.018)
  a.swept_beam(side+' reactor cathedral arch',[(s*.45,.05,2.48),(s*.65,.03,2.99),(s*.93,.05,3.27),(s*1.28,.12,3.15),(s*1.56,.18,2.61)],[.12,.14,.16,.17,.19],.13,'CROWN_'+side,3)
  a.swept_beam(side+' arch inset thermal structure',[(s*.59,-.09,2.73),(s*.78,-.09,3.08),(s*.97,-.08,3.20),(s*1.26,-.03,3.08)],[.04,.04,.045,.05],.028,'CROWN_'+side,7)
  a.tube(side+' arch local telegraph channel',(s*.76,-.11,3.00),(s*.95,-.10,3.14),.018,mat=6,bone='CROWN_'+side)
  a.panel(side+' mantle layered armor',[(s*.77,2.32),(s*1.24,2.26),(s*1.46,2.60),(s*1.23,2.79),(s*.89,2.81)],-.14,.07,7,'CROWN_'+side,.026,.012)
  for zz in [2.37,2.48,2.59,2.70]:
   a.panel(side+' mantle thermal shutter '+str(zz),[(s*.94,zz),(s*1.24,zz+.026),(s*1.27,zz+.085),(s*.95,zz+.058)],-.196,.042,3,'CROWN_'+side,.005,.004)
  a.swept_beam(side+' mantle outboard stepped armor',[(s*1.17,-.02,2.88),(s*1.46,-.01,2.68),(s*1.55,.03,2.34),(s*1.33,.07,2.08)],[.12,.17,.145,.1],.10,'CROWN_'+side,3)
  a.panel(side+' reactor cheek armor',[(s*.31,1.72),(s*.56,1.64),(s*.78,1.92),(s*.77,2.31),(s*.51,2.64),(s*.38,2.55)],-.26,.14,3,'REACTOR',.04,.012)
  a.actuator(side+' mantle elevator',(s*.53,.34,1.99),(s*1.13,.30,2.60),.079,'CROWN_'+side)
  a.bone('TOOL_'+side,(s*1.18,-.06,2.29),(s*1.58,-.29,1.56),'CROWN_'+side)
  a.limb(side+' boss telegraph actuator',(s*1.18,-.06,2.29),(s*1.58,-.29,1.56),.21,'TOOL_'+side,3)
  a.panel(side+' telegraph energy routing',[(s*1.4,1.67),(s*1.53,1.65),(s*1.42,2.04),(s*1.28,2.14)],-.39,.032,6,'TOOL_'+side,.003,.003)
  a.ring(side+' boss discharge throat',(s*1.58,-.40,1.55),.17,.043,.09,2,'TOOL_'+side,(0,-1,-.2),40)
  a.ring(side+' contained discharge lens',(s*1.58,-.44,1.54),.113,.029,.02,6,'TOOL_'+side,(0,-1,-.2),32)
  for z in [2.26,2.47,2.68]:
   a.tube(side+' mantle service bolt',(s*1.11,-.181,z),(s*1.11,-.20,z),.043,mat=2,bone='CROWN_'+side,steps=6)
  a.socket('BossDischarge_'+side,(s*1.58,-.47,1.54),'TOOL_'+side); a.socket('MantlePivot_'+side,(s*.65,.05,2.43),'CROWN_'+side)
  a.tube(side+' rear reactor conduit',(s*.36,.48,1.77),(s*.26,.33,3.06),.049,mat=5,bone='REACTOR')
  a.actuator(side+' reactor vault structural damper',(s*.49,.42,1.44),(s*.36,.36,2.78),.09,'REACTOR')
  for zz in [2.89,3.05,3.20]:
   a.ring(side+' chimney axial reinforcement '+str(zz),(s*.03,.04,zz),.23 if zz<3.1 else .20,.024,.045,0,'REACTOR',(0,0,1),40)
 a.ring('Chimney armored cap',(0,.045,3.36),.17,.032,.08,2,'REACTOR',(0,0,1),48)
 a.ring('Contained reactor vent',(0,.045,3.40),.12,.026,.014,6,'REACTOR',(0,0,1),40)
 a.panel('Boss ventral reactor armor',[(-.35,1.71),(-.24,1.36),(0,1.24),(.24,1.36),(.35,1.71)],-.29,.16,3,'BODY',.055,.012)
 a.core('Auxiliary ventral reactor',(0,-.36,1.52),.13,'BODY')
 a.socket('BossReactor',(0,-.47,2.22),'REACTOR'); a.socket('AttackOrigin',(0,-.47,2.22),'REACTOR'); a.socket('BossVent',(0,.045,3.43),'REACTOR'); common_sockets(a,3.44); a.finish()

if __name__=='__main__':
 p=argparse.ArgumentParser(); p.add_argument('--actors',nargs='+',default=['Scout','Warden','Carrier','Cutter','ArcDrone','Magnetar','Custodian']); p.add_argument('--atlas-only',action='store_true'); p.add_argument('--no-render',action='store_true')
 args=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
 RENDER_IMAGES=not args.no_render
 if not (IMPORT/'Textures/HostileV2_BaseColor.png').exists() or args.atlas_only: atlas()
 if not args.atlas_only:
  builders={'Scout':lambda:biped('Scout'),'Warden':lambda:biped('Warden',True),'Carrier':carrier,'Cutter':cutter,'ArcDrone':arc,'Magnetar':magnetar,'Custodian':custodian}
  for name in args.actors: builders[name]()
