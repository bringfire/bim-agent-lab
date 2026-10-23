# Start here: use BIM Agent Lab with your agent

BIM Agent Lab teaches you to direct an AI coding agent to build useful BIM tools. You bring the work question, the rules and the judgment about whether the result is useful. Your agent researches, inspects code, builds and tests, and leaves an explanation you can revisit.

You can read the lessons without installing Revit. To try the working example, use an agent that can work with files on your Windows computer and follow the setup path below. You do not need to memorize terminal commands; ask the agent to perform the technical steps and explain the evidence.

## What you can try today

The first example checks **required wall and ceiling type information** in Revit 2024 against a written checklist. It reports missing values, the elements they affect, what was examined, and what could not be checked.

There are two ways to use it:

- **Revit command:** run the required-information check and receive saved evidence and an Excel workbook with four worksheets. This is the separate export workflow.
- **Desktop agent:** ask a question in Codex desktop or Claude Desktop Chat. Two MCP tools let the agent identify the active model and request the same readiness calculation. MCP is the connection that lets the agent call these tools. These calls return evidence; they do not edit the model or export a workbook.

As of October 22, 2026, both desktop applications passed the five prepared sample cases below and rejected an old context after a model switch. A context is a temporary reference to the model and checklist the agent is about to check. The [recorded results](../projects/revit-readiness/docs/verification/mcp-readiness.md) establish this for the tested configuration. Queued shutdown and installation recovery have also passed. The [review packet and acceptance ledger](../projects/revit-readiness/docs/verification/2026-10-22-r3-acceptance.md) distinguish those results from the remaining live-build, cancellation, native-operation and reproduction-detail gaps. Independent review has returned and keeps acceptance open: correct tool data was retained, but Codex’s corresponding incomplete-coverage explanations were not fully captured. See the [review response](superpowers/reviews/revit-mcp-bridge/2026-10-22-r3-feedback.md). This is a working teaching example, not a finished public release. Physical-condition checks and quantities are future extensions.

## 1. Give your agent the repository

