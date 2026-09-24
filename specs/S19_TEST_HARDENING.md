# S19 TEST HARDENING

## Goal

Harden the slice before gameplay evaluation.

## Scope

- complete required edit/play tests
- missing-reference validator
- save interruption checks
- pool/leak checks
- 15-minute max-population profiling pass
- bug fixes limited to slice

## Out of scope

- new features
- new content chapter

## Required architecture constraints

- Follow `AGENTS.md`.
- Use data/configuration rather than special-case branches where content may grow.
- Do not modify unrelated systems.
- Preserve backward-compatible save behavior once save exists.
- No paid dependencies/assets.

## Acceptance criteria

- [ ] No known blocker/critical defects
- [ ] No duplicate reward exploit from normal lifecycle
- [ ] No missing scripts/references
- [ ] Steady combat has no obvious recurring GC spikes from project code
- [ ] APK survives background/resume

## Verification

- full suite
- manual device checklist
- profiling notes

## Definition of Done

- Implementation compiles.
- Relevant tests pass, or unavailable execution is truthfully reported.
- No new missing references/errors in the target scene.
- Documentation/config updated when contracts or authoring steps changed.
- Agent completion report follows `AGENTS.md`.
