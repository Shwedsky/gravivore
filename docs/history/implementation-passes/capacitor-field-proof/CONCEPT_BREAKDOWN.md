# Capacitor Field — playable concept environment proof

Explicit owner task, 2026-10-12. This is pass evidence, not a new authority for the whole Chapter.

Base: `integration/chapter01-r2-actors-v2`, pinned `9e0713ca08deebf43890cf23d20e954d23b7265c`. APK 50 build source: `57dfde50485a0db3d966e980bd94dddd1ea48488`. Implementation is isolated on `codex/capacitor-field-playable-proof`; no merge.

## Concept interpretation and construction

1. **Large forms:** three pressure/induction columns, a higher central chamber and two lower wings, backed by a load-bearing bus carrier. New project-owned models: lathed nested pressure pedestals/crowns, curved shell sectors with real thickness, preloaded tie rods, insulating sleeves and service openings. A stock Cryo Tube cannot supply this construction and is not used as the hero.
2. **Energy relationship:** the central vessel feeds two side accumulators through insulated phase conductors. Lower grounded returns converge into a collector; four transformers connect into protected underdeck cable trays. These routes terminate visibly at bases, contacts and bushings rather than as floating glowing bars.
3. **Secondary function:** opaque transformer enclosures, exposed cooling fin stacks, ceramic bushings, a switch/contact cabinet, captive coupling studs, service stairs and handrails. Transformers and cabinet are new models. Uniformly scaled Quaternius piped column/access point/pipe holder and Molten console supply subordinate service details with native UVs; they do not define the installation silhouette.
4. **Floor/platforms:** asymmetric connected load slabs, an east access throat, an elevated rear service plinth, two actual recessed cable volumes with slotted load-bearing grates and discrete crossings. The battle floor remains at y=0; height layers are surrounding presentation. No multi-level gameplay or R2 `deck()` regeneration.
5. **Scale:** wing columns 5.65 m, central 7.15 m; broad bases 3.2 m diameter. They are industrial installations beside the actual bipedal G-0, not player-sized props. Models export in metres at unit scale; no nonuniform global squeeze. The combat court and entry remain clear.
6. **Portrait composition:** one fixed production follow camera at offset (3.4,19.8,-13.6), FOV 48°, target height 3.2 m. The elevated framing places the player lower in the portrait and allocates more of the upper frame to machinery. The normal entry, battle centre, approach, edge and return are reviewed at that same camera, with paired UI visibility changes only. Taller crowns may leave frame on approach; base/coupling/contained core still disclose function. No beauty camera.

## Surface/render decisions

Graphite structural recesses, painted blue-grey shells, exposed steel, matte deck, ceramic insulators and limited warning paint use separate Lit PBR maps. Custom UVs use one repeating surface metre; they never normalize each face to an entire atlas cell. Macro panel joints, chamfers, cooling openings and mechanical thickness are geometry. Donor trim/palette UVs remain intact. Localized blue energy is inside opaque pressure armor; warm status is on control hardware.

Renderer `postProcessData` is explicitly populated from the installed URP package. Linear PBR, ACES and bloom 0.12/threshold 1.25 are the revised render contract. Existing shader/material/Android API checks remain. The render proof compares pixels with post-processing on/off. Captures use an sRGB SDR target matching the Android backbuffer; raw linear half-float readback into PNG is rejected as misleading.

## Gameplay scope

Actor V2, G-0, enemy/content IDs, movement/combat/HP/ranges/timers/rewards, equipment, quests, repair, progression and saves remain from APK 50. Only the sector's explicit blocker volumes and the shared fixed camera/render configuration change. The minimap consumes the same live layout authority. Other sector models remain unchanged. Tests use isolated temporary profiles; controlled damage/cooldown advancement and recording god mode are evidence fixtures, not gameplay balance edits.

Device visual judgement, Android frame time/touch/thermal/startup require actual Android review. The next stage is assessment of this reference APK on device before any Chapter-wide reproduction.
