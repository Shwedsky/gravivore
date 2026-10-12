# Actor presentation in the complete concept chapter

Baseline: `7dcd6652c0c7232aaaed5e134edbfc5ea1c3c7e2` (APK 53 source `3b1396e`).
Production edits use existing project-owned FBX, maps, sockets and clips.
These tools do not recreate the family or alter the world layout.

`finish_surfaces.py` reads the exact baseline textures and produces the
current graphite/service-metal treatment, contained red armor and G-0 finish.
`prepare_baseline.py` retains original maps/material values for the in-memory
matched comparison. `inspect_source.py` and `texture_audit.py` are diagnosis.

Run the existing Unity wrapper from this checkout with `-ActorCombat`.
`-Action Author` applies actor finishes, existing LOD thresholds and masked
support animation layers; `EditMode`, `PlayMode`, `Compile`, `Validate` are
the corresponding gates. `-VideoCapture` enables the actual gameplay captures.
`-Before` selects the in-memory original surface comparison only.

`collect_evidence.py` must run before restoring regenerated old report files.
`boards.py` preserves screenshot aspect ratio and never substitutes missing
captures. `encode_video.py` consumes the latest full-chapter recording;
`verify_video.py` decodes the delivered MP4 and checks its length.
Both use the existing Blender 5.2 encoder; no new dependency is needed.

`preservation.py` checks the protected world/models/IDs/save/camera paths and
the retained owner APK 53. `summarize.py` retains the original failed full XML
and separately records the successful targeted retries. `verify_apk.py`
executes SDK/ZIP checks and verifies packed actor resources and actual
GLES3/Vulkan shader variants after APK 54 is built. `inspect_controllers.py`
uses the existing typed SerializedFile reader to decode native class 91
controllers and compare compiled skeleton mask hashes/weights to the source
AvatarMasks. Unity compiles these masks into the controllers; standalone
class 319 mask objects are not required in the APK.

Evidence: `docs/history/implementation-passes/actors-combat-concept-world/`.
Editor profiling and generated APK do not establish Android performance.
