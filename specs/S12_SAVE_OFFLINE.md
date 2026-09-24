# S12 SAVE OFFLINE

## Goal

Implement resilient versioned local save and capped offline reward.

## Scope

- JSON save DTO
- schema version
- atomic temp/backup strategy
- migration pipeline
- ITimeProvider
- lastSeenUtc
- 2h cap
- offline reward calculation
- return summary model

## Out of scope

- cloud save
- anti-cheat server time
- account auth

## Required architecture constraints

- Follow `AGENTS.md`.
- Use data/configuration rather than special-case branches where content may grow.
- Do not modify unrelated systems.
- Preserve backward-compatible save behavior once save exists.
- No paid dependencies/assets.

## Acceptance criteria

- [ ] Kill app/relaunch preserves progression
- [ ] Corrupt main save can recover from valid backup
- [ ] Offline reward never exceeds cap
- [ ] Negative/implausible local clock delta yields no negative reward and is clamped/logged
- [ ] Gameplay code does not call DateTime.UtcNow directly

## Verification

- save round trip
- migration fixture
- backup recovery
- offline cap/clock anomaly tests

## Definition of Done

- Implementation compiles.
- Relevant tests pass, or unavailable execution is truthfully reported.
- No new missing references/errors in the target scene.
- Documentation/config updated when contracts or authoring steps changed.
- Agent completion report follows `AGENTS.md`.
