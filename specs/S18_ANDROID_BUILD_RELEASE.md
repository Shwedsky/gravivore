# S18 ANDROID BUILD RELEASE

## Goal

Create repeatable one-command private APK output on Windows.

## Scope

- build-android.ps1
- AndroidBuild editor class
- versioning
- logs
- dev/candidate flavors
- validation pre-build

## Out of scope

- Play/RuStore publishing
- release keystore secrets
- store listing

## Required architecture constraints

- Follow `AGENTS.md`.
- Use data/configuration rather than special-case branches where content may grow.
- Do not modify unrelated systems.
- Preserve backward-compatible save behavior once save exists.
- No paid dependencies/assets.

## Acceptance criteria

- [ ] Script either builds APK or exits non-zero with actionable error
- [ ] Output follows naming convention
- [ ] Build does not require manual Editor clicking once environment exists
- [ ] No secrets committed

## Verification

- run build when Unity available
- install smoke on at least one physical Android device manually

## Definition of Done

- Implementation compiles.
- Relevant tests pass, or unavailable execution is truthfully reported.
- No new missing references/errors in the target scene.
- Documentation/config updated when contracts or authoring steps changed.
- Agent completion report follows `AGENTS.md`.
