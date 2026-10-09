# S08 WORLD ZONE GATES

## Goal

Create Chapter01 Scrap Exclusion layout and progression gates.

## Scope

- central basin / Repair Hub semantics
- five readable farming spots
- paths/loop
- elite gate
- boss arena/gate
- world unlock state
- explicit navigation blockers / collision proxies

## Gameplay-semantics boundary

This spec freezes **progression and gameplay semantics**, not the historical physical map geometry.

A Chapter/world visual rebuild may change:

- exact sector coordinates;
- old path shapes and lane positions;
- old wall/floor layout;
- facility footprints;
- presentation hierarchy;
- gate framing and transition geometry;
- gameplay collision proxies when rebuilt deliberately;
- minimap geometry derived from the rebuilt world.

It must preserve:

- one safe Repair Hub/start function;
- five ordinary encounter identities and their objective/progression meaning;
- reachability of required content;
- elite gate condition/flow;
- boss gate condition/flow;
- save/unlock identity;
- readable combat and telegraph space;
- practical Chapter traversal length/tempo unless an explicit design task changes it.

Do **not** require the old collision fingerprint, old transform coordinates or V44–V47 presentation layout to remain identical during an explicitly approved world rebuild.

Instead, rebuild explicit collision/navigation proxies for the new authored layout and verify traversal afterward.

## Presentation boundary

Current/future presentation may use substantial industrial buildings, machinery, multi-layer walls/gates, gantries, wrecks, service networks and hero landmarks.

A visually rich zone is compatible with a simple data-driven world definition as long as required traversal, combat space, telegraphs and authority remain correct.

For current visual work follow `docs/CURRENT_VISUAL_TARGET.md`.

For the next Chapter 01 full rebuild also follow:

`docs/visual-blueprints/chapter01/CHAPTER01_VISUAL_BLUEPRINT_V1.md`

The blueprint is allowed to replace the old physical scene composition rather than layering another visual pass on top of it.

## Out of scope

- multiple chapters
- streaming world
- procedural map
- changing combat/progression balance merely to make a new scene easier to author

## Required architecture constraints

- Follow `AGENTS.md`.
- Use data/configuration rather than special-case branches where content may grow.
- Do not modify unrelated systems.
- Preserve backward-compatible save behavior once save exists.
- No paid dependencies/assets unless the owner explicitly changes the zero-spend decision.
- Keep gameplay authority explicit even when presentation meshes become much richer.

## Acceptance criteria

- [ ] All five ordinary encounter identities are reachable from the hub in the rebuilt world
- [ ] Player can understand spot/sector separation visually
- [ ] Elite gate remains locked until its configured condition
- [ ] Boss gate unlocks after elite according to current progression rules
- [ ] Save/reload preserves unlocked gates and stable content identity
- [ ] Collision proxies match the rebuilt authored layout rather than stale historical geometry
- [ ] Fast player traversal does not snag on decorative/minor presentation clutter
- [ ] The rebuilt minimap/world representation matches the new layout

## Verification

- gate rule tests
- play-mode gate persistence smoke
- rebuilt-world traversal smoke
- collision/blocked-route review
- objective reachability across all five ordinary sectors + elite + boss
- minimap/world-layout consistency check

## Definition of Done

- Implementation compiles.
- Relevant tests pass, or unavailable execution is truthfully reported.
- No new missing references/errors in the target scene.
- Documentation/config updated when contracts or authoring steps changed.
- Agent completion report follows `AGENTS.md`.