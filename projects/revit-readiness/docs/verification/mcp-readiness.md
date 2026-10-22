# Independent live readiness comparisons

**Consolidated acceptance:** the [A1–A9 ledger](2026-10-22-r3-acceptance.md) supersedes stale pending statements in chronological sections below. The five-case desktop matrix is retained evidence for the recorded installed candidate; overall acceptance remains open. [R3 packet](../../../../docs/superpowers/reviews/revit-mcp-bridge/03-implementation-review.md).

Date: 2026-10-22. All five **tool report/wire** comparisons and both desktop context-switch/recovery tests are verified. Returned R3 identifies missing Codex presentation evidence; A8 and full acceptance remain open. See [R3 feedback/dispositions](../../../../docs/superpowers/reviews/revit-mcp-bridge/2026-10-22-r3-feedback.md).

Baseline: source checkpoint `6f0c1523f1e4d7eac03f46d6ad6573565e96b6b4`; normal Revit host `9a93f6d`, installed external server `d88de67`; Revit 2024 build 24.3.60.12, PID 149972, bridge session `a3163353950948eabd8cfb8a4955d860`. No production, dependency, installation or desktop configuration changes in this increment.

Required desktop baseline: Codex app 26.1016.6573.0 / wire client 0.163.0-alpha.11.1 on `2025-06-18`; Claude Desktop 3.1623.4.0 / wire client local-agent-mode-bim-agent-lab-revit-readiness 1.0.0 on `2025-11-25`. The author reports the five-case interface as Claude **Chat**, AI model **Opus 5.5**, permissions **Auto**. These are reported UI settings, not inferred from protocol traffic; individual prior permission prompts are not reconstructed. Separate official C# SDK 2.2.0 runs retain `2026-07-28`.

The [earlier continuity record](2026-10-22-desktop-readiness.md) proves the clean saved-as model through Codex and Claude and distinguishes Claude's human-confirmed UI from its wire name. It does not replace the independent five-case comparisons below.

| Case | Independent expected outcome | Codex report/wire | Claude Chat report/wire | SDK July report/wire |
|---|---|---|---|---|
| Complete | 16 examined, 0 findings, both complete | Verified | Verified | Verified |
| Wall defects | 2 findings, 6 affected, both complete | Verified | Verified | Verified |
| Ceiling negative | 3 findings, 8 affected, both complete | Verified | Verified | Verified |
| NC1 absent declared identity | 15 examined, scope issue, coverage false/evaluator true | Verified | Verified | Verified |
| NC2 absent ACT requirement | 16 examined, two unexamined ACT instances, both false | Verified | Verified | Verified |
| Old context after document switch | ContextChanged, no report, fresh recovery | Verified | Verified | Separate SDK lifecycle evidence; not a substitute for these desktop tests |

## Preparation and retained unsuccessful setup

Private local run `mcp-task8/f82eea66e10b43ebaafd7fe40ea4bc87` contains five independently prepared expectations and NC1/NC2 control copies. The three principal RVT hashes match the earlier native-schedule evidence. Source/copy hashes and intended paths were recorded before calls. NC1's later independent absent-element confirmation was appended before its first context call, with the prior expectation and immutable preparation receipt retained; details appear below.

Two Codex context calls identified `samples/local/required-information/BAL-info-wall-defects.rvt` instead of the prepared `mcp-live-task7/.../models` copy: request IDs `78c7ff7a103344139d5b2e614d8e0608` and `b26b1ea60bef43d9a112594666ba85ba`. The second had a new document lifetime. Revit journal.0015 independently records both opens of the earlier path, at 09:30:15 and 09:35:33 local time. No check was run with either context. Retain these as setup evidence, not accepted defect results. The person was asked to open the full prepared path through the File name box.

## Wall-defect result verified through both desktops and July SDK

The prepared copy was confirmed after the full-path Open step. The [retained evidence](mcp-readiness-evidence/2026-10-22-wall-defects/evidence-manifest.json) includes both unsuccessful context attempts and all three successful comparisons. Codex used `2025-06-18`, Claude Chat `2025-11-25`, and the separate official SDK client `2026-07-28` against the same installed server. No protocol or package change was made.

