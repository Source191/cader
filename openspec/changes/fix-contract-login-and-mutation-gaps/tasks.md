# Tasks

## 1. Domain

- [x] 1.1 `TabTitleShould.NotShowASessionInAnyOtherTitle`: add `◐xfix-123`. Apply `""` for `" "` by hand on line 29 and see it fail, then revert. Commit.
- [ ] 1.2 `TabTitle.FollowsOneGlyph`: remove the `glyphLength > 0` guard so `EndsWith` comes first (design 3); `TabTitleShould` stays green. Commit.

## 2. Wizard

- [ ] 2.1 `WizardState.FirstChoice`: answer `Nearest(rows, 0, 1)`, moving `Nearest` from `WizardView` into `WizardState`; `WizardViewShould` and `WizardStateShould` stay green. Apply `<=` for `<` in the walk's bound by hand and see `NarrowChoicesAsTheFilterChanges` fail, then revert. Commit.
- [ ] 2.2 `WizardViewShould`: with no selection, a click on the Folders heading selects the root below it. Commit.
- [ ] 2.3 `WizardView.Selected`: `?? index` for `?? -1` (design 3); the new fact stays green, and `<=` for `<` in `Landing` by hand makes it fail. Commit.

## 3. Contract

- [ ] 3.1 `ClaudeContractShould.ReadTheLoginFromAuthStatus`: expect exit 0 when `loggedIn` is true and 1 when false, and say in its summary why the runner sees no login. Run it with `CADER_CLAUDE_CONTRACT=1`, once as this machine's login and once with a scratch `CLAUDE_CONFIG_DIR` (logged out). Commit.
- [ ] 3.2 `claude-code.md` Accounts: record the exit codes and the token's answer, replacing the "not yet known" note. Commit.

## 4. Check

- [ ] 4.1 Raise `Version` with `.github\scripts\Step-Version.ps1` and add an Internal entry under Unreleased in `CHANGELOG.md`. Verify `dotnet build -c Debug`, `dotnet test -c Debug --no-build` at the solution and `dotnet format --verify-no-changes` pass.

## Workflow follow-up

- Archive the change in the same pull request (`/opsx:archive`) and verify that `openspec validate` passes.
- Open the pull request with Why, Benefit and What changed, saying it closes #338 and #329.
