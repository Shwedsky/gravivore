# S16 LOCAL ANALYTICS DEVTOOLS

## Goal

Add development telemetry and cheat/debug controls without external analytics SDK.

## Scope

- local structured session events
- dev overlay FPS/entity count
- reset save
- grant stats
- unlock elite/boss
- god mode under DEVELOPMENT_BUILD

## Out of scope

- Firebase
- remote analytics
- production admin panel

## Required architecture constraints

- Follow `AGENTS.md`.
- Use data/configuration rather than special-case branches where content may grow.
- Do not modify unrelated systems.
- Preserve backward-compatible save behavior once save exists.
- No paid dependencies/assets.

## Acceptance criteria

- [ ] Debug controls unavailable/disabled in non-development configuration
- [ ] Events can diagnose tutorial/progression during private testing
- [ ] No PII collected

## Verification

- build-symbol guards
- event serialization unit test if used

## Definition of Done

- Implementation compiles.
- Relevant tests pass, or unavailable execution is truthfully reported.
- No new missing references/errors in the target scene.
- Documentation/config updated when contracts or authoring steps changed.
- Agent completion report follows `AGENTS.md`.