| Client | Context request | Check request |
|---|---|---|
| Codex | afcd810e465e45deac8acf1b4edd57f6 | 8c935101e22f48278738d3172ba24a28 |
| Claude Chat | b9156b5c4e53405e9d8d5c5adbfe1b29 | 2680ec5df1c449f08e7b9ebc5ef7b3bf |

Each full report matches the independently frozen expectation: Wall B's required FireRating is blank (2492785–2492787), and Wall D's Code is blank (2492795–2492797). All 16 declared instances were examined across six types/18 field rows, with two findings/six affected, both completeness flags true, no unexamined instances, 1,196 other host instances and six links excluded. Modified-before/after are false. The [author-pasted Claude answer](mcp-readiness-evidence/2026-10-22-wall-defects/claude-wall-defects/answer.txt) correctly says this scope does not pass despite complete coverage.

Desktop records are immutable complete-line prefixes of active sessions, not EOF/shutdown tests. SDK completion and independent comparison have separate markers. The author subsequently supplied a [native BAL INFO Walls schedule screenshot](mcp-readiness-evidence/2026-10-22-wall-defects/native-walls-schedule.png). It visibly confirms blank FireRating for WB-01/02/03 and blank Code for WD-01/02/03. Wall A retains Code/FireRating/AssemblyReference, and Wall B retains Code/AssemblyReference. This independently corroborates the two wall findings across six instances. The image shows case IDs and displayed values, not the file path, element UniqueIds, null-versus-empty-string API representation or ceiling scope; those rely on the separately retained context/checklist/native-export evidence. It was supplied in response to the coordinated inspection request after the prepared-path check. The post-close RVT hash is recorded below.

After the author reported the prepared ceiling model open, the closed wall model was readable and its [SHA-256 comparison](mcp-readiness-evidence/2026-10-22-wall-defects/wall-post-close-hash.json) matched the pre-call value `efd2717188956f7a070480323d85bcffe765007135915c1908a909fe04d50d17`. This supplies the previously pending saved-file check for the wall sample, alongside its false modified-state flags.

## Ceiling-negative model verified through both desktops and July SDK

Codex context `11ef191542d64bceaba00b03a888cb39` confirmed the exact prepared `BAL-info-ceiling-negative.rvt` path and checklist hash before the check. Check `3b547e26f8194f5782318bc3942d52ea` independently matched all expected fields and identities on `2025-06-18`; the separate official-SDK `2026-07-28` run also passed. [Retained captures and hashes](mcp-readiness-evidence/2026-10-22-ceiling-negative/evidence-manifest.json).

All three full reports contain the original wall defects plus missing ACT ceiling Code, affecting ceiling elements 2492802/2492809 (CACT-01/02). Totals are three findings, eight affected instances, all 16 examined, six types/18 field rows, both completeness flags true, no unexamined cases, 1,196 other host instances and six links excluded. Modified-before/after are false. This is a correct report of a failing declared scope, not a readiness pass.

Claude Chat's context `30786f8b02d848048d4449ae049adc70` and check `50d1d1b958d94f12a878fe490e2a45cc` passed the independent full comparison and wire correlation on `2025-11-25`. The [author-pasted answer](mcp-readiness-evidence/2026-10-22-ceiling-negative/claude-ceiling-negative/answer.md) correctly reports all three missing fields/eight exact affected ElementIds, distinguishes complete coverage from a pass, and limits its verdict to the 16 declared instances. The same neutral prompt was used for both principal defect cases.

The author then supplied the [native BAL INFO Ceilings schedule](mcp-readiness-evidence/2026-10-22-ceiling-negative/native-ceilings-schedule.png): CACT-01 and CACT-02 have blank Code cells; CGWB-01 and CGWB-02 show `C-GWB`. Preserve the original PNG bytes. Type names are clipped in this image; case IDs and code values are visible. As with the wall image, document path/full identities and whole-scope completeness come from separate evidence. The [post-close ceiling hash](mcp-readiness-evidence/2026-10-22-complete-and-context-switch/ceiling-post-close-hash.json) matches its original bytes.

## Complete sample and timed context rejection/recovery

