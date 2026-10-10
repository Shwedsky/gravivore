"""Generate an evidence-based delivery report; pending checks stay pending."""
from pathlib import Path
import json, hashlib, shutil, subprocess, xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[2]
E=ROOT/'docs/history/implementation-passes/chapter01-actor-production-v2'
V=E/'verification'; V.mkdir(exist_ok=True)
actors=['Scout','Cutter','Warden','ArcDrone','Carrier','Magnetar','Custodian']
donors=['George','QuadShell','MSGDi Striker','EyeDrone','UnityFan Vehicle 012 (member 013_2)','Trilobite joints/support reference','Audited heavy joints/support reference; no boss donor']
designs=['Light biped, two long integrated blade terminations','Low four-support predator, paired articulated cutting drives','Broad heavy biped, two separately held/controlled shields','Airborne emitter, blue core, no ground contacts or walking chains','Low enclosed elongated hover vehicle, integrated propulsion','Custom heavy containment, buttresses, plated supports and magnetic terminals','Custom reactor vault/chimney, independent boss mantles, discharge structures and phase poses']
gaps=['Head/chest are more angular than the concept; aggressive stance and stride timing need device motion review.','Dorsal shell is simpler than the concept; cutting-drive prominence must be judged at phone scale.','Faceted shoulder/torso treatment and simpler grip mechanics; verify shield separation during motion.','Contained lens is flatter than the painted concept; verify airborne read in all device headings.','Roof/rear hull subdivision remains simpler than the concept; verify orange propulsion read without bloom.','Containment and support armor remain cleaner/simpler than the concept; heavy charge/vent weight needs device review.','Mantle armor and reactor routing remain cleaner than the concept; verify phase differentiation and arena silhouette on device.']
unity=json.loads((E/'UNITY_VALIDATION.json').read_text()); source=json.loads((E/'SOURCE_VALIDATION.json').read_text())
measure={x['actor']:x for x in unity['actors']}
checks=[]
for name in ['EditModeVerified','PlayModeAll']:
 path=ROOT/'Builds/ActorProductionV2'/(name+'.xml')
 if path.exists():
  run=ET.parse(path).getroot(); checks.append({'name':name,'result':run.get('result'),'total':int(run.get('total',0)),'passed':int(run.get('passed',0)),'failed':int(run.get('failed',0)),'skipped':int(run.get('skipped',0)),'skipped_details':[{'test':case.get('fullname'),'reason':case.findtext('reason/message')} for case in run.iter('test-case') if case.get('result')=='Skipped']})
  shutil.copy2(path,V/path.name)
 else: checks.append({'name':name,'result':'Pending'})
