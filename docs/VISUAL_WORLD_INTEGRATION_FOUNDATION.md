# Visual World Integration Foundation — Phase 3A

Base: `origin/main` `8171376c539b9247808ede428711dd44070b73ec`.
Branch: `codex/visual-world-integration-foundation`.

This milestone prepares art intake. It does not select, import or claim final art.
Current ArtSpike/Kenney/prototype geometry remains the default. No new third-party content or package is added.
Gameplay values, spawn/layout coordinates, camera settings, save schema, combat timing and audio direction stay unchanged.

## Ownership and hierarchy

The composition root injects `ChapterVisualEnvironment`; gameplay code never depends on an imported art hierarchy.
Character controllers, target points, sensing colliders, health, AI, rewards and progression stay on existing authority roots.
`CharacterVisualBinding` is an attachment adapter on those roots; it caches the current visual model and optional sockets and owns no gameplay state.

```text
S01 Composition Root
  Chapter 01 Visual Environment                  [authored ChapterVisualEnvironment]
    FloorTerrain / WallsStructures / Props / Pipes / Machinery / Debris
    Lighting / Chapter Baseline Key Light
    Regions
      repair-hub                                [RepairHub_Anchors prefab instance]
      start-region
      relay-yard / cutting-floor / shield-dump
      capacitor-field / hauler-graveyard
        LandmarkAnchor / DressingAnchor
      elite-approach / elite-arena
      boss-approach / boss-arena
    FallbackPrototypeEnvironment
      Chapter 01 Placeholder Geometry           [runtime; no collision]
  Chapter 01 World                              [runtime world presenter]
    Gameplay Geometry                           [ground, perimeter, gates, flank colliders]
  Player                                        [original gameplay/controller root]
    Player Visual Root / active Tier0/1/2 form
  Enemy Pool / ordinary enemy authority
    Enemy Art Root / cached archetype model
  Magnetar Guard / Custodian M0                  [original authority roots]
    Encounter Visual Root / fallback or candidate
```

Names of existing runtime authority objects are retained. The schematic abbreviates intermediate runtime parents.
Collision geometry now has no renderer; the same meshes/materials/transforms are mirrored into the visual layer.
Gate unlocks still toggle the authoritative blocker and its matching fallback visual.
Disabling the visual layer or fallback geometry never removes collision or changes gate state.
The temporary world renderer creates and destroys its own materials as before.

## Exact bindings

### Player

- Asset: `Assets/_Game/Content/Definitions/S07_Evolution.asset`.
- `_tierPrefabs[0..2]`: unchanged ArtSpike whole-form fallbacks.
- `_tierOverrides[0..2]`: optional `PresentationModelBinding` for Tier0, Tier1, Tier2.
- Each binding exposes `_prefab`, `_localPosition`, `_localEulerAngles`, `_localScale`.
- Empty override chooses the existing fallback; tier selection/thresholds/dominant stat calculations are unchanged.
- Component: `PlayerEvolutionView`; adapter: `CharacterVisualBinding`.

The current procedural gait remains unchanged for the prototype's joint hierarchy.
A candidate without that private joint hierarchy is accepted as static geometry: cosmetic facing and an optional
`AttackOrigin` work without writing the player controller transform. Integrating a candidate's own animated gait is future work.
The current gait/audio/VFX design has not been rebuilt. Switching to a form without an attack socket restores the configured
presentation origin rather than carrying a previous form's socket.

### Ordinary enemies and landmarks

- Asset: `Assets/_Game/Content/Definitions/S15_VisualCatalog.asset`.
- `_enemies`: scout-drone, cutter-unit, warden, arc-drone, carrier.
- `_landmarks`: relay-yard, cutting-floor, shield-dump, capacitor-field, hauler-graveyard.
- Each existing recipe supports optional `_presentationPrefab` plus local position/euler/scale.
- With a whole prefab assigned, legacy `_parts` are ignored and may be empty; clearing the prefab restores the kitbash recipe.
- Existing Cutter whole-form fallback is retained.
- Components: `S15EnemyVisualFactory` / pooled `S15EnemyVisualState`.

Models are cached per archetype within the existing pool. Reuse clears the active adapter/model and selects the appropriate cached
visual. No second health/AI/life identity is introduced. Legacy FBX parts are inspected before instantiation and sanitized
of their existing colliders/lights/cameras; scripts, Rigidbody and playback components are rejected.
New whole prefabs must already be presentation-only.
Do not run the legacy “Create S15 Asset Integration” generator after authoring candidate recipes; that command rebuilds its old recipe data.

### Elite / boss / repair hub

- Asset: `Assets/_Game/Content/Definitions/Chapter01_VisualIntegration.asset`.
- `_elite`: MagnetarGuard full prefab and local offsets.
- `_boss`: CustodianM0 full prefab and local offsets.
- `_repairHub`: optional whole visual placed under repair-hub/MainPlatform.
- Scene component: `ChapterVisualEnvironment`, injected through `S01SceneCompositionRoot._visualEnvironment`.

