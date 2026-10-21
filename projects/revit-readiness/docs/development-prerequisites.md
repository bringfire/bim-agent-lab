# Minimum development requirements for a Revit add-in

Checked against official documentation: 2026-10-20. First lab target: **Revit 2024, C#, .NET Framework 4.8 (`net48`)**. The [first probe](../../../docs/tutorials/02-build-a-read-only-revit-command.md) has now been compiled and run on Revit build `24.3.60.12`. Other releases need their own verified requirements. The [participant setup story](../../../docs/tutorials/02-build-a-read-only-revit-command.md) explains how the author requested the proof; the [agent setup guide](../../../docs/how-to/build-read-only-revit-command.md) contains its reproducible procedure.

For an agent-assisted first add-in, start with a Windows Revit workstation, a C# build toolchain, the matching framework references, the Revit API libraries, Git, and a coding agent that can work in the local repository. A basic command needs no additional third-party runtime library.

## What each participant needs

| Requirement | What to obtain or confirm | Why it is needed |
|---|---|---|
| Windows workstation | A supported 64-bit Windows configuration for the selected Revit version; use Windows 11 x64 for our proposed participant setup | Revit hosts and executes the add-in |
| Full Autodesk Revit 2024 | An installed, activated copy the participant is entitled to use; record its exact update/build | Required to load and test the add-in against the actual application. Revit LT does not support add-ins |
| C# build tools | Windows x64 .NET SDK matching the project's `global.json`; tested with 10.0.204 | Supplies the compiler, MSBuild, and `dotnet` commands used by the agent |
| .NET Framework 4.8 reference assemblies | The 4.8 Developer Pack is the straightforward installer route; a project can alternatively obtain Microsoft's reference-assembly NuGet package | Supplies the framework API definitions needed to compile for `net48` |
| Revit API libraries | `RevitAPI.dll` and `RevitAPIUI.dll` from the Revit 2024 installation | Supplies the application-specific interfaces used by our code |
| Git for Windows | Git available to the agent, with a commit name and email configured | Supports source research, reviewing changes, and recording the build process |
| One local coding agent | An authenticated agent with permission to read/write the working folder and run PowerShell, Git, and build commands | Can inspect resources, write source, compile, and interpret errors |
| PowerShell 7, 64-bit, for bridge installation/recovery | Use `pwsh` for the Task 7 build/install/undo procedures | The recovery module uses modern .NET file/hash APIs; Windows PowerShell 5.1 is not its supported host |
| Local working folder and test model | A writable checkout of BIM Agent Lab and a disposable local Revit project | Holds source/history and provides a place to test reads without risking project deliverables |

Autodesk lists 16 GB RAM and 30 GB free disk space for Revit 2024's entry-level configuration. Allow additional space for development tools, repositories, and models. Its balanced configuration uses 32 GB RAM. See the complete [Revit 2024 system requirements](https://www.autodesk.com/support/technical/article/caas/sfdcarticles/sfdcarticles/System-requirements-for-Revit-2024-products.html). [Revit LT limitation](https://help.autodesk.com/cloudhelp/2024/ENU/Revit-Installation/files/GUID-4159B7E9-3AFF-4474-8E86-35A4547AB084.htm).

Git is a requirement of our teaching workflow, not of the Revit compiler. A local coding agent is required for our agent-assisted method, not for running the finished add-in. A browser chat alone needs another way to edit local files and execute builds. The chosen agent may have its own installer prerequisites, authentication, and network requirements; follow its current installation guide. A GitHub account is useful for publishing or contributing, but local Git history does not require one.

## Choose one build route

**Lean agent route:** install a supported [.NET SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) for Windows x64 and the [.NET Framework 4.8 Developer Pack](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48). The agent can create an SDK-style C# class library targeting `net48` and build it with `dotnet build`. A separate editor is optional if the agent application already provides file and diff views. Microsoft documents [building libraries with the CLI, including .NET Framework targets](https://learn.microsoft.com/en-us/dotnet/core/tutorials/libraries).

