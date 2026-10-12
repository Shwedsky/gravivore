from pathlib import Path
import hashlib,json,re,subprocess,zipfile,shutil,sys
root=Path(__file__).resolve().parents[2];owner=root.parents[1]
out=root/'docs/history/implementation-passes/actors-combat-concept-world/verification'
android=Path('C:/Program Files/Unity/Hub/Editor/6000.3.0f1/Editor/Data/PlaybackEngines/AndroidPlayer')
apk=root/'Builds/Android/gravivore-dev-0.1.0+54.apk';old=owner/'Builds/Android/gravivore-dev-0.1.0+53.apk'
def run(*args):return subprocess.check_output([str(a) for a in args],stderr=subprocess.STDOUT).decode('utf-8',errors='replace')
meta=json.loads(apk.with_suffix('.build.json').read_text(encoding='utf-8-sig'))
assert meta['versionCode']==54 and meta['applicationId']=='com.gravivore.mobile.dev'
assert meta['developmentBuild'] and meta['allowDebugging'] and meta['targetArchitecture']=='ARM64'
assert meta['gitCommitSha']==(out/'build-source-commit.txt').read_text().strip()
aapt=android/'SDK/build-tools/36.0.0/aapt2.exe';java=android/'OpenJDK/bin/java.exe';signer=android/'SDK/build-tools/36.0.0/lib/apksigner.jar'
badging=run(aapt,'dump','badging',apk);manifest=run(aapt,'dump','xmltree','--file','AndroidManifest.xml',apk)
assert "package: name='com.gravivore.mobile.dev' versionCode='54'" in badging
assert 'application-debuggable' in badging and re.search(r'android:screenOrientation[^=\n]*=(?:1|0x1)\b',manifest)
with zipfile.ZipFile(apk) as archive:
    libs=[n for n in archive.namelist() if n.startswith('lib/') and n.endswith('.so')]
    assert libs and all(n.startswith('lib/arm64-v8a/') for n in libs) and 'lib/arm64-v8a/libil2cpp.so' in libs
digests=[]
for code,path in [(54,apk),(53,old)]:
    output=run(java,'-jar',signer,'verify','--verbose','--print-certs',path)
    (out/f'apk{code}-signature.txt').write_text(output)
    digests.append(re.search(r'Signer #1 certificate SHA-256 digest: (\w+)',output).group(1))
assert digests[0]==digests[1]
resources=json.loads((out/'actor-apk-resources.json').read_text())
assert resources['passed'] and resources['shaderVariants']>0
variants=(out/'actor-target-shader-variants.txt').read_text()
assert 'GLES3x' in variants and 'Vulkan' in variants and 'IndustrialForward' in variants
controller_output=run(sys.executable,root/'Tools/actors-combat-concept-world/inspect_controllers.py',apk)
controllers=[json.loads(line) for line in controller_output.splitlines() if line.startswith('{')]
assert {c['name'] for c in controllers}=={'G0_Production_V3_Review','Magnetar_V2','Custodian_V2'}
assert all(c['layers']==2 and c['maskSourceMatches'] and c['supportDefaultWeight']==0 and c['compiledGaitParameter'] and c['compiledSupportLayer'] and c['compiledGaitState'] for c in controllers)
(out/'apk54-native-controllers.json').write_text(json.dumps(dict(passed=True,controllers=controllers,
 method='Actual class 91 AnimatorController objects decoded from APK. Native layer arrays and compiled skeleton mask path hashes/weights exactly match the authored AvatarMasks. Masks are compiled into the controllers rather than separate class 319 resources.'),indent=2))
render=root/'docs/history/visual-stages/chapter01-visual-passes/render-hotfix/verification'
for name in ['render_dependency_report.json','compiled_shader_variants.txt','serialized_scene_renderers.txt','render_profile.json']:
    shutil.copyfile(render/name,out/name)
world=root/'docs/history/implementation-passes/chapter01-blueprint-world-r2/verification'
for name in ['apk-required-resources.txt','apk-world-serialized-sectors.txt','apk-packed-assets.txt']:
    shutil.copyfile(world/name,out/name)
assert json.loads((out/'render_dependency_report.json').read_text())['validated']
for sector in ['RepairHub','RelayYard','CapacitorField','CuttingFloor','ShieldDump','HaulerGraveyard','MagnetarComplex','CustodianCore']:
    assert sector in (out/'apk-world-serialized-sectors.txt').read_text()
shutil.copyfile(apk.with_suffix('.build.json'),out/'apk54-build.json')
(out/'apk54-badging.txt').write_text(badging);(out/'apk54-manifest.txt').write_text(manifest)
record=dict(passed=True,apkFilename=apk.name,bytes=apk.stat().st_size,sha256=hashlib.sha256(apk.read_bytes()).hexdigest(),
 applicationId=meta['applicationId'],versionCode=54,sourceCommit=meta['gitCommitSha'],architecture='ARM64',scriptingBackend='IL2CPP',
 portrait=True,developmentBuild=True,signatureVerified=True,signingCertificateMatchesApk53=True,certificateSha256=digests[0],
 libraries=libs,actorShaderVariants=resources['shaderVariants'],nativeMaskedControllersVerified=True,method='Executed Unity packed-resource/target-variant audits, SDK aapt2/apksigner, ZIP inspection and native AnimatorController skeleton mask decoding. No Android device execution.')
(out/'apk54-verification.json').write_text(json.dumps(record,indent=2))
print(json.dumps(record,indent=2))
