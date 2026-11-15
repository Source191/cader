# Tasks

## 1. The script

- [x] 1.1 Verify first, on temporary copies of the csproj, that `Step-Version.ps1 -Preview` turns `0.21.1` into `0.21.2-preview.1` and `0.21.2-preview.1` into `0.21.2-preview.2`, that without the switch `0.21.1` still becomes `0.21.2`, that `0.21.1-local` is still refused, that only the Version line differs and that no BOM appears (these fail before the switch exists); then add the switch and its help text.

## 2. The workflow

- [x] 2.1 `pipeline.yml`: the bump step passes `-Preview`, puts `[dependabot skip]` in the commit message, and fails with `::error::` naming `DEPENDABOT_BUMP_TOKEN` when the secret is empty; a new step after it removes `dependabot-branch` (`always()`, the Dependabot condition); the comments say why. Verify the workflow parses: this pull request's own `check` runs, and for a human actor both Dependabot steps and the removal step are skipped.
- [ ] 2.2 `dependabot.yml`: a `nuget` group (`applies-to: version-updates`, `patterns: ["*"]`) and a comment saying why. Verify after merging (task 4.2) that a week's updates arrive as one pull request.

## 3. The docs

- [x] 3.1 MAINTAINING.md's "Dependabot and the version bump token" (preview on a stable base, the grouped pull request, `[dependabot skip]`, `@dependabot recreate`, the error when the token is missing), CONTRIBUTING.md's Versions (`-Preview`) and The changelog (Dependabot's line), and the Dependabot sentence in `.claude\rules\checks.md`. Verify by reading each against design.md.

## 4. The owner

- [x] 4.1 Create a fine-grained personal access token, resource owner Source191, only the `cader` repository, Contents read and write, and store it as the Dependabot secret `DEPENDABOT_BUMP_TOKEN`. Verify `gh secret list --app dependabot --repo Source191/cader` lists it and record its expiry here.
  - Done by the owner on 14 November 2026: `gh secret list --app dependabot --repo Source191/cader` lists `DEPENDABOT_BUMP_TOKEN` (updated 2026-11-14T17:00:24Z). The expiry was not given; the owner records it in MAINTAINING.md, Dependabot and the version bump token.
- [ ] 4.2 On the next Dependabot pull request that touches `src\` (or one started from Insights, Dependency graph, Dependabot, "Check for updates"), verify the bump commit appears with a preview `Version` and the Internal line, the new run's `check` passes, and a pull request that touches only tests or actions passes the format step; record the pull request numbers and run ids here, then archive the change.
