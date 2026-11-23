---
paths:
  - "src/Source191.Cader.Tui/**"
  - "src/Source191.Cader.Core/Application/Views/**"
  - "src/Source191.Cader.Core/Application/Detail/**"
  - "test/Source191.Cader.Core.Tests/Application/Views/**"
  - "test/Source191.Cader.Core.Tests/Application/Detail/**"
  - "**/LaunchProgress*.cs"
  - "**/FolderLabels*.cs"
  - "**/WhereLabel*.cs"
  - "**/HeadingLine*.cs"
  - "**/TableLine*.cs"
  - "**/StatusTone*.cs"
---

# The console screen

How the console is laid out, and its detail pane, status line and dialogs. **Refreshing, worktree scanning and the Config tab are in `console-refreshing.md`.** The rules every view follows (reducers, effects, keys and columns as data in `Wording`) are in `effects-and-views.md`. The change names and versions that dated each part are in `docs\verified-facts\console-screen.md`.

## Layout

- **One application:** `Tui\ConsoleScreen`. At the top, one settings row (`Wording.SettingsRow`, orange labels and white values as `TextPart`s, an available update in amber) to the right of `Tui\LogoIcon` where the terminal draws images; the roots and the worktree root are on the Config tab.
- **The tab strip:** a `Tabs` strip with the fixed titles Sessions, Worktrees and Config (a changed title is re-measured only by a two-pass layout or `BorderView.TabLength`: `terminal-gui.md`). The active title is bold cyan and every frame grey (`Palette.TabFrame`), because frames coloured per tab broke at the joins the tabs share.
- **On the title row:** the counts after the titles (`Wording.SessionCounts`, `WorktreeCounts`, `ConfigCounts`; the STATE words they share with the STATE column are `Wording.StateWord`) and the table's title right-aligned (a filter being typed before it, cut at its end, and left out with under two cells left).
- **The body:** one `TableBody<TState, TRow>` per view, its header a closed grey box with white names, and at the bottom the two-row key bar (`Wording.SessionsBar`, `WorktreesBar`, `ConfigBar`: placed `KeyBarItem`s drawn as clickable label pairs with tooltips), fitted by `KeyBar.FittedTo`, which drops a row's last items but `? Help` and `q Quit`.
- **Tab and scroll keys:** Left and Right (`TabKeys.TryFor`) step between tabs as `NextTab` and `PreviousTab`; `SelectTab` is only Esc and a click on a title. `<` and `>` (`ScrollKeys.TryFor`) scroll a table's columns (`ColumnSizing.Scroll`), because Shift+Left and Shift+Right never reached Cader in Windows Terminal.
- **State:** `ConsoleState` holds every tab's state and the active tab. `ConsoleComposition` hands the console one `ScreenContext` (tab title, settings row, whether a release is known, a newly found one's announcement, the clipboard, the diagnostic log).
- **Table titles:** a `TableTitle` fact (`ListSelection.Title(noun, labelOf)`) that `Wording.Describe` words. The worktrees title counts branch and origin rows apart as branches (`TableTitle.Apart`: `10 worktrees, 3 branches`) and names the selected row by `WorktreeRow.Name`, its folder or, for a row with no folder, its branch.
- **Worktree row keys:** `WorktreeRow.Key` (the selection, the pane's answers) is the folder for a worktree row and `<repository key>|branch|<name>` for a branch or origin row, whose `Placement` is `NoFolder` and `Path` its repository. A repository's folder rows come before its branch rows.
- **BRANCH:** after HEALTH, `ColumnFit.Natural`; it shows `WorktreeRow.Flag` as `Wording.BranchCell` in `Wording.ToneOf(BranchFlagKind)` through `CellPaint.Text`, blank for `None`. `WordingOfBranches` holds each flag's glyph, word, tone and the pane's reason (`Wording.BranchReason`).
- **A branch or origin row's pane** reads no git (its folder is the main checkout's): the Branch line with the reason, the pull request, the Repository and the commits at risk, never Folder, Created or a listing.

## Grouping and cells

