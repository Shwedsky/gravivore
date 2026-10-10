"""One-time migration of explicit architecture footprints to portrait courts."""
import json
from pathlib import Path
path=Path(__file__).parent/'layout.json'
data=json.loads(path.read_text())
if not data.get('portraitCourtsApplied'):
    groups=[('hub-',0,-30,.70,.90,.85),('relay-',0,-10,.75,.85,.85),
            ('capacitor-',-15,10,.77,.74,.85),('cutting-',15,10,.77,.74,.85),
            ('shield-',-15,32,.77,.74,.85),('hauler-',15,32,.77,.74,.85)]
    for volume in data['blockers']:
        for prefix,x,z,sx,sy,sz in groups:
            if not volume['id'].startswith(prefix):continue
            c=volume['center'];s=volume['size']
            if 'rear-' in volume['id'] and prefix!='hub-':s['z']=4.5
            c['x']=round(x+(c['x']-x)*sx,4);c['z']=round(z+(c['z']-z)*sy,4);c['y']*=sz
            s['x']*=sx;s['z']*=sy;s['y']*=sz
    data['portraitCourtsApplied']=True
    path.write_text(json.dumps(data,indent=2)+'\n')
print('Portrait collision footprints ready')
