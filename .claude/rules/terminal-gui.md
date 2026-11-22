---
paths:
  - "src/Source191.Cader.Tui/**"
  - "tools/HeadlessTerminal/**"
  - "tools/ReadmeScene/**"
  - "tools/New-IconPixels.ps1"
  - "assets/icon/**"
---

# Terminal.Gui 2.5.0 facts

"Read" means read from the 2.5.0 assembly decompiled with ilspycmd; "observed" means seen in a throwaway program in the headless terminal. The spikes, decompilation notes and measurements are in `docs\verified-facts\terminal-gui.md`.

## Basics

- **On .NET 11** (14 November 2026, runtime 11.0.0): 2.5.0 ships only a `net10.0` library and runs on `net11.0` unchanged (suite, smoke and hand-off checks passed, run 43638076129). The facts below keep their own dates; none was re-observed by hand on .NET 11.
- **Instance API:** `Application.Create().Init()`, `app.Run(window)`, `app.RequestStop`, `app.Invoke`, `app.AddTimeout`; set `app.Driver.Force16Colors = false` or the palette collapses to 16 colours.
- **Never block the UI thread** on an awaited call (`GetResult()`): Terminal.Gui installs a synchronisation context and the continuation deadlocks. The hand-off check was built because of it.
- **Obsolete but used:** `TextView` (pragma CS0618) is the wrapping box. `OptionSelector` replaces RadioGroup. Rows take colours from `TableView.Style.RowColorGetter`.
- **Logging** (read, 15 November 2026): `Logging.Warning` and its siblings put the caller only inside the message text, with `EventId` 0; `Logging.PushLogger` sets a logger per async flow (tests use it). So `TerminalGuiBridge` reads the raising method from the call stack, skipping its own frames, `Microsoft.Extensions.Logging`'s and `Logging`'s, which `Program.cs` hands it as a `Type` (Core cannot reference Terminal.Gui: `LayeringShould.KeepTerminalGuiInsideTui`).

## Focus, dialogs and labels