- **Rows are grouped by repository:** each view state's `Lines` (`IListViewState.Lines`) is `TableLines.Grouped` over the visible rows, a `TableLine<TRow>.Heading` (carrying its group's first row) before each run of one group key; Config's are `TableLines.Ungrouped`.
- **A heading is never a row:** `ListSelection`'s selection, filter, title and counts never see one, and a filter that empties a group leaves no heading.
- **Sessions' order:** by repository name ignoring case, then the main checkout's key, then the folder groups (`Outside`, then `FolderGone` of the same path) by path, each group in `Sessions.OrderedByAttention` order (stable).
- **Worktrees' order:** `RepositoryName` ignoring case, then `Repository.Key`, `Placement`, `FolderName`.
- **Heading words:** `IScreenDefinition.Heading(first)`: Sessions the repository's name, the folder's label, or `Wording.FolderGone` and the label; Worktrees `RepositoryName`; Config is never asked.
- **Drawing headings:** `TableSource` leaves a heading's cells empty (so it never widens a column) and indents a row's first cell by two when any line is a heading; `ColumnSizing` draws each heading from `DrawComplete` (`HeadingLine`: bold orange `Palette.HeadingText`, elided in the middle, then a `Palette.Rule` rule), because a rule drawn in cells breaks between columns.
- **The cursor** sits on the selected row's line, and a heading has no tooltip.
- **NAME** is the session's name cut at its end (`TextFit.CutEnd`). **WORKTREE** is the folder's part after its repository (`Domain\WhereLabel.PlaceOf`: the slug of `<repository>-<slug>`, the path inside the repository, else the whole label, over `Domain\FolderLabel` through `Application\FolderLabels`, roots read per redraw from `CurrentSettings`) or a branch row's branch.
- **Fit:** each is fitted to the width the screen hands its elastic column; the repository is the heading's and a worktree's branch the pane's. Which column is elastic (NAME, WORKTREE, VALUE) or right-aligned (AGE, LAST COMMIT) is its own `ColumnFit`, never read from a header's words.

## Detail pane

- **What it shows:** under the Sessions and Worktrees tables `Tui\DetailPane` shows what the selected row does not. A definition's `Detail` is a `Tui\TableDetail<TRow>`: `Pane` over an `Application\Detail\IDetailSource<TRow>` (`SessionDetails` and `WorktreeDetails` over `DetailLoader`, each fact failing on its own as `Unavailable`), or `None` on Config.
- **`DetailReader`** (built by `DetailReader<TRow>.For`, a no-op `None` for a tab with no pane) owns the pane, its `DetailRequests`, the key last asked for and the token that drops a read when the tab is disposed; `TableBody` adds its `Views` and compares its `Current` around a load.
- **Fixed height:** the source states the most lines it takes, so the table never jumps; `DetailRequests` numbers requests and shows only the newest answer for the selected row.
- **Words and layout:** `Wording.Detail` words a `DetailContent`: a session's facts in five rows of two columns, the left two of the pane's three columns wide, then its Account line, six rows (`Wording.SessionFactRows`); a worktree's in four rows of up to three columns (`Wording.WorktreeFactRows`), then its commits or changed files. `DetailLayout` sets it out at the pane's width.
- The help's Pane section comes from `Wording.SessionsPane` and `WorktreesPane`.

## Status line

