"""Immutable discovery and bounded local extraction. Never imports/executes payload code.

Python 3.11+ stdlib; Pillow is used only for image dimensions. Run from any cwd.
Archives are untrusted: reject traversal, links, special files, ADS, case collisions
with different content, oversized files and excessive expansion. Existing extraction
is reused only when its completed receipt matches the immutable input SHA-256.
"""
from __future__ import annotations
import argparse
import collections
import gzip
import hashlib
import json
import re
import shutil
import stat
import struct
import tarfile
import zipfile
from pathlib import Path, PurePosixPath

ROOT = Path(__file__).resolve().parents[2]
WORK = ROOT / 'ExternalAssetIntake/FreeAssetIntakeV1/_work_v2'
ARCHIVES = {'.unitypackage', '.zip', '.rar', '.7z', '.tar', '.gz'}
ART = {'.fbx', '.obj', '.mtl', '.glb', '.gltf', '.bin', '.blend', '.png', '.jpg', '.jpeg', '.psd', '.fig', '.tga', '.dds', '.tif', '.tiff', '.exr', '.hdr', '.bmp', '.webp', '.asset', '.prefab', '.mat', '.anim', '.controller', '.shader', '.shadergraph', '.shadersubgraph', '.meta'}
CODE = {'.cs', '.dll', '.exe', '.bat', '.cmd', '.ps1', '.sh', '.py', '.js', '.so', '.dylib', '.bundle', '.a', '.jar', '.aar', '.asmdef', '.asmref', '.compute', '.hlsl', '.cginc'}
MAX_FILE = 2 * 1024**3
MAX_TOTAL = 30 * 1024**3
MAX_MEMBERS = 100000

def digest(path):
    with Path(path).open('rb') as f:
        return hashlib.file_digest(f, 'sha256').hexdigest()

def write_json(path, data):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')

def safe_path(root, name):
    name = name.replace('\\', '/')
    p = PurePosixPath(name)
    if p.is_absolute() or not p.parts or any(x in {'..', '.'} or ':' in x or x.endswith((' ', '.')) for x in p.parts):
        raise ValueError('Unsafe archive path: ' + name)
    if any(re.fullmatch(r'(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(\..*)?', x, re.I) for x in p.parts):
        raise ValueError('Windows reserved path: ' + name)
    target = root.joinpath(*p.parts)
    if not target.resolve().is_relative_to(root.resolve()):
        raise ValueError('Archive escaped staging: ' + name)
    return target

def copy_member(stream, target, size):
    if size > MAX_FILE:
        raise ValueError('Archive member exceeds size limit')
    target.parent.mkdir(parents=True, exist_ok=True)
    # Duplicate Sketchfab texture entries are legitimate only if byte-identical.
    if target.exists():
        incoming = hashlib.sha256()
        count = 0
        while chunk := stream.read(1024 * 1024):
            incoming.update(chunk)
            count += len(chunk)
        if count != size or incoming.hexdigest() != digest(target):
            raise ValueError('Conflicting duplicate member: ' + str(target))
        return
    count = 0
    with target.open('wb') as dst:
        while chunk := stream.read(1024 * 1024):
            count += len(chunk)
            if count > size:
                raise ValueError('Member size mismatch')
            dst.write(chunk)
    if count != size:
        raise ValueError('Truncated member')

def extract_archive(path, dest, preserve_conflicts=False):
    total = count = 0
    seen = {}
    def check(name, size):
        nonlocal total, count
        total += size
        count += 1
        if total > MAX_TOTAL or count > MAX_MEMBERS:
            raise ValueError('Archive expansion limit exceeded')
        target = safe_path(dest, name)
        key = str(target).casefold()
        if key in seen and seen[key] != str(target):
            raise ValueError('Case-insensitive path collision')
        seen[key] = str(target)
        return target
    if path.suffix.lower() == '.zip':
        with zipfile.ZipFile(path) as z:
            for m in z.infolist():
                target = check(m.filename.rstrip('/'), m.file_size)
                mode = (m.external_attr >> 16) & 0xffff
                if stat.S_ISLNK(mode):
                    raise ValueError('Archive link rejected')
                if not m.is_dir():
                    with z.open(m) as src:
                        try:
                            copy_member(src, target, m.file_size)
                        except ValueError as error:
                            if not preserve_conflicts or not str(error).startswith('Conflicting duplicate member:'):
                                raise
                            # Some exporter ZIPs contain distinct textures at the same path.
                            # Preserve all versions; never overwrite or silently pick the last.
                            duplicate = safe_path(dest / '_duplicate_members' / f'{count:05d}', m.filename)
                            with z.open(m) as duplicate_src:
                                copy_member(duplicate_src, duplicate, m.file_size)
    elif path.suffix.lower() in {'.unitypackage', '.gz', '.tar'}:
        # gzip handles Unity's FEXTRA metadata correctly; tarfile r|gz does not.
        source = gzip.open(path, 'rb') if path.suffix.lower() != '.tar' else path.open('rb')
        with source, tarfile.open(fileobj=source, mode='r|') as t:
            for m in t:
                target = check(m.name.rstrip('/'), m.size)
                if m.isdir():
                    continue
                if not m.isfile():
                    raise ValueError('Archive link/special file rejected')
                with t.extractfile(m) as src:
                    copy_member(src, target, m.size)
    else:
        raise ValueError('Recognized archive requires available vetted extractor: ' + path.suffix)
    return {'archiveMembers': count, 'expandedBytes': total}

