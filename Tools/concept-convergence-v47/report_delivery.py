"""Write the delivery report only from completed V47 verification artifacts."""
import hashlib,json,subprocess,xml.etree.ElementTree as ET
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'docs/history/implementation-passes/chapter01-visual-replacement-v3/v47';V=OUT/'verification'
def read(name):return json.loads((V/name).read_text(encoding='utf-8-sig'))
scope=read('scope_and_cost_audit.json');apk=read('apk_verification.json');upgrade=read('apk_upgrade_and_shader_proof.json')
assert scope['validated'] and apk['validated'] and upgrade['validated']
tests={'EditModeFinal':ET.parse(V/'EditModeFinal.xml').getroot().attrib,'PlayModeFinal':read('test_summary.json')['PlayModeResolved']}
assert tests['EditModeFinal']['failed']=='0' and tests['EditModeFinal']['result']=='Passed'
assert tests['PlayModeFinal']['failed']==0 and tests['PlayModeFinal']['passed']==138
soak=read('regression_receipts/five_minute_runtime.json');assert soak['seconds']>=300 and soak['completedStops']>=9
baseline=scope['metrics']['v46'];current=scope['metrics']['v47'];delta=scope['delta']
actors=[]
for a in current['actors']:
    old=next(b for b in baseline['actors'] if b['name']==a['name'])
    actors.append(f"- {a['name']}: LOD0 {old['lodTriangles'][0]:,} → {a['lodTriangles'][0]:,} ({a['lodTriangles'][0]-old['lodTriangles'][0]:+,}); current LOD0/1/2 {a['lodTriangles']}; {a['bones']} preserved bones.")