An empty encounter binding retains the original colored capsule under `Encounter Visual Root`.
The root controller dimensions and gameplay target point are computed only from the original gameplay definitions.
Visual scale never resizes a controller or sensing collider. A large or offset candidate can visibly disagree with the
physical hitbox: review alignment at the gameplay camera and on device before accepting art.

## Environment, regions and lighting

The Chapter01 scene contains separate authoring categories for floor/terrain, structures/walls, props, pipes, machinery,
lighting and debris. Empty categories intentionally await external art.
`_dressing[]` pairs an anchor inside the layer with a safe model binding. Alternatively, place a cleaned visual prefab
directly under a category or region in the scene. Validation rejects physics, missing scripts and gameplay scripts there.
`_showFallbackEnvironment` controls the old ground/path/arena/gate display, leaving gameplay geometry active.
Landmarks have independent recipe bindings and remain available when world fallback geometry is hidden.

There are exactly eleven initial regions:

- repair-hub at the existing player spawn (0, 0, -30); start-region is a decorative anchor six metres forward.
- Five ordinary regions at their existing spawn origins; metadata is the original enemy ID.
- elite-approach four metres before the existing elite gate; elite-arena at the original elite spawn.
- boss-approach four metres before the existing boss gate; boss-arena at the original arena centre.

Spot landmark anchors use the existing landmark positions and rotations. These transforms are visual metadata,
not spawn/controller references and not saved progression state. Their movement does not relocate gameplay.
The new approach/start anchors are presentation-only authoring defaults, not map layout changes.

Lighting keeps one directional key light at the previous rotation (50, -30, 0), intensity 1.1, white, no dynamic shadows.
Its authored location is `Lighting`; the composition root does not create a duplicate.
No post-processing, bake pipeline, costly effects or camera alteration is added.

## Repair hub anchors

Prefab: `Assets/_Game/Content/Prefabs/Integration/RepairHub_Anchors.prefab`.
It contains transforms only, instanced at the original spawn.

- MainPlatform, PlayerDockPoint (both at the existing spawn).
- ManipulatorLeft, ManipulatorRight, RearManipulatorA, RearManipulatorB.
- RepairBeamOriginLeft, RepairBeamOriginRight, RepairBeamOriginRearA, RepairBeamOriginRearB.
- AmbientFxRoot.

Manipulator/beam anchors have simple local staging offsets; they are not final models or animated controls.
Nothing calls regen, moves/docks the player, applies healing or changes player recovery authority.

## Optional socket convention

`PresentationSocketSet` caches these exact names:
Root, Core, Sensor, WeaponLeft, WeaponRight, AttackOrigin, HitCenter, GroundContact, VfxTop, VfxRear, TelegraphOrigin.
Center may be represented by Core; no asset is required to provide every socket.

Prefer a direct child named `Presentation Sockets`; named socket transforms may be nested within that container.
Without the container, only exact-name direct children of the model are opt-in sockets.
Arbitrary imported skeleton names deeper in the model are not interpreted as sockets.
Duplicate names within the opted-in area are rejected. Root defaults to the model transform;
other missing sockets use the caller's existing origin/target fallback.

The player AttackOrigin is consumed by the existing motion presenter for cosmetic attack origin.
HitCenter/TelegraphOrigin and other roles are cached attachment contracts for future art integration;
existing hit/death/VFX timing and gameplay target selection remain unchanged.
Never use a socket to move an authoritative telegraph footprint, change attack reach, damage, cadence, hitbox or AI.
No animation event or VFX completion may become a combat commit condition.

## Intake tooling and isolated review

- Select a prefab/model, then `Gravivore > Art Intake > Report Selected Candidate`; save its JSON report.
- Editor API: `ArtIntakeTool.Inspect(candidate, ArtCandidateRole)` supports Player/Ordinary/Elite/Boss/Environment budgets.
  The menu defaults to Environment; the report inspector offers the role-specific choices.
- Report: source, mesh/render/triangle counts, material slots and unique materials, textures and dimensions,
  static/skinned counts, bones/rig, clips, colliders/scripts/missing scripts, lights/cameras/LOD,
  shader names/URP compatibility, missing geometry/materials and broken/unassigned texture references, root scale and renderer bounds.
- Triangle totals include repeated instances and all LODs; they are an upper-bound intake inventory, not visible-triangle profiling.
- Unassigned shader texture properties are reported separately because many URP textures are optional.
  Imported missing texture references still need visual/material review.
- Budgets produce warnings, never automatic rejection. Runtime component safety is checked separately.
- `Gravivore > Art Intake > Create Missing Integration Foundation` creates only missing foundation assets/layers.
  It preserves an existing layer, definition, overrides, review scene and hub prefab.

