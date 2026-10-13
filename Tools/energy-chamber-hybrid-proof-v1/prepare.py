"""Inspect only named CURRENT donors. Archives and prior imports stay untouched."""
from pathlib import Path
import zipfile, hashlib, json
ROOT=Path(__file__).resolve().parents[2]
OWNER=ROOT.parents[1]
PASS=ROOT/'docs/history/implementation-passes/energy-chamber-hybrid-proof-v1'
SCRATCH=ROOT/'ExternalAssetIntake/Current/energy-chamber-inspection'
PASS.mkdir(parents=True,exist_ok=True);SCRATCH.mkdir(parents=True,exist_ok=True)
choices={'quaternius-modular-scifi-megakit-standard':['Platform_Round1','Platform_Metal_Curve','Platform_Stairs_4','Prop_Rail_Round_Big','Prop_Rail_4','Column_Pipes','Prop_PipeHolder','Prop_Cable_3'],
         'molten-maps-scifi':['Generator','Generator Pile Large','Centrifuge','Cryo Tube ON','Cryo Tube OFF','Wall Pipe']}
rows=[]
for family,names in choices.items():
    archive=next((OWNER/'ExternalAssetIntake/Current'/family).glob('*.zip'))
    archive_hash=hashlib.sha256(archive.read_bytes()).hexdigest()
    with zipfile.ZipFile(archive) as z:
        for entry in z.infolist():
            p=Path(entry.filename)
            selected=p.stem in names and p.suffix.lower()=='.fbx' and ('/FBX/' in entry.filename or '/Assets/fbx/' in entry.filename)
            license_file='license' in p.name.lower() and not entry.is_dir()
            if not(selected or license_file):continue
            data=z.read(entry);target=SCRATCH/family/p.name;target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(data)
            rows.append(dict(package=family,archive=str(archive),archiveSha256=archive_hash,entry=entry.filename,sha256=hashlib.sha256(data).hexdigest(),inspectionPath=str(target.relative_to(ROOT)),licenseEvidence=license_file))
            if license_file:print(family,p.name,data.decode('utf-8',errors='replace'))
(PASS/'intake-candidates.json').write_text(json.dumps(rows,indent=2),encoding='utf-8')
print('NARROW_INTAKE_CANDIDATES',len(rows))
