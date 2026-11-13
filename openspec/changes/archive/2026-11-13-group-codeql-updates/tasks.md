# Tasks

## 1. The bump and the group

- [x] 1.1 Pin `init` and `analyze` in `.github\workflows\codeql.yml` to `33d7b85da1a04171e211befe2c739303b9a4c066 # v4.39.3` (verified as the commit `v4.39.3` points at with `gh api repos/github/codeql-action/commits/v4.39.3`).
- [x] 1.2 Add the `codeql-action` group to the `github-actions` entry of `.github\dependabot.yml`.
- [x] 1.3 Say the steps are grouped, and why, in `MAINTAINING.md` and `.claude\rules\checks.md`.

## 2. Close out

- [x] 2.1 Archive the change and run `openspec validate --all`.
- [ ] 2.2 On the pull request, `analyse (csharp)`, `analyse (actions)`, `check` and `handoff` pass; merge, then close #220 and #221 as superseded.
