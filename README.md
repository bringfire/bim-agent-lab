# BIM Agent Lab

**Learn to build useful BIM tools with AI coding agents.**

BIM Agent Lab is a teaching repository for BIM professionals. It pairs working examples with the prompts, research, decisions, source code and verification that explain how we built them. Use it to revisit the seminar, guide your agent, or extend an example for your office.

Revit model readiness is the first project. The lab can grow to other BIM applications and workflows.

## Start here

**Before a hands-on build:** use the [participant installation checklist](docs/reference/participant-install-checklist.md). The current source requires **Microsoft .NET SDK 10.0.204 (Windows x64)**, plus the other listed tools. Revit's installed .NET runtime does not supply that compiler SDK. The checklist distinguishes watching, building/extending and running a future prebuilt package.

| You want to… | Start with… |
|---|---|
| Learn how to engage an agent | [Participant starting guide](docs/start-here.md) and [learning path](docs/tutorials/README.md) |
| Give your agent technical context | [Agent handoff](docs/how-to/agent-handoff.md), [AGENTS.md](AGENTS.md) and the relevant project guidance |
| Build or try the Revit example | [Revit project](projects/revit-readiness/README.md), [prerequisites](projects/revit-readiness/docs/development-prerequisites.md) and [sample preparation](projects/revit-readiness/samples/README.md) |
| Revisit the SWACCA seminar | [Participant PDF](output/pdf/swacca-2026-participant-handout-oct29-30.pdf), [editable Word handout](output/docx/swacca-2026-participant-handout-oct29-30.docx) and [two-session agenda](workshops/swacca-2026/agenda.html) |

Reading the lessons requires no Revit installation. Running the example requires the documented Windows/Revit environment. Cloning the repository installs nothing; build and installation are separate steps.

