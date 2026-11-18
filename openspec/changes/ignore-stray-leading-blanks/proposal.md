# Proposal

## Why

`n` sits next to the space bar, so pressing both at once is easy, and the space lands in the new-session wizard's filter as it opens (issue #339). A filter of one space matches no repository, so the list goes empty and the user thinks Cader has lost them. #298's diagnostics showed exactly that (`Whitespace, 1 character, list filled` 92 ms after opening), and the reporter has since reproduced it by hand. A blank character at the start of a filter or a typed name is never intended, yet today it empties the wizard's list, narrows a table's `/` filter to names with a space before the text, and becomes part of a session's name from the fixed-folder wizard and `cader here`.

## What Changes

- The wizard's repository filter ignores blank and invisible characters (whitespace, control and format characters) at its start when matching, so a stray space shows every repository and `" north"` matches like `"north"`. Spaces after the first visible character still count, so repository names with spaces can still be found. The filter's text is kept as received, so the `wizard filter first changed` diagnostic is unchanged.
- **Removed:** the wizard's "The filter holds only blank or invisible characters; Backspace clears it." problem line (`WizardProblem.InvisibleFilter`), since such a filter now shows every repository and has nothing to explain.
- The tables' `/` filter (Sessions, Worktrees and Config) follows the same rule: leading blank and invisible characters are ignored; a filter holding only them already shows every row.
- A typed session name loses blank characters at both ends before it is checked, in the wizard (both kinds), `cader here` and rename, so a stray space never reaches a session's name or tab title. Rename already trims its name; the rule moves into the one place every typed name passes through. A name of only blanks is still refused with "A session name is required."
- Patch version bump and a changelog entry.

Out of scope:
- Stopping the space from reaching the filter at all (cancelling the text change in the dialog), which would only hide the cursor offset.
- Trailing blanks in a filter, which are meaningful (`"my "` separates a word).
- Any other dialog's text box: the rename, typed-confirmation and problem-report dialogs are opened by keys away from the space bar.
- Multiple Claude accounts beyond naming a session's owner, which is a later change.

## Capabilities

### New Capabilities

(none)

### Modified Capabilities

- `session-launch`: the session name drops blanks at both ends; the repository picker ignores leading blank or invisible characters instead of explaining them.
- `session-listing`: the `/` filter ignores leading blank or invisible characters.

## Impact

- `Source191.Cader.Core`: `Domain\CharacterKinds` gains the leading-invisible rule and loses `AllInvisible`; `Domain\RowFilter`, `Application\Launch\RepositoryChoices` and `Domain\SessionName.Validate` apply it; `WizardProblem.InvisibleFilter`, its rule in `WizardView` and its words in `Wording` go; `SessionsView`'s rename stops trimming on its own.
- `tools\HeadlessTerminal`: the smoke script's step that typed a space into the wizard's filter and waited for the problem line goes.
- `.claude\rules\terminal-gui.md` and `checks.md`: the sentences naming `WizardProblem.InvisibleFilter` and the smoke step are updated.
- Patch version bump in `src\Source191.Cader\Source191.Cader.csproj` and a `## Unreleased` changelog line.
