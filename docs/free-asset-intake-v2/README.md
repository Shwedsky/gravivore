# Free Asset Intake V2 — review ready

Selection and evidence only, based on merged V1 `a6541ef0c78f1489354bb4a5777de5e5b8847ce5`, in the **existing** `.codex-worktrees/free-asset-intake-v1` worktree on `art/free-asset-intake-v2`. Draft PR: https://github.com/Shwedsky/gravivore/pull/67.

Found **26 actual art payloads + one owner concept image**, totaling **5,000,627,772 bytes**. The downloads actually sit in this worktree's root `98_unclassified/` and `00_reference/`; the planned `ExternalAssetIntake/FreeAssetIntakeV1/` also remains intact. Names/folders were resolved without owner sorting. Inputs stay ignored and immutable; never remove this worktree, use git clean or overwrite the first snapshot.

Expected states: **27 FOUND, one MISSING (`pipes-armored`), 11 SKIPPED_SUBSCRIPTION (Meshy)**. Art-source selection: **USE4 / DONOR14 / REJECT6 / UNRESOLVED2**. Reference, missing/skipped sources and embedded alternate formats are excluded from those26 decisions. TechLab's exact license and Tiago/Icons8 mixed rights remain unresolved; the stack works without them.

Unity6000.3.0f1 / URP17.3.0 measured **1171 canonical FBX models** and captured **120 representative models** in an art-only scratch project. The inventory has2727 rows including static prefab/graph audits and UI dimensions. Eight annotated sheet pages are lightweight review evidence. VFX animation, prefab runtime behavior and Android performance are not claimed. G-0 keeps the approved **bipedal** identity.

## Review order

1. [Recommended stack](RECOMMENDED_ASSET_STACK.md) and [replacement matrix](CHAPTER01_REPLACEMENT_MATRIX.md): one stack and all24 Chapter01 roles.
2. [Selection scores](ASSET_SELECTION_MATRIX.md) and [art gaps](UNFILLED_ART_GAPS.md): explicit source/item decisions and custom work.
3. [Zoo report](ASSET_ZOO_REPORT.md), [inventory](TECHNICAL_ASSET_INVENTORY.csv) and [evidence manifest](EVIDENCE_MANIFEST.json): measured facts and preview limits.
4. [Discovery](SOURCE_DISCOVERY_REPORT.md), [resolution](SOURCE_RESOLUTION.json), [provenance](ASSET_PROVENANCE_RESOLVED.csv), [attribution](ATTRIBUTION_DRAFT.md) and [validation](VALIDATION_REPORT.md).

## Reproduce locally

Use Python with Pillow and Unity6000.3.0f1. Bundled Python here: `C:\Users\pamak\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe`. Run from this existing worktree, which has the owner downloads. All generated work stays in ignored `ExternalAssetIntake/FreeAssetIntakeV1/_work_v2/`.

```powershell
python -X utf8 Tools/free-asset-intake/intake_v2.py discover
python -X utf8 Tools/free-asset-intake/intake_v2.py extract
python -X utf8 Tools/free-asset-intake/intake_v2.py inspect
python -X utf8 Tools/free-asset-intake/resolve_sources_v2.py
python -X utf8 Tools/free-asset-intake/prepare_zoo.py
.\Tools\free-asset-intake\run_zoo.ps1 -Action Inspect
python -X utf8 Tools/free-asset-intake/build_review_v2.py
python -X utf8 Tools/free-asset-intake/write_selection_v2.py
python -X utf8 -m unittest discover -s Tools/free-asset-intake -p 'test*.py' -v
python -X utf8 Tools/free-asset-intake/validate_review_v2.py
```

Finish each step before the next. Discovery preserves the first immutable snapshot; extraction receipts reuse matching input hashes. `prepare_zoo.py --samples-only` updates sample flags without recopying art. Inspect compiles owned editor code before execution; `run_zoo.ps1 -Action Compile` offers a separate scratch compile check. Unity needs normal permitted local process access for licensing; a restricted named-pipe environment cannot complete it. No Hub/license files need changing.

Scratch scene: `_work_v2/unity_zoo/Assets/Zoo/AssetZoo.unity`. Full local reports/normalization audit/Unity log and individual renders stay under `_work_v2/reports` and `_work_v2/renders`. Tracked `Tools/free-asset-intake/urp-baseline/` contains owned generated render configuration only, copied into scratch; its Assets/ProjectSettings names do **not** modify production paths. Raw meshes/textures/UI sources, third-party code, archives and packages are never committed.

No production scene/prefab/gameplay/ProjectSettings integration, APK or device gate. Next spec: **FREE ASSET INTAKE V3 — production integration plan**, selected after ChatGPT reviews this PR.
