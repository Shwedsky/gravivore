"""Adversarial extraction checks, with synthetic fixtures only."""
import io
import tempfile
import tarfile
import unittest
import zipfile
from pathlib import Path
import intake_v2 as intake


class SafeExtractionTests(unittest.TestCase):
    def test_traversal_and_windows_paths(self):
        with tempfile.TemporaryDirectory() as temp:
            for path in ['../escape.fbx', '/absolute.png', 'C:/escape', 'a/../b', 'a:stream', 'CON.txt', 'foo.']:
                with self.subTest(path=path), self.assertRaises(ValueError):
                    intake.safe_path(Path(temp), path)

    def test_identical_duplicate_allowed_conflicting_duplicate_rejected(self):
        with tempfile.TemporaryDirectory() as temp:
            target = Path(temp) / 'x.png'
            intake.copy_member(io.BytesIO(b'abc'), target, 3)
            intake.copy_member(io.BytesIO(b'abc'), target, 3)
            with self.assertRaises(ValueError):
                intake.copy_member(io.BytesIO(b'xyz'), target, 3)
            self.assertEqual(target.read_bytes(), b'abc')

    def test_zip_crc_and_original_unchanged(self):
        with tempfile.TemporaryDirectory() as temp:
            archive = Path(temp) / 'pack.zip'
            with zipfile.ZipFile(archive, 'w') as z:
                z.writestr('Assets/models/test.obj', b'v 0 0 0\n')
            before = intake.digest(archive)
            intake.extract_archive(archive, Path(temp) / 'out')
            self.assertEqual(before, intake.digest(archive))
            self.assertEqual((Path(temp) / 'out/Assets/models/test.obj').read_bytes(), b'v 0 0 0\n')

    def test_tar_links_rejected(self):
        with tempfile.TemporaryDirectory() as temp:
            archive = Path(temp) / 'pack.tar'
            with tarfile.open(archive, 'w') as t:
                m = tarfile.TarInfo('link')
                m.type = tarfile.SYMTYPE
                m.linkname = '../../escape'
                t.addfile(m)
            with self.assertRaises(ValueError):
                intake.extract_archive(archive, Path(temp) / 'out')

    def test_case_collisions_rejected(self):
        with tempfile.TemporaryDirectory() as temp:
            archive = Path(temp) / 'pack.zip'
            with zipfile.ZipFile(archive, 'w') as z:
                z.writestr('Mesh.obj', b'abc')
                z.writestr('mesh.obj', b'abc')
            with self.assertRaises(ValueError):
                intake.extract_archive(archive, Path(temp) / 'out')

    def test_size_limit(self):
        with tempfile.TemporaryDirectory() as temp:
            with self.assertRaises(ValueError):
                intake.copy_member(io.BytesIO(), Path(temp) / 'x', intake.MAX_FILE + 1)


if __name__ == '__main__':
    unittest.main()
