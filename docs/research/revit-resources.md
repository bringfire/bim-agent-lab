# Research and resources for the Revit readiness tool

Checked: 2026-10-20
Status: initial resource and precedent review. No candidate repository has been built, adopted, or installed. The inspected commits identify research evidence, not selected production dependencies.

The workshop should begin by demonstrating how to find existing work, inspect it, and gather the resources needed to build. The outcome of research is a justified reuse decision and a reproducible resource list.

## What research must establish

For each candidate, record its purpose, official source, inspected revision, supported host versions, runtime requirements, dependencies, license text, and intended use: development tool, runtime dependency, code to adapt, or architectural precedent. Record which claims come from documentation, source inspection, or a local test.

Separate the decisions “this solves part of our problem,” “we can build it in our environment,” and “we can distribute the resulting package under the intended terms.” A repository license does not establish the licenses of every dependency, model, icon, or bundled binary.

Full license texts and selected dependency manifests from seven pinned commits are saved in [the repository evidence snapshot](evidence/repository-evidence-2026-10-20.json). The snapshot includes source URLs and SHA-256 hashes of the downloaded evidence files.

The seven candidates were cloned under `references/` in the original authoring
workspace at those same commits. These are historical inspection results; the
clones are not included here. See the [clone guide](../how-to/clone-research-repositories.md). Origins, revisions, clean working trees, and the 14 saved license/manifest
files were verified. See the [local checkout manifest](evidence/local-checkouts-2026-10-20.json)
and [first source code map](reference-code-map.md). RevitLookup and pyRevit declare
ten additional submodules that have not been fetched in this inspection pass.

This is an early workshop exercise: [direct an agent to clone and inspect source](../tutorials/01-directing-an-agent-with-git.md)
as part of research, and use Git to checkpoint our own project from its beginning.

## Resources already available locally

| Resource | Observation | Purpose |
|---|---|---|
| Revit 2024 | Executable and API DLL file version `24.3.60.12` | First host for development and acceptance tests |
| Revit API assemblies | `C:\Program Files\Autodesk\Revit 2024\RevitAPI.dll` and `RevitAPIUI.dll` | Database and UI API references |
| Revit API XML documentation | `RevitAPI.xml` and `RevitAPIUI.xml` in the same directory | Version-matched documentation for the coding agent |
| .NET Framework 4.8 reference assemblies | `C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8` exists | Compile the Revit 2024 add-in |
| .NET SDKs | `9.0.318` and `10.0.204` | Build tooling; distinct from the runtime embedded in Revit |
| Python | `3.12.10` | Candidate external MCP server runtime |
| Node.js | `24.11.1` | Candidate TypeScript server/runtime and development tools |
| Git and uv | Executables found on PATH | Version control and Python environment management |
| Snowdon Towers | Architectural, structural, and other discipline models in Revit 2024 Samples | Building context for demos |

The full Revit 2024 SDK was not found in the three conventional SDK locations checked. This does not establish that it is absent elsewhere.

## Official resources to collect for the build

