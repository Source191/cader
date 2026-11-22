## Why

The console is laid out for 120 by 36, and below that it breaks in ways a user sees at once. Windows Terminal opens at 120 by 30 by default, and a split pane is often 60 to 80 columns wide.

- **The new-session wizard:** at 120 by 30 its form is drawn over the repository filter and list, so the repository can't be chosen; at 120 by 36 one of the four repository rows the spec promises is hidden under the Session name caption; below 80 columns the dialog's left edge is off screen. Observed in the headless terminal on 22 November 2026. The list keeps a minimum of four rows however little room the 90%-high dialog leaves it, so it runs under the form.
- **The table's title:** with no room left after the counts, an ellipsis is still drawn on top of them (`● 3 working…·` at 60 columns).
- **The key bar:** it isn't fitted to the width, so at 60 columns the second row stops at `/ F` and `? Help` and `q Quit` are off screen.

**Implement with:** Opus 5.5 at medium effort: layout arithmetic in pure Application code with sociable tests, then Terminal.Gui wiring checked in the headless terminal at several sizes.

## What Changes

- The wizard opening on the overview fits the screen: as tall as the screen allows, its repository list taking the rows the form leaves (at least one), and nothing drawn over anything else. At 120 by 36 at least four repositories are listed, as the spec already says.
- When the terminal is smaller than the wizard's minimum (80 columns and its form plus one repository row for the overview's wizard; 84 by 23 for the fixed-folder one), the wizard does not open, and the status line names the size it needs and the size the terminal is.
- The table's title is left out when it has no room after the counts, so nothing is drawn over them.
- On a terminal narrower than a key bar row, the row drops items from its end, keeping `? Help` and `q Quit`, so the help is always one key away; the help still lists every key.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `session-launch`: the wizard fits a terminal smaller than 120 by 36 or says it is too small.
- `console-layout`: the table's title never covers the counts, and the key bar keeps Help and Quit on a narrow terminal.

## Out of scope

- A scrolling wizard or a different wizard layout for small screens.
- Any dialog other than the new-session wizard (they are smaller, and none was seen to break at 120 by 30).
- Cutting or wrapping the counts themselves; the terminal clips them at its edge.
- Multiple Claude accounts beyond naming a session's owner, a later change.

## Impact

- `Application`: a pure fit for the wizard's height and list rows, and `KeyBar` fitted to a width; `Wording` gains the too-small words with their facts.
- `Tui\WizardControls`, `Tui\LaunchWizardDialog`, `Tui\DialogEffects` (the refusal), `Tui\ConsoleScreen` (`ShowFrameTitle`, `ShowBar`).
- `tools\HeadlessTerminal`'s smoke script is unchanged; the sizes are checked by hand in the headless terminal.
- `CHANGELOG.md` and the version in `src\Source191.Cader\Source191.Cader.csproj`.
