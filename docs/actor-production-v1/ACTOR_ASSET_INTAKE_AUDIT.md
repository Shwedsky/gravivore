# Chapter 01 actor asset intake audit

Audit checkpoint: 2026-10-10. Model assessment and final donor mapping still in progress.

Complete warehouse enumeration found 24 payloads (9 ZIP archives and 15 Unity packages). Nineteen useful payloads were copied without moving originals into the owner-local canonical Current tree; every source/destination SHA-256 matched. Five packages were rejected from canonical promotion. Four pre-existing UnityFan vehicles and both active world archives were inspected without changing their payloads.

`ARCHIVE_MANIFEST.json` covers every member path and format count. `NESTED_ARCHIVES.json` covers two embedded Unity packages and five PSD-source RAR archives. `COPY_MANIFEST.json` gives exact local destinations; `SOURCE_LICENSES.json` records current official source verification and item-level restrictions.

No runtime/scene/production-prefab changes and no final modeling. Raw assets, scratch, and renders remain local and ignored. Draft PR #72 remains independent of PR #71.
