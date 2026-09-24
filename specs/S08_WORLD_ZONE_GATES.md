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

## Out of scope

- multiple chapters
- streaming world
- procedural map

## Required architecture constraints

- Follow `AGENTS.md`.
- Use data/configuration rather than special-case branches where content may grow.
- Do not modify unrelated systems.
- Preserve backward-compatible save behavior once save exists.
- No paid dependencies/assets.

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
