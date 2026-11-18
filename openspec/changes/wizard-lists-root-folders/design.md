# Design

## Context

`RepositoryCatalogue.Choices` answers one flat list (recent first, then the rest), `WizardState.Choices` filters it, and `SelectedIndex` indexes that filtered list, which `LaunchWizardDialog` binds straight to its `ListView` (`SelectedItem = state.SelectedIndex`; the list's own keys and clicks come back as `WizardInput.ChoiceSelected(index)`, arrows in the filter as `ListMoved(delta)`). A `RepositoryChoice` with `IsRepository: false` is already understood by `LaunchPreview` and `LaunchPlanBuilder` (`cader here` makes one), which refuse a new worktree for it.

## Goals / Non-Goals

**Goals:** headings that are rows of the list the `ListView` shows, so the dialog stays a plain binder; one place that decides which row the selection rests on.

**Non-Goals:** a styled heading (the `ListView` shows strings); changing what `cader here` plans or records.

## Decisions

- **Grouped choices from the catalogue.** `RepositoryCatalogue.Choices(recent)` answers a `RepositoryChoices(Recent, Folders, Repositories)` record, the filter moving to the wizard (its one caller passed an empty filter). A folder choice is `RepositoryChoice(root.Value, root, IsRepository: false)`, so the full path is its name, which keeps two roots with the same leaf apart and lets the filter match the path. Alternative: a group enum on `RepositoryChoice`; rejected because `cader here` and the fixed origin build choices with no group.
- **Rows in the state, indexed by the selection.** `WizardRow` is a two-case record (`Heading(ChoiceGroup)`, `Choice(RepositoryChoice)`); `WizardState.Rows` lays out each non-empty group after the filter as its heading and its choices, and `SelectedIndex` indexes `Rows`, so the binder still sets `SelectedItem` directly and maps each row to `Wording.WizardRow(row)`. `Chosen` answers only a `Choice` row. Alternative: keep indexing choices and translate in the binder; rejected because the binder would then hold a rule.
- **One landing rule.** `ListMoved` and `ChoiceSelected` both go through one function: take the target index (clamped for a move), and if it is a heading take the nearest choice in the direction of travel (the sign of the delta, or of target minus current for a click), else the nearest the other way. Opening and filtering select the first `Choice` row. The rows of every group lead with a heading, so a list with a choice always has one at index 1.
- **A folder choice plans as the current branch.** `LaunchPlanBuilder` treats `!Repository.IsRepository` as `LaunchMode.CurrentBranch`, so `LaunchProblem.WorktreeNeedsRepository` can no longer be raised and is removed with its words; `LaunchPreview` answers `Wording.PreviewFolder` for such a choice before looking at the mode. `cader here` already plans on the current branch, so its plans are unchanged. `LaunchService` fetches only for `LaunchPlace.NewWorktree`, so nothing is fetched for a folder choice. Alternative: switch Where in the state when a folder is selected; rejected by the owner (Where would stay switched after moving back to a repository).
- **The caption follows the choice.** `Wording.WizardChosenCaption` reads Folder while a folder choice is chosen, Repository for a repository and Directory for a fixed folder.

## Risks / Trade-offs

- [A session in a root is recorded as a recent repository, as `cader here` records any folder] → the Recent group shows repositories only, so the root never appears there; it only takes one of the ten recent places until newer launches push it out. Keeping it out would need the launch to know where the plan came from, which is more than it is worth.
- [A page move lands on a heading at the top] → the landing rule falls back to the nearest choice the other way, so Page Up at the top keeps the first choice.
