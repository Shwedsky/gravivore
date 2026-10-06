"""Convert downloaded CC0 PBR maps to bounded, linear-data production PNGs."""
import bpy
import json
import hashlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / '.asset-intake-tmp/materials'
OUT = ROOT / 'art/visual-production-v2/materials'
DATA = ROOT / 'docs/visual-production-v2/modeling/data'
records = []
for folder in sorted(SOURCE.iterdir()):
    blend = next(folder.glob('*.blend'))
    bpy.ops.wm.open_mainfile(filepath=str(blend), load_ui=False, use_scripts=False)
    source_materials = [dict(name=m.name, nodes=[n.bl_idname for n in m.node_tree.nodes])
                        for m in bpy.data.materials if m.use_nodes]
    name = folder.name.replace('_2k.blend','')
    dest = OUT / name
    dest.mkdir(parents=True, exist_ok=True)
    record = dict(name=name, source_blend=str(blend.relative_to(ROOT)), source_materials=source_materials, maps=[])
    for channel, token in [('basecolor','_diff_'), ('normal_gl','_nor_gl_'), ('roughness','_rough_'), ('metallic','_metal_')]:
        matches = [p for p in folder.rglob('*') if (name+token) in p.name and p.suffix.lower() in ('.jpg','.png','.exr') and p.is_file()]
        if not matches:
            continue
        source = matches[0]
        img = bpy.data.images.load(str(source), check_existing=False)
        img.colorspace_settings.name = 'sRGB' if channel == 'basecolor' else 'Non-Color'
        original_size = list(img.size)
        if max(img.size) > 2048:
            ratio = 2048/max(img.size)
            img.scale(int(img.size[0]*ratio), int(img.size[1]*ratio))
        target = dest / (name+'_'+channel+'_2k.png')
        settings = bpy.context.scene.render.image_settings
        settings.file_format = 'PNG'
        settings.color_mode = 'RGB' if channel in ('basecolor','normal_gl') else 'BW'
        settings.color_depth = '8'
        if channel == 'basecolor':
            # save() encodes scene-linear pixels back into the image's sRGB colorspace.
            img.filepath_raw = str(target)
            img.file_format = 'PNG'
            img.save()
        else:
            bpy.context.scene.view_settings.view_transform = 'Raw'
            bpy.context.scene.view_settings.look = 'None'
            bpy.context.scene.view_settings.exposure = 0
            bpy.context.scene.view_settings.gamma = 1
            img.save_render(str(target), scene=bpy.context.scene)
        record['maps'].append(dict(channel=channel, source=str(source.relative_to(ROOT)).replace('\\','/'),
                                  output=str(target.relative_to(ROOT)).replace('\\','/'),
                                  source_size=original_size, size=list(img.size),
                                  source_sha256=hashlib.file_digest(source.open('rb'),'sha256').hexdigest(),
                                  sha256=hashlib.file_digest(target.open('rb'),'sha256').hexdigest(),
                                  bytes=target.stat().st_size, color_space='sRGB' if channel=='basecolor' else 'linear',
                                  bit_depth=8))
    record['omitted'] = [p.name for p in folder.rglob('*') if p.is_file() and '_disp_' in p.name]
    records.append(record)
    print('PREPARED', name, [m['channel'] for m in record['maps']], flush=True)
(DATA/'material_prep.json').write_text(json.dumps(records, indent=2), encoding='utf-8')
