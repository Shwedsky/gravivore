# S09 ELITE BOSS

## Goal

Implement one elite and one boss supporting 80/20 stat-to-skill combat.

## Scope

- Magnetar Guard elite
- Custodian M-0 boss
- boss phase/state machine
- circle pulse
- cone sweep
- line charge
- telegraphs
- arena reset
- boss completion event

## Out of scope

- raid systems
- multiple bosses
- random affixes

## Required architecture constraints

- Follow `AGENTS.md`.
- Use data/configuration rather than special-case branches where content may grow.
- Do not modify unrelated systems.
- Preserve backward-compatible save behavior once save exists.
- No paid dependencies/assets.

## Acceptance criteria

- [ ] Boss cannot be displaced
- [ ] Major attacks have readable telegraph before damage
- [ ] Leaving/reset/death returns boss to valid starting state
- [ ] Boss defeat triggers once and unlocks completion
- [ ] Adequately progressed player can win by dodging most telegraphs

## Verification

- boss state transitions where possible
- telegraph-before-damage play test
- completion idempotency

## Definition of Done

- Implementation compiles.
- Relevant tests pass, or unavailable execution is truthfully reported.
- No new missing references/errors in the target scene.
- Documentation/config updated when contracts or authoring steps changed.
- Agent completion report follows `AGENTS.md`.
