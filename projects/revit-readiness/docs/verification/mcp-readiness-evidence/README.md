# Task 8 evidence

[Current verification matrix](../mcp-readiness.md).

`2026-10-22-wall-defects/` retains Codex desktop, Claude Desktop Chat and official-SDK July exchanges for the prepared wall-defect model. Each has the frozen expectation, full context/check result, raw protocol streams, capture hashes and comparison completion record. `setup-attempts/` retains the two contexts from the wrong folder; neither was used for a check. The manifest lists exact retained file bytes and hashes. No RVT, dependency binaries or private configuration backups are committed.

Run from the repository root, replacing the folder with each of `codex-wall-defects-02`, `claude-wall-defects`, and `sdk-july-wall-defects`:

```powershell
python projects/revit-readiness/tests/verify_mcp_evidence.py projects/revit-readiness/docs/verification/mcp-readiness-evidence/2026-10-22-wall-defects/codex-wall-defects-02 --case wall-defects
```

An `attempt.json` or SDK `partial.json` records the start and is intentionally retained after completion. Only `comparison-complete.json` indicates that the independent BIM comparison also passed; the SDK's `complete.json` alone means its exchange assertions passed. Desktop streams are captured prefixes of ongoing sessions. A prefix can contain earlier calls; `capture.json` identifies the exact pair being verified. These records do not prove EOF shutdown.

`2026-10-22-ceiling-negative/` contains independently verified Codex, Claude Chat and July SDK captures. Run the same verifier with `--case ceiling-negative` for its `codex-ceiling-negative`, `claude-ceiling-negative` and `sdk-july-ceiling-negative` directories. Claude's author-pasted answer is retained separately from its actual wire/result evidence. The wall evidence includes the author's native schedule screenshot and the unchanged RVT hash after close.

`2026-10-22-complete-and-context-switch/` retains independently verified `codex-complete`, `claude-complete-02` and `sdk-july-complete` reports. Run the ordinary verifier with `--case complete`. Its `codex-stale-ceiling-to-complete` and `claude-stale-ceiling-to-complete-02` directories pass `verify_stale(directory)`, including before-expiry refusal and complete fresh recovery. The first Claude `ContextExpired` attempt and the wrong-active-model complete attempt are retained without successful comparison markers. The original error-fixture assumption and its correction are described in the verification record.

`2026-10-22-nc1/` retains native ID rejection evidence, its prior and confirmed expectations, immutable preparation/Ready receipts, and all three independently verified NC1 captures. Run the ordinary verifier with `--case nc1`. Its image establishes the displayed ID rejection, with the model binding and source/control byte equality recorded separately.

`2026-10-22-nc2/` retains the three NC2 captures (`--case nc2`), the author-pasted Claude answer and final comparisons of all five RVT files and five checklists after the user closed both remaining models without saving. No runtime, configuration or dependency change was needed. Native ceiling evidence is in the earlier ceiling folder.

The SDK July captures supplement both required desktops. Reusable workflow consolidation, outstanding Task 7 observations and full R3 remain separate requirements; successful sample tests do not close them.
