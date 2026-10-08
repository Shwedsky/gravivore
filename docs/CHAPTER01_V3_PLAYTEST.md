# Chapter 01 V3 integrated playtest

Baseline: `c55fb7dcf14dd8bc19fa58bfec3ed4e69c7dda39` (accepted Concept Fidelity V2, PR #64).

Concept board accepted with bipedal G-0 override.

The supplied concept board is the visual authority for industrial density, warm/cool internal lighting, enemy style and Custodian intimidation. G-0 retains the approved bipedal silhouette, scale and controls.

Scope: repair-exit traversal; stronger strong spots; pooled outgoing/incoming damage; minimap live count then first-kill countdown; elite/boss basic pressure and combat leash; source/travel/impact presentation; one ranked M-0 weapon and durable boss rewards; hero-route, periphery and Custodian V3 presentation.

Delivery gate: integrated ARM64 Android development APK, ProjectValidator, full EditMode and PlayMode results. No intermediate art approval gate. Preserve Chapter 01 topology, 120-second ordinary waves, repair, Russian UI, saves, gates and repeat encounters.

## Integrated behavior

Repair exit investigation reproduced the stop at player X=4.4268, Z=-27 with the real capsule. Exact offending AABBs: `Chapter01 Service corridors Structural_Support 100` at (4.98,1.45,-28), size (1.04,2.89,.90); `Chapter01 Service corridors Maintenance_Station 101` at (5.56,.98,-26.57), size (1.27,1.96,1.22). Both authored objects and their corresponding proxies move +3 X. Retired repair-platform service-frame proxies are also omitted for the production platform. Regression walks from (0,0,-27) to (6,0,-27), then checks stylized perimeter lanes with the real controller.

Strong spots retain their actual ordinary archetype, reward ladder and population cap. HP multipliers are 1.7/1.85/2.0/2.2; damage 1.3/1.4/1.45/1.6; movement is 10% faster and attack intervals 10% shorter. The east elite-side spot moves two metres outward so every individual spawn anchor clears Magnetar's wider acquisition boundary.

Magnetar has 510 HP, 16 armor, 27 shockwave damage and 21-damage basics with .35s warning and 1.25s cadence. Custodian retains 820 HP / 15 armor, uses 46/52/62 special damage and 29-damage basics. Initial aggro is 8m; active combat leash is 21m; pursuit is 3.2m/s. Grace accumulates only beyond that leash and clears immediately upon re-entry.

Map markers show ×live count while survivors remain, then the authoritative first-kill countdown, then ГОТОВО while safe distance prevents respawn. The underlying 120-second wave model is retained.

Outgoing damage is pale cyan at the target. Incoming damage is larger red text beside G-0, prefixed with a minus sign, with dark outline and scale response. Three incoming slots merge hits arriving within .15s to reduce clutter. Hostile attack presentation reuses 16 source/travel/impact instances; authoritative damage still commits on the existing combat frame.

The common impulse emitter uses slot index 3, unlocks and auto-equips on the first legitimate rewarded Custodian kill, adds +12 damage at rank 1 and +4 per later rank through rank 5 (+28). Equipment UI supports removal/re-equipping. All three approved bipedal forms have a right-tool attachment and cached muzzle. Extra copies at rank 5 acknowledge loot without raising stats or creating a new economy. Core XP, loot, rank and first-clear state share the existing durable Prepared → Applied → clear reward transaction. Schema 2 → 3 migration preserves progression and inventory without granting historical boss loot.

V3 adds six original runtime meshes, broken deck edges, exposed conduits, trenches, collapsed hulls and nearer warm/cool route machinery. Custodian has six supports, layered raised command armor and an exposed powered dorsal cavity. The existing industrial atlas is shared. Atmosphere retains one spark instance and two reused unshadowed lights, plus fixed steam/dust systems capped at 32 particles. Existing scanners, repair arms, machinery animation and cold-start diagnostics remain.

## Delivery evidence

Final full EditMode: 455/455 passed. Final full PlayMode: 131/131 passed, no failures or skips; the optional structural capture test was explicitly enabled. Standalone ProjectValidator exited 0. The five-minute real-locomotion/combat scenario completed 17 route stops with at most 20 live enemies against the cap of 25. Shared materials stayed at 54; all 402 retained additional transforms were the accepted lazy actor-variant cache, and non-actor presentation remained fixed.

The 90-second cold-start recorder remains enabled in development builds and was exercised by the full PlayMode suite. These captures and performance inventories are Editor evidence, not device FPS measurements. Final test XML, runtime inventories and lossless structural PNGs are under `docs/chapter01-v3/`.

Unity compile, standalone ProjectValidator and Android DEV build all exited 0. The final APK verifier exited 0. All four accepted asset-packing checks match the delivered APK hash; the original production settings and actual serialized M-0 definition were independently inspected in the APK.

Branch: `integration/chapter01-v3-playtest-build`. Draft PR: https://github.com/Shwedsky/gravivore/pull/65.

APK source commit: `bc4b424907afffadfbc255aebea95903e9d4b01f`. The final evidence checkpoint changes documentation and verification artifacts only; runtime code and content are identical to this tested build source.

Delivered APK: `C:/Users/pamak/Documents/ChatGPT/gravivore/Builds/Android/gravivore-dev-0.1.0+42.apk`.

Version: `0.1.0`, versionCode `42`, package `com.gravivore.mobile.dev`, ARM64 only, IL2CPP, development/debuggable build. Size: **82,642,069 bytes**. SHA256: `efe69bc5c4e415dc05b0c3c4f23b30c1ff12d96107149699eb08f295d962f6c8`. APK signature verification passed and the signer matches accepted v41, allowing an in-place update. The delivered copy has the same SHA256 as the build output.

Evidence: `docs/chapter01-v3/verification/validation_summary.json`, `apk_verification.json`, `apk_delivery.json`, build metadata, signature/badging output, four asset-packing reports, `accepted_runtime_content.json` and `v3_runtime_content.json`. Full changed-file inventory: `docs/chapter01-v3/changed_files.txt`.

Runtime graphics captures are internal evidence, not an intermediate human approval gate. Android device frame rate and the owner playtest remain the next human gate. Hero-route dressing receives the most detail; distant peripheral alleys retain simpler baseline kit and lighting. The world uses the existing mobile atlas and the new original meshes; it remains a stylized mobile interpretation of the board rather than an exact reproduction of its rendered detail.

Assumptions: M-0's first copy auto-equips so the owner immediately sees the loot; additional copies at rank 5 acknowledge the kill's loot without further stats or an invented currency. Existing schema-2 profiles retain their history but receive the new weapon from their next legitimate rewarded Custodian kill.

Next specification/gate: owner installation and Chapter 01 V3 device playtest. No subsequent numbered implementation specification is authorized by this milestone.