def unity_header(path):
    with path.open('rb') as f:
        header = f.read(10)
        if header[:2] != b'\x1f\x8b' or not header[3] & 4:
            return {}
        extra = f.read(struct.unpack('<H', f.read(2))[0])
    # Unity gzip extra subfield A$ contains store link/package metadata.
    try:
        start = extra.index(b'{')
        return json.loads(extra[start:].decode('utf-8').rstrip('\x00'))
    except (ValueError, UnicodeDecodeError):
        return {'rawExtraHeader': extra.hex()}

def original_files():
    inputs = [ROOT / 'ExternalAssetIntake/FreeAssetIntakeV1', ROOT / '98_unclassified', ROOT / '00_reference']
    return sorted({p for d in inputs if d.exists() for p in d.rglob('*') if p.is_file() and not p.is_symlink() and p.name not in {'.gitkeep', 'DROP_HERE.md'} and not {'_work_v2', '_local_reports'} & set(p.parts)})

def discover():
    files = original_files()
    payloads = []
    covered = set()
    # Already extracted Unity roots are one payload, irrespective of archive absence.
    folders = sorted({p.parent for p in files if p.name == 'package.json'} | {p.parent for p in files if p.parent.name in {'Assets', 'Packages'}}, key=lambda x: len(x.parts))
    for folder in folders:
        if any(folder.is_relative_to(x) for x in covered):
            continue
        members = [p for p in files if p.is_relative_to(folder)]
        if not any(p.suffix.lower() in ART for p in members):
            continue
        covered.add(folder)
        payloads.append({'path': str(folder.relative_to(ROOT)), 'kind': 'UNITY_FOLDER', 'files': len(members), 'bytes': sum(p.stat().st_size for p in members)})
    for p in files:
        if any(p.is_relative_to(x) for x in covered):
            continue
        if p.suffix.lower() in ARCHIVES | ART:
            payloads.append({'path': str(p.relative_to(ROOT)), 'kind': 'ARCHIVE' if p.suffix.lower() in ARCHIVES else 'LOOSE_ART', 'bytes': p.stat().st_size, 'sha256': digest(p)})
    write_json(WORK / 'reports/discovered.json', payloads)
    return payloads

def extract_all():
    payloads = discover()
    for i, payload in enumerate(payloads):
        src = ROOT / payload['path']
        key = f'p{i:02d}'
        dest = WORK / 'extracted' / key
        receipt = WORK / 'reports' / (key + '_extraction.json')
        payload['payloadId'] = key
        if payload['kind'] == 'LOOSE_ART':
            payload['contentRoot'] = str(src.relative_to(ROOT))
            continue
        if receipt.exists() and json.loads(receipt.read_text())['input'] == payload:
            print(key, 'reuse', src.name, flush=True)
            continue
        dest.mkdir(parents=True, exist_ok=True)
        if payload['kind'] == 'UNITY_FOLDER':
            for p in src.rglob('*'):
                if p.is_symlink():
                    raise ValueError('Input symlink rejected')
                if p.is_file():
                    target = safe_path(dest, p.relative_to(src).as_posix())
                    with p.open('rb') as f:
                        copy_member(f, target, p.stat().st_size)
            stats = {'folderCopy': True}
        else:
            stats = extract_archive(src, dest / ('guid_payload' if src.suffix.lower() == '.unitypackage' else 'content'), preserve_conflicts=src.name == 'techlab-modular-scifi-pipes.zip')
        content = dest / 'content'
        metadata = unity_header(src) if src.suffix.lower() == '.unitypackage' else {}
        if src.suffix.lower() == '.unitypackage':
            for pathname in (dest / 'guid_payload').rglob('pathname'):
                # Unity pathname records may append a newline plus serialization flags.
                logical = pathname.read_text(encoding='utf-8-sig').splitlines()[0].rstrip('\x00')
                asset = pathname.parent / 'asset'
                if not asset.is_file():
                    continue
                target = safe_path(content, logical)
                with asset.open('rb') as f:
                    copy_member(f, target, asset.stat().st_size)
                meta = pathname.parent / 'asset.meta'
                if meta.exists():
                    with meta.open('rb') as f:
                        copy_member(f, Path(str(target) + '.meta'), meta.stat().st_size)
        elif payload['kind'] == 'UNITY_FOLDER':
            content = dest
        write_json(receipt, {'input': payload.copy(), 'storeMetadata': metadata, **stats})
        print(key, 'extracted', src.name, stats, metadata, flush=True)
    # Rebuild contentRoot even for receipts reused on subsequent runs.
    for p in payloads:
        if p['kind'] != 'LOOSE_ART':
            p['contentRoot'] = str((WORK / 'extracted' / p['payloadId'] / ('content' if p['kind'] == 'ARCHIVE' else '')).relative_to(ROOT))
    write_json(WORK / 'reports/discovered.json', payloads)

