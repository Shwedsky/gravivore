# GRAVIVORE asset source of truth

Status: **CURRENT / AUTHORITATIVE**

This file defines where Codex should look for art and where new downloads belong. It replaces path assumptions scattered across old art-spike and intake documents.

## 1. Production runtime assets

The authoritative asset for the game is the asset actually referenced by current `main` under:

`Assets/_Game/Content/`

When an asset is already integrated and tracked there, use that runtime asset first. Do not reacquire it from an old download cache unless the active task explicitly requires source reconstruction or license verification.

Current scene/definition/prefab references are more authoritative than historical filenames.

## 2. Project-owned editable source art

Editable project-owned source art belongs under:

`art/`

Examples include Blender sources authored specifically for GRAVIVORE and safe to keep in the public repository.

A tracked editable source should have a documented relationship to the runtime FBX/prefab/material it produces. Historical source experiments may be moved under history when superseded.

## 3. Canonical location for NEW external raw intake

All future manual or Codex-assisted external downloads must be dropped under:

`ExternalAssetIntake/Current/`

Use one subfolder per source/package. Raw archives, Asset Store packages, third-party project files and inspection scratch outputs remain git-ignored unless their license explicitly permits public source redistribution and the active task deliberately promotes them.

Do **not** place new downloads at repository root.

## 4. Historical local caches — do not use as current source paths

These locations existed during earlier acquisition experiments:

- `ExternalAssetIntake/FreeAssetIntakeV1/`
- repository-root `98_unclassified/`
- repository-root `00_reference/`
- `.local-g0-v2/`
- old Codex worktree-specific scratch paths

They are historical/reproduction locations only. New Codex passes must not scan them automatically or treat their existence as required project state.

The files under `docs/history/asset-intake/` explain those old stages.

## 5. V2 vetted asset library

`docs/free-asset-intake-v2/` is a **reference library and evidence set**, not a live payload location.

Use it for:
- provenance;
- license decisions;
- measured model/material information;
- USE / DONOR / REJECT decisions;
- previously identified gaps.

Do not use its old worktree paths as current instructions.

## 5.1 Current Chapter 01 blueprint payloads

For the owner-approved Chapter 01 Visual Blueprint V1, the current owner-local raw packages are:

`ExternalAssetIntake/Current/quaternius-modular-scifi-megakit-standard/Modular_SciFi_MegaKit_Standard.zip`

and

`ExternalAssetIntake/Current/molten-maps-scifi/Molten Maps SciFi Asset Pack.zip`

These two payload locations are the canonical current raw inputs for the next Chapter 01 visual/world rebuild.

They are intentionally separate from the old `FreeAssetIntakeV1` cache.

The raw ZIPs remain owner-local / git-ignored unless an exact license review and deliberate source-redistribution decision says otherwise. Codex may inspect/extract them in ignored scratch space for the active blueprint task.

Primary intended usage:

**Quaternius Modular Sci-Fi MegaKit [Standard]**
- `Platform_*` families;
- `Door_*` and `Door_Frame_*`;
- `ShortWall_*`, `Wall*`, `Bottom*`, `Top*`;
- `Column_*`, especially structural/piped columns;
- rails, ramps and stairs;
- cable, vent, pipe-holder, access-point and light props.

**Molten Maps SciFi Asset Pack**
- `Generator`, `Generator Pile Large/Small`;
- `Cryo Tube ON/OFF`;
- `Centrifuge`;
- `Command Console`, `Wall Command`;
- `Corridor Large/Small`;
- `Catwalk`;
- metal/mid-path/hazard floor families;
- `Wall Pipe`, wall-light, wall-door and second-floor wall families;
- batteries and selected functional machinery.

Do not interpret this as an instruction to scatter stock assets across the map. The Chapter blueprint defines composition; donor modules are ingredients. Unique hero structures should be custom-authored or materially reworked when the kit cannot satisfy the approved concept.

## 6. Public repository safety

Before committing third-party source bytes, verify the exact item license permits redistribution in a public source repository.

- CC0: generally suitable, still record provenance.
- CC BY: record attribution and verify redistribution terms.
- Unity Asset Store / custom licenses: use in the game as permitted, but do not publish restricted raw/editable source files merely because the game may ship them in compiled form.
- NC, SA when incompatible with the project, unclear terms, ripped content and recognizable franchise derivatives: do not promote to production.

If source redistribution is not permitted, keep raw source local/private and commit only project-owned integration code/data plus legally distributable game output/derived content as allowed by the license.

## 7. Intake workflow for new assets

1. Put original payload under `ExternalAssetIntake/Current/<source-id>/`.
2. Record source URL, publisher, exact license and date checked.
3. Inspect in an ignored scratch workspace; do not execute vendor scripts by default.
4. Measure meshes, materials, textures, animation/rig data and mobile risk.
5. Judge fit against `CURRENT_VISUAL_TARGET.md`, not historical low-poly/proxy criteria.
6. Promote only the minimum legally distributable production subset.
7. Update `ThirdPartyNotices.md` and any local/private acquisition receipt required for reproducibility.
8. Bind production art through the current presentation architecture without moving gameplay authority into art prefabs.

## 8. Concept references

The owner concept is design reference, not a third-party asset pack. General visual requirements are normalized into `CURRENT_VISUAL_TARGET.md`; domain image gates still apply. Text availability does not permit production against a missing primary actor image.

When the owner reference image is available to the task, treat it as primary visual evidence, subject to the explicit bipedal G-0 override and newer owner corrections.

For Chapter 01 actors, the exact owner-approved primary reference is:

`docs/visual-blueprints/chapter01-actors/CHAPTER01_ACTOR_VISUAL_TARGETS_V2.png`

Read its V2 Markdown contract and README. Preserve the existing original bytes; do not regenerate, retouch, recolor or substitute an older image. If the exact PNG is missing, stop and report missing visual authority. The owner-approved reference is tracked design evidence, not a donor download.

PR #72 / `chore/actor-asset-intake-v1` supplies actor donor audit/matrix manufacturing evidence, pinned by the V2 contract. It does not determine approved silhouette, anatomy or colors. Raw donors remain owner-local under `ExternalAssetIntake/Current/`; audit and exact license evidence must be checked before later source promotion. This authority package imports no donor models, materials or raw/editable external source.

For Chapter 01 scene composition, also read:

`docs/visual-blueprints/chapter01/CHAPTER01_VISUAL_BLUEPRINT_V1.md`

and the board image at:

`docs/visual-blueprints/chapter01/CHAPTER01_VISUAL_BLUEPRINT_V1.jpg`
