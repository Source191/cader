# Spec Delta

## ADDED Requirements

### Requirement: The status line names the launch's current step
While a session started from the wizard is being launched, the status line SHALL read `Starting <name>: <step> (<n> of <count>)...`, where the count is the number of steps that launch runs, in order: fetching or pulling origin's default branch (as `fetching origin/main` or `pulling origin/main`), creating the worktree, starting Claude Code, finding the new session and opening its tab. Until the first step begins it SHALL read `Starting <name>...`.

#### Scenario: A new worktree fetched from origin
- **WHEN** the user starts fix-123 in a new worktree from origin's default branch main with Before branching Fetch origin
- **THEN** the status line reads, in turn, `Starting fix-123: fetching origin/main (1 of 5)...`, `creating the worktree (2 of 5)`, `starting Claude Code (3 of 5)`, `finding the new session (4 of 5)` and `opening its tab (5 of 5)`, each after `Starting fix-123: `

#### Scenario: Pull default branch
- **WHEN** Before branching is Pull default branch for the same launch
- **THEN** the first step reads `pulling origin/main (1 of 5)`

#### Scenario: No fetch
- **WHEN** the user starts fix-123 in a new worktree with Before branching Nothing, or with the base HEAD, or in a repository with no `origin/HEAD`
- **THEN** the steps are counted out of 4, starting with `creating the worktree (1 of 4)`

#### Scenario: No new worktree
- **WHEN** the user starts fix-123 on the current branch, in a folder choice or in an existing worktree
- **THEN** the steps are counted out of 3, starting with `starting Claude Code (1 of 3)`

#### Scenario: The spinner stays
- **WHEN** a step begins
- **THEN** the status line's words change and its spinner keeps turning until the launch's outcome replaces the words

#### Scenario: The launch ends early
- **WHEN** the workspace is not trusted, the start fails, or the session is not listed yet
- **THEN** the step words are replaced by that outcome's words as today, and no later step is shown

### Requirement: Refreshes wait while a session starts from the console
While a session started from the wizard is being launched, the Sessions and Worktrees tabs SHALL start no refresh on their timers or for a change on disc. A change seen meanwhile SHALL be kept, and each tab SHALL refresh after the launch ends, whatever its outcome. F5 and showing a tab SHALL still refresh at once, and a refresh already running SHALL run to its end.

#### Scenario: The new worktree's record
- **WHEN** creating the worktree writes its record under the repository's `.git\worktrees` folder
- **THEN** no worktrees scan starts until the launch ends, and a scan starts after it

#### Scenario: The Sessions timer
- **WHEN** Sessions refresh is 5 seconds and the launch takes 9 seconds
- **THEN** no listing starts during the launch, and the sessions are listed after it

#### Scenario: F5 during a launch
- **WHEN** the user presses F5 on the Worktrees tab while a launch runs
- **THEN** a scan starts at once

#### Scenario: A failed launch
- **WHEN** the launch fails at any step, or throws
- **THEN** the hold ends and the tabs refresh as after any action

### Requirement: The base is fetched while the wizard is open
Once a repository is chosen in the wizard, by Enter on it or by moving on from the list, and the form says a new worktree from origin's default branch with Fetch origin or Pull default branch, Cader SHALL start fetching that one branch, at most once per repository each time the wizard opens. At F5 the launch SHALL wait for that fetch when it is still running or finished within the last minute without failing, and SHALL fetch again otherwise. The fetch SHALL change no local branch or worktree.

#### Scenario: Fetched while the name is typed
- **WHEN** the user presses Enter on Northwind.Orders, takes five seconds to type fix-123 and presses F5
- **THEN** one fetch of origin's main is made, started when Enter was pressed, and the launch's first step passes as soon as it is shown

#### Scenario: Still fetching at F5
- **WHEN** the user presses F5 while that fetch is still running
- **THEN** the launch shows `fetching origin/main (1 of 5)` until the fetch ends and starts no second fetch

#### Scenario: An early fetch that failed
- **WHEN** the early fetch failed because origin could not be reached
- **THEN** F5 fetches again, and if that fails too the status line says origin could not be fetched, with git's reason, as for any failed fetch

#### Scenario: Left open
- **WHEN** the early fetch finished two minutes before the user presses F5
- **THEN** the launch fetches again

#### Scenario: Moving through the list
- **WHEN** the user moves the cursor over five repositories with the arrow keys without pressing Enter or moving on from the list
- **THEN** no fetch is started

#### Scenario: Pull default branch
- **WHEN** Before branching is Pull default branch and the early fetch has finished
- **THEN** the local default branch is fast-forwarded only after F5, as today

#### Scenario: The wizard cancelled
- **WHEN** the user presses Esc after the early fetch started
- **THEN** no session starts, the fetch runs to its end or the time limit for git commands, and nothing is shown about it

#### Scenario: Nothing chosen in the wizard
- **WHEN** the setting is Fetch origin and the user chooses Nothing in the wizard before choosing the repository
- **THEN** no fetch is made

## MODIFIED Requirements

### Requirement: Nothing is fetched or pulled outside a new worktree from origin
Cader SHALL NOT fetch or pull for a session started on the current branch, in a fixed folder, in a folder choice, with `cader here`, in an existing worktree the wizard offered instead of creating one, or in a new worktree whose base is HEAD. The one exception is a fetch the wizard started early because its form then said a new worktree from origin's default branch with Fetch origin or Pull default branch; it SHALL NOT be followed by a fast-forward or any other branch update.

#### Scenario: Current branch
- **WHEN** Before branching is Pull default branch and the user starts a session on Current branch
- **THEN** no git fetch, merge or branch update is run before the session starts

#### Scenario: Changed to Current branch after the early fetch
- **WHEN** the early fetch started for Northwind.Orders and the user then chooses Current branch and starts the session
- **THEN** no further fetch is made, and no merge or branch update is run

#### Scenario: HEAD base
- **WHEN** Before branching is Fetch origin and the user starts a session in a new worktree with the base HEAD
- **THEN** no fetch is made and the branch starts at HEAD

#### Scenario: cader here
- **WHEN** Before branching is Pull default branch and the user runs cader here fix-tests
- **THEN** no fetch or pull is made

#### Scenario: A folder choice
- **WHEN** Before branching is Pull default branch and the user starts a session in the folder choice C:\Users\me\source\repos while Where reads New worktree
- **THEN** no fetch or pull is made
