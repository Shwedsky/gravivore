# S03 TARGETING GRAVITY ATTACK

## Goal

Implement sticky automatic targeting and Gravity Lash auto-attack.

## Scope

- candidate detection
- front/distance scoring
- target stickiness
- attack cadence
- damage request
- pull capability contract
- standard/elite/boss displacement classes
- basic pooled lash VFX placeholder

## Out of scope

- boss attack AI
- final VFX/audio
- manual attack button

## Required architecture constraints

- Follow `AGENTS.md`.
- Use data/configuration rather than special-case branches where content may grow.
- Do not modify unrelated systems.
- Preserve backward-compatible save behavior once save exists.
- No paid dependencies/assets.

## Acceptance criteria

- [ ] Player attacks automatically only with a valid in-range target
- [ ] Target does not flicker rapidly between near-equal enemies
- [ ] Standard enemy can be pulled safely
- [ ] Boss is never displaced
- [ ] Attack stops when target dies/leaves release radius
- [ ] No wall-clipping pull in test layout

## Verification

- target scoring tests
- cadence tests
- displacement policy tests
- play-mode auto-attack smoke

## Definition of Done

- Implementation compiles.
- Relevant tests pass, or unavailable execution is truthfully reported.
- No new missing references/errors in the target scene.
- Documentation/config updated when contracts or authoring steps changed.
- Agent completion report follows `AGENTS.md`.
