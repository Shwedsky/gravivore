"""Inspect delivered IL2CPP types and named serialized settings, using only stdlib.

Field layouts below are the Unity 6.3 serialized declaration order of the two
project-owned ScriptableObjects. Candidate objects must match their leading
pool/size fields before any tuned values are accepted. IL2CPP metadata is never
used as evidence for serialized meshes, which have separate build callbacks.
"""
import argparse
import json
import struct
import zipfile
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument('apk', type=Path)
parser.add_argument('--output', required=True, type=Path)
args = parser.parse_args()

def close(actual, expected):
    return abs(actual - expected) < .0001

def object_fields(data, name):
    needle = struct.pack('<i', len(name)) + name.encode()
    offset = 0
    while (offset := data.find(needle, offset)) >= 0:
        pos = (offset + len(needle) + 3) & ~3
        offset += len(needle)
        if pos + 4 > len(data):
            continue
        count = struct.unpack_from('<i', data, pos)[0]
        if not 0 <= count <= 1024 or pos + 4 + count > len(data):
            continue
        # m_EditorClassIdentifier follows m_Name (empty in some player builds).
        yield (pos + 4 + count + 3) & ~3

types = ['CharacteristicsPresenter', 'CharacteristicsReadModel', 'BossHealthHudPresenter',
         'EnemyCombatReadabilityPresenter', 'PresentationHitchDiagnostics',
         'PlayerAttackResolvedEvent', 'ConfigureCharacteristics', 'PreviewLevel', '_sourceGains']
settings = {}
with zipfile.ZipFile(args.apk) as apk:
    metadata = apk.read('assets/bin/Data/Managed/Metadata/global-metadata.dat')
    for name in types + ['ХАРАКТЕРИСТИКИ', 'ДОПУСК ПОЛУЧЕН']:
        if name.encode('utf8') not in metadata:
            raise RuntimeError('Missing compiled runtime content: ' + name)
    for entry in apk.infolist():
        if not entry.filename.startswith('assets/bin/Data/sharedassets') or '.resS' in entry.filename:
            continue
        data = apk.read(entry)
        for pos in object_fields(data, 'S14_Presentation'):
            if pos + 264 > len(data) or struct.unpack_from('<4i', data, pos) != (8, 6, 8, 2):
                continue
            gait_pos = pos + 16 + 28 + 80 + 96
            gait = struct.unpack_from('<7f', data, gait_pos)
            expected = (1.15, 540, 27, 42, .09, .6, .36)
            if not all(close(a, b) for a, b in zip(gait, expected)):
                continue
            gain = struct.unpack_from('<f', data, gait_pos + 28 + 12)[0]
            if not close(gain, .168):
                raise RuntimeError('Packed step gain differs from production source: ' + str(gain))
            settings['audio'] = {'entry': entry.filename, 'minimumStepInterval': gait[-1], 'stepGain': gain}
        for pos in object_fields(data, 'PostDevicePresentation'):
            if pos + 44 > len(data) or struct.unpack_from('<3i', data, pos) != (6, 10, 4):
                continue
            floats = struct.unpack_from('<8f', data, pos + 12)
            expected = (270, 98, 7, 2.5, .75, 1.1, .6, 2.25)
            if all(close(a, b) for a, b in zip(floats, expected)):
                settings['movement'] = {'entry': entry.filename, 'runCycleDistance': floats[-1],
                                        'healthPlates': 6, 'damageTextSlots': 10, 'rewardTextSlots': 4}
if set(settings) != {'audio', 'movement'}:
    raise RuntimeError('Named serialized production audio/movement settings not verified: ' + repr(settings))
evidence = {'validated': True, 'compiledRuntimeTypesAndMethods': types,
            'russianPanelStrings': True, 'serializedSettings': settings}
args.output.write_text(json.dumps(evidence, indent=2), encoding='utf8')
print(json.dumps(evidence, indent=2))
