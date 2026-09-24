# AGENTS.md — Mandatory rules for Codex / coding agents

These rules are project constraints, not suggestions.

## 1. Decision hierarchy

When requirements appear ambiguous, use this priority:
1. Current explicit task from the human.
2. This `AGENTS.md`.
3. `docs/DECISIONS.md`.
4. Current specification under `specs/`.
5. `docs/ARCHITECTURE.md`.
6. `docs/GAME_DESIGN.md`.
7. Existing code conventions.

If ambiguity remains, choose the **smallest reversible implementation** consistent with all higher-priority rules and record the assumption in the completion report. Do not interrupt the user for routine implementation choices.

Only ask the user when blocked by something that cannot be reasonably inferred or mocked, such as credentials, a private signing key, a legal/business identity required by an external service, or a destructive migration with no safe default.

## 2. Scope discipline

- Implement only the current specification and its explicit prerequisites.
- Do not "improve" unrelated systems.
- Do not add multiplayer, ECS/DOTS, backend services, auth, ads, payments or live-ops in v0.1 unless the active spec explicitly requires an interface/stub.
- Do not introduce a package just because it is convenient.
- No paid dependencies or assets.
- No code/assets extracted from Butcher Hero or any other game.
- Do not use Dota/Pudge names, models, sounds, icons or derivative art.

## 3. Architecture rules

- Unity 6.3 LTS.
- C#.
- URP.
- Portrait Android.
- Data-driven content.
- Runtime logic must not depend on a specific scene name where an injected reference/config can be used.
- Balance values must live in configuration/ScriptableObjects, not scattered magic numbers.
- UI may observe/application-call gameplay systems; gameplay domain code must not depend on UI.
- Platform integrations must sit behind interfaces.
- Save schema must be versioned and migrated.
- `PlayerPrefs` may store user settings only, never progression.
- No global `GameManager` god object.
- No scene-wide `FindObjectOfType`/`FindFirstObjectByType` as normal dependency injection.
- Prefer serialized references, small installers/composition root, constructor injection for plain C# services, and explicit runtime initialization.
- Avoid static mutable gameplay state.
- No silent exception swallowing.
- Cancellation/lifecycle must be handled for asynchronous work.
- Use object pooling where repeated combat VFX/projectiles would otherwise allocate frequently.

## 4. Content rules

Game entities are defined by data:
- enemy archetype
- spawn spot
- stat reward/core type
- equipment item
- quest/objective
- evolution module
- boss phase

Adding a new ordinary enemy should primarily mean adding data/prefabs, not adding a new branch to a central switch statement.

## 5. Performance rules

Target:
- 60 FPS on a reasonable mid-range Android device.
- No per-frame managed allocations in steady-state movement/combat.
- Pool repeated projectiles/VFX/floating text.
- Keep simultaneous live enemies for v0.1 under the configured cap (default 25).
- Mobile-friendly meshes/materials; avoid expensive transparent overdraw.
- Use baked/simple lighting for the slice where possible.
- Quality scaling must be possible without rewriting gameplay.

## 6. Testing rules

Every spec must add/update tests where the behavior is testable outside presentation.
At minimum:
- edit-mode tests for deterministic domain calculations;
- play-mode smoke tests for scene wiring and critical flows where practical.

Before marking a spec complete:
1. compile;
2. run relevant tests;
3. run project validation;
4. if Unity CLI is available, build the dev Android APK;
5. report any step that could not run and why.

Never claim a build/test passed if it was not executed.

## 7. Agent completion report

Return:
- what changed;
- files changed;
- tests run and results;
- build result and APK path if built;
- known limitations;
- assumptions made;
- next spec id.

Do not ask "what should I do next?" when the next spec is defined. State it.

## 8. Forbidden shortcuts

Do not:
- hardcode progression into MonoBehaviours;
- use `Resources.Load` as the general content architecture;
- serialize runtime service references into saves;
- encode scene object instance IDs in saves;
- store purchases as trusted client booleans in future payment work;
- create fake "server validation" on device;
- copy third-party assets without recording their license/source.