Use [bringfire/bim-agent-lab](https://github.com/bringfire/bim-agent-lab), or the folder supplied with the seminar. The repository was created private on October 23, 2026; participant access has not yet been announced. Ask your agent to clone it into a new local working folder once access is available, or open your existing copy. Preserve existing work and inspect the branch before making changes. The [printed packet](../output/pdf/swacca-2026-participant-handout.pdf) provides a short refresher.

**Suggested starting prompt** — written for participants, not a quotation from the original build:

> Read README.md, AGENTS.md, docs/start-here.md and docs/how-to/agent-handoff.md in this repository. Explain what currently works and what still needs verification. I want to learn how to direct you and then try the Revit readiness example. Inspect my Git state and development prerequisites. Use the repository's technical guides, check current official sources where versions matter, and tell me what you can do and what I need to do in Revit. Start with inspection; report any software or setup changes needed before making them.

Keep the [agent handoff](how-to/agent-handoff.md) available for a new chat or another agent. It supplies the technical detail behind these steps.

## 2. Start with research and Git

Begin by asking the agent to find existing work, read its source, and collect the resources needed to build and test. That includes the host API, SDKs, libraries, dependencies and their licenses. An old article or familiar version number is a starting point for research, not proof of current compatibility.

**Suggested research prompt:**

> Research what already exists for this capability. Use official documentation and relevant GitHub repositories. Clone the promising source repositories into the lab's ignored references folder, inspect the relevant code and licenses, and record their exact revisions. Explain which pieces we would use as dependencies, adapt, or use only to learn from. Check versions against the software we actually need to run.

Git records a sequence of changes you can inspect and revisit. Ask the agent to work on a suitable feature branch, show you the diff, and commit a coherent, verified step with its teaching notes. Publishing to a shared repository is a separate action. You do not have to type Git commands yourself, but you should explicitly direct the agent to use Git. See the [research story](tutorials/00-research-before-building.md) and [Git story](tutorials/01-directing-an-agent-with-git.md).

## 3. Let the agent prepare your workstation and a sample

For the full example you need Windows, full Revit 2024, the matching development tools, Git, PowerShell, Python for verification, and an authenticated desktop agent. The [prerequisites](../projects/revit-readiness/docs/development-prerequisites.md) distinguish what builds the add-in from what runs the MCP server. Your agent should check your machine rather than assume the author's setup is present. The sample model, Autodesk API libraries and optional research clones are obtained separately; they are not bundled in Git.

**Suggested setup prompt:**

> Use the agent handoff to prepare this example on my machine. Explain the missing prerequisites, then build and test the chosen version. Prepare recovery before installing or changing client settings. Use a disposable copy of the documented Autodesk sample and its matching checklist. Verify expected results independently before asking me to try the desktop tools. Record the commands, versions and results. Give me one clear Revit action at a time.

The agent can build, compare results and prepare files. You handle Revit's normal UI and any trust/permission prompts. It should give you the **exact full model path** and menu steps. In this build, selecting a recent-model tile opened the wrong copy; the full path resolved it. If two documents are open, ask the agent which document must be active and use Revit's Switch Windows menu to activate a view in that document. A similarly named view is not proof of the active model.

Do not reopen Revit merely to read this guide. During setup, the agent will tell you when Revit must be closed for installation and when to open the sample and choose **Add-Ins → External Tools → BIM Agent Lab – Start readiness bridge**.

## 4. Ask a useful question and check the answer

Once your agent has verified the setup, use this **suggested check prompt** in the chosen desktop application:

> Get a fresh readiness context from Revit and check required wall and ceiling type information. Report the full active model path, declared scope, requested and examined counts, missing findings and affected instances, both completeness flags, scope issues and unexamined cases. Explain whether the declared scope passes and why. Do not edit or export anything.

The prepared demonstration has 16 declared instances across six types. Its expected results are:

| Prepared case | Expected result | What you learn |
|---|---|---|
| Complete | 16 examined, zero missing findings; both completeness flags true | The declared scope passes |
| Wall defects | Two missing values affecting six walls; coverage complete | The check detects deliberate defects |
| Ceiling negative | Three missing values affecting eight instances; coverage complete | The additional ceiling defect is detected |
| NC1: declared element absent | 15 of 16 examined; zero findings; coverage false, evaluation complete true | A missing element prevents a scope pass |
| NC2: type has no requirement | 16 found; two ACT ceiling instances unexamined by the evaluator; both flags false | A missing rule prevents a scope pass |

**Zero findings alone does not mean ready.** A pass applies only to the stated scope and checklist. The demonstration excludes other host elements and links; it does not establish that the whole project is ready. Native Revit schedules and separately prepared expected answers help check what the agent reports. The [desktop testing story](tutorials/14-test-readiness-through-desktop-agents.md) explains how we directed those comparisons and the timed model-switch test.

## 5. Extend one useful capability

**Suggested extension prompt:**

> I want to add [one BIM question] for [a specific use]. Inspect the existing flow and research the required API or dependency. Define scope, required information, valid exceptions and independently checkable passing, failing and incomplete cases. Write a spec and implementation plan for review before coding. Then implement the agreed change, verify it, show the diff and record both the participant story and agent procedure.

Use an independent reviewer for the spec, plan and implemented result. If review finds two or three major issues, ask whether they share a missing concept; the count is a useful signal, not proof. In this build, repeated cancellation defects exposed a missing model of which work shutdown waits for. Ask the reviewer to explain that connection so the coding agent can correct its reasoning across the feature, then verify the correction. The [review lesson](tutorials/06-direct-design-and-external-review.md) includes the actual exchange.

For a refresher, follow the [learning path](tutorials/README.md). For the exact build procedure, use the [agent handoff](how-to/agent-handoff.md). Continue the remaining verification and review steps before polishing the seminar presentation or treating the repository as a release.
