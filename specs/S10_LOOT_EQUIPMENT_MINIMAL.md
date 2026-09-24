# S10 LOOT EQUIPMENT MINIMAL

## Goal

Add a deliberately small equipment framework without making it the vertical slice's primary progression.

## Scope

- 3 equipment slots maximum for v0.1
- EquipmentDefinition
- flat/simple modifiers
- inventory state
- equip/unequip
- one or two test items per relevant slot
- save persistence

## Out of scope

- random affix generator
- rarity treadmill
- crafting
- item evolution trees

## Required architecture constraints

- Follow `AGENTS.md`.
- Use data/configuration rather than special-case branches where content may grow.
- Do not modify unrelated systems.
- Preserve backward-compatible save behavior once save exists.
- No paid dependencies/assets.

## Acceptance criteria

- [ ] Equipment modifiers compose with base stats without mutating base levels
- [ ] Equip swap updates derived stats immediately
- [ ] Inventory/equipment survives reload
- [ ] System is data-driven

## Verification

- modifier composition
- equip swap
- save mapping

## Definition of Done

- Implementation compiles.
- Relevant tests pass, or unavailable execution is truthfully reported.
- No new missing references/errors in the target scene.
- Documentation/config updated when contracts or authoring steps changed.
- Agent completion report follows `AGENTS.md`.
