# Art Spike measured prefab snapshot

Counts include inactive children. Triangles sum mesh index counts per instance, including Unity primitives. Unique meshes/materials are distinct shared asset references; material slots count draw submissions before batching. Projected pixels are a conservative bounding box at the settled S20 camera (1080×1920), not a pixel-accurate silhouette mask.

## G0_Tier0_ArtSpike

- Child renderers: 19; SkinnedMeshRenderer: 0; MeshRenderer: 19.
- Materials: 4 unique; 36 slots. Meshes: 8 unique; 19 instances.
- Triangles: 5 928; textures: 4 shared maps, maximum size 1024.
- Animation: 0 Animator, 0 legacy Animation; colliders: 0.
- Bounds (metres): (1.500, 0.826, 1.768); S20 projected bounding box: (215.1, 258.3) pixels.

## G0_Tier1_ArtSpike

- Child renderers: 24; SkinnedMeshRenderer: 0; MeshRenderer: 24.
- Materials: 4 unique; 43 slots. Meshes: 10 unique; 24 instances.
- Triangles: 6 936; textures: 4 shared maps, maximum size 1024.
- Animation: 0 Animator, 0 legacy Animation; colliders: 0.
- Bounds (metres): (1.961, 0.826, 1.792); S20 projected bounding box: (281.4, 261.0) pixels.

## G0_Tier2_ArtSpike

- Child renderers: 33; SkinnedMeshRenderer: 0; MeshRenderer: 33.
- Materials: 4 unique; 58 slots. Meshes: 11 unique; 33 instances.
- Triangles: 9 216; textures: 4 shared maps, maximum size 1024.
- Animation: 0 Animator, 0 legacy Animation; colliders: 0.
- Bounds (metres): (2.498, 0.862, 2.073); S20 projected bounding box: (361.0, 294.1) pixels.

## Cutter_ArtSpike

- Child renderers: 13; SkinnedMeshRenderer: 0; MeshRenderer: 13.
- Materials: 3 unique; 25 slots. Meshes: 8 unique; 13 instances.
- Triangles: 3 352; textures: 4 shared maps, maximum size 1024.
- Animation: 0 Animator, 0 legacy Animation; colliders: 0.
- Bounds (metres): (1.535, 0.614, 1.857); S20 projected bounding box: (216.9, 247.2) pixels.

## Performance limits

V3 uses original articulated proxy modules with two PBR submeshes per mechanical module, not a blanket combine of donor characters. Renderer caps: Tier 0 <=30, Tier 1 <=35, Tier 2 <=40, Cutter <=25; target <=50,000 triangles each. Slots above expose the unbatched draw cost, including shadow submissions separately at runtime. Four shared 1024-pixel PBR maps use mipmaps and an Android ASTC 6x6 override. The unchanged source ARM file is retained for provenance but not referenced by renderers. The scene also uses one shared 32×32-per-face static reflection cubemap, one directional light and modest bloom. Static character prefabs have no Animator, Animation, colliders, lights or gameplay scripts. Pivot articulation is an editor-only preview. This structural snapshot does not establish Android frame time or 60 FPS; batching, shadows, repeated enemies and a future gait require device profiling before production adoption.