Scene: `Assets/_Game/ArtReview/Scenes/VisualIntegration_Review.unity`.
Four `ArtReviewSlot` components: PlayerTier2, Ordinary_CutterUnit, Elite_MagnetarGuard, Boss_CustodianM0.
Assign a candidate binding then press “Refresh safe candidate / fallback” on the slot inspector.
The refresh validates before instantiating. Empty candidates reveal the existing fallback.
Overview, current gameplay camera option (reads S01 camera settings) and close review camera are provided,
with a one-metre reference and neutral shadow-free lighting.
“Focus gameplay camera” / “Focus close camera” select a slot and adjust only the review camera.
The scene contains no combat, regen or progression controller and is excluded from EditorBuildSettings.

## Performance guardrails

- Player: <=50k visible triangles, preferably <=40 renderers, 4–6 primary materials.
- Ordinary: <=25k triangles, preferably <=20 renderers, 3–4 primary materials.
- Elite: approximately 40–60k triangles, preferably <=30 renderers.
- Boss: approximately 80–120k triangles for one visible boss, preferably <=40 renderers and LOD.
- Environment: modular shared materials, 1k–2k textures, static/batching where practical; review visible density and overdraw.

The report compares unique materials as a conservative proxy for primary materials; it does not distinguish tiny accents.
Skinned meshes, texture dimensions above 2k, shader mismatch and all-LOD counts generate review notes.
Exceptions require conscious device review, not a blanket asset ban. Existing pool reuse and cached sockets avoid new steady-state allocation.

## Safe model replacement

1. Record source/license outside gameplay data. Inspect the raw candidate before using it.
2. Create a cleaned presentation prefab; retain geometry, materials, transforms, bones, LOD.
3. Remove imported controllers, scripts, colliders, Rigidbody, lights, cameras and animation playback.
   Skinned geometry/bones are allowed; imported Animator/Animation controllers are not wired in this milestone.
   Clips may be reported but playback/gait integration is deferred.
4. Place pivot near ground contact; preview bounds/scale at the one-metre reference. Add optional sockets.
5. Compare in the isolated scene using the existing gameplay camera option and close view.
6. Assign the appropriate binding and offsets; retain fallback data. Save the presentation asset/scene.
7. Validate, run relevant suites and inspect runtime collider/visual/HP alignment and pooled respawn.
   Device review is required for the actual arriving art's rendering/skin/material cost.

Never place gameplay controllers, damage/health logic, spawn/reward/quest/save components or animation callbacks that
write gameplay state inside a presentation prefab. Runtime validation uses a component allowlist:
Transform, MeshFilter, Renderer (including SkinnedMeshRenderer), LODGroup.
Other components must be removed or explicitly integrated later; missing scripts are rejected before instantiation.
Scene environment lighting belongs to the authored Lighting category rather than character/dressing prefabs.

## Structural review captures and verification

The images below are Unity captures of current fallbacks with temporary test-only anchor markers and captions.
They illustrate integration structure, not final art quality. Markers/captions are never saved in gameplay assets.

- [Chapter roots](visual-world/images/01-chapter-roots.png)
- [Repair hub anchors](visual-world/images/02-repair-hub-anchors.png)
- [Five ordinary regions](visual-world/images/03-five-ordinary-spots.png)
- [Elite / boss roots](visual-world/images/04-elite-boss-roots.png)
- [Isolated art review](visual-world/images/05-isolated-art-review.png)

Executed with Unity 6000.3.0f1 on 2026-10-04:

- Compile / foundation asset generation: passed (exit 0); latest source also compiled during the test/validator runs.
- Full EditMode: 312/312 passed, zero failures/skips (296 existing + 16 foundation cases).
- Full PlayMode: 75/75 passed, zero failures/skips (71 existing + 4 foundation flows).
- Standalone ProjectValidator: passed (exit 0), including scene bindings, region/hub validation and review scene exclusion.
- Five structural captures: exported and visually inspected after isolating the review scene and widening only the capture clip range.

Existing Phase 2 gait/attack/audio/HP timing, save/progression/adaptive-respawn and boss reset suites remain green.
New checks cover the component allowlist before instantiation, all ordinary/landmark full-prefab slots, cached pool reset,
player/elite/boss visual offsets versus controller dimensions, generic player geometry/sockets,
visual-environment isolation and hub anchors without a second recovery authority.
Logs/XML are local under `Builds/Logs/visual-foundation-*`; the committed result summary is
[VERIFICATION.json](visual-world/VERIFICATION.json).
Generated Unity settings/importer/material reserialization are excluded from the change.
Gameplay source/configuration, Packages, EditorBuildSettings and the current camera asset have no diff.

Android IL2CPP was intentionally not run, as explicitly permitted for this milestone: all suites pass and no gameplay
or build settings changed. No new APK/device result is claimed. The review scene exclusion and Android
ARM64/IL2CPP/portrait configuration are checked by ProjectValidator. Actual candidate art still needs device performance review.

## Deferred and next milestone

Final character/environment models, final VFX/SFX/music, candidate rig playback, animated repair manipulators,
elite/boss repeatability, stronger spots and minimap UI are not implemented.
Next work is external art selection/intake and the subsequent Phase 3 visual world integration pass.
No new numbered gameplay specification is started by this foundation.
