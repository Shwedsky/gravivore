from pathlib import Path
import json,xml.etree.ElementTree as ET
root=Path(__file__).resolve().parents[2];out=root/'docs/history/implementation-passes/actors-combat-concept-world/verification'
executed=[]
accepted={}
for name in ['EditModeFinal','PlayModeAccepted','FinalRouteRetry']:
    path=out/(name+'.xml')
    if not path.exists():continue
    node=ET.parse(path).getroot()
    executed.append(dict(run=name,total=int(node.get('total')),passed=int(node.get('passed')),failed=int(node.get('failed')),skipped=int(node.get('skipped')),seconds=float(node.get('duration').replace(',','.')),failures=[dict(name=n.get('fullname'),message=n.findtext('failure/message')) for n in node.findall('.//test-case') if n.get('result')=='Failed'],skippedTests=[n.get('fullname') for n in node.findall('.//test-case') if n.get('result')=='Skipped']))
    if name!='EditModeFinal':
        for test in node.findall('.//test-case'): accepted[test.get('fullname')]=test.get('result')
closed=dict(total=len(accepted),passed=sum(v=='Passed' for v in accepted.values()),failed=sum(v=='Failed' for v in accepted.values()),skipped=sum(v=='Skipped' for v in accepted.values()),
            method='Full PlayMode suite followed by targeted execution of both corrected route fixtures. This combines actually executed results; the original full-run XML retains its failures.')
(out/'results.json').write_text(json.dumps({'executed':executed,'closedPlayModeRegression':closed},indent=2))
print(json.dumps(executed,indent=2))
print(json.dumps(closed,indent=2))
