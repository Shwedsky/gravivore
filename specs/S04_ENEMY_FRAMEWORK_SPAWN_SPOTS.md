# S04 ENEMY FRAMEWORK SPAWN SPOTS

## Goal

Build reusable ordinary enemy AI and five-spot spawner architecture.

## Scope

- EnemyDefinition
- ordinary enemy state machine
- aggro/approach/basic attack
- pool reset
- SpawnSpotDefinition
- anchors
- desired population 3–5
- respawn jitter
- global cap

## Presentation and world-layout boundary

`SpawnSpotDefinition` owns **encounter/spawn semantics**, not permanent world coordinates and not the artistic size/shape of the surrounding sector.

A spot may be presented by substantial authored industrial architecture, deployment/maintenance machinery, docks, wrecks or facilities while population/timers/reward/progression identity remain unchanged.

For an explicitly approved Chapter/world rebuild, spawn anchors may be repositioned inside the rebuilt sector when required by the new visual blueprint. The rebuild must preserve:

- the same logical encounter/spot identity;
- configured enemy archetype and population semantics;
- respawn rules/jitter/caps;
- progression/objective references;
- minimum safe distance from the player;
- reachable and readable combat space;
- save compatibility by stable content identity rather than scene coordinates.

Do not interpret old anchor transforms, old spawn-marker footprints or an old scene collision fingerprint as permanent art/layout authority.

Presentation should follow `docs/CURRENT_VISUAL_TARGET.md`. Chapter 01 rebuild work must also follow `docs/visual-blueprints/chapter01/CHAPTER01_VISUAL_BLUEPRINT_V1.md` and keep gameplay/collision authority explicit.

Do not interpret the simple spawn data model as a requirement for a simple visual spawn marker.

## Out of scope

- elite/boss bespoke mechanics
- final art for the original S04 implementation
- procedural world generation
- changing encounter balance merely to simplify a visual rebuild

## Required architecture constraints

- Follow `AGENTS.md`.
- Use data/configuration rather than special-case branches where content may grow.
- Do not modify unrelated systems.
- Preserve backward-compatible save behavior once save exists.
- No paid dependencies/assets unless the owner explicitly changes the zero-spend decision.

## Acceptance criteria

- [ ] Five configured ordinary spot identities can coexist
- [ ] Each spot maintains configured live population over time
- [ ] Enemy reuse from pool has clean HP/state
- [ ] No spawn occurs directly on top of player
- [ ] Global cap is respected
- [ ] A rebuilt Chapter may relocate anchors without changing encounter identity/progression semantics
- [ ] Current visual blueprint facilities do not own spawn authority

## Verification

- state transitions where separable
- pool reset play test
- spawn population play test
- after a world rebuild: verify all five logical spot ids, objectives and respawn behavior at their new authored locations

## Definition of Done

- Implementation compiles.
- Relevant tests pass, or unavailable execution is truthfully reported.
- No new missing references/errors in the target scene.
- Documentation/config updated when contracts or authoring steps changed.
- Agent completion report follows `AGENTS.md`.