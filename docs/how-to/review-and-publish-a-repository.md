# Review and publish a teaching repository

Participant companion: [how the author directed publication](../tutorials/17-review-before-publication.md). Recorded application: [October 26 review](../reference/public-distribution-review-2026-10-26.md), starting at `9a4ac9243b4570f1eab808036c3a0c90e9a3ba8a`.

## Establish the scope before mutation

Read root/project guidance. Inspect status, current branch, remote URL, remote default branch and every remote head/tag/PR ref. Fetch normally and record exact commits. Do not assume `.gitignore` removes material already committed or that deleting a current file removes its history. Do not force-push or rewrite history as a routine publication step.

The author explicitly authorized making this repository public once ready. Preserve that authorization and carry out the review before changing visibility. A real credential requires revocation/rotation and appropriate history remediation before publication; a genuine unresolved rights or confidentiality question requires the owner's decision. Do not introduce another approval gate when the authorized conditions have been satisfied.

Inspect GitHub issues/PRs, releases/assets, Actions runs/artifacts, Pages, discussions and any separate wiki. Repository visibility can expose more than the working tree. Read the [GitHub visibility guidance](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/managing-repository-settings/setting-repository-visibility) for the actual destination.

## Inventory and inspect the content

Use a fresh ignored directory for raw reports and decoded material. They may themselves contain sensitive content. The [inventory helper](../reference/public-distribution/inspect_history.py) requires an ignored output path and explicit refs. It reads Git objects, decodes text/PDF/Office XML/gzip evidence, extracts images for human review and records unsupported objects or extraction errors. It requires Python, Pillow and pypdf; it is review tooling, not a Revit dependency.

Example from the repository root, with destination and refs adapted to the actual checkout:

```powershell
python docs/reference/public-distribution/inspect_history.py --repo . --output references/publication-audit --ref HEAD --ref origin/main --ref origin/feat/revit-mcp-bridge
```

Review all extraction errors and unsupported data explicitly. Scan the decoded output as well as history. The helper's small pattern set is supplementary, not a replacement for a dedicated credential scanner. Inspect document metadata, comments/revisions, hyperlinks, embedded objects, screenshot pixels and compressed logs. Distinguish public author/workstation metadata from private customer/project information. Confirm that original BIM files, Autodesk binaries, private configuration backups and research checkouts are excluded.

For the recorded review, Gitleaks **8.30.1** was downloaded from its [official release](https://github.com/gitleaks/gitleaks/releases/tag/v8.30.1); the archive digest was verified before execution. The exact default configuration and digests are in the sanitized evidence. No custom suppressions were used. Future reviews must select and record their own current tool baseline.

```powershell
gitleaks git . --log-opts="--full-history HEAD origin/main origin/feat/revit-mcp-bridge" --config <pinned-default-config> --gitleaks-ignore-path <ignored-audit-directory> --ignore-gitleaks-allow --redact=100 --report-format json --report-path <ignored-audit-directory>/history.json
gitleaks dir <ignored-audit-directory>/extracted --config <pinned-default-config> --gitleaks-ignore-path <ignored-audit-directory> --ignore-gitleaks-allow --redact=100 --report-format json --report-path <ignored-audit-directory>/decoded.json
```

Replace angle-bracket placeholders; they are not runnable PowerShell syntax. Exit 1 means findings in this tool, not a clean pass. Review each class and retain counts/dispositions. Do not sum overlapping history and decoded scans as distinct secrets. Do not broadly suppress a word such as `token`: a future account credential could resemble a harmless historical context.

## Check rights and claims

Verify the owner's license, copied/adapted source attribution, locked package notices and embedded assets. Compare retained notice hashes against their manifests and pinned upstream content. Identify encoding differences precisely: equal normalized text is not byte equality. Preserve upstream copyright statements. Our root MIT grant does not relicense third-party materials or separately acquired Autodesk products/models.

Read prominent support, safety and completion claims against actual evidence. Label archived aspirations explicitly, and preserve historical source bytes when they are part of an evidence chain. Keep the current README, handoff and sharing status consistent. A public teaching preview may honestly include known implementation/acceptance limitations; do not call it a verified release or silently waive requirements.

## Publish, then prove access

Commit the reviewed changes and push the intended public refs without rewriting unrelated branches. Inspect the resulting heads and recheck new content. In this repo, remote `main` and `feat/revit-mcp-bridge` share the publication checkpoint; the older local `main` is intentionally preserved.

After the authorized readiness conditions are met:

```powershell
gh repo edit bringfire/bim-agent-lab --visibility public --accept-visibility-change-consequences
```

Verify `private: false` using an unauthenticated HTTP client, fetch README and LICENSE without cookies or authorization headers, and clone over HTTPS into a fresh ignored folder with Git credential helpers/global URL rewrites disabled. Use `git clone -c core.longpaths=true ...` on Windows: the recorded first checkout failed in a deeply nested folder, while the fresh long-path checkout passed. Compare that clone's HEAD and Git object hashes with the published commit. Account explicitly for native line-ending conversion when comparing working-copy text; the first LICENSE working-copy byte assertion was too strict. An authenticated `gh repo view` alone does not establish participant access.

Record actual time, published commit and anonymous results; update current visibility statements and push that documentation. Confirm the final remote head again. Preserve raw findings privately and publish only a reviewed summary. No release asset, invitation, announcement or application installation is implied by changing repository visibility.

### Verify the repository landing page, not only the raw file

The author caught a missing publication check: the root README was correct and publicly accessible, but an old `.github/README.md` appeared on the repository landing page. GitHub's [documented selection order](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-readmes) is `.github/`, root, then `docs/`. The correction renamed the internal note to `.github/hosting-and-automation.md`; the participant README remains the single source of landing content.

Before claiming the published introduction is correct, inspect the unauthenticated `GET /repos/bringfire/bim-agent-lab/readme` response and require `path: README.md`. Decode its content and compare it with the intended root Git object. Also fetch the actual repository page and inspect the rendered README article: it must begin with `BIM Agent Lab`, show the participant starting links and contain the current public-access statement. Do not mistake a raw-file request, a repository-title match elsewhere on the page, or a successful clone for this rendered-page check. Retain the observed path, heading, commit and result with the publication evidence.
