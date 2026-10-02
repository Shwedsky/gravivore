# Art pipeline — V2 isolated proof

## Reproduce the proof

1. Acquire original-author archives only. Verify commercial use, modification, repository redistribution and attribution on the page and bundled notice; record hashes. CC0, CC-BY and other explicit free commercial licenses are eligible. Exclude NC, unclear and paid-only assets.
2. Retain unchanged sources under Assets/_Game/ArtSpike/Imported. V2 keeps Julius's OBJ/MTL/license and a small Kenney scenery subset. The scoped .gitattributes prevents source line-ending normalization; verify committed blobs against original archive bytes. No vendor scripts or engine packages are imported.
3. ArtSpikeModelImporter acts only on this folder. Animation, blend shapes, cameras, lights, embedded materials and read/write are disabled. Compression is off. Julius normals are calculated with a 15° smoothing angle to separate existing armor planes. Source vertices/topology/UVs/bytes remain unchanged. Rebuild reapplies the import settings before mesh selection.
4. ArtSpikeMechComposition selects one existing OBJ child by source name. There is no splitting, combining or vendor mesh copying. Wrappers rotate, recenter and scale chosen parts. Intact limbs include their existing joint/armor geometry. Head and complete humanoid geometry are not instantiated. Small Unity primitives form the core, hips and containment posts; a shared small ring mesh supplies the allowed ring augmentation.
5. Shared project-owned URP materials replace donor materials. No character textures are needed. The scene uses one directional light, restrained bloom and a shared 32×32-per-face static reflection cubemap to reveal metal planes.
6. ArtSpikeScene retains four review areas and numeric layers 24–27 without modifying TagManager. S20 camera settings are read unchanged. A supplementary close three-quarter camera supplies image 08.
7. ArtSpikeCapture writes eight actual URP renders under docs/art-spike/images. Image 07 joins three actual full portrait renders. No generated/painted substitutes are used. Capture-only visibility changes are restored by reopening the saved scene.
8. ArtSpikeAudit measures renderers, meshes, triangle indices, materials/slots, textures and animation/collider state. Full tests and ProjectValidator verify baseline flows and isolation.

Menu: **Gravivore → Art Spike → Rebuild Comparison Assets**, **Render Review Images**, **Write Performance Snapshot**. Save unrelated open scene work first; these authoring commands overwrite isolated spike assets.

Command line: Unity 6000.3.0f1 with -batchmode -quit -projectPath <worktree> -executeMethod Gravivore.ArtSpike.Editor.ArtSpikeCapture.BuildAndCapture -logFile <log>. Omit -nographics for capture. Do not run simultaneous Unity processes on one worktree. Fresh bootstrap generates local URP/player settings; unrelated serialization is excluded from the PR.

## Source updates and future binding

One unit is one metre, +Y is up, +Z is forward; roots remain (1,1,1). Normalize bounds after rotation under identity ancestors. Intentional nonuniform transforms are visible in wrappers; no source topology or rig is edited.

Vendor source → project-owned composition/materials → approved visual recipe/catalog → runtime presentation. S15's catalog and evolution observer remain unbound. Future integration requires explicit art approval and preserves gameplay authority, pooling and lifecycle. Art prefabs own no health, damage, progression, movement, rewards, saves, purchases or menus.

For updates, recheck licenses, hashes, units, normals, pivots, names and submeshes. Preserve GUIDs only for the same intended asset; regenerate screenshots and isolation checks. V2 reduces submissions through intact child selection, without blindly combining geometry.

Julius-derived adaptations carry CC BY-SA 3.0 with credits and modification notices; see ThirdPartyNotices.md and [license scope](art-spike/ART_LICENSE.md). This covers the adapted visual work, not independent gameplay code or the whole repository. Future game distributions must carry applicable credits/license information and honor these asset terms.

No user Blender work is required. Portable official Blender 4.5.0 was used only to inspect other candidate blend files in the ignored acquisition folder. The selected OBJ imports directly in Unity. No Blender dependency, executable, converter or rejected candidate blend file enters the PR.
