# Design

## Context

See proposal.md, Why. What the code does today:

- **Titles:** `Wording.TabTitleFor` answers the fixed titles Sessions, Worktrees and Config, and `ConsoleScreen.HeaderSpan()` works out the strip's width from them once ("the titles never change, so neither does this").
- **Counts:** `IScreenDefinition.Counts` answers the shown tab's `TextPart`s (`Wording.SessionCounts`, `WorktreeCounts`, `ConfigCounts`); `ConsoleScreen.ShowCounts` lays them out as one label per part in its tone, at `HeaderSpan() + 2` on the titles' row, and `ShowFrameTitle` puts the table's title at the right end, cut where it would reach the counts.
- **Hidden tabs:** the Sessions tab lists whether shown or not; the Worktrees tab rescans for a change while hidden but on its timer only while shown; Config reads its setup only when shown.
- **Specs:** `console-layout` holds the layout in "Sessions and worktrees are two tabs of one screen" (scenario "Tab titles stay put") and "The counts and the table's title sit beside the tabs".
- **Known `Tabs` facts (`.claude\rules\terminal-gui.md`, `console-screen.md`):** the strip takes three rows and spans each title plus a shared border cell; a title is drawn with the Focus role and the lines with Normal; a title that changed left the strip measured for the old one; a spinner redrawing its own tab drew over the shown tab's frame; removing a SubView selects another and raises `ValueChanged`.

## Goals / Non-Goals

**Goals:**

- Answer each open question below with an observed or read fact, recorded in `terminal-gui.md` with its date and version.
- Weigh the four options against the same criteria and recommend one, with its cost.

**Non-Goals:**

- Production code, tests or a spec delta: those belong to a follow-up change, if any.
- Prototyping inside `ConsoleScreen`: the spike stays a separate program, so nothing half-built lands on a branch.

## Decisions

### 1. The questions the spike must answer

1. **Re-measuring:** after `Title` changes on a tab of a `Tabs`, does the strip draw the new width? If not, which public call makes it (`SetNeedsLayout`, `Layout()`, `SetNeedsDraw` on the strip or the tab, setting `Title` before or after `Add`)? Does that call keep the shown tab, its focus and its scroll, and raise no `ValueChanged`?
2. **Colour:** can one title draw in more than one colour (a `TextFormatter` setting, a hot-key colour, a Focus role per tab), or is it one role for the whole title? Does the shown tab's title differ from a hidden one's?
3. **Glyphs:** do the counts' glyphs (`◐ ● ○ ✓ ✗ ■ ◇`) measure and draw as one cell each in a title, in Windows Terminal and in the headless terminal?
4. **Movement and clicks:** as a title grows and shrinks, do the tabs after it move with it, and does a click on a title still select the tab drawn under the pointer? What does the strip do when the titles are wider than the screen?
5. **Overlay:** does text drawn over the tab's top edge (the row under the titles) survive a redraw of the strip, a tab switch and a spinner on the hidden tab?
6. **Cost per refresh:** does a changed title redraw the whole window or only the strip, and does it flicker at the Sessions refresh rate (5 s) and on a burst of folder changes?

### 2. Read first, then observe

Each question is read from the decompiled Terminal.Gui 2.5.0 assembly (`ilspycmd`, as the earlier `Tabs` and `TableView` facts were) before it is tried, so the spike checks a claim rather than searches. A fact is recorded as observed only when it was seen on screen or in a headless capture; otherwise it says read.

- Alternative: try the options inside `ConsoleScreen` on a branch. Rejected: it mixes a guess with production code and needs reverting; a throwaway program answers the same questions in isolation.

### 3. Where the spike runs

A throwaway console program in a scratch folder under `%TEMP%`, referencing the same Terminal.Gui 2.5.0 package, never committed. It shows a `Tabs` with three tabs and a timer that grows and shrinks the first title through counts like `Sessions ◐3 ●2`, and logs `ValueChanged`, focus and the strip's measured width. It runs in `tools\HeadlessTerminal` (with `count`, captures and SGR clicks at 120 x 36) and by hand in Windows Terminal, the latter for colour and glyph width. Started with NO_COLOR, CLAUDECODE, CLAUDE_PID and CLAUDE_CODE_* cleared, as `CLAUDE.md` says.

### 4. The options weighed

- **A, counts in each title:** `Sessions ◐3 ●2 ✓1 ✗1 ■4`, `Worktrees 2 changes`, `Config 1 to set up`; a compact form, since today's words take about 60 cells. Shows every tab's counts at once, but the hidden tabs' may be stale (Context).
- **B, one marker on Sessions:** `Sessions ◐3` while any session needs input, `Sessions` otherwise; the full counts stay where they are. The smallest change that answers "a session needs input" from any tab.
- **C, inside the tab's frame:** today's counts moved onto the tab's top edge (needs question 5) or onto a row inside the tab (costs one table row).
- **D, unchanged:** today's layout, if none of the above is worth its cost.

### 5. The criteria

Each option is scored on: the strip re-measures without switching the shown tab or moving focus (1); amber and red attention stay visible (2, 3); clicks land on the tab drawn under the pointer (4); no table row is lost at 120 x 36; no flicker at the refresh rate (6); and the cost of the follow-up, counted as the files, facts and spec scenarios it would change (`ConsoleScreen`, `Wording` and its facts, `console-layout`, the smoke script, `tools\ReadmeScene`'s images). An option that fails either of the first two is not recommended.

### 6. Where the answer goes

The findings and the recommendation are added to this file under a Findings heading, so they are archived with the change; the verified facts go to `terminal-gui.md`, where later work will look. If the recommendation is A, B or C, the Findings name the follow-up change and the `console-layout` requirements it would modify; the follow-up is proposed only when the owner agrees.

## Risks / Trade-offs

- [The headless terminal does not draw colours or glyph widths as Windows Terminal does] → question 2 and 3 are answered in Windows Terminal by hand, with a screenshot attached to the pull request.
- [Read facts differ from behaviour] → nothing is recommended on a read fact alone where it can be observed; each fact says which it is.
- [The spike grows into a prototype] → it answers the six questions and stops; it is not kept.
- [A re-measure works only through a call that is not public] → recorded as a blocker for A and B, not worked round with reflection.
