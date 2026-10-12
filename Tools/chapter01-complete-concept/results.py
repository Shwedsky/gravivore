"""Summarize executed checks; never replace a missing run with an assumed pass."""
from pathlib import Path
import xml.etree.ElementTree as ET
import json,shutil
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'docs/history/implementation-passes/chapter01-complete-concept/verification'
records=[]
for name in ['EditModeFinal','PlayModeFinal']:
    path=OUT/(name+'.xml')
    root=ET.parse(path).getroot()
    record=dict(run=name,total=int(root.attrib['total']),passed=int(root.attrib['passed']),failed=int(root.attrib['failed']),skipped=int(root.attrib['skipped']),seconds=float(root.attrib['duration'].replace(',','.')),
                failures=[dict(name=t.attrib['fullname'],message=t.findtext('failure/message')) for t in root.iter('test-case') if t.attrib['result']=='Failed'],
                skippedTests=[t.attrib['fullname'] for t in root.iter('test-case') if t.attrib['result']=='Skipped'])
    records.append(record)
    if record['failed']:raise RuntimeError('Final regression failed: '+json.dumps(record,ensure_ascii=False))
for name in ['CompileFinal','ProjectValidator']:
    path=OUT/(name+'.log');content=path.read_text(encoding='utf-8',errors='replace')
    assert 'Application will terminate with return code 0' in content,name+' successful exit missing'
    assert 'error CS' not in content and 'executeMethod method' not in content,name+' source/validation error'
    records.append(dict(run=name,passed=True,log=path.name))
source=ROOT/'docs/history/implementation-passes/chapter01-r2-actors-v2-integration'
copies={source/'full-chapter/verification/five-minute-soak.json':'chapter-five-minute-soak.json',
        source/'verification/route-arena-clearance.json':'actor-route-arena-clearance.json',
        source/'verification/actor-traversal.txt':'actor-traversal.txt',
        ROOT/'docs/history/implementation-passes/chapter01-blueprint-world-r2/verification/world-audit.json':'world-audit.json'}
for src,dst in copies.items():
    if not (OUT/dst).exists():shutil.copyfile(src,OUT/dst)
soak=ROOT/'docs/history/visual-stages/chapter01-visual-passes/concept-fidelity-v2'
for path in soak.rglob('*.json'):
    if 'sustained' in path.name or 'runtime' in path.name.lower():
        target=OUT/('retained-'+path.name)
        if not target.exists():shutil.copyfile(path,target)
visibility=json.loads((OUT/'player-silhouette.json').read_text(encoding='utf-8'))
records.append(dict(run='productionCameraPlayerSilhouette',samples=len(visibility['samples']),minimumVisibleFraction=min(v['fraction'] for v in visibility['samples'])))
(OUT/'results.json').write_text(json.dumps(dict(executed=records),indent=2,ensure_ascii=False),encoding='utf-8')
print(json.dumps(records,indent=2,ensure_ascii=False))
