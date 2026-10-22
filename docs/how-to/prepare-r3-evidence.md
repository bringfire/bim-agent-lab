# Consolidate evidence for an independent implementation review

Companion to the [participant story](../tutorials/15-prepare-an-evidence-based-review.md). The worked [R3 packet](../superpowers/reviews/revit-mcp-bridge/03-implementation-review.md) and [A1–A9 ledger](../../projects/revit-readiness/docs/verification/2026-10-22-r3-acceptance.md) describe the actual October 22 handoff. This procedure prepares a review; it does not authorize external messaging or close acceptance.

## 1. Freeze identities before interpreting results

Record a clean Git state, full implementation base/head, normative spec/plan revision and amendments. Record installed host/server and diagnostic source separately. Inventory relevant tracked files with Git blob IDs and actual byte hashes. Compare the current source against each installed pin; classify differences by the behavior they can affect. Conditional diagnostic code is not enough to infer binary identity.

The worked handoff reviews implementation `627e227c8054feaf91539976b32dd09228c4e93f` against baseline `ff8a4f359fd0a9ff9147cab366a9cb81683a98cc`. Its packet is a later documentation/evidence-only commit. Include both identities in the reviewer request. For a new release built from changed source, follow plan Task 9: install through the recoverable procedure and rerun affected live cases after coordinating application availability. Do not replace installed files merely to make the evidence ledger look complete.

## 2. Build the requirement-to-evidence map

For each A1–A9 row, identify the exact assertions, commands, source/binary identity, client/protocol/configuration, input model/checklist, independent expected answer and retained output. State whether evidence is offline, actual Windows IPC, diagnostic Revit, normal Revit, desktop application, or independent review. Keep known failed behavior separate from unperformed tests and successful negative controls.

Check producer/consumer boundaries: deadlines must survive admission, token validity must survive preparation, rejected arguments must not be decoded again, transport cleanup must not await borrowed callbacks, and recovery bytes must exist before mutation. Trace safety and progress separately. Carry the plan's five review operations into the packet.

For desktop claims preserve exact UI mode, selected AI model and relevant permission settings, not just wire `clientInfo`. Missing historical metadata stays missing; do not infer it from today's settings. An SDK or CLI run cannot stand in for the required desktop.

## 3. Verify without rewriting history

With Revit closed, run current `Test-McpBridge.ps1 -Suite All`, `Test-Readiness.ps1` and locked `Build-McpBridge.ps1` once, sequentially. Inspect exit codes, warning/failure output and totals. Preserve failures before any justified retry. Builds do not install. If Revit is open, coordinate the required shutdown instead of terminating it.

Run the [read-only retained-evidence replay](../../projects/revit-readiness/docs/verification/r3-evidence/replay.py). It verifies 21 explicitly listed original manifests, all 15 case/client reports, two unexpired switch/recovery tests, initial desktop continuity, the post-reinstall wrapper and host/SDK diagnostic captures. Keep the frozen expectations and original captures unchanged. Hash equality proves byte consistency, not correctness; semantic assertions and independent oracle sources remain necessary.

Audit dependency locks and runtime boundaries, upstream license/notice pins and actual code reuse. Refresh advisory observations without changing package pins. Distinguish TypeScript 1.26 precedent research from the actual C# 2.2 stdin-helper adaptation and reused lab evaluator/reader. SDK package version is not a dated protocol revision. Do not label a version “latest” from an old research snapshot.

## 4. Assemble a reviewable packet

Use the ledger for claim detail and the packet for the read set, exact diff commands and reviewer questions. Include useful failed attempts and explain their evidentiary limits. Name remaining live-build, native-event, cancellation and reproduction-metadata gaps; do not silently amend requirements to close them. Keep current status pages concise and route chronological checkpoints through the ledger so old “pending” text is not mistaken for today's state.

Record the actual human instruction in the participant lesson, the actions/decisions it caused, and a separately labelled reusable prompt. Put commands, source pins, evidence verification and ownership details here or in linked technical records. Record the step in the journal and update both entry points.

Check new/changed Markdown links and anchors, source/evidence manifests, staged diff and `git diff --check`. Keep credentials, unrelated configs, proprietary binaries, RVT models and private backups out of Git. Commit the concrete packet and give the author its actual full SHA. Preparing it does not transmit it to a reviewer.

## 5. Keep review and acceptance separate

The external reviewer should state their own inspected inputs and checks. Use the [review template](../../templates/external-review.md); retain original findings separately from author dispositions. Several major findings can signal a shared missing reasoning model: ask the reviewer to investigate that possibility, verify it against source, and apply the supported insight across the relevant paths.

Only actual returned review can establish independent R3 status. Only appropriate verification can close an implementation claim. Only the author's decision can accept a documented limitation. The original packet was handed over with open items. R3 has now returned; the response below records what remains, without claiming a completed release.


## Respond to R3-01: check the last consumer

Read the [original review and separate dispositions](../superpowers/reviews/revit-mcp-bridge/2026-10-22-r3-feedback.md). Do not confuse report replay with presentation acceptance. The [mandatory presentation gate](../../projects/revit-readiness/docs/verification/r3-followup-evidence/README.md#presentation-completeness-gate) requires prompt, actual public answer, case/request/candidate linkage, run-specific UI baseline and semantic assessment for each required desktop case. Its current result is NOT MET.

Recover historical messages only from original public records, retaining message/call IDs, UTC, phase and a traceable source/hash. Exclude private reasoning and unrelated settings. Compare the original tool result to the retained report, then assess the answer separately. A fresh answer written today or a broad completion claim cannot stand in for the original consumer response. Keep append-only recovery artifacts separate from the original case directories/manifests.

NC1 now has its original commentary answer with request linkage. NC2's inspected history supplies a summary of Claude and a general completion claim, neither sufficient to close its missing Codex interpretation. Do not reconstruct past model/permission settings from current configuration. Full A8 rerun on the selected normal candidate should record all required prompts/answers and baseline, and address O1/O5 together. Preserve the reviewer's scoped passes and known SDK defect; no new production cancellation defect was found.
