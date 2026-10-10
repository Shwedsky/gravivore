# Chapter 01 concept fidelity corrective pass — V45

Branch: `art/chapter01-visual-replacement-v3`. Continue [draft PR #68](https://github.com/Shwedsky/gravivore/pull/68); do not merge. Baseline: `f57c8ac94b73c74b6a05cd242b57576f615c0b3b`, the delivered V44 checkpoint.

## Result and implementation

The scene now combines serviced machinery clusters, suspended utility gantries, walkable service channels, local power accents and explicit enemy docking origins. Broad empty transit is compressed; the accepted V44 actor models, mounted rank 1–5 weapon, UI skin and menu implementation are retained.

- Composition: smaller 4m deck panels, worn/damaged sections, denser corridor edges, connected machine/support/vent clusters, suspended service bearers and terminated utility runs. Repeated small service geometry is baked offline into shared sector/material meshes; production prefabs remain referenced through scene overrides.
- Readability: graphite deck, brighter worn cold-steel housings, non-emissive ochre warning paint, cyan Repair Hub diagnostics, amber power feeds, white inspection lamps and red hostile containment. The existing pair of reused unshadowed point lights gains actual beacon sources and a larger local response; no additional runtime light pool or shader is introduced.
- Ecology: five ordinary spots and four stronger spots have charging cradles, service consoles, rear power racks, connected conduits and foreground maintenance machinery. The capacitor field emphasizes energy services, freight areas use storage equipment, relay areas use inspection lighting, and stronger nests use heavier fabrication support. Origins remain presentation, with no new spawning/reward system.
- Ambient life: all pooled ordinary and strong enemies take short deterministic patrols, pause and change heading through their real CharacterControllers. Radius 0.9m around each leased spawn anchor; speed 0.5m/s; pauses 1.4–3.6s; maximum travel phase 2.4s. Combat decisions take priority. Pool reuse resets the patrol home and timers. Existing animation observers see the real movement; accepted elite/boss idle and combat rigs remain.
- Compaction: world 72×140m → 56×118m. Repair-to-elite-gate distance 90m → 68m (24.4% shorter). Five zone/spawn positions, visual anchors, obstacle proxies and map bounds move together. The elite/boss complex translates by −22m with the same arena radius, gate spacing, combat parameters, progression ids and rewards. Repair origin remains −30m.
- Encounters: Magnetar has paired powered induction banks, exposed laminations and stabilizer services. Custodian has taller red containment towers, pressure machinery and a rear control gantry outside its charge apron. New solid machinery has authored collision proxies; suspended detail and flush walkable gratings stay decorative.

## Internal review

[Six before/after boards](concept-corrective-v45/internal) compare archived V44 production captures with the same portrait camera settings at the corresponding V45 subjects. Encounter/UI states differ; these are visual comparisons, not synchronized performance captures. The owner concept board is retained locally and not redistributed.

The first corridor mask measured 65.7% visible flat deck and failed the density review. Further iteration added serviced gratings, closer perimeter clusters and suspended structures. The final reviewed corridor measures **36.8% flat deck**, with UI hidden; worn and marked deck still count as floor. Connected service gratings count as machinery detail. This is one measured production view, not an assertion that every frame in the chapter is below 40%; deliberate combat aprons retain maneuvering space. See [mask receipt](concept-corrective-v45/verification/04_service_corridor_floor_coverage.json).

The [idle sequence](concept-corrective-v45/internal/idle_patrol_sequence.jpg) and [controller receipt](concept-corrective-v45/verification/idle_life.json) record real movement, pauses, bounded home distance, unchanged HP and stable transform count.

## Validation and delivery

Standalone compile and ProjectValidator exited 0. All 477 EditMode tests passed. The final PlayMode run passed 135 cases with zero failures and one optional legacy capture skipped; the unchanged runtime/layout also passed the five-minute soak, for 136 unique passing PlayMode cases. The ordering regression passed 27/27 after the test-only capsule planner correction. The soak covered 17 stops, capped live enemies at 20, held materials at 65 and preserved all non-actor transforms. See [executed validation](concept-corrective-v45/verification/validation.json).

The ARM64 IL2CPP DEV build succeeded with Unity 6000.3.0f1. Package verification passed: application id `com.gravivore.mobile.dev`, version `0.1.0`, versionCode **45**, debuggable flag, ARM64-only native libraries, compiled accepted UI/weapon types, preserved production resources, all eight corrective materials resolving to URP/Lit, all nine spawn origins, actual static meshes, serialized 56x118 layout and bounded patrol values. Vulkan/GLES3x shader variants are present; both serialized scenes have zero missing/error materials. The signing certificate matches V44.

Delivered APK: `C:/Users/pamak/Documents/ChatGPT/gravivore/Builds/Android/gravivore-dev-0.1.0+45.apk`. Size: **100,790,065 bytes** (96.12 MiB). SHA256: `7486e448ab9304d3fc3fd61d1f8a2b90dd4743585b4f41e1b51e25a45b0d31d0`. Build source: `e66994b747e739fb63373cafc02607ce0672646c`. The final follow-up commit adds delivery evidence only; runtime/assets are unchanged from that build source. The main-folder copy matches the verified build byte for byte.

V44 is preserved at its existing path with SHA256 `4abb00bb960c6569e4ffa08e3bb9d2d29b8aba1012b61ebb4f0108eb22a0c044`. See [delivery receipt](concept-corrective-v45/verification/delivery.json), [APK verification](concept-corrective-v45/verification/apk_verification.json) and [typed packed-content proof](concept-corrective-v45/verification/apk_typed_production.json). No Android device execution or device FPS is inferred from Editor evidence.

The [changed-file manifest](concept-corrective-v45/verification/files_changed.txt) records the scene/data assets, bounded-patrol implementation and injection, renderer audit, adapted coordinate/route tests and local build-verification tools. Historical V44 captures/reports are retained; V45 evidence has its own directory.

The renderer inventory is 1,112 enabled authored renderers and 1,392,396 enabled static triangles, compared with V44's 884 and 1,110,624. This approximately 25% geometry increase is a material mobile-performance limitation to check on device. Small service details are merged into sector meshes offline, Unity static batching remains enabled, and the existing two reused local lights are retained.

The [scope audit](concept-corrective-v45/verification/scope_audit.json) verifies 490 existing runtime/package files against V44, permits only the five explicit runtime integration changes, and checks that ordinary/elite/boss balance is unchanged apart from positions. Accepted V44 actor/weapon/UI/material content is byte-equivalent after line-ending/serialization whitespace normalization. No package or external asset was added; the existing 36-source-file/12-trim-texture license audit passes.

Assumptions: compact layout is authored offline and retains all location/encounter ids; only ordinary and strong packs gain patrol movement, while elite/boss behavior remains accepted. The walking lane includes the flush service gratings, and real capsule route tests govern reachability. The 35–40% floor guideline is evaluated as visible homogeneous deck, excluding connected machinery/gratings. Device visual approval and sustained mobile performance remain human gates.

Next gate/spec id: **V45-DEVICE-ACCEPTANCE**. PR #68 remains draft and unmerged.
