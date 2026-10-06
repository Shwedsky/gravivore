# First playable visual slice — APK v1

Base: `a0aacf33d8739d167e3dd81e94ef97efe1f333cc` (verified origin/main).
Branch: `integration/first-visual-slice-apk-v1`.

## Delivery contract

Integrate the merged G-0 Production V3 at its accepted 1.10 presentation scale,
original Scout/Cutter/Magnetar production visuals, and a dense industrial rebuild
of the Chapter01 pre-Magnetar approach and encounter. Preserve gameplay authority,
normal progression, saves, Russian UI, combat, audio, VFX, and repeatability.

The next human visual gate is the installable ARM64 DEV APK. There are no
intermediate human art gates. Internal validation and visual checks continue
through the Android build. Commit and push at each major checkpoint.

## Required release checks

- Compile and project validation.
- Relevant EditMode and PlayMode tests.
- Build dependency verification for all five production visual dependencies.
- Android ARM64 DEV APK, `com.gravivore.mobile.dev`.
- APK metadata, SHA256, size, exact path, and build/source revision recorded here.

Status: implementation in progress; no test/build success is claimed yet.
