"""Delivery gate: real files, source provenance, color data, motion and scope."""
import hashlib
import json
import subprocess
import unittest
from pathlib import Path
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'docs/visual-production-v2/modeling'

def read(name):
    return json.loads((OUT/'data'/name).read_text(encoding='utf-8'))

class ProductionGate(unittest.TestCase):
    def test_material_hashes_dimensions_channels(self):
        materials=read('material_prep.json')
        self.assertEqual(len(materials),4)
        self.assertEqual(sum(len(m['maps']) for m in materials),14)
        for m in materials:
            channels={i['channel'] for i in m['maps']}
            self.assertTrue({'basecolor','normal_gl','roughness'}<=channels)
            self.assertFalse({'displacement','height'}&channels)
            if m['name'] in ('blue_metal_plate','rusty_metal_grid'):
                self.assertNotIn('metallic',channels)
            for item in m['maps']:
                p=ROOT/item['output']
                self.assertEqual(hashlib.file_digest(p.open('rb'),'sha256').hexdigest(),item['sha256'])
                with Image.open(p) as im:
                    self.assertLessEqual(max(im.size),2048)
                    if item['channel']=='normal_gl':
                        # OpenGL neutral normals must retain blue-positive Z.
                        sample=im.resize((1,1)).convert('RGB').getpixel((0,0))
                        self.assertTrue(90<sample[0]<170 and 90<sample[1]<170 and sample[2]>180,sample)

    def test_source_archive_and_model_coverage(self):
        archives=read('archive_inventory.json')
        self.assertEqual(len(archives),13)
        self.assertTrue(all(len(a['sha256'])==64 and a['bytes']>0 for a in archives))
        self.assertEqual(len(read('candidates_audit.json')),8)
        environment=read('environment_audit.json')
        self.assertEqual(len(environment),189)
        names={Path(r['path']).stem for r in environment}
        subset=read('environment_subset.json')
        self.assertEqual(len({r['module'] for r in subset}),16)
        self.assertTrue(all(r['module'] in names for r in subset))

    def test_scout_motion_is_real_not_identical_pose(self):
        frames=read('scout_animation_samples.json')
        for action in ('Idle','Walk','Fire','BigBoom'):
            poses=[r for r in frames if r['action']==action]
            self.assertEqual(len(poses),2)
            self.assertNotEqual(poses[0]['pose'],poses[1]['pose'],action)

    def test_saved_blend_and_evidence_are_consistent(self):
        model=read('g0_blockout.json')
        verified=read('g0_blend_verification.json')
        self.assertEqual(verified['status'],'passed')
        self.assertEqual(model['triangles'],verified['triangles'])
        self.assertEqual(model['mesh_objects'],verified['mesh_objects'])
        self.assertEqual(model['total_objects'],verified['model_objects'])
        self.assertEqual(verified['unique_model_materials'],4)
        self.assertEqual(model['donor_parts_used'],0)
        self.assertGreater(model['dimensions'][0],model['dimensions'][2]*1.5)
        self.assertLess(model['triangles'],40000)
        self.assertTrue((ROOT/model['source']).is_file())
        manifest=read('g0_evidence_manifest.json')
        self.assertEqual(len(manifest['images']),7)
        for i,item in enumerate(manifest['images'],1):
            self.assertTrue(item['file'].startswith(f'{i:02d}_G0_'))
            p=OUT/'g0-evidence'/item['file']
            self.assertEqual(hashlib.file_digest(p.open('rb'),'sha256').hexdigest(),item['sha256'])
            with Image.open(p) as im:
                im.verify()
        with Image.open(OUT/'g0-evidence/06_G0_black_silhouette.png') as im:
            histogram=im.convert('L').histogram()
            total=sum(histogram)
            self.assertGreater(sum(histogram[:20]),total*.15)
            self.assertGreater(sum(histogram[240:]),total*.20)

    def test_git_scope_has_no_raw_sources_or_unity_changes(self):
        committed=subprocess.check_output(['git','ls-files','docs/visual-production-v2','art/visual-production-v2','Tools/art'],cwd=ROOT,text=True).splitlines()
        self.assertFalse(any(Path(p).suffix.lower() in ('.zip','.rar','.fbx','.glb','.gltf','.dae','.exr','.pyc') for p in committed))
        changed=subprocess.check_output(['git','diff','--name-only','origin/main...HEAD'],cwd=ROOT,text=True).splitlines()
        self.assertFalse(any(p.startswith(('Assets/','Packages/','ProjectSettings/')) for p in changed))
        self.assertEqual(subprocess.run(['git','check-ignore','.asset-intake-tmp/candidates.log'],cwd=ROOT,stdout=subprocess.DEVNULL).returncode,0)

if __name__=='__main__':
    suite=unittest.defaultTestLoader.loadTestsFromTestCase(ProductionGate)
    result=unittest.TextTestRunner(verbosity=2).run(suite)
    (OUT/'data/artifact_validation.json').write_text(json.dumps(dict(status='passed' if result.wasSuccessful() else 'failed',
        tests_run=result.testsRun,failures=len(result.failures),errors=len(result.errors)),indent=2),encoding='utf-8')
    raise SystemExit(0 if result.wasSuccessful() else 1)
