from pathlib import Path
import subprocess, json, re
root=Path(__file__).resolve().parents[2]
commit='7dcd6652c0c7232aaaed5e134edbfc5ea1c3c7e2'
out=root/'Tools/actors-combat-concept-world/baseline'
out.mkdir(parents=True,exist_ok=True)
for name,rel in {
    'HostileBase.png':'Assets/_Game/Content/ActorProductionV2/Textures/HostileV2_BaseColor.png',
    'G0Base.png':'Assets/_Game/ArtReview/G0ProductionV3/Textures/G0_V3_BaseColor.png'
}.items():
    (out/name).write_bytes(subprocess.check_output(['git','show',commit+':'+rel],cwd=root))
records=[]
for name in ['HostileV2_Red','HostileV2_Blue','HostileV2_Amber','G0_V3_Tier0','G0_V3_Tier1','G0_V3_Tier2']:
    folder='ActorProductionV2' if name.startswith('Hostile') else 'VisualSlice'
    text=subprocess.check_output(['git','show',commit+':Assets/_Game/Content/'+folder+'/Materials/'+name+'.mat'],cwd=root).decode()
    entry={'name':name}
    for key in ['_BumpScale','_Smoothness','_OcclusionStrength']:
        entry[key.strip('_')[0].lower()+key.strip('_')[1:]]=float(re.search(r'- '+key+r': ([\d.]+)',text).group(1))
    for key,label in [('_BaseColor','baseColor'),('_EmissionColor','emission')]:
        vals=re.search(r'- '+key+r': \{r: ([\d.]+), g: ([\d.]+), b: ([\d.]+), a: ([\d.]+)\}',text).groups()
        entry[label]=dict(zip('rgba',map(float,vals)))
    records.append(entry)
(out/'original-material-values.json').write_text(json.dumps({'sourceCommit':commit,'materials':records},indent=2))
print('Original APK53 actor surfaces copied for in-memory matched sRGB comparison; no runtime dependency.')
