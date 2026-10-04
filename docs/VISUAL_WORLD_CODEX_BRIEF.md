# GRAVIVORE — Phase 3 Codex Integration Brief

Implement **Phase 3 — Visual World Baseline** from current `main`.

Read first:
- `docs/POST_S20_PRODUCT_ROADMAP.md`
- `docs/VISUAL_WORLD_BASELINE.md`
- `docs/VISUAL_WORLD_ASSET_RESEARCH.md`
- `docs/ART_ASSET_POLICY.md`
- `docs/ART_PIPELINE.md`
- `AGENTS.md`

## Objective

Produce a DEV Android build in which Chapter 1 reads as one coherent modern industrial sci-fi world rather than an empty mechanics prototype.

Do not change balance/gameplay authority. Do not start final G-0 gait/VFX/SFX polish in this task.

## Required implementation scope

1. Environment shell
- Dress the full 72x140 Chapter 1 space.
- Establish dark industrial floor/perimeter/gantry/pipe language.
- Preserve the existing authoritative world coordinates.

2. Repair hub
- Build the semi-hangar/service-bay shell at spawn.
- 2–4 presentation-only manipulator/service arms.
- Restrained cyan repair/diagnostic VFX.
- No change to heal authority.

3. Named zone landmarks
- Relay Yard
- Cutting Floor
- Shield Dump
- Capacitor Field
- Hauler Graveyard
- Elite approach
- Boss approach/arena

Each must be recognizable by silhouette from the gameplay camera.

4. Ordinary enemy family visuals
- scout-drone
- cutter-unit
- arc-drone
- warden
- carrier

Do not use one recolored/scaled model. Each needs a distinct topology/silhouette.

5. Elite and boss visuals
- magnetar-guard: credible larger guardian mech, magnetic/coil identity.
- custodian-m0: large asymmetric industrial maintenance mech, not generic humanoid.

6. Presentation architecture
- keep all new art replaceable;
- no gameplay colliders on imported presentation models unless explicitly justified;
- no third-party scripts;
- no new package/framework;
- reuse/extend S15 presentation binding rather than coupling gameplay to visuals;
- preserve root scale and gameplay collision values.

7. Asset policy
- initial spend remains 0;
- prefer the selected CC0 sources in the research note;
- if an asset cannot be autonomously/directly acquired, do not block: use project-authored proxy geometry;
- record exact provenance and ThirdPartyNotices for every imported file.

8. Performance
- shared materials;
- 1K PBR textures by default, mipmaps + Android ASTC 6x6;
- ordinary actor target 3k–12k triangles;
- elite <=25k preferred;
- boss <=50k preferred;
- avoid many tiny renderer objects;
- profile actual Android result.

## Validation

Run the project-prescribed EditMode/PlayMode tests and ProjectValidator.

Build:
`build-android.ps1 -Flavor Dev`

Produce the 12 screenshot/capture views listed in `VISUAL_WORLD_BASELINE.md`.

Verify on the implementation side:
- no movement/combat/HP/regen/save/progression/respawn/boss-reset regression;
- damage timing still gameplay-authoritative;
- enemy and boss target points remain correct;
- combat telegraphs are not obscured;
- no obvious camera clipping from dressing;
- no new runtime errors;
- no obvious Android performance regression.

## Delivery

Open one focused Phase 3 PR. Include:
- summary of visual changes by zone/actor;
- asset provenance and exact licenses;
- files changed;
- automated test output;
- validator output;
- Android build result;
- measured performance notes;
- links/paths to the 12 capture images;
- explicit list of deferred art polish.

Do not merge until human visual/device review.
