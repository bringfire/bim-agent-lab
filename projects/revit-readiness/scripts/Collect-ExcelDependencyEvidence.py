"""Capture the locked NuGet graph, package metadata and license notices for review.

Run after restore. Python's standard library is a research aid, not a plugin dependency.
Downloads license text only; never executes upstream code.
"""
import hashlib
from datetime import datetime, timezone
import json
import os
from pathlib import Path
import subprocess
import urllib.error
import urllib.request
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "third-party"
CACHE = Path(os.environ.get("NUGET_PACKAGES", str(Path.home() / ".nuget/packages")))


def download(url, target):
    with urllib.request.urlopen(url, timeout=30) as response:
        target.write_bytes(response.read())
    return {"file": str(target.relative_to(OUT)).replace("\\", "/"), "source": url,
            "sha256": hashlib.sha256(target.read_bytes()).hexdigest()}


def main():
    OUT.mkdir(exist_ok=True)
    graph = json.loads((ROOT / "src/excel-report/packages.lock.json").read_text())
    records = []
    for package, item in graph["dependencies"][".NETFramework,Version=v4.8"].items():
        if item["type"] == "Project":
            continue
        version = item["resolved"]
        folder = CACHE / package.lower() / version
        destination = OUT / (package + "-" + version)
        destination.mkdir(exist_ok=True)
        nuspec = folder / (package.lower() + ".nuspec")
        (destination / "package.nuspec").write_bytes(nuspec.read_bytes())
        metadata = ET.fromstring(nuspec.read_bytes()).find("{*}metadata")
        license_node = metadata.find("{*}license")
        repository = metadata.find("{*}repository")
        repo = repository.get("url") if repository is not None else None
        commit = repository.get("commit") if repository is not None else None
        if package == "ExcelNumberFormat":
            if version != "1.1.0":
                raise RuntimeError("Review and pin the new ExcelNumberFormat license source before collecting it.")
            repo, commit = "https://github.com/andersnm/ExcelNumberFormat", "38c6a71919ad3895e3ba61d7a888012670d26303"
        record = {"id": package, "version": version, "relationship": item["type"],
                  "nuget": f"https://www.nuget.org/packages/{package}/{version}",
                  "contentHash": item["contentHash"], "dependencies": item.get("dependencies", {}),
                  "licenseDeclaration": license_node.text if license_node is not None else None,
                  "repository": repo, "commit": commit, "notices": []}
        for file in sorted(folder.glob("*")):
            if file.is_file() and ("license" in file.name.lower() or "notice" in file.name.lower()):
                target = destination / file.name
                target.write_bytes(file.read_bytes())
                record["notices"].append({"file": str(target.relative_to(OUT)).replace("\\", "/"),
                    "source": f"NuGet package {package}/{version}/{file.name}", "sha256": hashlib.sha256(file.read_bytes()).hexdigest()})
        if not record["notices"]:
            if not repo or not commit:
                raise RuntimeError("No pinned license source: " + package)
            raw = repo.replace("https://github.com/", "https://raw.githubusercontent.com/").removesuffix(".git") + "/" + commit + "/"
            for candidate in ("LICENSE", "LICENSE.txt", "LICENSE.TXT", "LICENSE.md"):
                try:
                    record["notices"].append(download(raw + candidate, destination / "LICENSE.txt"))
                    break
                except urllib.error.HTTPError as error:
                    if error.code != 404:
                        raise
            else:
                raise RuntimeError("License not found: " + package)
            # Preserve an upstream NOTICE when present (especially Apache-licensed dependencies).
            for candidate in ("NOTICE", "NOTICE.txt", "NOTICE.md"):
                try:
                    record["notices"].append(download(raw + candidate, destination / "NOTICE.txt"))
                    break
                except urllib.error.HTTPError as error:
                    if error.code != 404:
                        raise
        records.append(record)
        print(package, version, record["licenseDeclaration"] or "see packaged LICENSE.TXT")
    font_dir = OUT / "Carlito"
    font_dir.mkdir(exist_ok=True)
    previous = OUT / "dependency-evidence.json"
    old = json.loads(previous.read_text()) if previous.exists() else {}
    commit = old.get("embeddedFont", {}).get("licenseSourceCommit")
    if not commit:
        commit = subprocess.check_output(["git", "ls-remote", "https://github.com/googlefonts/carlito.git", "HEAD"], text=True).split()[0]
    font = {"name": "CarlitoBare (embedded inside ClosedXML.dll)",
            "observedCopyright": "Copyright 2013 The Carlito Project Authors (https://github.com/googlefonts/carlito)",
            "license": "OFL-1.1", "licenseSourceCommit": commit,
            "note": "Copyright read from the embedded source TTF name table. OFL text obtained from the named font project; this does not identify the original font build revision.",
            "notice": download(f"https://raw.githubusercontent.com/googlefonts/carlito/{commit}/OFL.txt", font_dir / "OFL.txt")}
    previous.write_text(json.dumps({"reviewDate": datetime.now(timezone.utc).date().isoformat(), "target": "net48", "packages": records, "embeddedFont": font}, indent=2) + "\n", encoding="utf-8")


if __name__ == "__main__":
    main()
