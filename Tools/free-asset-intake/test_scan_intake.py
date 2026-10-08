"""Behavioral checks using synthetic files only; never extract/import any package."""

import contextlib
import copy
import hashlib
import io
import json
import os
from pathlib import Path
import stat
import subprocess
import sys
import tempfile
import unittest
from unittest.mock import patch

import scan_intake as intake


class InventoryTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory(prefix="gravivore-intake-test-")
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name) / "drop"
        self.root.mkdir()
        self.manifest = intake.load_manifest(intake.MANIFEST)
        for source in self.manifest["sources"]:
            folder = self.root / intake.folder_relative(source["expectedLocalFolder"])
            folder.mkdir(parents=True)
            (folder / ".gitkeep").write_text("")
            (folder / "DROP_HERE.md").write_text("Reserved marker\n")
        for folder in self.manifest["fallbackFolders"]:
            (self.root / intake.folder_relative(folder)).mkdir()
        self.manifest_path = Path(self.temporary.name) / "manifest.json"
        self.manifest_path.write_text(json.dumps(self.manifest), encoding="utf-8")
        self.folder = self.root / intake.folder_relative(self.manifest["sources"][1]["expectedLocalFolder"])

    def source(self, report, index=1):
        return report["sources"][index]

    def test_empty_drop_is_successful_and_no_markers_are_payloads(self):
        report = intake.scan(self.root, self.manifest)
        self.assertEqual({"expectedSources": 39, "present": 0, "missing": 39,
                          "unexpectedFiles": 0, "totalBytes": 0}, report["summary"])
        self.assertEqual([], report["files"])
        self.assertTrue(all(source["status"] == "EMPTY" for source in report["sources"]))
        with contextlib.redirect_stdout(io.StringIO()):
            self.assertEqual(0, intake.main(["--intake-root", str(self.root), "--verify"]))

    def test_hashes_bytes_nested_paths_and_payloads_remain_unchanged(self):
        nested = self.folder / "original subfolder"
        nested.mkdir()
        data = b"original fake zip; never extracted\x00\xff"
        payload = nested / "original name.ZIP"
        payload.write_bytes(data)
        before = payload.stat()
        report = intake.scan(self.root, self.manifest)
        record = report["files"][0]
        self.assertEqual(hashlib.sha256(data).hexdigest(), record["sha256"])
        self.assertEqual(len(data), record["bytes"])
        self.assertEqual(".zip", record["extension"])
        self.assertEqual(self.folder.relative_to(self.root).as_posix(), record["sourceFolder"])
        self.assertEqual(payload.relative_to(self.root).as_posix(), record["relativePath"])
        self.assertEqual("PRESENT", self.source(report)["status"])
        self.assertEqual(data, payload.read_bytes())
        self.assertEqual(before.st_mtime_ns, payload.stat().st_mtime_ns)

    def test_required_formats_are_inventoried_and_license_sidecars_do_not_count(self):
        required = {".unitypackage", ".zip", ".rar", ".7z", ".fbx", ".obj", ".glb", ".gltf",
                    ".blend", ".png", ".jpg", ".jpeg", ".psd", ".fig", ".txt", ".pdf"}
        for extension in required:
            (self.folder / ("original" + extension)).write_bytes(b"synthetic")
        report = intake.scan(self.root, self.manifest)
        self.assertEqual(required, {f["extension"] for f in report["files"]})
        self.assertEqual(len(required) - 2, self.source(report)["payloadCount"])
        self.assertEqual("MULTIPLE_PAYLOADS", self.source(report)["status"])
        self.assertEqual([], report["unexpectedFiles"])

    def test_sidecar_only_folder_is_empty(self):
        (self.folder / "license.txt").write_text("License evidence")
        (self.folder / "terms.pdf").write_bytes(b"evidence")
        report = intake.scan(self.root, self.manifest)
        self.assertEqual("EMPTY", self.source(report)["status"])
        self.assertEqual(2, len(report["files"]))

    def test_zero_byte_payload_is_not_present(self):
        (self.folder / "unfinished.zip").touch()
        report = intake.scan(self.root, self.manifest)
        self.assertEqual("EMPTY", self.source(report)["status"])
        self.assertIn("EMPTY_FILE", report["unexpectedFiles"][0]["reasons"])

    def test_missing_leaf_is_an_observation_not_error(self):
        for marker in intake.MARKERS:
            (self.folder / marker).unlink()
        self.folder.rmdir()
        report = intake.scan(self.root, self.manifest)
        self.assertEqual("MISSING", self.source(report)["status"])
        with contextlib.redirect_stdout(io.StringIO()):
            self.assertEqual(0, intake.main(["--intake-root", str(self.root)]))

    def test_concept_board_any_filename_and_uppercase_extension(self):
        folder = self.root / intake.folder_relative(self.manifest["sources"][0]["expectedLocalFolder"])
        (folder / "my authoritative board.JPEG").write_bytes(b"synthetic image")
        (folder / "not-a-board.zip").write_bytes(b"synthetic zip")
        report = intake.scan(self.root, self.manifest)
        self.assertEqual("PRESENT", self.source(report, 0)["status"])
        self.assertEqual(1, self.source(report, 0)["payloadCount"])
        self.assertIn("REFERENCE_REQUIRES_PNG_JPG_JPEG", report["unexpectedFiles"][0]["reasons"])

    def test_unmapped_unknown_files_count_bytes_but_never_complete_a_source(self):
        (self.root / "unmapped.zip").write_bytes(b"abc")
        (self.folder / "notes.md").write_bytes(b"1234")
        nested = self.root / "98_unclassified" / "nested"
        nested.mkdir()
        (nested / "fallback.fbx").write_bytes(b"abcde")
        report = intake.scan(self.root, self.manifest)
        self.assertEqual(3, report["summary"]["unexpectedFiles"])
        self.assertEqual(12, report["summary"]["totalBytes"])
        self.assertEqual(0, report["summary"]["present"])
        fallback = next(f for f in report["files"] if f["relativePath"].endswith("fallback.fbx"))
        self.assertEqual("98_unclassified", fallback["sourceFolder"])

    def test_reports_are_deterministic_excluded_and_manifest_is_unchanged(self):
        (self.folder / "b.zip").write_bytes(b"bbb")
        (self.folder / "a.fbx").write_bytes(b"aaa")
        before = self.manifest_path.read_bytes()
        first = intake.scan(self.root, self.manifest)
        report_path = intake.write_report(self.root, first)
        initial_bytes = report_path.read_bytes()
        (report_path.parent / "other-local-report.json").write_text("{}")
        second = intake.scan(self.root, self.manifest)
        self.assertEqual(first, second)
        intake.write_report(self.root, second)
        self.assertEqual(initial_bytes, report_path.read_bytes())
        self.assertEqual(before, self.manifest_path.read_bytes())
        self.assertEqual([], list(report_path.parent.glob(".inventory-*.tmp")))

    def test_missing_root_cli_returns_structural_error(self):
        with contextlib.redirect_stderr(io.StringIO()) as output:
            code = intake.main(["--intake-root", str(self.root / "absent")])
        self.assertEqual(2, code)
        self.assertIn("INTAKE_TOOLING_ERROR", output.getvalue())

    def test_source_folder_replaced_by_file_is_structural_error(self):
        for marker in intake.MARKERS:
            (self.folder / marker).unlink()
        self.folder.rmdir()
        self.folder.write_bytes(b"not a folder")
        with self.assertRaises(intake.IntakeError):
            intake.scan(self.root, self.manifest)

    def test_category_replaced_by_file_is_structural_error(self):
        isolated = Path(self.temporary.name) / "invalid-category"
        isolated.mkdir()
        (isolated / "01_environment").write_bytes(b"not a folder")
        with self.assertRaises(intake.IntakeError):
            intake.scan(isolated, self.manifest)

    @unittest.skipUnless(os.name == "nt", "Windows wrappers require Windows")
    def test_windows_wrappers_preserve_incomplete_success_and_tooling_error_exit_codes(self):
        for name in ("scan-intake.ps1", "verify-drop.ps1"):
            command = ["powershell.exe", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File",
                       str(intake.WORKSPACE / "Tools/free-asset-intake" / name),
                       "-PythonPath", sys.executable, "-IntakeRoot", str(self.root)]
            with self.subTest(wrapper=name):
                success = subprocess.run(command, capture_output=True, text=True)
                self.assertEqual(0, success.returncode, success.stderr)
                self.assertIn("EXPECTED SOURCES: 39", success.stdout)
                failure = subprocess.run(command + ["-ManifestPath", str(self.root / "absent.json")], capture_output=True, text=True)
                self.assertEqual(2, failure.returncode, failure.stderr)
                self.assertIn("INTAKE_TOOLING_ERROR", failure.stderr)

    def test_reparse_points_are_rejected_instead_of_followed(self):
        class ReparseStat:
            st_mode = stat.S_IFDIR
            st_file_attributes = 0x400
        with patch.object(Path, "lstat", return_value=ReparseStat()):
            with self.assertRaises(intake.IntakeError):
                intake.scan(self.root, self.manifest)

    def test_report_hardlink_is_rejected_without_modifying_original(self):
        original = self.folder / "original.zip"
        original.write_bytes(b"untouched")
        report_dir = self.root / intake.REPORT_DIR
        report_dir.mkdir()
        (report_dir / intake.REPORT_NAME).hardlink_to(original)
        with self.assertRaises(intake.IntakeError):
            intake.write_report(self.root, {})
        self.assertEqual(b"untouched", original.read_bytes())

    def test_permission_error_is_not_silently_swallowed(self):
        (self.folder / "original.zip").write_bytes(b"data")
        with patch.object(intake, "hash_file", side_effect=PermissionError("denied")):
            with contextlib.redirect_stderr(io.StringIO()) as output:
                self.assertEqual(2, intake.main(["--intake-root", str(self.root)]))
        self.assertIn("denied", output.getvalue())

    def test_file_changing_during_hash_is_reported(self):
        payload = self.folder / "copying.zip"
        payload.write_bytes(b"old")
        original_stat = intake.checked_stat
        calls = []
        def changed_stat(path):
            calls.append(path)
            if len(calls) == 2:
                path.write_bytes(b"changed during hash")
            return original_stat(path)
        with patch.object(intake, "checked_stat", side_effect=changed_stat):
            with self.assertRaises(intake.IntakeError):
                intake.hash_file(payload)

    def test_invalid_manifest_paths_duplicates_licenses_and_statuses_are_rejected(self):
        variants = []
        for key, value in [("expectedLocalFolder", intake.INTAKE + "/../escape"),
                           ("expectedLocalFolder", intake.INTAKE + "/01_environment//folder"),
                           ("expectedLicense", "FREE_IS_NOT_A_LICENSE"),
                           ("inspectionStatus", "APPROVED"),
                           ("attributionRequired", "false"), ("priority", True)]:
            data = copy.deepcopy(self.manifest)
            data["sources"][0][key] = value
            variants.append(data)
        duplicate = copy.deepcopy(self.manifest)
        duplicate["sources"].append(duplicate["sources"][0])
        variants.append(duplicate)
        for data in variants:
            with self.subTest(source=data["sources"][0]):
                self.manifest_path.write_text(json.dumps(data))
                with self.assertRaises(intake.IntakeError):
                    intake.load_manifest(self.manifest_path)


