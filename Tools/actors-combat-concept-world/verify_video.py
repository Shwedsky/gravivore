"""Decode the actual delivered video; retain first, middle and last frame for review."""
import bpy, json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'docs/history/implementation-passes/actors-combat-concept-world'
scene=bpy.context.scene;scene.sequence_editor_create()
strip=scene.sequence_editor.strips.new_movie('Delivered actor gameplay MP4',filepath=str(OUT/'actors-combat-continuous.mp4'),channel=1,frame_start=1)
scene.render.resolution_x=540;scene.render.resolution_y=960;scene.render.resolution_percentage=100
scene.render.fps=10;scene.render.use_sequencer=True
scene.view_settings.view_transform='Standard';scene.view_settings.look='None';scene.view_settings.exposure=0;scene.view_settings.gamma=1
scene.render.image_settings.file_format='PNG'
frames=[1,int(strip.frame_duration/2),int(strip.frame_duration)]
for frame in frames:
    scene.frame_set(frame);scene.render.filepath=str(OUT/'video-stills'/f'decoded-{frame:04d}.png')
    bpy.ops.render.render(write_still=True)
record=dict(decodedFrames=int(strip.frame_duration),width=540,height=960,fps=10,verifiedFrames=frames,
            method='Delivered MP4 decoded by Blender 5.2; first, middle and last frame rendered for visual review.')
assert record['decodedFrames']==json.loads((OUT/'verification/video.json').read_text())['frames']
(OUT/'verification/video-decode.json').write_text(json.dumps(record,indent=2),encoding='utf-8')
print(json.dumps(record))
