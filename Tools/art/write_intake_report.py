"""Compose review records from measured source data and explicit art decisions."""
import json
import hashlib
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT/'docs/visual-production-v2/modeling'
DATA = OUT/'data'
archives = json.loads((DATA/'archive_inventory.json').read_text())
candidates = json.loads((DATA/'candidates_audit.json').read_text())
environment = json.loads((DATA/'environment_audit.json').read_text())
materials = json.loads((DATA/'material_prep.json').read_text())

PROVENANCE = {
'robot-spider': ('Robot spider','PAndras','https://sketchfab.com/3d-models/robot-spider-c9c7188c7f9e4504b8499f1131693b72'),
'spider-robot': ('Spider Robot','Muhammad Hasan Alasady','https://sketchfab.com/3d-models/spider-robot-7ea58c2e0e7e48f281d7a840f02aa7b1'),
'spider-robot-rigged': ('Spider Robot (Rigged)','Keralt','https://sketchfab.com/3d-models/spider-robot-rigged-419632470d3b4328b8d8ea7ae0462ce7'),
'scorpion-mech': ('Scorpion mech','gwim','https://sketchfab.com/3d-models/scorpion-mech-db3225ec5dcf431d8b3b8dc328a42bbb'),
'scorpion-robot': ('Scorpion Robot','Mirandanimator','https://sketchfab.com/3d-models/scorpion-robot-e44be9a622af4e85b4b10cf59b25de1a'),
'gunslinger-crab-mech-v12': ('Gunslinger Crab Mech v1.2','Vaportrash','https://sketchfab.com/3d-models/gunslinger-crab-mech-v12-5147619f337a45b0974f349eac27b34a'),
'mechanical-spider': ('Mechanical Spider','Preview_Tempest','https://sketchfab.com/3d-models/mechanical-spider-d1f67d2e995d4c81a056242df1b97390'),
'stalenhag-environment-project-spider-mech': ('Stalenhag Environment Project: Spider Mech','Enrico Labarile','https://sketchfab.com/3d-models/stalenhag-environment-project-spider-mech-ec5914b53b6a4cde8de4820050bc46c5')}

# Each grade concerns the actual source, with donor usefulness separate from body fit.
GRADES = {
'robot-spider': ('B','B','Reject','Reject','B','C','C','B','Six-legged gun drone has useful tapered hinges/feet; no rig or clips. Body and cannon must be replaced. Many boundary edges reflect split glTF vertices, not automatic holes.'),
'spider-robot': ('B','C','B','B','C','A','C','B','Preferred Scout donor. Low articulated four-support anatomy and actual Idle/Walk/Fire/BigBoom actions. Base 1,962 triangles becomes 7,848 through subdivision. Repair 14 degenerate faces, four non-manifold edges and palette reference. Fire and explosion are reference-only after weapon redesign.'),
'spider-robot-rigged': ('Reject','C','B','Reject','C','A','Reject','C','Tall literal box body fails GRAVIVORE. 39-bone rig but no actions; 150 non-manifold edges and six material slots. Reject as preferred donor.'),
'scorpion-mech': ('B','Reject','Reject','Reject','C','C','C','C','Reject as preferred chassis: one fused Crawler mesh, 2,040 zero-area faces and 74 non-manifold edges, missing referenced textures, no rig. Forward gun/tail identity needs replacement.'),
'scorpion-robot': ('B','B','Reject','Reject','B','C','C','B','Preferred Cutter chassis donor. Eight named rigid mesh objects, UVW and one Mat material, no degenerate/non-manifold faces in audit. Delete raised tail and replace front claws/core/shell. Requires component separation and a new rig; raw 29,274 triangles exceed ordinary-enemy targets.'),
'gunslinger-crab-mech-v12': ('B','B','Reject','Reject','C','Reject','C','B','Mechanics only: crab_legs (15,612 triangles) has robust pivots/feet. Do not reuse full body, launcher, gun arms, decals or stock identity. 4K source maps and ten materials are unsuitable as delivered. Split glTF vertices require welding before topology cleanup.'),
'mechanical-spider': ('B','C','Reject','Reject','C','Reject','C','B','Mechanics only: Cylinder001 representative leg/joint chain. Each repeated chain is 10,277 triangles and contains 44 degenerate faces and three non-manifold edges. Rebuild/reduce around hinges; no delivered rig despite articulated appearance.'),
'stalenhag-environment-project-spider-mech': ('C','B','Reject','Reject','B','Reject','Reject','Reject','Reject production reuse. Creator explicitly credits Simon Stalenhag designs; uploader CC BY record does not establish rights to the underlying design. Static COLLADA XML import preserves geometry/scene transforms only; source has four UV inputs, no controllers/animation, but this inspection adapter does not transfer UVs/materials.')}

