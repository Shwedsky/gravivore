"""Record executed gates and an actual APK, without manufacturing a success receipt."""
import hashlib, json, subprocess, xml.etree.ElementTree as ET, zipfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'docs/history/implementation-passes/chapter01-architecture-production-v1'
VERIFY=OUT/'verification'
def digest(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def tests(name):
    xml=ET.parse(VERIFY/name).getroot()
    assert xml.attrib['result']=='Passed',name
    return {k:xml.attrib[k] for k in ['total','passed','failed','skipped','start-time','end-time']}
receipt=dict(editMode=tests('EditModeFinal.xml'),playMode=tests('FinalIntegration.xml'))
for gate,log in [('compile','ProductionClearance.log'),('projectValidation','ProjectValidationFinal.log')]:
    text=(VERIFY/log).read_text(errors='replace')
    assert 'Application will terminate with return code 0' in text,log
    receipt[gate]=dict(passed=True,log=log,method='Executed Unity batch process; successful exit receipt retained locally.')
apk=ROOT/'Builds/Android/gravivore-dev-0.1.0+55.apk'
if apk.is_file():
    with zipfile.ZipFile(apk) as z:
        assert z.testzip() is None
        names=z.namelist()
        for member in ['AndroidManifest.xml','lib/arm64-v8a/libil2cpp.so','lib/arm64-v8a/libunity.so']:
            assert member in names,member
        required=['Energy Chamber / production','Architectural collision','EC_CentralChamber','EC_VerticalDistribution','EC_CoreWarm','EC_NodeCool']
        found={}
        for member in names:
            if not member.startswith('assets/bin/Data/'): continue
            name=member.rsplit('/',1)[-1]
            if '.assets' not in name and not name.startswith(('level','globalgamemanagers')): continue
            data=z.read(member)
            for term in required:
                if term not in found and term.encode() in data: found[term]=member
            if len(found)==len(required): break
        assert len(found)==len(required),('Missing production serialization',found)
        (VERIFY/'apk-energy-chamber-packing.json').write_text(json.dumps(dict(validated=True,apk=str(apk),serializedEntries=found),indent=2)+'\n')
    receipt['android']=dict(passed=True,path=str(apk),bytes=apk.stat().st_size,sha256=digest(apk),architecture='ARM64 IL2CPP',versionCode=55,flavor='Dev',zipVerified=True)
    metadata=apk.with_suffix('.build.json')
    if metadata.is_file():
        receipt['android']['metadata']=json.loads(metadata.read_text())
else:
    receipt['android']=dict(passed=False,path=None,reason='No final APK file exists; inspect the actual build log.')
receipt['deviceGate']=dict(performed=False,reason='No new human on-device visual or Android FPS/heat acceptance is claimed.')
scene=ROOT/'Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity'
receipt['sourceSceneSha256']=digest(scene)
(VERIFY/'delivery-checks.json').write_text(json.dumps(receipt,indent=2)+'\n')
print(json.dumps(receipt,indent=2))
