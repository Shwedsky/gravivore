from pathlib import Path
from PIL import Image
import numpy as np, json
root=Path(__file__).resolve().parents[2]
folder=root/'Assets/_Game/Content/ActorProductionV2/Textures'
data={}
for path in folder.glob('*.png'):
    a=np.asarray(Image.open(path).convert('RGBA'))
    data[path.name]=[a[512*(1-row):512*(2-row),256*col:256*(col+1)].mean(axis=(0,1)).tolist() for row in range(2) for col in range(4)]
out=root/'docs/history/implementation-passes/actors-combat-concept-world/verification'
(out/'texture-channel-diagnosis.json').write_text(json.dumps(data,indent=2))
print(json.dumps(data,indent=2))
