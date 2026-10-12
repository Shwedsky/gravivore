"""Extract a narrow licensed subset from CURRENT; author repeatable PBR surfaces.

No downloaded/vendor code is executed. Authored UVs use metres, not a stretched
atlas rectangle on every face. Donor UVs and their runtime material maps survive.
"""
from pathlib import Path
import json, hashlib, zipfile
import numpy as np
from PIL import Image, ImageFilter

ROOT = Path(__file__).resolve().parents[2]
OWNER = ROOT.parents[1]
OUT = ROOT / 'Assets/_Game/Content/CapacitorFieldProof/Textures'
PASS = ROOT / 'docs/history/implementation-passes/capacitor-field-proof'
OUT.mkdir(parents=True, exist_ok=True)
PASS.mkdir(parents=True, exist_ok=True)
rows = []
selection = {'quaternius-modular-scifi-megakit-standard': ['Column_Pipes', 'Prop_PipeHolder', 'Prop_AccessPoint'],
             'molten-maps-scifi': ['Command Console']}
for family, names in selection.items():
    archive = next((OWNER / 'ExternalAssetIntake/Current' / family).glob('*.zip'))
    destination = ROOT / 'ExternalAssetIntake/Current' / family / 'inspection'
    destination.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(archive) as z:
        for entry in z.infolist():
            p = Path(entry.filename)
            if not (p.stem in names and p.suffix.lower() == '.fbx' and ('/FBX/' in entry.filename or '/Assets/fbx/' in entry.filename)):
                continue
            data = z.read(entry)
            (destination / p.name).write_bytes(data)
            rows.append(dict(family=family, archive=str(archive), archiveSha256=hashlib.sha256(archive.read_bytes()).hexdigest(),
                             entry=entry.filename, sha256=hashlib.sha256(data).hexdigest(), license='CC0-1.0'))
        if family == 'molten-maps-scifi':
            entry = next(i for i in z.infolist() if Path(i.filename).name == 'Gradient Texture Atlas by Imphenzia.png')
            data = z.read(entry)
            (OUT / 'CF_DonorPalette_BaseColor.png').write_bytes(data)
            rows.append(dict(family=family, archive=str(archive), archiveSha256=hashlib.sha256(archive.read_bytes()).hexdigest(),
                             entry=entry.filename, sha256=hashlib.sha256(data).hexdigest(), license='CC0-1.0',
                             runtimePath='Assets/_Game/Content/CapacitorFieldProof/Textures/CF_DonorPalette_BaseColor.png', nativeUV=True))
    # Exact archive license bytes are retained for provenance, not inferred from
    # the package title. R1 already reviewed both packages as CC0.
    with zipfile.ZipFile(archive) as z:
        licenses = [i for i in z.infolist() if 'license' in Path(i.filename).name.lower() and not i.is_dir()]
        for i, entry in enumerate(licenses):
            target = ROOT / 'Assets/_Game/Content/CapacitorFieldProof/Licenses' / (family + str(i) + Path(entry.filename).suffix)
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_bytes(z.read(entry))
            rows.append(dict(family=family, entry=entry.filename, sha256=hashlib.sha256(z.read(entry)).hexdigest(),
                             license='CC0-1.0', evidence=str(target.relative_to(ROOT))))

rng = np.random.default_rng(5103)
N = 512
y, x = np.mgrid[0:N, 0:N]
# One repeat = one metre. Low amplitude manufacture/wear, no painted seam
# that repeats across unrelated mechanical faces. Recesses come from geometry.
grain = rng.normal(0, 1.7, (N, N)) + np.sin(y * .32) * .7
smudge = np.asarray(Image.fromarray(np.uint8(rng.uniform(0, 255, (N, N)))).filter(ImageFilter.GaussianBlur(14)), dtype=float)
smudge = (smudge - smudge.mean()) * .8
for name, color, metal, smooth in [
    ('Graphite', (53, 61, 72), 135, 110), ('Shell', (119, 137, 151), 100, 135),
    ('Steel', (148, 159, 164), 235, 174), ('Deck', (75, 84, 94), 100, 100),
    ('Ceramic', (72, 58, 39), 30, 90), ('Warning', (158, 99, 24), 85, 108),
    ('Coil', (27, 86, 125), 150, 170), ('Cold', (55, 132, 184), 130, 190),
    ('Hot', (177, 49, 19), 105, 140)]:
    rgb = np.stack([np.clip(c + grain + smudge, 0, 255) for c in color], axis=-1).astype('uint8')
    Image.fromarray(rgb).save(OUT / f'CF_{name}_BaseColor.png')
    gloss = np.zeros((N, N, 4), dtype='uint8')
    gloss[:, :, 0] = metal
    gloss[:, :, 3] = np.clip(smooth + grain * 2 + smudge * 3, 0, 255)
    Image.fromarray(gloss).save(OUT / f'CF_{name}_MetallicSmoothness.png')
    normal = np.zeros((N, N, 3), dtype='uint8')
    normal[:, :, 0] = np.clip(128 + np.gradient(grain, axis=1) * .7, 0, 255)
    normal[:, :, 1] = np.clip(128 + np.gradient(grain, axis=0) * .7, 0, 255)
    normal[:, :, 2] = 255
    Image.fromarray(normal).save(OUT / f'CF_{name}_Normal.png')
    Image.fromarray(np.uint8(np.clip(247 + smudge * .2, 230, 255))).save(OUT / f'CF_{name}_Occlusion.png')
(PASS / 'provenance.json').write_text(json.dumps(rows, indent=2), encoding='utf-8')
print('CF_SURFACES_AND_CURRENT_DONORS_READY', len(rows))
