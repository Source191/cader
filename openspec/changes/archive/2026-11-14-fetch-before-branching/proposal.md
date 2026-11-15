# Proposal

## Why

A new worktree branches from `origin`'s default branch as it stood at the last `git fetch`, and Cader never fetches, so a session often starts on a base that is days old and the first thing it does is rebase. Cader should bring the base up to date first by default, and let anyone who works offline, or wants to control fetches themselves, turn that off.

## What Changes

- A new setting, **Before branching**, kept in `config.json` as `beforeBranching` with three values:
  - **Fetch origin** (`Fetch`, the default, and what a file without the property reads as): fetch origin's default branch, then branch the new worktree from the updated `origin/<default>`.
  - **Pull default branch** (`Pull`): fetch as above, then fast-forward the local default branch (such as `main`) to it as well. When the main checkout has that branch checked out, it is fast-forwarded there with `git merge --ff-only`. When no worktree has it checked out, the branch is moved without a checkout. It is never merged or rebased. The new worktree still branches from `origin/<default>`.
  - **Nothing** (`None`): today's behaviour, branching from `origin/<default>` as last fetched.
- On the Config tab, a **Before branching** row after Default base. Enter opens a dialog to choose one of the three values, like Default place.
- In the new-session wizard opened with `n`, a **Before branching** choice under Worktree base, starting on the saved setting and changing only that one launch.
- The choice applies only when a new worktree is created from origin's default branch. Current branch, a fixed folder, `cader here`, the HEAD base and a wizard that reuses an existing worktree never fetch or pull.
- **Warn and continue:** when the fetch fails or times out, the worktree is still made from the last-fetched base. The same applies when the fast-forward is refused (local commits, local changes git would overwrite, or the branch checked out in another worktree). The session starts, and the status line says what was not updated and why.
- The words that say Cader never fetches change: the Default base row, its tooltip, its dialog and the wizard's base choice.

The wizard choice comes from the second round of questions. The first answer was "Config only", and the later answer asked for the choice in the wizard as well.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `session-launch`: before a new worktree is branched from origin's default branch, the base is fetched (and, for Pull, the local default branch fast-forwarded) as the wizard's Before branching choice says, with warn-and-continue on failure; the wizard gains that choice.
- `settings`: `config.json` gains `beforeBranching` (Fetch, Pull or None; Fetch when missing or unreadable), applied without a restart.
- `config-tab`: a Before branching row after Default base, edited in its own dialog; Default place moves to follow it.

## Impact

- **Code:**
  - Core Domain: a new `BeforeBranching` enum, `Settings` and `SettingsText`, and a `RemoteBranch` text value for `origin/main`.
  - Core Ports: `UserConfig`, `IGit` (fetch one branch, whether a local branch exists, fast-forward a checkout, fast-forward a branch that is not checked out), and `ProcessRequest` (variables set for one child, so the fetch cannot prompt).
  - Core Application: `SettingsFile`; a new `Launch\BaseUpdater` with its `BaseUpdate` result; `LaunchService`, `LaunchPlan`, `LaunchRequest`, `LaunchPlanBuilder`, `LaunchOutcome`, `HereCommand`; `StartSessionHandler` and `StartSessionOutcome`.
  - Core Views: `WizardState`, `WizardView`, `WizardInput`, `WizardField`, `SettingsScope`, `SettingsField`, `SettingsState`/`SettingsView`, `ConfigRow`, `ConfigRows`, `ConfigView`.
  - Wording: `WordingOfConfig` and `WordingOfOutcomes`, and the tone in `Wording.ToneOf`.
  - Infrastructure: `GitCli` and `ProcessRunner`.
  - Tui: `WizardControls`, `LaunchWizardDialog`, `SettingsControls`.
- **Tests:** the matching `*Should` classes. `GitCliIntegrationShould` uses a local bare repository as origin, never the network.
- **Tools and docs:**
  - The smoke script, if its wizard steps count fields.
  - `assets\readme\new-session.gif`, remade with `tools\New-ReadmeScreenshot.ps1`.
  - `docs\user-guide.md`, `.claude\rules\console-screen.md`, and `CHANGELOG.md` with a minor `Version` raise.
- **Overlap:** `diagnostic-log-setting` also adds a property to `Settings`, `UserConfig` and `SettingsFile`, and a row to `ConfigRows`. This change adds requirements rather than modifying the ones that change copies, apart from config-tab's "Default place is a setting row", which no open change touches. Whichever lands second rebases its constructor and `Equals` edits onto the other.

## Out of scope

- Multiple Claude accounts, which is a later change.
- Fetching for Current branch sessions, `cader here`, a fixed folder or the HEAD base, and pulling the branch a session is about to start on.
- Updating `origin/HEAD` when the remote's default branch has been renamed (`git remote set-head`).
- Remotes other than `origin`, fetching all branches, tags beyond what git fetches by default, and pruning.
- Signing in to a remote: a fetch that needs credentials fails at once and is reported, so the user can sign in from a terminal.
- A separate time limit for the fetch, which shares the 60-second limit of every captured child.
