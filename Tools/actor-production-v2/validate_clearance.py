"""Read-only rest-envelope audit against the unchanged main collision authoring.

This is a conservative static spawn/arena check, not an animation or route pass.
It never opens or saves a Unity scene, changes collision, or places actors in it.
"""
from pathlib import Path
import hashlib
import json
import math
import re

ROOT = Path(__file__).resolve().parents[2]
E = ROOT / 'docs/history/implementation-passes/chapter01-actor-production-v2'
D = ROOT / 'Assets/_Game/Content/Definitions'
SCENE = ROOT / 'Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity'
scene = SCENE.read_text(encoding='utf-8-sig')


def vector(text):
    return {axis: float(value) for axis, value in re.findall(r'([xyzw]):\s*([-+\d.eE]+)', text)}


def field(text, name):
    found = re.search(r'^\s*' + re.escape(name) + r':\s*(\{[^\n]+\})', text, re.M)
    if not found:
        raise ValueError('Missing vector: ' + name)
    return vector(found[1])


def number(text, name):
    return float(re.search(r'^\s*' + re.escape(name) + r':\s*([-+\d.eE]+)', text, re.M)[1])


# The baseline has unrotated, unscaled region roots. Fail loudly if that changes
# instead of silently treating authored collision boxes as world-axis aligned.
documents = {int(key): body for key, body in re.findall(r'--- !u!\d+ &(\d+)\n(.*?)(?=--- !u!|\Z)', scene, re.S)}


def position(transform_id):
    body = documents[transform_id]
    rotation = field(body, 'm_LocalRotation')
    scale = field(body, 'm_LocalScale')
    if any(abs(rotation.get(axis, 0)) > 1e-6 for axis in 'xyz') or abs(abs(rotation['w']) - 1) > 1e-6:
        raise ValueError('Static audit requires unrotated authored roots')
    if any(abs(scale[axis] - 1) > 1e-6 for axis in 'xyz'):
        raise ValueError('Static audit requires unit-scale authored roots')
    local = field(body, 'm_LocalPosition')
    parent = int(re.search(r'm_Father: \{fileID: (\d+)\}', body)[1])
    if parent:
        inherited = position(parent)
        local = {axis: local[axis] + inherited[axis] for axis in 'xyz'}
    return local


elite_id = int(re.search(r'_id: elite-arena\n\s*_enemyId: [^\n]*\n\s*_root: \{fileID: (\d+)\}', scene)[1])
elite_origin = position(elite_id)
obstacles = [{'name': name, 'center': vector(center), 'size': vector(size)} for name, center, size in re.findall(
    r'- _name: ([^\n]+)\n\s*_center: (\{[^\n]+\})\n\s*_size: (\{[^\n]+\})', scene)]
assert '_fullChapterProduction: 1' in scene and obstacles, 'Unexpected collision authoring'
for name, x, z in [('Elite Arena Left Containment', -4.6, .35), ('Elite Arena Right Containment', 4.6, -.2)]:
    obstacles.append({'name': name, 'center': {'x': elite_origin['x'] + x, 'y': elite_origin['y'] + .7, 'z': elite_origin['z'] + z}, 'size': {'x': 1.4, 'y': 1.4, 'z': 3}})
world = (D / 'S08_Chapter01World.asset').read_text()
for key in ['_eliteGate', '_bossGate']:
    block = re.search(re.escape(key) + r':\n(.*?)(?=\n  _|\Z)', world, re.S)[1]
    obstacles.append({'name': key + ' (closed)', 'center': field(block, '_position'), 'size': field(block, '_size')})
