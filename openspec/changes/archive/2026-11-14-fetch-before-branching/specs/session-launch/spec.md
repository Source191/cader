## ADDED Requirements

### Requirement: The base is fetched before a new worktree branches from it
When a new worktree is created from origin's default branch and Before branching is Fetch origin or Pull default branch, Cader SHALL fetch that one branch from origin before creating the worktree, and SHALL branch the worktree from the updated remote-tracking branch. The fetch SHALL NOT show a prompt or a sign-in window, and a fetch still running at the time limit for git commands SHALL be ended and treated as failed.

#### Scenario: Fetch by default
- **WHEN** `config.json` has no Before branching, origin's default branch is main, and the user starts a session named fix-123 in a new worktree of Northwind.Orders with the base origin's default branch
- **THEN** Cader fetches main from origin, then creates the worktree on branch fix-123 at the commit origin's main has after the fetch

#### Scenario: Nothing
- **WHEN** Before branching is Nothing and the user starts a session in a new worktree with the base origin's default branch
- **THEN** no fetch is made and the branch starts at origin's main as last fetched

#### Scenario: Origin has no default branch recorded
- **WHEN** the repository has no `origin/HEAD` and Before branching is Fetch origin
- **THEN** no fetch is made, the branch starts at HEAD as it does today, and the session starts without a warning

#### Scenario: A remote that needs signing in
- **WHEN** fetching from an HTTPS origin needs credentials that are not already stored
- **THEN** no prompt or sign-in window appears, the fetch fails, and the launch carries on as for any failed fetch

#### Scenario: A fetch that does not finish
- **WHEN** the fetch is still running when the time limit for git commands passes, for example waiting on an SSH key's passphrase
- **THEN** it is ended, and the launch carries on as for any failed fetch, the status line naming the limit

### Requirement: Nothing is fetched or pulled outside a new worktree from origin
Cader SHALL NOT fetch or pull for a session started on the current branch, in a fixed folder, with `cader here`, in an existing worktree the wizard offered instead of creating one, or in a new worktree whose base is HEAD.

#### Scenario: Current branch
- **WHEN** Before branching is Pull default branch and the user starts a session on Current branch
- **THEN** no git fetch, merge or branch update is run before the session starts

#### Scenario: HEAD base
- **WHEN** Before branching is Fetch origin and the user starts a session in a new worktree with the base HEAD
- **THEN** no fetch is made and the branch starts at HEAD

#### Scenario: cader here
- **WHEN** Before branching is Pull default branch and the user runs cader here fix-tests
- **THEN** no fetch or pull is made

### Requirement: Pull also fast-forwards the local default branch
When Before branching is Pull default branch, Cader SHALL, after a successful fetch, fast-forward the local branch of the same name to origin's: in the main checkout when it has that branch checked out, or without a checkout when no worktree has it checked out. It SHALL never merge, rebase or create the branch, and the new worktree SHALL still branch from origin's default branch.

#### Scenario: Checked out in the main checkout
- **WHEN** Before branching is Pull default branch, the main checkout of Northwind.Orders is on main with no local changes, and origin's main is two commits ahead
- **THEN** the main checkout's main is fast-forwarded by those two commits, its files updated, and the new worktree branches from origin's main

#### Scenario: Checked out nowhere
- **WHEN** Before branching is Pull default branch, the main checkout is on fix-99 and no worktree has main checked out
- **THEN** the local main branch is moved to origin's main and no worktree's files change

#### Scenario: No local default branch
- **WHEN** Before branching is Pull default branch and the repository has no local main
- **THEN** no branch is created, the worktree branches from origin's main, and the session starts without a warning

#### Scenario: Local changes git leaves alone
- **WHEN** the main checkout is on main with an uncommitted change to a file the incoming commits do not touch
- **THEN** main is fast-forwarded and the uncommitted change is kept

### Requirement: A failed update warns and the launch carries on
When the fetch fails or times out, or the fast-forward is refused, Cader SHALL still create the worktree from origin's default branch as it stands, start the session and open its tab, and the status line SHALL say the session started and, as a notice, what was not updated and why.

#### Scenario: Offline
- **WHEN** Before branching is Fetch origin and origin cannot be reached
- **THEN** the worktree branches from origin's main as last fetched, the session starts and opens, and the status line says it started and that origin could not be fetched, with git's reason

#### Scenario: Local commits on main
- **WHEN** Before branching is Pull default branch and the local main has a commit origin's main does not
- **THEN** main is left as it was, the session starts from origin's main, and the status line says main could not be fast-forwarded

#### Scenario: Local changes in the way
- **WHEN** the main checkout is on main with an uncommitted change to a file the incoming commits change
- **THEN** main and the change are left as they were, the session starts from origin's main, and the status line says main could not be fast-forwarded and why

#### Scenario: Checked out in another worktree
- **WHEN** Before branching is Pull default branch and main is checked out in the linked worktree C:\Users\me\source\worktrees\Northwind.Orders-main
- **THEN** main is left as it was, the session starts from origin's main, and the status line says main is checked out in that worktree and was not fast-forwarded

### Requirement: The wizard chooses what happens before branching
The wizard opened with `n` SHALL offer Before branching, with the choices Fetch origin, Pull default branch and Nothing, under Worktree base and before the opening prompt in Tab order, starting on the saved setting. A choice made there SHALL apply to that launch only and SHALL NOT change `config.json`. The wizard opened on a fixed folder SHALL NOT show it.

#### Scenario: Starts on the setting
- **WHEN** Before branching is saved as Pull default branch and the user opens the wizard with `n`
- **THEN** Before branching reads Pull default branch

#### Scenario: One launch only
- **WHEN** the setting is Fetch origin, the user chooses Nothing in the wizard and starts the session, then opens the wizard again
- **THEN** that launch makes no fetch, `config.json` still says Fetch, and the new wizard reads Fetch origin

#### Scenario: Fixed folder
- **WHEN** the user presses `n` on a worktree row
- **THEN** the wizard shows no Before branching choice

#### Scenario: The base choice no longer says it is stale
- **WHEN** the user opens the wizard with `n`
- **THEN** the Worktree base choice reads origin's default branch, without "as last fetched"
