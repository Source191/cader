## Context

- **Wizard:** `Tui\LaunchWizardDialog` opens the overview's wizard at `Width = 80, Height = Dim.Percent(90)` and the fixed-folder one at 84 by 23. `Tui\WizardControls` puts the filter at Y 0 and 1, the repository list at Y 2 with `Height = Dim.Fill(FormRows, 4)` (`FormRows` is 21), and anchors every form row with `Pos.AnchorEnd`. A dialog's border and button area take the remaining rows: at 36 rows the 32-row dialog gave the list three rows, so they are six.
- **What was observed (headless terminal, 22 November 2026, Debug build of 0.22.0-preview.53):**
  - **120 by 36:** three repository rows show, and the fourth is drawn under "Session name (required)". The list's minimum of four is the cause: `Dim.Fill`'s minimum wins over its margin.
  - **120 by 30:** the list's four rows and the filter lie under the form's top rows.
  - **70 by 24:** the dialog's left edge is off screen.
- **Title:** `ConsoleScreen.ShowFrameTitle` computes `room` after the counts; when `room` is 0 it still builds `"…"` and places it at the right edge, over the counts.
- **Key bar:** `Wording.SessionsBar`, `WorktreesBar` and `ConfigBar` answer a `KeyBar` with each item's `X`, laid out for 120 columns (`KeyBarShould` holds that width). `ConsoleScreen.ShowBar` draws it as given at any width.

## Goals / Non-Goals

**Goals:**
- At every terminal size the wizard either shows every control with no control over another, or does not open and says why.
- Nothing is drawn over the counts.
- `? Help` and `q Quit` stay on the key bar at any width down to the width they need.

**Non-Goals:** a scrolling form, other dialogs, cutting the counts.

## Decisions

1. **The wizard's height is worked out in `Application`, as a pure fit.**
   - **Shape:** `Application\Views\WizardFit.For(screenColumns, screenRows, chooser)` answers `WizardFit.Fits(width, height, listRows)` or `WizardFit.TooSmall(neededColumns, neededRows)`.
   - **The overview's wizard:** width 80, and height the screen's less a margin of `min(2, screen rows - minimum)`, so the console's frame shows round it once there is room and the dialog never jumps smaller as the screen grows.
   - **Its minimum** is the form (21 rows), the filter (2), one list row and the dialog's chrome (6, measured above; task 1.1 confirms the figure in the headless terminal before it is written down).
   - **The list's rows** are whatever the form and chrome leave.
   - **The fixed-folder wizard:** 84 by 23, as today, and too small below that.
   - **Why:** the numbers are testable without Terminal.Gui, and the 120 by 36 promise of four rows becomes a fact rather than a hope. The list's `Dim.Fill(FormRows, 4)` becomes `Dim.Fill(FormRows)` with the dialog's height set from the fit, so no minimum can push it under the form.
   - **Alternative rejected:** a scrollable dialog (Terminal.Gui 2.5.0's scrolling inside a `Dialog` is unverified here, and the user chose shrink-then-explain).
2. **Too small is a status message, not a dialog.**
   - `DialogEffects.StartSession` asks `WizardFit` with `app.Screen`'s size before opening; a `TooSmall` answer shows `StatusMessage.WizardTooSmall(neededColumns, neededRows, columns, rows)` on the tab, with words in `Wording` ("The new-session window needs at least 80 x 30; this terminal is 70 x 24.") and its tone in `Wording.ToneOf` (a refusal).
   - **Why:** a dialog that cannot fit cannot carry its own explanation either.
3. **The title is left out when it has no room.** `ShowFrameTitle` draws nothing when `room` is under two cells (an ellipsis and one character), so the counts are never overdrawn. The rule is tiny and stays in the binder, with a headless check.
4. **`KeyBar.FittedTo(int width)` drops items from a row's end, keeping Help and Quit.**
   - **What it does:** a row that ends past `width` loses its last items other than the ones whose `Press` is `?` or `q`, one at a time, and the kept items after a dropped one are moved left by its width and spacing. A row that fits is returned unchanged.
   - **Why in `Application`:** `KeyBar` is already a tested record there, and `ShowBar` just calls `Active.Bar(...).FittedTo(ScreenWidth)`; the comparison with the shown bar still works because the fitted bar is a new value only when the width cut it.
   - **Alternative rejected:** wrapping onto a third row (it takes a table row on every tab).

## Risks / Trade-offs

- **One repository row at 120 by 30 is cramped** → the filter still narrows it to the one wanted, and it is the user's chosen trade-off over refusing.
- **The chrome figure (6 rows) is measured, not read** → task 1.1 confirms it at two heights before the fit's tests pin it.
- **A smaller dialog changes the smoke check's screen** → the smoke check runs at 120 by 40 and is unaffected; the sizes are checked by hand in the headless terminal (task 3.1).

## Open Questions

None.