- **The console's, not a tab's:** `ConsoleScreen` draws it under the settings row from the shown tab's `ITabBody.Status` and `Running`, with rows only while `State.Message` has words or an action runs (`StatusLayout.Lines` wraps it onto at most three, and the strip, counts and title move down by that many).
- **Mark and colours** come from `Wording.ToneOf` (a `StatusTone` per message and outcome case, each chosen in `StatusTonesShould`), `StatusMark` and `StatusWordsTone`; the mark shares its cell with the spinner and hides while it spins.
- **Dismissing:** the line starts at the settings row's indent; a blue ✕ after the last word (`StatusLayout.Closes`) and a success's ten seconds (`ClearsBySelf`, `SuccessShownFor`) both reduce `MessageDismissed`, which clears it only while it is still that same `StatusMessage` instance.
- **A launch's progress** is one holder, `Application\Launch\LaunchProgress` (an `IProgress<LaunchStep>` over a `SharedSnapshot<LaunchStep>`, `LaunchStep.None` until reported), built once in `LaunchParts`. `LaunchService` reports each of its `LaunchSteps` as it begins; `StartSessionHandler` reports `None` as it starts and `OpeningTab` last.
- **Showing it:** `ShowStatus` passes every message through `LaunchProgress.Shown`, which answers `StatusMessage.Launching` only for a `Working(StartSession)` whose plan has the step's name, comparing the words too (the `Working` instance stays while its words change). The ticker (`RefreshWhatIsDue`) redraws when `Latest` changes, so a launch never waits on the UI thread.
- **Prefetch:** the wizard's `WizardEffect.Prefetch` (a confirmed repository, a new worktree from origin's default branch, Fetch or Pull, once per repository a wizard opens) makes `LaunchWizardDialog` call `Application\Launch\BaseFetches.Begin`, which `BaseUpdater` joins.
- **A newly found release** is the one message no reducer raises: `UpdateCheck.TakeAnnouncement` answers `StatusMessage.NewerReleaseFound` once, and `FrameChanged` hands it to the shown tab's `ITabBody.ShowMessage` before drawing, so it shows on the first redraw after the check.
- **Clearing:** the detail pane closes the tab, and the reducers clear a message when the selection moves or the filter changes. No tab has a standing message: a Config row's description (`Wording.ConfigDescription`) is only its hover tooltip (`IScreenDefinition.RowTip`), and Sessions' one row tip (`Wording.SessionRowTip`) is for a session whose folder has gone (`SessionRowEnricher`, through `IFolders`).

## Dialogs

- **The wizard is a reducer:** `WizardView.Reduce` over `WizardState`, opened on a `WizardOrigin` (`Anywhere` for `n`, or `Fixed` on a worktrees row's folder, `WizardOrigin.In`), whose `Chosen` answers a `RepositoryPick`; it returns a `WizardEffect` to launch, close or do nothing (`WizardEffect.None` and `WizardProblem.None` stand where a null would).
- **Its binder:** `WizardControls` holds the layout, each control event becomes a `WizardInput`, and the state is pushed back to the controls under a `binding` guard so the events that pushing raises are ignored.
- **Its size** comes from `WizardFit`: 80 wide and the screen's height less up to two rows for the overview (80 x 30 for one list row), 84 x 23 for a fixed folder; smaller, `DialogEffects.FitOrRefuse` shows `WizardTooSmall`.
- **Its form,** top to bottom under the repository list: name, Where, the worktree base, Before branching (a three-option `OptionSelector` on the saved setting, changing only that session; its caption and last option must show at 120 x 36, which the smoke script checks) and the opening prompt. `WizardControls.FormRows` is 21 and the `AnchorEnd` offsets above the preview count from it.
- A fixed folder leaves out Where, the base and Before branching. Enter moves Base to Before branching to Prompt (`WizardView.Next`).
- **A setting's dialog** is a reducer the same way: `SettingsView.Reduce` over `SettingsState`, scoped by a `SettingsScope` to the one value its Config row edits, held inside the saved settings so `Settings.Validate` checks it, bound by `SettingsDialog` and `SettingsControls`.
- **Roots:** every repository root row opens `SettingsScope.Roots`, all the roots one per line, where Browse (F3) runs Terminal.Gui's folder picker and `SettingsInput.FoldersChosen` adds each as a line for F5 to check; the worktree root's Browse replaces the field. `SettingsState.Browses` says which dialogs have it; `SettingsDialog` shows the button only while it is not `No`.
- **Open folder with** browses (F3, `SettingsBrowse.OneFile`) only while Custom is chosen, with `OpenDialog` in file mode showing folders and `.exe` files (`SettingsInput.FileChosen`).
- **Missing editors:** as it opens it asks `Application\EditorPresets` off the UI thread which of VS Code, Visual Studio and Rider cannot be found, applies the answer as `SettingsInput.PresetsLocated` through `app.Invoke` only while open, and retitles the choices from `Wording.Describe(EditorPreset, PresetSearch)` (`Rider (not found)`).
