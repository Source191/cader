# Design

## Context

See proposal.md, Why. `codeql.yml` runs `github/codeql-action/init` then `github/codeql-action/analyze` in each matrix leg; Dependabot treats each path as its own dependency and so opens one pull request per step.

## Goals / Non-Goals

**Goals:** CodeQL passes on 4.39.3, and later CodeQL releases arrive as one pull request.

**Non-Goals:** changing what CodeQL analyses or when it runs.

## Decisions

- **One pull request of our own, not an edit to a Dependabot branch.** The group has to land in `dependabot.yml` on `main` anyway, and a hand-edited Dependabot branch is no longer rebased by Dependabot.
- **A Dependabot group matching `github/codeql-action*`.** CodeQL's error message recommends it, and the pattern covers `init`, `analyze` and any other step from the same repository added later.

## Risks / Trade-offs

- [The group only takes effect on Dependabot's next run] → acceptable: #220 and #221 are closed by hand, and this pull request does the 4.39.3 bump itself.
