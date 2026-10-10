"""Derive major platform edge rails from the authored movement surface union."""
from pathlib import Path
import json
path=Path(__file__).parent/'layout.json'
data=json.loads(path.read_text())
rects=[(v['center']['x']-v['size']['x']/2,v['center']['x']+v['size']['x']/2,
        v['center']['z']-v['size']['z']/2,v['center']['z']+v['size']['z']/2) for v in data['surfaces']]
xs=sorted(set(x for r in rects for x in r[:2]));zs=sorted(set(z for r in rects for z in r[2:]))
def inside(x,z):return any(a<=x<=b and c<=z<=d for a,b,c,d in rects)
cells={(i,j) for i in range(len(xs)-1) for j in range(len(zs)-1) if inside((xs[i]+xs[i+1])/2,(zs[j]+zs[j+1])/2)}
edges={}
for i,j in cells:
    for di,dj,axis,coord,a,b,sign in [(-1,0,'x',xs[i],zs[j],zs[j+1],-1),(1,0,'x',xs[i+1],zs[j],zs[j+1],1),(0,-1,'z',zs[j],xs[i],xs[i+1],-1),(0,1,'z',zs[j+1],xs[i],xs[i+1],1)]:
        if (i+di,j+dj) not in cells:edges.setdefault((axis,coord,sign),[]).append((a,b))
data['blockers']=[b for b in data['blockers'] if not b['id'].startswith('platform-edge/')]
for (axis,coord,sign),segments in sorted(edges.items()):
    merged=[]
    for a,b in sorted(segments):
        if merged and abs(merged[-1][1]-a)<.001:merged[-1]=(merged[-1][0],b)
        else:merged.append((a,b))
    for a,b in merged:
        c=coord+sign*.12
        center={'x':c if axis=='x' else (a+b)/2,'y':1.2,'z':(a+b)/2 if axis=='x' else c}
        size={'x':.24 if axis=='x' else b-a+.1,'y':2.4,'z':b-a+.1 if axis=='x' else .24}
        data['blockers'].append(dict(id=f'platform-edge/{axis}/{coord:g}/{a:g}/{b:g}',center=center,size=size))
path.write_text(json.dumps(data,indent=2)+'\n')
print('layout surfaces',len(data['surfaces']),'architectural/edge blockers',len(data['blockers']))
