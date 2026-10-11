"""Record/verify the owner's canonical archives and accepted version 47 APK."""
import hashlib,json,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
OWNER=ROOT.parents[1]
OUTPUT=ROOT/'docs/history/implementation-passes/chapter01-blueprint-world-r1/verification/owner-preservation.json'
PATHS=[
 'ExternalAssetIntake/Current/quaternius-modular-scifi-megakit-standard/Modular_SciFi_MegaKit_Standard.zip',
 'ExternalAssetIntake/Current/molten-maps-scifi/Molten Maps SciFi Asset Pack.zip',
 'Builds/Android/gravivore-dev-0.1.0+47.apk',
 'Builds/Android/gravivore-dev-0.1.0+47.build.json']
def record(path):
    file=OWNER/path
    return dict(path=path,bytes=file.stat().st_size,sha256=hashlib.sha256(file.read_bytes()).hexdigest())
current=[record(path) for path in PATHS]
if '--capture' in sys.argv:
    if OUTPUT.exists():raise RuntimeError('Preservation baseline already exists; do not replace it.')
    data=dict(ownerRoot=str(OWNER),beforeBuild=current,verifiedAfterDelivery=False)
else:
    data=json.loads(OUTPUT.read_text())
    if data['beforeBuild']!=current:raise RuntimeError('Owner original archive or V47 delivery changed.')
    data.update(afterDelivery=current,verifiedAfterDelivery=True)
OUTPUT.write_text(json.dumps(data,indent=2)+'\n')
print('OWNER_ORIGINALS',len(current),'captured' if '--capture' in sys.argv else 'UNCHANGED')
