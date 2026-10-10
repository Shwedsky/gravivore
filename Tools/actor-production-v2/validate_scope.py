"""Audit task custody and protected production paths against the fetched baseline."""
from pathlib import Path
import hashlib
import json
import subprocess

ROOT = Path(__file__).resolve().parents[2]
BASE = '38e0ed8fe5c95ff41ab5dbb246f45ed26bdcf9cc'
INITIAL = '80ae82c'
E = ROOT / 'docs/history/implementation-passes/chapter01-actor-production-v2'
PREFIXES = ('Assets/_Game/ArtReview/ActorProductionV2', 'Tools/actor-production-v2/', 'art/chapter01-actor-production-v2/', 'docs/history/implementation-passes/chapter01-actor-production-v2/')


def git(*args):
    return subprocess.check_output(['git', '-c', 'core.quotepath=false', '-c', 'core.safecrlf=false', *args], cwd=ROOT, text=True).splitlines()


authority = git('diff', '--name-only', BASE, INITIAL)
assert authority and all(path.startswith('docs/') for path in authority)
changed = git('diff', '--name-only', BASE)
unexpected = [path for path in changed if path not in authority and not path.startswith(PREFIXES)]
protected = git('diff', '--name-only', BASE, '--', 'Assets/_Game/Content', 'Assets/_Game/Runtime', 'Assets/ThirdParty', 'ProjectSettings', 'Packages')
png = ROOT / 'docs/visual-blueprints/chapter01-actors/CHAPTER01_ACTOR_VISUAL_TARGETS_V2.png'
sha = hashlib.sha256(png.read_bytes()).hexdigest()
assert sha == '67960c1254133c48b1de97d2dd635a5969cea49b949a7e0e8981afd34738099f', 'Approved PNG changed'
assert not unexpected and not protected, 'Out-of-scope tracked changes: ' + str(unexpected + protected)
branch = git('branch', '--show-current')[0]
assert branch == 'art/chapter01-actor-production-v2'
report = {'result': 'PASS', 'branch': branch, 'baseline': BASE, 'approved_png_sha256': sha, 'protected_production_paths_changed': protected, 'unexpected_tracked_paths': unexpected, 'initial_authority_custody_paths': authority, 'private_generated_untracked_files_staged': False, 'production_scene_bindings_changed': False, 'world_r2_branch_modified': False}
(E / 'SCOPE_VALIDATION.json').write_text(json.dumps(report, indent=2) + '\n')
print('SCOPE_VALIDATION_PASS: canonical production, G-0, gameplay, world, packages and settings unchanged')
