# Free Asset Intake V1

Local download intake infrastructure only. Do not extract, modify, import,
render, or select third-party assets during V1. G-0 remains the approved bipedal
mech; the owner's concept board does not override that silhouette.

## Baseline checkpoint

- Repository: `Shwedsky/gravivore`.
- Fetched main: `66aed96bb23c4f02180255d6111861c1cb05bb8e` (merge of PR #65).
- Accepted PR #65 head: `d00234d622ef54e6a4a072db2f530a869fbfffb3`.
- `git merge-base --is-ancestor` passed for the accepted head and fetched main.
- Main includes Chapter 01 V3, M-0 weapon/equipment, the rendering hotfix,
  `UI/Default` and `UI/DefaultETC1` preservation, and
  `docs/CHAPTER01_V43_RENDER_HOTFIX.md`.

## Prepared workspace

- [Owner download workflow](OWNER_DOWNLOAD_WORKFLOW.md): Windows commands and exact checkout.
- [Source manifest](SOURCE_MANIFEST.md) and [JSON manifest](SOURCE_MANIFEST.json):
  39 expected sources and two fallback folders.
- [Manifest schema](SOURCE_MANIFEST.schema.json): V1 metadata and license classes.
- 41 leaf folders each contain a committed `.gitkeep` and `DROP_HERE.md`.
- Exact Git allowlist permits those 82 marker files; all other intake files and
  local reports are ignored, regardless of extension.
- `Tools/free-asset-intake/scan_intake.py` and Windows wrappers perform read-only
  inventory and report observed presence without changing the manifest.

Run behavioral tests with an existing Python 3.9+ executable:

```powershell
python -B -m unittest discover -s Tools/free-asset-intake -p 'test_*.py' -v
```

No Python packages are required. Tests use temporary synthetic bytes only and
remove the real Git-ignore ZIP probe in a `finally` block.

Next task after owner download: **FREE ASSET INTAKE V2**.