def write(name, body):
    (OUT/name).write_text(body.rstrip()+'\n', encoding='utf-8')

texture_records = []
for archive in archives:
    folder = ROOT/archive['extracted_relative']
    record = dict(archive=archive['filename'], textures=[])
    for p in folder.rglob('*'):
        if p.suffix.lower() in ('.png','.jpg','.jpeg'):
            with Image.open(p) as img:
                record['textures'].append(dict(path=str(p.relative_to(folder)).replace('\\','/'),size=list(img.size),mode=img.mode))
    texture_records.append(record)
(DATA/'physical_texture_inventory.json').write_text(json.dumps(texture_records,indent=2),encoding='utf-8')

intro = '''# Actual source inspection results

Inspection: 2026-10-06. Blender 5.2.2 LTS (`d13f752e3b9c`). All thirteen ZIPs passed CRC integrity checks and were SHA256 hashed before safe extraction into ignored `.asset-intake-tmp/`. Originals remain in the user's intake directory. Two nested ZIPs and one RAR were also extracted and hashed. No source archive or donor geometry is committed.

Reference commits: PR #53 `478cd3e7cf0f28ffccd227a0ce9c60273f567455`; PR #54 `bfe989529a8e4057ceaade967fa45d7a8edfd6fb`; PR #55 `135a9acd9c699a79200adde19cfbe0edc76e95c7`.

Evidence: `data/archive_inventory.json` records every archive member and nested hash; `data/candidates_audit.json` and `data/environment_audit.json` record every imported object, parent, UV layer, material slot, modifier, rig, action and mesh topology. `data/physical_texture_inventory.json` records actual supplied bitmap sizes. `source-review/*contact_sheet.png` are neutral renders of these actual sources, not marketplace thumbnails.

Eight character sources and all 189 distinct non-Unity FBX modules were opened. The Standard ZIP contains 378 FBX copies, 190 glTF/BIN pairs and 191 OBJ/MTL files, not the complete 277-model paid/source pack advertised online. Equivalent format copies are not added together as unique models. The native `wm.fbx_import` handles both binary and ASCII FBX; legacy Python FBX import rejects Mirandanimator's ASCII file. At least one glTF refers to nonexistent `Decal_Line_90_001.bin`; several relative textures also fail. Use inspected FBX geometry and explicit texture relinking for any later handoff.

Boundary counts alone do not prove holes: exported glTF meshes split vertices at UV/normal seams. Degenerate faces and edges with more than two attached faces are reported separately. Audits use source geometry and evaluated modifiers at frame zero. Neutral renders expose shape, not final material quality. The COLLADA-only model uses a documented static XML adapter because native COLLADA import is unavailable; source UV/controller counts are reported separately from converted data. No rig/UV conversion is claimed for it.

Sketchfab provenance/CC Attribution follows PR #55's exact title/author/URL matching; seven live page retries returned HTTP 403, so this pass does not claim new license-page verification for those. Stalenhag's accessible page reconfirmed CC Attribution and the underlying-design credit; it is excluded. Quaternius CC0 and Poly Haven CC0 were independently reconfirmed on official pages. No licenses were bundled in the character ZIPs. Future extracted parts must carry exact object/component identity plus archive hash and attribution. Earlier PR #54's assertion that CC BY inherently prohibits unmodified redistribution is incorrect; avoiding raw archive commits is project policy.
'''
parts = [intro]
for archive in archives:
    parts += [f"\n## {archive['filename']}\n\n- Category: {archive['category']}. Size: {archive['bytes']:,} bytes.\n- SHA256: `{archive['sha256']}`.\n- Contained formats: {', '.join(f'{k or "extensionless"}: {v}' for k,v in archive['formats'].items())}.\n"]
    key = archive['filename'].removesuffix('.zip')
    matches = [r for r in candidates if r['id']==key]
    if matches:
        r = matches[0]
        title, author, url = PROVENANCE[key]
        grades = GRADES[key]
        parts += [f"- Provenance: [{title}]({url}) by {author}; CC Attribution per #55.\n- Meshes: {len(r['meshes'])}; base triangles: {r['triangles']:,}; evaluated triangles: {r['evaluated_triangles']:,}.\n- Rig: {', '.join(a['name']+' / '+str(a['bones'])+' bones' for a in r['armatures']) or 'none supplied'}. Actions: {', '.join(a['name'] for a in r['actions']) or 'none supplied'}.\n- Materials: {', '.join(r['materials']) or 'not converted; see COLLADA limit'}. UV: {', '.join(sorted({uv for m in r['meshes'] for uv in m['uv_layers']})) or 'not converted; see source UV inputs'}.\n- Grades (silhouette / topology / rig / animation / materials / mobile / GRAVIVORE fit / donor): {' / '.join(grades[:8])}.\n- Review: {grades[8]}\n"]
    elif archive['category']=='environment':
        parts += [f"- Quaternius CC0. Inspected 189 unique FBX modules: {sum(r['triangles'] for r in environment):,} aggregate triangles (not a runtime scene budget). Per-module UV/material/hierarchy and rigs/animations are in the audit. The pack contains animated organic aliens, a chest and a fan; these do not provide character locomotion.\n- Silhouette B as structural donors, topology B with open modular surfaces, rig/animation Reject for this static subset, material structure B after relinking, mobile A for selected compact modules, GRAVIVORE fit C as a complete pack, donor value B. Only the explicit compact subset advances.\n"]
    else:
        m = next(m for m in materials if key.startswith(m['name']+'_'))
        parts += [f"- Poly Haven CC0; actual material-only Blender library opened and shader nodes recorded. No mesh/rig/animation hierarchy in these material libraries.\n- Prepared channels: {', '.join(i['channel'] for i in m['maps'])}, all 2048×2048. Displacement discarded. No AO map supplied. Metallic absent in blue/grid sets; do not invent it from diffuse.\n- Grade B as reusable surface sources; final color/tiling calibration and Android compression remain later work.\n"]
    tr = next(t for t in texture_records if t['archive']==archive['filename'])
    parts += [f"- Supplied external bitmaps: {len(tr['textures'])}; sizes: {', '.join(str(s) for s in sorted({tuple(t['size']) for t in tr['textures']})) or 'none'}. Packed/embedded maps are additionally recorded in Blender audit.\n"]
