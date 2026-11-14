# Tasks

## 1. Domain rules

- [x] 1.1 **Red:** `RemoteBranchShould` facts.
  - `TryParse("origin/main")` gives remote `origin` and branch `main`.
  - `origin/release/2026` gives branch `release/2026`.
  - Blank text, null, `main` with no remote and `origin/` answer false.
  - `From` throws for text that does not parse.

  **Green:** `Domain\RemoteBranch` as an `ITextValue<RemoteBranch>` with `From` and `TryParse`. **Verify:** the facts pass, along with `TextValuesShould`, `NullFreeModelsShould` and `SealedTypesShould`.
- [x] 1.2 **Red:** `SettingsShould` facts that `Resolve` with no before-branching argument gives `Fetch`, that `Validate` keeps the `SettingsText`'s value, that `ToText` writes it back, and that `Equals` tells two values apart. **Green:** `Domain\BeforeBranching { Fetch, Pull, None }`, `Settings.BeforeBranchingWhenUnset`, and the member in `Settings` (constructor, `Resolve`, `Validated`, `ToText`, `Equals`, `WithUpdateCheckToggled`) and `SettingsText`. **Verify:** `SettingsShould` passes, and every switch over the new enum names all three members (the build fails with CS8509 otherwise).

## 2. Settings file

- [x] 2.1 **Red:** `SettingsFileShould` facts.
  - A config with no `beforeBranching` reads `Fetch`; `Pull` reads `Pull`.
  - A number (`7`) and an undefined name read `Fetch` and leave the roots as given.
  - `ToConfig` names the current value.

  **Green:** `UserConfig.BeforeBranching` (nullable, as the other members are, with its `Equals`, `GetHashCode` and the "names all seven" comment), and `SettingsFile.From`/`ToConfig` through `DefinedOr`. Add a `NullFreeModelsShould` `Kept` entry for `UserConfig.BeforeBranching` ("config.json may leave the property out"). **Verify:** `SettingsFileShould` and `NullFreeModelsShould` pass.
- [x] 2.2 **Red:** `JsonFileStoreShould` facts (temporary folder).
  - `beforeBranching` of `"Rebase"` or `7` reads as a corrupt file does, `UserConfig.Missing`, without throwing.
  - A written config holds `"beforeBranching": "Fetch"`.

  **Green:** nothing is expected, since `JsonStringEnumConverter(allowIntegerValues: false)` already does this; the facts pin it. **Verify:** the facts pass.

## 3. Process and git adapters

- [x] 3.1 **Spike (owner, recorded, before any port change):** on a developer machine with git 2.55 and Git Credential Manager, run `git fetch` against an HTTPS remote that has no stored credential, with `GIT_TERMINAL_PROMPT=0` and `GCM_INTERACTIVE=false` set and output captured. Check that it fails within a few seconds, that no window or prompt appears, and that its stderr is readable. Record the fact with its date and versions in `.claude\rules\windows.md`. If it prompts or hangs, revisit design decision 2 before 3.2. **Verify:** the rule line is present.
- [x] 3.2 **Red:** `ProcessRunnerIntegrationShould` facts.
  - A captured child given `Variables` of `GIT_TERMINAL_PROMPT=0` sees that value, even when the test sets `GIT_TERMINAL_PROMPT` in its own environment, so the variable is dropped first and then set.
  - A request without `Variables` passes nothing extra.

  **Green:** an init-only `ProcessRequest.Variables`, empty by default, applied in `ProcessRunner.StartInfo` after the `InheritedEnvironment` drop. **Verify:** `ProcessRunnerIntegrationShould`, `InheritedEnvironmentShould`, `FakeProcessRunnerShould` and `NullFreeModelsShould` pass.
- [x] 3.3 **Red:** `GitCliShould` facts over `FakeProcessRunner`.
  - `FetchBranchAsync` runs `fetch origin +refs/heads/main:refs/remotes/origin/main` in the repository, with `GIT_TERMINAL_PROMPT=0` and `GCM_INTERACTIVE=false` in its `Variables`.
  - A non-zero exit throws `GitException` carrying git's error.

  **Green:** the `IGit` member, its convention line in the doc comment, and `GitCli`. **Verify:** `GitCliShould` passes.
