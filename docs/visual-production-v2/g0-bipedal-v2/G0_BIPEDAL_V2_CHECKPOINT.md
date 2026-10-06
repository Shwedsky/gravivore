# G-0 bipedal blockout V2 checkpoint

Date: 2026-10-06 (Europe/Moscow)

## Authorized direction

Replace the rejected spider art direction with a narrow, athletic, visibly mechanical bipedal G-0. The Catfish donor is a production reference/base only. Create custom torso, recessed gravity core, sensor, shoulders, arms, weapons, armor and rear silhouette. Stop at human art-direction review; no Chapter01 integration, production prefab replacement or merge.

## Git baseline

- Repository: Shwedsky/gravivore.
- Branch: art/g0-bipedal-blockout-v2.
- Freshly fetched and remotely verified origin/main: 40b682d3570d08c427999d42514501bb6c1c2dfd.
- Isolated checkout: C:/Users/pamak/Documents/ChatGPT/gravivore/.codex-worktrees/g0-bipedal-blockout-v2.
- Existing checkout changes and previous spider work are preserved.

## Initial verified facts

- Archive found: C:/Users/pamak/Documents/GRAVIVORE_ASSET_INTAKE/g0/catfish-mech-low-poly-animated.zip (175998644 bytes).
- Required tool available: Blender 5.2.2 LTS, build d13f752e3b9c.
- Import, geometry, license, rig and animation inspection are pending; none are assumed passed.

## Checkpoint sequence

1. Commit/push this actual initial checkpoint and open a new Draft PR.
2. Inspect archive and donor in Blender; record provenance and design decision before modeling. Commit/push.
3. Create editable G0_Bipedal_Blockout_V2.blend and seven actual Blender render PNGs. Inspect all views, validate geometry/file reopening, document findings. Commit/push.
4. Leave the PR draft and report the human review gate.

## Scope of validation

This is an art-only blockout gate. Blender import, mesh metrics, render evidence and file reopening are required. No runtime spec is being completed. Unity compile/tests/project validation/APK status is recorded in the final review.

## Completed checkpoints

- Initial checkpoint: fa72478bc7c5794f694b43029d450b942f299a4e, pushed; new [Draft PR #57](https://github.com/Shwedsky/gravivore/pull/57) opened immediately.
- Donor inspection and pre-modeling design decision: 60c7843, committed and pushed.
- Editable modeling and rigid skeleton checkpoint: ef90e1e, committed and pushed.
- Final seven-view evidence/review and verification: complete; final commit/push follows this update.
- Final model: 134 editable mesh objects, 24705 evaluated triangles, 16-bone original mechanical rig; 17 retained donor mechanisms / 1499 triangles.
- Visual verdict B, human approval pending. Scope ends here; no runtime integration, old-work overwrite or merge.
- Unity compile/project validation passed; EditMode 386 passed; PlayMode 92 passed, 1 optional capture skipped. ARM64 dev APK 0.1.0+2 built and manifest-verified. Generated runtime serialization changes were restored; runtime source matches the main baseline.
