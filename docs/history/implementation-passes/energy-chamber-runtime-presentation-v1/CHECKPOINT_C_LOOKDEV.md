# Checkpoint C — LookDev and living-machine presentation

The accepted architecture is frozen. FBXs, UVs, three Blender sources, all 248 prefab records outside material/LOD references, collision, production placement, Chapter scene and route layout remain unchanged. No other architecture object is started.

## Material roles and surface response

Twenty existing chamber material assets now use a scoped opaque URP PBR shader. Two production-only copies replace its references to shared R1 trims; the other facilities keep their existing trim materials. Unique used material counts remain 22 / 22 / 21 across LOD0/1/2.

Eight project-owned 512px families provide albedo wear, tangent normal, metallic/smoothness and occlusion maps. The reproducible authoring script is in Tools, and the maps use ASTC 6x6 with mipmaps on Android. Nominal metallic / smoothness (roughness = 1 - smoothness):

- Heavy graphite frame/base: **.68 / .26** (roughness .74), dark load-bearing structure.
- Cooler machined steel/hardware/interface: **.82 / .64** (.36), brighter edge and bracket response.
- Cold industrial casing: **.66 / .43** (.57), separate from frame and pipes.
- Mechanical pipe steel: **.85 / .70** (.30), localized sharper reflection.
- Gaskets/recesses: **.12 / .16** (.84), deep matte separation; practical emission disabled.
- Restrained copper couplings/insets: **.75 / .55** (.45).
- Service deck: **.45 / .30** (.70), stronger maintenance scuff and roughness variation.
- Pale cold panels: **.68 / .47** (.53), differentiated from structural steel.

Each map carries within-role grain, brushing, aging/scuff, scratch relief and roughness variation; these are nominal values, not a flat uniform scalar over the whole station. New role normal strength is .8 versus the former .3. The accepted upper segmented casing plates additionally use a machined zone at metallic .78 / smoothness .57 while the lower wall remains casing. Existing energy/marking/donor masks are retained. All original asset GUIDs and exported mesh submesh slots are preserved.

## Practical sources, local spill and motion

- Primary orange core HDR emission changes from `(2.6,.48,.045)` to `(4.3,.88,.07)`; inner energy uses `(2.8,.62,.045)`. Cyan nodes change from `(.045,.8,1.45)` to `(.035,1,1.9)`.
- Short orange plinth/service slots, collar sources, selected casing indicators, tiny cyan inner-node status sources and small side-machine service strips are masks on existing opaque surfaces. Geometry survey corrected their placement; they are not all-edge outlines.
- Warm opening/collar response and cool nearest-node response fall off locally across existing frame, panels, bases and pipes. Underlying albedo, roughness, normal relief and occlusion remain visible. This is a bounded surface spill approximation using the existing lighting/reflection, not a painted whole-object tint or a realtime point-light field.
- Core breathing combines slow 20.3/45.9 s waves at at most ±12%. Node periods/phases differ (about 22–25 s plus a slower 52.8 s component). Tiny indicators stay between .86 and 1 intensity. A selected conduit crest travels about .11 m/s and never switches off. There is no synchronized flashing or gameplay signal.
- **Realtime lights added: 0. Runtime C# presenters: 0. Particles/VFX systems: 0. Transparent layers/full-screen effects: 0.** Motion uses shader time; no new per-frame C# update or managed allocation is introduced.

## Camera and concept assessment

Eight required images A–H use the real loaded Chapter, initialized production PortraitFollowCamera, live G-0, Android's actual default quality and unchanged lens. A/B are equivalent problematic west/south views. F sets this station's emission/spill scale to 5% for material diagnosis; G restores normal treatment. H is a close north-facing gameplay detail view (the station naturally clips at the frame edge with this fixed camera). Additional energy-time and quality/distance samples support the diagnosis. Test-only combat/other actor animation is paused; it is not a runtime change.

Compared with the supplied approved concept:

- Material richness now comes from distinct metal roles and mapped surface response, with restrained copper rather than a single gray family.
- Dark gaskets/frame, colder panels, brighter hardware and machined caps create deeper panel separation and a clearer dark/light hierarchy.
- Orange energy warms local collars and joints; cyan nodes cool nearby frame and base. The surrounding Chapter lighting is preserved, so environmental bounce is more restrained than the concept image.
- Small practical slots punctuate service bands and side machinery instead of every edge glowing.
- Shader-time energy breathing, asynchronous nodes and conduit motion provide a working-machine layer without a large animation/VFX system.
- The concept still contains stronger cinematic bounce and surface aging. These captures show a closer material/light family; they do not establish a literal concept match or owner acceptance. The phone decides final emission strength, dark-panel readability and small-source visibility.

## Costs and executed gates

- LOD0: **45 / 279,694 triangles / 156 slots / 22 materials**.
- LOD1: **40 / 253,846 / 135 / 22**.
- LOD2: **26 / 215,286 / 115 / 21**.
- Renderer and near triangle deltas: **0**; material assets **20 changed, 2 added**. Thirty-two new maps occupy an estimated 5,069,824 bytes (4.835 MiB) in ASTC 6x6 including mip levels, before platform/driver overhead. The shader uses one normal opaque PBR forward pass plus standard shadow/depth passes.
- Shader fragment cost increases with local spill/indicator masks and slow sine evaluation. Retained distant assemblies cost more geometry/slots than APK55. There is no new overdraw layer or CPU presenter; Android GPU/FPS/thermal measurements remain a device gate.
- Final Unity compile and existing project validation: **exit 0**.
- Full EditMode: **533 passed, 0 failed**.
- Final PlayMode: **9 passed, 0 failed**: camera/quality sweeps, GPU energy variation, production wiring/map collision, actual G-0 locomotion/perimeter, full Chapter route, ordinary actor routes/arena clearance, attack/death/respawn regression.
- GPU screenshots at 0/9/21 s show bounded visible energy change with other actor animation frozen. Raw receipt is `verification/lookdev/operational-gpu.json`.

This runtime checkpoint precedes the ARM64 IL2CPP APK56 build. APK55 is preserved. Final Android artifact identity and READY FOR HUMAN DEVICE REVIEW status will be recorded only after the real build succeeds.
