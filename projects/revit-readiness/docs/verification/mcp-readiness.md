# Independent live readiness comparisons

Date: 2026-10-22. Task 8 is **in progress**; five-case A8 and R3 are not closed.

Baseline: source checkpoint `6f0c1523f1e4d7eac03f46d6ad6573565e96b6b4`; normal Revit host `9a93f6d`, installed external server `d88de67`; Revit 2024 build 24.3.60.12, PID 149972, bridge session `a3163353950948eabd8cfb8a4955d860`. No production, dependency, installation or desktop configuration changes in this increment.

The [earlier continuity record](2026-10-22-desktop-readiness.md) proves the clean saved-as model through Codex and Claude and distinguishes Claude's human-confirmed UI from its wire name. It does not replace the independent five-case comparisons below.

| Case | Independent expected outcome | Codex | Claude Chat | SDK July |
|---|---|---|---|---|
| Complete | 16 examined, 0 findings, both complete | Pending | Pending | Pending |
| Wall defects | 2 findings, 6 affected, both complete | Verified | Verified | Verified |
| Ceiling negative | 3 findings, 8 affected, both complete | Pending | Pending | Pending |
| NC1 absent declared identity | 15 examined, scope issue, coverage false/evaluator true | Pending | Pending | Pending |
| NC2 absent ACT requirement | 16 examined, two unexamined ACT instances, both false | Pending | Pending | Pending |
| Old context after document switch | ContextChanged, no report, fresh recovery | Pending | Pending | Separate SDK lifecycle evidence exists; this coordinated run pending |

## Preparation and retained unsuccessful setup

Private local run `mcp-task8/f82eea66e10b43ebaafd7fe40ea4bc87` contains five frozen expectations and prepared NC1/NC2 control copies. The three principal RVT hashes match the earlier native-schedule evidence. Source/copy hashes and intended paths were recorded before calls. NC1's independent absent-element confirmation remains pending.

Two Codex context calls identified `samples/local/required-information/BAL-info-wall-defects.rvt` instead of the prepared `mcp-live-task7/.../models` copy: request IDs `78c7ff7a103344139d5b2e614d8e0608` and `b26b1ea60bef43d9a112594666ba85ba`. The second had a new document lifetime. Revit journal.0015 independently records both opens of the earlier path, at 09:30:15 and 09:35:33 local time. No check was run with either context. Retain these as setup evidence, not accepted defect results. The person was asked to open the full prepared path through the File name box.

## Wall-defect result verified through both desktops and July SDK

The prepared copy was confirmed after the full-path Open step. The [retained evidence](mcp-readiness-evidence/2026-10-22-wall-defects/evidence-manifest.json) includes both unsuccessful context attempts and all three successful comparisons. Codex used `2025-06-18`, Claude Chat `2025-11-25`, and the separate official SDK client `2026-07-28` against the same installed server. No protocol or package change was made.

| Client | Context request | Check request |
|---|---|---|
| Codex | afcd810e465e45deac8acf1b4edd57f6 | 8c935101e22f48278738d3172ba24a28 |
| Claude Chat | b9156b5c4e53405e9d8d5c5adbfe1b29 | 2680ec5df1c449f08e7b9ebc5ef7b3bf |

Each full report matches the independently frozen expectation: Wall B's required FireRating is blank (2492785–2492787), and Wall D's Code is blank (2492795–2492797). All 16 declared instances were examined across six types/18 field rows, with two findings/six affected, both completeness flags true, no unexamined instances, 1,196 other host instances and six links excluded. Modified-before/after are false. The [author-pasted Claude answer](mcp-readiness-evidence/2026-10-22-wall-defects/claude-wall-defects/answer.txt) correctly says this scope does not pass despite complete coverage.

Desktop records are immutable complete-line prefixes of active sessions, not EOF/shutdown tests. SDK completion and independent comparison have separate markers. A fresh native schedule inspection and model hash after close were requested; those observations remain pending at this checkpoint.

## Verification work

New independent comparator tests pass four methods with five positive cases and 147 negative controls. Wire/binding tests pass four methods with 20 negative controls, checking request/response correlation, protocol, catalog, exact saved-result agreement and context/report semantic agreement. Control preparation passes seven methods covering interruptions before completion, interrupted cleanup, foreign modifications and ownership checks. All three retained wall captures pass the strengthened checker. `Test-McpBridge.ps1 -Suite Protocol` passes, including compatible exchanges for all three revisions and strict July/legacy-rejection regressions; its full log is retained with the evidence. Deployment/All was not run while Revit was open.

The comparator initially assumed missing raw values were null. Existing fixture source showed deliberate clearing uses an empty string; untouched optional empty values remain null. The expectation was corrected from that source, keeping this distinction explicit. Native displayed schedules establish visible content, not this API representation distinction.

Participant story: [how the author directed the tests](../../../../docs/tutorials/14-test-readiness-through-desktop-agents.md). Agent procedure: [exact workflow and limits](../../../../docs/how-to/verify-readiness-through-desktop-agents.md). Remaining Task 7 shutdown/disposal observations, unresolved prior observations and full R3 review remain open.