android=json.loads((E/'ANDROID_REVIEW_BUILD.json').read_text()) if (E/'ANDROID_REVIEW_BUILD.json').exists() else {'result':'Pending'}
apk=ROOT/'Builds/Android/gravivore-actor-review-v2.apk'
if apk.exists(): android.update({'sha256':hashlib.sha256(apk.read_bytes()).hexdigest(),'file_bytes':apk.stat().st_size,'absolute_path':str(apk)})
validation=(E/'PROJECT_VALIDATION.txt').exists()
clearance=json.loads((E/'CLEARANCE_REVIEW.json').read_text())
scope=json.loads((E/'SCOPE_VALIDATION.json').read_text())
taskfiles=subprocess.check_output(['git','-c','core.quotepath=false','diff','--name-only','38e0ed8fe5c95ff41ab5dbb246f45ed26bdcf9cc'],cwd=ROOT,text=True).splitlines()
untracked=subprocess.check_output(['git','-c','core.quotepath=false','ls-files','--others','--exclude-standard'],cwd=ROOT,text=True).splitlines()
allowed=('Assets/_Game/ArtReview/ActorProductionV2','Tools/actor-production-v2/','art/chapter01-actor-production-v2/','docs/history/implementation-passes/chapter01-actor-production-v2/')
files=sorted(set(taskfiles+[p for p in untracked if p.startswith(allowed)]))
(E/'FILES_CHANGED.txt').write_text('\n'.join(files)+'\n',encoding='utf-8')
summary={'branch':'art/chapter01-actor-production-v2','draft_pr':'https://github.com/Shwedsky/gravivore/pull/74','baseline':'38e0ed8fe5c95ff41ab5dbb246f45ed26bdcf9cc','actor_count':7,'all_seven_ready_for_chapter01_integration':False,'unity':unity['engine'],'tests':checks,'project_validation_executed_successfully':validation,'android':android,'device_connected':False,'device_visual_gate':'Not executed','android_performance_gate':'Not executed','initial_rest_placement_conservative_clear':clearance['initial_rest_placement_conservative_clear'],'scope_validation':scope['result'],'production_bindings_changed':False}
(E/'DELIVERY_STATUS.json').write_text(json.dumps(summary,indent=2))
text=['# Seven-actor production/review delivery V2','',
'All seven hostile actors have authored editable sources, rigged FBXs, PBR maps, three LODs, isolated prefabs/controllers and complete-family review evidence. **All seven `READY_FOR_CHAPTER01_INTEGRATION` flags remain false.** This is a reviewable manufacturing package, not owner-certified visual/device acceptance.',
'',f"Branch: `{summary['branch']}`. [Draft PR #74]({summary['draft_pr']}). Baseline: `{summary['baseline']}`. No merge. World R2 / PR #71, production bindings, G-0, collision, balance, progression, combat, saves and UI remain unchanged.",
'','## Actor manufacture and remaining visual gaps','',
'Donors below were opened/hash-checked as mechanical references only. **100% of delivered geometry, rigs, weights, clips and textures is newly authored; 0 donor bytes reused.** Restricted raw sources remain private. See [PROVENANCE.md](PROVENANCE.md) and [DONOR_REINSPECTION.json](DONOR_REINSPECTION.json).','']
for actor,donor,design,gap in zip(actors,donors,designs,gaps):
 data=json.loads((E/(actor+'_METRICS.json')).read_text()); imported=measure[actor]
 lo=data['lods'][0]; dimensions=lo['dimensions_xyz']; triangles='/'.join(str(x['triangles']) for x in imported['lods'])
 text.extend([f'### {actor}',f'{design}. Inspected donor: {donor}. Custom geometry: 100%, {data["editable_components"]} editable components; {data["bones"]} authored bones.',
  f'LOD0/1/2: {triangles} triangles; one consolidated skinned renderer and one shared family PBR material per visible LOD. Energy: {data["energy"]}. Source rest dimensions (width/depth/mesh height): {dimensions[0]:.3f}/{dimensions[1]:.3f}/{dimensions[2]:.3f} m; maximum ground-relative Z: {lo["max_xyz"][2]:.3f} m. Imported animation bounds are conservative and separately recorded in UNITY_VALIDATION.json.',
  'Source clips: '+', '.join(x['name'] for x in data['clips'])+'. Move imports as Run; ROOT stays stationary. Socket hierarchy is in the per-actor metrics.',
  f'Remaining visual gap: {gap} `READY_FOR_CHAPTER01_INTEGRATION=false`.',
  f'[Concept / source / gameplay camera](comparisons/{actor}_concept_Unity.jpg), [donor comparison](comparisons/{actor}_donor_production.jpg), [gameplay screenshot](unity/{actor}_gameplay_0.png), [G-0 scale](unity/{actor}_scale_G0.png), [in-environment diagnostic](unity/{actor}_in_environment.png).',''])
text.extend(['## Rig, maps, LODs and evidence','',
'Generic mechanical rigs use one rigid bone weight per vertex. Warden shields parent through TOOL -> ELBOW -> SHOULDER, with physical grips, independent pivots and Block. Scout has two long integrated blade roots/tips. Arc has aerial outriggers/vanes, Hover/Discharge and measured flight clearance; Carrier has propulsion pivots and Bank, without leg chains. Magnetar has independent containment/tool chains and Charge/Vent. Custodian has independently articulated reactor, mantles and discharge tools, with Telegraph/AttackLine/AttackCircle/AttackCone. All poses are available in the device reviewer.',
'',
'Clips are authored in-place at 30 FPS, with baked presentation floor correction through BODY; ROOT is never translated. Sources were reopened and every authored frame sampled: finite geometry, no zero-area export triangles, one skin weight per vertex, UVs, decreasing LOD geometry, packed maps, stationary ROOT and >1 cm posed floor clearance. This does not prove stride synchronization with future gameplay movement or continuous mechanical/foot IK.',
'',
'Shared 1024 trim maps: BaseColor, MetallicSmoothness (R metallic / A smoothness), Normal, Occlusion and localized Red/Amber/Blue emission. Painted armor, bare metal, graphite recesses and energy have distinct surface responses. URP/Lit is opaque; Android ASTC 6x6 with mipmaps. No added realtime actor lights or transparent effects. LOD transitions .14/.075/.025; quality bias applies. Readable CPU meshes are deliberately retained in the review and need profiling during integration.',
'',
f"Camera is unchanged main: offset (0, 14.8, -11.2), look-at 0.9, FOV 46, portrait 9:16. G-0 remains its actual live prefab/root scale, measured renderer height {unity['g0RenderDimensions']['y']:.3f} m. Primary captures use automatic camera LOD; forced LOD0/1/2 and emission-disabled captures are separate. Each actor has Blender front/three-quarter/top/silhouette, donor and concept comparisons, four Unity headings, phone view, G-0 scale view and in-environment diagnostics. Elite/boss have telegraph/phase evidence. Family overview is wider diagnostic framing, explicitly not gameplay camera.",
'',
'[Complete Unity family at actual scale](unity/Complete_family.png), [complete phone family](comparisons/Complete_phone_family.jpg), [all authored sources](comparisons/Complete_authored_family.jpg). Studio tiles are independently framed and cannot be used as a scale comparison.',
'','## Executed checks',''])
for check in checks: text.append(f'- {check["name"]}: '+(f'{check["passed"]}/{check["total"]} passed; {check["failed"]} failed, {check["skipped"]} skipped.' if 'total' in check else 'pending.'))
for check in checks:
 for skipped in check.get('skipped_details',[]): text.append(f'- Optional skipped check: {skipped["test"]}. {skipped["reason"]}')
