# S08 WORLD ZONE GATES

## Goal

Create Chapter01 Scrap Exclusion layout and progression gates.

## Scope

- central basin
- five readable farming spots
- paths/loop
- elite gate
- boss arena/gate
- world unlock state
- basic navigation blockers

## Presentation boundary

This spec freezes gameplay topology and unlock semantics, not a low-detail visual style.

Current/future presentation may use substantial industrial buildings, machinery, multi-layer walls/gates, gantries, wrecks, service networks and hero landmarks around the same gameplay topology. Use presentation meshes plus explicit gameplay collision proxies where appropriate.

A visually rich zone is compatible with a simple data-driven world definition as long as required traversal, combat space, telegraphs and authority remain correct.

For current visual work follow `docs/CURRENT_VISUAL_TARGET.md`.

## Out of scope

- multiple chapters
- streaming world
- procedural map

## Required architecture constraints

- Follow `AGENTS.md`.
- Use data/configuration rather than special-case branches where content may grow.
- Do not modify unrelated systems.
- Preserve backward-compatible save behavior once save exists.
- No paid dependencies/assets unless the owner explicitly changes the zero-spend decision.

## Acceptance criteria

- [ ] All five ordinary spots are reachable from hub
- [ ] Player can understand spot separation visually
- [ ] Elite gate remains locked until condition
- [ ] Boss gate unlocks after elite
- [ ] Save/reload preserves unlocked gates

## Verification

- gate rule tests
- play-mode gate persistence smoke

## Definition of Done

- Implementation compiles.
- Relevant tests pass, or unavailable execution is truthfully reported.
- No new missing references/errors in the target scene.
- Documentation/config updated when contracts or authoring steps changed.
- Agent completion report follows `AGENTS.md`.