- [x] 3.4 **Red:** `GitCliShould` facts.
  - `BranchExistsAsync` runs `rev-parse --verify --quiet refs/heads/main` and answers true on exit 0 and false otherwise.
  - `FastForwardCheckoutAsync` runs `merge --ff-only refs/remotes/origin/main` in the checkout.
  - `FastForwardBranchAsync` runs `fetch . refs/remotes/origin/main:refs/heads/main`.
  - Both fast-forwards throw `GitException` on a non-zero exit.

  **Green:** the three `IGit` members and `GitCli`. Update `PortParametersShould`'s `Kept` reason for `IGit`, which counts its methods. **Verify:** `GitCliShould` and `PortParametersShould` pass.
- [x] 3.5 **Red:** `GitCliIntegrationShould` facts against a temporary bare repository as origin and a clone (real git, no network). Push a commit to origin from a second clone, then:
  - `FetchBranchAsync` moves `origin/main`.
  - `FastForwardCheckoutAsync` moves the checked-out `main` and its files, and leaves an uncommitted change to an untouched file in place.
  - It throws when an uncommitted change is in the way, and when local `main` has its own commit.
  - `FastForwardBranchAsync` moves a `main` that is not checked out, and throws for a non-fast-forward.
  - `BranchExistsAsync` is false for a missing branch.

  **Green:** fix whatever the real git shows. **Verify:** the facts pass, and `dotnet test test\Source191.Cader.Infrastructure.Tests -c Debug --no-build` is green.

## 4. Base update rule

- [x] 4.1 **Red:** `FastForwardPlaceShould` facts over `GitWorktreeEntry` lists.
  - The main entry on `main` gives `MainCheckout`.
  - A linked entry on `main` gives `ElsewhereAt` with its folder.
  - No entry on `main`, including a detached head, gives `NotCheckedOut`.

  **Green:** `Application\Launch\FastForwardPlace`. **Verify:** the facts pass, along with `NullFreeModelsShould`.
- [x] 4.2 **Red:** `BaseUpdaterShould` facts (a real `GitCli` over `FakeProcessRunner`).
  - `None` runs no git.
  - `Fetch` runs the fetch and answers `Current`.
  - A failed fetch answers `NotFetched` with git's words.
  - A `ProcessTimedOutException` answers `NotFetched` with the timeout's words.

  **Green:** `Application\Launch\BaseUpdater` and the `BaseUpdate` hierarchy. **Verify:** the facts pass, along with `NullFreeModelsShould` and `SealedTypesShould`.
- [x] 4.3 **Red:** `BaseUpdaterShould` facts for `Pull`.
  - After the fetch, `MainCheckout` runs `merge --ff-only` in the repository and `NotCheckedOut` runs `fetch .`; both answer `Current`.
  - `ElsewhereAt` runs neither and answers `NotFastForwarded(main, CheckedOutElsewhere(folder))`.
  - A missing local branch runs neither and answers `Current`.
  - A refused fast-forward answers `NotFastForwarded(main, Refused(reason))`.
  - A failed fetch runs no fast-forward.

  **Green:** the `Pull` path in `BaseUpdater`. **Verify:** the facts pass.

## 5. Launch

- [x] 5.1 **Red:** `LaunchPlanShould` facts that `LaunchPlanBuilder.Build` carries a request's `BeforeBranching` into the plan, and `HereCommandShould` that `cader here` plans `None`. **Green:** the member on `LaunchRequest` and `LaunchPlan`, copied by `LaunchPlanBuilder`; `HereCommand` passes `None`. **Verify:** both test classes pass.
- [x] 5.2 **Red:** `LaunchServiceShould` facts.
  - A new-worktree plan with the base `RemoteDefault` and `Fetch` fetches before `worktree add` and branches from `origin/main`.
  - With `None`, it runs no fetch.
  - With no `origin/HEAD`, it runs no fetch and branches from `HEAD`.
  - The `Head` base and a current-branch plan run no fetch.
  - A failed fetch still creates the worktree, starts the session, and answers `Started` whose `Update` is `NotFetched`.

  **Green:** `LaunchService` takes a `BaseUpdater`, calls it in `CreateWorktreeAsync`, and `LaunchOutcome.Started` gains `Update`. **Verify:** `LaunchServiceShould` and `HereCommandRunnerShould` pass.
