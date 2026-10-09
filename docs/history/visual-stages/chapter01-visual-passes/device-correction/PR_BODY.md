The accepted SOLID B visual slice now includes the owner's device corrections: G-0 faces movement correctly, grows by 15%/22%/30% across tiers, and gains original articulated armor with distinct silhouettes. Custodian access changes no longer teleport/full-heal an active boss; arena exits use the existing grace timer. The rebuilt slice and successful elite/boss danger-zone language are retained.

The same playable Chapter01 section also receives a tactical Russian minimap skin and gate markers, original 72-second exploration/combat music with fades and a softer SFX mix, richer industrial color and service markings, short bounded cosmetic gravity pulls, and three pooled player attack presentations plus enemy strike cues. Gameplay colliders, attack balance, camera, progression, saves and encounter layout retain their authority. Original Blender sources and procedural music composition scripts are included.

The new installable ARM64 DEV APK is verified:

- Source: `5895497dca108a1e69b0e477b74bd470d21423f4`.
- Unity 6000.3.0f1, IL2CPP, ARM64 only, `com.gravivore.mobile.dev`, 0.1.0 / versionCode **37**.
- Size: **66,765,975 bytes (63.67 MiB)**.
- SHA256: `3a6c4359e02181a48f660a99f2ad2d23e3c9c5d29843ee85a5f16e134e8622b2`.
- Signature, debuggable manifest, ARM64 libraries and required packed assets verified; signer matches accepted APK-v1.

Validation:

- Compilation and independent ProjectValidator passed; validation also ran in the successful Android build.
- Full EditMode: **411 passed, 0 failed**.
- Full PlayMode: **102 passed, 0 failed, 1 optional capture skipped**. The correction camera test ran.
- Regression coverage includes facing/tier size/collider, boss exit/access semantics, pull boundedness, music/fades/mute, minimap, three variants/pool reuse, telegraphs, respawns/deaths, gates, repeats, Russian HUD and save/reload.
- Nine required scene dependencies, nine model sources and fourteen serialized APK entries verified against this APK's SHA256.

Exact APK path, changed files, original sources, internal QA, test/build logs and disk recovery are recorded in `docs/device-correction/DELIVERY.md`. Three build attempts exhausted disk space; scoped generated-cache cleanup/compression allowed the final build to succeed. The primary dirty checkout and APK-v1 were preserved. Device FPS, speaker balance, touch feel and final B+/A- acceptance await phone play. This remains the same Draft PR #60, unmerged.

Next action: **INSTALL APK ON DEVICE AND PLAY IT.**
