# Spec Delta

## MODIFIED Requirements

### Requirement: The pane shows the selected session
On the Sessions tab the pane SHALL show what the selected row does not, titled with its name: what it waits for, when it started and was last prompted, the full folder, the session id, the worktree, the branch, its tab, the folder's changes, its memory and the account it belongs to. A fact that cannot be read SHALL show as unavailable without hiding the rest.

#### Scenario: A session at a permission prompt
- **WHEN** fix-123 is selected, listed as waiting for a permission prompt, started at 14:02 today in a Cader tab coloured yellow, in the worktree Northwind.Orders-fix-123 on branch fix-123 with three changed files
- **THEN** the pane is titled fix-123 and reads Waiting permission prompt, Started 14:02 (5m ago), Worktree fix-123, Branch fix-123, Tab open, yellow, and Changes 3 changed files · 0 unpushed commits with the last commit's age

#### Scenario: Facts in two columns of five rows
- **WHEN** a session is selected
- **THEN** the pane's left column reads, top to bottom, Waiting, Folder, Worktree, Branch and Changes, and its right column Started, Prompted, Session, Tab and Memory, each fact beside the one in the same row, and Account follows on a sixth row under the left column with nothing beside it

#### Scenario: A session never sent a prompt
- **WHEN** a session that needs input and has no transcript is selected
- **THEN** the pane's Waiting line reads a first prompt

#### Scenario: A working session
- **WHEN** a working session is selected
- **THEN** the pane's Waiting line reads nothing

#### Scenario: The main checkout
- **WHEN** a session running in C:\Users\me\source\repos\Northwind.Orders on branch main is selected
- **THEN** the pane reads Worktree main checkout and Branch main

#### Scenario: Memory in the pane
- **WHEN** a running session whose tree uses 903 MB is selected
- **THEN** the pane's Memory line reads 903 MB and names m for the breakdown

#### Scenario: A stopped session
- **WHEN** a stopped session is selected
- **THEN** the pane shows its folder and branch and reads nothing to measure for its memory

#### Scenario: A terminal session
- **WHEN** a plain claude session in its own terminal is selected
- **THEN** the pane shows its facts and its Tab line reads its own terminal

#### Scenario: A session under the current login
- **WHEN** a session owned by the account labelled work, me@northwind.example in Northwind Traders, is selected while that account is logged in
- **THEN** the pane's Account line reads work · me@northwind.example · Northwind Traders · logged in now

#### Scenario: A session under another account
- **WHEN** a session owned by the account labelled personal, me@home.example, is selected while another account is logged in
- **THEN** the pane's Account line reads personal · me@home.example, followed by its organisation and log in as me@home.example to resume it

#### Scenario: An owner not seen yet
- **WHEN** a session whose owner Cader has never seen logged in is selected
- **THEN** the pane's Account line says the account has not been seen logged in, and that logging in as it once lets Cader name it

#### Scenario: No recorded owner
- **WHEN** a session whose transcript has no `session-owner` line is selected
- **THEN** the pane's Account line says Claude Code did not record the account for this session
