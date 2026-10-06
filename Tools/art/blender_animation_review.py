"""Sample the actual animated Scout donor before judging its clip value."""
import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent))
from blender_source_review import load, render_neutral, SCRATCH, IMAGES
import bpy
import json

path = next((SCRATCH/'scout/spider-robot').rglob('*.blend'))
load(path)
rig = next(o for o in bpy.context.scene.objects if o.type == 'ARMATURE')
result = []
for name, frames in [('Idle',[1,20]),('Walk',[1,9]),('Fire',[1,4]),('BigBoom',[1,45])]:
    action = bpy.data.actions[name]
    rig.animation_data_create()
    rig.animation_data.action = action
    if action.slots:
        rig.animation_data.action_slot = action.slots[0]
    for frame in frames:
        bpy.context.scene.frame_set(frame)
        render_neutral(IMAGES/f'Scout_animation_{name}_{frame}.png', size=450)
        result.append(dict(action=name, frame=frame, rig_scale=list(rig.scale),
            pose=[dict(bone=b.name, matrix=[list(row) for row in b.matrix]) for b in rig.pose.bones]))
(IMAGES.parent/'data/scout_animation_samples.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
