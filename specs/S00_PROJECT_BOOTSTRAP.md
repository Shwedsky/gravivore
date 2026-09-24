# S00 PROJECT BOOTSTRAP

## Goal

Create a clean Unity 6.3 LTS URP Android project foundation that compiles and can be built by script.

## Scope

- Project/folder layout and asmdefs
- URP project baseline
- portrait Android PlayerSettings
- Bootstrap and Chapter01 placeholder scenes
- root build-android.ps1
- Editor AndroidBuild entry point
- project validation command
- basic README/version constant

## Out of scope

- Gameplay systems
- real enemies/combat
- payments/ads/backend
- third-party art downloads

## Required architecture constraints

- Follow `AGENTS.md`.
- Use data/configuration rather than special-case branches where content may grow.
- Do not modify unrelated systems.
- Preserve backward-compatible save behavior once save exists.
- No paid dependencies/assets.

## Acceptance criteria

- [ ] Project opens in Unity 6.3 LTS without compile errors
- [ ] Portrait Android settings are applied
- [ ] ARM64 Android build can be started from build-android.ps1
- [ ] Build output naming follows docs/BUILD_AND_RELEASE.md
- [ ] No package beyond Unity/official required baseline is added without justification

## Verification

- Editor compile
- build entry-point validation
- if Unity CLI available, produce an installable dev APK

## Definition of Done

- Implementation compiles.
- Relevant tests pass, or unavailable execution is truthfully reported.
- No new missing references/errors in the target scene.
- Documentation/config updated when contracts or authoring steps changed.
- Agent completion report follows `AGENTS.md`.
