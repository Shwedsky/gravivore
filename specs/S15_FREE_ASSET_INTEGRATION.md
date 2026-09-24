# S15 FREE ASSET INTEGRATION

## Goal

Replace primitive placeholders selectively with coherent zero-cost commercially usable assets.

## Scope

- license verification
- ThirdPartyNotices.md
- one consistent environment family
- player base/kitbash
- five enemy silhouettes using free sources/variants
- animation retarget/import settings

## Out of scope

- paid packs
- ripped content
- unlicensed AI marketplace downloads

## Required architecture constraints

- Follow `AGENTS.md`.
- Use data/configuration rather than special-case branches where content may grow.
- Do not modify unrelated systems.
- Preserve backward-compatible save behavior once save exists.
- No paid dependencies/assets.

## Acceptance criteria

- [ ] Every third-party imported directory has documented provenance/license
- [ ] No paid asset required for build
- [ ] Silhouettes distinguish five enemy roles
- [ ] Asset import remains mobile friendly

## Verification

- editor validation for missing materials/animations
- manual license audit

## Definition of Done

- Implementation compiles.
- Relevant tests pass, or unavailable execution is truthfully reported.
- No new missing references/errors in the target scene.
- Documentation/config updated when contracts or authoring steps changed.
- Agent completion report follows `AGENTS.md`.
