# Choose and verify an MCP bridge: agent guide

For coding agents and technical readers. The [participant story](../tutorials/05-research-and-choose-the-mcp-bridge.md) records the author's direction. Read root/project `AGENTS.md`, [current status](../reference/development-status.md), and the [dated comparison](../research/revit-mcp-bridge-comparison.md). This guide records the research increment; its implementation guidance is proposed acceptance, not a completed server or an instruction to silently install one during research. The subsequent [approved spec and written implementation plan](../superpowers/reviews/revit-mcp-bridge/README.md) now govern the next stage, with all four R2 findings externally closed at the plan level on revision 2 (`cfbfe04`), awaiting author plan approval/execution choice.

Baseline: `4784b24`. The [version/source observation](../research/evidence/mcp-version-observations-2026-10-20.json) supports the recommendation to test an original net48 Revit bridge with an external C# server using `ModelContextProtocol` 2.2.0. Recheck current releases before adopting that pin in a later implementation. Historical package evidence is not a fresh compatibility pass.

## 1. Check versions at every boundary

Record the observation date and keep these separate:

1. Official protocol revision: follow the [latest specification](https://modelcontextprotocol.io/specification/latest) and record its resolved dated URL and release status. At this check it was `2026-07-28`.
2. SDK release: check the selected language's official release, prerelease flag, tag, and full commit.
3. Published package: confirm the registry actually contains that version, with expected framework/runtime requirements and license metadata.
4. Existing implementation: inspect both manifest ranges and lockfile versions. A default branch, release tag, package publication, and restored dependency graph may differ.
5. Host/client compatibility: retain the required Revit target and record the protocol revision actually used by the intended client. JSON-RPC 2.0 and a server's product version are not MCP protocol revisions.

Read-only PowerShell examples from any working folder:

```powershell
Invoke-RestMethod 'https://api.github.com/repos/modelcontextprotocol/csharp-sdk/releases/latest' |
    Select-Object tag_name, published_at, prerelease, html_url, target_commitish
Invoke-RestMethod 'https://api.nuget.org/v3-flatcontainer/modelcontextprotocol/index.json'
Invoke-RestMethod 'https://registry.npmjs.org/@modelcontextprotocol%2fsdk/latest' |
    Select-Object name, version, engines, license
Invoke-RestMethod 'https://registry.npmjs.org/@modelcontextprotocol%2fserver/latest' |
    Select-Object name, version, engines, license
Invoke-RestMethod 'https://pypi.org/pypi/mcp/json' |
    Select-Object -ExpandProperty info |
    Select-Object name, version, requires_python, license
```

Inspect prerelease/yanked status and release notes; do not treat a registry's last element or a search snippet as universal evidence of latest stable compatibility. Resolve a release tag to a commit using Git or the GitHub commits endpoint. The bridge's `^1.7.0` range resolved to 1.26.0 in its lockfile, while the old npm package's latest was 1.32.1 and the new server package was 2.3.1. The 2024 RevitMCPSDK helper line also differs from its highest overall version.

Inspect upgrade guides and exact license/notices, then pin and lock selected packages after reviewing the resolved graph. The desired workflow is current-source research followed by reproducible builds. Do not use floating `latest` installation commands as the lab's repeatable build specification.

## 2. Inspect pinned source without disturbing earlier evidence

Use the [Git guide](work-with-git.md) and [clone procedure](clone-research-repositories.md). The original bridge checkout at `D:\SWACCA_BIM\references\mcp-servers-for-revit` was reused unchanged. That is authoring provenance; future machines should use their own manifest paths. New SDK inspection copies were placed under ignored `references/`.

| Inspected source | Pin | Local research directory used |
|---|---|---|
| Existing Revit bridge | `86cf7057785f2423fc0d0892c9a5d9c93deb1aa3` | Original authoring checkout above |
| TypeScript SDK v2.3.1 | `fcef852f6cdafea07f36cf6fae69b37c8adf85be` | `references/typescript-sdk-2.3.1` |
| Python SDK v2.3.0 | `2118f14f8a19bc158d8a1cf90af58d85d187f849` | `references/python-sdk-2.3.0` |
| C# SDK v2.2.0 | `6fa3825973949a9c4f0cd8af344e15a8db09dc35` | `references/csharp-sdk-2.2.0` |
| RevitMCPSDK 2024.0.0.5 source | `2d46e0c4965fff73f3677ab8eccadd21c1edd769` | `references/RevitMCPSDK-2024.0.0.5` |

For example, from the lab root, only when the destination is absent:

```powershell
git clone --depth 1 --branch v2.2.0 https://github.com/modelcontextprotocol/csharp-sdk.git references/csharp-sdk-2.2.0
git -C references/csharp-sdk-2.2.0 rev-parse HEAD
git -C references/csharp-sdk-2.2.0 status --short
```

Inspect source instructions in their scope. Do not run upstream builds just to read source: the existing bridge's build targets include deployment operations. Retain observed origins/HEADs and file hashes; compare Git-blob and working-file bytes with line-ending effects understood. Keep old research snapshots intact when writing a new dated observation.

Trace the full existing route: MCP registration -> TypeScript connection manager/socket client -> C# socket service/command registry -> command -> RevitMCPSDK ExternalEvent base -> Revit handler. The comparison links each source. The 2024.0.0.5 helper package's nuspec identifies its source commit; that pin is package provenance, not a claim that the bridge's floating graph was restored here.

## 3. Choose the smallest useful experiment

The preferred experiment has two named tools: one to describe the current Revit/document/checklist context, and one to run the existing required-information check for that context. Use an explicit allowlist. No arbitrary code, file path supplied by a model, editing command, or whole-model inventory is needed for the first slice.

Proposed source boundaries:

| Boundary | Existing source or work to add |
|---|---|
| MCP protocol and process | New executable under `projects/revit-readiness/src/mcp-server/`; official C# SDK, stdio, diagnostics on stderr |
| Revit bridge lifecycle | New startup/stop integration in the net48 add-in; initialize/dispose ExternalEvent in valid Revit context |
| Request queue and IPC | New framed, bounded, per-instance local channel; only named operations and plain DTOs cross it |
| BIM observation | Reuse [RequiredInformationReader.cs](../../projects/revit-readiness/src/revit-addin/RequiredInformationReader.cs) and [Checklist.cs](../../projects/revit-readiness/src/readiness-core/Checklist.cs) inside Revit |
| Deterministic evaluation and evidence | Reuse [TypeInformation.cs](../../projects/revit-readiness/src/readiness-core/TypeInformation.cs) and [InformationReport.cs](../../projects/revit-readiness/src/readiness-core/InformationReport.cs) |
| Existing UI and exports | Preserve [RequiredInformationCommand.cs](../../projects/revit-readiness/src/revit-addin/RequiredInformationCommand.cs) and its verified reports; do not invoke its TaskDialog-driven `Execute` from a background request |

The first MCP query should return a fresh structured report without creating files. Report export can be a separately scoped operation later; model-read-only and filesystem side effects must be described accurately. Do not substitute reading an old JSON file for a live Revit query. Compare against retained reports only as acceptance evidence.

Target a separate modern .NET server, initially net10.0 for the installed toolchain, while retaining Revit 2024/net48. Inspect the actual runtime/publish requirements and dependency graph before adoption. A candidate package reference is exact `[2.2.0]`; commit the resolved lockfile and use locked restoration. The official package's netstandard2.0 asset does not require us to load it into Revit. The new server must not reference Autodesk DLLs or assume net48-only core assemblies can be loaded directly into its process.

If adaptation of the TypeScript bridge is selected instead, pin the split v2 packages, migrate imports and `.tool()` registrations/schema definitions, choose the documented current-protocol/dual-era stdio entry point, and test error semantics. Review the diff of any migration utility. A package-name/version change alone is not an accepted migration.

## 4. Specify the Revit request lifecycle

Create an ExternalEvent in valid Revit API context; the listener thread only parses/validates messages and queues plain request data. The handler accesses `UIApplication`/`Document` and executes the read when Revit permits it. See [Autodesk's execution model](https://help.autodesk.com/cloudhelp/2024/ENU/Revit-API/files/Revit_API_Developers_Guide/Advanced_Topics/Revit_API_Revit_API_Developers_Guide_Advanced_Topics_External_Events_html.html).

The existing command's `TransactionMode.ReadOnly` attribute does not automatically protect a new ExternalEvent handler. Establish that boundary separately: no transactions or modifying API calls in the query path, focused source review, modified-state checks, and independent before/after model evidence.

Use a bounded queue with immutable per-request parameters, correlation IDs, operation/contract version, deadline, and cancellation state. Handle the result of `ExternalEvent.Raise()` explicitly. A timeout must invalidate queued work and prevent a late result from being presented as a fresh answer. Once an API call is executing, cancellation may only be observed after it returns; do not promise forced interruption or kill Revit. Cancel/remove pending work and close IPC cleanly on shutdown.

Return a context token identifying the selected Revit process/session and open document, including project identity and path/checklist identity. Revalidate it in the handler immediately before reading. A copied model can share project identity, and the active document can change while a request waits. Reject mismatches rather than querying whichever document happens to be active. Keep file-path resolution on the trusted Revit side, using the established sidecar convention.

Use explicit message framing and a size limit. For the named-pipe experiment, enforce the intended user/session/local access and select the specific Revit instance; do not assume a pipe name alone prevents remote access. Test split/coalesced reads and messages larger than 8192 bytes. If a report exceeds the supported size, return an explicit failure or a deliberately designed complete paging mechanism; never silently truncate a report marked complete.

Keep the existing report evidence: document/checklist identities, generated time, scope and exclusions, requirement sources, raw values, field/rule/type/case/element identities, completeness, findings, and distinct affected counts. Return structured failures for unavailable Revit, unavailable document, changed context, missing checklist, busy queue, deadline, and unsupported operation. A successful tool response must not contain only a failure sentence; use the SDK's error/structured-result facilities appropriately. Tool read-only annotations describe intent but do not enforce it.

## 5. Verify protocol and BIM behavior separately

No checks below have been run for a lab MCP server yet.

| Gate | Required evidence |
|---|---|
| Dependency/build | Exact SDK and resolved packages, licenses/notices and advisory review, locked restore, modern server build, unchanged existing command tests |
| Protocol, independent of Revit | Tools list/schema, current-revision discovery/per-request exchange, real tool call, structured errors, shutdown, stdout containing only protocol messages |
| Legacy compatibility | Deliberate older-client initialization/call if required; record actual revision separately from the modern test |
| IPC, independent of Revit API | Framing, limits, identity, correlation, queue saturation, deadline/cancellation, connection loss, no cross-talk or stale response |
| Revit lifecycle | No project/family/unsaved project, missing/mismatched checklist, document switch while queued, two clients, selected Revit instance, busy/modal Revit, shutdown/restart |
| Readiness equivalence | Three sample copies: 0/0, 2/6, 3/8; compare values, IDs, rule/source, scope and completeness against independent schedules/answer sheet |
| No mutation | Modified-state and file-hash evidence for the query, with preparation edits kept separate |
| Actual seminar client | Recorded client version/configuration, protocol revision actually used, discovery and end-to-end call into the intended Revit process |

Use the [existing readiness acceptance](../../projects/revit-readiness/tests/required-information.md) and [independent expected answers](../../projects/revit-readiness/samples/expected-results/required-information.md). The official SDK source supports evaluating both protocol eras, but its tests and our source inspection do not establish the installed client's compatibility. Latest-spec support and tested fallback are separate claims.

## 6. Record the decision and its limits

Update the [research record](../research/revit-mcp-bridge-comparison.md), participant story, journal, and current status with actual evidence. Preserve the distinction between the author's request to compare/update and the agent's architecture recommendation. Link a real implementation checkpoint only after implementation exists; this research begins from `4784b24` and will have its own documentation checkpoint.

Refresh version observations at dependency selection, when upgrading, and before producing a seminar release. If primary metadata, release artifacts, and installed behavior disagree, retain the discrepancy and investigate it. Keep SDK inspection trees and downloaded packages ignored. Do not describe a research-only version review as an upgraded or verified bridge.

## Execution checkpoint

The author approved plan revision 2 and selected Native execution. The [agent compatibility procedure](verify-desktop-mcp-compatibility.md) now has runnable SDK/stream/settings tests and [actual desktop evidence](../../projects/revit-readiness/docs/verification/mcp-client-compatibility.md). The SDK harness passes 2026-07-28; Codex sends 2025-06-18 and Claude sends 2025-11-25, both rejected before discovery. Resolve CP0 before Task 2. The [dependency review](../research/mcp-sdk-dependencies.md) records actual package consumption and source adaptation; earlier comparison statements describe the research stage.
