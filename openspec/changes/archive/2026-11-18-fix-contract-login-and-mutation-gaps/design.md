# Design

## Decisions

1. **The contract fact follows the CLI's exit codes.** `claude auth status --json` answers JSON either way and exits 0 only when `loggedIn` is true (seen 18 November 2026, CLI 2.1.327: a scratch `CLAUDE_CONFIG_DIR` gave `loggedIn: false` and exit 1; the same folder with an unverified `CLAUDE_CODE_OAUTH_TOKEN` gave `loggedIn: true`, `authMethod: oauth_token`, no `email` or `orgId`, and exit 0). The fact asserts that pairing, then its existing rule. `ClaudeCli.LoginAsync` already answers `NotRead` for any non-zero exit, and nothing in the console tells `NotRead` from `LoggedOut` (only `LoggedIn` names an account), so the adapter stays as it is.
2. **The token stays dropped here.** The runner drops `CLAUDE_CODE_OAUTH_TOKEN` with the session markers, so on the contract runner the fact sees the logged-out answer. Keeping the token would make the fact see a login, but it changes what every child of Cader inherits; that is out of scope (proposal) and recorded in `claude-code.md` so the next change starts from the fact.
3. **Delete before marking.** Following `checks.md` ("Before a mark"), each equivalent mutant is removed by a rewrite, and each killable one by a fact; no Stryker mark is added.
   - `TabTitle`: once `EndsWith(" " + name)` holds, `glyphLength` is at least zero, and at zero `Rune.DecodeFromUtf16` of an empty span answers `NeedMoreData`, so `glyphLength > 0` adds nothing. Removing it removes `>= 0` (equivalent) and the `||` mutant; the `" "` to `""` mutant and the `||` between the remaining checks are killed by `◐xfix-123`, a glyph with the name straight after it.
   - `WizardState.FirstChoice`: rows always start with a heading, so `index < 0` against `index <= 0` cannot be told apart. `FirstChoice` becomes `Nearest(rows, 0, 1)`, the walk `WizardView.Landing` already uses, moved into `WizardState` beside it. The walk's `index < rows.Count` (mutated to `<=`) then runs on an empty list in every "nothing matches" fact, where the mutant reads past the end and throws.
   - `WizardView.Selected`: a selection is null only when the list is empty, where no index is in range, so the `-1` of `?? -1` was never run. `?? index` gives direction zero, which `Landing` reads as down, the same as before. A fact on `Opened with { SelectedIndex = null }` (a state the record allows though the reducer never makes it, as `KeepTheSelectionWhenAnArrowMovesInAnEmptyList` already does for the opposite case) clicking the Folders heading expects the root below it, which kills `direction <= 0`.
4. **Red means the mutant.** The code is already right, so each new fact passes at once. To show it fails for the right reason, apply the issue's replacement by hand, run the fact, see it fail, then revert. The removed guard and the moved walk are refactors held by the existing facts.

## Risks

- **A future CLI that names an account under the token:** the fact holds either way, since it asserts only how the console reads what `auth status` prints.
- **A test on a state the reducer never makes:** it pins a rule of the record, not of a path; if the reducer later makes it, the rule is already the one wanted.