**Visual Studio route:** use Visual Studio with the **.NET desktop development** workload and the **.NET Framework 4.8 targeting/development components**. This adds an integrated editor and debugger. Build Tools for Visual Studio provides command-line tools without the full editor; its corresponding workload is **.NET desktop build tools**. Check the [Microsoft component list](https://learn.microsoft.com/en-us/visualstudio/install/workload-component-id-vs-build-tools?view=vs-2022) and use an edition appropriate to your organization's license. These are alternative ways to supply build tools; installing all of them is unnecessary.

Three names must remain distinct:

- **.NET SDK:** the tools that build the project.
- **.NET Framework 4.8 targeting references:** the framework APIs that the Revit 2024 project is allowed to use.
- **Runtime:** the environment that executes the compiled code inside Revit.

Installing a modern .NET SDK does not change Revit 2024's add-in target to .NET 10. Autodesk specifies [.NET Framework 4.8 and the two Revit API assemblies](https://help.autodesk.com/cloudhelp/2024/ENU/Revit-API/files/Revit_API_Developers_Guide/Introduction/Getting_Started/Welcome_to_the_Revit_Platform_API/Revit_API_Revit_API_Developers_Guide_Introduction_Getting_Started_Welcome_to_the_Revit_Platform_API_Development_Requirements_html.html). Having a runtime installed does not establish that the development reference assemblies are present. [Microsoft's developer-pack guidance](https://learn.microsoft.com/en-us/dotnet/framework/install/guide-for-developers) explains the distinction.

The strictly necessary build input is the correct reference assemblies. A separately installed Developer Pack is not mandatory when the project obtains them through [Microsoft.NETFramework.ReferenceAssemblies](https://learn.microsoft.com/en-us/dotnet/framework/migration-guide/reference-assemblies). That alternative introduces a build-time NuGet dependency and package restoration; it does not add a runtime library to ship with the plugin. Record the chosen route and resolved versions.

The probe uses installed 4.8 targeting references and zero NuGet packages. Its [global.json](../global.json) selects SDK `10.0.204`, allowing later patches within the `10.0.2xx` feature band. Other feature bands require an intentional pin change and repeat verification. See the [observed build and host result](verification/2026-10-20-read-only-probe.md); this pin records the tested toolchain, not a claim that it is the newest SDK.

## API libraries and the files the agent creates

The API libraries are [installed with Revit](https://help.autodesk.com/cloudhelp/2024/ENU/Revit-API/files/Revit_API_Developers_Guide/Introduction/Getting_Started/Welcome_to_the_Revit_Platform_API/Revit_API_Revit_API_Developers_Guide_Introduction_Getting_Started_Welcome_to_the_Revit_Platform_API_Installation_html.html). For a default installation, reference:

```text
C:\Program Files\Autodesk\Revit 2024\RevitAPI.dll
C:\Program Files\Autodesk\Revit 2024\RevitAPIUI.dll
```

Set **Copy Local = false** for both references, as in Autodesk's [Hello World walkthrough](https://help.autodesk.com/cloudhelp/2024/ENU/Revit-API/files/Revit_API_Developers_Guide/Introduction/Getting_Started/Revit_API_Revit_API_Developers_Guide_Introduction_Getting_Started_Walkthrough_Hello_World_html.html). Read the matching API XML documentation alongside them. Store a configurable local installation path rather than committing Autodesk DLLs.

The agent produces these project artifacts; participants do not need separate software for each:

| Artifact | Purpose |
|---|---|
| `.csproj` | Declares target framework, API references, and build settings |
| C# source | Implements the first `IExternalCommand`; use a read-only command for the lab probe |
| Compiled plugin `.dll` | Code Revit loads into its own process |
| `.addin` XML manifest | Identifies the DLL, entry-point class, and command ID |

For a per-user Revit 2024 installation, place the manifest in `%APPDATA%\Autodesk\Revit\Addins\2024\` and point it to the plugin DLL's actual local path. The manifest must match the built class and target year. Autodesk describes the [per-user and all-user registration mechanism](https://help.autodesk.com/cloudhelp/2024/ENU/Revit-API/files/Revit_API_Developers_Guide/Introduction/Getting_Started/Using_the_Autodesk_Revit_API/Revit_API_Revit_API_Developers_Guide_Introduction_Getting_Started_Using_the_Autodesk_Revit_API_Registration_of_add_ins_html.html). Some examples retain older release labels; substitute the actual year and verify loading in that Revit installation.

## Resources to collect for the learning process

These support research and debugging, even though they are not all required to compile the first command:

- **Version-matched Revit SDK and developer guide:** examples, API reference, and supported execution patterns. Start at the [Autodesk Revit API developer center](https://aps.autodesk.com/developer/overview/revit-api) and the [resource register](../../../docs/research/revit-resources.md).
- **RevitLookup for the target year:** inspect the values and parameter identities actually present in the model. The research register links the inspected project and release.
- **Selected source precedents:** clone candidates, inspect their manifests and licenses, and record exact commits. Follow the [Git lesson](../../../docs/tutorials/01-directing-an-agent-with-git.md) and [clone guide](../../../docs/how-to/clone-research-repositories.md). Cloning a repository does not install its dependencies.
- **A sample and independent answers:** a small local project is sufficient for the first read. Use a working copy of an [Autodesk sample](https://help.autodesk.com/cloudhelp/2024/ENU/Revit-GetStarted/files/GUID-61EF2F22-3A1F-4317-B925-1E85F138BE88.htm) and the [demo brief](demo-questions-and-sample-model.md) for the readiness demonstration.

The full SDK download adds useful documentation and examples; it is not a prerequisite installer for the API DLLs already supplied with Revit. RevitLookup, templates, Python, Node.js, pyRevit, Dynamo, and Rhino.Inside are not dependencies of a basic standalone C# command. A local add-in can run without an AI service or API key; the coding agent has its own authentication requirements.

## Additional requirements when we add MCP

MCP enables an agent to request operations from the running tool. Our proposed architecture adds an external server connected to the Revit add-in:

1. An MCP-capable client, which may be the same application used for coding.
2. One chosen server language/runtime and matching MCP SDK: for example C#/.NET, Python, or TypeScript/Node.js. Follow the [official server guide](https://modelcontextprotocol.io/docs/develop/build-server) for the selected SDK release.
3. A local communication bridge and defined request/response format. External requests must be dispatched into a valid Revit API context; [External Events](https://help.autodesk.com/cloudhelp/2024/ENU/Revit-API/files/Revit_API_Developers_Guide/Advanced_Topics/Revit_API_Revit_API_Developers_Guide_Advanced_Topics_External_Events_html.html) provides the relevant Autodesk pattern.
4. Documented client configuration, connection checks, and pinned packages for whichever bridge we select.

The external server can use a different runtime from the add-in. We have not selected the server language or adopted a bridge. Dependencies found in a candidate repository are requirements of that candidate, not universal Revit prerequisites. See the [source inspection findings](../../../docs/research/reference-code-map.md) before choosing.

## Preparation order and proof that it works

1. **Inspect first.** Ask the agent to read the repository guidance, identify installed software and the exact Revit build, and produce a missing-items list.
2. **Gather and record.** Obtain missing build inputs from official sources; record versions, local paths, selected research commits, and dependency terms. Use [Git for Windows](https://git-scm.com/install/windows) to begin or clone the working repository.
3. **Build a minimal probe.** Create one command that reports the active document title and an explicitly scoped element count without changing the model.
4. **Register and run in Revit.** Verify the command loads, runs on a known test model, and agrees with an independent check. Retain the build output and observed result.
5. **Checkpoint the evidence.** Review the diff and commit source, repeatable instructions, and the verification record. Then introduce readiness rules and MCP.

Suggested participant prompt, not a command already executed:

> Read README.md, AGENTS.md, and the Revit project's prerequisites. Inspect my machine for developing a standalone Revit 2024 C# add-in. Report the Revit build, compiler/SDK, .NET Framework 4.8 reference assemblies, Revit API DLLs, Git, and your ability to edit files and run build commands. Separate required, optional, and later MCP dependencies. Research official sources for anything missing. Give me the smallest setup plan and a read-only probe that will prove it works. Do not install software or change Revit during this inspection.

### Evidence on the author's workstation

Initial inspection on 2026-10-20 found Revit and API DLL file version `24.3.60.12`, .NET SDKs `9.0.318` and `10.0.204`, Git `2.53.0.windows.2`, and .NET Framework 4.8 references. In the subsequent implementation step, all required inputs passed inspection, the probe compiled with zero warnings/errors, and it loaded and returned independently checked counts in Revit. No prerequisite software needed installation. The new command was installed per user. See the [verification record](verification/2026-10-20-read-only-probe.md) for scope and limits.

The [first readiness check](verification/2026-10-20-required-information.md) subsequently used the same toolchain with no added NuGet packages. Its 18-test console harness also uses framework libraries only. Python was used solely for an independent comparison of saved native schedule exports and JSON evidence; it remains optional for add-in development and execution.
