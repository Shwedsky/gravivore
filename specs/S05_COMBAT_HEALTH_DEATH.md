# S05 COMBAT HEALTH DEATH

## Goal

Complete deterministic health, armor, damage, death and player respawn flow.

## Scope

- DamageRequest/Result
- armor formula
- Health component/state
- enemy death event
- player death/central respawn
- invulnerability window if needed after respawn
- combat presentation hooks

## Out of scope

- complex elemental system
- status effects
- resurrection monetization

## Required architecture constraints

- Follow `AGENTS.md`.
- Use data/configuration rather than special-case branches where content may grow.
- Do not modify unrelated systems.
- Preserve backward-compatible save behavior once save exists.
- No paid dependencies/assets.

## Acceptance criteria

- [ ] Damage formula matches documented rules
- [ ] Death fires once
- [ ] Dead enemy cannot deal/receive normal combat actions
- [ ] Player respawns without losing permanent stats
- [ ] No duplicate death rewards

## Verification

- damage math
- death idempotency
- player respawn smoke

## Definition of Done

- Implementation compiles.
- Relevant tests pass, or unavailable execution is truthfully reported.
- No new missing references/errors in the target scene.
- Documentation/config updated when contracts or authoring steps changed.
- Agent completion report follows `AGENTS.md`.
