import bpy, math
from mathutils import Vector

def material(name, color, metal=.6, rough=.34, emission=0):
    m = bpy.data.materials.new(name); m.use_nodes = True
    p = next(n for n in m.node_tree.nodes if n.type == 'BSDF_PRINCIPLED')
    p.inputs['Base Color'].default_value = (*color, 1)
    p.inputs['Metallic'].default_value = metal
    p.inputs['Roughness'].default_value = rough
    if emission:
        p.inputs['Emission Color'].default_value = (*color, 1)
        p.inputs['Emission Strength'].default_value = emission
    m.diffuse_color = (*color, 1)
    return m

def aim(o, target): o.rotation_euler = (Vector(target) - o.location).to_track_quat('-Z', 'Y').to_euler()

def setup_studio():
    scene = bpy.context.scene
    scene.render.engine = 'CYCLES'
    scene.cycles.samples = 32
    scene.cycles.use_denoising = True
    scene.render.resolution_x = 1200; scene.render.resolution_y = 1400
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = 'PNG'
    scene.world = bpy.data.worlds.new('G0_Studio_World')
    scene.world.use_nodes = True
    bg=next(n for n in scene.world.node_tree.nodes if n.type=='BACKGROUND')
    bg.inputs[0].default_value = (.11,.14,.19,1)
    bg.inputs[1].default_value = .4
    scene.view_settings.view_transform = 'AgX'
    for name, loc, power, size, col in [
        ('Key_softbox', (3,-4,7), 1000, 5, (.78,.87,1)),
        ('Fill_softbox', (-4,-2,4), 700, 4, (.64,.77,1)),
        ('Rear_rim', (2,4,6), 1500, 3, (.65,.9,1))]:
        d = bpy.data.lights.new(name, 'AREA'); d.energy=power; d.shape='DISK'; d.size=size; d.color=col
        o = bpy.data.objects.new(name,d); scene.collection.objects.link(o); o.location=loc; aim(o,(0,0,1.8))
    bpy.ops.mesh.primitive_plane_add(size=200)
    ground=bpy.context.object; ground.name='Studio_floor'
    ground.data.materials.append(material('Studio_graphite', (.055,.069,.085),.12,.5))
    ground.location.z=-.02
    camd=bpy.data.cameras.new('Review_camera'); cam=bpy.data.objects.new('Review_camera',camd)
    scene.collection.objects.link(cam); scene.camera=cam
    camd.type='ORTHO'; camd.ortho_scale=4.8; camd.lens=46
    return cam, ground

def render(path, pos, target=(0,0,1.85), scale=4.8, resolution=(1200,1400)):
    s=bpy.context.scene; c=s.camera
    c.location=pos; aim(c,target); c.data.ortho_scale=scale
    s.render.resolution_x,s.render.resolution_y=resolution
    s.render.filepath=str(path)
    bpy.ops.render.render(write_still=True)
