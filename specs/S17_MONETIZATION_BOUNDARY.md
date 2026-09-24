# S17 MONETIZATION BOUNDARY

## Goal

Prepare interfaces/data boundaries for future ads and RuStore without integrating vendor SDKs.

## Scope

- product/entitlement domain types only if needed
- IPurchaseService interface stub
- IRewardedAdService interface stub
- shop feature flag off
- documentation of future RuStore adapter

## Out of scope

- RuStore package import
- real transactions
- server
- ads SDK

## Required architecture constraints

- Follow `AGENTS.md`.
- Use data/configuration rather than special-case branches where content may grow.
- Do not modify unrelated systems.
- Preserve backward-compatible save behavior once save exists.
- No paid dependencies/assets.

## Acceptance criteria

- [ ] v0.1 has no visible/functional purchase path
- [ ] Gameplay code is not coupled to RuStore
- [ ] Future entitlement can be injected without rewriting progression core

## Verification

- fake provider contract tests only if implementation introduced

## Definition of Done

- Implementation compiles.
- Relevant tests pass, or unavailable execution is truthfully reported.
- No new missing references/errors in the target scene.
- Documentation/config updated when contracts or authoring steps changed.
- Agent completion report follows `AGENTS.md`.
