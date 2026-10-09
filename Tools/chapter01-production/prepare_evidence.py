"""Collect executed results and preserve diagnostics without local license identifiers."""
import hashlib
import json
from pathlib import Path
import re
import subprocess
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
OUTPUT = ROOT / 'docs/history/visual-stages/chapter01-visual-passes/chapter01-production/verification'
BASELINE = '0152ec421eed53cc64c0b07fb10c4496589ae047'
PRESERVED = [
    'Assets/_Game/Content/VisualSlice/Models', 'Assets/_Game/Content/VisualSlice/Materials',
    'Assets/_Game/Content/VisualSlice/Prefabs', 'Assets/_Game/Content/Definitions/S02*',
    'Assets/_Game/Content/Definitions/S03*', 'Assets/_Game/Content/Definitions/S04*',
    'Assets/_Game/Content/Definitions/S06*', 'Assets/_Game/Content/Definitions/S08_Chapter01World.asset',
    'Assets/_Game/Content/Definitions/S09*', 'Assets/_Game/Content/Definitions/S11*',
    'Assets/_Game/Runtime/Gameplay/Combat', 'Assets/_Game/Runtime/Gameplay/Encounters',
    'Assets/_Game/Runtime/Persistence'
]

def git(*args):
    return subprocess.check_output(['git', *args], cwd=ROOT, text=True).strip()

def sanitize(path):
    content = path.read_text(encoding='utf-8-sig', errors='replace')
    patterns = [r'(?im)^.*(?:Machine Id:|Machine ID:|User Id:|User ID:|LicensingClient.*(?:token|serial)|License serial number:|Entitlement Id:|access token|refresh token).*$',
                r'(?im)^.*(?:Unity Editor license serial|License Serial|Session Id:|Correlation Id:).*$']
    for pattern in patterns:
        content = re.sub(pattern, '[local licensing identifier redacted]', content)
    path.write_text(content, encoding='utf8')

results = {}
for mode in ('EditMode', 'PlayMode'):
    xml = ET.parse(OUTPUT / (mode + '.xml')).getroot()
    results[mode] = {key: int(xml.attrib[key]) for key in ('total', 'passed', 'failed', 'skipped')}
    results[mode]['startedUtc'] = xml.attrib['start-time']
    results[mode]['finishedUtc'] = xml.attrib['end-time']
    results[mode]['skippedTests'] = [node.attrib['fullname'] for node in xml.iter('test-case') if node.attrib.get('result') == 'Skipped']
    if results[mode]['failed']:
        raise RuntimeError(mode + ' has failed tests')
for path in OUTPUT.glob('*.log'):
    sanitize(path)
results['sourceSha'] = json.loads((OUTPUT / 'build_metadata.json').read_text(encoding='utf-8-sig'))['gitCommitSha']
(OUTPUT / 'test_results.json').write_text(json.dumps(results, indent=2), encoding='utf8')
if git('diff', '--name-only', BASELINE, '--', *PRESERVED):
    raise RuntimeError('Accepted art or gameplay preservation audit failed')
audit = {'validated': True, 'baseline': BASELINE, 'buildSourceSha': results['sourceSha'], 'unchangedPaths': PRESERVED}
(OUTPUT / 'preservation_audit.json').write_text(json.dumps(audit, indent=2), encoding='utf8')
paths = git('diff', '--name-only', BASELINE, results['sourceSha']).splitlines()
(OUTPUT / 'source_files_changed.txt').write_text('\n'.join(paths) + '\n', encoding='utf8')
captures = {path.name: hashlib.sha256(path.read_bytes()).hexdigest() for path in (ROOT / 'docs/history/visual-stages/chapter01-visual-passes/chapter01-production/internal').glob('*.png')}
(OUTPUT / 'internal_capture_hashes.json').write_text(json.dumps(captures, indent=2), encoding='utf8')
print(json.dumps(results, indent=2))
