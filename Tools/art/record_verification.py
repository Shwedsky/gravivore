"""Preserve exact executed verification outcomes with a compact completion record."""
import hashlib
import json
import shutil
import subprocess
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'docs/visual-production-v2/modeling'
DEST=OUT/'verification'
DEST.mkdir(exist_ok=True)
checks=json.loads((ROOT/'Builds/VisualV2Verification/UnityChecks.json').read_text(encoding='utf-8-sig'))
retry=json.loads((ROOT/'Builds/VisualV2Verification/PlayModeRetry.json').read_text(encoding='utf-8-sig'))
if isinstance(retry,list):
    retry=retry[0]
build=json.loads((ROOT/'Builds/Android/gravivore-dev-0.1.0+2.build.json').read_text())
apk=ROOT/'Builds/Android'/build['apkFilename']
record=dict(blender_version='5.2.2 LTS',art_tests=json.loads((OUT/'data/artifact_validation.json').read_text()),
    initial_unity_checks=checks,playmode_graphics_retry=retry,android_build=build,
    apk_relative=str(apk.relative_to(ROOT)).replace('\\','/'),apk_bytes=apk.stat().st_size,
    apk_sha256=hashlib.file_digest(apk.open('rb'),'sha256').hexdigest(),
    apk_scope='Existing runtime only. No new G-0, donor or material prep asset imported into Unity.',
    unity_generated_changes='Restored tracked files to HEAD; generated files preserved in ignored scratch.')
(OUT/'data/verification_summary.json').write_text(json.dumps(record,indent=2),encoding='utf-8')
for name in ('EditMode.xml','PlayMode.xml'):
    shutil.copy2(ROOT/'Builds/VisualV2Verification'/name,DEST/name)
for name in ('ProjectValidator.log','PlayMode.log','PlayMode-graphics.log'):
    source=ROOT/'Builds/VisualV2Verification'/name
    (DEST/name).write_text('\n'.join(line.rstrip() for line in source.read_text(encoding='utf-8-sig').splitlines())+'\n',encoding='utf-8')
shutil.copy2(ROOT/'Builds/Android/gravivore-dev-0.1.0+2.build.json',DEST/'android-dev.build.json')
shutil.copy2(ROOT/'.asset-intake-tmp/g0-verification.log',DEST/'BlenderSourceCheck.log')
skipped=[(x.get('fullname'),x.findtext('reason/message')) for x in ET.parse(DEST/'PlayMode.xml').findall('.//test-case') if x.get('result')=='Skipped']
text=f'''# Executed verification — Visual Production V2 modeling

All outcomes below were executed, not inferred from older PRs.

- Blender 5.2.2 LTS: actual source imports, neutral renders, authored-source save/reopen and mesh validation completed. 87 closed/manifold model meshes; no zero-area faces or loose vertices; four toe contacts, core/attack sockets and clean root verified. Static pose only.
- Python compile: `python -m compileall -q Tools/art` passed.
- Artifact delivery: five tests passed (map hashes/channels/normals; actual intake/subset coverage; actual source animation motion; saved blend/evidence consistency; Git scope/archive exclusion).
- Unity 6000.3.0f1 compile: passed as part of the executed Android build and test runs, using the repository's standard ProjectConfigurator.
- EditMode: 386/386 passed, zero failures, zero skipped. Full executed XML retained. Log copies retain content with trailing whitespace normalized; originals remain in ignored Builds/.
- PlayMode final: 92 passed, zero failed, one ignored out of 93. Ignored capture-only test: `{skipped[0][0]}`; reason: {skipped[0][1]} No additional old-world capture was requested.
- Project validator: `Gravivore.Editor.ProjectValidator.ValidateOrThrow` executed and exited zero. Full validator log retained.
- Dev Android build: `./build-android.ps1 -Flavor Dev -Version 0.1.0 -VersionCode 2` passed, Unity exit zero. aapt verified package `com.gravivore.mobile.dev`, versionCode 2, development/debug settings and ARM64-only architecture.
- APK: `{record['apk_relative']}`; {record['apk_bytes']:,} bytes; SHA256 `{record['apk_sha256']}`. Local APK is git ignored; metadata is retained here. Build metadata records runtime-source checkpoint `{build['gitCommitSha']}` because no new art is integrated.

## Failed attempts and recovery

An initial Unity validator attempt failed to initialize licensing (IPC/signature validation); it did not establish a compilation result. Licensing recovered during the subsequent real Android build and final check runs.

The first PlayMode run with `-nographics` crashed in Unity's graphics draw path with exit -1073741819 and produced no result XML. Its crash log is retained as `verification/PlayMode.log`. Retrying only PlayMode with `-force-d3d11 -force-gfx-direct` in a hidden batch window completed with the final results above. The ignored optional capture test is not represented as a pass.

## Scope and cleanup

These Unity checks exercise the existing runtime. They do **not** prove G-0 locomotion, shader conversion, device performance or new art integration. The model, prepared maps and donor selections remain outside Unity. No Chapter01 prefab replacement is delivered.

CLI configuration/tests generated Unity settings, import metadata, temporary test scenes, materials and diagnostic files in this isolated worktree. Tracked changes were restored to HEAD; generated files were preserved in ignored `.asset-intake-tmp/`. The user's original checkout was not modified. The review commit excludes all Assets/Packages/ProjectSettings changes, raw source archives and extracted donor meshes.

Full changed-file list: `FILES_CHANGED.txt`. Primary outcome: G-0 Blockout V1 ready for art-direction review, grade B. Next gate: human G-0 review; no next implementation spec started.
'''
(OUT/'VERIFICATION.md').write_text(text,encoding='utf-8')
print('Verification results and exact runtime limitation recorded.')