write('SOURCE_INSPECTION_RESULTS.md',''.join(parts))

write('SCOUT_FINAL_DONOR_DECISION.md', '''# Scout — final donor decision

**ONE preferred donor: Spider Robot — Muhammad Hasan Alasady. Grade B donor.**

Use `spider-robot.zip`, SHA256 in the source register, `source/spider.blend`, mesh `Body`, armature `Spider` (33 bones). The four-support chain and source Idle/Walk are the useful foundation. Real source action samples are saved as `Scout_animation_*.png` and their bone matrices in `data/scout_animation_samples.json`. This is a source-motion sample, not validation after redesign.

1,962 base triangles become 7,848 evaluated with subdivision. Preserve the economy of the base cage; do not accidentally export uncontrolled subdivisions. UVMap exists. Four non-manifold edges and 14 degenerate faces need repair. The material references missing `ImphenziaPalette01.png`; supplied `textures/Colors_Map.png` is present but not assumed equivalent. Replace the palette with the shared hostile material family.

Custom work remains: replace top shell, forward weapon and core housing; remove broad blade/gun identity; preserve fast radial stance and make compact red core visible. Idle and Walk are B useful; Fire is a timing reference, BigBoom is a destruction reference, `_Default` is not a useful clip. Rebind and validate all clips after redesign. Final target follows #53: 6–14k triangles, two to three materials, <=4 renderers where articulation permits.

PAndras's Robot spider: B parts donor but no rig/clips; six-legged gun silhouette and 18,754 triangles lose to the animated economy donor. Keralt: C donor / Reject body; upright box body, no clips, 150 non-manifold edges and six material slots. Neither is an additional preferred Scout donor. No Scout production model is completed in this task.
''')
write('CUTTER_FINAL_DONOR_DECISION.md', '''# Cutter — final chassis donor decision

**ONE preferred chassis donor: Scorpion Robot — Mirandanimator. Grade B donor.**

Use `scorpion-robot.zip`, exact hash in source results, `source/scorpion.fbx` (ASCII FBX, imported successfully with Blender's native importer). Eight rigid objects `rdmobj07`, `rdmobj03`, `rdmobj04`, `rdmobj01`, `rdmobj02`, `rdmobj00_006`, `rdmobj06`, `rdmobj05`; source `UVW` and one `Mat` material. 29,274 triangles. Audit found no non-manifold edges beyond boundaries and no zero-area faces; open edges remain expected in separate hard-surface parts.

This is a chassis decision only. Remove the raised tail and recognizable stock claws/front forms, retain useful low mechanical support anatomy, separate connected limb islands, build a new mechanical rig, custom paired long cutting blades, core housing and top shell. There is no delivered armature or animation. Raw source exceeds #53 ordinary-enemy guardrails; reduce and consolidate toward 6–14k, soft max 18k pending profiling. No donor texture set is delivered.

gwim's Scorpion mech is rejected as preferred chassis despite promising stance: one fused `Crawler` mesh, 23,808 triangles, 2,040 zero-area faces, 74 non-manifold edges, absent rig/clips and ten missing referenced maps. Salvaging it is a larger first-slice risk. No Cutter production mesh is completed here.
''')
write('MAGNETAR_COMPONENT_POOL.md', '''# Magnetar — restricted mechanics pool

**Custom elite torso, armor and containment/core. No full donor body selected.**

- Vaportrash Gunslinger Crab Mech v1.2: `crab_legs`, 15,612 triangles, CC Attribution, archive/hash in source results. B mechanics donor for low-identity bearing housings, elbow linkages and terminal foot segments. Source has all legs merged into one mesh and split UV/normal vertices; isolate component islands and weld only compatible seams before cleanup. Do not approve the entire leg cluster for runtime. Reject `crab_main`, launcher, gun arms, label meshes and decals. No rig/clips. 4K source maps and ten materials require fresh reduced atlas; nothing is shipped unchanged.
- Preview_Tempest Mechanical Spider: `Cylinder001` as the representative repeated leg chain; 10,277 triangles, CC Attribution. B mechanism reference / C raw topology. Restrict reuse to generic joint collars and actuator-link sections after isolation/rebuild. Each chain contains 44 zero-area faces and three non-manifold edges; clean and retopologize. Related `Cylinder002/003/004/006/007/008` and misleadingly named mesh `PhysCamera001` are repeated chains, not eight distinct new donor designs. Reject `Box002` torso and complete spider silhouette. No armature/clips.
- Enrico Labarile Stalenhag Spider Mech: **Reject production reuse**, including signature feet/body/claws. CC Attribution is listed, but the creator explicitly bases the design on Simon Stalenhag. The existing sourcing record does not establish permission for that underlying design. Keep inspection evidence only; no parts enter the production pool.

No separate piston object is delivered by these candidates. Do not claim a finished isolated piston asset. Generic pistons/hinges may be custom-modeled more cheaply than extracting expensive merged geometry. Final Magnetar rig, body and animations remain custom; components are donor selections for later controlled extraction, not finished mobile parts.
''')
write('G0_COMPONENT_POOL.md', '''# G-0 component policy

**CUSTOM-FIRST. Blockout V1 uses zero third-party parts.**

Custom torso, deep core cavity, main shell, four supports, forward mandible/weapon forms and rear machinery define identity. The eight inspected full bodies are rejected for G-0 reuse.

Potential low-identity component pool for later production: Vaportrash `crab_legs` pivot/foot fragments and Preview_Tempest `Cylinder001` hinge/actuator fragments, subject to the cleanup and exact provenance restrictions in `MAGNETAR_COMPONENT_POOL.md`. These are B donor ideas; no extracted component is a production-ready file. Stalenhag components are excluded. Scout donors are not transplanted into G-0.

No donor geometry, rig, textures or animations enter the authored blockout. Custom mechanics are kept coarse to support proportion/stance review; adding donor greebles before approval would obscure the silhouette gate.
''')

