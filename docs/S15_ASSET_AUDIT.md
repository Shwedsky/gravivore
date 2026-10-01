# S15 Free Asset Integration Audit

## Source and license

- Pack: Kenney Factory Kit 3.0
- Author: Kenney
- Official source: https://kenney.nl/assets/factory-kit
- License: Creative Commons CC0 1.0 Universal
- Verification date: 2026-09-30
- Bundled license: `Assets/ThirdParty/KenneyFactoryKit/License.txt`
- Attribution: not required

Only 14 used FBX files are committed. The package archive, source files, previews, example content, and unused models are intentionally excluded.

## Imported subset

Player and enemy kitbash: `machine-connection-hole`, `cog-a`, `cog-b`, `cone`, `hopper-round`, `piston-round`, `robot-arm-a`, `box-large`, and `structure-wall`.

Environment landmarks: `structure-wall`, `structure-yellow-medium`, `door-wide-closed`, `machine-fortified`, `pipe-large-long`, `crane-magnet`, and `box-large`.

Some models serve both categories. All runtime combinations are authored in `S15_VisualCatalog.asset`; production code does not load assets by path.

## Presentation mapping

- Player: asymmetrical machine core with exposed cog and paired pistons; intentionally non-humanoid.
- Scout Drone: light round hopper with a narrow sensor cone.
- Cutter Unit: compact core with a prominent industrial arm.
- Warden: tall armored block silhouette.
- Arc Drone: broad concentric cog silhouette with energy accent.
- Carrier: wide hopper body with upper cargo block and rear piston.
- Five Chapter 01 zones: distinct machine, crane, wall, pipe, and scrap-door landmarks.

Imported visuals are presentation only. Existing CharacterController, sensing collider, ground, boundary, and gate geometry remains authoritative.

## Import and mobile budget

- Animation, blend shapes, cameras, lights, visibility tracks, and embedded materials are disabled.
- Mesh read/write is disabled.
- Mesh compression is Medium and mesh optimization is enabled.
- Tangents are omitted because the authored materials do not use normal maps.
- Three shared project-owned URP Lit materials replace embedded materials.
- No runtime `Shader.Find`, `Resources.Load`, texture duplication, or per-frame material allocation is introduced.
- Pooled enemies lazily cache each encountered recipe and only toggle cached roots on reuse.

## Animation audit

The selected Factory Kit subset is static and contains no character rigs or clips. No Mecanim controller, humanoid retargeting, root motion, or animation-event dependency is introduced. Movement and combat remain driven by existing gameplay transforms and S14 feedback. More elaborate animation is intentionally deferred rather than introducing a new animation architecture in S15.

## Review notes

- Imported model orientation, scale, silhouette readability, material appearance, and landmark framing require one combined S13+S14+S15 device review.
- Verify evolution attachments remain readable around the new player body.
- Verify S14 hit, death, lash, telegraph, audio, and haptic presentation remains unobstructed.
- Verify all pooled enemy visuals reset cleanly after death and respawn.