The repository is **public** at [bringfire/bim-agent-lab](https://github.com/bringfire/bim-agent-lab). No account or invitation is needed to read or clone it. This is a teaching-source preview; downloadable release packaging and full implementation acceptance remain pending. See [sharing status](docs/reference/repository-sharing.md) and the [publication review](docs/reference/public-distribution-review-2026-10-26.md).

Clone it into a folder of your choice. The long-path option avoids Windows checkout failures in deeply nested folders:

```powershell
git clone -c core.longpaths=true https://github.com/bringfire/bim-agent-lab.git
cd bim-agent-lab
```

Open that folder in your coding agent and begin with this suggested prompt:

> Read README.md, AGENTS.md, docs/start-here.md and docs/how-to/agent-handoff.md. Explain what the example does, what has been verified and what remains open. Inspect my Git state and development prerequisites without changing them. Then guide me through one useful next step. Distinguish research, source inspection, offline tests and results observed inside Revit.

## The first example: Revit model readiness

The example checks required **wall and ceiling type information** against an explicit checklist: Code (Type Mark), Fire Rating and Assembly Reference, with recorded exceptions. It reports missing information, affected instances, scope exclusions and incomplete coverage. It does not certify that a fire rating or assembly is correct.

There are two ways to use the same calculation:

- **Run a command in Revit.** Save JSON/text evidence and an Excel workbook with four worksheets: Summary, Findings, Affected elements, and Rules and coverage. The Excel exporter uses ClosedXML.
- **Ask an agent through MCP.** Codex desktop or Claude Desktop Chat requests a fresh model context, then checks that context. The tools are `revit_get_readiness_context` and `revit_check_required_type_information`. These calls read the model; they do not edit or export it.

The prepared samples demonstrate a complete case, wall defects, an additional ceiling defect, a missing declared element and a missing type requirement. **Zero findings is a readiness pass only when the declared scope was completely evaluated.**

Inventory explains what was examined and affected. Physical-condition checks and broader quantity workflows remain future extensions.

## What you will learn

1. **Define the BIM question.** Specify purpose, scope, rules, boundaries, output and independent evidence.
2. **Research before building.** Inspect official APIs, SDKs, libraries, existing repositories, exact versions and licenses. Clone useful precedents and inspect their code.
3. **Direct the agent's Git work.** Inspect the checkout, choose an appropriate branch, review diffs and record coherent checkpoints.
4. **Build one small useful step.** Establish a read-only command before adding reporting or an agent bridge.
5. **Verify independently.** Use native Revit schedules and deliberately prepared cases, including failures and incomplete coverage.
6. **Use independent review.** Hand over a specification, plan and evidence. When several findings recur, ask whether they reveal a shared missing concept.
7. **Leave a usable handoff.** Preserve the participant story and the technical instructions an agent needs to continue.

The [participant lessons](docs/tutorials/README.md) focus on **how we engaged the agent**. Their linked technical guides explain implementation, dependencies, commands, tests and recovery. Suggested teaching prompts are labeled separately from actual development history.

## Compatibility and verification

**This is a teaching project under development. Full implementation acceptance remains open.**

| Area | Recorded baseline and limits |
|---|---|
| Revit | The 2024/net48 route is retained. On October 27 the local candidate passed principal command/Excel and SDK-MCP checks in **Revit 2026.5.1, API build 26.5.10.38**, using separate net10.0-windows host outputs. Earlier 2026/.NET 8 and 2027 remain unverified. See the [current laptop record](projects/revit-readiness/docs/verification/2026-10-27-revit-2026-5.md) for scope and remaining desktop/2024 checks. |
| MCP server | Separate .NET 10 process using official C# MCP SDK **2.2.0**. The SDK version and MCP protocol revision are separate identifiers. |
| Desktop clients | Historical live runs cover Codex desktop and Claude Desktop Chat, using protocols `2025-06-18` and `2025-11-25`. A separate SDK regression retains `2026-07-28`. |
| Results | Retained evidence supports the five sample computations and stale-context rejection/recovery. The manual command and Excel output have their own verification records. |
| Open acceptance | Historical Codex NC2 answer evidence, Codex configuration baseline and acceptance on the frozen current candidate remain open. Desktop Stop and normal-host timing have separate limits. The known SDK token-only cancellation issue remains documented; explicit cancellation controls do not prove desktop Stop behavior. |

These are dated observations. Historical live results do not establish acceptance of a later build. See [development status](docs/reference/development-status.md), the [live matrix](projects/revit-readiness/docs/verification/mcp-readiness.md), [Excel evidence](projects/revit-readiness/docs/verification/2026-10-20-excel-output.md) and the [R3 acceptance ledger](projects/revit-readiness/docs/verification/2026-10-22-r3-acceptance.md).

The author has scheduled Revit 2026 work for October 27. Support will be added to this table after building and verifying that version.

## Repository map

| Folder | Contents |
|---|---|
| `docs/tutorials/` | Participant stories and suggested prompts |
| `docs/how-to/` | Technical procedures for agents and developers |
| `docs/research/` | Source research, dependency decisions and provenance |
| `docs/reference/` | Prerequisites, status, licensing and sharing |
| `docs/build-story/` | Journal of actual prompts, decisions and results |
| `docs/superpowers/` | Specifications, plans and independent reviews |
| `projects/revit-readiness/` | Revit/MCP source, scripts, tests, sample recipes and evidence |
| `templates/` | Reusable briefs, research, lesson and verification records |
| `workshops/swacca-2026/` | October 29–30 seminar materials, agenda and handout sources |
| `output/` | Participant PDF and Word artifacts |

Each project owns its dependencies and host-specific instructions. Autodesk SDK/API binaries and working RVT models are acquired separately; source links and preparation recipes are included. Optional research clones belong in the Git-ignored `references/` folder.

## Extend the lab

Start with [Contributing](CONTRIBUTING.md) and the [extension workflow](docs/how-to/extend-a-project.md). Keep changes small enough to inspect and verify. Record what changed, why it helps a BIM professional, how it was checked and what remains uncertain. Preserve third-party provenance and both teaching layers.

## License

Original BIM Agent Lab code and teaching materials are licensed under the **[MIT License](LICENSE)**. Retain the copyright and license notice when reusing or distributing them.

Third-party code, dependencies, quoted research and assets retain their own terms. See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) and the [licensing guide](docs/reference/licensing.md). The lab's MIT license does not grant rights to Autodesk products, API binaries or sample models acquired separately.
