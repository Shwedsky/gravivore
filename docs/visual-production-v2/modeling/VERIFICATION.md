# Executed verification — Visual Production V2 modeling

All outcomes below were executed, not inferred from older PRs.

- Blender 5.2.2 LTS: actual source imports, neutral renders, authored-source save/reopen and mesh validation completed. 87 closed/manifold model meshes; no zero-area faces or loose vertices; four toe contacts, core/attack sockets and clean root verified. Static pose only.
- Python compile: `python -m compileall -q Tools/art` passed.
- Artifact delivery: five tests passed (map hashes/channels/normals; actual intake/subset coverage; actual source animation motion; saved blend/evidence consistency; Git scope/archive exclusion).
- Unity 6000.3.0f1 compile: passed as part of the executed Android build and test runs, using the repository's standard ProjectConfigurator.
- EditMode: 386/386 passed, zero failures, zero skipped. Full executed XML retained. Log copies retain content with trailing whitespace normalized; originals remain in ignored Builds/.
- PlayMode final: 92 passed, zero failed, one ignored out of 93. Ignored capture-only test: `Gravivore.Tests.PlayMode.VisualIntegrationSmokeTests.CaptureStructuralFoundationWhenRequested`; reason: Set GRAVIVORE_VISUAL_INTEGRATION_QA to export structural review captures. No additional old-world capture was requested.
- Project validator: `Gravivore.Editor.ProjectValidator.ValidateOrThrow` executed and exited zero. Full validator log retained.
- Dev Android build: `./build-android.ps1 -Flavor Dev -Version 0.1.0 -VersionCode 2` passed, Unity exit zero. aapt verified package `com.gravivore.mobile.dev`, versionCode 2, development/debug settings and ARM64-only architecture.
- APK: `Builds/Android/gravivore-dev-0.1.0+2.apk`; 59,176,025 bytes; SHA256 `544e5d66eab51b63f3ea49b3259a5f496456cf394bff0caa14ba952532f9334a`. Local APK is git ignored; metadata is retained here. Build metadata records runtime-source checkpoint `72547ed4ca691a5e540767415df7eab1c53e341d` because no new art is integrated.

## Failed attempts and recovery

An initial Unity validator attempt failed to initialize licensing (IPC/signature validation); it did not establish a compilation result. Licensing recovered during the subsequent real Android build and final check runs.

The first PlayMode run with `-nographics` crashed in Unity's graphics draw path with exit -1073741819 and produced no result XML. Its crash log is retained as `verification/PlayMode.log`. Retrying only PlayMode with `-force-d3d11 -force-gfx-direct` in a hidden batch window completed with the final results above. The ignored optional capture test is not represented as a pass.

## Scope and cleanup

These Unity checks exercise the existing runtime. They do **not** prove G-0 locomotion, shader conversion, device performance or new art integration. The model, prepared maps and donor selections remain outside Unity. No Chapter01 prefab replacement is delivered.

CLI configuration/tests generated Unity settings, import metadata, temporary test scenes, materials and diagnostic files in this isolated worktree. Tracked changes were restored to HEAD; generated files were preserved in ignored `.asset-intake-tmp/`. The user's original checkout was not modified. The review commit excludes all Assets/Packages/ProjectSettings changes, raw source archives and extracted donor meshes.

Full changed-file list: `FILES_CHANGED.txt`. Primary outcome: G-0 Blockout V1 ready for art-direction review, grade B. Next gate: human G-0 review; no next implementation spec started.
