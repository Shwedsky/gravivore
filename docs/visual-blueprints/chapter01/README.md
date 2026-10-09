# Chapter 01 visual blueprint workspace

Status: **CURRENT for Chapter 01 rebuild planning**

Primary specification:

`CHAPTER01_VISUAL_BLUEPRINT_V1.md`

Expected owner-approved board:

`CHAPTER01_VISUAL_BLUEPRINT_V1.jpg`

The board is project-owned/generated visual reference and should be committed next to this README.

## Owner-local raw kit payloads

Before the next full Chapter 01 rebuild, make these local paths exist in the repository working copy:

```text
ExternalAssetIntake/Current/quaternius-modular-scifi-megakit-standard/Modular SciFi MegaKit[Standard].zip
ExternalAssetIntake/Current/molten-maps-scifi/Molten Maps SciFi Asset Pack.zip
```

The ZIP payloads remain git-ignored/local. Do not publish raw external packages merely because they are available locally.

The generated blueprint board is different: it is project-owned and should be tracked in Git at the path above.

## Implementation gate

Do not start another incremental V48-style presentation layer.

The next implementation task is a **full Chapter 01 world/presentation rebuild** only after:

1. the blueprint board is present;
2. current S04/S08 semantic-preservation rules are merged;
3. the two current raw kit payloads are available locally to the implementing Codex environment;
4. the implementation task explicitly permits rebuilding positions/collision proxies/minimap geometry while preserving gameplay/progression semantics.
