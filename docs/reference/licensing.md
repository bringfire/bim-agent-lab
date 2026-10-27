# Licensing

On October 26, 2026, the owner selected **MIT** for original BIM Agent Lab code and teaching material. The canonical terms are in the root [LICENSE](../../LICENSE), with the notice `Copyright (c) 2026 BIM Agent Lab contributors`. The license text follows the [Open Source Initiative MIT template](https://opensource.org/license/mit).

Retain that copyright and license notice when reusing or distributing the original material. Licensing, repository visibility and implementation acceptance are separate: selecting MIT does not make the repository public or close the outstanding review items. See [sharing status](repository-sharing.md).

Research candidates retain their own terms. Read the [resource register](../research/revit-resources.md) and [third-party notices](../../THIRD_PARTY_NOTICES.md) for the inspected evidence. A research candidate is not automatically an adopted dependency.

Before distributing an implementation, identify copied or adapted files, direct and transitive dependencies, and bundled assets. Preserve their required notices. SDKs, API binaries, and Autodesk sample files are currently acquired from their original sources; only links and preparation guidance are included here.

The Excel exporter adopts [ClosedXML and its resolved dependencies](../research/closedxml-dependency.md). Their notices are retained and installed with the plugin. The MCP server's [dependency review](../research/mcp-sdk-dependencies.md) identifies its SDK consumption and adapted source. The lab's MIT grant does not replace these upstream terms or cover Autodesk products and models acquired separately.

## Agent procedure for keeping the grant accurate

1. Keep the standard MIT grant in the root LICENSE and link it from README.md. Preserve the owner's recorded choice; do not infer a different license from a dependency or public visibility.
2. Inventory copied/adapted source, packages and assets before distribution. Update THIRD_PARTY_NOTICES.md and the project-specific retained notices without relicensing upstream material.
3. Update current status documents when a licensing decision changes. Preserve dated journal/review records as history rather than rewriting what was true at the time.
4. Verify local documentation links, the license text and the final Git diff. Follow the [publication procedure](../how-to/review-and-publish-a-repository.md) and the owner's actual direction. The [October 26 review](public-distribution-review-2026-10-26.md) records the history/distribution checks for this publication; later changes require review of their own.

The [participant account](../tutorials/16-prepare-the-participant-packet.md#choose-the-license-and-make-the-readme-useful) explains how the owner directed this decision.
