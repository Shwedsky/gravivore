"""Inspect the built APK and preserve every previous owner build; no device acceptance inferred."""
import argparse, hashlib, json, re, shutil, subprocess, zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
OWNER = Path(r"C:\Users\pamak\Documents\ChatGPT\gravivore\Builds\Android")
OUT = ROOT / "docs/history/implementation-passes/chapter01-r2-actors-v2-integration/verification"
ANDROID = Path(r"C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Data\PlaybackEngines\AndroidPlayer")
NAME = "gravivore-dev-0.1.0+50.apk"
def sha(path):
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()
def run(*args):
    return subprocess.check_output([str(a) for a in args], stderr=subprocess.STDOUT).decode("utf-8", "replace")
parser=argparse.ArgumentParser();parser.add_argument("mode",choices=["baseline","deliver"]);args=parser.parse_args()
OUT.mkdir(parents=True,exist_ok=True)
baseline_path=OUT / "owner-builds-before.json"
if args.mode=="baseline":
    assert not (OWNER/NAME).exists(), "VersionCode 50 already exists; never overwrite"
    baseline={p.name:{"bytes":p.stat().st_size,"sha256":sha(p)} for p in OWNER.iterdir() if p.is_file()}
    baseline_path.write_text(json.dumps(baseline,indent=2)+"\n")
    print("BASELINE_PREVIOUS_OWNER_FILES",len(baseline));raise SystemExit()
baseline=json.loads(baseline_path.read_text())
apk=ROOT/"Builds/Android"/NAME
assert apk.is_file()
metadata_path=apk.with_suffix(".build.json");metadata=json.loads(metadata_path.read_text())
assert metadata["applicationId"]=="com.gravivore.mobile.dev" and metadata["versionCode"]==50
assert metadata["targetArchitecture"]=="ARM64" and metadata["developmentBuild"] and metadata["allowDebugging"]
aapt=sorted((ANDROID/"SDK/build-tools").glob("*/aapt.exe"))[-1]
badging=run(aapt,"dump","badging",apk);manifest=run(aapt,"dump","xmltree",apk,"AndroidManifest.xml")
(OUT/"apk-badging.txt").write_text(badging);(OUT/"apk-manifest.txt").write_text(manifest)
assert "name='com.gravivore.mobile.dev'" in badging and "versionCode='50'" in badging
assert "native-code: 'arm64-v8a'" in badging
assert re.search(r"android:debuggable.*0xffffffff",manifest)
assert re.search(r"android:screenOrientation.*0x1\b",manifest), "Portrait manifest required"
java=ANDROID/"OpenJDK/bin/java.exe"
signer=sorted((ANDROID/"SDK/build-tools").glob("*/lib/apksigner.jar"))[-1]
signature=run(java,"-jar",signer,"verify","--verbose","--print-certs",apk)
(OUT/"apk-signer.txt").write_text(signature)
assert "CN=Android Debug" in signature
previous_signature=run(java,"-jar",signer,"verify","--print-certs",OWNER/"gravivore-dev-0.1.0+49.apk")
certificate_pattern=r"certificate SHA-256 digest: ([a-fA-F0-9]+)"
assert re.search(certificate_pattern,signature).group(1)==re.search(certificate_pattern,previous_signature).group(1), "DEV signer differs from APK 49"
with zipfile.ZipFile(apk) as archive:
    entries=archive.namelist()
    assert "lib/arm64-v8a/libil2cpp.so" in entries
    assert not any(p.startswith("lib/") and not p.startswith("lib/arm64-v8a/") for p in entries)
packing=json.loads((OUT/"apk-actor-world-packing.json").read_text())
assert packing["validated"] and packing["apkSha256"]==sha(apk)
assert packing["buildScenes"]==["Assets/_Game/Content/Scenes/Bootstrap.unity","Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity"]
for name,record in baseline.items():
    path=OWNER/name
    assert path.exists() and path.stat().st_size==record["bytes"] and sha(path)==record["sha256"], name+" changed"
for path in (apk,metadata_path):
    target=OWNER/path.name
    assert not target.exists(), "Never overwrite existing owner delivery"
    shutil.copy2(path,target)
    assert sha(path)==sha(target)
report=dict(validated=True,ownerApkPath=str(OWNER/NAME),apkBytes=apk.stat().st_size,apkSha256=sha(apk),
    metadata=metadata,manifestPortrait=True,manifestDebuggable=True,arm64Only=True,il2cppLibraryPacked=True,
    debugSignerVerified=True,signerMatchesWorld49=True,previousOwnerFilesPreserved=len(baseline),world49Sha256=baseline["gravivore-dev-0.1.0+49.apk"]["sha256"],
    androidDeviceAcceptance="NOT RUN: no connected Android device; owner gate remains pending")
(OUT/"apk-metadata.json").write_text(json.dumps(report,indent=2)+"\n")
shutil.copy2(metadata_path,OUT/"build-metadata.json")
print("APK_DELIVERY_VERIFIED",json.dumps(report,indent=2))