The [retained principal and switch evidence](mcp-readiness-evidence/2026-10-22-complete-and-context-switch/evidence-manifest.json) verifies complete-model reports through Codex (`77107660e9ef499697b9de5bfcd08b48` / `8332ecf2448b434e81c7a94fc867993f`), Claude Chat (`f3b92a6c93f34219b8d5a05ab28895c8` / `0d2774e57ebb481fa32cea190eb367ff`) and the July SDK client. All 16 requested instances, six types and 18 field rows match the independent oracle; zero findings, both completeness flags true, no unexamined cases, 1,196 other host instances and six links excluded. Modified flags are false. Claude explicitly limits the pass to this scope. The [principal complete post-close hash](mcp-readiness-evidence/2026-10-22-complete-and-context-switch/complete-post-close-hash.json) also matches the original.

Codex retained a ceiling context expiring at 14:16:11.0629526Z. After the human switched to complete, old-token check `8ea5e39f7113494189dfbb4b4f3250d1` returned `ContextChanged`, observed by 14:15:19.024Z. The fresh complete report above supplies recovery. The independent transition checker verifies original binding, timing before expiry, matching request/token, no report on refusal, then a different path/lifetime/token and a complete independently verified recovery within the same Revit PID/session.

Claude's first attempt returned `ContextExpired` (`313256924ca845b8b4d717ec379f08e3`). It remains a retained **unsuccessful switch test**, with no comparison-complete marker. For the repeat, both models were opened before context acquisition. The intended complete-model acquisition actually returned ceiling; `claude-complete/` retains that rejected setup, while the next transition explicitly used the observed ceiling source. The author activated complete using Switch Windows. Old-token check `0356c77a89f444a9b6cc4cf073c05a3a` returned `ContextChanged`, observed at 14:24:19.114086Z before its 14:26:16.0989833Z expiry. Claude's fresh complete report independently proves recovery. Timing observations accompany the raw streams; arrival times are not invented from JSON-RPC lines.

## Incomplete-coverage controls

The author used Revit's native Select by ID on the principal complete model. The [unchanged screenshot](mcp-readiness-evidence/2026-10-22-nc1/native-invalid-element-id.png) shows rejection of numeric ID `2147483647`; [separate metadata](mcp-readiness-evidence/2026-10-22-nc1/native-absence.json) records the exact message, the subsequent matching context and source/control file hashes. The image does not itself show the model path or full UniqueId. The NC1 expectation was marked confirmed before any NC1 context; the original Prepared receipt and its Ready hash binding are unchanged.

| Case/client | Context request | Check request |
|---|---|---|
| NC1 Codex | 40052cb6e20441bdbadc31731709393c | f4a6209aefb74b64815e1a810ce54d61 |
| NC1 Claude Chat | fde929106fe943b2ab7a1f0e25b0bce4 | ef425953ca4b4dc08a2e9f330a07528d |
| NC2 Codex | 178ae99eb3a247678cce6ef92ca43530 | fb0ab42a093e4736b8daa3a6b92a54df |
| NC2 Claude Chat | 0e3e28a5e4ee468e9452ad01de7b8d2c | d8ce532c26e24b83bb10c6e839ac4b26 |

