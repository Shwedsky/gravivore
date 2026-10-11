"""Summarize final executed gates; keep raw NUnit XML and measured observations authoritative."""
import json, xml.etree.ElementTree as ET
from pathlib import Path
from collections import Counter
ROOT=Path(__file__).resolve().parents[2]
BASE=ROOT/"docs/history/implementation-passes/chapter01-r2-actors-v2-integration"
OUT=BASE/"verification"
tests={}
for label in ["EditModeCandidate","PlayModeCandidate","DamageAnchorRegression"]:
    node=ET.parse(OUT/(label+".xml")).getroot()
    tests[label]={k:node.attrib.get(k) for k in ["result","total","passed","failed","skipped","duration","start-time","end-time"]}
    tests[label]["nonPassingCases"]=[dict(name=c.attrib["fullname"],result=c.attrib["result"],reason="".join(c.find("reason").itertext()).strip() if c.find("reason") is not None else "")
        for c in node.iter("test-case") if c.attrib["result"]!="Passed"]
    assert int(node.attrib["failed"])==0, label+" has failures"
samples=[]
for filename in ["animated-clearance.json","route-arena-clearance.json"]:
    samples+=json.loads((OUT/filename).read_text())["samples"]
clearance=dict(samples=len(samples),perActor=dict(Counter(s["actor"] for s in samples)),
    penetratingVertices=sum(s["penetratingVertices"] for s in samples),minimumFloorHeight=min(s["minimumY"] for s in samples),
    perActorMinimumY={name:min(s["minimumY"] for s in samples if s["actor"]==name) for name in sorted({s["actor"] for s in samples})})
assert clearance["penetratingVertices"]==0
report=dict(tests=tests,clearance=clearance,editorPerformance=json.loads((OUT/"editor-performance.json").read_text()),
    fiveMinuteSoak=json.loads((BASE/"full-chapter/verification/five-minute-soak.json").read_text()),
    deviceAcceptance="NOT RUN: no connected Android device; final visual/device acceptance belongs to owner")
(OUT/"final-gates.json").write_text(json.dumps(report,indent=2)+"\n")
gallery=["# Portrait gameplay gallery","","Actual production camera, 540 × 960, UI disabled. Controlled actor fixtures are documented in the clearance report.",
    "No beauty camera or review-only scene is used. These are Editor captures, not Android screenshots.",""]
for folder in [BASE/"gallery",BASE/"full-chapter/internal/no-ui",BASE/"full-chapter/internal",BASE/"combat-hud"]:
    if not folder.exists():continue
    for path in sorted(folder.glob("*.png")):
        if "diagnostic_return" in path.name:continue
        assert path.stat().st_size>1000, str(path)+" is not a valid capture"
        relative=path.relative_to(BASE).as_posix()
        gallery.append(f"- [{path.stem}]({relative})")
(BASE/"GALLERY.md").write_text("\n".join(gallery)+"\n",encoding="utf-8")
print(json.dumps(report,indent=2))
