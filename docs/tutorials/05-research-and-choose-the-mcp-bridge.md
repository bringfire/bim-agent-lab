# How we directed the agent: choosing a current MCP bridge

**For seminar participants.** Learn to ask for a source-backed comparison, check what “latest” means, and choose the next experiment without mistaking a proposal for a working integration. The [agent guide](../how-to/choose-and-verify-an-mcp-bridge.md) contains the technical procedure; the [research record](../research/revit-mcp-bridge-comparison.md) contains the dated versions, source findings, and recommendation.

We started from `4784b24`, with a working Revit readiness check and Excel output. The next goal was to let an agent request that same check through MCP. This chapter records research and a proposed direction. No MCP server has yet been built or run in Revit.

## Ask for a comparison around proven work

The author's actual prompt was:

> Ok, let's move on to the MCP server. Compare the existing Revit bridge with a minimal custom bridge around our readiness check, and record the research and decisions in both teaching layers. We need to update he MCP bridge to MCP 2.0 or whatever the latest is and this teaching note about checking for the latest versions is important.  Start that research now.

The author supplied both alternatives and kept the business operation fixed: our verified readiness check. The request also made current-version research a teaching requirement. The agent's job was to investigate the alternatives and recommend a next step, including what would need to change to use current MCP technology.

An MCP server gives an agent named operations it can discover and call. A Revit bridge passes those requests into Revit's permitted execution context. The readiness rules still decide the result. Adding the connection should not change what “required,” “affected,” or “unexamined” means.

## Have the agent check the meaning of “latest”

The agent checked official documentation, release pages, package registries, and source at exact released revisions. On 2026-10-20, the latest official protocol revision was **2026-07-28**. The SDKs—the libraries used to implement that protocol—had their own versions: TypeScript **2.3.1**, Python **2.3.0**, and C# **2.2.0**. JSON-RPC **2.0**, seen inside messages, is another identifier. The [version record](../research/revit-mcp-bridge-comparison.md) links the supporting sources.

The author's “2.0 or whatever the latest is” was useful direction to verify, rather than an exact technical specification to copy blindly. Participants should expect the agent to resolve that ambiguity and explain the consequence.

Three discoveries made this concrete:

- The existing bridge's dependency file allowed an older SDK range, while its lockfile—the exact resolved package record—selected version 1.26.0.
- The old TypeScript package name still had a latest 1.x release. The v2 server library had a different package name, so asking a package manager to update the old name would not complete the migration.
- Installing a v2 SDK would not alone prove that the client and server exchanged messages using the latest protocol. The migration guide described additional setup, and actual interoperability still needed testing.

Another example came from Revit itself: the helper library's highest published version targeted a newer Revit year. Our tool still needed the compatible 2024 line. Checking for updates includes deciding which updates fit the project.

## Compare the work we would inherit

| Option | Useful starting point | Work still required |
|---|---|---|
| Adapt the existing bridge | Command routing, Revit integration patterns, and a broader tool set | Update MCP integration, narrow the offered operations, review connection handling, and add our exact readiness check |
| Build a small bridge around our check | Our existing reader, rules, expected answers, and report format | Create and test the connection, request handling, document selection, and client setup |

The agent traced an actual request through the existing source, including its dispatch dependency. It found specific questions about message boundaries, timeouts, document selection, and how failures are returned. These findings affect whether a request can reliably produce the right BIM evidence. They are source observations; the agent did not claim to have reproduced failures in Revit.

The recommendation is a small original Revit bridge and a separate C# server using the current official SDK. It keeps the first tool focused and uses the .NET workflow we already know. Existing bridge code remains a precedent. Python and TypeScript remain viable alternatives; the recommendation follows this example's scope and setup.

That architecture recommendation belongs to the agent. The author directed the comparison and current-version requirement; the author did not prescribe a particular communication mechanism or SDK language in this prompt.

## Ask what would prove the recommendation works

The next experiment should let an agent request one check on the intended open model and get the same **0/0, 2/6, and 3/8** findings/affected-instance results already verified through the Revit command. It must retain identities, requirements, exclusions, and incomplete coverage. A successful connection alone is not enough.

We also need clear answers when Revit is closed, the wrong document is active, a checklist is missing, or a request times out. The server should report those conditions without turning them into a misleading zero-finding result. The actual client's protocol version should be visible in verification evidence.

## Reuse the version-checking habit

This is a **suggested prompt**, not another historical instruction:

> Before choosing or upgrading this dependency, check the current official specification, stable releases, package registry, and migration guide. Compare them with our manifest and lockfile. Explain which versions are compatible with our host and client, what changed in the API and license terms, and what tests would prove the update works. Pin the selected versions and record the date and evidence in both teaching layers.

For practice, ask your agent to explain the difference between the existing bridge's declared SDK range, locked version, and newest compatible replacement. Then ask what observable result would establish a successful upgrade. The [agent guide](../how-to/choose-and-verify-an-mcp-bridge.md) gives the repeatable checks and acceptance plan.

In the next increment, the author selected the proposed C# bridge and requested a spec, implementation plan, and external review using superpowers brainstorming. Follow [the design/review story](06-direct-design-and-external-review.md) for those actual prompts and current stage. This research lesson remains the record of the earlier comparison, not evidence that a bridge has been built.
