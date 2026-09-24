# S01 INPUT CAMERA MOVEMENT

## Goal

Implement one-thumb portrait movement and stable follow camera.

## Scope

- floating joystick touch input
- mouse simulation in Editor
- player locomotion
- rotation toward movement
- camera follow/damping
- UI touch exclusion
- safe-area aware HUD root

## Out of scope

- attacking
- enemies
- dash/manual skills
- camera rotation

## Required architecture constraints

- Follow `AGENTS.md`.
- Use data/configuration rather than special-case branches where content may grow.
- Do not modify unrelated systems.
- Preserve backward-compatible save behavior once save exists.
- No paid dependencies/assets.

## Acceptance criteria

- [ ] Touch drag moves player analogly
- [ ] No input means no auto movement
- [ ] Dragging UI does not move player
- [ ] Editor mouse can test movement
- [ ] Player remains controllable at portrait aspect ratios

## Verification

- input vector normalization/dead zone edit test where separable
- play-mode spawn/move smoke test

## Definition of Done

- Implementation compiles.
- Relevant tests pass, or unavailable execution is truthfully reported.
- No new missing references/errors in the target scene.
- Documentation/config updated when contracts or authoring steps changed.
- Agent completion report follows `AGENTS.md`.