selections = {
'floors': ['Platform_Metal2','Platform_Window_Thin'],
'walls': ['WallAstra_Straight','WallAstra_Straight_Divided'],
'corners': ['WallAstra_Corner_Square_Inner','WallAstra_Corner_Square_Outer'],
'barriers': ['Prop_Rail_4','Prop_Rail_Round_Small'],
'columns': ['Column_Hollow','Column_Large_Straight'],
'pipes': ['Prop_PipeHolder'],
'tanks': ['Prop_Barrel_Large'],
'machinery': ['Prop_Fan_Small','Prop_Vent_Big','Prop_AccessPoint'],
'gate_structure': ['Column_MetalSupport','Column_Hollow']}
subset = []
text = '''# Compact first-slice environment donor subset

Quaternius Standard, CC0. Inspect all 189 actual FBX models; approve only the following sixteen unique structural/component donors. Grade B donor subset; no whole-pack approval or final-art approval. Normals, trim texture relinking, meter scale/grid and pivots still need preparation for eventual Unity handoff.

The inspected free pack is much simpler than the approved concept machinery. Flat trim-driven panels are topology donors, not finished layered floors/walls. Add custom seam/edge/medium forms where needed. Neutral clay evidence makes this limitation visible.

All 189 modules have separate silhouette/topology/rig/animation/material/mobile/fit/donor grades in `data/environment_grades.json`. A B donor grade does not approve a weak stock piece as finished scenery.
'''
for role,names in selections.items():
    text += '\n## '+role.replace('_',' ').title()+'\n\n'
    for name in names:
        r = next(r for r in environment if Path(r['path']).stem==name)
        entry=dict(role=role, module=name, path=r['path'], triangles=r['triangles'], materials=len(r['materials']),
                   grade='B', approval='donor structure/component only')
        subset.append(entry)
        text+=f"- `{name}` — {r['triangles']:,} triangles, {len(r['materials'])} material(s); {', '.join(r['meshes'][0]['uv_layers']) or 'no UV'}. Exact source: `{r['path']}`.\n"
