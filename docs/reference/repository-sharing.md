# Repository hosting and publication status

On October 23, 2026, the author asked: **"we should create a remote repo that we can make public and disseminate."**

Canonical destination: [https://github.com/bringfire/bim-agent-lab](https://github.com/bringfire/bim-agent-lab).

The agent created it **private** under the authenticated `bringfire` account and configured local `origin`. Public visibility has not been enabled. The private remote is a teaching-source checkpoint, not a packaged plugin release or evidence of full acceptance. Participants without access will not yet be able to open or clone it.

The initial upload uses current `feat/revit-mcp-bridge` history, with the same checkpoint on remote `main` and `feat/revit-mcp-bridge`; remote `main` is the participant landing branch. The older local `main` is preserved. No force push, history rewrite or merge into that local branch is required. Continue inspecting branch/upstream state before changes; do not assume local `main` equals the new remote landing branch.

## Pre-upload inspection

At source checkpoint `b8ff62ebed07f59d81c061cd9611b4af546e65d0`, inspected 76 reachable commits and 3,301 named historical blob objects. There were no matches for the checked private-key, GitHub-token, OpenAI-key, AWS access-key or literal long bearer-token signatures. The historical path check found no RVT/RFA/RTE/RFT/IFC, DLL/EXE, NuGet/ZIP, PFX/P12, `.env`, `auth.json` or `credentials` paths. These are bounded checks, not a certification that all confidential information is absent.

The tracked history intentionally contains source, teaching materials, third-party license notices, sample screenshots, logs, local machine paths/usernames and historical review/evidence records. No private installation receipts or whole desktop configuration backups were uploaded. Ignored research clones and local sample models remain local. The new packet adds original editorial content, its generator, a PDF and verification metadata; rendering scratch files are ignored.

## Before public dissemination

**October 26 update:** the author explicitly authorized review and public publication once ready. The [complete distribution review](public-distribution-review-2026-10-26.md) found no unresolved publication blocker for a teaching-source preview; changing visibility and checking anonymous access are the next steps. The checklist below preserves the earlier preparation requirements. Full Revit/MCP acceptance remains separate.

- **Completed October 26:** the owner selected MIT for original code and teaching material; the root [LICENSE](../../LICENSE) and participant-facing [README](../../README.md) now record it. Preserve third-party terms and notices. [Licensing guide](licensing.md).
- Review the complete published history and archived proposal/evidence for personal, organizational and redistribution concerns. The private-upload pattern scan does not substitute for this review.
- Keep live-build acceptance gaps and dependency limitations explicit. Decide whether to publish a teaching preview or a verified release, and label it accordingly.
- Enable public visibility only on the author's direction, then verify unauthenticated access and the clone URL. Update the packet's access caption/edition and render all pages again before printing.

The participant handout uses the stable repository URL now, while identifying its preparation-time private status. No external announcement, collaborator invitation or release publication was sent.
