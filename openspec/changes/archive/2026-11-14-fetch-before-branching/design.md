# Design

## Context

- `LaunchService.CreateWorktreeAsync` reads `origin/HEAD` with `IGit.DefaultRemoteBranchAsync` (a local `symbolic-ref`, no network), falls back to `HEAD` when there is none, then runs `git worktree add <path> -b <slug> <base>`. Nothing fetches.
- Every git call goes through `GitCli` over `IProcessRunner`. `ProcessRunner` captures output with `CreateNoWindow`, drops the variables `InheritedEnvironment` names (including `GIT_TERMINAL_PROMPT`, so tabs can still prompt), and ends any child after the shared 60-second capture limit with `ProcessTimedOutException`. `ProcessRequest` has no way to set a variable for one child.
- `IGit`'s convention: commands throw `GitException`, nullable queries answer null, `IsMerged` answers false. `LaunchService` catches `GitException`, `IOException` and `UnauthorizedAccessException` around the worktree step and fails the launch. `ProcessTimedOutException` derives from `Exception` and is not caught.
- `LaunchOutcome.Started` and `StartSessionOutcome.StartedAndOpened`/`NotListedYet` can only say "started". `StatusTone.Notice` is the existing tone for "part of what was asked".
- Default base and Default place are three-part settings (Domain enum, `SettingsScope` dialog with an `OptionSelector`, `ConfigRow`). The wizard's base is an `OptionSelector` that is always shown in the `n` wizard and ignored for Current branch.
- `LaunchServiceShould` is sociable: a real `GitCli` over `FakeProcessRunner`, answering by arguments. There is no `IGit` fake.

## Goals / Non-Goals

**Goals:**
- A fetch that can never prompt, never fails a launch, and is visible only when it did not work.
- One home for the update rule (`BaseUpdater`), so `LaunchService` stays a sequence of steps.
- The new setting follows Default place exactly, so nothing new is invented for the dialog or the row.

**Non-Goals:**
- A progress message of its own while fetching: the wizard's start already runs under the status line's spinner.
- A diagnostic event for the fetch: the child process is already counted by `RecordedProcessRunner` as a git `ChildCommand`.

## Decisions

### 1. Fetch one branch with an explicit refspec

`git fetch origin +refs/heads/<branch>:refs/remotes/origin/<branch>`, where `<branch>` comes from `origin/HEAD`.
- **Why:** it updates exactly the ref the worktree branches from, whatever the repository's `remote.origin.fetch` says. The `+` matches git's default refspec, which forces remote-tracking refs.
- **Alternatives:**
  - `git fetch origin` fetches every branch, which is slower on big repositories.
  - `git fetch origin main` relies on git's opportunistic update of the remote-tracking ref, which a custom refspec can switch off.
  - `git pull` in the main checkout touches the user's files for the Fetch case too.

