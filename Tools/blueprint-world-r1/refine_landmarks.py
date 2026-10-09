"""Keep the final visible landmark footprints aligned with authored meshes."""
import json
from pathlib import Path
path=Path(__file__).parent/'layout.json'
data=json.loads(path.read_text())
if data['strongSpotPositions'][0]['z']==49:
    data['strongSpotPositions'][0]=dict(x=-7.5,y=0,z=38.5)
    data['route']=data['route'][:12]+[dict(x=-7.5,y=0,z=32),dict(x=-7.5,y=0,z=38.5),dict(x=-7.5,y=0,z=32),dict(x=0,y=0,z=32),dict(x=0,y=0,z=54)]+data['route'][14:]
data['route']=[point for i,point in enumerate(data['route']) if i==0 or point!=data['route'][i-1]]
data['eliteEncounterPosition']=dict(x=0,y=0,z=53.5)
for volume in data['blockers']:
    if volume['id']=='magnetar-reactor-footprint':
        volume['center']['x']=-5;volume['size']['x']=8.6
for side in [-1,1]:
    name=f'relay-north-assembly-jamb-{side}'
    if not any(v['id']==name for v in data['blockers']):
        data['blockers'].append(dict(id=name,center=dict(x=side*4.7,y=1.7,z=-5.5),size=dict(x=1.3,y=3.4,z=2.8)))
path.write_text(json.dumps(data,indent=2)+'\n')
