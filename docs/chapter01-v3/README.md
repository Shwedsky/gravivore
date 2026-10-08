# Chapter 01 V3 verification

`verification/EditMode.xml` and `verification/PlayMode.xml` contain the complete Unity Test Runner results for this milestone. `internal/` contains gameplay and structural captures from the canonical scene, including incoming damage, the ranked weapon UI, the bipedal weapon attachment, the live Custodian V3, and traversal at six perimeter locations.

Reproduce from the integration worktree with Unity 6000.3.0f1:

```powershell
.\Tools\chapter01-v3\run_unity.ps1 -Action Compile
.\Tools\chapter01-v3\run_unity.ps1 -Action EditMode
$env:GRAVIVORE_VISUAL_INTEGRATION_QA = Join-Path (Get-Location) 'docs/chapter01-v3/internal/structure'
.\Tools\chapter01-v3\run_unity.ps1 -Action PlayMode
.\Tools\chapter01-v3\run_unity.ps1 -Action Validate
.\Tools\chapter01-v3\run_unity.ps1 -Action Build
.\Tools\chapter01-v3\verify_apk.ps1 -ExpectedSourceSha (git rev-parse HEAD)
```

The optional environment variable enables the existing structural capture test; it does not enable a runtime cheat or change the shipped APK. The five-minute route test explicitly uses development gate unlock and god mode to exercise actual locomotion, combat, waves and saves across the chapter. Editor throughput and stall measurements are diagnostics, not Android FPS claims.

The APK verifier checks manifest version and package, ARM64 IL2CPP libraries, development flags, signing compatibility with accepted v41, build commit metadata, four asset-packing proofs, retained production settings and the serialized M-0 weapon. `apk_verification.json` is written only after all checks pass.

See `docs/CHAPTER01_V3_PLAYTEST.md` for the behavior changes, assumptions, delivery metadata and the remaining owner device-playtest gate. `changed_files.txt` lists milestone source and evidence changes against the accepted main baseline.
