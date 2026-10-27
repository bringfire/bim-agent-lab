# Excel dependency notices

These files accompany the ClosedXML-based Excel exporter. [dependency-evidence.json](dependency-evidence.json) maps every locked package to its exact version, source metadata, license/notice files, and hashes. Each package folder preserves its `.nuspec` and license text; packaged third-party notices are retained unchanged. `Carlito/` preserves the embedded font's license and copyright notice.

The [research record](../../../docs/research/closedxml-dependency.md) explains selection, observed terms, and compatibility findings. The [collection script](../scripts/Collect-ExcelDependencyEvidence.py) reproduces the evidence after package restore. Review new package versions and sources deliberately; the script is not a substitute for license review. Some older Microsoft packages declare only a license URL; their bundled MIT license text is retained here.

The install script copies this directory and verifies file hashes. Preserve it when distributing the exporter. Source clones, NuGet packages, Autodesk DLLs, and sample RVTs are not vendored in this repository. Original lab material uses the owner-selected root [MIT License](../../../LICENSE); these upstream materials retain their own terms.
