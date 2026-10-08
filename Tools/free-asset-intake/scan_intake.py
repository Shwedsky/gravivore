#!/usr/bin/env python3
"""Read-only, deterministic inventory of original downloads. Python 3.9+, stdlib only."""

import argparse
import hashlib
import json
import os
from pathlib import Path, PurePosixPath
import re
import stat
import sys
import tempfile


WORKSPACE = Path(__file__).resolve().parents[2]
INTAKE = "ExternalAssetIntake/FreeAssetIntakeV1"
MANIFEST = WORKSPACE / "docs/free-asset-intake-v1/SOURCE_MANIFEST.json"
REPORT_DIR = "_local_reports"
REPORT_NAME = "intake_inventory.json"
LICENSES = {
    "UNITY_ASSET_STORE_EULA", "CC0", "CC_BY_4_0",
    "CUSTOM_FREE_COMMERCIAL", "UNKNOWN_REQUIRES_REVIEW",
}
PAYLOAD_EXTENSIONS = {
    ".unitypackage", ".zip", ".rar", ".7z", ".fbx", ".obj", ".glb", ".gltf",
    ".blend", ".png", ".jpg", ".jpeg", ".psd", ".fig",
    ".tga", ".tif", ".tiff", ".bmp", ".exr", ".hdr", ".dds", ".webp",
}
RECOGNIZED_EXTENSIONS = PAYLOAD_EXTENSIONS | {".txt", ".pdf"}
REFERENCE_EXTENSIONS = {".png", ".jpg", ".jpeg"}
MARKERS = {".gitkeep", "DROP_HERE.md"}
SOURCE_FIELDS = {
    "id", "displayName", "category", "expectedLocalFolder", "sourceUrl",
    "publisher", "sourcePlatform", "expectedLicense", "attributionRequired",
    "intendedRoles", "priority", "downloadStatus", "inspectionStatus", "notes",
}


class IntakeError(ValueError):
    """Structural/tooling error, distinct from incomplete owner downloads."""


def folder_relative(value):
    """Accept only canonical, portable folder paths under this intake root."""
    if not isinstance(value, str) or not value.startswith(INTAKE + "/"):
        raise IntakeError(f"Folder must be under {INTAKE}: {value!r}")
    parts = value.split("/")
    if any(not re.fullmatch(r"[a-zA-Z0-9_-]+", part) for part in parts):
        raise IntakeError(f"Unsafe/noncanonical folder: {value!r}")
    return PurePosixPath(*parts[2:])


def load_manifest(path):
    data = json.loads(Path(path).read_text(encoding="utf-8-sig"))
    if not isinstance(data, dict) or type(data.get("schemaVersion")) is not int or data["schemaVersion"] != 1:
        raise IntakeError("Manifest schemaVersion must be 1")
    if set(data) != {"schemaVersion", "intakeRoot", "licenseClasses", "productionPolicy", "fallbackFolders", "sources"}:
        raise IntakeError("Manifest fields do not match the V1 schema")
    if data.get("intakeRoot") != INTAKE:
        raise IntakeError(f"Manifest intakeRoot must be {INTAKE}")
    classes = data.get("licenseClasses")
    if not isinstance(classes, list) or set(classes) != LICENSES or len(classes) != len(LICENSES):
        raise IntakeError("Manifest must declare exactly the five supported license classes")
    if not isinstance(data.get("productionPolicy"), str) or not data["productionPolicy"].strip():
        raise IntakeError("Manifest requires an explicit productionPolicy")
    sources = data.get("sources")
    if not isinstance(sources, list) or not sources:
        raise IntakeError("Manifest requires a non-empty sources array")
    ids, folders = set(), set()
    for source in sources:
        if not isinstance(source, dict) or set(source) != SOURCE_FIELDS:
            raise IntakeError("Source fields do not match the V1 schema")
        for key in SOURCE_FIELDS - {"sourceUrl", "attributionRequired", "intendedRoles", "priority"}:
            if not isinstance(source[key], str) or not source[key].strip():
                raise IntakeError(f"Source {key} must be non-empty text")
        if not re.fullmatch(r"[a-z0-9]+(?:-[a-z0-9]+)*", source["id"]):
            raise IntakeError(f"Invalid source id: {source['id']}")
        folder = folder_relative(source["expectedLocalFolder"]).as_posix()
        if len(PurePosixPath(folder).parts) != 2 or PurePosixPath(folder).parts[0] != source["category"]:
            raise IntakeError(f"Source folder/category mismatch: {source['id']}")
        if source["id"] in ids or folder.casefold() in folders:
            raise IntakeError("Duplicate source id/folder (case-insensitive folders)")
        ids.add(source["id"])
        folders.add(folder.casefold())
        if source["expectedLicense"] not in LICENSES:
            raise IntakeError(f"Unsupported license: {source['expectedLicense']}")
        if source["attributionRequired"] is not None and type(source["attributionRequired"]) is not bool:
            raise IntakeError("attributionRequired must be true, false or null")
        url = source["sourceUrl"]
        if url is not None and (not isinstance(url, str) or not re.fullmatch(r"https?://[^\s]+", url)):
            raise IntakeError("sourceUrl must be an HTTP(S) URL or null (unresolved)")
        roles = source["intendedRoles"]
        if not isinstance(roles, list) or not roles or any(not isinstance(role, str) or not role.strip() for role in roles):
            raise IntakeError("intendedRoles must contain non-empty strings")
        if type(source["priority"]) is not int or source["priority"] not in (1, 2, 3):
            raise IntakeError("priority must be 1, 2 or 3")
        if source["downloadStatus"] != "AWAITING_OWNER_DOWNLOAD" or source["inspectionStatus"] != "NOT_INSPECTED":
            raise IntakeError("V1 manifest statuses must remain awaiting download / not inspected")
    fallbacks = data.get("fallbackFolders")
    if not isinstance(fallbacks, list) or len(fallbacks) != 2:
        raise IntakeError("Manifest requires the two fallback folders")
    if set(fallbacks) != {INTAKE + "/98_unclassified", INTAKE + "/99_failed_or_replacements"}:
        raise IntakeError("Fallback folders do not match the V1 layout")
    return data


