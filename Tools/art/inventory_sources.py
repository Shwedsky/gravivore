"""Hash and safely extract user-supplied archives into ignored local scratch."""
import hashlib
import json
import zipfile
import subprocess
from collections import Counter
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
INTAKE = Path.home() / 'Documents' / 'GRAVIVORE_ASSET_INTAKE'
SCRATCH = ROOT / '.asset-intake-tmp'
OUT = ROOT / 'docs/visual-production-v2/modeling/data'
OUT.mkdir(parents=True, exist_ok=True)
records = []
for archive in sorted(INTAKE.rglob('*.zip')):
    digest = hashlib.file_digest(archive.open('rb'), 'sha256').hexdigest()
    destination = SCRATCH / archive.parent.name / archive.stem
    destination.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(archive) as source:
        bad = source.testzip()
        if bad:
            raise ValueError(f'Corrupt ZIP member: {bad}')
        for member in source.infolist():
            target = (destination / member.filename).resolve()
            if not target.is_relative_to(destination.resolve()):
                raise ValueError(f'Unsafe archive path: {member.filename}')
        source.extractall(destination)
        files = [m for m in source.infolist() if not m.is_dir()]
        record = dict(category=archive.parent.name, filename=archive.name,
                      bytes=archive.stat().st_size, sha256=digest,
                      formats=dict(Counter(Path(m.filename).suffix.lower() for m in files)),
                      entries=[dict(path=m.filename, bytes=m.file_size) for m in files],
                      extracted_relative=str(destination.relative_to(ROOT)).replace('\\', '/'))
        records.append(record)
        print(record['category'], record['filename'], record['bytes'], record['formats'], flush=True)
for record in records:
    destination = ROOT / record['extracted_relative']
    record['nested_archives'] = []
    for nested in list(destination.rglob('*.zip')) + list(destination.rglob('*.rar')):
        nested_out = nested.parent / (nested.stem + '_unpacked')
        nested_out.mkdir(exist_ok=True)
        if nested.suffix.lower() == '.zip':
            with zipfile.ZipFile(nested) as source:
                names = source.namelist()
                for name in names:
                    if not (nested_out / name).resolve().is_relative_to(nested_out.resolve()):
                        raise ValueError(name)
                source.extractall(nested_out)
        else:
            names = subprocess.check_output(['tar', '-tf', str(nested)], text=True).splitlines()
            for name in names:
                if not (nested_out / name).resolve().is_relative_to(nested_out.resolve()):
                    raise ValueError(name)
            subprocess.run(['tar', '-xf', str(nested), '-C', str(nested_out)], check=True)
        record['nested_archives'].append(dict(path=str(nested.relative_to(destination)),
            sha256=hashlib.file_digest(nested.open('rb'), 'sha256').hexdigest(), entries=names))
        print('NESTED', record['filename'], names[:15], flush=True)
(OUT / 'archive_inventory.json').write_text(json.dumps(records, indent=2), encoding='utf-8')
