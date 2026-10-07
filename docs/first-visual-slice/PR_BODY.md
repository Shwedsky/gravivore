Chapter01 now uses the approved G-0 Production V3 as the live player, original articulated Scout/Cutter/Magnetar visuals, and a rebuilt industrial section from Cutting Floor through the western strong spot, elite gate and Magnetar arena. New deck/bulkhead/barrier/gate geometry, a hero reactor and six prop types replace the local presentation. Gameplay owns movement, collision, combat, rewards, saves and progression; animations only observe state.

The installable ARM64 DEV APK is ready for the owner's device review. It was built from `0df48be51af221ca69875b8917039fd863a97cc0` with Unity 6000.3.0f1: `com.gravivore.mobile.dev`, version 0.1.0 / versionCode 36, 64,880,064 bytes (61.87 MiB).

SHA256: `fb9fd7a17cca69ffc8a9f5ae116cd5c456204de2ff66ebbe5674b92344ece67f`.

Validation:
- Compilation and project validation passed.
- EditMode: 405 passed, 0 failed.
- PlayMode: 97 passed, 0 failed; one optional structural capture skipped.
- Android build: success, exit 0; signature, manifest version and ARM64-only IL2CPP verified.
- Production dependencies and exact serialized actor/environment entries inside the APK verified; evidence is bound to its SHA256.

Delivery path, changed files, original Blender sources, build/test logs, recovered build failures and limitations are recorded in `docs/first-visual-slice/DELIVERY.md`. Phone performance and visual acceptance await device testing; the remaining chapter art is unchanged. This PR remains a draft.

Next action: **INSTALL APK ON DEVICE AND PLAY IT.**
