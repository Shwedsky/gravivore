"""Use Blender's existing FFmpeg encoder; no added package or download."""
import bpy
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'docs/history/implementation-passes/capacitor-field-proof'
frames=sorted((OUT/'video-frames').glob('frame_*.png'))
if not frames:raise RuntimeError('Actual gameplay recording frames missing')
scene=bpy.context.scene;scene.sequence_editor_create();scene.render.resolution_x=540;scene.render.resolution_y=960;scene.render.resolution_percentage=100
scene.render.fps=10;scene.frame_start=1;scene.frame_end=len(frames)
strip=scene.sequence_editor.strips.new_image('Uninterrupted production gameplay',filepath=str(frames[0]),channel=1,frame_start=1)
for frame in frames[1:]:strip.elements.append(frame.name)
strip.frame_final_duration=len(frames)
scene.view_settings.view_transform='Standard';scene.view_settings.look='None';scene.view_settings.exposure=0;scene.view_settings.gamma=1
scene.render.image_settings.media_type='VIDEO';scene.render.image_settings.file_format='FFMPEG';scene.render.ffmpeg.format='MPEG4';scene.render.ffmpeg.codec='H264';scene.render.ffmpeg.constant_rate_factor='MEDIUM';scene.render.ffmpeg.audio_codec='NONE'
scene.render.filepath=str(OUT/'capacitor-field-continuous.mp4');scene.render.use_sequencer=True
bpy.ops.render.render(animation=True)
print('CF_CONTINUOUS_VIDEO_ENCODED',len(frames))
