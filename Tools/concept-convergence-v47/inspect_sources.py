import bpy, json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
rows=[]
for name in ['Scout_V1','Cutter_V1','Warden_V1','ArcDrone_V1','Carrier_V1','Magnetar_V1','Custodian_V3']:
    bpy.ops.wm.open_mainfile(filepath=str(ROOT/'art/visual-replacement-v3'/f'{name}.blend'))
    rig=next(o for o in bpy.data.objects if o.type=='ARMATURE')
    rows.append({'name':name,'rig':rig.name,'bones':{b.name:list(b.head_local) for b in rig.data.bones},'meshes':[{ 'name':o.name,'triangles':len(o.data.polygons),'min':[min(v.co[i] for v in o.data.vertices) for i in range(3)],'max':[max(v.co[i] for v in o.data.vertices) for i in range(3)]} for o in bpy.data.objects if o.type=='MESH'],'actions':[a.name for a in bpy.data.actions]})
out=ROOT/'docs/history/implementation-passes/chapter01-visual-replacement-v3/v47/source_inspection.json'
out.write_text(json.dumps(rows,indent=2))
print(json.dumps(rows))
