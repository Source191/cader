# Spec Delta

## Purpose

Lets a person add a second Claude account with one command, choosing in plain words how it starts and what, if anything, it shares with the default account, with nothing written before they agree and everything Cader added removable again.

## ADDED Requirements

### Requirement: setup-account makes and registers the account
`cader setup-account <name>` SHALL make the folder `%USERPROFILE%\.claude-<name>` and add the account to `config.json`'s accounts, first adding the existing Claude folder as `default` when no list exists yet. A name that is not an account name SHALL be refused with the reason, writing nothing, and exit 2.

#### Scenario: The first extra account
- **WHEN** `config.json` has no accounts and the user runs `cader setup-account personal --yes`
- **THEN** `C:\Users\me\.claude-personal` exists, and `config.json`'s accounts are default at `C:\Users\me\.claude` then personal at `C:\Users\me\.claude-personal`

#### Scenario: A name that is not plain
- **WHEN** the user runs `cader setup-account "my personal"`
- **THEN** the output says a name is letters, digits and dashes, nothing is written, and the exit code is 2

### Requirement: The default account is the folder in the user profile
The account the command copies, links or shares from, and the one it registers as `default`, SHALL always be `%USERPROFILE%\.claude`, whatever `CLAUDE_CONFIG_DIR` the shell has. When the shell has `CLAUDE_CONFIG_DIR` set, the command SHALL say it is not used and name the folder it reads instead.

#### Scenario: Run from a shell set up for another account
- **WHEN** the shell's `CLAUDE_CONFIG_DIR` is `C:\Users\me\.claude-personal` and the user runs `cader setup-account hobby --mode copy`
- **THEN** the output says CLAUDE_CONFIG_DIR is not used here, the items offered are those in `C:\Users\me\.claude`, and default is registered at `C:\Users\me\.claude`

### Requirement: Nothing is written before the preview is accepted
The command SHALL first print every folder it will make, every file it will copy, move or edit, every junction it will make, the profile and the `config.json` change, then ask once; nothing SHALL be written unless the answer is yes or `--yes` was given. A declined preview SHALL exit 0 with nothing written.

#### Scenario: Declined
- **WHEN** the user runs `cader setup-account personal`, reads the preview and answers n
- **THEN** no folder, file, junction, profile or `config.json` entry is made, and the exit code is 0

### Requirement: The person chooses how the account starts
Before the preview the command SHALL ask how the account starts, offering Clean, Copy, Link to default and Shared folder, each with one plain sentence on what it touches and whether edits are shared. `--mode clean|copy|link|shared` SHALL answer the question, and `--yes` without `--mode` SHALL choose Clean.

#### Scenario: The choice explained
- **WHEN** the user runs `cader setup-account personal`
- **THEN** the four choices are listed, Clean saying nothing outside the new folder is touched and Shared folder saying the default account's files are moved and backed up first

#### Scenario: Unattended
- **WHEN** the user runs `cader setup-account personal --yes`
- **THEN** the account starts Clean and nothing outside `C:\Users\me\.claude-personal`, `config.json` and Cader's fragment changes

### Requirement: Clean touches only the new folder
A Clean start SHALL put no instructions, rules or skills in the new account's folder, leaving it for Claude Code to fill on its first run; the only file Cader SHALL write there is `settings.json`, and only when the bell is accepted. It SHALL NOT read, copy or change anything in any other Claude folder.

#### Scenario: Clean
- **WHEN** the user chooses Clean, declines the bell and accepts
- **THEN** `C:\Users\me\.claude-personal` is empty and `C:\Users\me\.claude` is byte for byte as it was

#### Scenario: Clean with the bell
- **WHEN** the user runs `cader setup-account personal --yes`
- **THEN** `C:\Users\me\.claude-personal` holds only `settings.json`, with `"preferredNotifChannel": "terminal_bell"`

### Requirement: Copy leaves both accounts standard
A Copy start SHALL copy the items the person chose from the default account (its `CLAUDE.md`, rule files and skill folders) into the same places in the new folder, SHALL NOT change the default account, and SHALL make no link, so later edits in either account stay in that account.

#### Scenario: Copy two skills
- **WHEN** the default account has the skills dotnet and sql, the user chooses Copy and picks both and the `CLAUDE.md`
- **THEN** the new folder holds copies of `CLAUDE.md`, `skills\dotnet` and `skills\sql`, nothing in it is a junction, and the default account is unchanged

