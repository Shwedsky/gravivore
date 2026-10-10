"""Select CC0 donor bytes from the two owner-local canonical archives."""
from pathlib import Path
import zipfile, hashlib, json

ROOT = Path(__file__).resolve().parents[2]
Q = ['Column_Pipes','Column_MetalSupport','Column_Astra','Column_Round',
     'Door_Frame_A','Door_Frame_SquareTall','Door_DarkMetal','Platform_Metal2',
     'Platform_3Plates','Platform_Round1','Platform_Ramp_4','Platform_Rails_4',
     'WallAstra_Straight','WallAstra_Straight_Broken','WallBand_Straight',
     'BottomMetal_Straight','TopCables_Straight','ShortWall_MetalPlates_Straight',
     'Prop_Cable_3','Prop_PipeHolder','Prop_AccessPoint','Prop_Vent_Wide','Prop_Rail_4',
     'Prop_Crate4','Prop_Computer']
M = ['Generator','Generator Pile Large','Generator Pile Small','Cryo Tube ON',
     'Cryo Tube OFF','Centrifuge','Command Console','Wall Command','Corridor Large',
     'Corridor Small','Catwalk','Wall Pipe','Battery Orange','Battery Blue',
     'Floor Metal 1','Floor Mid Path 1','Floor Hazard 1']
rows=[]
for family, names in [('quaternius-modular-scifi-megakit-standard',Q),('molten-maps-scifi',M)]:
    source=ROOT/'ExternalAssetIntake/Current'/family
    archive=next(source.glob('*.zip'))
    archive_hash=hashlib.sha256(archive.read_bytes()).hexdigest()
    destination=source/'inspection'
    with zipfile.ZipFile(archive) as z:
        for entry in z.infolist():
            p=Path(entry.filename)
            take=(p.suffix.lower()=='.fbx' and p.stem in names and ('/FBX/' in entry.filename or '/Assets/fbx/' in entry.filename))
            take|=(p.suffix.lower()=='.png' and ('/Textures/' in entry.filename or '/Textures + Materials/' in entry.filename) and '/Unity Materials/' not in entry.filename)
            take|=('license' in p.name.lower() or p.suffix=='.url')
            if not take: continue
            output=destination/p.name
            output.parent.mkdir(parents=True,exist_ok=True)
            content=z.read(entry)
            output.write_bytes(content)
            if p.suffix.lower()=='.txt' and 'license' in p.name.lower():
                license_output=ROOT/'Assets/_Game/Content/BlueprintWorldR1/Licenses'/f'{family}.txt'
                license_output.parent.mkdir(parents=True,exist_ok=True);license_output.write_bytes(content)
            rows.append(dict(family=family,archive=archive.name,archiveSha256=archive_hash,entry=entry.filename,file=str(output.relative_to(ROOT)),sha256=hashlib.sha256(content).hexdigest(),license='CC0-1.0'))
    print(family, 'selected', len([r for r in rows if r['family']==family]))
output=ROOT/'docs/history/implementation-passes/chapter01-blueprint-world-r1'
output.mkdir(parents=True,exist_ok=True)
(output/'asset-intake.json').write_text(json.dumps(rows,indent=2),encoding='utf-8')
