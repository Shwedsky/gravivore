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

## Presentation boundary

`SpawnSpotDefinition` owns spawn behavior, not the artistic size or complexity of the surrounding world.

A spot may be presented by substantial authored industrial architecture, deployment/maintenance machinery, docks, wrecks or facilities while spawn anchors/population/timers remain unchanged. Presentation should follow `docs/CURRENT_VISUAL_TARGET.md` and keep collision/gameplay authority explicit.

Do not interpret the simple spawn data model as a requirement for a simple visual spawn marker.

## Out of scope

- elite/boss bespoke mechanics
- final art for the original S04 implementation
- procedural world generation

## Required architecture constraints

- Follow `AGENTS.md`.
- Use data/configuration rather than special-case branches where content may grow.
- Do not modify unrelated systems.
- Preserve backward-compatible save behavior once save exists.
- No paid dependencies/assets unless the owner explicitly changes the zero-spend decision.

## Acceptance criteria

- [ ] Five configured spots can coexist
- [ ] Each spot maintains configured live population over time
- [ ] Enemy reuse from pool has clean HP/state
- [ ] No spawn occurs directly on top of player
- [ ] Global cap is respected

## Verification

- state transitions where separable
- pool reset play test
- spawn population play test

## Definition of Done

- Implementation compiles.
- Relevant tests pass, or unavailable execution is truthfully reported.
- No new missing references/errors in the target scene.
- Documentation/config updated when contracts or authoring steps changed.
- Agent completion report follows `AGENTS.md`.
