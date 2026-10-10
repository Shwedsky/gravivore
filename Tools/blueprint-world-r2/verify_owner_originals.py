import hashlib,json,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];OWNER=ROOT.parents[1]
VERIFY=ROOT/'docs/history/implementation-passes/chapter01-blueprint-world-r2/verification/owner-preservation.json'
paths=[
 'ExternalAssetIntake/Current/quaternius-modular-scifi-megakit-standard/Modular_SciFi_MegaKit_Standard.zip',
 'ExternalAssetIntake/Current/molten-maps-scifi/Molten Maps SciFi Asset Pack.zip',
 *[f'Builds/Android/gravivore-dev-0.1.0+{version}.{extension}' for version in [47,48] for extension in ['apk','build.json']]]
records=[dict(path=p,bytes=(OWNER/p).stat().st_size,sha256=hashlib.sha256((OWNER/p).read_bytes()).hexdigest()) for p in paths]
if '--capture' in sys.argv:
    if VERIFY.exists():raise RuntimeError('Do not overwrite an existing preservation baseline.')
    data=dict(ownerRoot=str(OWNER),beforeBuild=records,verifiedAfterDelivery=False)
else:
    data=json.loads(VERIFY.read_text())
    if records!=data['beforeBuild']:raise RuntimeError('Owner ZIP or previous APK changed.')
    data.update(afterDelivery=records,verifiedAfterDelivery=True)
VERIFY.write_text(json.dumps(data,indent=2)+'\n');print('OWNER_ORIGINALS',len(records),'PRESERVED')