### Requirement: Link to default leaves the default account untouched
A Link to default start SHALL write a new `CLAUDE.md` whose first line imports `~/.claude/CLAUDE.md`, link the new account's `rules\default` to the default account's rules folder, and link each chosen skill, and SHALL NOT change anything in the default account. The preview SHALL say edits made in either account show in both.

#### Scenario: Link
- **WHEN** the user chooses Link to default with the skill dotnet and accepts
- **THEN** the new `CLAUDE.md` starts with `@~/.claude/CLAUDE.md`, `rules\default` and `skills\dotnet` in the new folder are junctions to the default account's folders, and the default account is unchanged

### Requirement: Shared folder backs up before it moves
A Shared folder start SHALL back up every file it will move or edit to `%LOCALAPPDATA%\Cader\backups\<UTC time>\` and name that folder, then move the chosen rule files and skills into `%USERPROFILE%\.claude-shared`, link them into both accounts, and add an import of `~/.claude-shared/CLAUDE.md` at the top of both accounts' `CLAUDE.md`, keeping each file's line endings.

#### Scenario: Shared
- **WHEN** the user chooses Shared folder with the rule file testing.md and the skill dotnet, and moves the whole `CLAUDE.md` into the shared one
- **THEN** the backup holds the originals, `C:\Users\me\.claude-shared` holds `CLAUDE.md`, `rules\testing.md` and `skills\dotnet`, both accounts have `rules\shared` and `skills\dotnet` as junctions to them, and both `CLAUDE.md` files start with `@~/.claude-shared/CLAUDE.md`

#### Scenario: A CRLF file keeps its endings
- **WHEN** the default `CLAUDE.md` uses CRLF line endings and the import line is added to it
- **THEN** every line in it, the new one included, ends with CRLF

### Requirement: The items to share are chosen one by one
For Copy, Link to default and Shared folder the command SHALL list the default account's `CLAUDE.md`, rule files and skill folders it found and ask which to take, accepting all, none or their numbers; for Link to default the rules SHALL be one item, the whole rules folder, because a link can only point at a folder. An item already present in the new folder SHALL be left as it is and listed as kept.

#### Scenario: Picking by number
- **WHEN** the list shows 1 CLAUDE.md, 2 skills\dotnet and 3 skills\sql, and the user answers 1 3
- **THEN** the preview names `CLAUDE.md` and `skills\sql` only

#### Scenario: Link offers the rules whole
- **WHEN** the default account has the rule files testing.md and style.md and the user chooses Link to default
- **THEN** the list offers rules as one item, naming both files, and not each file on its own

### Requirement: --yes makes the smallest change it can
With `--yes`, Copy, Link to default and Shared folder SHALL take every item listed; Shared folder SHALL start the shared `CLAUDE.md` empty and keep the default account's text where it is; and `--remove` SHALL NOT put the default account back, saying instead how to do it.

#### Scenario: Shared folder unattended
- **WHEN** the user runs `cader setup-account personal --mode shared --yes`
- **THEN** the default `CLAUDE.md` keeps all its text under the new import line, and `C:\Users\me\.claude-shared\CLAUDE.md` is empty

#### Scenario: Remove unattended
- **WHEN** personal is the last shared account and the user runs `cader setup-account personal --remove --yes`
- **THEN** the default account keeps its junctions and import line, and the output says to run `cader setup-account personal --remove` without --yes to put it back

### Requirement: The preview says what is never shared
Every preview SHALL say that the new account has its own login, session history, `settings.json` (hooks, permissions, status line), plugins, MCP servers and repository trust, that each repository must be trusted again in it, and that Claude Console sign-ins without an API key are not kept apart by the folder.

#### Scenario: The limits are stated
- **WHEN** the preview is printed for any start
- **THEN** it says the new account trusts each repository again and keeps its own settings.json, plugins and MCP servers

### Requirement: A folder Cader did not make is only registered
When `%USERPROFILE%\.claude-<name>` already exists, holds anything, and Cader has no setup record for it, the command SHALL NOT write into it or apply a start: it SHALL offer to register it as the account, add the profile and offer the bell, and say why nothing else is done.

#### Scenario: Set up by hand already
- **WHEN** the user made `C:\Users\me\.claude-personal` by Anthropic's guide and has logged in there, then runs `cader setup-account personal`
- **THEN** the preview offers only to register it, add the profile and the bell, and no file in that folder is changed

### Requirement: Running it again is safe
Running the command again for an account Cader has a setup record for SHALL carry on from that record: skip every step already done, say so for each, and do only what is missing, including a step an interrupted run left undone, whatever Claude Code has since written in the folder.

#### Scenario: Again
- **WHEN** the user runs `cader setup-account personal --yes` twice
- **THEN** the second run makes no change and says every step was already done

#### Scenario: After the first login
- **WHEN** personal was set up Clean, the user logged in through the tab, and then runs `cader setup-account personal` again
- **THEN** the folder is not treated as one Cader did not make, and the run says every step was already done

### Requirement: The account is reachable outside Cader
The command SHALL add a "Claude (<name>)" profile to Cader's Windows Terminal fragment that opens PowerShell with `CLAUDE_CONFIG_DIR` set to the account's folder, and SHALL print the one PowerShell line that does the same in any shell. It SHALL NOT change the person's PowerShell profile or environment variables.

#### Scenario: The profile and the line
- **WHEN** the user accepts the preview for personal
- **THEN** the fragment has a Claude (personal) profile, the output ends with `$env:CLAUDE_CONFIG_DIR = 'C:\Users\me\.claude-personal'; claude` and says to restart Windows Terminal

### Requirement: The first login happens in a tab
After writing, the command SHALL open a Windows Terminal tab running Claude Code as the new account so its first login and setup happen there, and SHALL offer the Claude bell in the account's `settings.json` as `setup-terminal` does.

#### Scenario: Login tab
- **WHEN** a new account has been set up
- **THEN** a tab titled for the account runs claude with `CLAUDE_CONFIG_DIR` set to its folder

### Requirement: Roots can be given to the account
`--root <folder>`, given once per root, SHALL give each named repository root to the new account in `rootAccounts`; a folder that is not a configured root SHALL be refused before anything is written.

#### Scenario: A personal root
- **WHEN** `D:\personal` is a root and the user runs `cader setup-account personal --root D:\personal --yes`
- **THEN** `rootAccounts` gives `D:\personal` to personal

### Requirement: Remove takes out only what Cader added
`cader setup-account <name> --remove` SHALL remove the account's profile, the junctions and import lines Cader recorded for it, and its `config.json` entries, after a preview and a question. It SHALL NOT delete any Claude folder, the shared folder, or anything copied or moved, and SHALL say where they are.

#### Scenario: Remove a linked account
- **WHEN** personal was started with Link to default and the user runs `cader setup-account personal --remove --yes`
- **THEN** the profile, `rules\default` and the skill junctions are gone, the import line is gone from personal's `CLAUDE.md`, the account is gone from `config.json`, `C:\Users\me\.claude-personal` still exists with its login, and the default account is unchanged

### Requirement: Remove can restore the default account
When the account removed is the last one linked to the shared folder, the command SHALL offer, as a separate question, to put the default account back: move its shared items back in place of its junctions and remove its import line, backing up first.

#### Scenario: The last shared account
- **WHEN** personal is the only account besides default that uses the shared folder and the user removes it and accepts the restore
- **THEN** the default account's rule files and skills are ordinary files and folders again, its `CLAUDE.md` has no shared import, and `C:\Users\me\.claude-shared` still exists

### Requirement: Removing a link never touches its target
Cader SHALL remove only a junction it recorded, only while it is still a junction pointing at the recorded target, and SHALL remove the link alone, never the folder it points to; anything else in its place SHALL be left and reported.

#### Scenario: A junction replaced by a real folder
- **WHEN** the user replaced `skills\dotnet` in the personal folder with a real folder, then removes the account
- **THEN** `skills\dotnet` is left as it is, and the output says it was not a link Cader made

#### Scenario: The target survives
- **WHEN** Cader removes the junction `rules\default`
- **THEN** every file in `C:\Users\me\.claude\rules` is still there

### Requirement: Setup stays inside the Claude folders
Every folder the command makes, writes, moves into or links SHALL be inside `%USERPROFILE%\.claude`, `%USERPROFILE%\.claude-<name>`, `%USERPROFILE%\.claude-shared` or Cader's own state folder, and SHALL NOT be `.credentials.json`, `.claude.json`, `settings.json`, `projects`, `sessions`, `plugins` or the history.

#### Scenario: A skill that is itself a link elsewhere
- **WHEN** the default account's `skills\tools` is a junction to `D:\tools\skill`
- **THEN** Shared folder offers it only to link, never to move, and the preview says it already points outside the Claude folders
