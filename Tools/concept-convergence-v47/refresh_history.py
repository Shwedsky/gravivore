"""Relocate branch delivery evidence without changing recorded evidence bytes."""
from pathlib import Path
import hashlib
import json
import shutil

ROOT = Path(__file__).resolve().parents[2]
HISTORY = 'docs/history/implementation-passes/chapter01-visual-replacement-v3'
MAPPING = {
    'docs/visual-replacement-v3': f'{HISTORY}/v44',
    'docs/concept-corrective-v45': f'{HISTORY}/v45',
    'docs/surface-hero-v46': f'{HISTORY}/v46',
}
REPORTS = {
    'docs/CHAPTER01_VISUAL_REPLACEMENT_V3.md': f'{HISTORY}/v44/DELIVERY.md',
    'docs/CHAPTER01_CONCEPT_FIDELITY_CORRECTIVE_V45.md': f'{HISTORY}/v45/DELIVERY.md',
    'docs/CHAPTER01_SURFACE_HERO_PROP_V46.md': f'{HISTORY}/v46/DELIVERY.md',
}
receipts = []
for old, new in {**MAPPING, **REPORTS}.items():
    src, dst = ROOT / old, ROOT / new
    if not src.exists():
        continue
    dst.parent.mkdir(parents=True, exist_ok=True)
    assert not dst.exists(), f'Never overwrite evidence: {dst}'
    files = sorted(src.rglob('*')) if src.is_dir() else [src]
    before = {str(p.relative_to(src)) if src.is_dir() else '.': hashlib.sha256(p.read_bytes()).hexdigest() for p in files if p.is_file()}
    shutil.move(str(src), str(dst))
    for rel, digest in before.items():
        target = dst if rel == '.' else dst / rel
        assert hashlib.sha256(target.read_bytes()).hexdigest() == digest
    receipts.append({'old': old, 'new': new, 'preservedFiles': len(before), 'sha256Verified': True})

updated = []
for folder in ('Assets', 'Tools'):
    for p in (ROOT / folder).rglob('*'):
        if p.suffix not in ('.cs', '.py', '.ps1', '.md') or p == Path(__file__).resolve():
            continue
        original = p.read_text(encoding='utf-8-sig')
        lines = []
        for line in original.splitlines(keepends=True):
            # A git-show path belongs to its historical commit, not this checkout.
            if "git('show'" not in line and "'git','show'" not in line:
                for old, new in {**REPORTS, **MAPPING}.items():
                    line = line.replace(old, new)
            lines.append(line)
        revised = ''.join(lines)
        if revised != original:
            p.write_text(revised, encoding='utf-8', newline='')
            updated.append(p.relative_to(ROOT).as_posix())

out = ROOT / HISTORY / 'v47'
out.mkdir(parents=True, exist_ok=True)
(out / 'authority_refresh.json').write_text(json.dumps({
    'synchronizedMain': '81de953e9df2f0a09d03035d0809a00186d34630',
    'mergeCommit': '863757da38689eb02b4e0d4cb251ace2d5ff80ab',
    'mergeConflicts': [], 'evidenceMoves': receipts, 'portableToolsUpdated': updated,
    'currentAuthoritiesReread': True, 'historyIsNonAuthoritative': True,
    'g0Bipedal': True, 'lowPolyIsOptimizationOnly': True,
    'productionSources': ['Assets/_Game/Content/', 'art/'],
    'newExternalIntake': 'ExternalAssetIntake/Current/'
}, indent=2), encoding='utf-8')
print(json.dumps({'preservedFiles': sum(r['preservedFiles'] for r in receipts), 'updatedTools': len(updated)}))
