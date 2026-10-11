"""Record executed Unity gates and independently inspect the delivered APK."""
import hashlib, json, subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
VERIFY = ROOT / 'docs/history/implementation-passes/chapter01-blueprint-world-r1/verification'
ANDROID = Path('C:/Program Files/Unity/Hub/Editor/6000.3.0f1/Editor/Data/PlaybackEngines/AndroidPlayer/SDK')
APK = ROOT / 'Builds/Android/gravivore-dev-0.1.0+48.apk'
LOGS = {
    'compile': VERIFY / 'compile-delivery-final.log',
    'projectValidator': VERIFY / 'validator-delivery-final.log',
    'androidBuild': ROOT / 'Builds/Logs/android-dev-20261009-225604.log',
}

def sha(file):
    return hashlib.sha256(file.read_bytes()).hexdigest()

# Unity writes empty trailing columns with a space; canonicalize copied text
# evidence before hashing it so the proof commit passes whitespace validation.
for name in ['apk-packed-assets.txt', 'compiled_shader_variants.txt']:
    file = VERIFY / name
    file.write_text('\n'.join(line.rstrip() for line in file.read_text().splitlines()) + '\n', encoding='utf-8')

gates = {}
for name, file in LOGS.items():
    body = file.read_text(encoding='utf-8', errors='replace')
    success = 'Application will terminate with return code 0' in body
    if name == 'projectValidator':
        success &= 'Gravivore.Editor.ProjectValidator:ValidateOrThrow' in body
    if name == 'androidBuild':
        success &= 'Build Finished, Result: Success.' in body
    if not success:
        raise RuntimeError('Executed Unity gate did not succeed: ' + name)
    gates[name] = dict(passed=True, unityExitCode=0,
        log=str(file.relative_to(ROOT)).replace('\\', '/'), logSha256=sha(file))

badging = subprocess.check_output([str(ANDROID / 'build-tools/36.0.0/aapt2.exe'),
    'dump', 'badging', str(APK)], text=True, encoding='utf-8')
required = ["name='com.gravivore.mobile.dev' versionCode='48'", "native-code: 'arm64-v8a'", 'application-debuggable']
if any(token not in badging for token in required):
    raise RuntimeError('APK manifest does not match the requested development build.')
essential = '\n'.join(line for line in badging.splitlines()
    if line.startswith(('package:', 'minSdkVersion:', 'targetSdkVersion:', 'native-code:', 'application-debuggable')))
(VERIFY / 'apk-manifest.txt').write_text(essential + '\n', encoding='utf-8')
devices = subprocess.check_output([str(ANDROID / 'platform-tools/adb.exe'), 'devices'], text=True)
attached = [line for line in devices.splitlines()[1:] if line.strip()]
apk_sha = sha(APK)
if (VERIFY / 'apk-sha256.txt').read_text().strip() != apk_sha:
    raise RuntimeError('Post-build callback APK hash differs from delivered file.')
post_device = json.loads((VERIFY / 'apk-post-device-combat.json').read_text())
if post_device['apkSha256'] != apk_sha:
    raise RuntimeError('Preserved combat packing evidence belongs to a different APK.')
render = json.loads((VERIFY / 'render_dependency_report.json').read_text())
if not render['validated']:
    raise RuntimeError('Rendering dependency audit failed.')
variants = (VERIFY / 'compiled_shader_variants.txt').read_text()
for token in ['Universal Render Pipeline/Lit', 'Universal Render Pipeline/Particles/Unlit', 'UI/Default', 'GLES3x', 'Vulkan', '_NORMALMAP', '_METALLICSPECGLOSSMAP', '_OCCLUSIONMAP', '_EMISSION']:
    if token not in variants:
        raise RuntimeError('Required compiled shader evidence missing: ' + token)
serialized = (VERIFY / 'apk-world-serialized-sectors.txt').read_text().splitlines()
if len(serialized) != 10:
    raise RuntimeError('Expected eight sectors, world root and service network in APK.')
evidence = dict(validated=True, executedGates=gates, apkSha256=apk_sha,
    proofTextNormalization='Trailing whitespace removed from copied packed-asset and shader-variant lists; substantive entries unchanged.',
    manifest=essential, adbDevices=attached, androidDeviceMeasurementPerformed=False,
    shaderAuditPassed=True, compiledShaderVariantRows=len(variants.splitlines()),
    serializedWorldEntries=serialized,
    serializedOldWorldLayersAbsent=True,
    oldWorldAbsenceMethod='Executed Chapter01BlueprintWorldBuildAudit checks length-prefixed serialized scene names in the actual APK and fails the build on V45/V46/V47 world or old deck names.',
    proofFiles={file.name: sha(file) for file in [VERIFY / name for name in
        ['apk-required-resources.txt', 'apk-packed-assets.txt', 'apk-world-serialized-sectors.txt',
         'compiled_shader_variants.txt', 'render_dependency_report.json', 'render_profile.json',
         'serialized_scene_renderers.txt', 'apk-post-device-combat.json']]})
(VERIFY / 'build-evidence.json').write_text(json.dumps(evidence, indent=2) + '\n')
print('BUILD_EVIDENCE', 'all gates passed', apk_sha, 'attached devices', len(attached))
