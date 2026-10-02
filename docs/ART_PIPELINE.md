# Art pipeline — V3 isolated proxy proof

## Reproduction

1. Check original-author license, commercial use, modification, public source redistribution and required credits before importing. CC0 is preferred, CC BY acceptable with precise notices; exclude SA, NC, unclear, paid-only and recognizable franchise content. Skip any acquisition requiring login/manual user interaction.
2. Retain byte-identical external source files under Assets/_Game/ArtSpike/Imported. V3 contains only the existing nine Kenney scenery FBXs/two bundled notices and three CC0 Poly Haven surface files plus its documented license/source notice. The scoped .gitattributes prevents normalization of original bytes; ASSET_MANIFEST.json records hashes.
3. ArtSpikeModelImporter affects only this isolated folder. Current Kenney scenery has no character rig. No vendor scripts, complete engine packages or research archives enter Assets.
4. ArtSpikeProxyMeshes authors eight original hard-surface modules under Proxy/Models. Chamfered shells, joints, pistons, feet, fork housings and shear blades have flat face normals, planar UVs and generated tangents. Two deliberate armor/structure submeshes per module preserve material variation; they are not combined from a licensed character. No Julius source is read.
5. ArtSpikePbr retains original diffuse/normal/ARM maps and creates URP metallic/smoothness (R=ARM.B, A=1-ARM.G) and AO (RGB=ARM.R). Normal, mask and AO are linear; diffuse is sRGB. All maps use mipmaps, 1024 max size, Android ASTC 6x6, no runtime CPU readback. GL normal orientation is used directly.
6. ArtSpikeMechComposition creates low G-0 tiers and asymmetric Cutter with independent positive-scale module transforms and hip/knee/foot/weapon pivots. One unit is one metre, +Y up, +Z forward, roots (1,1,1). The common cyan core remains at (0,0.68,0), diameter 0.30, across all tiers.
7. Project-owned URP materials tint the original paint toward cool armor/graphite without replacing its normals, roughness, metallic or AO. Common core and connector materials remain shared. The studio uses one directional light, modest bloom and a shared 32×32-per-face reflection cubemap.
8. ArtSpikeArticulation authors a three-second transform-only mechanical idle clip, samples a temporary clone, and verifies four independent hip pivots move. Main prefabs and scene have no Animation/Animator or gameplay binding. This is an idle/pivot viability proof, not a donor skeleton, gait, IK, ground-contact solution or combat integration.
9. ArtSpikeScene retains the four review areas on numeric layers 24–27; production TagManager/build scenes are untouched. S20 camera parameters are read without change. Added close surface and exploded-composition cameras support images 09/10.
10. ArtSpikeCapture creates all ten actual Unity/URP images. Image 07 joins three complete portrait renders. Image 10 temporarily separates module groups, restores every transform, then reopens the saved scene. No AI/painted substitute or postprocessed asset image is used.
11. ArtSpikeAudit measures renderer/material/slot/mesh/triangle/texture counts and conservative projected bounds at the exact S20 camera. Full EditMode/PlayMode and standalone ProjectValidator verify isolation and existing behavior.

Menus: **Gravivore → Art Spike → Rebuild Comparison Assets**, **Render Review Images**, **Write Performance Snapshot**, **Preview Proxy Mechanical Idle**. Save unrelated scene work before rebuilding. The preview menu requires the isolated comparison scene, creates a temporary DontSave clone sampled at 0.75 seconds; its Animation can play in Play mode. Delete the temporary clone after review.

CLI: Unity 6000.3.0f1, -batchmode -quit -projectPath <worktree> -executeMethod Gravivore.ArtSpike.Editor.ArtSpikeCapture.BuildAndCapture -logFile <log>. Capture needs D3D11; omit -nographics. Only one Unity process per worktree. No new Unity package, production renderer configuration or Android build is introduced.

## Future final art

The geometry is an explicitly authorized **temporary proxy**, not a claimed game-ready Vanguard replacement. Replace its mechanical module assets with refined original final geometry or legally obtained permissive separable meshes, preserving socket paths, metre units, core transform and tier silhouette. Texture density/UV distortion still need final artist work. A production gait needs rig/IK/ground-contact authoring and device profiling.

S15 catalog, S07 evolution definitions, S20 gameplay/balance and Chapter 01 remain unbound. Future binding requires explicit visual approval and a separate authorized specification, preserving gameplay authority and pooling/lifecycle. No new spec is started here.

Blender 4.5.0 was used read-only to inspect rejected .blend/.fbx candidates, with factory settings and autoexec disabled. Blender, downloaded archives and rejected character assets stay outside the public commit. Reproduction of the selected V3 scene requires Unity only.
