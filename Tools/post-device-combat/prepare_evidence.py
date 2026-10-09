"""Collect final test/build evidence and redact local licensing identifiers from logs."""
import json
from pathlib import Path
import re
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
OUTPUT = ROOT / "docs/history/visual-stages/chapter01-visual-passes/post-device-combat-readability/verification"

def sanitize(source, destination=None):
    text = source.read_text(encoding="utf-8-sig", errors="replace")
    # Keep diagnostic messages; omit account/machine identifiers emitted by Unity licensing.
    patterns = [r"(?im)^.*(?:Machine Id:|Machine ID:|User Id:|User ID:|LicensingClient.*(?:token|serial)|License serial number:|Entitlement Id:|access token|refresh token).*$",
                r"(?im)^.*(?:Unity Editor license serial|License Serial|Session Id:|Correlation Id:).*$"]
    for pattern in patterns:
        text = re.sub(pattern, "[local licensing identifier redacted]", text)
    (destination or source).write_text(text, encoding="utf-8")

def main():
    results = {}
    for mode in ("EditMode", "PlayMode"):
        xml = ET.parse(OUTPUT / f"{mode}.xml").getroot()
        results[mode] = {key: int(xml.attrib[key]) for key in ("total", "passed", "failed", "skipped")}
        results[mode]["skippedTests"] = [node.attrib["fullname"] for node in xml.iter("test-case") if node.attrib.get("result") == "Skipped"]
        if results[mode]["failed"]:
            raise RuntimeError(f"{mode} still has failed tests")
    for path in OUTPUT.glob("*.log"):
        sanitize(path)
    build_logs = list((ROOT / "Builds/Logs").glob("android-dev-*.log"))
    if build_logs:
        sanitize(max(build_logs, key=lambda path: path.stat().st_mtime), OUTPUT / "AndroidDev.log")
    (OUTPUT / "test_results.json").write_text(json.dumps(results, indent=2), encoding="utf-8")
    print(json.dumps(results, indent=2))

if __name__ == "__main__":
    main()