- [x] 5.3 **Red:** `StartSessionHandlerShould` facts that `StartedAndOpened` and `NotListedYet` carry the launch's `Update`. **Green:** the member on both outcome cases and the handler's pass-through. **Verify:** `StartSessionHandlerShould`, `EffectDispatcherShould` and `HandlersShould` pass.
- [x] 5.4 **Red:** `WordingOfOutcomesShould` facts.
  - A started outcome whose update is `Current` reads exactly as today.
  - `NotFetched` appends that origin could not be fetched, with the reason.
  - `NotFastForwarded` with `CheckedOutElsewhere` names the branch and the folder.
  - `Refused` names the branch and the reason.

  **Red:** `StatusTonesShould` lines that those three cases are `Notice`, and that a failed tab stays `Failure`. **Green:** `WordingOfOutcomes` and `Wording.ToneOf`. **Verify:** both test classes and `SentenceLiteralsShould` pass.

## 6. Config tab

- [x] 6.1 **Red:** `ConfigRowsShould` fact that `ConfigRow.BeforeBranching` comes directly after `DefaultBase` and before `DefaultPlace`, its key stable. **Red:** `ConfigViewShould` facts that Enter on it raises the dialog effect for `SettingsScope.BeforeBranching`. **Green:** the row, `ConfigRows.For`, the scope, and `SettingsField.BeforeBranching`. **Verify:** the three test classes pass.
- [x] 6.2 **Red:** `SettingsViewShould` facts that the Before branching dialog opens on the saved value, that a choice then F5 saves it through `Settings.Validate`, and that Esc leaves the settings unchanged. **Green:** `SettingsState`/`SettingsView` for the new field. **Verify:** `SettingsViewShould` and `SaveSettingsHandlerShould` pass.
- [x] 6.3 **Red:** `WordingOfConfigShould` facts.
  - The row is labelled Before branching and its values read Fetch origin, Pull default branch and Nothing.
  - Its dialog title, and a tooltip saying it is what happens before a new worktree branches from origin, within the 110-character description limit.
  - `Describe(WorktreeBase.RemoteDefault)` reads "origin's default branch".
  - The Default base tooltip names Before branching and contains neither "no fetch" nor "never fetches".

  **Green:** `WordingOfConfig`. **Verify:** `WordingOfConfigShould`, `SourceConventionsShould.SpellBritish` and `KeyHelpParityShould` pass.

  **Done in part 1:** `Describe(WorktreeBase.RemoteDefault)` reads "origin's default branch", and the Default base tooltip reads "New worktrees branch from origin's default branch, fetched first by default, or from HEAD." (`WordingOfConfigShould.SayTheDefaultBaseIsFetchedFirstByDefault`); with the row in place, make it name Before branching.
- [x] 6.4 **Proved by the smoke check in CI (pull request 273), not by eye:** the new dialog opens from its row and shows its caption and last choice; its change, save and config.json by eye are the owners check after merge. **Green (Tui binder, untested):** the Before branching `OptionSelector` in `SettingsControls`, laid out as Place's is. **Verify:** `dotnet build -c Debug`, then in a local install (`tools\install-local.ps1`) open the dialog from the row, change it, save, and see the row and `config.json` follow.

## 7. Wizard

- [x] 7.1 **Red:** `WizardViewShould` facts.
  - `Open` takes `BeforeBranching` from the settings.
  - `BeforeBranchingChanged` changes it.
  - `NextField` from `Base` goes to `BeforeBranching` and from there to `Prompt`.
  - The `LaunchRequest` on start carries the wizard's choice.
  - Starting with a choice other than the setting leaves the settings unchanged.

  **Green:** `WizardState`, `WizardInput.BeforeBranchingChanged`, `WizardField.BeforeBranching` and `WizardView`. **Verify:** `WizardViewShould` and `ReducersShould` pass.

  **Done in part 1:** `WizardState.BeforeBranching`, taken from the settings on `Open` and put in the `LaunchRequest` on start (`WizardViewShould.StartWithTheSavedBeforeBranching`), so `config.json` applies before the field exists.
- [x] 7.2 **Proved by the smoke check, which now runs at 120 x 36 (pull request 273):** the wizard shows its Before branching caption and last choice; the list has 5 rows there by the layouts arithmetic (the 28 content rows less 2 above and 21 for the form). **Green (Tui binder, untested):**
  - In `WizardControls`, add the captioned three-option selector between the base and the preview, raise `FormRows` to 21, and move the `AnchorEnd` offsets.
  - Add its field to `Fields` and to `WhenEnterInSelectors`.
  - In `LaunchWizardDialog`, bind its `ValueChanged`.
  - The fixed-folder layout leaves it out.

  **Verify:** `dotnet build -c Debug`. Then, in the headless terminal at 120 x 36, check that the wizard shows the list with at least four rows and every caption. If it does not, switch to a horizontal selector, as design decision 5 allows.
