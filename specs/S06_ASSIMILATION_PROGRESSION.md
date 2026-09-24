# S06 ASSIMILATION PROGRESSION

## Goal

Grant permanent stat progression from defeated enemy archetypes.

## Scope

- core reward definition
- reward grant pipeline
- stat XP/levels
- first-kill feedback hooks
- total assimilation score
- save dirty notification

## Out of scope

- premium currency
- gacha
- prestige

## Required architecture constraints

- Follow `AGENTS.md`.
- Use data/configuration rather than special-case branches where content may grow.
- Do not modify unrelated systems.
- Preserve backward-compatible save behavior once save exists.
- No paid dependencies/assets.

## Acceptance criteria

- [ ] Each of five enemy archetypes advances the correct stat
- [ ] Reward is authoritative even if pickup VFX fails
- [ ] A death cannot grant twice
- [ ] Progress persists through service state
- [ ] Total assimilation increments deterministically

## Verification

- reward routing
- level-up threshold
- duplicate event protection

## Definition of Done

- Implementation compiles.
- Relevant tests pass, or unavailable execution is truthfully reported.
- No new missing references/errors in the target scene.
- Documentation/config updated when contracts or authoring steps changed.
- Agent completion report follows `AGENTS.md`.
