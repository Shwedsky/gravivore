# S07 VISUAL EVOLUTION

## Goal

Make permanent growth visibly alter the player at two milestones.

## Scope

- EvolutionDefinition
- Tier0/1/2 selection
- attachment sockets
- module prefab activation
- dominant-stat accent selection
- evolution VFX hook

## Out of scope

- runtime mesh fusion
- paid skins
- deep character creator

## Required architecture constraints

- Follow `AGENTS.md`.
- Use data/configuration rather than special-case branches where content may grow.
- Do not modify unrelated systems.
- Preserve backward-compatible save behavior once save exists.
- No paid dependencies/assets.

## Acceptance criteria

- [ ] Tier1 and Tier2 are visually obvious
- [ ] Reload restores correct evolution
- [ ] Dominant stat can alter at least one accent/module without corrupting tier
- [ ] Evolution is presentation driven by progression state

## Verification

- tier threshold tests
- play-mode reload/presenter smoke

## Definition of Done

- Implementation compiles.
- Relevant tests pass, or unavailable execution is truthfully reported.
- No new missing references/errors in the target scene.
- Documentation/config updated when contracts or authoring steps changed.
- Agent completion report follows `AGENTS.md`.