[NC1 captures](mcp-readiness-evidence/2026-10-22-nc1/evidence-manifest.json) match independently through both desktops and July: 16 requested/15 examined, zero findings/affected instances, exactly one ScopeIssue for `BAL-INFO-WA-01` / `6e5a3689-c170-4e43-b22b-e80a27806cdb-7fffffff`, `Unexamined` / `Element not found`. Wall A has two instances; evaluator `UnexaminedElements` is empty. `CoverageComplete=false` while `Evaluation.Complete=true`; 1,197 other host instances and six links are excluded. [Claude's answer](mcp-readiness-evidence/2026-10-22-nc1/claude-nc1/answer.txt) explicitly rejects a readiness pass and explains the distinction between scope and evaluator completeness. Its proposed cause is labelled unverified; the actual deliberate change is established independently by the prepared checklist. The [NC1 post-close model hash](mcp-readiness-evidence/2026-10-22-nc1/nc1-post-close-hash.json) matches the original.

[NC2 captures](mcp-readiness-evidence/2026-10-22-nc2/evidence-manifest.json) likewise match through all three clients: 16 requested/16 examined, zero findings, no ScopeIssues, exactly CACT-01/02 (2492802/2492809) unexamined. All three ACT fields are `Unexamined` / `NoMatchingRequirement`, required/source null, observed Code `C-ACT` preserved. Both completeness flags are false; 1,196 other host instances and six links excluded. [Claude's answer](mcp-readiness-evidence/2026-10-22-nc2/claude-nc2/answer.txt) correctly explains that found elements cannot be judged without a rule and does not infer missing requirements or declare a pass. Both controls have false modified-before/after flags. After the author closed both remaining model files without saving, [all five model and five checklist hashes](mcp-readiness-evidence/2026-10-22-nc2/all-samples-post-close-hashes.json) matched their pre-test values. Revit remains open with the sample documents closed.

All five cases have verified actual desktop **reports**, plus separate July SDK evidence. Claude answers are retained. R3-01 established that the Codex NC1/NC2 folders did not retain the corresponding public answers, so the earlier blanket interpretation claim was unsupported. The [original NC1 commentary](r3-followup-evidence/codex-nc1-recovered/answer.txt) has now been recovered with request linkage; an adequate independent Codex NC2 interpretation remains missing. Historical Codex UI metadata and current-source desktop acceptance also remain open. See the [presentation completeness gate](r3-followup-evidence/README.md#presentation-completeness-gate). Full Task 8/A8 closure still requires that evidence and independent follow-up; remaining Task 7 observations and overall acceptance are separate. At the original sample-test checkpoint these SDK captures used the official-SDK driver directly; the wrapper was subsequently implemented and exercised, as recorded in the [runner follow-up](2026-10-22-readiness-runner.md) and [post-reinstall read](2026-10-22-shared-recovery-execution.md#fresh-revit-recovery-passes).

## Verification work

New independent comparator tests pass four methods with five positive cases and 147 negative controls. Wire/binding tests pass four methods with 20 negative controls, checking request/response correlation, protocol, catalog, exact saved-result agreement and context/report semantic agreement. Control preparation passes seven methods covering interruptions before completion, interrupted cleanup, foreign modifications and ownership checks. All three retained wall captures pass the strengthened checker. `Test-McpBridge.ps1 -Suite Protocol` passes, including compatible exchanges for all three revisions and strict July/legacy-rejection regressions; its full log is retained with the evidence. Deployment/All was not run while Revit was open.

The comparator initially assumed missing raw values were null. Existing fixture source showed deliberate clearing uses an empty string; untouched optional empty values remain null. The expectation was corrected from that source, keeping this distinction explicit. Native displayed schedules establish visible content, not this API representation distinction.

Live `ContextChanged` results exposed a second verification assumption: the synthetic error fixture incorrectly included the success-envelope `contractVersion`. Production `ReadinessTools.Failure` emits exactly code/message/requestId/retryable. The positive fixture was corrected first and failed; the checker then adopted the actual separate error contract. Three stale-checker methods/11 negative controls and both real switch/recovery captures pass. The oracle, wire, preparation and desktop-continuity checker suites also pass. This changes evidence validation only; installed production code and package pins are unchanged.

Participant story: [how the author directed the tests](../../../../docs/tutorials/14-test-readiness-through-desktop-agents.md). Agent procedure: [exact workflow and limits](../../../../docs/how-to/verify-readiness-through-desktop-agents.md). Remaining Task 7 shutdown/disposal observations, unresolved prior observations and full R3 review remain open.

The final [consolidated replay](mcp-readiness-evidence/2026-10-22-nc2/five-cases-verification.txt) passes all 15 independent report comparisons (five cases across three clients), both desktop transition/recovery checks and all five focused Python checker suites. Exact retained manifest inventories and file hashes were checked. The [fresh Protocol log](mcp-readiness-evidence/2026-10-22-complete-and-context-switch/protocol-regression.txt) retains compatible three-revision exchanges and strict July/legacy-rejection checks. No Deployment/All run occurred while Revit was open.


## Subsequent runner increment

The planned `Invoke-ReadinessMcp.ps1` wrapper is now [implemented and verified offline](2026-10-22-readiness-runner.md), superseding the earlier missing-wrapper note. The actual unavailable-host control passes with Revit closed; no new successful live wrapper run is claimed. Previous sample and desktop evidence is unchanged. The author requested one increment at a time, so broader workflow refinement, remaining Task7 observations and R3 have not been started in this increment.
