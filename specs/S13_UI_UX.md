# S13 UI UX

## Goal

Deliver readable portrait HUD and menus for the vertical slice.

## Scope

- HP
- five stat summary/access
- objective tracker
- boss HP
- pause/settings
- offline reward panel
- chapter completion panel
- safe areas
- basic accessibility/readability

## Out of scope

- full shop
- account screen
- ads UI
- live event UI

## Required architecture constraints

- Follow `AGENTS.md`.
- Use data/configuration rather than special-case branches where content may grow.
- Do not modify unrelated systems.
- Preserve backward-compatible save behavior once save exists.
- No paid dependencies/assets.

## Acceptance criteria

- [ ] Core HUD readable on 9:16 and tall Android ratios
- [ ] No essential controls overlap system safe areas
- [ ] Combat does not require opening menus
- [ ] Stat changes have clear feedback
- [ ] Boss telegraphs remain visible under HUD

## Verification

- basic presenter tests if practical
- manual aspect-ratio checklist

## Definition of Done

- Implementation compiles.
- Relevant tests pass, or unavailable execution is truthfully reported.
- No new missing references/errors in the target scene.
- Documentation/config updated when contracts or authoring steps changed.
- Agent completion report follows `AGENTS.md`.
