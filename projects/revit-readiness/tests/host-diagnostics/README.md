# Diagnostic host controls (Task 7)

This separately built add-in links the production Revit source and defines `BIM_BRIDGE_DIAGNOSTICS`. The normal project does not define that symbol or compile the diagnostic helper. No dependency is added. Install only through the existing prepared-recovery installer, with Revit closed; retain a receipt that restores the normal candidate.

## Execution and waiting model

The approved host checklist permits diagnostic instrumentation. A timeout response alone does not establish whether a reader later starts. These controls observe the existing production flow without replacing its queue, context validation, budget or reader.

| Boundary | Execution owner and observation | Allowed waiting |
|---|---|---|
| Admission | Endpoint wrapper records original request ID and attaches observers to the original Response/Quiesced tasks | No wait for observers; no registrations on the private transport signal |
| Preparation | API executor captures and validates context, then optionally holds before returning from preparation | At most 45 seconds, or explicit release; the original request deadline keeps running |
| Final read guard | Unchanged `ReadinessExecutionFlow` checks the same context after preparation | No diagnostic replacement of this guard |
| Started reader | Actual reader records entry, initializes its report using Autodesk data, then optionally holds | Same bounded hold; already-started reads may finish, but the original post-read/publication guards still apply |
| Cancellation | Recorder polls authoritative invalidation/expiry, without running borrowed callbacks | Observation only; it does not cancel, reset or extend the request |
| Stop during a callback | An explicitly armed diagnostic control invokes the existing Stop in valid API context, then holds the callback | Network progress must be independent of the held callback; this is injected Stop, not a human click during blocked UI |
| Native events | Observe Raise begin/result, API disposal, hook/unhook, application shutdown | No waits added to native event ownership |
| Retirement | Observe original Response, Quiesced and network-stop tasks separately | Observer work is not part of those tasks' completion; physical lifetime disposal is not inferred from Quiesced |

Control files are local test inputs, never MCP arguments. Each stage arm is consumed once; release files are bound to that request and stage. A stale release file makes the control invalid rather than immediately passing a hold. Missing release automatically unblocks by the diagnostic limit. Invalid controls are recorded and do not hold execution. The recorder has a bounded event count; missing/truncated/error evidence cannot pass acceptance. No model write, transaction or export is added.

First demonstrate the controls offline, including cancellation during a held preparation and actual reader stage, unchanged deadline, stale release rejection, bounded automatic release and asynchronous endpoint observations. Then identify diagnostic source/binary hashes and capture live tests. A diagnostic pass is not a normal-build desktop A8 pass. Restore the normal build before final sample/client acceptance. Retain failed attempts and list native outcomes that could not be observed.
