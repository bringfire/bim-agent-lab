# Clone and inspect research repositories

Technical procedure for agents and readers following the [research guide](research-existing-solutions.md) or [Git guide](work-with-git.md). The [research participant story](../tutorials/00-research-before-building.md) and [Git participant story](../tutorials/01-directing-an-agent-with-git.md) explain why the author asked for these operations. Follow the current request and repository instructions; do not rerun historical clone work unnecessarily.

The [repository manifest](../research/repositories.json) records seven source candidates and exact inspected commits. These are research pins, not selected application dependencies. The [resource register](../research/revit-resources.md) explains their potential roles and license observations.

From the BIM Agent Lab root, give your agent this suggested prompt:

> Read docs/research/repositories.json. Clone the candidates we need into their listed local paths. Check out the recorded commits, verify each origin and HEAD, and report the working-tree state. Preserve any existing checkout without resetting it. Read upstream project instructions, licenses, dependency manifests, and the source relevant to our question. Keep this pass to source inspection and record any missing submodules.

For one candidate, the corresponding commands are:

```powershell
git clone --filter=blob:none --no-checkout https://github.com/mcp-servers-for-revit/mcp-servers-for-revit.git references/mcp-servers-for-revit
git -C references/mcp-servers-for-revit checkout --detach 86cf7057785f2423fc0d0892c9a5d9c93deb1aa3
git -C references/mcp-servers-for-revit remote get-url origin
git -C references/mcp-servers-for-revit rev-parse HEAD
git -C references/mcp-servers-for-revit status --short
git -C references/mcp-servers-for-revit submodule status
```

Run the clone command only when its destination does not exist. For an existing checkout, first inspect its origin, branch, and local changes. The expected HEAD is the full commit above; a clean checkout has no entries in `status --short`. Detached HEAD is suitable for a pinned research copy; create a development branch or separate project when deliberately adapting source.

The `references/` directory is ignored by the lab. Partial cloning fetches file contents as needed. Submodules are additional repositories and are not fetched by these commands. The original inspection found ten uninitialized submodules across RevitLookup and pyRevit; a future build must determine which are needed.

Use the [code map](../research/reference-code-map.md) to locate useful entry points. Record findings with the [research template](../../templates/research-record.md). A clone and source review do not establish build compatibility, runtime correctness, or a decision to redistribute code.