- **Focus roles:** a scheme with Focus equal to Normal hides focus everywhere; ListView draws the selected row with Focus when focused and Active otherwise; a TextField needs its own background or is invisible when empty, and a read-only one is drawn with ReadOnly once it loses focus.
- **Selected row of an unfocusable table:** drawn with the scheme's Active role, which Terminal.Gui derives as a lighter shade when unset, so `Palette.SelectedRow` names those colours and sets every role, and a cell that keeps its own colour (the TAB block, `CellPaint.Fill`) uses `Palette.SelectedRow(colour)`.
- **Dialogs:** a Button with IsDefault takes the initial focus; set focus from `Initialized`, not before `Run`. A Dialog of Height 12 has 8 usable rows and the button row overdraws the last. A Label that receives text later needs a fixed Height and non-empty initial Text.
- **TextField keys:** Enter raises the field's own `Accepting`, which KeyDown handlers on the field or dialog never see. Ctrl+C copies its selection through `app.Clipboard`, which Windows Terminal passes through when it has no selection of its own.
- **Hotkeys:** a Label reads `_` as its hotkey mark unless `HotKeySpecifier` is set to U+FFFF before `Text`.
- **`MemoryDialog`** uses Height lines + 8: a Dialog of one Label per line and a Button drew nothing in its last two content rows at lines + 6, two rows fewer than the rule above allows, unexplained.
- **`Initialized` is synchronous** (read, 15 November 2026): `app.Run` raises a dialog's `Initialized` before its first draw, SubViews first, so a binder's `Push` from `Initialized` always runs.
- **Invisible characters** (read, 15 November 2026): `TextField` inserts any key with a grapheme but a control rune or a Ctrl or Alt chord, so a space or U+200B is kept and draws blank. Both filters therefore ignore leading blank, control and format characters (`CharacterKinds.WithoutInvisibleStart`).
- **The help** (`Tui\HelpDialog`) is a read-only `TextView` up to nine tenths of the screen, focused so it scrolls. A Dialog reserves its own button area, so the box is `Y = 0` with `Dim.Fill()`.
- **A first runnable dialog** (What's new, before the main window) raises `IsModalChanged` with true like any dialog, so `DialogTabTitle` writes its title and the window run after it writes its own (observed 1 November 2026). `count` reads plain text only, so find a title in `drive-raw.txt` or by a smoke step's marker.

## Keys and input

- **Printable keys:** `KeyMapping.TryFrom` reads a printable character with `key.AsRune`; `KeyMappingShould` pins the keys the reducers use (the named keys, F5 among them, `d`, `D`, space, `<`, `/`, `?`, and nothing for Ctrl, Alt, F1, Tab or a rune outside the basic plane).
- **`Key.TryGetPrintableRune`** reads the same keys per its XML docs, but whether it agrees with `AsRune` for Alt, AltGr and Ctrl chords is untried at a real Windows Terminal, so the swap was not made (2 November 2026). The spike to run first is in the evidence.
- **Typing into a running console from outside:** `WriteConsoleInputW` KEY_EVENT records (virtual key, scan code, character, down then up) into the program's own console input are read as typed keys, focused or not (observed 4 November 2026, Windows Terminal 1.25.2733.0, `tools\ReadmeScene`); `NewKeyDownEvent` is out of reach because `ConsoleScreen.Run` creates the `IApplication`.

## Timers

- **Timers tick under a dialog** (observed and read, 1 November 2026): `app.AddTimeout` callbacks keep running while a modal dialog runs its own `app.Run`, so the refresh timers and `Heartbeat` tick under a dialog.
- **A dialog opened from a timer blocks `Invoke`:** `RunTimers` holds its lock while calling each callback, and `app.Invoke` from another thread is a zero timeout, so a dialog opened inside a timeout callback (and, read only, inside an `Invoke` callback) blocks every other thread's `app.Invoke` until it closes.
- **`SpinnerView.AutoSpin`** is safe to set repeatedly and before the view has an `App`; the default style is Line, and `SpinnerStyle.Dots` renders in the headless capture.

## Tooltips and disposal

- **`View.RemoveAll()`** returns the removed views and the caller owns them, so `ConsoleScreen` disposes what it removes; a tooltip stays registered for a disposed label until `RemoveToolTip(view)`, so the key bar removes each label's first (2 November 2026).
- **Table tooltips:** a table raises no position reports unless `MousePositionTracking` is true and its tooltip is evaluated only on entry, so the header tooltip is shown by hand (`ApplicationToolTip.ShowFor` from `table.MouseEvent` with `ScreenToCell(position, out header)`).
- **Placing them:** a tooltip is drawn at its target's bottom-left, so `TableToolTips` targets a zero-width `Anchor` over the header (or a Config row, `IScreenDefinition.RowTip`) at the pointer's column; it still shifts a wide one left. `ToolTipHost` disposes the content views it replaces (read, 16 November 2026).
- **Hover headless** with SGR motion `\u001b[<35;col;rowM` (1-based) after 8000 ms.

## Tabs

- **The strip:** `Tabs` (no `TabView` or `TileView` in 2.5.0) shows each SubView as a tab titled by its `Title`, in three rows (outer border, titles, separator); the selected body starts on the fourth, framed. The selected tab is the focused SubView, so a tab's view has `CanFocus = true`.
- **Its span** is each title's length plus a border cell either side, adjacent tabs sharing one; `BorderView.TabOffset` and `EffectiveTabLength` are public (20 November 2026), so the counts label sits after it on the title row.
- **Clicks:** setting `Value` and clicking a title both raise `ValueChanged`, so the console applies a click under a binding guard. Removing a SubView selects another and raises `ValueChanged`, so `ConsoleScreen` disposes its tabs under that guard.
- **Spinners on a hidden tab** redraw their own tab, title box and frame over the shown one, so `ConsoleScreen.Show` tells both tabs which is shown (`ITabBody.Shown`) and a hidden tab keeps its spinners still. A table refresh is safe: `FrameChanged` redraws the window.
- **Arrow keys:** `Tabs` binds arrows and paging to moving between tabs, so `ConsoleScreen` removes those bindings from the strip and both tab views.
- **`Shortcut`** draws its command before its key with Terminal.Gui's key names, so the key bar is label pairs with `MouseEvent` handlers on `MouseFlags.LeftButtonClicked` (30 October 2026).
- **A changed title is not re-measured** (read and observed, 20 November 2026, SDK 11.0.100): `Title`'s setter only calls `SetNeedsDraw`, and one `SetNeedsLayout` leaves the tabs after it placed for the old width.
- **Two ways that work:** `tabs.SetNeedsLayout(); tabs.Layout(); tabs.SetNeedsLayout();`, or setting `BorderView.TabLength` (via `(BorderView)tab.Border.View`) to the title's width plus two. Neither changes the shown tab or focus or raises `ValueChanged`.
- **Coloured titles:** cancel the header's `DrawingText` (`BorderView.TitleView`) and draw the parts with `Move`, `SetAttribute` and `AddStr` (observed headless only).
- **Glyphs** `◐ ● ✓ ✗ ■` measure one cell each in a title, and a title change redraws every tab, about 4 KB at 120 x 36 (headless only). Later tabs move with a changed title; a click selects the tab drawn under the pointer; text on a tab's frame top edge never shows.

## Tables

- **Cell edges** (read and observed in Windows Terminal, 18 November 2026): with `ShowVerticalCellLines` off and a space separator, `RenderRow` writes a blank at each column's last cell and `TruncateOrPad` cuts a full-width cell by one, so text drawn per cell always breaks between columns; a line across a row is drawn from `DrawComplete` (`ColumnSizing.DrawHeadings`).
- **`ScreenToCell`:** with the header's outer vertical line shown the first column starts at `X` 1, so `ScreenToCell(0, y)` answers null for every row; ask at the right edge for the row. `CalculateMaxCellWidth` measures every rendered cell, so an empty cell never widens its column.
- **Scroll** (read, held by `TableScrollShould`, 19 November 2026): setting `Table` calls `SetSelection(0, 0)` then `Update()`, whose `EnsureCursorIsVisible` scrolls to the top; `SetSelection` never scrolls; `View`'s `Viewport` setter keeps `RowOffset` within the content.
- So `Tui\TableScroll` reads `RowOffset` before a swap, puts it back, calls `EnsureValidScrollOffsets`, and calls `EnsureCursorIsVisible` only when the selected key or filter changed; `ColumnSizing.RestoreOffset` runs after it, because that call also scrolls back to the cursor's first column.
- **`TopResizable`** was observed for the withdrawn resizable-detail-pane (20 November 2026); what it does is in the evidence.

## Pickers and images

- **`OpenDialog` for folders** (`OpenMode.Directory`, `AllowsMultipleSelection`; spike, 30 October 2026) is the roots picker. It derives from `Dialog<IReadOnlyList<string>>`, so `DialogTabTitle.Apply` takes a `Runnable`. It opens at 96 x 28 in 120 x 36 and takes `Palette.Focusable`.
- **Its keys:** focus starts in the path box; Tab reaches the list, Down moves (the path follows), Shift+Down extends, Enter in the list confirms, Space toggles nothing. `FilePaths` answers in the order chosen (`Path` is empty for several); `Canceled` is true after Esc.
- **`OpenDialog` for a file** (read, 1 November 2026) is Open folder with's Custom picker. `new AllowedType("Programs", ".exe")` matches the path's ending ignoring case; with any type the list shows folders and matching files, and a typed path of another type is refused in the dialog.
- **Its values:** `MustExist` defaults to false (F5 checks it), but a folder typed in File mode is refused. `Path` set before `Run` opens there; with one file allowed `FilePaths` answers `[Path]` once accepted; `Canceled` is `Result == null`.
- **`OptionSelector.Labels`'** setter disposes and rebuilds every option, so the dialog retitles the existing `CheckBox`es instead, keeping focus.
- **`ImageView`** takes a `Color[,]` and draws it through Sixel or Kitty when `app.Driver.SixelSupport` says so (Windows Terminal draws Sixel); otherwise it colours one cell per pixel, so `LogoIcon` stays hidden. The headless terminal reports no support, so captures never show the icon.
- **The icon's pixels** are `assets\icon\logo-48.rgba`, written by `tools\New-IconPixels.ps1` and embedded in the Tui assembly; `LogoIcon.Fit` sizes the view from the reported cell size.
