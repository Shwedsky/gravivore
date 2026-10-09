import bpy,json
from pathlib import Path
root=Path(__file__).resolve().parents[2]
entries=json.loads((root/'docs/visual-replacement-v3/ingestion_manifest.json').read_text())
for e in entries:
 if e['sourceId']!='env-quaternius-megakit':continue
 bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.fbx(filepath=str(root/e['sourcePath']))
 print('DONOR',e['outputName'],[(o.name,tuple(o.dimensions),[m.name for m in o.data.materials]) for o in bpy.context.scene.objects if o.type=='MESH'])
