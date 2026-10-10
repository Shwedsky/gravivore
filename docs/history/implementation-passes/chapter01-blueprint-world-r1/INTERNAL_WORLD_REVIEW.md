# Chapter 01 Blueprint World-only R1 — internal portrait review

Status: internal review completed before the Android build; owner/device visual acceptance remains pending.

The review uses the unchanged production camera (portrait 540 × 960, FOV 46, follow offset 0/14.8/-11.2, look height 0.9). All eight final sector captures and the two facility-walk views were inspected. The camera, actor models, weapon presentation and UI skin were retained.

The previous V45/V46/V47 environment object was removed. The new sector models own their foundations, architecture, machinery and services. Movement surfaces and major collision footprints are separate authored data; decorative meshes have no collision.

- Repair Hub: circular maintenance installation, paired accepted repair manipulators, diagnostic consoles, cyan service equipment and a clear north exit. The central repair pocket remains open.
- Relay Yard: a deployment building frames the patrol apron with a continuous roof, open service mouth, control fascia, side assembly conveyors and communications backbone. The initial empty central composition was corrected by bringing the building across the north service mouth.
- Capacitor Field: vertical contained-energy vessels, a connected power collector, switching cabinets, generator and raised side catwalk define an energy-distribution facility. Cyan and warning red remain distinct.
- Cutting Floor: the broad cutting press, hydraulic cheeks, suspended ram and heated jaw frame a fabrication work cell. Floor rails connect its open combat apron to the machinery.
- Shield Dump: broad armored storage/inspection cells, sorted slabs, a centrifuge and service feed communicate armor processing. The central Warden space remains clear.
- Hauler Graveyard: offset, tilted freight hulls with split lids and structural ribs, a loading crane, overhead catwalk and damaged hydraulic return communicate a logistics yard. The loading apron is clear.
- Magnetar Complex: a bespoke amber pressure chamber with curved shielding, exposed core, pressure heads, inspection spines and laminated magnetic return yokes replaces the ordinary-sector generator silhouette. The installation sits to the left of the elite lane, connected to the facility feed and side commissioning bays.
- Custodian Core: a larger red containment chamber and supporting energy vessels anchor the rear of the arena, with enclosing industrial walls, side gantries, redundant coolant and an inspection apron. The boss telegraph foreground stays open.

Iterations before this review corrected FBX unit scale, oversized service-network culling groups, localized Blender UV-channel names, URP emission flags, architecture placement for the fixed portrait camera, Relay bay massing and the elite/boss reactor identities. The remaining source has one canonical UVMap per mesh.

The eight sector-center views show the combat foreground together with the functional machinery/bay behind it. The facility-walk views check the larger elite/boss installations from normal follow-camera positions. These are internal scene observations, not a claim that the owner's final concept-fidelity gate has passed.

The remapped expanded map uses the same authored ground union and actual collision/gate authority as gameplay. Its existing frame, heading behavior, marker semantics and interaction remain. The elite and boss labels are now «Комплекс Магнетара» and «Ядро Кустодиана».

No intermediate owner approval was requested. The Draft PR remains the container for the device/world review after versionCode 48 is built.