def checked_stat(path):
    info = path.lstat()
    # Junctions are reparse points, even when is_symlink() would be false.
    if stat.S_ISLNK(info.st_mode) or getattr(info, "st_file_attributes", 0) & 0x400:
        raise IntakeError(f"Links/junctions/reparse points are not supported: {path}")
    return info


def inventory_files(root):
    def walk(directory):
        checked_stat(directory)
        with os.scandir(directory) as entries:
            paths = sorted((Path(entry.path) for entry in entries), key=lambda p: (p.name.casefold(), p.name))
        for path in paths:
            info = checked_stat(path)
            if directory == root and path.name == REPORT_DIR:
                if not stat.S_ISDIR(info.st_mode):
                    raise IntakeError("_local_reports must be a directory")
                continue
            if stat.S_ISDIR(info.st_mode):
                yield from walk(path)
            elif stat.S_ISREG(info.st_mode):
                yield path
            else:
                raise IntakeError(f"Unsupported filesystem entry: {path}")
    yield from walk(root)


def hash_file(path):
    before = checked_stat(path)
    digest = hashlib.sha256()
    count = 0
    with path.open("rb") as stream:
        opened = os.fstat(stream.fileno())
        if (opened.st_dev, opened.st_ino) != (before.st_dev, before.st_ino):
            raise IntakeError(f"File replaced while opening; retry when copy finishes: {path}")
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            count += len(block)
            digest.update(block)
        after = os.fstat(stream.fileno())
    current = checked_stat(path)
    signature = lambda s: (s.st_dev, s.st_ino, s.st_size, s.st_mtime_ns)
    if signature(before) != signature(after) or signature(before) != signature(current) or count != before.st_size:
        raise IntakeError(f"File changed during scan; retry when copy finishes: {path}")
    return count, digest.hexdigest()


