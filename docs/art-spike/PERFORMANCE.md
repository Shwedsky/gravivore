# Art Spike measured prefab snapshot

Counts include inactive children. Triangles sum mesh index counts per instance, including Unity primitives. Unique meshes/materials are distinct shared asset references; material slots count draw submissions before batching. Projected pixels are a conservative bounding box at the settled S20 camera (1080×1920), not a pixel-accurate silhouette mask.

## G0_Tier0_ArtSpike

- Child renderers: 13; SkinnedMeshRenderer: 0; MeshRenderer: 13.
- Materials: 4 unique; 13 slots. Meshes: 8 unique; 13 instances.
- Triangles: 6 252; maximum texture size: 0 (0 means no textures).
- Animation: 0 Animator, 0 legacy Animation; colliders: 0.
- Bounds (metres): (1.270, 0.830, 1.650); S20 projected bounding box: (182.2, 246.8) pixels.

## G0_Tier1_ArtSpike

- Child renderers: 18; SkinnedMeshRenderer: 0; MeshRenderer: 18.
- Materials: 4 unique; 18 slots. Meshes: 11 unique; 18 instances.
- Triangles: 6 900; maximum texture size: 0 (0 means no textures).
- Animation: 0 Animator, 0 legacy Animation; colliders: 0.
- Bounds (metres): (1.560, 0.830, 1.650); S20 projected bounding box: (223.8, 246.8) pixels.

## G0_Tier2_ArtSpike

- Child renderers: 25; SkinnedMeshRenderer: 0; MeshRenderer: 25.
- Materials: 4 unique; 25 slots. Meshes: 11 unique; 25 instances.
- Triangles: 8 292; maximum texture size: 0 (0 means no textures).
- Animation: 0 Animator, 0 legacy Animation; colliders: 0.
- Bounds (metres): (2.000, 0.865, 2.070); S20 projected bounding box: (289.7, 294.8) pixels.

## Cutter_ArtSpike

- Child renderers: 9; SkinnedMeshRenderer: 0; MeshRenderer: 9.
- Materials: 4 unique; 9 slots. Meshes: 8 unique; 9 instances.
- Triangles: 4 104; maximum texture size: 0 (0 means no textures).
- Animation: 0 Animator, 0 legacy Animation; colliders: 0.
- Bounds (metres): (1.420, 0.805, 2.185); S20 projected bounding box: (204.0, 299.0) pixels.

## Performance limits

V2 selects existing single-renderer mech child meshes without combining them. V1 Tier 2 had 60 renderers; compare the measured V2 count above. Character materials have no textures; the scene additionally uses one shared 32×32-per-face static reflection cubemap (mipmapped RGBAHalf), one directional light, and a small bloom pass. No dynamic lights, colliders, rig, animation, or gameplay scripts live in character prefabs. This is a structural snapshot, not Android frame-time evidence. Repeated enemies, shadows, batching, and future animation still require device profiling before production adoption.
