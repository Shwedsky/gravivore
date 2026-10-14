"""Record the observed APK55 regression before changing any runtime asset."""
import hashlib,json,shutil,subprocess,xml.etree.ElementTree as ET
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'docs/history/implementation-passes/energy-chamber-runtime-presentation-v1'
inventory=json.loads((OUT/'verification/diagnosis/renderer-inventory.json').read_text())
views=json.loads((OUT/'verification/diagnosis/camera-sweep.json').read_text())['items']
assert ET.parse(OUT/'verification/DiagnosisCamera.xml').getroot().get('result')=='Passed'
references=OUT/'references';references.mkdir(parents=True,exist_ok=True)
ref_receipts=[]
for source in (ROOT/'Builds/Android').iterdir():
    if source.suffix.lower() not in ('.jpg','.png'):continue
    target=references/source.name;shutil.copyfile(source,target)
    ref_receipts.append(dict(source=str(source),file=str(target.relative_to(ROOT)),sha256=hashlib.sha256(target.read_bytes()).hexdigest(),authority='Human supplied APK55 phone screenshot' if source.suffix=='.jpg' else 'Human approved Energy Chamber concept'))
(OUT/'verification/reference-custody.json').write_text(json.dumps(ref_receipts,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
baseline={p.relative_to(ROOT).as_posix():hashlib.sha256(p.read_bytes()).hexdigest()for prefix in ['Assets/_Game/Content/Chapter01Architecture/EnergyChamber/Models','art/energy-chamber-hybrid-proof-v1','art/energy-chamber-visual-polish-v1','art/energy-chamber-vertical-nodes-v1']for p in (ROOT/prefix).rglob('*')if p.is_file()}
for p in [ROOT/'Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity',ROOT/'Assets/_Game/Content/BlueprintWorldR1/Chapter01WorldLayout.asset']:
    baseline[p.relative_to(ROOT).as_posix()]=hashlib.sha256(p.read_bytes()).hexdigest()
(OUT/'verification/architecture-freeze.json').write_text(json.dumps(dict(baselineHead=subprocess.check_output(['git','rev-parse','HEAD'],cwd=ROOT).decode().strip(),hashes=baseline),indent=2)+'\n')
report=['# Checkpoint A — runtime visibility diagnosis (before correction)','',
'Human authority: the four supplied APK55 phone screenshots and Energy Chamber concept in `references/`. The architecture/modeling is accepted and frozen. This diagnosis uses equivalent player positions because the JPEGs contain no world-coordinate metadata. No runtime art, LOD membership, materials, lighting, scene, route, collision or gameplay code has been changed at this checkpoint.','',
'## Observed cause','',
'The root production LODGroup drops complete secondary assemblies at thresholds **.38 / .30 / .08**. Android defaults to **Medium, LOD bias .7**; the previous editor captures ran at editor quality Fantastic, bias **2**. Unity scales projected size by this bias. Nearby south/west views therefore select LOD1, and the wider approach selects LOD2. The same view rendered with diagnostic-only ForceLOD(0) immediately restores the absent parts. ForceLOD is reset to automatic after this comparison and is not a production fix.','',
'All 45 Renderers remain enabled/active, with finite nonzero bounds; each absent component in the reproduction is still inside the camera frustum. GPU camera visibility agrees with the selected membership. There are **zero nested LODGroups**. All materials are opaque, queue 2000, with no transparent sorting. There is no runtime Renderer toggle or animation on the station. Fixed-yaw camera movement changes distance to the LOD reference point; this is distance/projected-size selection, not a camera-angle or bad-bounds defect. A component outside the frame in a west view remains ordinary frustum clipping.','',
'## Reproduction','',
'The real Chapter scene is loaded through its normal composition. Its live G-0 and actual initialized PortraitFollowCamera are used; SnapToTarget preserves the real offset, look-at, FOV 48 and portrait aspect 9:16. Only combat/population updates are paused in the capture fixture. Android quality is explicitly selected for the test.','']
for view in views[:5]:
    report.append(f"- {view['name']}: reference distance {view['distance']:.2f} m, bias-adjusted height {view['relativeHeight']:.3f}, LOD{view['lod']}; {sum(i['cameraVisible'] for i in view['items'])} camera-visible Renderers; {sum(i['inFrustum'] and not i['member'] for i in view['items'])} in-frustum parts excluded by LOD.")
report+=['','## Every affected assembly / Renderer','',
'Each bullet below records the exact hierarchy path, baseline membership, observed root cause and proposed correction. All are distance/projected-size based; no bounds repair is justified.','']
for item in inventory['items']:
    if item['lods']==[0,1,2]:continue
    mesh=Path(item['mesh']).name
    if mesh=='EC_FoundationPolish.fbx':fix='keep complete floor interfaces through nearby LOD0; drop this finer plinth assembly only at range'
    elif mesh=='EC_ConnectorPolish.fbx':fix='keep connector interfaces through nearby LOD0; allow fine conduit extras to drop at range'
    elif mesh in ('VR3_generator.fbx','VR3_centrifuge.fbx','VR3_proppipeholder.fbx','EC_NodeVariant.fbx'):fix='retain the side-machine/support assembly in LOD1; remove only at long range'
    else:fix='retain this identity/silhouette-critical assembly in LOD1 and the far silhouette level'
    report.append(f"- `{item['name']}` — {item['triangles']:,} triangles; LOD {', '.join(map(str,item['lods']))}. Root cause: excluded from the next level, with early bias-adjusted transition. Proposed: {fix}.")
report+=['','## Checkpoint B proposal','',
'Rebalance local thresholds to **.26 / .13 / .035**, keeping automatic LOD selection and the existing bounds. LOD1 retains the guard rails, full top/cap assembly, node outer detail, asymmetric side details and all major machinery/support. LOD2 retains guard silhouette, stairs, chamber top/cap and the node outer frames. Fine plinth/conduit dressing may disappear at range. At Android default bias .7, estimated reference distances become 34.7 / 69.4 / 257.8 m versus 23.7 / 30.1 / 112.8 m before. Camera far clipping still applies. Actual captured membership will be verified after correction. No global quality override is proposed.','',
'## Executed evidence','',
'- Unity inspection/compile exit 0 (`DiagnosisInventory.log`, local ignored log).','- Real Chapter camera PlayMode sweep: 1 passed, 0 failed (`verification/DiagnosisCamera.xml`).',
'- Machine-readable per-Renderer bounds/material/LOD inventory and actual camera visibility receipts under `verification/diagnosis/`.','- Eight normal captures plus two diagnostic LOD0 controls under `screenshots/diagnosis/`.','- Original FBX/source/scene/layout SHA-256 freeze manifest stored before the fix.','- Phone and concept copies carry SHA-256/source receipts; original files in Builds/Android are untouched.','',
'This checkpoint is diagnosis only. APK56 and human acceptance remain pending.']
(OUT/'CHECKPOINT_A_DIAGNOSIS.md').write_text('\n'.join(report)+'\n',encoding='utf-8')
print('Recorded diagnosis, accepted geometry freeze and',len(ref_receipts),'human references.')
