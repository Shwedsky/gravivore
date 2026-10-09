# Owner download workflow (Windows)

The prepared checkout is the isolated `chore/free-asset-intake-v1` worktree.
Open PowerShell here, not in the older parent checkout:

```powershell
Set-Location 'C:\Users\pamak\Documents\ChatGPT\gravivore\.codex-worktrees\free-asset-intake-v1'
```

1. Find the source in `docs/free-asset-intake-v1/SOURCE_MANIFEST.md`, follow its
   publisher link and download the **free** asset. If a link is unavailable,
   paid, or unresolved, report it as missing; do not purchase or silently substitute.
2. Leave the original file/archive unchanged, including its filename and internal files.
3. Copy it into that source's exact `ExternalAssetIntake/FreeAssetIntakeV1/.../`
   DROP folder. Preserve any separately supplied license/provenance TXT/PDF beside it.
4. Do not import into GRAVIVORE or place downloads under `Assets/`.
5. Do not unpack manually, optimize, convert, render or rename internal files.
6. Run:

   ```powershell
   powershell -ExecutionPolicy Bypass -File Tools/free-asset-intake/verify-drop.ps1
   ```

7. Report missing/unavailable assets and the observed status list to ChatGPT
   before any integration starts. Request **FREE ASSET INTAKE V2** only after downloading.

Place the authoritative concept board (any PNG/JPG/JPEG filename) in
`00_reference/01_gravivore_concept_board/`. G-0 remains the approved **BIPEDAL**
mech; the board's player silhouette does not override it.

Put unmapped originals in `98_unclassified/`; put failed originals or replacement
candidates in `99_failed_or_replacements/`. Those folders do not satisfy a named
source automatically. Keep `.gitkeep` and `DROP_HERE.md` intact in every DROP folder.

## Read the output

- `PRESENT`: exactly one non-empty recognized payload file; no inspection or approval implied.
- `MULTIPLE_PAYLOADS`: more than one recognized payload file; may simply be a model
  with textures or multiple original packages. V2 will inspect them.
- `EMPTY`: folder exists but has no qualifying payload (markers/TXT/PDF alone do not count).
- `MISSING`: expected folder does not exist.

The summary's `MISSING` includes both MISSING and EMPTY sources. PRESENT includes
MULTIPLE_PAYLOADS sources. Unknown extensions, zero-byte files and unmapped files
are listed as unexpected in the JSON report. These observations never fail an
incomplete drop. Exit code 2 means a structural/tooling problem (unreadable or
changing file, unsafe link/junction, invalid manifest, invalid report destination,
missing intake root or Python).

The scanner reads original files in chunks, computes SHA256 and writes only:

`ExternalAssetIntake/FreeAssetIntakeV1/_local_reports/intake_inventory.json`

The report and all raw payloads are ignored by Git. Do not force-add them.
The scanner skips its own report directory and the exact 82 committed marker files.
It does not unpack archives or inspect package contents. A glTF's external files,
license texts and font terms remain unverified until V2.

## Scanner only / Python

```powershell
powershell -ExecutionPolicy Bypass -File Tools/free-asset-intake/scan-intake.ps1
```

Python 3.9+ is required; no packages are required or installed. The wrapper checks
an explicit `-PythonPath`, then `GRAVIVORE_INTAKE_PYTHON`, PATH Python/py, and the
existing bundled Codex Python. If needed, pass the full path to an existing Python:

```powershell
powershell -ExecutionPolicy Bypass -File Tools/free-asset-intake/verify-drop.ps1 -PythonPath 'C:\path\to\python.exe'
```

Absolute paths and defaults are anchored to the script checkout, so invoking the
script by full path also works from another directory. Scans can take time for
large files. Finish copying downloads before running them.
