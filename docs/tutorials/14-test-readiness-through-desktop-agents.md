# Test the answer, not just the connection

The participant directs the work by naming the situations the tool must handle and asking for evidence. A successful connection and one clean model are useful milestones; they do not show whether the agent recognizes defects, admits incomplete coverage, or refuses an answer based on an old model context.

## How we engaged the agent

After seeing the clean-model result in both applications, the author asked:

> “Ok, let's test the deliberate defects, incomplete coverage, and stale contexts in both desktop apps. Tell me what to do in Revit and keep recording the process in both teaching layers.”

The person operates Revit and Claude Desktop's Chat interface. The agent prepares comparisons, calls the bridge from Codex, checks the recorded exchanges, and records the results. This division avoids making Windows UI automation a prerequisite for the seminar.

We prepared the expected answers before running the new checks. The original native Revit schedule exports, checklist identities and authored answer sheet establish what should be found. Comparing two agent answers alone would not establish that either answer is correct.

The cases ask different questions:

| Situation | What the participant should look for |
|---|---|
| Complete declared scope | All requested instances examined, no missing required values, both completeness flags true |
| Deliberate wall and ceiling defects | The expected missing fields, correct types and affected instance identities |
| A declared element cannot be found | An explicit scope issue and incomplete coverage, even with zero missing-value findings |
| A used type lacks a checklist requirement | Unexamined instances and fields; the agent must not infer requirements or announce a pass |
| The active model changes after context acquisition | A structured refusal of the old context, followed by a successful fresh-context check |

## A useful surprise: identical names can conceal different files

At the start of the wall-defect test, the author opened a model with the expected filename. The bridge returned the path of an earlier sample folder. A second open produced a new document lifetime but still the earlier path. The agent checked Revit's own journal before asking the person to repeat the action: the journal confirmed both opens used the earlier folder. No defect check was counted from either context.

The lesson is to ask the agent to verify the actual file path and checklist, rather than trust the title bar or a recent-file tile. We retained those unsuccessful setup attempts and asked the person to paste the complete prepared path into Revit's Open dialog. The next context confirmed the prepared copy. This is evidence about setup, not a defect in the readiness rule.

The wall-defect result then matched the independent expectation in Codex, Claude Chat and the separate July-protocol SDK check. Both desktops identified the two missing fields affecting six walls. Claude correctly explained that complete coverage means the requested examination finished; it does not mean the model passed. The incomplete-coverage cases require their own evidence.

The author then supplied Revit's native wall schedule. The person could see the three blank Wall B fire ratings and three blank Wall D codes directly. This gives participants a practical way to challenge an agent's answer: inspect the underlying application. Keep the limits clear: a wall schedule corroborates those wall values; it cannot by itself prove coverage of ceilings, the file path or the entire model.

After the author opened the ceiling-defect sample, Codex and the separate July-protocol check found the additional missing ceiling code: three findings affecting eight instances. The agent requested the same neutral prompt in Claude, without supplying expected results in that prompt. The author pasted Claude's answer, and the agent independently compared the actual tool exchange as well: all three results matched. Claude explicitly said the declared scope did not pass despite complete coverage. The closed wall file's hash matched its original value, giving a separate check that the saved model had not changed.

For the old-context test, the person switched models after each agent acquired a context. Codex refused the old context before it expired and recovered using a fresh context. Claude's first attempt returned `ContextExpired`: safe behavior, but insufficient evidence that the model switch caused the refusal. We kept that attempt and repeated the test instead of counting any refusal as success.

The author also supplied the native ceiling schedule, visibly confirming two blank ACT codes alongside the two populated GWB controls. The agent then supplied a narrowly scoped prompt asking Claude to acquire a fresh context and stop. Separating acquisition from checking lets the person deliberately change the model between the two operations, without asking the agent to edit anything.

For Claude's repeat, the person opened both models before starting the timer. The next context revealed that the ceiling model was still active, despite the intended complete-model selection. Opening a file or selecting a different view in the project browser is not sufficient evidence of the active document. The agent retained the mismatch, used the observed ceiling context, and gave the person a precise Switch Windows instruction. Claude then returned `ContextChanged` before expiry. A fresh check on the complete model passed all 16 declared instances. All three principal samples now match independent expectations through both desktops and the separate July SDK client.

## Give people usable instructions too

Before the first incomplete-coverage case, the agent asked the person to confirm that a deliberately absent element ID could not be found in Revit. The author replied, “I'm not a Revit expert...how do I execute that command in Revit?” The agent had supplied a command name without the steps needed to use it.

The corrected instruction gave the ribbon path, exact ID, button and expected reporting action: Manage → Inquiry → Select by ID, enter `2147483647`, click Show, then report the exact message. The author supplied a screenshot showing Revit rejected that ID and left the model open. The agent retained the image and verified that the prepared control model had the same file bytes. This confirms the independent setup observation; it does not yet prove the bridge handles incomplete coverage correctly.

