"""Verify linked V3 runtime types and the actual packed weapon's serialized settings."""
import argparse, json, struct, zipfile
from pathlib import Path

parser=argparse.ArgumentParser()
parser.add_argument('apk',type=Path)
parser.add_argument('--output',required=True,type=Path)
args=parser.parse_args()
types=['EncounterBasicAttackCadence','AttackCausalityPresenter','WeaponEquipmentPresenter','WeaponEquipmentPanel',
       'ItemRankSaveDto','SaveMigrationV2ToV3','GrantRankedCopy','ModifierAtRank']
def read_string(data,pos):
    size=struct.unpack_from('<i',data,pos)[0]
    if not 0<=size<=2048:raise ValueError('Not a string')
    return data[pos+4:pos+4+size].decode('utf8'),(pos+4+size+3)&~3
weapon=None
with zipfile.ZipFile(args.apk) as apk:
    metadata=apk.read('assets/bin/Data/Managed/Metadata/global-metadata.dat')
    for name in types:
        if name.encode() not in metadata:raise RuntimeError('V3 runtime type/method missing: '+name)
    needle=struct.pack('<i',len('ImpulseEmitterM0'))+b'ImpulseEmitterM0'
    for entry in apk.infolist():
        if not entry.filename.startswith('assets/bin/Data/sharedassets') or '.resS' in entry.filename:continue
        data=apk.read(entry);offset=0
        while (offset:=data.find(needle,offset))>=0:
            pos=(offset+len(needle)+3)&~3;offset+=len(needle)
            for candidate in (pos,pos+4):
                try:
                    item_id,next_pos=read_string(data,candidate)
                    if item_id!='impulse-emitter-m0':continue
                    display,next_pos=read_string(data,next_pos)
                    slot,*modifiers=struct.unpack_from('<i5f',data,next_pos)
                    maximum_rank,per_rank=struct.unpack_from('<if',data,next_pos+24)
                    if slot!=3 or modifiers!=[12.0,0.0,0.0,0.0,0.0] or maximum_rank!=5 or per_rank!=4:continue
                    weapon={'entry':entry.filename,'id':item_id,'displayName':display,'slot':slot,'baseDamageBonus':modifiers[0],
                            'maximumRank':maximum_rank,'damagePerRank':per_rank}
                except (ValueError,UnicodeError,struct.error):continue
if weapon is None:raise RuntimeError('Named packed M-0 equipment definition not verified')
evidence={'validated':True,'compiledRuntimeTypesAndMethods':types,'serializedWeapon':weapon}
args.output.write_text(json.dumps(evidence,indent=2,ensure_ascii=False),encoding='utf8')
print(json.dumps(evidence,indent=2,ensure_ascii=False))