| Resource | Source and intended use | Next action |
|---|---|---|
| Revit 2024 SDK | [Autodesk Revit API developer center](https://aps.autodesk.com/developer/overview/revit-api); its page supplies [REVIT_2024_2_SDK.msi](https://autodesk-adn-transfer.s3.us-west-2.amazonaws.com/ADN+Extranet/Revit/REVIT_2024_2_SDK.msi) | Acquire the versioned SDK, read its included terms, and locate the ExternalEvent and API samples |
| Revit 2024 development requirements | [Autodesk requirements](https://help.autodesk.com/cloudhelp/2024/ENU/Revit-API/files/Revit_API_Developers_Guide/Introduction/Getting_Started/Welcome_to_the_Revit_Platform_API/Revit_API_Revit_API_Developers_Guide_Introduction_Getting_Started_Welcome_to_the_Revit_Platform_API_Development_Requirements_html.html) | Confirm the project targets .NET Framework 4.8 and the installed API references |
| External events | [Autodesk External Events guide](https://help.autodesk.com/cloudhelp/2026/ENU/Revit-API/files/Revit_API_Developers_Guide/Advanced_Topics/Revit_API_Revit_API_Developers_Guide_Advanced_Topics_External_Events_html.html) | Confirm the example against local 2024 API documentation and queue external requests into a valid Revit API context |
| MCP protocol and SDK | [Official server guide](https://modelcontextprotocol.io/docs/develop/build-server) and the language SDK selected below | Pin a release and use its matching documentation and examples |
| RevitLookup | [Release 2027.0.4](https://github.com/lookup-foundation/RevitLookup/releases/tag/2027.0.4) includes `RevitLookup-2024.3.4-SingleUser.msi` | Use the 2024 asset for inspection; the release title alone does not identify host compatibility |
| Sample model | [Autodesk Revit 2024 samples](https://help.autodesk.com/cloudhelp/2024/ENU/Revit-GetStarted/files/GUID-61EF2F22-3A1F-4317-B925-1E85F138BE88.htm) | Prepare a local working copy and record the exact source |
| Dependency notices | License files of the chosen packages and copied source, including transitive dependencies | Preserve required notices in the takeaway package |

SDK downloads and runtime packages have not been installed during this research. API binaries, sample models, and downloaded SDK content retain their own Autodesk terms. For the takeaway, initially provide source links and preparation instructions; review distribution rights separately before bundling those assets.

### API evidence for the three questions

The installed Revit 2024 XML documentation contains the following members:

| Need | API surface | Meaning for our design |
|---|---|---|
| Inspect wall and ceiling build-ups | `HostObjAttributes.GetCompoundStructure()`, `CompoundStructure.GetLayers()` | Compound structure can be absent; handle that case explicitly |
| Read layers | `CompoundStructure.GetLayers()` | Walls return exterior-to-interior ordering; ceilings return top-to-bottom ordering |
| Convert quantities | `UnitUtils.ConvertFromInternalUnits(double, ForgeTypeId)` | Convert explicitly and return the unit with each value |
| Identify findings | `Element.UniqueId` | Stable identifier within its document; also retain document identity |
| Inspect links | `RevitLinkInstance.GetLinkDocument()` | Linked elements belong to a separate document |
| Read modeled area | `BuiltInParameter.HOST_AREA_COMPUTED` | Documented as Area; the business interpretation still needs validation |
| Read top constraint | `BuiltInParameter.WALL_HEIGHT_TYPE` | Supplies constraint data, not proof of physical contact with structure |
| Read ceiling offset | `BuiltInParameter.CEILING_HEIGHTABOVELEVEL_PARAM` | Offset from the associated level |

Use built-in parameter identifiers where available and shared-parameter GUIDs for custom fields. Display names alone are not a dependable cross-project mapping.

## Existing repositories and possible roles

| Repository | License observed at inspected commit | Potential use | Decision from this review |
|---|---|---|---|
| [mcp-servers-for-revit](https://github.com/mcp-servers-for-revit/mcp-servers-for-revit) | [MIT](https://github.com/mcp-servers-for-revit/mcp-servers-for-revit/blob/86cf7057785f2423fc0d0892c9a5d9c93deb1aa3/LICENSE) | Existing TypeScript server, C# bridge, and command system | First candidate to evaluate as a starting point; its source explicitly has a Revit 2024/net48 configuration |
| [Nice3point RevitTemplates](https://github.com/Nice3point/RevitTemplates) | [MIT](https://github.com/Nice3point/RevitTemplates/blob/d703757a01e2cde2eb5392cd59666e6b0e578cf6/LICENSE.md) | Add-in scaffolding and multi-version build conventions | Candidate scaffold; inspected application template includes R23 through R27 configurations, including R24 |
| [RevitLookup](https://github.com/lookup-foundation/RevitLookup) | [MIT](https://github.com/lookup-foundation/RevitLookup/blob/766aa41a3821b64befc0b81e3c6b164725b26e87/LICENSE.md) | Inspect the actual model/API before implementing readers | Development resource; it need not become part of the shipped plugin |
| [Official Python MCP SDK](https://github.com/modelcontextprotocol/python-sdk) | [MIT](https://github.com/modelcontextprotocol/python-sdk/blob/2af9e3089ac66b35c1550c500aae86a82efb1e17/LICENSE) | External Python server if we build a small bridge | Candidate runtime dependency; release API reports v2.3.0; pin and inspect the selected release before adoption |
| [Official C# MCP SDK](https://github.com/modelcontextprotocol/csharp-sdk) | [Transition notice](https://github.com/modelcontextprotocol/csharp-sdk/blob/3d8af7f98a49f55e0ac70e4fcd42df76fe1d1410/LICENSE): Apache-2.0, retained MIT contributions, and separate documentation terms | An external C# server could keep application code in one language | Viable alternative to evaluate against packaging and teaching needs; inspect the exact package's terms |
| [pyRevit](https://github.com/pyrevitlabs/pyRevit) | [GPL version 3 text](https://github.com/pyrevitlabs/pyRevit/blob/fc524b132f13d2d7cb4d085976efd9a1431d469b/LICENSE.txt) | Scripting lesson and existing automation examples | Evaluate independently from the standalone add-in; using an installed tool and copying its code are different reuse decisions |
| [tillbaum/revit-mcp-python](https://github.com/tillbaum/revit-mcp-python) | [MIT](https://github.com/tillbaum/revit-mcp-python/blob/9cc3f195b0e79f511e4ed8bce729402dd659644e/LICENSE) | Compact example of pyRevit Routes plus a Python MCP server | Teaching precedent; its documented prototype status and pyRevit dependency matter to adoption |

MIT candidates still require preservation of their copyright and permission notices when copying substantial portions. Record adapted files and their provenance. A suitable top-level license is one input to the choice, alongside dependencies, maintenance, and successful local tests.

### Dependencies discovered by inspecting source

The existing TypeScript/C# bridge declares Node `>=20.0.0`, `@modelcontextprotocol/sdk`, `ws`, `zod`, and native `better-sqlite3`. Its plugin references `RevitMCPSDK`, Newtonsoft.Json, and versioned Revit API packages. These create installation, version, and license work beyond cloning the repository.

The Nice3point application template uses its custom build SDK plus toolkit, extension, and API packages. Local source inspection also confirms that the SDK selects `net48` for Revit 2024. Optional dependency injection and packaging features add further packages. Its `$(RevitVersion).*` references are floating ranges; a reproducible workshop build needs recorded, locked resolved versions.

The Python precedent declares `mcp[cli]>=1.9.0` without an upper bound. The official Python SDK now documents a v2 migration, including renamed server APIs. An older example plus an unconstrained current install therefore needs compatibility checking. The inspected SDK main-branch manifest requires Python 3.11+, while some indexed documentation still states 3.10+. Match instructions to the release we actually choose. [SDK migration overview](https://github.com/modelcontextprotocol/python-sdk/blob/main/docs/whats-new.md)

These are useful workshop discoveries: read manifests and release notes as well as README summaries.

## How to choose a starting point

| Approach | Advantage | Work we inherit |
|---|---|---|
| Adapt the existing MIT bridge | Existing transport, command routing, and older Revit configurations | TypeScript/Node setup, native dependencies, broad command surface, and dependency review |
| Use an add-in template with a small custom bridge | A compact teaching example tailored to the three questions | We must implement and test request routing, timeouts, document selection, and API-context dispatch |
| Build through pyRevit Routes | Accessible Python example for attendees already scripting | An additional host dependency and prototype limitations described by the precedent |

**Recommended next comparison:** attempt a minimal Revit 2024 connection using the existing MIT bridge, evaluating only a read operation and the code needed to support it. Compare the effort to retain and explain that infrastructure with a small bridge based on the SDK's ExternalEvent example. Make the reuse decision after this evidence. The earlier C#/Python proposal remains provisional.

The intended end-user capabilities remain the same whichever infrastructure wins: explicit scope, model inspection, deterministic readiness checks, quantities, and evidence navigation.

## Resource completion criteria

Before the main implementation, establish:

1. A selected, pinned starting point and written reasons for adopting or rejecting each serious candidate.
2. Host version, build SDK, runtime, API references, and packages with exact resolved versions.
3. A successful local build and a small read request in Revit 2024.
4. An inventory of direct and transitive dependencies and required notices for what we actually distribute.
5. A prepared sample with an independent answer sheet for the three questions.

Keep version claims precise. Revit 2024 uses .NET Framework 4.8; current Autodesk documentation says 2025.5 and 2026.5 move to .NET 10. Supporting “Revit 2025” or “Revit 2026” therefore requires point-release-aware checks. [Autodesk runtime guidance](https://help.autodesk.com/view/RVT/2025/ENU/?caas=caas/sfdcarticles/sfdcarticles/Revit--Requirements-for-products-affected-by-the-Microsoft--NET-10-transition.html)

The earlier domain research's “nothing free fills the gap” claim is not a finding this review establishes. Our case for building rests on these specific workflows and teaching goals, not a claim that no comparable tool exists.
