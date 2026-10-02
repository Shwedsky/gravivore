# Art pipeline — isolated proof and future integration boundary

## Current reproducible workflow

1. Fetch a free archive from the author's official source. Check its page and bundled license before import; record provenance and retained file hashes.
2. Retain only useful unmodified FBX files beneath `Assets/_Game/ArtSpike/Imported/<Vendor>/<Pack>/`. Never overwrite the existing S15 donors. Omit vendor scripts, sample scenes, texture palettes, unused models, and paid assets.
3. `ArtSpikeModelImporter` disables animation, blend shapes, cameras, lights, visibility tracks, embedded materials, and read/write. Geometry compression is off to preserve source geometry for this proof; optimization changes import layout only. The vendor FBX bytes are unchanged.
4. `ArtSpikeBuilder` creates project-owned URP materials and prefab hierarchies outside the vendor folders. It instantiates donor children, applies rotations, recenters their bounds, and scales the wrapper to the authored dimensions in Unity metres. No vertices, topology, UVs, or rigs are edited. Planar donor floors retain their zero thickness.
5. `ArtSpikeScene` places four review areas and copies the settled S20 camera pose/FOV from `S01_CameraFollowSettings.asset`. Review-only layers 24–27 isolate the cameras without changing production `TagManager`. One directional light and an isolated volume profile provide the preview lighting.
6. `ArtSpikeCapture` renders the actual comparison scene through URP into RenderTextures. It warms the renderer before reading pixels and writes PNGs under `docs/art-spike/images/`. The supplementary triptych joins three actual 1080×1920 S20-camera renders. No generated concept images or painted substitutes are used.
7. `ArtSpikeAudit` reports renderer/material/mesh/triangle/texture/animation/collider counts and projected bounds. The isolated EditMode assembly checks presentation-only character contents, geometry evolution, enemy differentiation, camera consistency, and production dependency isolation.

Menu commands: **Gravivore → Art Spike → Rebuild Comparison Assets**, **Render Review Images**, and **Write Performance Snapshot**. Rebuild overwrites the authored spike assets; render opens the saved review scene. Save any unrelated open scene work before using these authoring commands.

For command-line reproduction, run Unity 6000.3.0f1 with `-batchmode -quit -projectPath <isolated-worktree> -executeMethod Gravivore.ArtSpike.Editor.ArtSpikeCapture.BuildAndCapture -logFile <log>`. Omit `-nographics` for PNG rendering. Import/probe and tests can run separately. Existing bootstrap tooling creates local URP/player settings in a fresh checkout; those generated production settings are not part of this PR.

## Scale and orientation

One Unity unit means one metre. Prefab roots remain scale `(1,1,1)`, ground-centered, with attack direction `+Z` and `+Y` up. Wrapper transforms normalize a donor after its rotation; intentional nonuniform scale is documented by the prefab. No negative scale or destructive mirror operation is used. Core dimensions and the common support hierarchy persist across tiers.

Materials are shared project-owned asset references. Every donor slot is replaced with a palette material. No vendor texture or shader is required. Piston child meshes remain independently selectable; this preserves a path for later articulation but introduces no animation in this spike.

## Future flow, after explicit art approval

Vendor source → project-owned prefab → project-owned materials → visual recipe/catalog → runtime presentation.

The current S15 catalog is a set of serialized source-model parts and transforms, and the current evolution view activates tier socket modules. Neither references this spike. Future integration must intentionally bind approved prefabs or their constituent modules into a catalog/view, preserving progression thresholds and gameplay authority. Do not bypass the existing runtime architecture with path loading or a second progression system.

An art prefab has no health, damage, progression, sensing, movement, rewards, save state, purchases, or menu authority. Gameplay colliders stay on the established gameplay root. Any later animation/presentation observer must handle pooling reset and lifecycle without modifying domain state.

## Safe vendor updates

Keep the original archive outside `Assets` in the ignored local acquisition folder. Verify a new release's license again; preserve existing GUIDs only for the same intended retained asset. Check hashes, dimensions, orientation, pivots, submesh layout, and child names. Review regenerated screenshots and rerun isolation tests before accepting changes. Do not overwrite source files to make a composition fit; change project-owned transforms or choose another donor.

Renderer consolidation and animation are future measured work. This proof deliberately preserves donor children and original meshes; it does not promise a final mobile-ready character rig or seamless custom armor mesh.
