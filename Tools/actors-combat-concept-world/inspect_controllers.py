"""Read the shipped native AnimatorController objects, without downloading a parser."""
from pathlib import Path
import json, struct, sys, re, zlib
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'Tools/render-hotfix'))
from serialized_apk import apk_files,objects,Reader
apk=Path(sys.argv[1])
for entry,data in apk_files(apk):
    for obj in objects(data):
        if obj['classId'] not in (91,319): continue
        r=Reader(obj['data']);name=r.string()
        if obj['classId']==91 and name in ('G0_Production_V3_Review','Magnetar_V2','Custodian_V2'):
            size=r.unpack('<I');layers=[]
            for _ in range(r.unpack('<I')):
                machine,motion=r.unpack('<2I');bodyMask=r.unpack('<3I')
                mask=[r.unpack('<If') for _ in range(r.unpack('<I'))]
                binding,blend,weight=r.unpack('<IIf');ik,sync=r.unpack('<2?');r.align()
                layers.append(dict(machine=machine,mask=mask,binding=binding,blend=blend,weight=weight,ik=ik,sync=sync))
            actor='G0' if name.startswith('G0') else name.removesuffix('_V2')
            source=ROOT/f'Assets/_Game/Content/ActorProductionV2/{actor}_MovingSupports.mask'
            paths=re.findall(r'- m_Path:[ \t]*(.*?)\n\s+m_Weight: ([\d.]+)',source.read_text())
            expected={zlib.crc32(path.encode()):float(weight) for path,weight in paths}
            actual=dict(layers[1]['mask']) if len(layers)>1 else {}
            print(json.dumps(dict(entry=entry,name=name,classId=91,bytes=len(obj['data']),layers=len(layers),supportDefaultWeight=layers[1]['weight'] if len(layers)>1 else None,skeletonMaskEntries=len(actual),
                 maskSourceMatches=actual==expected,activePaths=[path for path,weight in paths if float(weight)>0],
                 compiledGaitParameter=b'IndustrialGaitPhase' in obj['data'],compiledSupportLayer=b'Industrial moving supports' in obj['data'],compiledGaitState=b'Distance driven support gait' in obj['data'])))
        if obj['classId']==319: print('AvatarMask',name)