class RepositoryPolicyTests(unittest.TestCase):
    def test_all_41_leaf_folders_and_82_markers_exist_and_are_trackable(self):
        manifest = intake.load_manifest(intake.MANIFEST)
        folders = [s["expectedLocalFolder"] for s in manifest["sources"]] + manifest["fallbackFolders"]
        self.assertEqual(41, len(folders))
        paths = []
        for folder in folders:
            for marker in intake.MARKERS:
                relative = folder + "/" + marker
                self.assertTrue((intake.WORKSPACE / relative).is_file(), relative)
                paths.append(relative)
        result = subprocess.run(["git", "check-ignore", "--no-index", "-z", "--stdin"], cwd=intake.WORKSPACE,
                                input=("\0".join(paths) + "\0").encode("utf-8"), capture_output=True)
        self.assertEqual(1, result.returncode, (result.stderr + result.stdout).decode("utf-8"))

    def test_real_fake_zip_is_ignored_and_removed_and_other_payloads_are_ignored(self):
        manifest = intake.load_manifest(intake.MANIFEST)
        folder = manifest["sources"][1]["expectedLocalFolder"]
        with tempfile.NamedTemporaryFile(dir=intake.WORKSPACE / folder, prefix="gitignore-test-", suffix=".zip", delete=False) as stream:
            probe = Path(stream.name)
            stream.write(b"synthetic test only")
        try:
            result = subprocess.run(["git", "check-ignore", "--no-index", str(probe)], cwd=intake.WORKSPACE, capture_output=True, text=True)
            self.assertEqual(0, result.returncode, result.stderr)
        finally:
            probe.unlink()
        self.assertFalse(probe.exists())
        paths = [folder + "/download" + ext for ext in intake.RECOGNIZED_EXTENSIONS | {".bin", ".md", ".svg", ".json"}]
        paths += [intake.INTAKE + "/_local_reports/intake_inventory.json",
                  intake.INTAKE + "/unknown/DROP_HERE.md", folder + "/nested/DROP_HERE.md"]
        result = subprocess.run(["git", "check-ignore", "--no-index", "-z", "--stdin"], cwd=intake.WORKSPACE,
                                input=("\0".join(paths) + "\0").encode("utf-8"), capture_output=True)
        self.assertEqual(0, result.returncode, result.stderr.decode("utf-8"))
        self.assertEqual(set(paths), set(result.stdout.decode("utf-8").rstrip("\0").split("\0")))


if __name__ == "__main__":
    unittest.main()
