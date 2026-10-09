# AGENTS.md — Mandatory rules for Codex / coding agents

These rules are project constraints, not suggestions.

## 1. Decision hierarchy

When requirements appear ambiguous, use this priority:

1. Current explicit task from the human.
2. This `AGENTS.md`.
3. `docs/CURRENT_AUTHORITIES.md`.
4. Domain authority for the task: `docs/CURRENT_VISUAL_TARGET.md` for presentation/art work and `docs/ASSET_SOURCE_OF_TRUTH.md` for asset work.
5. `docs/DECISIONS.md`.
6. Current specification under `specs/`.
7. `docs/ARCHITECTURE.md`.
8. `docs/GAME_DESIGN.md`.
9. Existing code conventions.

Before substantial work, refresh these current-authority files from the branch you are actually implementing.

### Historical documentation is not instruction

Everything under `docs/history/` is historical evidence only. Never inherit requirements, visual targets, source paths, acceptance criteria or budgets from that directory unless the current task explicitly asks for historical comparison/reproduction.

A filename containing `Phase`, `V3`, `V43`, `Art Spike`, `Concept Fidelity`, `First Visual Slice` or similar does not make it current. Use `docs/CURRENT_AUTHORITIES.md` to determine current authority.

If ambiguity remains, choose the **smallest reversible implementation that still satisfies the current quality bar** and all higher-priority rules. For a production visual task, a primitive/proxy/blockout solution is not considered compliant merely because it is reversible.

Do not interrupt the user for routine implementation choices. Only ask when blocked by something that cannot be reasonably inferred or mocked, such as credentials, a private signing key, a legal/business identity required by an external service, or a destructive migration with no safe default.

## 2. Scope discipline

- Implement only the current specification/task and its explicit prerequisites.
- Do not "improve" unrelated systems.
- Do not add multiplayer, ECS/DOTS, backend services, auth, ads, payments or live-ops in v0.1 unless the active spec explicitly requires an interface/stub.
- Do not introduce a package just because it is convenient.
- No paid dependencies or assets unless a later explicit owner decision changes that rule.
- No code/assets extracted from Butcher Hero or any other commercial game.
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

### Presentation separation

Presentation may be visually rich while gameplay authority remains simple. Large buildings, machinery, animated presentation, materials, VFX and lighting do not need to own gameplay state.

Keep collision/interaction/spawn authority explicit and testable. Where appropriate, use gameplay collision proxies separate from complex presentation meshes. Do not downgrade required visual quality merely because presentation and gameplay are separated.

## 4. Content rules

Game entities are defined by data:
- enemy archetype;
- spawn spot;
- stat reward/core type;
- equipment item;
- quest/objective;
- evolution module;
- boss phase.

Adding a new ordinary enemy should primarily mean adding data/prefabs, not adding a new branch to a central switch statement.

Gameplay definitions do not define the maximum visual complexity of their presentation. For example, a spawn spot owns spawn behavior; its surrounding facility/industrial presentation may be much richer without changing spawn authority.

## 5. Visual rules

For all visual/presentation work, read `docs/CURRENT_VISUAL_TARGET.md` before implementation.

Key constraint: **low-poly is not the target aesthetic**. Low triangle count, LOD, atlasing and batching are optimization techniques only.

Do not accept obvious cubes, primitive stacks, generic proxy geometry, flat recolors, sparse test-floor composition or unchanged stock assets as production-quality art when the active task asks for concept fidelity.

Current G-0 is bipedal. Historical radial/four-support G-0 descriptions are superseded.

## 6. Asset rules

For any external/project-owned art task, read `docs/ASSET_SOURCE_OF_TRUTH.md`.

- New raw external downloads go only under `ExternalAssetIntake/Current/`.
- Do not treat `98_unclassified/`, `00_reference/`, `.local-g0-v2/`, old worktrees or `ExternalAssetIntake/FreeAssetIntakeV1/` as current source paths.
- Do not commit third-party raw/editable source into the public repository unless its exact license permits redistribution.
- Record provenance/license for every promoted third-party asset.

## 7. Performance rules

Target:
- 60 FPS on a reasonable mid-range Android device.
- No per-frame managed allocations in steady-state movement/combat.
- Pool repeated projectiles/VFX/floating text.
- Keep simultaneous live enemies for v0.1 under the configured cap (default 25).
- Avoid expensive transparent overdraw and unbounded realtime lighting.
- Quality scaling must be possible without rewriting gameplay.

Performance budgets are not art-style rules. Prefer profiling/device evidence over blindly preserving prototype triangle/material ceilings. A hero asset may be more complex if the real device remains within target and the gain is meaningful.

## 8. Testing rules

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

For visual work, automated tests do not replace the real-device human visual gate.

## 9. Agent completion report

Return:
- what changed;
- files changed;
- tests run and results;
- build result and APK path if built;
- known limitations;
- assumptions made;
- next spec/task id when defined.

Do not ask "what should I do next?" when the next task is defined. State it.

## 10. Forbidden shortcuts

Do not:
- hardcode progression into MonoBehaviours;
- use `Resources.Load` as the general content architecture;
- serialize runtime service references into saves;
- encode scene object instance IDs in saves;
- store purchases as trusted client booleans in future payment work;
- create fake "server validation" on device;
- copy third-party assets without recording their license/source;
- use historical documentation to override current authority;
- call proxy/blockout art "production quality" solely because it passes technical validation.
