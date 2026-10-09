# Frozen product decisions — Vertical Slice v0.1

Status: **CURRENT**  
Original freeze: 2026-09-24. Later explicit owner decisions and `docs/CURRENT_AUTHORITIES.md` supersede conflicting historical detail.

## Product

Working codename: **GRAVIVORE**. Public brand name is not legally cleared and may change before store release.

Player fantasy: an escaped techno-organism powered by an unstable gravity core. It assimilates compatible cores/modules and visibly changes as it grows.

The game is inspired by genre/formula, not by protected IP/assets/code from other games.

## Platform and engine

- Unity 6.3 LTS.
- Universal Render Pipeline.
- Android portrait-only.
- ARM64 required.
- Minimum Android API 26 for v0.1.
- Touch first; mouse simulation in Editor.

## Session and account

- No login at launch.
- Local anonymous profile/save.
- No backend in v0.1.
- Future account/cloud recovery must be possible without rewriting gameplay.

## Controls and combat

- One-thumb movement.
- No auto-run.
- No manual basic-attack button.
- Auto-target/auto-attack when a hostile target is valid/in range.
- UI touches do not move the player.
- Standard enemies can be displaced by gravity attack; elites reduced; bosses immune.
- Player chooses engagements by positioning.
- Design objective: about 80% progression/stat check, 20% movement skill.

## Vertical-slice content

- One Chapter 01 zone.
- Five ordinary enemy spots, each configurable for 3–5 live enemies.
- Five ordinary archetypes.
- One elite.
- One boss.
- Five core progression stats.
- At least two clearly visible evolution milestones.
- First-play target remains roughly 30–45 minutes while tuning continues.
- Death does not remove permanent stats.

## Progression

- More chapters later.
- Effectively unbounded stat growth.
- Prestige/rebirth later, not v0.1.
- Offline reward secondary/capped.
- Full auto-battle out of scope.

## Monetization

Not implemented in v0.1. Future providers stay behind platform abstractions.

## Art and visual direction

Initial asset spend remains **zero until the owner explicitly changes it**.

Current art authority is:
- `docs/CURRENT_VISUAL_TARGET.md`
- `docs/ART_DIRECTION.md`
- `docs/ART_ASSET_POLICY.md`

The current target is **mobile-optimized premium hard-surface industrial sci-fi**. `Low-poly` is not the target aesthetic; mesh/material/light reduction is an optimization technique only.

Current G-0 is **bipedal**. Historical radial/four-support G-0 directions are superseded.

Allowed art:
- original project-created art;
- Unity built-ins where not relied on as visible final art;
- verified free commercial assets, CC0 preferred, CC BY acceptable with correct handling;
- other free sources only after item-level license/public-repository review.

Every promoted external asset requires provenance/notice. New raw acquisition paths are defined only by `docs/ASSET_SOURCE_OF_TRUTH.md`.

## Repository and delivery

- GitHub.
- Codex-oriented current-authority documentation.
- `docs/history/` is non-authoritative development history.
- Windows `build-android.ps1`.
- Dev APK may use debug signing.
- Release signing deferred.
