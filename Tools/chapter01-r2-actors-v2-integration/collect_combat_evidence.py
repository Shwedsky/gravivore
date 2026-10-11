"""Promote fresh full-suite captures out of legacy test output paths, not legacy visual authority."""
import datetime, shutil, xml.etree.ElementTree as ET
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
BASE=ROOT/"docs/history/implementation-passes/chapter01-r2-actors-v2-integration"
run=ET.parse(BASE/"verification/PlayModeCandidate.xml").getroot()
assert int(run.attrib["failed"])==0
started=datetime.datetime.fromisoformat(run.attrib["start-time"].replace("Z","+00:00")).timestamp()
sources=["docs/history/visual-stages/chapter01-visual-passes/chapter01-production/internal",
    "docs/history/visual-stages/chapter01-visual-passes/chapter01-v3/internal",
    "docs/history/visual-stages/chapter01-visual-passes/post-device-combat-readability/internal"]
destination=BASE/"combat-hud";destination.mkdir(parents=True,exist_ok=True);records=[]
for source in sources:
    for path in sorted((ROOT/source).glob("*.png")):
        if path.stat().st_mtime<started:continue
        assert path.stat().st_size>1000
        target=destination/(path.parent.parent.name+"_"+path.name)
        shutil.copy2(path,target);records.append(str(path.relative_to(ROOT))+" => "+str(target.relative_to(ROOT)))
(BASE/"verification/combat-capture-origins.txt").write_text("\n".join(records)+"\n")
print("FRESH_COMBAT_HUD_CAPTURES",len(records))
