# S02 PLAYER STATS

## Goal

Create data-driven player stat state and derived values.

## Scope

- Power/Hull/Armor/Flux/Mobility state
- stat curve definitions
- derived MaxHP/damage/attack interval/move speed
- caps/validation
- change events

## Out of scope

- enemy rewards
- UI polish
- equipment modifiers beyond extension hooks

## Required architecture constraints

- Follow `AGENTS.md`.
- Use data/configuration rather than special-case branches where content may grow.
- Do not modify unrelated systems.
- Preserve backward-compatible save behavior once save exists.
- No paid dependencies/assets.

## Acceptance criteria

- [ ] Stats have integer levels and configured effects
- [ ] No balance constants are duplicated across controllers
- [ ] Flux cannot reduce attack interval below configured minimum
- [ ] Mobility respects configured cap

## Verification

- curve tests
- caps tests
- serialization mapping tests if DTO exists

## Definition of Done

- Implementation compiles.
- Relevant tests pass, or unavailable execution is truthfully reported.
- No new missing references/errors in the target scene.
- Documentation/config updated when contracts or authoring steps changed.
- Agent completion report follows `AGENTS.md`.
