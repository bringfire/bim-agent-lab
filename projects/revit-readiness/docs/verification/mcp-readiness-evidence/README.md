# Task 8 evidence

[Current verification matrix](../mcp-readiness.md).

`2026-10-22-wall-defects/` retains Codex desktop, Claude Desktop Chat and official-SDK July exchanges for the prepared wall-defect model. Each has the frozen expectation, full context/check result, raw protocol streams, capture hashes and comparison completion record. `setup-attempts/` retains the two contexts from the wrong folder; neither was used for a check. The manifest lists exact retained file bytes and hashes. No RVT, dependency binaries or private configuration backups are committed.

Run from the repository root, replacing the folder with each of `codex-wall-defects-02`, `claude-wall-defects`, and `sdk-july-wall-defects`:

```powershell
python projects/revit-readiness/tests/verify_mcp_evidence.py projects/revit-readiness/docs/verification/mcp-readiness-evidence/2026-10-22-wall-defects/codex-wall-defects-02 --case wall-defects
```

An `attempt.json` or SDK `partial.json` records the start and is intentionally retained after completion. Only `comparison-complete.json` indicates that the independent BIM comparison also passed; the SDK's `complete.json` alone means its exchange assertions passed. Desktop streams are captured prefixes of ongoing sessions. A prefix can contain earlier calls; `capture.json` identifies the exact pair being verified. These records do not prove EOF shutdown.

`2026-10-22-ceiling-negative/` currently contains independently verified Codex and July SDK captures; Claude remains pending. Run the same verifier with `--case ceiling-negative` for its `codex-ceiling-negative` and `sdk-july-ceiling-negative` directories. The wall evidence now includes the author's native schedule screenshot and the unchanged RVT hash after close.

The complete principal case, remaining Claude ceiling check, incomplete controls, stale-context workflow, remaining post-close hashes, outstanding Task 7 observations and R3 remain separate acceptance requirements.
