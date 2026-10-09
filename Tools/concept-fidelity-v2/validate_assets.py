"""Reopen the actual .blend sources and validate production bounds and rigs."""
import bpy,json,math
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
metrics=json.loads((ROOT/'docs/history/visual-stages/chapter01-visual-passes/concept-fidelity-v2/asset_metrics.json').read_text())
results={}
for name,expected in metrics.items():
    bpy.ops.wm.open_mainfile(filepath=str(ROOT/'art/concept-fidelity-v2'/(name+'.blend')),load_ui=False,use_scripts=False)
    meshes=sorted((o for o in bpy.data.objects if o.type=='MESH'),key=lambda o:o.name)
    assert len(meshes)==len(expected['triangles']),(name,'LOD count')
    assert [len(o.data.polygons) for o in meshes]==expected['triangles'],name
    for ob in meshes:
        assert len(ob.data.materials)==1,(name,'materials')
        assert len(ob.data.uv_layers)==1,(name,'UV atlas')
        assert all(math.isfinite(c) for v in ob.data.vertices for c in v.co),(name,'finite vertices')
        assert all(s>0 for s in ob.scale),(name,'positive scale')
        assert ob.location.length<.0001,(name,'origin pivot')
        assert all(len(p.vertices)==3 for p in ob.data.polygons),(name,'triangulation')
    rigs=[o for o in bpy.data.objects if o.type=='ARMATURE']
    if expected['bones']:
        assert len(rigs)==1 and len(rigs[0].data.bones)==expected['bones'],(name,'rig')
        assert {'ROOT','BODY'}.issubset(rigs[0].data.bones.keys()),(name,'root bones')
        if name=='Custodian_V2':assert {'Idle','Run','Windup','Release','Special','Hit','Death'}.issubset(bpy.data.actions.keys())
        for ob in meshes:
            assert all(len(v.groups)==1 and abs(v.groups[0].weight-1)<.001 for v in ob.data.vertices),(name,'rigid weights')
    assert (ROOT/'Assets/_Game/Content/ConceptFidelityV2/Models'/(name+'.fbx')).stat().st_size>10000,name
    results[name]={'validated':True,'triangles':expected['triangles'],'rigBones':expected['bones'],'source':str(Path('art/concept-fidelity-v2')/(name+'.blend'))}
(ROOT/'docs/history/visual-stages/chapter01-visual-passes/concept-fidelity-v2/source_validation.json').write_text(json.dumps(results,indent=2))
print('CONCEPT_FIDELITY_SOURCE_VALIDATION_PASS')
