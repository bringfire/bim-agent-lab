# How we directed the agent: prepare a lasting handout

**For participants.** The seminar is short; the handout helps you remember what to ask your agent, while the repository supplies the detailed technical steps. The [packet source and agent guide](../../workshops/swacca-2026/handout/README.md) explain how to rebuild and verify it.

## Give the artifact a clear audience and constraint

After discussing beginner Dynamo and pyRevit routes, the author redirected the work to the printed packet. Asked about print constraints, the author specified **"US Letter, black-and-white friendly, about 8 pages"**. Those constraints shaped an eight-page field guide with reusable prompts, a result-reading table and writing space.

The agent used the existing two-hour agenda, participant stories and verified sample expectations. It kept SDK/runtime details in the repository and labeled new reusable prompts as suggestions. It also kept a distinction participants need: Dynamo is included in the selected Revit environment; pyRevit is an optional separate installation. The author installing it did not establish that every attendee should do so, or that either quick demo had been rehearsed.

## Give people a stable place to continue

The agent found that the local Git repository had no remote. Asked what address the packet should use, the author's actual direction was:

> we should create a remote repo that we can make public and disseminate.

The agent created [bringfire/bim-agent-lab](https://github.com/bringfire/bim-agent-lab) privately, inspected tracked history before uploading, and used that address for the printed link and QR code. A hosted copy, public access and a finished plugin release are separate decisions. Original-material licensing and a public-distribution review remain pending; the packet states its current access status.

## Ask for evidence about the artifact itself

The agent generated a PDF from editable content, rendered all eight pages and inspected the actual layout. An initial overflow was corrected. Page count, Letter size, text bounds, embedded fonts and link destinations were checked. That establishes a reviewed print layout; it does not establish a physical printer test, a phone scan or a successful Revit scripting demonstration.

**Suggested prompt for your own workshop:**

> Prepare a handout for [audience], using [page count, paper size and printing constraints]. Focus on what participants need to decide and ask the agent. Link the technical procedures instead of compressing them onto every page. Keep examples and verification claims faithful to the evidence. Retain editable source, render every page and check the final layout. Identify what must be settled before distribution.

The [technical guide](../../workshops/swacca-2026/handout/README.md) links the sources, generation commands and remaining preparation steps. The [hosting record](../reference/repository-sharing.md) separates private upload checks from future public release.
