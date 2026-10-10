# GRAVIVORE current authority index

Status: **CURRENT / AUTHORITATIVE**

This file exists to stop historical phase documents from silently becoming requirements for new work.

## Mandatory read order for every new Codex task

1. `AGENTS.md`
2. `docs/CURRENT_AUTHORITIES.md`
3. `docs/CURRENT_VISUAL_TARGET.md` for any presentation, world, art, VFX, UI, model, material, lighting or asset task
4. `docs/visual-blueprints/chapter01/CHAPTER01_VISUAL_BLUEPRINT_V1.md` for any Chapter 01 world-layout, environment-composition, facility, collision-layout, minimap-geometry or full visual-rebuild task
5. `docs/visual-blueprints/chapter01-actors/README.md`, the exact `CHAPTER01_ACTOR_VISUAL_TARGETS_V2.png` and `CHAPTER01_ACTOR_VISUAL_TARGETS_V2.md` for any Chapter 01 actor anatomy/presentation/production task; inspect the image before authoring and stop if it is missing
6. `docs/ASSET_SOURCE_OF_TRUTH.md` for any task that touches external or project-owned art assets
7. `docs/DECISIONS.md`
8. `docs/PROJECT_BIBLE.md`
9. `docs/GAME_DESIGN.md`
10. `docs/ARCHITECTURE.md`
11. the active specification or explicit task
12. `docs/TEST_STRATEGY.md` and `docs/BUILD_AND_RELEASE.md`

The current explicit human task still has highest authority.

## Historical documentation rule

Everything under `docs/history/` is **non-authoritative historical evidence**.

Historical files exist so a future developer can understand why the project changed, reproduce old evidence when necessary, or compare previous decisions. They must **not** be used as current:

- requirements;
- art direction;
- acceptance criteria;
- source paths;
- performance limits;
- implementation instructions;
- model or asset recommendations;
- current branch/baseline assumptions.

A current task may explicitly ask to inspect a historical document. That makes it evidence for that task, not a restored project rule.

Versioned names such as `Phase3`, `V3`, `V43`, `Art Spike`, `Concept Fidelity`, `First Visual Slice`, or similar are not evidence of current authority by themselves.

### Where new phase reports go

New versioned delivery reports, screenshots, validation snapshots, before/after boards and one-off implementation notes belong under a clearly historical path such as:

`docs/history/implementation-passes/<pass-id>/`

They must not be added to the root of `docs/` as if they were permanent project rules. If a stable decision emerges from a pass, update the appropriate CURRENT document separately.

## Current visual authority

The project is targeting **mobile-optimized premium hard-surface industrial sci-fi**, not a low-poly aesthetic.

Low polygon counts, LODs, shared materials, atlases, batching and restrained dynamic lighting are optimization techniques. They do not define the visual style and must not be used to justify toy-like geometry, flat materials, generic blockouts or sparse test-scene composition.

The current visual contract is defined in `docs/CURRENT_VISUAL_TARGET.md` and `docs/ART_DIRECTION.md`.

For Chapter 01 actors, the primary visual authority is the exact owner-approved `docs/visual-blueprints/chapter01-actors/CHAPTER01_ACTOR_VISUAL_TARGETS_V2.png`, normalized by its V2 Markdown contract. Newest explicit owner correction > V2 PNG > V2 text > CURRENT visual target > ART_DIRECTION > donor audit/matrix > historical evidence. Donors define manufacturing possibilities, never approved anatomy. Historical documents cannot restore superseded requirements.

Actor V2 supersedes the radial/spider Scout in `docs/visual-production-v2/ENEMY_VISUAL_TARGETS_APPROVED_V1.md`: Scout is a light two-legged biped with long blade-arm terminations. Warden carries two separate arm/hand-controlled shields; Carrier is a low enclosed streamlined vehicle. Arc Drone stays airborne and retains the blue energy visibly approved in the V2 PNG. This actor authority does not redefine world/layout or gameplay.

For Chapter 01, `docs/visual-blueprints/chapter01/CHAPTER01_VISUAL_BLUEPRINT_V1.md` is the current scene-composition authority. It intentionally supersedes the old practice of preserving V44–V47 physical presentation layout/collision fingerprint while merely replacing props.

## Current asset authority

Use `docs/ASSET_SOURCE_OF_TRUTH.md`.

Do not discover current assets by scanning old local caches, old worktrees or historical intake paths. Runtime references on current `main` and tracked project-owned source art are authoritative; the V2 intake documentation is a vetted reference library, not a live source-path contract.

## Current G-0 override

G-0 is a **bipedal robotic combat mech** in the current game. Any historical four-support, spider-like, radial or tank-like G-0 description is superseded.

The owner concept remains the visual-quality and world-composition target, except for that G-0 anatomy override and any later explicit owner correction.

The current V2 actor board already depicts bipedal G-0. Retain the approved current player direction and its distinction from the hostile faction; do not use the proof trio as scope to replace G-0.

## Next actor production contract

`docs/actor-production-v1/SCOUT_WARDEN_CARRIER_PRODUCTION_PROOF_BRIEF.md` defines **SCOUT_WARDEN_CARRIER_PRODUCTION_PROOF**: agile integrated-blade biped, heavy shield-carrying biped and non-biped vehicle. This is the next production task, not an asset implementation delivered by the authority package. PR #72's audit is pinned manufacturing evidence; PR #71's world work remains separate.

## Conflict-audit status

The current-documentation conflict audit is recorded in `docs/CURRENT_DOCUMENTATION_AUDIT.md`. If a future task discovers a conflict between CURRENT documents, fix the authority documents rather than adding another one-off phase rule.