text+='''
## Category limits and rejects

- Floors: Metal2 is an economical flat foundation; Window_Thin is an inset/trench frame requiring an opaque custom infill, not walkable glass. Do not approve featureless single plates as hero final surfaces.
- Tanks: Barrel_Large is only a small vessel/collar donor. **Full industrial tank CUSTOM REQUIRED**; no hero pressure vessel exists in this Standard ZIP.
- Pipes: PipeHolder is a compact caged pipe manifold. Dedicated bends/flanges and broader service routing are custom; do not pretend cables are pipes.
- Machinery: fan, vent and access-point shapes can form a service cluster. **Hero pump/reactor/generator CUSTOM REQUIRED**. Computer/Chest/Crate3/Crate4 are rejected from this first slice for weak/toy-like identity.
- Gate: hollow columns and structural truss only. **Hero leaf, lock/core, rails and actuator assemblies CUSTOM REQUIRED**. Flat Door_DarkMetal, Door_Simple, blocked frame and generic frame slabs are rejected as finished gates.
- Reject the aliens, all decal-only planes, thin ShortWall strips as standalone barriers, smooth basic columns, gratuitous alternate floor/wall families, crates and decorative neon duplication. Unselected modules do not receive implicit approval.
- Column_Pipes (6,480 triangles) is above repeated-module soft maximum; reject as delivered. Prefer PipeHolder (4,390) as a limited accent and lower-poly authored routing.

The subset remains local in the ignored extraction workspace. No third-party FBX is added to Unity or committed. No entire kit is promoted to production-ready status.
'''
write('ENVIRONMENT_FIRST_SLICE_SUBSET.md',text)
(DATA/'environment_subset.json').write_text(json.dumps(subset,indent=2),encoding='utf-8')
selected_names={r['module'] for r in subset}
environment_grades=[]
for r in environment:
    name=Path(r['path']).stem
    selected=name in selected_names
    category=Path(r['path']).parent.name
    topology=sum(m['topology']['zero_area_faces']+m['topology']['non_manifold_edges'] for m in r['meshes'])
    shape='B' if selected and name not in ('Platform_Metal2','Prop_Barrel_Large') else 'C'
    if category in ('Aliens','Decals') or name.startswith(('ShortWall_','Door_','Prop_Crate','Prop_Chest')):
        shape='Reject'
    environment_grades.append(dict(module=name,source=r['path'],grades=dict(
        silhouette=shape,topology='C' if topology else 'B',
        rig='B' if selected and r['armatures'] else 'Reject',
        animation_usefulness='B' if selected and r['actions'] else 'Reject',
        material_structure='C' if len(r['materials'])>2 else 'B',
        mobile_suitability='C' if r['triangles']>6000 else 'A',
        gravivore_fit='B' if selected else 'Reject',donor_value='B' if selected else 'C'),
        decision='selected structural/component donor; final art unapproved' if selected else 'excluded from first-slice subset',
        basis='Neutral source clay review and measured audit. Material grade concerns slot/trim structure; final shader appearance is unverified.'))