ground = field(world, '_groundCenter')
ground_size = field(world, '_groundSize')
arena_radius = number(world, '_bossArenaRadius')
arena_center = field(world, '_bossArenaCenter')
imported = {record['actor']: record for record in json.loads((E / 'UNITY_VALIDATION.json').read_text())['actors']}
actors = ['Scout', 'Cutter', 'Warden', 'ArcDrone', 'Carrier', 'Magnetar', 'Custodian']
spots = ['RelayYard', 'CuttingFloor', 'ShieldDump', 'CapacitorField', 'HaulerGraveyard']
records = []
for index, actor in enumerate(actors):
    source = json.loads((E / (actor + '_METRICS.json')).read_text())['lods'][0]
    lo, hi = source['min_xyz'], source['max_xyz']
    # Z-up source -> Y-up Unity: source XY is the ground plane. Include the
    # conservative imported renderer dimensions as a second, independent bound.
    radius = math.hypot(max(abs(lo[0]), abs(hi[0])), max(abs(lo[1]), abs(hi[1])))
    native = imported[actor]['renderDimensions']
    radius = max(radius, math.hypot(native['x'], native['z']) * .5)
    if index < 5:
        definition = (D / ('S04_SpawnSpot_' + spots[index] + '.asset')).read_text()
        origin = field(definition, '_worldOrigin')
        offset_block = re.search(r'_anchorOffsets:\n(.*?)(?=\n  _)', definition, re.S)[1]
        anchors = [{axis: origin[axis] + offset[axis] for axis in 'xyz'} for offset in map(vector, re.findall(r'\{[^\n]+\}', offset_block))]
    else:
        filename, key = ('S09_MagnetarGuard.asset', '_spawnPosition') if actor == 'Magnetar' else ('S09_CustodianM0.asset', '_startPosition')
        anchors = [field((D / filename).read_text(), key)]
    checks = []
    for anchor in anchors:
        candidates = []
        for obstacle in obstacles:
            center, size = obstacle['center'], obstacle['size']
            if center['y'] + size['y'] * .5 < lo[2] or center['y'] - size['y'] * .5 > hi[2]:
                continue
            dx = max(0, abs(anchor['x'] - center['x']) - size['x'] * .5)
            dz = max(0, abs(anchor['z'] - center['z']) - size['z'] * .5)
            candidates.append((math.hypot(dx, dz) - radius, obstacle['name']))
        nearest = min(candidates)
        boundary_margin = min(ground_size['x'] * .5 - abs(anchor['x'] - ground['x']), ground_size['y'] * .5 - abs(anchor['z'] - ground['z'])) - radius
        checks.append({'anchor': anchor, 'nearest_blocker': nearest[1], 'conservative_blocker_margin_m': round(nearest[0], 4), 'boundary_margin_m': round(boundary_margin, 4), 'clear': min(nearest[0], boundary_margin) >= 0})
    pair_margin = min((math.hypot(a['x'] - b['x'], a['z'] - b['z']) - 2 * radius for i, a in enumerate(anchors) for b in anchors[i + 1:]), default=None)
    record = {'actor': actor, 'rest_planar_bounding_radius_m': round(radius, 4), 'unchanged_gameplay_collision_radius_m': imported[actor]['collisionRadius'], 'presentation_exceeds_center_collider': radius > imported[actor]['collisionRadius'], 'spawn_checks': checks, 'minimum_spawn_pair_envelope_margin_m': None if pair_margin is None else round(pair_margin, 4), 'initial_placement_conservative_clear': all(c['clear'] for c in checks) and (pair_margin is None or pair_margin >= 0)}
    if actor == 'Custodian':
        anchor = anchors[0]
        record['boss_arena_rest_margin_m'] = round(arena_radius - radius - math.hypot(anchor['x'] - arena_center['x'], anchor['z'] - arena_center['z']), 4)
        record['initial_placement_conservative_clear'] &= record['boss_arena_rest_margin_m'] >= 0
    records.append(record)
report = {'method': 'Read-only serialized baseline collision boxes, closed gates, bounds and actual spawn anchors; conservative rest-envelope circles. Negative margins are potential overlap, not exact mesh intersection.', 'baseline_scene_sha256': hashlib.sha256(SCENE.read_bytes()).hexdigest(), 'blocker_count_including_gates': len(obstacles), 'actors': records, 'initial_rest_placement_conservative_clear': all(r['initial_placement_conservative_clear'] for r in records), 'animated_routes_turns_wall_clearance': 'Not certified. Center collision radii cannot guarantee outboard presentation clearance.', 'world_or_gameplay_modified': False}
(E / 'CLEARANCE_REVIEW.json').write_text(json.dumps(report, indent=2) + '\n')
for record in records:
    print(record['actor'], 'initial rest clearance:', record['initial_placement_conservative_clear'], 'radius:', record['rest_planar_bounding_radius_m'])
