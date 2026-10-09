# Presentation audit and original audio

The isolated severe device stall has not reproduced in the editor. No claim is
made that its root cause has been fixed or that device FPS has been measured.

The audit covered the lash/Phase6B VFX and LineRenderer pools, eight pooled death
meshes, UI pools, audio voices, streamed music, minimap refresh, material ownership,
subscriptions and lifecycle. Combat creates no new presentation objects. The
64-kill regression checks unchanged Transform, AudioSource and VFX capacity counts,
and expired floating text returns to zero. Existing full-suite repeat encounter,
respawn, scene teardown, telegraph and save tests remain the regression baseline.
S14 audio now rejects repeated initialization before another four-voice pool can
be allocated. This is a lifecycle guard, not a demonstrated cause of the phone stall.

Combat plates cap at six, with overlap suppression and minimap avoidance. Floating
text reserves ten damage and four reward slots. Actor/life bindings, UI objects and
sorting arrays are preallocated; string formatting happens on hits, rewards or
changed health. The minimap already uses cached marker visuals and throttled text
refresh; its existing snapshot work was not rewritten speculatively. Music remains
the same streaming assets and presentation. VFX materials use owned shared assets
or existing disposable materials/property blocks; no new per-hit material cloning
or coroutines were introduced.

DEV/Editor diagnostics retain 64 wall-clock frame intervals. A frame of at least
250 ms triggers a snapshot after a five-second warmup; a 20-second cooldown limits
follow-ups. App pause/resume suppresses expected lifecycle stalls. Four rotating
JSON files under `Application.persistentDataPath/hitch-diagnostics/` bound storage.
Snapshots contain UTC/session time, frame history, live enemies, active/capacity
VFX and text, shutdown meshes, AudioSource/playing counts, LineRenderer/particle
counts, Transform/Renderer counts and profiler memory counters. Hierarchy counting
and JSON/file allocation occur only on a triggered snapshot, using prewarmed lists.
The type and its composition wiring are excluded from non-development players.

Locomotion previously ran a fixed animation rate despite changing world speed.
The presentation now matches the measured planar displacement to a configured
1.8 m run cycle, eases visual playback over 90 ms, preserves short state transitions,
adds at most two degrees of directional lean and bounds foot stance correction to
18 cm on visual bones. Movement/rotation authority, input, colliders and root-motion
settings are unchanged. Teleport deltas do not generate a sprint or footstep burst.

`Tools/post-device-combat/compose_sfx.py` creates all nine replacement 48 kHz mono
PCM masters from deterministic noise, low/mid filters and damped resonators. No
external samples were used. `sfx_masters.json` records exact clip hashes, duration,
peak and RMS. Steps are discrete 220 ms contacts, servos 180 ms, lash charge/release/
impact 160/240/260 ms. Restrained transients replace tonal chirps. Peak is bounded
to 0.78 with click-preventing fades. Existing GUIDs and clip references are retained.

The player lash bank levels become charge 0.27, release 0.48 and impact 0.37; S14
footstep volume is 0.24. Enemy/elite/boss warning mix values and music assets are
unchanged, preserving their established priority and ducking. Speaker timbre and
perceived foot contact still require the requested physical-device review.
