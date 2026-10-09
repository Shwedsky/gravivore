# Chapter 01 surface and hero prop production pass — V46

Continue [draft PR #68](https://github.com/Shwedsky/gravivore/pull/68) on `art/chapter01-visual-replacement-v3`. Do not merge. Accepted V45 baseline: `ce92f5744b0cd8f4064150c827828992f4dc1c85`.

## Surface system

One original 2048 shared hero atlas supplies BaseColor, tangent-space Normal, URP Metallic R / Smoothness A, Occlusion G and an emissive mask. Cyan and red emission variants reuse the geometry, base, normal and response maps. Sixteen-stripe 1024 maps preserve the accepted actor UVs; the ten-stripe slice mapping retains its different spacing. Small structural trim uses a shared 1024 surface. Existing donor trim gains occlusion extracted from the already verified Quaternius CC0 ORM textures, preserving its atlas quadrant order. Android import settings use mipmaps and ASTC 6×6; no 4K texture, custom shader, package or paid asset is introduced.

Panels combine cool steel, dark mechanical recesses, lighter exposed alloy, ochre paint and localized wear. Recessed seams, vent grooves, bolt heads and scratches carry normal/occlusion detail; painted edges expose a different metallic/smoothness response. Broad rolled-metal variation and darker joins avoid random camouflage. Contained emission has internal gradients and dark collars. Broad V45 corrective neon recedes behind authored status lenses and internal cores. G-0 keeps its unique baked base/emission and cyan identity, with additional normal/occlusion detail.

## Machinery and spawn origins

Nine editable original Blender sources are committed under `art/surface-hero-v46/`. Separate named mesh parts and relative texture links are retained in each source; one atlas mesh is exported per family. Blender reopened all nine files and verified their UVs and texture links. Runtime creates neither these meshes nor material variants.

- Pressure vessel: closed capsule pressure shell, forged collars, bolted inspection crown, attached saddles/feet, recessed access panel, elbow/flange, return pipe and valve actuator. Replaces the stacked-ring/cup language.
- Energy cylinder: contained luminous volume, six bowed cage rails, armored top/bottom coolant housings, isolators, attached service panels and coolant braids. Amber industrial and red containment uses are fitted to the accepted footprints; cyan service response is available in the shared material family.
- Curved machinery: forged partial induction frame, recessed winding, segmented service cassettes, articulated shoes, focus rotor/core, asymmetric cabinet and braided bus feeds.
- Wall/gate: layered jambs, darker cavity, separate inset shield panels, structural ribs, warning paint, service access and a small diagnostic emitter.
- Light/heavy/special docks: service backplate, paired chargers, recessed parking bed, front guides/stops, power spine, conduits, status sources and open patrol-facing entrance. Heavy docks add lifting cross-heads/arms; special docks add industrial header services. All nine ordinary/strong origin markers remain at their V45 positions. Low authored berth guides retain the four existing nearby anchors.
- Service gantries and dock aprons replace the corresponding baked primitive assemblies. The remaining baked primitive geometry is explicitly audited as low gratings, rails, small beacon pedestals and inaccessible utility trim.

## Intersections and accepted behavior

The pass consolidates legacy machinery skins that occupied the same proxy and suppresses older conduit/scaffold/vent cladding crossing the new authored machines. Other affected art is scaled/repositioned inside its original envelope to clear neighboring machinery. New attached pipes, shoes and brackets remain intentional structural connections. The final conservative bounds scan reports zero overlapping V46 major machinery candidates; the reviewed close/production captures show zero visible intersection or z-fighting defects. This is an internal camera review, not a claim of exhaustive device-frame coverage.

The complete V45 collision fingerprint is unchanged. A 763-file scope audit verifies runtime code, packages, world/spawn/encounter definitions, patrol settings, accepted actor/weapon models and referenced presentation prefabs against V45. World size remains 56×118, Repair-to-elite distance remains 68m, arena/gate dimensions remain accepted, and ordinary patrol speed/radius stay 0.5m/s / 0.9m. Combat, spawn counts, progression, saves, map semantics, UI layout and M-0 mounting/rank presentation remain unchanged. No new runtime light is added; the existing authored key light remains.

Magnetar and Custodian receive richer shared atlas response, lighter armor versus dark mechanics, panel relief, localized wear and contained core falloff. Their meshes, skeletons, clips and combat authority are unchanged. Scout, Cutter, Warden, Arc Drone and Carrier use the same enriched shared surface family.

## Internal visual gate

The local owner concept board and inspected donor contact sheets were used as design references; the owner image is not redistributed. Neutral-lighting renders and the real portrait gameplay camera were reviewed for silhouette, large/medium forms, small detail, within-object material variation, roughness response, edge highlights, recesses, integrated emission and top-down readability. Pressure and containment silhouettes, attached services, inset plates and dock openings pass the internal production gate. Final owner judgment remains the next gate.

Five required boards combine fresh V45 baseline captures, corresponding V46 camera views and close/neutral asset views:

- [Surface quality](surface-hero-v46/internal/surface_quality_board.jpg)
- [Spawn docks](surface-hero-v46/internal/spawn_dock_board.jpg)
- [Hero machinery](surface-hero-v46/internal/hero_machinery_board.jpg)
- [Actor surfaces](surface-hero-v46/internal/actor_surface_board.jpg)
- [Intersection cleanup](surface-hero-v46/internal/intersection_cleanup_board.jpg)

## Executed validation

Standalone compile and ProjectValidator exited 0. Full EditMode: **485 passed, zero failures**. Full PlayMode: **137 passed, zero failures, one optional legacy capture ignored**. The full PlayMode run includes locked/unlocked capsule paths, actual controller traversal, accepted UI/equipment/encounter flows and the five-minute runtime route. The soak ran 300.0004 seconds, covered all 17 stops, held materials at 68 before/after, capped live enemies at 20 and preserved all non-actor transforms. The existing per-archetype pooled actor cache accounts for the retained actor transform increase.

Material quality audit: **376 hero material rows; zero flat-color-only hero renderers; zero primitive-only hero props**. Every renderer is recorded with material/mesh paths, base/normal/response/AO/emission presence, hero classification and explicit support exemptions. Rendering audits and actual GPU captures report supported URP materials with no missing or error material. License audit re-verifies the 36 existing licensed source files and 12 trim textures, reused CC0 occlusion data, shipped notices and nine original editable sources.

The final portability cleanup shortened five generated support-mesh asset names. Their vertices, triangles, normals and UVs are identical to the full-suite tested geometry (`mesh_alias_equivalence.txt`). The final aliases additionally pass all eight V46 EditMode checks and the V46 production-scene PlayMode capture test.

Final static scene inventory: 1,008 enabled renderers and 1,611,176 static triangles versus V45's 1,112 and 1,392,396. This is about 9% fewer renderers and 16% more authored triangles across the entire chapter. Higher geometric detail and the additional maps require owner/device performance review; Editor results do not establish Android 60 FPS. The five-minute Editor run is functional/stability evidence.

## Android delivery

Unity's Android build exited 0 after the internal visual gate. Delivered APK: `C:/Users/pamak/Documents/ChatGPT/gravivore/Builds/Android/gravivore-dev-0.1.0+46.apk`, **115,291,303 bytes**, SHA256 `10e31659731d931f62eb04e670c2369eee4322d43eef4505d7e9dc77c8cc495d`. Build source: `dbc0dcc0b8324ba6a7cedb239d403a8b7c8aff44`; the follow-up commit records delivery evidence only. VersionCode 46, versionName 0.1.0, package `com.gravivore.mobile.dev`, ARM64-only IL2CPP DEV/debuggable, signature and same signer as V45 all passed actual package verification.

Typed packed-asset inspection resolves all nine V46 families to actual serialized meshes, the three shared hero materials to URP/Lit with normal/occlusion/metallic/emission keywords, and the required texture maps in the package. Build dependency callbacks confirm the referenced PBR resources are packed; Vulkan and GLES3x compiled shader receipts include the required surface keywords. The preserved world, patrol settings, M-0 ranks/thumbnails and production UI also pass typed package inspection. Delivery verification is recorded in `surface-hero-v46/verification/delivery.json`. V45 remains at its existing path with unchanged SHA256 `7486e448ab9304d3fc3fd61d1f8a2b90dd4743585b4f41e1b51e25a45b0d31d0`.

The changed-file manifest is under `surface-hero-v46/verification/files_changed.txt`. Raw execution logs remain available locally; compact validation receipts, NUnit XML, source/mapping audits, camera evidence and package verification are retained with this pass.

Assumption: visual cleanup may consolidate duplicate skins and adjust art within the existing occupied envelopes; collision and gameplay authority remain V45-exact. Inspection focuses on the authored families and corresponding production viewpoints. No physical Android execution is claimed.

Next gate/spec id: **V46-DEVICE-ACCEPTANCE**. Install V46 and judge material depth, object quality, spawn-dock readability and whether the world still looks toy-like. PR #68 remains draft and unmerged.
