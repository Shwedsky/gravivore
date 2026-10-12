"""Validate public audit metadata; optionally verify the owner's local payloads.

Python 3 standard library only. Does not extract archives or execute vendor code.
Usage: python VALIDATE_MANIFESTS.py [--local-root PATH] [--output PATH]
"""
import argparse
import collections
import hashlib
import json
import pathlib
import re
import subprocess


def safe_relative(value):
    value = value.replace('\\', '/')
    path = pathlib.PurePosixPath(value)
    return bool(value) and not path.is_absolute() and '..' not in path.parts and not any(':' in p for p in path.parts) and '\x00' not in value


def sha256(path):
    digest = hashlib.sha256()
    with path.open('rb') as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b''):
            digest.update(chunk)
    return digest.hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--local-root', type=pathlib.Path)
    parser.add_argument('--output', type=pathlib.Path)
    args = parser.parse_args()
    base = pathlib.Path(__file__).resolve().parent
    read = lambda name: json.loads((base / name).read_text(encoding='utf8'))
    archive = read('ARCHIVE_MANIFEST.json')
    records = archive['records']
    models = read('MODEL_METRICS.json')
    textures = read('TEXTURE_INVENTORY.json')
    licenses = read('SOURCE_LICENSES.json')
    copies = read('COPY_MANIFEST.json')
    nested = read('NESTED_ARCHIVES.json')
    checks = []

    def check(label, condition, detail):
        checks.append({'check': label, 'passed': bool(condition), 'detail': detail})
        if not condition:
            raise AssertionError(label + ': ' + str(detail))

    good = ['Assets/model.fbx', 'source/vehicle.blend', 'Folder\\Textures\\map.png']
    bad = ['../escape', 'Assets/../../escape', '/absolute', 'C:\\absolute', '//server/share', 'Assets/evil:stream', 'nul\x00byte']
    check('path_guard_examples', all(safe_relative(p) for p in good) and all(not safe_relative(p) for p in bad), '3 accepted and 7 rejected examples')
    ids = {r['source_id'] for r in records}
    warehouse = [r for r in records if r['origin'] == 'warehouse']
    retained = [r for r in records if r['curation'] == 'retained-existing']
    copied = [r for r in records if r['curation'] == 'copied']
    rejected = [r for r in records if r['curation'] == 'rejected']
    check('unique_sources_and_coverage', len(records) == len(ids) == len(licenses) == len(textures) == 30 and len(warehouse) == archive['warehouse_file_count'] == 24, '24 warehouse + 6 pre-existing archives; 30 unique source/licence/texture entries')
    check('curation_partition', len(copied) == len(copies) == 19 and len(rejected) == 5 and len(retained) == 6, '19 copied; 5 rejected; 6 retained')
    check('source_associations', ids == {r['source_id'] for r in licenses} == {r['source_id'] for r in textures}, 'Every archive has licence and texture records')
    check('warehouse_formats', collections.Counter(pathlib.Path(r['original_path']).suffix.lower() for r in warehouse) == {'.zip': 9, '.unitypackage': 15}, '9 ZIP, 15 UnityPackage')
    count = 0
    for r in records:
        check_paths = [r['original_path']] + [m['path'] for m in r['members']]
        if r['canonical_path']:
            check_paths.append(r['canonical_path'])
        assert all(safe_relative(p) for p in check_paths), r['source_id']
        assert r['member_count'] == len(r['members']), r['source_id']
        assert collections.Counter(pathlib.Path(m['path']).suffix.lower() or '<none>' for m in r['members']) == r['extensions'], r['source_id']
        assert not r['errors'], r['source_id']
        assert re.fullmatch('[0-9a-f]{64}', r['sha256']), r['source_id']
        count += len(r['members'])
    check('archive_paths_counts_hashes', True, f'{count} member records safe; extension counts agree; no archive inspection errors')
    check('nested_paths', len(nested) == 7 and all(n['parent'] in ids and safe_relative(n['path']) and all(safe_relative(p) for p in n['members']) and n.get('listing_exit_code', 0) == 0 for n in nested), '2 embedded Unity packages and 5 RAR listings; all paths safe')
    lookup = {r['source_id']: r for r in records}
    check('copy_manifest_associations', all(c['source_id'] in lookup and c['source'] == lookup[c['source_id']]['original_path'] and c['destination'] == lookup[c['source_id']]['canonical_path'] and c['sha256'] == lookup[c['source_id']]['sha256'] for c in copies), 'All source/destination paths and hashes match archive records')
    pairs = {(m['source_id'], m['path']) for m in models}
    check('model_measurement_coverage', len(models) == len(pairs) == 78 and collections.Counter(m['status'] for m in models) == {'ok': 53, 'ascii_metrics_ok': 25}, '53 Blender measurements; 25 static ASCII-parser measurements; no failed rows')
    for m in models:
        assert m['source_id'] in ids and m['path'] in lookup[m['source_id']]['models']
        assert m['donor_grade'] in {'A', 'B', 'C', 'D'}
        assert m['mesh_count'] == len(m['mesh_objects']) > 0
        assert m['triangles'] == sum(x['triangles'] for x in m['mesh_objects']) > 0
        assert all(r['bones'] == len(r['bone_names']) for r in m['rigs'])
        assert all(c['frames'][0] <= c['frames'][1] for c in m['animation_clips'])
        assert all(safe_relative(i['path']) for i in m['resolved_texture_images'] + m['additional_texture_candidates'])
        assert re.fullmatch('[0-9a-f]{64}', m['sha256'])
    check('model_geometry_rig_action_integrity', True, '78 geometry totals, source-member links, rig counts, frame ranges and donor grades validated')
    check('vehicle_motion_not_invented', all(not m['rigs'] and not m['animation_clips'] for m in models if m['source_id'].startswith('unityfan')), 'All four UnityFan vehicles have no rig or clips')
    check('raw_restrictions_recorded', all(not r['public_raw_redistribution_cleared'] for r in licenses if r['local_product_id']) and all(not r['public_raw_redistribution_cleared'] for r in licenses if r['source_id'].startswith('quaternius')), 'Unity raw sources and conflicting Quaternius notices conservatively stay local')
    if args.local_root:
        root = args.local_root.resolve()
        actual = {str(p.relative_to(root)).replace('\\', '/') for p in (root/archive['warehouse']).rglob('*') if p.is_file()}
        check('local_warehouse_exact_coverage', actual == {r['original_path'] for r in warehouse}, 'All actual warehouse files present, none omitted or invented')
        for r in records:
            assert sha256(root/r['original_path']) == r['sha256'], r['original_path']
        check('original_and_retained_hashes', True, 'All 30 original/retained archive SHA-256 hashes unchanged, including both active world payloads')
        for c in copies:
            assert sha256(root/c['destination']) == c['sha256'], c['destination']
        check('local_copy_hashes', True, 'All 19 canonical copies byte-identical to original warehouse payloads')
        scratch = root/'ExternalAssetIntake/Current/_actor-audit-v1'
        for m in models:
            assert sha256(scratch/m['source_id']/m['path']) == m['sha256'], m['path']
        for n in nested:
            assert sha256(scratch/n['parent']/n['path']) == n['sha256'], n['path']
        check('measured_source_hashes', True, '78 measured model sources + 7 nested archives match recorded hashes; no saved model changes')
        raw_paths = sorted({r['original_path'] for r in records if r['origin'] != 'warehouse'} | {c['destination'] for c in copies})
        proc = subprocess.run(['git', 'check-ignore', '-z', '--stdin'], cwd=root, input='\x00'.join(raw_paths)+'\x00', text=True, capture_output=True)
        check('canonical_raw_gitignored', proc.returncode == 0 and set(proc.stdout.rstrip('\x00').split('\x00')) == set(raw_paths), f'All {len(raw_paths)} canonical payload paths are ignored')
    summary = {'date': '2026-10-10', 'result': 'passed', 'checks': checks,
               'totals': {'warehouse_payloads': 24, 'new_canonical_copies': 19, 'rejected': 5, 'retained': 6, 'model_rows': 78, 'archive_member_records': count},
               'unity_compile': 'not run: audit/docs-only; no production import and parallel Unity session',
               'gameplay_tests': 'not run: no runtime changes', 'project_scene_validation': 'not run: no scene changes; audit manifest validation executed instead',
               'android_build': 'not run: no production import; no APK',
               'device_visual_performance_gate': 'not run; donor grade does not certify production art or performance'}
    if args.output:
        args.output.write_text(json.dumps(summary, indent=2)+'\n', encoding='utf8')
    print(json.dumps(summary, indent=2))


if __name__ == '__main__':
    main()
