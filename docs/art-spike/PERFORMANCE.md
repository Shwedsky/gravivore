# Art Spike measured prefab snapshot

Counts include inactive children. Triangles sum mesh index counts per instance, including Unity primitives. Unique meshes/materials are distinct shared asset references; material slots count draw submissions before batching. Projected pixels are a conservative bounding box at the settled S20 camera (1080×1920), not a pixel-accurate silhouette mask.

## G0_Tier0_ArtSpike

- Child renderers: 30; SkinnedMeshRenderer: 0; MeshRenderer: 30.
- Materials: 4 unique; 30 slots. Meshes: 11 unique; 30 instances.
- Triangles: 2 724; maximum texture size: 0 (0 means no textures).
- Animation: 0 Animator, 0 legacy Animation; colliders: 0.
- Bounds (metres): (1.430, 0.862, 1.735); S20 projected bounding box: (207.8, 261.5) pixels.

## G0_Tier1_ArtSpike

- Child renderers: 37; SkinnedMeshRenderer: 0; MeshRenderer: 37.
- Materials: 4 unique; 37 slots. Meshes: 12 unique; 37 instances.
- Triangles: 3 316; maximum texture size: 0 (0 means no textures).
- Animation: 0 Animator, 0 legacy Animation; colliders: 0.
- Bounds (metres): (1.760, 0.862, 1.950); S20 projected bounding box: (255.7, 283.4) pixels.

## G0_Tier2_ArtSpike

- Child renderers: 60; SkinnedMeshRenderer: 0; MeshRenderer: 60.
- Materials: 4 unique; 60 slots. Meshes: 16 unique; 60 instances.
- Triangles: 5 116; maximum texture size: 0 (0 means no textures).
- Animation: 0 Animator, 0 legacy Animation; colliders: 0.
- Bounds (metres): (2.380, 0.862, 2.420); S20 projected bounding box: (348.3, 333.3) pixels.

## Cutter_ArtSpike

- Child renderers: 15; SkinnedMeshRenderer: 0; MeshRenderer: 15.
- Materials: 4 unique; 15 slots. Meshes: 9 unique; 15 instances.
- Triangles: 1 362; maximum texture size: 0 (0 means no textures).
- Animation: 0 Animator, 0 legacy Animation; colliders: 0.
- Bounds (metres): (1.725, 0.818, 1.795); S20 projected bounding box: (248.7, 261.9) pixels.

## Performance limits

This is a structural snapshot, not Android frame-time evidence. Piston donors expose three separate renderers each; Tier 2 retains many renderer submissions. Consolidation/batching and animation must be measured before production adoption. No dynamic lights, colliders, or gameplay scripts live in character prefabs. The comparison scene has one directional light and a small bloom pass.
