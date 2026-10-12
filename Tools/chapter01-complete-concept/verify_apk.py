"""Read-only verification of the built APK and its preserved APK 52 predecessor."""
from pathlib import Path
import hashlib, json, re, shutil, subprocess, zipfile

ROOT=Path(__file__).resolve().parents[2]
OWNER=ROOT.parents[1]
OUT=ROOT/'docs/history/implementation-passes/chapter01-complete-concept/verification'
ANDROID=Path('C:/Program Files/Unity/Hub/Editor/6000.3.0f1/Editor/Data/PlaybackEngines/AndroidPlayer')
APK=ROOT/'Builds/Android/gravivore-dev-0.1.0+53.apk'
OLD=OWNER/'Builds/Android/gravivore-dev-0.1.0+52.apk'
META=APK.with_suffix('.build.json')
AAPT=ANDROID/'SDK/build-tools/36.0.0/aapt2.exe'
JAVA=ANDROID/'OpenJDK/bin/java.exe'
SIGNER=ANDROID/'SDK/build-tools/36.0.0/lib/apksigner.jar'

def run(*args):
    return subprocess.check_output([str(a) for a in args],stderr=subprocess.STDOUT).decode('utf-8',errors='replace')

meta=json.loads(META.read_text(encoding='utf-8-sig'))
assert meta['applicationId']=='com.gravivore.mobile.dev' and meta['versionCode']==53
assert meta['developmentBuild'] and meta['allowDebugging'] and meta['targetArchitecture']=='ARM64'
assert meta['gitCommitSha']==(OUT/'build-source-commit.txt').read_text().strip()
badging=run(AAPT,'dump','badging',APK)
manifest=run(AAPT,'dump','xmltree','--file','AndroidManifest.xml',APK)
assert "package: name='com.gravivore.mobile.dev' versionCode='53'" in badging
assert 'application-debuggable' in badging
assert re.search(r'android:screenOrientation[^=\n]*=(?:1|0x1)\b',manifest), 'Portrait manifest missing'
with zipfile.ZipFile(APK) as archive:
    libs=[n for n in archive.namelist() if n.startswith('lib/') and n.endswith('.so')]
    assert libs and all(n.startswith('lib/arm64-v8a/') for n in libs)
    assert 'lib/arm64-v8a/libil2cpp.so' in libs

signatures=[]
for name,apk in [('53',APK),('52',OLD)]:
    output=run(JAVA,'-jar',SIGNER,'verify','--verbose','--print-certs',apk)
    (OUT/f'apk{name}-signature.txt').write_text(output,encoding='utf-8')
    digest=re.search(r'Signer #1 certificate SHA-256 digest: (\w+)',output)
    assert digest, 'Verified signing certificate not found'
    signatures.append(digest.group(1))
assert signatures[0]==signatures[1], 'DEV upgrade signing identity changed'

render=ROOT/'docs/history/visual-stages/chapter01-visual-passes/render-hotfix/verification'
world=ROOT/'docs/history/implementation-passes/chapter01-blueprint-world-r2/verification'
for name in ['render_dependency_report.json','compiled_shader_variants.txt','serialized_scene_renderers.txt','render_profile.json']:
    shutil.copyfile(render/name,OUT/name)
for name in ['apk-required-resources.txt','apk-world-serialized-sectors.txt','apk-packed-assets.txt']:
    shutil.copyfile(world/name,OUT/name)
packing=json.loads((OUT/'render_dependency_report.json').read_text(encoding='utf-8'))
assert packing['validated']
compiled=(OUT/'compiled_shader_variants.txt').read_text(encoding='utf-8')
post=[s for s in compiled.splitlines() if 'Hidden/Universal Render Pipeline/' in s]
assert any('/Bloom |' in s for s in post) and any('/UberPost |' in s and '_TONEMAP_ACES' in s for s in post)
assert any('/UberPost |' in s and '_BLOOM_LQ' in s for s in post)
shutil.copyfile(META,OUT/'apk53-build.json')
(OUT/'apk53-badging.txt').write_text(badging,encoding='utf-8')
(OUT/'apk53-manifest.txt').write_text(manifest,encoding='utf-8')
record=dict(passed=True,apkFilename=APK.name,bytes=APK.stat().st_size,sha256=hashlib.sha256(APK.read_bytes()).hexdigest(),
            applicationId=meta['applicationId'],versionCode=53,sourceCommit=meta['gitCommitSha'],architecture='ARM64',scriptingBackend='IL2CPP',
            portrait=True,developmentBuild=True,signatureVerified=True,signingCertificateMatchesApk52=True,certificateSha256=signatures[0],
            libraries=libs,postProcessingVariants=len(post),postCompilerPlatforms=sorted({s.split(' | ')[3] for s in post}),
            method='Unity build audits plus SDK aapt2/apksigner and ZIP inspection. No Android device execution.')
(OUT/'apk53-verification.json').write_text(json.dumps(record,indent=2),encoding='utf-8')
print(json.dumps(record,indent=2))
