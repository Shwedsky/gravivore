# S14 VFX AUDIO HAPTICS

## Goal

Make core combat and progression satisfying using free/original lightweight presentation.

## Scope

- gravity lash VFX
- hit/death
- assimilation
- evolution burst
- boss telegraph/damage separation
- simple free audio or generated project-owned placeholders
- optional Android haptic abstraction

## Out of scope

- licensed paid music
- complex post-processing

## Required architecture constraints

- Follow `AGENTS.md`.
- Use data/configuration rather than special-case branches where content may grow.
- Do not modify unrelated systems.
- Preserve backward-compatible save behavior once save exists.
- No paid dependencies/assets.

## Acceptance criteria

- [ ] Attack wind-up/hit/death are distinguishable
- [ ] Telegraph is not confused with damage moment
- [ ] VFX pooled where repeated
- [ ] No required paid assets

## Verification

- pool reuse smoke
- manual readability/performance

## Definition of Done

- Implementation compiles.
- Relevant tests pass, or unavailable execution is truthfully reported.
- No new missing references/errors in the target scene.
- Documentation/config updated when contracts or authoring steps changed.
- Agent completion report follows `AGENTS.md`.
