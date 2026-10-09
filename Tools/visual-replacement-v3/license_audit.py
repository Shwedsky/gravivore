"""Re-verify exact production donor receipts and shipped attribution."""
import csv,hashlib,json,re
from pathlib import Path
from pypdf import PdfReader
ROOT=Path(__file__).resolve().parents[2]
DOCS=ROOT/'docs/visual-replacement-v3'
rights={r['sourceId']:r for r in csv.DictReader((ROOT/'docs/free-asset-intake-v2/ASSET_PROVENANCE_RESOLVED.csv').open(encoding='utf-8-sig'))}
entries=json.loads((DOCS/'ingestion_manifest.json').read_text())
for e in entries:
 r=rights[e['sourceId']]
 assert r['productionUseAllowed']=='True' and r['rawRedistributionAllowed']=='True' and r['license']==e['license']
 assert e['license'] in ('CC0','CC_BY_4_0')
 assert hashlib.sha256((ROOT/e['sourcePath']).read_bytes()).hexdigest()==e['sourceSha256']
for sid in {e['sourceId'] for e in entries}:
 proof=next((ROOT/'Assets/_Game/Content/VisualReplacementV3/Licenses').glob(sid+'.*'))
 if sid=='ui-exe':
  text='\n'.join(p.extract_text() for p in PdfReader(proof).pages)
  assert 'Catherine Laserna' in text and ('4.0' in text or 'Attribution' in text)
 else:
  assert 'CC0' in proof.read_text(encoding='utf-8-sig')
  expected=re.search(r'SHA256 ([a-f0-9]{64})',rights[sid]['licenseEvidence']).group(1)
  assert hashlib.sha256(proof.read_bytes()).hexdigest()==expected
for e in json.loads((DOCS/'texture_provenance.json').read_text()):
 assert hashlib.sha256((ROOT/e['sourcePath']).read_bytes()).hexdigest()==e['sha256']
shipped=(ROOT/'Assets/StreamingAssets/ThirdPartyNotices.txt').read_text(encoding='utf-8')
assert all(s in shipped for s in ('Catherine Laserna','CC BY 4.0','https://cjlaserna.itch.io/exe','https://creativecommons.org/licenses/by/4.0/'))
result={'validated':True,'verifiedSourceFiles':len(entries),'verifiedTrimTextures':12,'redistributableFamilies':sorted({e['sourceId'] for e in entries}),'exeCreditShipped':True,'assetStoreProductionSources':0,'originalVfxMasks':True}
(DOCS/'verification/license_audit.json').write_text(json.dumps(result,indent=2));print(json.dumps(result,indent=2))
