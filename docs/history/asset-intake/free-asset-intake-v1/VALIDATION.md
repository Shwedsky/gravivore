# Free Asset Intake V1 completion record

Validated on 2026-10-08 in the isolated `chore/free-asset-intake-v1` worktree.
Draft PR: https://github.com/Shwedsky/gravivore/pull/66 (base `main`).
Validated tooling commit: `4909c9b35b68849b4f650c9a221d90a13a9878c4`;
the following delivery commit changes this record only.

## Changed files and purpose

- `.gitignore`: deny all intake files, then allow only the exact 82 marker paths;
  explicitly ignore `_local_reports/`.
- `ExternalAssetIntake/FreeAssetIntakeV1/`: 41 leaves with `.gitkeep` and
  `DROP_HERE.md`, including 39 mapped sources and two fallback folders.
- `docs/free-asset-intake-v1/SOURCE_MANIFEST.json`, `.md`, `.schema.json`:
  folder mapping, expected provenance/license classes and metadata schema.
- `docs/free-asset-intake-v1/README.md`, `OWNER_DOWNLOAD_WORKFLOW.md`,
  `VALIDATION.md`: baseline, Windows owner instructions and this completion record.
- `Tools/free-asset-intake/scan_intake.py`, `scan-intake.ps1`, `verify-drop.ps1`,
  `test_scan_intake.py`: deterministic read-only hashing, status reporting and tests.

The scanner only writes its ignored local JSON report. It does not extract,
inspect package contents, render, convert, import or mutate downloads/the manifest.

## Executed validation

- Baseline ancestry: PASS. Fetched `origin/main`
  `66aed96bb23c4f02180255d6111861c1cb05bb8e` is the PR #65 merge and contains
  accepted head `d00234d622ef54e6a4a072db2f530a869fbfffb3`.
  Chapter 01 V3, M-0 equipment, rendering hotfix and UI shader preservation
  are present; `docs/CHAPTER01_V43_RENDER_HOTFIX.md` was read from main.
- Real checkpoint commit `409415a2160c5fa71ec7d6d629b2bffcb084c475`
  was pushed before Draft PR #66 was opened. Folder-map checkpoint `6534209`
  and tested-tooling checkpoint `4909c9b` were separately committed and pushed.
- Python compilation of both `.py` files: PASS, using Python `compile()` without
  creating bytecode artifacts.
- Behavioral suite: **20/20 PASS, no skips**, including chunked SHA256/byte counts,
  original file preservation, required extensions, arbitrary concept filenames,
  nested paths, deterministic report output, report exclusion, missing/empty/multiple
  statuses, structural failures, changing/unreadable files, reparse detection,
  hardlinked report rejection, and both Windows wrappers' 0/2 exit-code behavior.
- Manifest JSON parsing, runtime field/type/path/license validation and schema
  required-field correspondence: PASS. The optional external `jsonschema`
  package is not installed; no dependency was added and no full external JSON
  Schema validation is claimed.
- PowerShell parser checks for both wrappers: PASS.
- Direct scanner and both Windows commands on the real empty intake: exit **0**.
  Summary: EXPECTED SOURCES **39**, PRESENT **0**, MISSING **39**,
  UNEXPECTED FILES **0**, TOTAL BYTES **0**. Every existing mapped leaf is EMPTY.
- Git policy: PASS. All 41 leaves/82 marker files exist and are trackable.
  A real synthetic `.zip` was created in a DROP folder, verified ignored by
  `git check-ignore --no-index`, and removed in `finally`. All recognized payload
  extensions, unknown extensions, nested documentation names and reports are ignored.
  No fake probe remains.
- Tracked intake audit: exactly **82** files, all `.gitkeep` or `DROP_HERE.md`.
  No third-party payload or local report is tracked.
- Diff scope and `git diff --check origin/main...HEAD`: PASS. Only `.gitignore`,
  intake markers, `Tools/free-asset-intake/` and `docs/free-asset-intake-v1/` changed.
  No `Assets/`, scenes, runtime/gameplay/art/VFX/UI, `Packages/` or
  `ProjectSettings/` file changed. Worktree was clean after the tooling checkpoint.
- GitHub PR metadata confirms an open Draft PR to main and the remotely persisted
  validated tooling head. The final documentation checkpoint is pushed as well.

## Scope limits and assumptions

Unity CLI is installed, but Unity compilation, ProjectValidator, EditMode/PlayMode
and Android APK build were **not run**. The current explicit V1 task permits only
intake infrastructure and prohibits Unity import and PlayerSettings/GraphicsSettings
changes; launching a fresh Unity checkout/build would import existing project
assets and may generate/modify those settings. No new APK was produced or claimed.

PRESENT means a non-empty recognized raw payload, not archive validity, quality,
license approval or successful integration. TXT/PDF sidecars alone do not complete
a source. MULTIPLE_PAYLOADS counts raw recognized files and can be normal for a
model with textures; package relationships are deferred to V2. Reports exclude
their own directory and the exact committed markers.

Source license labels are expectations from public listings, not inspected license
inventory. All entries remain AWAITING_OWNER_DOWNLOAD / NOT_INSPECTED. The concept
board and owner-specific Meshy provenance remain unconfirmed. The armored pipes
license remains unknown. Exact free RTS source was not supplied; its similarly
named Unity v3 listing is paid and was not substituted or purchased. The generic
black-hole name has an explicitly provisional matching free listing.
No UNKNOWN_REQUIRES_REVIEW source may later enter production.

G-0 remains the approved **BIPEDAL** mech. The concept board's player silhouette
does not override it. All asset inspection, normalization, license inventory,
asset zoo and USE / DONOR / REJECT decisions belong to
**FREE ASSET INTAKE V2**, after owner download.
