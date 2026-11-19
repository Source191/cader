# Tasks

**Implement with:** Claude Opus 5.5 (`claude-opus-5-5`) at high effort, because the change crosses Application and the Tui and a heading's drawing needs judgement against a real run (design.md, Implementation model and effort).

Each task is one red, green, refactor cycle with a commit per step; the test named first is the one to make fail.

## 1. Grouped lines (Application)

- [x] 1.1 `TableLinesShould.PutAHeadingBeforeEachRunOfOneKey`, then add `Application\Views\TableLine<TRow>` (nested `Heading(TRow First)` and `Row(TRow Item)`) and `TableLines.Grouped`; verify the fact passes and `SealedTypesShould`, `NullFreeModelsShould` and `ClosedHierarchiesShould` stay green
- [x] 1.2 `TableLinesShould.GiveNoHeadingForNoRows` and `KeepOneHeadingForOneGroup`; verify they pass
- [x] 1.3 `ListSelectionShould` facts that a grouped list's `Move`, `Load`, first load and filter never select a heading and that a filter leaves no empty group's heading in `Lines`; add `Lines` to `IListViewState<TRow>` and each view state (Config one `Row` per row); verify `ListSelectionShould` and `ReducersShould` pass

## 2. Group order (Domain and Application)

- [x] 2.1 `SessionsViewShould.GroupRowsByRepositoryAToZIgnoringCase` and `PutFolderGroupsAfterRepositories`, then order `SessionsViewState` by group key first; verify they pass
- [x] 2.2 `SessionsViewShould.KeepAttentionOrderWithinAGroup` (needs input first, then working, idle, completed, failed, stopped, newest first within a state), reusing `Sessions.OrderedByAttention`; verify the old whole-table attention facts are rewritten to the grouped order and pass
- [x] 2.3 `SessionsViewShould.KeepFolderGoneApartFromTheLiveFolder` and `TellTwoRepositoriesOfOneNameApartByCheckout`; verify they pass
- [x] 2.4 `WorktreesViewShould.GroupRowsByRepositoryNameAcrossRoots` (Northwind.Alpha under D:\work before Northwind.Zeta under C:\Users\me\source\repos) and `KeepFoldersBeforeBranchesWithinAGroup`; change `WorktreesViewState`'s order; verify they pass
- [x] 2.5 `SessionsViewShould` and `WorktreesViewShould` facts that each state's `Lines` puts a heading before each group; verify they pass

## 3. Wording

- [x] 3.1 Remove `Wording.RepositoryColumn` and its `ColumnHelp` line from `SessionsColumns`; rewrite NAME's help to say what the heading names (repository, folder label, folder gone); verify `WordingOfScreensShould`, `KeyHelpParityShould` and the help-text facts pass and `SpellBritish` stays green
- [x] 3.2 Rewrite WORKTREE's help to say the heading names the repository; verify its `Wording*Should` fact passes

## 4. Screen (Tui)

- [x] 4.1 `SessionsScreenDefinitionShould.HaveNoRepositoryColumn` and `MakeNameTheElasticColumn`; drop REPOSITORY and make NAME `ColumnFit.Elastic()`; verify they pass and the one-elastic-column fact holds
- [x] 4.2 `SessionsScreenDefinitionShould.NameAHeadingByRepositoryFolderOrFolderGone`; add `IScreenDefinition.Heading(TRow first)` and implement it on Sessions (moving today's REPOSITORY cell words into it); verify it passes
- [x] 4.3 `WorktreesScreenDefinitionShould.NameAHeadingByRepository` and `ShowNoRepositoryInTheWorktreeCell` (folder slug, inner path, label, or a branch or origin row's branch); verify they pass
- [x] 4.4 Make `TableSource`, `ColumnSizing` (`Source`, `WidestCell`, `CellColour`, `RowColour`), `TableToolTips` and `TableBody.Redraw` work over `TableLine<TRow>`: a heading bold orange with its text elided in the middle in the first column and a faint rule in the rest, never widening a column and showing no tooltip; a row's first cell indented by two; the cursor on the selected row's line; verify `ColumnFitsShould`, `PaletteShould` and the Tui facts pass, and add facts for any rule the change makes testable there
- [x] 4.5 Update `tools\ReadmeScene`'s `SceneHost` for any changed constructor; verify `dotnet build -c Debug` passes with warnings as errors

## 5. Documents and release

- [x] 5.1 Describe the grouping in `docs\user-guide.md` (Sessions and Worktrees sections: headings, group order, attention within a group, the counts) and the README where it describes the tables; verify by reading them against the delta specs
- [x] 5.2 Remake `assets\readme\sessions.png` and `new-session.gif` with `tools\New-ReadmeScreenshot.ps1`; verify by eye that headings read as one line, nothing widens, and a 120-column terminal fits every column
- [x] 5.3 Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` (minor: the Sessions order changes) and add the changelog entry; verify `Assert-VersionBumped.ps1` passes

## 6. Integration checks

- [x] 6.1 Run `dotnet build -c Debug`, `dotnet test -c Debug --no-build` and `dotnet format --verify-no-changes`; verify all pass
- [ ] 6.2 Run the installed build with `tools\install-local.ps1` and `dotnet run -- smoke <exe> <worktree-folder>` from `tools\HeadlessTerminal`; verify exit 0, then check both tabs by hand: Down steps over headings, a filter hides empty groups, `>` scrolls and `<` brings the heading text back
- [x] 6.3 Run `openspec validate group-rows-by-repository --strict`; verify it reports the change valid

## Workflow follow-up

- Archive the change in the same pull request that completes it (`/opsx:archive`), so `openspec\specs` describes the code on `main`.