- [x] 7.3 **Done:** SmokeScript checks the wizard and the Config dialog; CI ran it on x64 and Arm. Update the smoke script if its wizard steps count fields or rows. **Verify:** `dotnet run -- smoke <exe> <worktree-folder>` from `tools\HeadlessTerminal` exits 0 against a local install.

## 8. Documentation and release

- [x] 8.1 **`docs\user-guide.md`:**
  - The Before branching setting and wizard choice, and what each value does.
  - That Pull fast-forwards only, and may change the main checkout's files under a running session.
  - The warn-and-continue notice, the SSH passphrase limit, and `git remote set-head origin --auto` for a renamed default branch.

  **`.claude\rules\console-screen.md`:** the wizard's new field in the Dialogs section. **Verify:** the guide's settings table and the wizard section name all three values.
- [ ] 8.2 **Left for the owner (part 2):** `tools\New-ReadmeScreenshot.ps1` opens a real Windows Terminal window on the desktop and needs ffmpeg, so it cannot run unattended; `tools\ReadmeScene\new-session.keys` now has the extra Enter for the new field, so the owner's remake is correct. Remake `assets\readme\new-session.gif` (and `sessions.png` if the wizard shows in it) with `tools\New-ReadmeScreenshot.ps1`, after adding the field to `scene.json` if the scene needs it. **Verify:** the GIF shows Before branching, and `tools\ReadmeScene.Tests` pass.
- [x] 8.3 Raise `Version` (minor) in `src\Source191.Cader\Source191.Cader.csproj` and add `CHANGELOG.md` lines, each ending with the pull request number:
  - **Added:** the Before branching setting and wizard choice.
  - **Changed:** new worktrees now fetch origin's default branch first; set Before branching to Nothing for the old behaviour.

  **Verify:** `Assert-VersionBumped.ps1` and `Assert-ChangelogUpdated.ps1` against `main`.

  **Done in part 1:** a Changed line for the fetch, naming `beforeBranching` in `config.json`; reword it for the row and the wizard choice. Part 1 also made `tools\ReadmeScene`'s `SceneProcessRunner` answer a fetch from a remote itself, so remaking the GIF never reaches the network.

## 9. Integration checks

- [x] 9.1 Run `dotnet build -c Debug`, then `dotnet test -c Debug --no-build` at the solution, then `dotnet format --verify-no-changes`, and `openspec validate fetch-before-branching --strict`. **Verify:** each exits 0.
- [ ] 9.2 **Manual (owner):** in a local install, against a scratch repository whose origin is a local bare repository:
  - With Fetch, push a commit to origin from elsewhere, start a new worktree session, and check that it starts at that commit.
  - With Pull and the main checkout on `main`, check that `main` moves too.
  - Make origin unreachable (rename the bare folder), start again, and see the session start with the notice.

  **Verify:** by eye and with `git log -1` in each worktree.

## Workflow follow-up

- Archive the change in the pull request that completes it, so `openspec\specs` describes `main`.
- Whichever of this change and `diagnostic-log-setting` merges second rebases its `Settings`, `UserConfig`, `SettingsFile` and `ConfigRows` edits onto the other.

## Implementation plan

See `openspec\implementation-plan.md` for the whole sequence.

- **PR:** H (part 1), then I (part 2)
- **Step:** 3 for H, 4 for I
- **Depends on:** F and C for both parts; I also needs H
- **Parallel with:** H with G; I with nothing
- **Model:** H opus, I sonnet
- **Effort:** H xhigh, I high
- **Why:** H is git behaviour that can hang on a credential prompt or fast-forward over local work, tested against real git with a bare origin; I is the Config tab row, the wizard field and the docs, all in established patterns.
- **Owner gates:** blocking for H, 3.1 (the credential-prompt spike, recorded with its date, before any port change). After merge, 9.2 in a scratch repository.
- **H (part 1):** tasks 1 to 5, plus the words that say Cader never fetches (the Default base row, tooltip and dialog, and the wizard's base choice), so no release says something untrue. With no row or wizard field yet, the default `Fetch` applies and `config.json` can set it. Raise `Version` one step; do not archive.
- **I (part 2):** tasks 6 to 9. The Config tab row, the wizard choice, the user guide, the remade `new-session.gif`, the minor `Version` raise and the `CHANGELOG.md` lines. Archive the change here.
- **Rebase:** F also adds a property to `Settings`, `UserConfig` and `SettingsFile` and a row to `ConfigRows`; take F's shape and rebase this change's constructor and `Equals` edits onto it.
