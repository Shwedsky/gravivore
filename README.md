# GRAVIVORE — Android vertical slice

Status: **active development**  
Engine: **Unity 6.3 LTS + URP**  
Target: **portrait Android**, single-player, local anonymous profile  
Primary implementation agent: **Codex**

## Product thesis

GRAVIVORE is an original one-thumb mobile action/idle RPG about a growing machine-organism powered by a gravity core.

Core loop:

`explore -> choose prey -> auto-engage in range -> destroy -> assimilate -> permanently grow -> unlock harder prey -> defeat elite/boss -> continue`

The game may be inspired by genre formulas, but must not copy protected assets, names, code or distinctive content from other games.

## Current implementation-agent read order

Every substantial Codex task starts by refreshing the branch and reading:

1. `AGENTS.md`
2. `docs/CURRENT_AUTHORITIES.md`
3. `docs/CURRENT_VISUAL_TARGET.md` for any art/presentation/world/UI/VFX task
4. `docs/ASSET_SOURCE_OF_TRUTH.md` for any asset task
5. `docs/DECISIONS.md`
6. `docs/PROJECT_BIBLE.md`
7. `docs/GAME_DESIGN.md`
8. `docs/ARCHITECTURE.md`
9. the active spec/task
10. `docs/TEST_STRATEGY.md`
11. `docs/BUILD_AND_RELEASE.md`

### Historical docs

`docs/history/` is development history only. It preserves old phases, visual experiments, previous budgets and acquisition workflows so changes in thinking remain traceable.

**It is not a source of current requirements.** A Codex task must not restore a historical art direction, source path, performance ceiling or acceptance criterion unless the current human task explicitly asks to reproduce/compare history.

## Current visual direction

GRAVIVORE targets **mobile-optimized premium hard-surface industrial sci-fi**.

Low polygon counts, LOD, material atlases and limited realtime lighting are optimization techniques, not the target aesthetic. Current visual authority is `docs/CURRENT_VISUAL_TARGET.md`.

G-0 is a **bipedal combat mech**. Historical radial/four-support G-0 concepts are superseded.

## Current asset locations

- Runtime production content: `Assets/_Game/Content/`
- Project-owned editable art sources: `art/`
- New external raw intake: `ExternalAssetIntake/Current/`
- Completed V2 asset research: `docs/free-asset-intake-v2/` — evidence/reference only, not a live source path

See `docs/ASSET_SOURCE_OF_TRUTH.md` before acquiring or promoting any asset.

## Locked product decisions

- Android first, portrait only.
- No registration/backend required for v0.1.
- Local save, future cloud boundary preserved.
- One-thumb movement, no auto-run, auto-attack in range.
- Combat target roughly 80% progression/stat check and 20% movement skill.
- One Chapter 01 vertical slice with five ordinary enemy spots, one elite and one boss.
- Player evolution is visibly represented on the model.
- Initial asset spend remains zero until the owner explicitly changes it.
- Monetization/backend are deferred.

## Build

Development Android build from repository root:

```powershell
.\build-android.ps1
```

Optional explicit Unity path:

```powershell
.\build-android.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe" -Clean -Version 0.1.0
```

Output is written under `Builds/Android/`. Do not claim a test/build/device result unless it was actually executed.

## Vertical-slice success criterion

A fresh tester can install the APK, understand movement without explanation, fight/farm five distinct enemy roles, feel permanent growth, visibly evolve G-0, unlock/defeat elite and boss, retain progress after restart, and complete a compelling first-play loop.

Functional completion is necessary but not sufficient for visual acceptance: production presentation is judged against the current concept and ultimately on a real device.
