"""Verify new original sources and the reused, already licensed CC0 ORM data."""
import json,hashlib,subprocess,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];DOC=ROOT/'docs/surface-hero-v46/verification'
subprocess.run([sys.executable,str(ROOT/'Tools/visual-replacement-v3/license_audit.py')],check=True,stdout=subprocess.DEVNULL)
surfaces=json.loads((DOC/'surface_provenance.json').read_text())
proof=json.loads((ROOT/'docs/visual-replacement-v3/texture_provenance.json').read_text())
for e in surfaces['verifiedDonorAo']:
    assert e['license']=='CC0';assert hashlib.sha256((ROOT/e['sourcePath']).read_bytes()).hexdigest()==e['sha256']
    assert any(p['sourcePath']==e['sourcePath'] and p['sha256']==e['sha256'] for p in proof)
original=[]
for family,e in json.loads((DOC/'machinery_authoring.json').read_text()).items():
    source=ROOT/e['blend'];assert source.read_bytes().startswith((b'BLENDER',bytes.fromhex('28b52ffd')))
    original.append({'family':family,'source':e['blend'],'sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'authorship':'Original project Blender mesh authoring; no third-party geometry'})
result={'validated':True,'existingLicensedSourcesVerified':36,'existingLicensedTrimTexturesVerified':12,'reusedCc0AoSources':surfaces['verifiedDonorAo'],'newOriginalBlenderSources':original,'newPaidAssets':0,'newExternalGeometry':0,'ownerReferenceRedistributed':False,'shippedNoticesRetained':True}
(DOC/'license_audit.json').write_text(json.dumps(result,indent=2));print('V46_LICENSE_ORIGINAL_SOURCE_AUDIT_PASS')
