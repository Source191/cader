# Proposal

## Why

The new-session wizard offers only repositories, so a session that works across several repositories, or in a folder that is not one, has to be started by leaving the console and running `cader here` from that folder. The roots used to be listed at the bottom as "(no repository)" choices; they were dropped because nothing said why they were there or why the list was not in order.

## What Changes

- The wizard lists each existing repository root as a folder choice, shown by its full path.
- The list is grouped under three headings, in this order: Recent (the recently used repositories, most recent first), Folders (no repository) (the roots) and Repositories (the rest, alphabetically). A group with nothing in it, by the filter or otherwise, is left out with its heading.
- A heading is never selected: the selection steps over it with the arrow and page keys, a click on one moves to the nearest choice, and the wizard opens, and the filter reselects, on the first choice, so `n` still lands on the most recent repository.
- A folder choice starts the session in that folder whatever Where says: Where, the worktree base and Before branching keep their values (so moving back to a repository still offers a new worktree by default) but do not apply, nothing is fetched or pulled, and the preview says the folder is not a repository and Where does not apply. The list's caption reads "Repository or folder".
- The wizard no longer refuses a folder that is not a repository with "A worktree needs a repository"; that problem goes, as nothing can raise it.

## Out of scope

- Folders other than the roots (any folder below a root that is not a repository); `cader here` still covers those.
- Colouring or styling the headings beyond their text.
- Keeping a root out of the recent repositories record: a session started in a root is recorded as `cader here` already records any folder, and the Recent group shows repositories only.
- Multiple Claude accounts, which is a later change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `session-launch`: the picker offers the roots as folder choices under grouped headings, and a folder choice starts in the folder whatever Where says, instead of being refused.

## Impact

- `Application\Launch\RepositoryCatalogue` (grouped choices with the roots), `RepositoryChoices` (new), `LaunchPlanBuilder` and `LaunchPreview` (a folder choice is planned on its current branch), `LaunchProblem` (the worktree-needs-repository case removed).
- `Application\Views\WizardState`, `WizardView`, `WizardRow` (new) for headings and selection; `Wording` for the headings, the folder preview; `Tui\WizardControls` for the list's caption.
- `Tui\LaunchWizardDialog` renders the rows.
- `docs\user-guide.md`, `CHANGELOG.md`, the version in `src\Source191.Cader\Source191.Cader.csproj`.
