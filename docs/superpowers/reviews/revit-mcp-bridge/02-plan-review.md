# R2: implementation-plan review instructions

**Not ready for review.** These are handoff instructions; the implementation plan will be written using superpowers writing-plans after the author approves the reviewed written spec. There is no plan or plan approval to review yet.

Before handoff attach the approved spec/revision, R1 findings and dispositions, actual plan path and full commit hash, relevant baseline code, and proposed dependency/build inputs. Refer to the [review hub](README.md) for the recording procedure.

## Suggested handoff prompt

> Independently review the attached implementation plan against the approved Revit MCP bridge spec and the current source at the supplied revisions. Do not implement. Trace every acceptance criterion A1-A9 to concrete tasks, tests, and retained evidence. Check exact file paths and runtime boundaries, dependency adoption/notices, behavioral red/green tests, queue/framing/context failure cases, host API lifecycle, installation and rollback, three live sample comparisons, and actual client/protocol verification. Flag steps that assume nonexistent APIs, need an unrecorded design choice, depend on unverified host access, or would overwrite research/model evidence. Check that review checkpoints occur before risky work and that a source-only reviewer is not asked to certify a runtime result. Identify tasks that are too large to verify independently or duplicate existing tests without value. Return prioritized findings with evidence, the smallest correction, and a verification method; state what you could not assess.

The plan must make tests and observable results actionable, including expected first failures where new behavior is introduced. It must identify original code versus adopted packages/adapted snippets, preserve manual exports and fixture isolation, and name the execution method for author selection. It must not promise a working seminar client based only on an SDK harness.

Use the [review form](../../../../templates/external-review.md) with stage R2. Record the author's approval of the revised written plan and execution choice separately from the external recommendation. If review changes the architecture or acceptance contract, revise the spec and resolve the affected R1 findings before execution.
