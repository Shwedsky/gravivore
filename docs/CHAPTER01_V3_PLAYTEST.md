# Chapter 01 V3 integrated playtest

Baseline: `c55fb7dcf14dd8bc19fa58bfec3ed4e69c7dda39` (accepted Concept Fidelity V2, PR #64).

Concept board accepted with bipedal G-0 override.

The supplied concept board is the visual authority for industrial density, warm/cool internal lighting, enemy style and Custodian intimidation. G-0 retains the approved bipedal silhouette, scale and controls.

Scope: repair-exit traversal; stronger strong spots; pooled outgoing/incoming damage; minimap live count then first-kill countdown; elite/boss basic pressure and combat leash; source/travel/impact presentation; one ranked M-0 weapon and durable boss rewards; hero-route, periphery and Custodian V3 presentation.

Delivery gate: integrated ARM64 Android development APK, ProjectValidator, full EditMode and PlayMode results. No intermediate art approval gate. Preserve Chapter 01 topology, 120-second ordinary waves, repair, Russian UI, saves, gates and repeat encounters.

## Integrated behavior

Repair exit investigation reproduced the stop at player X=4.4268, Z=-27 with the real capsule. Exact offending AABBs: `Chapter01 Service corridors Structural_Support 100` at (4.98,1.45,-28), size (1.04,2.89,.90); `Chapter01 Service corridors Maintenance_Station 101` at (5.56,.98,-26.57), size (1.27,1.96,1.22). Both authored objects and their corresponding proxies move +3 X. Retired repair-platform service-frame proxies are also omitted for the production platform. Regression walks from (0,0,-27) to (6,0,-27), then checks stylized perimeter lanes with the real controller.

Strong spots retain their actual ordinary archetype, reward ladder and population cap. HP multipliers are 1.7/1.85/2.0/2.2; damage 1.3/1.4/1.45/1.6; movement is 10% faster and attack intervals 10% shorter. The east elite-side anchor moves one metre outward to avoid crossing Magnetar's wider acquisition boundary.

Magnetar has 510 HP, 16 armor, 27 shockwave damage and 21-damage basics with .35s warning and 1.25s cadence. Custodian retains 820 HP / 15 armor, uses 46/52/62 special damage and 29-damage basics. Initial aggro is 8m; active combat leash is 21m; pursuit is 3.2m/s. Grace accumulates only beyond that leash and clears immediately upon re-entry.

Map markers show ×live count while survivors remain, then the authoritative first-kill countdown, then ГОТОВО while safe distance prevents respawn. The underlying 120-second wave model is retained.

Outgoing damage is pale cyan at the target. Incoming damage is larger red text beside G-0, prefixed with a minus sign, with dark outline and scale response. Three incoming slots merge hits arriving within .15s to reduce clutter. Hostile attack presentation reuses 16 source/travel/impact instances; authoritative damage still commits on the existing combat frame.

The common impulse emitter uses slot index 3, unlocks and auto-equips on the first legitimate rewarded Custodian kill, adds +12 damage at rank 1 and +4 per later rank through rank 5 (+28). Equipment UI supports removal/re-equipping. All three approved bipedal forms have a right-tool attachment and cached muzzle. Extra copies at rank 5 acknowledge loot without raising stats or creating a new economy. Core XP, loot, rank and first-clear state share the existing durable Prepared → Applied → clear reward transaction. Schema 2 → 3 migration preserves progression and inventory without granting historical boss loot.

V3 adds six original runtime meshes, broken deck edges, exposed conduits, trenches, collapsed hulls and nearer warm/cool route machinery. Custodian has six supports, layered raised command armor and an exposed powered dorsal cavity. The existing industrial atlas is shared. Atmosphere retains one spark instance and two reused unshadowed lights, plus fixed steam/dust systems capped at 32 particles. Existing scanners, repair arms, machinery animation and cold-start diagnostics remain.

## Delivery evidence

Full validation and APK provenance are recorded after the integrated build. Runtime graphics captures are internal evidence, not an intermediate human approval gate. Android device frame rate and the owner playtest remain the next human gate. Hero-route dressing receives the most detail; distant peripheral alleys retain simpler baseline kit and lighting.