(DATA/'environment_grades.json').write_text(json.dumps(environment_grades,indent=2),encoding='utf-8')

body='''# Mobile material preparation

All four actual Poly Haven material libraries were opened in Blender. Production maps live in `art/visual-production-v2/materials/` outside Unity. All are 2048×2048 PNG, 8-bit delivery copies; higher-precision EXR originals remain local. Source/output hashes and shader nodes are in `data/material_prep.json`.

Base color is sRGB. OpenGL tangent normals, roughness and metallic are linear data; EXR conversions use Raw view transform with zero exposure and gamma one. Normal green channel is preserved for later explicit importer/shader convention. No baked-lighting operation or normal inversion is performed. No displacement, height, transparency or tessellation is shipped. No AO was supplied in these downloaded sets, so none is invented.
'''
for m in materials:
    body+='\n## '+m['name']+'\n\n'
    for item in m['maps']:
        body+=f"- `{item['output']}` — {item['bytes']:,} bytes, {item['color_space']}.\n"
    body+='- Excluded: '+', '.join(m['omitted'])+'.\n'
body+='''
## Intended use / strict review

- Blue metal plate: B surface breakup source for cool worn metal; the supplied blue coating is too dark for the pale G-0 shell without later calibrated authored color. No metallic map supplied; assign a chosen material value later rather than guessing from color.
- Metal plate: B industrial floor/panel source, includes actual metallic. Hazard stripes must be authored later, not claimed as supplied.
- Metal grate rusty: B grate/drain source with actual metallic; avoid repeating high-contrast rust in combat center.
- Rusty metal grid: B localized wall/service breakup; no metallic map supplied. Do not coat every surface in this pattern.

Fourteen maps are preparation assets, not four finalized runtime materials. Shared tiling scale, masks/smoothness packing, 1K import where appropriate, ASTC/compression and Android shader/device verification remain after art direction. G-0 Blockout V1 uses plain material swatches; these maps do not finish its textures.
'''
write('MATERIAL_PREP.md',body)

notice='\n## Visual Production V2 — local intake review and prepared CC0 maps\n\n'
notice+='Inspected 2026-10-06. Raw archives and donor models remain local and uncommitted. Character review renders are project-generated neutral derivatives, solely for source comparison, with the following attribution; no final character is shipped from them. Source checks and exact archive SHA256: `docs/visual-production-v2/modeling/SOURCE_INSPECTION_RESULTS.md`.\n\n'
for key,(title,author,url) in PROVENANCE.items():
    notice+=f'- [{title}]({url}) by {author} — Creative Commons Attribution, license recorded in PR #55; [CC BY reference](https://creativecommons.org/licenses/by/4.0/). Changes: import/static neutral clay renders and contact-sheet layout only. Stalenhag source is excluded from production reuse.\n'
notice+='\nQuaternius [Modular Sci-Fi MegaKit](https://quaternius.com/packs/modularscifimegakit.html), Quaternius, [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/): neutral geometry review renders; no pack FBX redistributed.\n\n'
for name,author in [('blue_metal_plate','Rob Tuytel'),('metal_plate','Rob Tuytel'),('metal_grate_rusty','Dimitrios Savva / Rob Tuytel'),('rusty_metal_grid','Amal Kumar')]:
    notice+=f'- [{name}](https://polyhaven.com/a/{name}), {author}, [Poly Haven CC0](https://polyhaven.com/license): converted diffuse/normal/roughness and metallic when supplied, max 2K. Production copies under `art/visual-production-v2/materials/{name}/`; source/output SHA256 in `data/material_prep.json`. Displacement excluded.\n'
notices=ROOT/'ThirdPartyNotices.md'
previous=notices.read_text(encoding='utf-8')
if '## Visual Production V2 — local intake' not in previous:
    notices.write_text(previous.rstrip()+'\n'+notice,encoding='utf-8')