For participants, the useful direction is: ask the agent to make each human step executable at your experience level. The agent should verify the resulting application state and explain what the observation proves, rather than make the person diagnose the test.

## What the incomplete cases taught us

The person opened each prepared copy while the agent verified the actual path. Both desktops then returned zero missing-field findings in both cases, but correctly refused to call either scope ready. In the first case, one declared instance could not be found: 15 of 16 were examined. In the second, all 16 were found, but the two ACT ceiling instances lacked a checklist rule. The agent compared every returned identity, value and completeness flag against the prepared expectations, and separately checked how Claude explained the answer.

Claude's explanation made the practical distinction clear: a clean result for the elements it could evaluate does not cover an element it could not find or a type it could not judge. The agent used the same neutral prompt for both cases, without feeding Claude the expected defects. Suggested remedies in Claude's answers remained suggestions; no model or checklist was repaired during the tests.

The author closed both remaining models without saving, and the agent verified that all five model files and their checklists retained their original hashes. The author also identified Claude Chat's AI model as **Opus 5.5** and its permission setting as **Auto**. That detail required clarification: “which model?” can mean the Revit project or the AI model. Name the application and the setting when asking a participant to report it.

## Suggested prompts for your own project

Before moving on to the remaining reliability checks and review, the author said, “ok, let's take these one at a time, then we can refine things.” The agent limited the next increment to a reusable test runner and its teaching notes. The purpose was to make the process repeatable without reopening the whole design or asking the person to repeat all the manual sample steps.

The runner preserves the expected answer before asking the SDK client to read. It checks that the active model really is the prepared sample, keeps unsuccessful attempts, and marks success only when the independent comparison finishes. “Success” here means the tool behaved as expected: correctly reporting a defect or incomplete coverage is a successful test, even though the model does not pass readiness.

The agent used subprocess fixtures for offline tests and clearly separated them from live Revit proof. Trying the actual command with Revit closed also exposed a PowerShell default-path mistake that explicit-path tests had missed. A new test reproduced it before the correction. This illustrates a useful participant request: “Test the command exactly as you expect me to run it, and tell me what still requires the real application.” The wrapper is one step toward a repeatable handoff; broader onboarding and remaining host acceptance are later increments.

These are teaching suggestions, not additional quotations from the author:

- “Prepare the expected answers independently before calling the tool. Show where each expectation comes from.”
- “Give me one Revit action at a time. Verify the active path before reading the model.”
- “Test zero findings with incomplete coverage. Show how the agent distinguishes that from a pass.”
- “Hold the old context, change models, and prove that the old context is rejected.”
- “Keep failed and incomplete attempts. Tell me which acceptance conditions remain untested.”

The [verification record](../../projects/revit-readiness/docs/verification/mcp-readiness.md) states what has actually run. The [agent procedure](../how-to/verify-readiness-through-desktop-agents.md) supplies the lower-level steps. This lesson is in progress; the initial connection results do not close the full desktop acceptance test.

## October 28 laptop rehearsal: zero findings still needs complete evidence

Before the summit, the author's actual request was: “Ok, let's rehearse the incomplete-coverage cases next in Revit 2026.5 and make sure Codex and Claude don't report a pass when elements or requirements haven't been checked.” The agent prepared two isolated copies and independently recorded their expected answers before asking either client to check them. It handled the native Revit inquiry in this rehearsal, while the person ran the neutral questions in the existing Claude chat. This automation was the presenter's workflow, not an added attendee prerequisite.

In NC1 the checklist points WA-01 at a deliberately invalid identity. Revit's native Select by ID rejected that number first. Both agents then reported zero findings among 15 examined cases, but refused a readiness pass because 16 were requested. The six returned types could pass while the declared scope remained incomplete. The agent checked the recorded tool results and Claude's explanation separately; it also verified that the control changed a checklist reference, not the original wall.

In NC2 all 16 elements were found, but the checklist supplied no ACT requirement. Both agents refused a pass: ACT's two instances and all three type fields remained unexamined. The schedule still shows C-ACT in the model. A populated value cannot tell us that it meets a rule when no rule exists.

Claude's initial NC2 explanation had the correct outcome but used “missing requirement” for a missing required value. The agent asked for a wording correction without another model query. The revised section separates a known rule finding missing data, an explicit exemption, and a missing checklist rule that prevents evaluation. This makes the seminar lesson concrete: **missing data and missing instructions require different remedies; neither an exemption nor an unexamined field should be invented from null.** The agent independently compared both desktop reports with the prepared expectations and separate SDK runs, and confirmed unchanged saved model bytes. See the [current evidence](../../projects/revit-readiness/docs/verification/2026-10-27-revit-2026-5.md#incomplete-coverage-rehearsals). This finishes the two requested content rehearsals; the remaining reliability and release checks stay separate.