A new Domain text value, `RemoteBranch`, parses `origin/main` (`TryParse`, since it is git's output) into its remote and its `BranchName`. A value that does not parse, or no `origin/HEAD`, means nothing to fetch, and the base is `HEAD` as today.

### 2. The fetch cannot prompt: variables for one child

- `ProcessRequest` gains an init-only `Variables` property, an `IReadOnlyDictionary<string, string>` that is empty by default, so every existing caller is unchanged and nothing in it is nullable.
- `ProcessRunner.StartInfo` sets those variables after the `InheritedEnvironment` drop.
- `GitCli`'s fetch alone sets `GIT_TERMINAL_PROMPT=0` (git's own prompt) and `GCM_INTERACTIVE=false` (Git Credential Manager's prompts and sign-in window). A fetch needing credentials then fails at once with git's words, instead of waiting on a hidden console until the 60-second limit, or stealing focus with a window the user did not ask for.
- GCM's documentation (`docs/environment.md`, read through Context7 on 13 November 2026) says `false` or `0` disables interaction and returns an error when interaction is needed; `never` is the older spelling it still accepts.
- **Alternatives:**
  - `-c credential.interactive=never` covers Git Credential Manager without a port change, but cannot stop git's own terminal prompt.
  - Keeping `GIT_TERMINAL_PROMPT` inherited would bring back the bug `InheritedEnvironment` fixed for tabs.
- **To verify first** (task 3.1, a spike before the port change), with real git 2.55 and Git Credential Manager on a developer machine, and record with the date in `.claude\rules\windows.md`: an HTTPS remote with no stored credential fails at once under both variables, and nothing appears on screen. If it does not, the decision is revisited before `ProcessRequest` changes. The adapter tests cover only that the variables reach the child.

SSH remotes are not covered: an SSH key passphrase prompt still waits until the time limit (see Risks).

### 3. The commands throw; `BaseUpdater` turns them into a result

New `IGit` members, each documented by the port's convention:

| Member | git | Failure |
|---|---|---|
| `FetchBranchAsync(repository, remote, branch)` | `fetch <remote> +refs/heads/<b>:refs/remotes/<remote>/<b>` | throws `GitException` |
| `BranchExistsAsync(repository, branch)` | `rev-parse --verify --quiet refs/heads/<b>` | answers false |
| `FastForwardCheckoutAsync(checkout, target)` | `merge --ff-only <target>` in the checkout | throws `GitException` |
| `FastForwardBranchAsync(repository, branch, target)` | `fetch . <target>:refs/heads/<b>` (no `+`, so git refuses a non-fast-forward) | throws `GitException` |

- `Application\Launch\BaseUpdater(IGit git)` has one method: it takes the repository, the `RemoteBranch` and the `BeforeBranching` choice, and answers a `BaseUpdate`. It is the only place that catches `GitException` and `ProcessTimedOutException` for these steps; their messages are shown only to say what failed, as CLAUDE.md allows.
- **Why not have `IGit` answer results?** Every other `IGit` command throws, and the port is due its own migration to Domain values. A second convention would make that harder. The catch lives in one tested class instead.
- **Pull's placement** is a pure decision in `Application\Launch\FastForwardPlace`, made from `ListWorktreesAsync`'s entries and the branch:
  - `MainCheckout` when the main entry's head is that branch.
  - `ElsewhereAt(FolderPath)` when a linked worktree's head is.
  - `NotCheckedOut` otherwise.

  When `BranchExistsAsync` is false, nothing is fast-forwarded and no warning is given.
- **Why `fetch .` for a branch that is not checked out:** it is one command, refuses a non-fast-forward itself, and refuses a branch checked out anywhere (a second guard). The alternative, `merge-base --is-ancestor` then `update-ref` with the old value, is two steps for the same result.

### 4. The result travels on the started outcome

- `BaseUpdate` is a closed hierarchy in `Application\Launch`:
  - `Current`: fetched, pulled, or nothing was asked or needed. It has no words.
  - `NotFetched(string Reason)`.
  - `NotFastForwarded(BranchName Branch, FastForwardRefusal Refusal)`, where the refusal is `CheckedOutElsewhere(FolderPath)` or `Refused(string Reason)`.
- `LaunchOutcome.Started` gains `BaseUpdate Update`, so `Listed` and `NotListedYet` carry it. `HereCommandRunner` ignores it, since `cader here` never updates.
- `StartSessionOutcome.StartedAndOpened` and `NotListedYet` carry it on. `WordingOfOutcomes` appends one sentence after the started words, for example "Origin could not be fetched, so it branched from main as last fetched: <reason>".
- `Wording.ToneOf` answers the worse of the tab's tone and `Notice` when the update is not `Current`.
- **Alternative:** a separate status message after the start. The status line holds one message, so the second would hide the first.

### 5. The setting and the wizard

- **The setting:**
  - Domain enum `BeforeBranching { Fetch, Pull, None }`, with `Settings.BeforeBranchingWhenUnset = Fetch`.
  - `Settings`, `SettingsText`, `UserConfig` (`BeforeBranching?`, whose comment now says seven properties), and `SettingsFile` through `DefinedOr`.
  - `JsonFileStore` already writes enums by name.
- **The Config tab:**
  - `ConfigRow.BeforeBranching` follows `DefaultBase` in `ConfigRows`.
  - `SettingsScope.BeforeBranching` and `SettingsField.BeforeBranching` open the dialog with an `OptionSelector`, laid out as Place's is.
  - Config-tab Enter is already routed by `SettingsScope.For`.
- **The plan:** `LaunchRequest` and `LaunchPlan` carry `BeforeBranching`, and `LaunchPlanBuilder` copies it. `HereCommand` passes `None`. `LaunchService` consults it only inside `CreateWorktreeAsync`, and only for `WorktreeBase.RemoteDefault`.
- **The wizard:**
  - `WizardState.BeforeBranching` comes from the settings on `Open`, changed by `WizardInput.BeforeBranchingChanged`.
  - `WizardField.BeforeBranching` sits between `Base` and `Prompt` in Tab order.
  - It is shown only when `ShowsChooser`, and ignored for Current branch or the HEAD base, as the base selector already is.
- **Wizard layout:**
  - A vertical three-option `OptionSelector` under "Before branching (↑↓ to change)" adds four rows: `FormRows` goes from 17 to 21, and every `AnchorEnd` offset above the preview moves by four.
  - At 120 x 36 the dialog is 90 % of the height, about 32 rows, which leaves the repository list well above its four-row minimum.
  - If the headless check shows otherwise, the fallback is a horizontal selector on one row (two rows with its caption).
- **Words:** `Describe(WorktreeBase.RemoteDefault)` becomes "origin's default branch", and the Default base description and `Describe` comment lose "no fetch is made" and "the console never fetches".

## Risks / Trade-offs

- **A fast-forward changes the main checkout's files while a session may be running there:** Pull is a choice the user makes, git refuses when its local changes would be overwritten, and the user guide says so. Fetch, the default, never touches a checkout.
- **An SSH remote whose key needs a passphrase waits until the 60-second limit:** the timeout is caught, the launch carries on with a notice naming the limit, and the user guide suggests an SSH agent. Setting `GIT_SSH_COMMAND` would override the user's own SSH set-up.
- **A slow network adds up to 60 seconds to a launch:** the spinner shows it is working, and Nothing turns it off. A per-request limit would need another `ProcessRequest` member, and that is deferred (proposal: Out of scope).
- **`origin/HEAD` names a renamed default branch:** the fetch then fails or fetches the old branch, and the launch warns or uses it. `git remote set-head origin --auto` is the user's fix, named in the user guide.
- **`diagnostic-log-setting` edits the same `Settings` constructor, `Equals`, `UserConfig` and `ConfigRows`:** whichever lands second rebases by hand. The spec deltas avoid that change's modified requirements, so only code conflicts.

## Migration Plan

None. A `config.json` without `beforeBranching` reads as Fetch, which is a behaviour change on upgrade: the first launch after it fetches. The CHANGELOG's Changed line says so and names the setting that turns it off. Rolling back to an earlier version ignores the unknown property.
