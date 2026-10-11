# Capacitor Field proof — reproduction

Worktree must start at the pinned APK 50 integration (`9e0713c`), not main/design. No merge is part of this task.

1. Run `prepare.py` with the bundled Python/Pillow/NumPy runtime. It reads the two owner's canonical CURRENT ZIPs from the repository's delivery root, extracts only service donors into this worktree's ignored CURRENT intake, and writes original repeating PBR surface maps.
2. Run Blender 5.2 in background with `--python Tools/capacitor-field-proof/author.py`. It writes the independently editable `art/capacitor-field-proof/CapacitorField.blend` and the runtime `CapacitorField.fbx`. No R2 world generation, nonuniform hero fit or vendor script execution.
3. `run_unity.ps1 -Action Author` imports materials, rebuilds this sector's production prefab, substitutes that one sector in Chapter01, updates four explicit blocker volumes, fixes post-process resources and saves the fixed production camera/Linear render contract.
4. `run_unity.ps1 -Action Compile`, `-Action EditMode`, `-Action PlayMode -VideoCapture`, `-Action Validate`. Play tests run with temporary profiles, never the owner's profile. Video recording is opt-in and uses god mode solely to retain an uncut traversal; normal APK startup has no such override.
5. `build-android.ps1 -Flavor Dev -Version 0.1.0 -VersionCode <next-free-code>`. Preserve existing APKs and metadata. Do not use `-Clean`.
6. `comparison.py` assembles the unchanged owner crop, archived APK 50 playable-scene screenshot and new actual camera images. It explicitly labels the old camera. Each new UI/no-UI pair uses one camera transform at one game tick.
7. `encode_video.py` uses Blender's bundled FFmpeg encoder on the continuous gameplay frame sequence; no installed package is needed. Encoding uses Standard/sRGB, 540×960, H.264 MP4, 10 samples per second, no synthesized audio.
8. `evidence.py --strict` compares protected APK 50 runtime/content files (normalizing text line endings) and the preserved owner's original APKs/metadata/canonical ZIPs. Its owner manifest is captured before new APK delivery; do not overwrite it.

The report belongs under `docs/history/implementation-passes/capacitor-field-proof/`. This prototype reference sector is assessed on an actual Android device before any Chapter-wide reproduction. Automated green tests, source mesh counts and Editor measurements do not grant the owner's visual acceptance.