def scan(root, manifest):
    root = Path(os.path.abspath(root))  # Do not resolve away a root symlink.
    # Reject reparse points in ancestors too, before walking or writing a report.
    for ancestor in [root, *root.parents]:
        checked_stat(ancestor)
    if not root.is_dir():
        raise IntakeError(f"Intake root is not a directory: {root}")
    source_map = {folder_relative(s["expectedLocalFolder"]).as_posix(): s for s in manifest["sources"]}
    leaves = set(source_map) | {folder_relative(f).as_posix() for f in manifest["fallbackFolders"]}
    for folder in leaves:
        location = root
        for part in PurePosixPath(folder).parts:
            location = location / part
            if location.exists() and not stat.S_ISDIR(checked_stat(location).st_mode):
                raise IntakeError(f"Expected folder is a file: {location}")
    markers = {f"{folder}/{name}" for folder in leaves for name in MARKERS}
    files, unexpected = [], []
    payloads = {folder: [] for folder in source_map}
    for path in inventory_files(root):
        relative = path.relative_to(root).as_posix()
        if relative in markers:
            continue
        folder = next((leaf for leaf in sorted(leaves) if relative.startswith(leaf + "/")), path.parent.relative_to(root).as_posix())
        if folder == ".":
            folder = ""
        source = source_map.get(folder)
        extension = path.suffix.lower()
        size, digest = hash_file(path)
        record = {"relativePath": relative, "sourceFolder": folder,
                  "extension": extension, "bytes": size, "sha256": digest}
        files.append(record)
        reasons = []
        if source is None:
            reasons.append("UNMAPPED_SOURCE_FOLDER")
        if extension not in RECOGNIZED_EXTENSIONS:
            reasons.append("UNRECOGNIZED_EXTENSION")
        if size == 0:
            reasons.append("EMPTY_FILE")
        if source and source["category"] == "00_reference" and extension not in REFERENCE_EXTENSIONS | {".txt", ".pdf"}:
            reasons.append("REFERENCE_REQUIRES_PNG_JPG_JPEG")
        if reasons:
            unexpected.append({"relativePath": relative, "reasons": reasons})
        allowed = REFERENCE_EXTENSIONS if source and source["category"] == "00_reference" else PAYLOAD_EXTENSIONS
        if source and size > 0 and extension in allowed:
            payloads[folder].append(relative)
    files.sort(key=lambda f: f["relativePath"])
    unexpected.sort(key=lambda f: f["relativePath"])
    statuses = []
    for source in manifest["sources"]:
        folder = folder_relative(source["expectedLocalFolder"]).as_posix()
        location = root / folder
        if location.exists() and not location.is_dir():
            raise IntakeError(f"Expected source folder is a file: {location}")
        matches = sorted(payloads[folder])
        status = "MISSING" if not location.exists() else "EMPTY" if not matches else "PRESENT" if len(matches) == 1 else "MULTIPLE_PAYLOADS"
        statuses.append({"id": source["id"], "expectedLocalFolder": source["expectedLocalFolder"],
                         "status": status, "payloadCount": len(matches), "payloads": matches,
                         "inspectionStatus": "NOT_INSPECTED"})
    present = sum(s["payloadCount"] > 0 for s in statuses)
    return {"schemaVersion": 1, "intakeRoot": INTAKE, "files": files,
            "sources": statuses, "unexpectedFiles": unexpected,
            "summary": {"expectedSources": len(statuses), "present": present,
                        "missing": len(statuses) - present, "unexpectedFiles": len(unexpected),
                        "totalBytes": sum(f["bytes"] for f in files)}}


def write_report(root, report):
    directory = Path(root) / REPORT_DIR
    if directory.exists():
        if not stat.S_ISDIR(checked_stat(directory).st_mode):
            raise IntakeError("Report destination must be a directory")
    else:
        directory.mkdir()
    target = directory / REPORT_NAME
    if target.exists() or target.is_symlink():
        info = checked_stat(target)
        if not stat.S_ISREG(info.st_mode) or info.st_nlink > 1:
            raise IntakeError("Report destination must be a regular, unlinked file")
    temporary = None
    try:
        with tempfile.NamedTemporaryFile(mode="w", encoding="utf-8", newline="\n", dir=directory,
                                         prefix=".inventory-", suffix=".tmp", delete=False) as stream:
            temporary = Path(stream.name)
            json.dump(report, stream, ensure_ascii=False, indent=2)
            stream.write("\n")
        os.replace(temporary, target)
    finally:
        if temporary and temporary.exists():
            temporary.unlink()
    return target


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--intake-root", type=Path, default=WORKSPACE / INTAKE)
    parser.add_argument("--manifest", type=Path, default=MANIFEST)
    parser.add_argument("--verify", action="store_true", help="Print every source's observed drop status")
    args = parser.parse_args(argv)
    try:
        manifest = load_manifest(args.manifest)
        report = scan(args.intake_root, manifest)
        path = write_report(args.intake_root, report)
        summary = report["summary"]
        for label, key in [("EXPECTED SOURCES", "expectedSources"), ("PRESENT", "present"),
                           ("MISSING", "missing"), ("UNEXPECTED FILES", "unexpectedFiles"),
                           ("TOTAL BYTES", "totalBytes")]:
            print(f"{label}: {summary[key]}")
        if args.verify:
            for source in report["sources"]:
                print(f"{source['status']}: {source['id']} ({source['payloadCount']} payloads)")
        print(f"LOCAL REPORT: {path}")
        return 0
    except (OSError, ValueError, TypeError) as error:
        print(f"INTAKE_TOOLING_ERROR: {error}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    sys.exit(main())
