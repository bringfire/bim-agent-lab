# Public-distribution review — October 26, 2026

The author authorized a public-distribution review and publication once ready, with both teaching layers retained. **The review found no unresolved publication blocker for a teaching-source preview.** Full Revit/MCP implementation acceptance remains open. Publication/access results are recorded separately below and in [sharing status](repository-sharing.md).

Starting scope: `9a4ac9243b4570f1eab808036c3a0c90e9a3ba8a`, the identical remote `main` and `feat/revit-mcp-bridge` heads. There were no other remote heads, tags or PR refs. The worktree was clean. The review covered 79 reachable commits, 3,341 unique blobs (40,894,199 bytes) and 3,294 historical paths. Subsequent changes in this increment are publication documentation and a read-only inventory helper.

The [sanitized evidence](public-distribution/review-2026-10-26.json) records counts, tool/configuration digests, notice comparisons and limitations. Raw redacted scanner reports and extracted material remain local under ignored `references/publication-review-2026-10-26/`. This is an agent-performed distribution review, not an independent security certification.

## Findings and dispositions

| Area | Observation and disposition |
|---|---|
| Credentials | Gitleaks 8.30.1 with its pinned default configuration reported **652 history matches**: 650 historical local readiness-context values and two file hashes. The decoded-content scan reported **518 overlapping matches**: 512 context values and six hashes. No account credential was identified. There were no custom suppressions; matched values were fully redacted in scanner reports. |
| Context values | Source inspection of `ContextStore` confirms five-minute in-memory leases bound to bridge/document/checklist identity. Retained values describe earlier October sessions and have expired. They are evidence of local read requests, not cloud/API credentials. This disposition must not become a generic allowlist for future tokens. |
| Other sensitive patterns | Supplementary scans found no JWT, credential-assignment, credential-in-URL or signed-query match. Two confidentiality-label hits were our own publication guidance; six apparent private-network values were Windows/SDK version numbers. Thirteen email addresses occurred in upstream attribution/manifests. All commit author/committer identities used the public GitHub noreply address. |
| Models and private files | No tracked historical RVT/RFA/RTE/RFT/IFC/3DM, executable/DLL/package archives, private key stores, `.env`, whole desktop configuration or credential files were found by the path inventory. Working models, research clones, raw audit output and private installation/configuration recovery records remain ignored and local. |
| Documents and images | Reviewed text/metadata from three PDFs, one DOCX, one tracked XLSX and twelve decompressed gzip logs, plus all 22 unique screenshot images. The proposal was rendered for visual review. Office files contain no macros or embedded objects; DOCX has four intended public hyperlinks and an empty template bibliography part; tracked XLSX has no external relationships. Screenshot contents show teaching fixtures and author workstation context. |
| Retained identifying data | The public history intentionally retains the author's handle, local paths, sample identities, process/session IDs and expired context values. No participant contact list, customer model or confidential organizational attachment was identified. Public venue/organization research and upstream copyright attribution remain. |
| Unhandled binary | The only nontext diagnostic fragment is five bytes, `00 ff 0d 0a 2a`, in an existing malformed-wire fixture. It was inspected directly. Extraction reported no errors. |
| Redistribution | MIT covers original lab work. Thirty-one local package/font notice comparisons pass against recorded digests. Two additional SDK notice comparisons match pinned upstream **text** after CRLF/LF normalization, not bytes. Fourteen embedded upstream research items were checked: thirteen match declared digests directly; one manifest omits a UTF-8 BOM while its declared digest matches the original upstream bytes. All notices and historical bytes are preserved. |
| Unsupported archive claims | The original proposal overstates the current supported Revit versions and capabilities. Added an [archive notice](../../workshops/swacca-2026/archive/README.md) and linked current scope. Current entry points retain the 2024 baseline, known SDK limitation and open R3 items. Revit 2026 is planned for October 27, not accepted here. |
| GitHub surfaces | Before publication: zero issues/PRs, releases, Actions runs/artifacts, tags or PR refs; Pages and discussions disabled. An authenticated SSH lookup found no initialized wiki repository. No separate hosted content was observed. |

No history rewrite, credential rotation or deletion was justified by the findings. Two stale third-party README statements were corrected without modifying license texts or hashed implementation evidence. The review did not alter source dependencies, installed software, desktop settings or frozen acceptance binaries.

## Publication status and limits

Review preparation is complete; the publication operation and anonymous verification are the next steps in this record. The repository will be shared as a **teaching-source preview**, with no packaged plugin release or new runtime acceptance claim.

The approved October 29–30 Word/PDF handouts are preserved byte-for-byte because they were already supplied to organizers. Their repository URL and QR destination remain the publication destination; the online guide clarifies the preparation-time access caption. No new print-layout or organizer-branding claim is made.

This bounded review does not guarantee the absence of every sensitive statement or unknown vulnerability. It does not close historical Codex NC2 presentation, Codex baseline, current-build acceptance, desktop Stop or uninstrumented timing gaps. No application build or live test was needed for these publication-only changes.

Participant story: [review before publication](../tutorials/17-review-before-publication.md). Agent procedure: [review and publish](../how-to/review-and-publish-a-repository.md).