manifest='''# First-slice asset manifest — measured local intake

2026-10-06. Supersedes PR #55's unavailable-source gate for these thirteen actual archives. Reference art/design: PR #53; provenance: PR #54/#55 plus official rechecks recorded in modeling results. No whole donor body is approved unchanged.

- G0-T0: custom-first; zero donor parts for Blockout V1. Modeling follows the committed source-inspection gate. Outer identity, four supports, core cavity, weapons and rear mass are custom.
- EN-SCOUT: Hasan Spider Robot, B preferred donor; actual `.blend`, rig and clips inspected. Custom shell/core/weapon, topology repair and retargeted motion required.
- EN-CUTTER: Mirandanimator Scorpion Robot, B preferred chassis donor; eight static rigid objects inspected. Tail/front replacement, optimization, rig and animation required.
- EN-MAG: custom torso/armor/core; restricted Vaportrash `crab_legs` and Preview_Tempest `Cylinder001` generic mechanism pool only. Stalenhag reuse rejected.
- ENV-FLOOR/WALL/CORNER/BARRIER/COLUMN: explicit B donor subset in `modeling/ENVIRONMENT_FIRST_SLICE_SUBSET.md`; sixteen unique pieces, no final kit approval.
- ENV-PIPE: selected PipeHolder donor; extra routing/flanges custom.
- ENV-TANK: Barrel_Large fragment only; substantial tank custom required.
- ENV-MACHINERY/REACTOR: selected fan/vent/access-point components; hero machinery custom required.
- ENV-GATE: hollow-column/truss donors only; authored leaf, frame treatment and mechanisms custom required.
- MAT-ARMOR/FLOOR/GRATE/WALL: four Poly Haven CC0 source sets inspected; fourteen 2K preparation maps stored outside Unity. Not applied as final G-0 textures.

Source archive hashes, imported measurements, candidate grades and limitations: `modeling/SOURCE_INSPECTION_RESULTS.md`. No runtime prefab, scene or gameplay asset is changed. Next gate: G-0 Blockout V1 art-direction review; stop there before production texturing/rigging/enemy completion or Unity integration.
'''
if (DATA/'g0_blockout.json').exists():
    model=json.loads((DATA/'g0_blockout.json').read_text(encoding='utf-8'))
    old='- G0-T0: custom-first; zero donor parts for Blockout V1. Modeling follows the committed source-inspection gate. Outer identity, four supports, core cavity, weapons and rear mass are custom.'
    new=f"- G0-T0: **Blockout V1 READY FOR ART-DIRECTION REVIEW, grade B**. `{model['source']}`: {model['triangles']:,} triangles, {model['mesh_objects']} meshes / {model['total_objects']} model objects, four material swatches, zero donor parts. Source gate committed before modeling. Outer identity, four supports, core cavity, weapons and rear mass are custom. Seven PNGs and strict limitations: `modeling/G0_BLOCKOUT_V1_REVIEW.md`. No production/Unity approval."
    manifest=manifest.replace(old,new)
(OUT.parent/'FIRST_SLICE_ASSET_MANIFEST.md').write_text(manifest,encoding='utf-8')

# Keep individual environment tile scratch images local; retain readable contact sheets.
tile_folder=ROOT/'.asset-intake-tmp/environment-tiles'
tile_folder.mkdir(exist_ok=True)
for r in environment:
    name=Path(r['path']).stem+'_threequarter.png'
    src=OUT/'source-review'/name
    if src.exists():
        src.replace(tile_folder/name)

# A compact map sheet records actual converted color/normal/roughness samples.
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',20)
sheet=Image.new('RGB',(1200,980),'#20262d')
draw=ImageDraw.Draw(sheet)
for col,m in enumerate(materials):
    draw.text((col*300+8,15),m['name'],font=font,fill='white')
    for row,channel in enumerate(('basecolor','normal_gl','roughness')):
        p=next(i['output'] for i in m['maps'] if i['channel']==channel)
        im=Image.open(ROOT/p).convert('RGB').resize((284,284))
        sheet.paste(im,(col*300+8,60+row*300))
        draw.text((col*300+12,64+row*300),channel,font=font,fill='white',stroke_width=1,stroke_fill='black')
sheet.save(OUT/'source-review/Materials_contact_sheet.png')
print('Inspection documentation written; source decision gate ready to commit.')
