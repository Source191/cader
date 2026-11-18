# Proposal

## Why

Two automated checks are red for reasons that are not regressions in the console.

- **Contract (issue #338):** the nightly `claude-contract.yml` run failed `ClaudeContractShould.ReadTheLoginFromAuthStatus` on Claude Code 2.1.327 because it asserts that `claude auth status --json` exits 0. The CLI exits 1 when no one is logged in (checked 18 November 2026, CLI 2.1.327: `{"loggedIn": false, ...}` and exit 1), and on the runner no one is: the workflow logs in through `CLAUDE_CODE_OAUTH_TOKEN`, and the solution's `ProcessRunner` drops every `CLAUDE_CODE_*` variable from its children (`InheritedEnvironment`), so the `claude` the fact runs never sees the token. The rolling `claude-cli` issue will be raised every night until the fact matches the CLI.
- **Mutation (issue #329):** the release runs for 0.22.0-preview.36 and .40 left seven mutants that survived or had no covering test, in `TabTitle`, `WizardState` and `WizardView`. The strict release rule fails every release run while any stands.

## Benefit

- **Maintainers:** the nightly contract run and the release mutation run go green, so a red one means a real change in the CLI or a real gap in the tests again.
- **Behaviour:** two rules the console already has are held by facts: a tab title with a glyph not followed by a space is not a session's tab, and a click on a heading with no selection moves down to the next choice.
- **Knowledge:** `claude-code.md` records how `auth status` answers when logged out and under `CLAUDE_CODE_OAUTH_TOKEN`, replacing the "not yet known" note.

## What Changes

- **Contract fact:** `ReadTheLoginFromAuthStatus` expects exit 0 when `loggedIn` is true and exit 1 when it is false, instead of always 0, and keeps its check that the console reads a login exactly when `auth status` names an email and an organisation id.
- **`TabTitle`:** `FollowsOneGlyph` loses its `glyphLength > 0` guard, which the `EndsWith(" " + name)` check already implies (it is reordered first); a new fact, a glyph followed straight by the name (`◐xfix-123`), kills the two mutants of that line.
- **`WizardState`:** `FirstChoice` becomes the first choice found walking down from the top, the same walk `WizardView` uses to step over headings (`Nearest`, moved into `WizardState`), so the `FindIndex` comparison and its equivalent mutant go and the walk's bound is held by the existing empty-list facts.
- **`WizardView.Selected`:** with no selection the direction is taken as zero (`?? index`) rather than from a row above the list (`?? -1`), the same downward move with no literal to mutate; a fact on a wizard with no selection pins that a click on a heading moves down, which kills `direction <= 0`.
- **Version and changelog:** a preview step and an Internal entry.

Out of scope:

- Passing `CLAUDE_CODE_OAUTH_TOKEN` (or other user-set `CLAUDE_CODE_*` settings such as `CLAUDE_CODE_USE_BEDROCK`) through to the sessions Cader starts. Today a person who signs in only through that variable gets sessions that are not signed in; that is a behaviour change with security weight and belongs in its own change.
- Multiple Claude accounts beyond naming a session's owner, which is a later change.
- The static mutants left to the weekly full run.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None. No requirement changes; the change declares `skip_specs: true`.

## Impact

- **Tests:** `ClaudeContractShould` (Infrastructure.Tests, contract category), `TabTitleShould` and `WizardViewShould` (Core.Tests).
- **`src`:** `Domain\TabTitle`, `Application\Views\WizardState` and `WizardView`; no behaviour a user can reach changes.
- **Docs:** `.claude\rules\claude-code.md` (Accounts).
- **Version and changelog:** `Version` rises one preview step; `CHANGELOG.md` gets an Internal entry.
- **Users:** nothing visible.
