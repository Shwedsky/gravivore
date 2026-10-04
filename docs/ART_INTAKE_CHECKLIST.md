# Art intake checklist

Use with [Phase 3A binding/hierarchy reference](VISUAL_WORLD_INTEGRATION_FOUNDATION.md).

## Before a candidate enters gameplay

- Record author/source URL, license and permitted redistribution. No paid requirement, extracted game assets or derivative prohibited content.
- Keep the raw imported asset separate from a cleaned presentation prefab. Do not replace gameplay roots.
- Run `Gravivore > Art Intake > Report Selected Candidate`. Select the role for budget warnings; save the JSON with the review.
- Check mesh/render/triangle/material-slot totals, unique materials, texture sizes, skin/bones/clips,
  physics/scripts, lights/cameras/LODs, shader compatibility, missing references and scale/bounds.
- Resolve magenta/non-URP materials, missing meshes/materials and missing texture references.
  Empty optional texture properties are informational; inspect visible materials in the review scene.
- Remove MonoBehaviours (including third-party demo scripts), colliders, Rigidbody, lights, cameras, Animator/Animation/playback
  and missing scripts from the cleaned prefab. This milestone accepts meshes/skins/bones/LOD without imported playback authority.
- Confirm clean prefab contains only Transform/MeshFilter/Renderer/LODGroup.
- Establish metre scale, ground pivot, forward direction and a sensible renderer bounding box.
- Optional socket container `Presentation Sockets`: exact names from the foundation doc; no duplicate opted-in socket name.
- Open `Assets/_Game/ArtReview/Scenes/VisualIntegration_Review.unity`.
  Assign the appropriate `ArtReviewSlot._candidate`, refresh, compare gameplay/close cameras against the one-metre reference.
- Budget exceptions are warnings: assess visible LOD, draw calls, skins and device cost before acceptance.
  Player <=50k/40 renderers/4–6 primary materials; ordinary <=25k/20/3–4; elite 40–60k/30; boss 80–120k/40 + LOD.
  Environment: shared materials, modular pieces, usually 1k–2k textures.

## Bind a reviewed candidate

- Player: S07_Evolution `_tierOverrides[0..2]`; preserve `_tierPrefabs` fallback and all tier thresholds.
- Ordinary: S15_VisualCatalog `_enemies` recipe `_presentationPrefab` and offsets by original enemy ID.
- Elite / boss: Chapter01_VisualIntegration `_elite` / `_boss` prefab and offsets.
- Spot landmarks: S15_VisualCatalog `_landmarks` recipe by original spot ID; preserve gameplay spawn/zone coordinates.
- Environment: Chapter01 Visual Environment categories/region dressing anchors or validated `_dressing` entries.
  Keep physics, gate blockers and world state under Gameplay Geometry.
- Hub: optional `_repairHub` visual under MainPlatform; keep PlayerDockPoint at current spawn and all anchors available.
- Do not infer collider sizes from model bounds. Check visual/hitbox agreement explicitly for large elite/boss models.
- Generic player geometry works without prototype joints; its own gait/clip playback is separate future presentation work.
  Do not route damage through Animator, animation events or VFX completion.

## Verify and preserve rollback

- Keep fallback assets/recipes; clearing an override must restore them.
- Run ProjectValidator and relevant EditMode/PlayMode suites; Phase 2 gait/attack/HP/audio regressions must remain green.
- Inspect all player tiers; ordinary hit/death/pool reuse; elite/boss target points and telegraphs; repair hub with original recovery;
  collision/gates when environment fallback is hidden.
- Verify review scene remains outside gameplay Android build scenes.
- Review actual arriving art on device for frame time, draw calls, shader/texture/skin cost and visible scale.
- Commit source/license/report/binding changes together. Do not run the legacy S15 recipe generator over authored overrides.
