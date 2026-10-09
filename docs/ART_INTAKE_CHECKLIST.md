# Current art intake checklist

Status: **CURRENT**

Read first:
- `docs/CURRENT_VISUAL_TARGET.md`
- `docs/ASSET_SOURCE_OF_TRUTH.md`
- `docs/ART_ASSET_POLICY.md`

Do not use the historical Phase 3 binding documents as current instructions.

## Before a candidate enters production

- Record original publisher/author, source URL, exact license, date checked and redistribution constraints.
- Acquire new raw payload only under `ExternalAssetIntake/Current/<source-id>/`.
- Do not execute vendor scripts/demo code during inspection by default.
- Inspect mesh/render/triangle/material counts, texture maps/resolutions, LOD, rig/bones/clips, colliders, lights, cameras, particle systems, scripts, scale/pivots and shader compatibility.
- Verify real visible materials, not only whether Unity imported without errors.
- Judge the candidate against `CURRENT_VISUAL_TARGET.md`: silhouette, mechanical plausibility, surface depth, gameplay-camera readability and stock-asset recognizability.
- Treat mobile budgets as optimization inputs, not style targets.
- Establish metre scale, forward direction, ground/contact convention and required presentation sockets.

## Clean production asset

- Gameplay authority must not be hidden inside third-party scripts or art-prefab colliders.
- Remove/disable vendor gameplay scripts, demo cameras and unrelated components.
- Keep imported animation/Animator only when intentionally reviewed as presentation and root motion/authority remains compatible with gameplay.
- Use separate gameplay collision proxies when complex visual geometry should not own collision.
- Preserve useful authored material information; if materials are consolidated, rebake/recreate the visual variation rather than replacing everything with one flat generic material.
- Convert shaders/materials to the current URP path without losing required BaseColor/Normal/metal/roughness-or-smoothness/AO/emission information.
- Add LOD/material/texture optimization appropriate to the real gameplay-camera size.

## Bind through current presentation architecture

Use current scene/definition references on `main`; do not regenerate over authored production overrides from an old phase tool.

General rules:
- player art remains separate from movement/combat authority;
- ordinary-enemy art remains separate from pooled enemy authority;
- elite/boss art preserves attack/target/health/death sockets and encounter authority;
- environment art preserves world topology and explicit gameplay blockers while allowing rich presentation around them;
- hub/facility art preserves interaction/spawn points while presentation may be much larger/more detailed;
- damage timing never depends on VFX/animation completion.

## Verify

- ProjectValidator and relevant EditMode/PlayMode suites pass.
- No missing/error materials or references.
- Pool reuse, target points, attack origins, telegraphs and death presentation still align with the visible model.
- Traversal and collision proxies match the visual expectation for major solid structures.
- Visual quality is reviewed in the real portrait gameplay camera.
- Android/device frame time, thermals and visual result are checked before claiming production acceptance.
- Provenance/notices and the promoted production subset are committed together where licensing permits.