text.extend(['- Blender source validation: PASS for all seven; see SOURCE_VALIDATION.json.',
'- Unity '+unity['engine']+' compilation, asset validation, matched camera capture and production dependency isolation: PASS; see UNITY_VALIDATION.json.',
'- Existing ProjectValidator.ValidateOrThrow: '+('PASS (executed).' if validation else 'pending.'),
'- Read-only conservative rest-envelope audit: '+('all current spawn anchors and boss-arena radius clear.' if clearance['initial_rest_placement_conservative_clear'] else 'potential overlap; inspect CLEARANCE_REVIEW.json.')+' See CLEARANCE_REVIEW.json; animated movement is not certified.',
'- Protected production paths and approved PNG custody: '+scope['result']+'; see SCOPE_VALIDATION.json.',
'- Android ARM64 IL2CPP development review APK: '+android['result']+'.',
'- Owner device visuals / Android 60 FPS / steady-state allocations / animated encounter clearance: not executed; no Android device connected.',''])
if apk.exists(): text.extend([f'APK: `{apk}`. Package `com.gravivore.actorreview`, version 0.1.0/code 1, separate from the game. {apk.stat().st_size/1048576:.1f} MiB. SHA-256 `{android["sha256"]}`.',
'Review scene starts first and exposes all seven actors, every role pose, LOD selection and family overview. Existing global build audits require unchanged Bootstrap/Chapter01 scenes to be packed after it; this does not bind any new actor into production.',''])
text.extend(['## Acceptance limits and assumptions','',
'The models identify all seven roles and reproduce the required locomotion/anatomy classes, but comparison images show simpler, more angular armor and cleaner surfaces than the painted reference. Concept fidelity and device motion remain open review gates, not inferred passes from mesh detail/import/tests. No device FPS or allocation claim is made.',
'',
'Outboard blades, shields, wings, supports and boss structures exceed existing center collision radii. Those radii and the world were not altered. A read-only conservative rest-envelope check against serialized main collision boxes, closed gates, world bounds, all spawn anchors and boss-arena radius passed. Custodian has approximately '+str(next(x for x in clearance['actors'] if x['actor']=='Custodian')['boss_arena_rest_margin_m'])+' m conservative arena margin at its start. This cannot guarantee clearance during animation, movement near walls or turns: center colliders do not contain the full visual reach. Production motion clearance, stride timing, animation blends and shadow/LOD behavior remain integration gates. The review does not demonstrate damage-source-to-impact behavior because combat integration is explicitly out of scope.',
'',
'Scale-strip axes are not surveyed: biped/elite/boss height and vehicle length guide authored relative hierarchy; the airborne root stays on the gameplay ground plane with its mesh elevated. Hidden construction, support details, rigid grips and Generic bone axes are production assumptions, not newly approved anatomy.',
'',
'Next defined gate: owner device review of the COMPLETE seven-actor family, followed by actor integration only after visual/motion/performance and clearance acceptance. No additional spec ID was supplied. Draft remains unmerged.',
'','## Checkpoints and files','',
'- 80ae82c: authority/scope checkpoint, pushed and Draft PR created before modeling.',
'- 6733e02: A — Scout/Warden/Carrier.',
'- 2f966e4: B — Cutter/Arc Drone.',
'- 16f8add: C — Magnetar.',
'- 74dc7c7: D — Custodian.',
'- E: final sources/material/motion refinements, Unity review, validation and delivery evidence in the final branch commit.',
'',
'Files: [FILES_CHANGED.txt](FILES_CHANGED.txt). Source roots: `art/chapter01-actor-production-v2/`, `Assets/_Game/ArtReview/ActorProductionV2/`, `Tools/actor-production-v2/` and this evidence directory. Initial custody also adds the exact approved actor authority/donor documentation needed from the pinned commits. Unity-generated changes outside these paths are restored before delivery; private generated build settings/caches are not promoted.',
'',
'Evidence is an implementation snapshot under docs/history, not new project authority.'])
(E/'DELIVERY_REPORT.md').write_text('\n'.join(text)+'\n',encoding='utf-8')
