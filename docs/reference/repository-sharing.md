# Repository hosting and publication status

On October 23, 2026, the author asked: **"we should create a remote repo that we can make public and disseminate."**

Canonical destination: [https://github.com/bringfire/bim-agent-lab](https://github.com/bringfire/bim-agent-lab).

The agent created it **private** under the authenticated `bringfire` account on October 23 and configured local `origin`. The owner selected MIT and explicitly authorized public publication on October 26. Following the [distribution review](public-distribution-review-2026-10-26.md), the repository became **public on October 26, 2026 (Eastern)**. Participants can read or clone it without signing in. This is a teaching-source preview, not a packaged plugin release or evidence of full acceptance.

The initial upload uses current `feat/revit-mcp-bridge` history, with the same checkpoint on remote `main` and `feat/revit-mcp-bridge`; remote `main` is the participant landing branch. The older local `main` is preserved. No force push, history rewrite or merge into that local branch is required. Continue inspecting branch/upstream state before changes; do not assume local `main` equals the new remote landing branch.

## Pre-upload inspection

At source checkpoint `b8ff62ebed07f59d81c061cd9611b4af546e65d0`, inspected 76 reachable commits and 3,301 named historical blob objects. There were no matches for the checked private-key, GitHub-token, OpenAI-key, AWS access-key or literal long bearer-token signatures. The historical path check found no RVT/RFA/RTE/RFT/IFC, DLL/EXE, NuGet/ZIP, PFX/P12, `.env`, `auth.json` or `credentials` paths. These are bounded checks, not a certification that all confidential information is absent.

The tracked history intentionally contains source, teaching materials, third-party license notices, sample screenshots, logs, local machine paths/usernames and historical review/evidence records. No private installation receipts or whole desktop configuration backups were uploaded. Ignored research clones and local sample models remain local. The new packet adds original editorial content, its generator, a PDF and verification metadata; rendering scratch files are ignored.

## Public publication and verification

The reviewed publication checkpoint is `77397bbe952566f769f7735912da3bb496338bd6`; subsequent documentation records the outcome. Both remote branches were advanced normally. No force push, history rewrite, release asset or external announcement was made.

- GitHub reports public visibility and recognizes the root [MIT License](../../LICENSE). Upstream rights/notices remain separate.
- The review covered all 79 commits at starting checkpoint `9a4ac92`, decoded documents/logs, screenshots, notices and GitHub surfaces. The 80-commit follow-up scan introduced no new scanner matches. [Findings and dispositions](public-distribution-review-2026-10-26.md).
- An unauthenticated API request and raw README/LICENSE reads succeeded. A full HTTPS clone with credential helpers/global settings disabled matched the reviewed commit and had a clean checkout. [Publication receipt](public-distribution/publication-2026-10-26.json).
- The first clone hit Windows path limits in its deeply nested audit directory after downloading successfully. Use `git clone -c core.longpaths=true https://github.com/bringfire/bim-agent-lab.git`; that fresh checkout passed. The option is local to the new clone.
- Current entry points preserve Revit 2024 as the verified baseline, the SDK cancellation limitation and the open R3 items. Revit 2026 is planned for October 27, not yet supported or verified here.

The supplied participant handouts retain their original bytes and preparation-time private-access caption. Their stable URL/QR now leads to the public repository; the [online handout guide](../../workshops/swacca-2026/handout/README.md) explains the access update. A later revised print edition should change the shared content source and be rendered/rechecked before distribution. No organizer files were silently replaced.

The [participant story](../tutorials/17-review-before-publication.md) explains how the author engaged the agent. The [agent procedure](../how-to/review-and-publish-a-repository.md) supplies the reproducible checks. Later contributions and release assets require their own review.