def inspect():
    from PIL import Image
    result = []
    for payload in json.loads((WORK / 'reports/discovered.json').read_text(encoding='utf-8')):
        base = ROOT / payload['contentRoot']
        files = sorted(base.rglob('*')) if base.is_dir() else [base]
        files = [p for p in files if p.is_file()]
        counts = collections.Counter(p.suffix.lower() for p in files)
        texts = []
        images = []
        yaml = []
        for p in files:
            rel = str(p.relative_to(base)) if base.is_dir() else p.name
            if re.search(r'license|readme|copyright|credit|ofl|package.json', p.name, re.I) and p.suffix.lower() in {'.txt', '.md', '.json', '.html', '.pdf'}:
                if p.suffix.lower() != '.pdf':
                    texts.append({'path': rel, 'text': p.read_text(encoding='utf-8-sig', errors='replace')[:12000]})
                else:
                    texts.append({'path': rel, 'text': 'PDF; inspect separately'})
            if p.suffix.lower() in {'.png', '.jpg', '.jpeg', '.tga', '.psd', '.tif', '.tiff', '.dds'}:
                try:
                    with Image.open(p) as im:
                        images.append({'path': rel, 'width': im.width, 'height': im.height})
                except (OSError, ValueError) as error:
                    images.append({'path': rel, 'dimensionError': str(error)})
            if p.suffix.lower() in {'.prefab', '.mat', '.asset', '.anim', '.controller', '.shader', '.shadergraph'}:
                if p.stat().st_size > 100 * 1024**2:
                    continue
                text = p.read_text(encoding='utf-8-sig', errors='replace')
                classes = collections.Counter(re.findall(r'^--- !u!(\d+)', text, re.M))
                yaml.append({'path': rel, 'classes': dict(classes), 'particleMax': re.findall(r'maxNumParticles:\s*(\d+)', text), 'shaderGuids': re.findall(r'm_Shader:.*?guid:\s*([0-9a-f]+)', text), 'scripts': re.findall(r'm_Script:.*?guid:\s*([0-9a-f]+)', text), 'lod': 'LODGroup:' in text})
        entry = {**payload, 'extensionCounts': dict(counts), 'licensesAndReadmes': texts, 'images': images, 'yamlAssets': yaml,
                 'models': [str(p.relative_to(base)) for p in files if p.suffix.lower() in {'.fbx', '.obj', '.glb', '.gltf', '.blend'}],
                 'quarantinedCode': [str(p.relative_to(base)) for p in files if p.suffix.lower() in CODE or ('Plugins' in p.parts and p.suffix.lower() not in ART)],
                 'customDependencies': [str(p.relative_to(base)) for p in files if p.name in {'package.json', 'manifest.json', 'packages-lock.json'}]}
        receipt = WORK / 'reports' / (payload['payloadId'] + '_extraction.json')
        entry['storeMetadata'] = json.loads(receipt.read_text())['storeMetadata'] if receipt.exists() else {}
        result.append(entry)
        print(payload['payloadId'], Path(payload['path']).name, dict(counts), 'CODE', len(entry['quarantinedCode']), flush=True)
    write_json(WORK / 'reports/inspection.json', result)

def verify_originals():
    original = json.loads((WORK / 'reports/original_snapshot.json').read_text(encoding='utf-8'))
    current = {str(p.relative_to(ROOT)): p for p in original_files()}
    expected = {x['path'] for x in original}
    if set(current) != expected:
        raise ValueError('Original file set changed')
    for item in original:
        p = current[item['path']]
        if p.stat().st_size != item['bytes'] or digest(p) != item['sha256']:
            raise ValueError('Original mutated: ' + item['path'])
    print('PASS immutable original files:', len(original))

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('action', choices=['discover', 'extract', 'inspect', 'verify'])
    action = parser.parse_args().action
    {'discover': discover, 'extract': extract_all, 'inspect': inspect, 'verify': verify_originals}[action]()
