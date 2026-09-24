# S20 BALANCE VERTICAL SLICE

## Goal

Tune the first 30–45 minutes to validate fun, pacing and visible growth.

## Scope

- enemy HP/damage
- respawns
- stat curves
- elite/boss requirements
- offline rate
- tutorial pacing
- evolution thresholds

## Out of scope

- monetization-driven friction
- chapter 2
- prestige

## Required architecture constraints

- Follow `AGENTS.md`.
- Use data/configuration rather than special-case branches where content may grow.
- Do not modify unrelated systems.
- Preserve backward-compatible save behavior once save exists.
- No paid dependencies/assets.

## Acceptance criteria

- [ ] Fresh competent tester reaches boss in intended window
- [ ] No ordinary progression spot creates long forced waiting
- [ ] First meaningful stat bump occurs within first minutes
- [ ] Two visual evolution moments occur before boss
- [ ] Boss is mostly stat-check but rewards movement

## Verification

- record one clean fresh-profile playthrough
- capture timings/deaths/stat levels at milestones

## Definition of Done

- Implementation compiles.
- Relevant tests pass, or unavailable execution is truthfully reported.
- No new missing references/errors in the target scene.
- Documentation/config updated when contracts or authoring steps changed.
- Agent completion report follows `AGENTS.md`.
