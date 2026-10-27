# Ask the agent to review before making the repository public

The author wanted participants to keep using the lab after the seminar. A useful README and a license were part of that handoff, but the repository also contained the history of how we built it: failed attempts, screenshots, research and test evidence.

## How we engaged the agent

After choosing MIT, the author gave this actual instruction:

> Ok, let's do the public-distribution review and make the repo public once it's ready. Keep recording the process in both teaching layers.

This authorized a concrete sequence: inspect what would be exposed, address publication problems, record the evidence, then change access and verify it. The agent did not need to ask again for the same publication permission. It did need to stop if the inspection found a real unresolved confidentiality or rights issue.

The agent reviewed all 79 reachable commits at the starting checkpoint, not just the files visible in the current folder. It also inspected document contents and metadata, screenshots, compressed evidence, dependency notices and GitHub's other publication surfaces. Research clones, working Revit models and private installation/configuration backups were excluded from Git.

## What the review taught us

The secret scanner reported 652 matches in history. Inspection classified 650 as expired, temporary references to a local Revit model/checklist session and two as file hashes. Those are different from account passwords or API keys. The agent preserved the findings and the reasons for their disposition; it did not describe this as a scan with zero matches. A second scan of decoded documents and logs overlapped the first and was recorded separately.

The archived workshop proposal claimed support for Revit 2022–2027 and broader capabilities than we had verified. The agent kept the original planning artifact intact and added a clear archive notice. Current readers are directed to the verified Revit 2024 baseline and the actual readiness-check scope.

The owner then said, **“we will add revit 2026 tomorrow.”** That is a next-work intention for October 27, not evidence that the current version supports it.

The approved PDF and Word packet had already gone to the organizers. Their repository URL and QR destination stay valid. We preserved those supplied files and explained their preparation-time private-access caption in the online handout guide.

After publication, the agent tested access without an account. Download worked, but the first checkout failed in a deeply nested Windows folder. A fresh clone with Git's long-path option succeeded, and the README now includes that option. This is why asking the agent to test the participant's actual starting step matters. The public repository, README and MIT license were verified without signing in.

## How to judge the result

Ask for the reviewed commit, the findings and their dispositions, and proof that someone who is not signed in can read and clone the repository. Check that the license covers our original work while upstream notices remain intact.

Public access and product acceptance answer different questions. This publication shares a **teaching source preview**. It does not close the outstanding R3 review, establish desktop Stop behavior, add Revit 2026 support or create a downloadable plugin release.

For your own project, this is a suggested prompt, not a quotation from our development:

> Review everything that would become public, including reachable Git history, documents, screenshots and repository attachments. Identify credentials, private information, third-party obligations and unsupported claims. Explain every finding and what you changed. Preserve evidence rather than silently rewriting it. Once the repository is ready under my authorization, publish it and verify anonymous access. Keep publication separate from runtime acceptance.

The [dated review and publication record](../reference/public-distribution-review-2026-10-26.md) contains the result. The [agent procedure](../how-to/review-and-publish-a-repository.md) explains how to reproduce the review and publication checks.
