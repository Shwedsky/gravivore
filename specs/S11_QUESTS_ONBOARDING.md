# S11 QUESTS ONBOARDING

## Goal

Guide first-time player through movement, first kill, five spot objectives, elite and boss without text walls.

## Scope

- QuestDefinition/objective types
- onboarding sequence
- objective tracker
- world markers
- per-spot introductory objective
- elite unlock condition integration

## Out of scope

- daily quests
- battle pass
- dialogue system

## Required architecture constraints

- Follow `AGENTS.md`.
- Use data/configuration rather than special-case branches where content may grow.
- Do not modify unrelated systems.
- Preserve backward-compatible save behavior once save exists.
- No paid dependencies/assets.

## Acceptance criteria

- [ ] Fresh player receives movement guidance
- [ ] First kill explains assimilation through feedback
- [ ] Each spot objective can progress exactly once as intended
- [ ] Elite cannot unlock from farming one single spot only
- [ ] Onboarding state persists

## Verification

- objective rule tests
- unlock aggregation tests
- play-mode onboarding smoke

## Definition of Done

- Implementation compiles.
- Relevant tests pass, or unavailable execution is truthfully reported.
- No new missing references/errors in the target scene.
- Documentation/config updated when contracts or authoring steps changed.
- Agent completion report follows `AGENTS.md`.
