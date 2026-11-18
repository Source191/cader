# Tasks

## 1. Launch rules for a folder choice (Application\Launch)

- [ ] 1.1 Red: `LaunchPlanShould.StartAFolderChoiceInTheFolderWhateverWhereSays` builds a New worktree request for the folder choice `C:\repos` and asserts a planned `LaunchPlace.CurrentBranch` in `C:\repos` (replacing `RefuseAWorktreeForTheNoRepositoryChoice`). Green: `LaunchPlanBuilder` plans a choice that is not a repository on its current branch; remove `LaunchProblem.WorktreeNeedsRepository` and its words in `Wording`. Verify the Core suite passes.
- [ ] 1.2 Red: `LaunchPreviewShould.SayAFolderChoiceIsNotARepositoryWhateverTheMode` asserts both modes answer `Wording.PreviewFolder("C:\repos")`, and `WordingShould` asserts its sentence (replacing `WarnThatAWorktreeNeedsARepository`). Green: `LaunchPreview` checks the choice before the mode; `PreviewNeedsRepository` becomes `PreviewFolder`. Verify the Core suite passes.

## 2. Grouped choices (Application\Launch)

- [ ] 2.1 Red: `RepositoryCatalogueShould.ListTheRootsAsFolderChoices` asserts `Choices(recent)` answers Recent, Folders (each existing root by its full path, not a repository) and Repositories, and `OfferNoRootAsAChoice` goes; the other facts read the groups and filter through `RepositoryChoices.Matching`. Green: `RepositoryChoices` record and the catalogue without its filter parameter; `WizardState.Open` and `WizardFilterWatch` follow. Verify the Core suite passes.

## 3. Rows and selection (Application\Views)

- [ ] 3.1 Red: `WizardViewShould.OpenWithRecentRepositoriesFirstAndTheFirstChoiceSelected` asserts the rows are the Recent heading, Northwind.Templates, the Folders heading, the root, the Repositories heading and the other two, with Northwind.Templates selected; `NarrowChoicesAsTheFilterChanges` asserts a group the filter empties loses its heading. Green: `WizardRow`, `ChoiceGroup`, `WizardState.Rows` (and `Choices` as the choice rows), `SelectedIndex` over the rows, first choice on open and filter. Verify the Core suite passes.
- [ ] 3.2 Red: `WizardViewShould.StepOverAHeading` (Down from the last Recent choice lands on the root; Page Up from the first choice stays; `ChoiceSelected` on the Repositories heading from a Recent choice lands on the first repository; on the first heading from below lands on the first choice). Green: one landing rule for `ListMoved` and `ChoiceSelected`. Verify the Core suite passes, `MoveAndPageTheListWithinBounds` and `SelectTheFirstChoiceAndRefuseOneBeyondTheLast` included.
- [ ] 3.3 Red: `WizardViewShould.StartAFolderChoiceInTheFolderKeepingWhere` selects the root with Where on New worktree, starts, and asserts a launch in the root on its current branch with `Mode` still New worktree; then selecting a repository previews a worktree. Green: nothing beyond 1.1 should be needed. Verify the fact passes.
- [ ] 3.4 Red: `WordingShould` asserts `WizardRow` text (a heading's words, a choice indented by two spaces) and `WizardChosenCaption` reads Folder for a folder choice. Green: the words in `Wording`. Verify the Core suite passes.

## 4. The dialog (Tui) and the documents

- [ ] 4.1 `LaunchWizardDialog.Push` lists `state.Rows` through `Wording.WizardRow` and the caption follows `WizardChosenCaption`. Verify `dotnet build -c Debug` and `dotnet test -c Debug --no-build` pass.
- [ ] 4.2 `docs\user-guide.md` (the wizard's grouped list and folder choices), a `CHANGELOG.md` entry under Unreleased, Added, and the version raised in `src\Source191.Cader\Source191.Cader.csproj`. Verify `dotnet format --verify-no-changes` and `openspec validate wizard-lists-root-folders` pass.

## Workflow follow-up

- Archive the change in the same pull request, so `openspec\specs` describes the code on `main`.