files=subprocess.check_output(['git','diff','--name-only','41010938b1ce1ef31e81f6381c12f26f7f2131c8','HEAD','--','Assets','Tools','art'],cwd=ROOT,text=True).splitlines()
(OUT/'files_changed.txt').write_text('\n'.join(files)+'\n')
canonical=Path('C:/Users/pamak/Documents/ChatGPT/gravivore/Builds/Android/gravivore-dev-0.1.0+47.apk')
assert canonical.exists() and hashlib.sha256(canonical.read_bytes()).hexdigest()==apk['sha256']
text=f'''# CHAPTER 01 CONCEPT CONVERGENCE V47 READY FOR DEVICE REVIEW

Status: historical delivery evidence, NON-AUTHORITATIVE. Continue the same [draft PR #68](https://github.com/Shwedsky/gravivore/pull/68), branch `art/chapter01-visual-replacement-v3`. No merge or new visual PR.

## Synchronization and authority

Synchronized main: `81de953e9df2f0a09d03035d0809a00186d34630`. Conservative merge: `863757da38689eb02b4e0d4cb251ace2d5ff80ab`. There were no textual merge conflicts. The branch's V44/V45/V46 evidence locations conflicted with main's documentation policy, so reports and evidence were moved into the corresponding history folders. All 361 locally preserved files were hash-checked during relocation; useful historical evidence was retained. Existing callback/tool output paths were updated to avoid recreating superseded docs roots. Old embedded paths in historical receipts remain historical facts.

All 15 requested authority documents were reread in the requested order from the synchronized branch. Their final bytes match current main, as recorded in [scope_and_cost_audit.json](verification/scope_and_cost_audit.json). The owner concept binary was inspected again as reference only. No historical art document or asset cache became a current source. No current authority document was changed to justify this implementation.

Build source SHA: `{apk['buildSourceSha']}`. The final branch SHA is reported in the delivery chat after the evidence-only commit; the APK's source SHA is deliberately recorded separately. Synchronization, history cleanup, actor replacement, facilities/deck, and reactor/test corrections were each committed and pushed during work.

**G-0 remains BIPEDAL. LOW-POLY was treated only as optimization, not style. docs/history was treated as NON-AUTHORITATIVE.** The target remains mobile-optimized premium hard-surface industrial sci-fi.

## V46 inspection and V47 changes

The baseline used the real 540×960 production camera, HUD and scene wiring. It exposed three high-impact gaps: slab-like actor bodies and weak mechanical joints, encounter origins that read as small consoles rather than industrial facilities, and repetitive bright deck strips that competed with combat. V46's larger machinery and PBR response were valuable and were preserved. Equipment source/travel/impact, equipment inventory, minimap, boss HUD, G-0 anatomy, zone transitions and repair machinery were reviewed; their interaction semantics were preserved.

- Scout: low split swept recon carapace, pointed forward sensors, articulated supports and rear heat sinks.
- Cutter: forward predator armor, independent forged cutting jaws, visible drive bearings and powered blade edges.
- Warden: overlapping bastion plates, broad defensive cheeks, supported shield mechanism and inset warning/core treatment.
- Arc Drone: suspended drive hull, swept electrode wings, rear stabilizer and ducted rotor housings.
- Carrier: load-bearing traction wheels/axles, swept fenders, contained load capsules and frontal impact ram.
- Magnetar Guard: broad induction shoulders, exposed heat-exchanger banks, articulated crushing forearms/claws and contained amber core.
- Custodian M-0: separate curved moving shutters, attached load struts/feet, charged sweep weapons, offset command housing, supported forward dorsal reactor and concentric animated circulation bearing. The core is now visible from the production camera instead of sitting behind the rear command mass.
- Nine existing ordinary/strong encounter origins: two DeploymentBay structures (relay), five FabricationBay structures (cutting, shield, hauler and four strong origins), and two? See the exact manifest below rather than infer distribution from family names. The three authored facility families use planted jambs, a substantial open 7.48m bridge, central machinery, service routing and role-specific magazines, fabrication tools or induction containment. Grounded solid forms fit the existing console/rack footprints; overhead tooling stays behind the spawn apron. See [facility_manifest.txt](verification/facility_manifest.txt) for all nine exact placements.
- World: three fitted 4m deck panel variants with opaque recessed subdeck and flush service grating replace the prior small-panel deck presentation across the unchanged 56×118m chapter. Existing gates, large machinery, repair hub and corridor hierarchy are retained. The neutral key was adjusted to RGB (0.91,0.94,1.0), intensity 1.65, to expose metal and recesses.
- UI/player/equipment: no interaction or presentation asset redesign. Fresh regression captures record the existing ranked M-0 inventory, visible equipped model and source → travel → impact sequence. G-0 and M-0 source assets, equipment definitions and save behaviour are unchanged.

Editable sources: [art/concept-convergence-v47/README.md](../../../../../art/concept-convergence-v47/README.md). Runtime facilities/materials/prefabs: `Assets/_Game/Content/ConceptConvergenceV47/`. Actor FBXs retain existing runtime paths/GUIDs in VisualSlice, Chapter01Production and Chapter01V3; prefabs retain their rig/anchor/controller structure. [files_changed.txt](files_changed.txt) lists changed production/source/tool files against V46; the authority refresh receipt lists history moves.

## PBR and optimization

All pre-existing V46 material and texture payloads are preserved. Whole actor prefabs use V46 HeroRed (ordinary enemies/boss) or HeroAmber (Magnetar), with base color, normal, metallic/smoothness, occlusion and emission maps intact. Facilities share HeroRed. The new WorkingDeck material clones HeroAmber's five maps, changes its base multiplier to (0.55,0.59,0.62), smoothness to 0.42 and normal scale to 0.52. No new textures or lights were added. New rich art bypasses the legacy S15 Body/Accent/Dark material recipe.

Serialized-scene inventory, before runtime population/culling:

- Enabled renderers: {baseline['renderers']} → {current['renderers']} ({delta['renderers']:+}).
- Unique enabled materials: {baseline['materials']} → {current['materials']} ({delta['materials']:+}).
- Static mesh triangles across the whole chapter: {baseline['staticTriangles']:,} → {current['staticTriangles']:,} ({delta['staticTriangles']:+,}, about 0.47%). This is scene inventory, not visible phone-camera triangles or GPU work.
- Dependency textures: {baseline['textureCount']} → {current['textureCount']} ({delta['textureCount']:+}); Editor runtime texture-memory estimate {baseline['textureBytes']:,} → {current['textureBytes']:,} bytes ({delta['textureBytes']:+}). It is not a device resident-memory measurement.
- Serialized lights: {baseline['lights']} → {current['lights']}; actual runtime has three realtime lights including one shadow light, unchanged. Seven actors retain all three LODs and the same 86 total rig bones. Total one-instance-per-archetype LOD0 falls from 63,982 to 55,272 triangles (−8,710).

{chr(10).join(actors)}

The Tier2 G-0 plus four actual Cutters audit records 53,984 LOD0 triangles, six renderers/six material slots, three materials and 15 textures; the corresponding V46 geometry was 46,416 triangles. The ASTC6×6 mip-inclusive estimate for that subset is 18,751,440 bytes. Individual V47 portrait/close captures include camera-frustum component inventories (`*_cost.json`): these include occluded bounds and enabled lower LOD components and exclude shadow/UI passes, so they are upper-bound inventories, not draw-call or GPU measurements. Static facilities/decks are batched in the shipped scene; actor geometry uses rigid mechanical skinning. Opaque meshes avoid adding transparent overdraw.

## Validation and APK

- Compile: executed successfully (`CompileFinal.log`).
- ProjectValidator: executed successfully (`ValidateFinal.log`). Presentation/content checks include whole-prefab validity, missing meshes/materials, URP shaders, all five PBR maps, dependency source policy and the nine V47 placements. [presentation_audit.json](verification/presentation_audit.json) records zero missing mesh/material references and zero historical source dependencies.
- Full final EditMode: {tests['EditModeFinal']['passed']} passed, {tests['EditModeFinal']['failed']} failed.
- Full PlayMode plus affected-test rerun: {tests['PlayModeFinal']['passed']} passed, {tests['PlayModeFinal']['failed']} unresolved failures, {tests['PlayModeFinal']['skipped']} skipped. The last full run was 137 passed / one diagnostic-helper failure / one skipped; the affected camera test then passed after its Unity null-check correction. Gameplay code and geometry were unchanged after that full run. The skipped structural-export test is explicitly opt-in; V47 real-camera/close captures were executed by the integrated V47 test. Exact raw full-suite results and the bounded rerun are retained in [test_summary.json](verification/test_summary.json) and XML.
- The initial PlayMode failures were obsolete Art Spike assertions: a fixed VR3 material name and a 50,000-triangle proxy ceiling. The revised test checks the actual rich material, shader and all five maps and records the current geometry inventory. Gameplay health, root physics, death recycle, target alignment and progression checks remain. The full suites were rerun after correction.
- Collision fingerprint is identical to V46. Full PlayMode verifies all spawn capsules and required routes, unchanged world/gates/encounters, progression, respawns, equipment persistence, boss phases/telegraphs and repeatable flow. [reachability.txt](verification/regression_receipts/reachability.txt) records capsule and CharacterController route checks.
- Sustained runtime: {soak['seconds']:.3f}s, {soak['completedStops']} completed stops, live-enemy peak {soak['maximumLiveEnemies']}; materials {soak['initialMaterials']} → {soak['finalMaterials']}; non-actor transform count unchanged. Actor transforms rise by {soak['finalActorTransforms']-soak['initialActorTransforms']} within the existing explicit pooled visual-variant cache. See [five_minute_runtime.json](verification/regression_receipts/five_minute_runtime.json). Editor worst frame gap was {soak['worstFrameGapMilliseconds']:.2f}ms, including batch rendering/capture/diagnostic activity; it cannot establish device FPS.
- Android build succeeded: DEV, 0.1.0, versionCode 47, `com.gravivore.mobile.dev`, portrait, ARM64-only IL2CPP, debuggable. Package/signature/provenance checks, typed shipped scene/actor/facility/texture/shader references and Vulkan/GLES3x PBR variants passed. The signer matches distributed V46, permitting the normal DEV upgrade path. See [apk_verification.json](verification/apk_verification.json), [apk_v47_scene_skin_pbr.json](verification/apk_v47_scene_skin_pbr.json) and [apk_upgrade_and_shader_proof.json](verification/apk_upgrade_and_shader_proof.json).

Exact review APK path: `{canonical}`.

APK SHA256: `{apk['sha256']}`. Size: {apk['bytes']:,} bytes. Build source: `{apk['buildSourceSha']}`. The copied review APK was independently hash-checked against the audited worktree APK.

## Remaining gaps, limits and assumptions

The pass materially improves actor silhouettes, attached mechanisms, encounter origin scale and structural depth. It does not claim complete concept fidelity. Open areas still show repeated deck modules; larger bespoke zone-scale architecture and transition asymmetry remain. Some actors still share manufactured shell vocabulary and would benefit from further individually sculpted industrial profiles. The boss HUD/minimap can overlap upper machinery at close player distances; their layout and interaction semantics were retained. UI polish beyond the equipment panel remains a later presentation gap. At the camera scale, small mechanical details can still disappear and hostile emission can appear saturated; device review must judge those trade-offs.

No physical Android run, GPU frame timing, thermal soak, battery or resident-memory measurement was performed. 60 FPS on a mid-range Android device remains a target, not a measured result. Whole-chapter static geometry remains about 1.62M triangles and the existing dependency set remains substantial; culling, batching, shadows and skinning need real-device confirmation. New facilities extend vertically above existing ground collision footprints, with open suspended tooling; no new gameplay collision was introduced because movement and topology were explicitly preserved. Their exact visible/proxy fit should be judged on device as well as by the recorded route and footprint checks.

No gameplay/runtime, balance, definition, spawn timer, phase, progression, persistence, package, G-0 or equipment source was changed. No new external download, paid dependency, restricted raw payload or game-derived asset was used. Unity's incidental legacy-material/import/settings serialization changes were restored and excluded from the delivery. Historical receipts were preserved; fresh regression snapshots are under V47. No stable project decision changed.

Next acceptance/spec gate: **V47-DEVICE-ACCEPTANCE — REAL APK ON DEVICE**. PR #68 remains DRAFT and unmerged.
'''
# Exact family distribution comes from the authored placement manifest.
manifest=(V/'facility_manifest.txt').read_text().splitlines()
distribution={family:sum(line.startswith('V47 '+family+' / ') for line in manifest) for family in ('DeploymentBay','FabricationBay','InductionStation')}
start=text.index('- Nine existing ordinary/strong encounter origins:')
end=text.index(' The three authored facility families',start)
text=text[:start]+f"- Nine existing ordinary/strong encounter origins: {distribution['DeploymentBay']} DeploymentBay, {distribution['FabricationBay']} FabricationBay and {distribution['InductionStation']} InductionStation placements."+text[end:]
(OUT/'DELIVERY.md').write_text(text,encoding='utf-8')
print(json.dumps({'report':str(OUT/'DELIVERY.md'),'apkSha256':apk['sha256'],'buildSourceSha':apk['buildSourceSha'],'tests':{n:t['passed'] for n,t in tests.items()}}))
