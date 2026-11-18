# Cader user guide

Everything Cader does, in more detail than the [README](../README.md). In the console itself, `?` on any tab lists every key, column and detail line, and every column header and key in the key bar has a tooltip.

## Contents

- [Installing](#installing)
- [The console](#the-console)
- [Sessions](#sessions)
- [Worktrees](#worktrees)
- [Configuration](#configuration)
- [Notifications and profiles](#notifications-and-profiles)
- [Every key](#every-key)
- [Privacy](#privacy)
- [Troubleshooting](#troubleshooting)

## Installing

It runs on Windows 10 or 11. The one program you install yourself is Claude Code 2.1.309 or later, with its native installer (winget's own Claude Code package can be older than Cader needs, and does not update itself):

```powershell
irm https://claude.ai/install.ps1 | iex   # Claude Code
[Environment]::SetEnvironmentVariable('Path', [Environment]::GetEnvironmentVariable('Path', 'User') + ";$HOME\.local\bin", 'User')   # puts claude on the PATH
winget install Source191.Cader
```

Open a new terminal, run `claude` once to sign in, then `cader`.

- **What winget brings:** Windows Terminal, PowerShell 7 and git, if any of them is missing. Cader carries its own .NET runtime.
- **Pull requests:** the GitHub CLI (`gh auth login`) or the Azure CLI (`az extension add --name azure-devops`, `az login`) fills in the PR column. Without them the column stays blank.
- **Upgrading:** it tells you when a newer release is out; `U` shows what changed and upgrades it with winget in a tab of its own. With it closed, `winget upgrade Source191.Cader` does the same.
- **Uninstalling:** `winget uninstall Source191.Cader` removes it and leaves your settings and your Claude Code sessions alone.
- **Without winget:** download `cader_<version>_win-x64.zip`, or `cader_<version>_win-arm64.zip` on an Arm machine, from the [latest release](https://github.com/Source191/cader/releases/latest), check it with `gh attestation verify <zip> --repo Source191/cader`, extract it to a folder of your own and add that folder to your PATH. Windows Terminal, PowerShell 7 and git are then yours to install, and `U` can only show what changed and the release page.
- **Previews:** each preview is a [GitHub prerelease](https://github.com/Source191/cader/releases) with the same zip; extract it to any folder and run `cader.exe` from there. It never replaces the winget install, and previews never reach winget.

Start Cader from an ordinary Windows Terminal tab, not inside a Claude Code session, which turns colour off in every tab Cader opens.

## The console

One Windows Terminal tab with three tabs of its own; Left and Right step between them.

- **Settings row:** Cader's version and memory, and Claude Code's version, with a warning when it is below 2.1.309. A newer release shows here too.
- **Status line:** under the settings row while an action runs or has something to say; ✕ closes it, and a success clears itself after ten seconds.
- **Counts:** beside the tab titles, how many sessions are in each state, and the automated runs (Agent SDK and `claude -p`) with their memory.
- **Detail pane:** under the table, what the selected row does not show.
- **Key bar:** the keys for the selected row; click one to press it.

### The Sessions tab

Every Claude Code session on the machine, whether it was started here or not.

| Column | What it shows |
| --- | --- |
| NAME | The session name given at launch, or the folder name for an unnamed one |
| TAB | A block in the tab's colour while a Cader tab shows the session; ▯ grey for a plain `claude` in its own terminal |
| STATE | ◐ needs input (amber), ● working (cyan), ○ idle (white), ✓ completed (green), ✗ failed (red), ■ stopped (grey), ? unknown (grey) |
| REPOSITORY | The repository it runs in, or the folder's label outside a repository, with folder gone: before the label when the folder no longer exists |
| PR | Its pull request: #123 on GitHub (open, draft, merged, closed), !123 on Azure DevOps (active, draft, completed, abandoned) |
| ACCOUNT | The label of the Claude account the session belongs to: ● before it for the account logged in now, ? for one Cader has not yet seen logged in, blank when Claude Code did not record it (sessions from before Claude Code 2.1.325) |
| AGE | Time since the session started |

The detail pane shows, in its left column, what the session is waiting for, its folder, worktree, branch and changes (changed files, unpushed commits, the last commit), and in its right column when it started and was last prompted, its session id, its tab and its memory (`m` for the breakdown). Beneath them the Account line gives the session's account in full (label, email and organisation) and says whether it is logged in now, or which email to log in as (`/login` in any Claude Code session) before you resume, rename or adopt it.

### The Worktrees tab

Every git worktree under your repository roots, with or without a session, then each repository's branches that have no worktree:

- **Branch rows (⑂ branch):** a local branch checked out neither in the main checkout nor in any worktree, except the one that tracks origin's default branch.
- **Origin rows (○ origin):** a branch only on origin, with no local branch of its name, whose newest commit is yours (by your git `user.email`) and which is flagged left behind or closed. It appears a scan after its pull request has been looked up.

| Column | What it shows |
| --- | --- |
| WORKTREE | The owning repository › the folder's slug, or its label when not named after it; on a branch or origin row, the repository › the branch |
| HEALTH | ✓ ok, ◆ locked (by another tool), △ orphan (a folder git has forgotten), ✗ broken (registration or owner gone), □ empty (a leftover folder), ↗ elsewhere (a worktree of a repository outside the roots), ⑂ branch (no worktree), ○ origin (only on origin) |
| BRANCH | The branch's most urgent flag: ▲ left behind (red: work after its pull request merged that main lacks), ✗ gone (amber: deleted on origin), ✗ closed (amber: its pull request closed without merging), · no PR (grey: none on its host); blank when none applies or the pull request is not yet known |
| LAST COMMIT | Time since the last commit |
| SESSION | The session running in it, if any |

Gone and origin rows are as of your last `git fetch --prune`: Cader never fetches, so a branch deleted on GitHub since then is not yet gone. A branch is left behind only when its shown pull request has merged and the branch has commits that the pull request's head commit does not contain and whose changes origin's default branch lacks, so a squash merge, a branch rebased before its merge and a branch moved forward to main after it are judged correctly. Merge commits never count. In a repository that records no default branch for origin (no `origin/HEAD`; `git remote set-head origin -a` sets it), every commit after the head counts. Nothing reads · no PR or ✗ closed while the pull request cannot be looked up (no origin, the CLI missing or signed out).

The detail pane gives the health's reason in full, the branch and whether it is merged into origin's default branch (or, when BRANCH flags it, the flag's reason, such as `1 commit after #298 merged`), the pull request, the folder and when Cader created it. For a branch or origin row it gives the repository instead of the folder, and the branch's unpushed commits.

### The Config tab

The settings (see [Configuration](#configuration)), then a Setup heading with what the console reads but does not change: the settings file, the Windows Terminal profiles, Claude Code's terminal bell and the last update check. Enter on a setup row says how to change it. Once Cader has seen a Claude login, an Accounts heading lists each account (see [Accounts](#accounts)).

## Sessions

- **Start one:** `n` opens the new-session wizard. The list is grouped: Recent (the repositories you used last), Folders (no repository) (each repository root, for work across repositories) and Repositories (the rest, alphabetically); the selection steps over the headings. Type to filter the list and press Enter, type a name, choose where (a new worktree, or the repository's current branch) and, for a worktree, what it branches from and what happens before it does (Fetch origin, Pull default branch or Nothing; see Before branching under [Configuration](#configuration)), then type an opening prompt if you like. A folder choice starts the session in the folder itself, whatever Where says, and fetches nothing. Enter moves between the fields; F5 starts the session and opens it in a coloured tab named after it. With no prompt the session waits at its own prompt.
- **Where it runs:** a new worktree goes to `<worktree root>\<repository>-<name>` on a branch named after the session, so sessions never trip over each other's changes. The gitignored files a `.worktreeinclude` at the repository's root names are copied in, as Claude Code does.
- **Open it:** Enter opens the session in its own tab, or switches to that tab when it is already open.
- **Run several:** each `n` is a new session in its own worktree and tab. They are all in one list, with the ones waiting for you in amber.
- **Close the terminal, keep the sessions:** sessions Cader starts are background sessions under Claude Code's own supervisor, so closing a tab, Cader or Windows Terminal stops nothing. Start Cader again and every session is listed; Enter reattaches.
- **After a reboot:** sessions show as stopped; `R` resumes every stopped or failed session shown, each in its own tab. Filter first with `/` to resume only some.
- **Stop:** `x` stops a session; it stays listed and can be resumed.
- **Rename:** `r` gives a session a new name. It restarts under the new name and keeps its conversation.
- **Delete:** `d` deletes a session, and its worktree too when no work would be lost: never one with uncommitted changes, unpushed commits, a lock or another live session.
- **When its folder has gone:** a session whose folder was deleted reads folder gone: in REPOSITORY and in its detail pane, and resting the pointer on its row says so. Enter (on one without a process), `r` and `R` leave it alone, because a resume cannot land in a folder that no longer exists; `d` deletes it.
- **Adopt:** `b` moves a plain `claude` started in its own tab under Cader's supervision, so it survives the tab closing.
- **Close empty tabs:** `C` closes the tabs of terminal sessions that were never sent a message.
- **From a folder you are already in:** `cader here [name] [--prompt <text>]` starts a supervised session there, on the current branch, in the same tab, and lists it on the Sessions tab. Ctrl+Z detaches; the session keeps running.

## Worktrees

- **Remove:** `d` removes a worktree, refused when it holds work that would be lost. `D` removes it whatever it holds, after you type its name.
- **Delete a branch row's branch:** `d` asks before deleting the local branch when every commit on it is on a remote branch or inside its merged pull request, and otherwise says how many commits would be lost. `D` deletes it whatever it holds, after you type the branch's name. Neither ever deletes a branch on origin, so both refuse on an origin row.
- **Keys on a branch or origin row:** `p` opens its pull request, `c` copies the repository's folder and `P` prunes the repository, as on any row; `n`, `o`, `f`, `s` and Enter only say the branch has no worktree.
- **Folder in use:** when a tab or program still has the worktree's folder open, Cader waits a few seconds for it, then removes the worktree and deals with its branch anyway. A folder still held is left behind, empty, and listed as □ empty, so `d` deletes it once that tab or program is closed.
- **Prune:** `P` prunes the stale worktree registrations of the row's repository.
- **New session here:** `n` starts a session in the worktree, on the branch checked out there.
- **Shell:** `o` opens a PowerShell tab in the worktree's folder.
- **Open its session:** Enter opens the worktree's session in its own tab, or switches to it.

## Configuration

The Config tab edits the settings, which are kept in `%LOCALAPPDATA%\Cader\config.json` and read again on every scan and refresh, so a change applies at once.

- **Repository roots:** the folders searched for git repositories; `~\source\repos` until you change it. Enter edits every root, one per line, and F3 browses for a folder.
- **Worktree root:** where new worktrees go; a `worktrees` folder beside the first root until you change it.
- **Open folder with:** the program `f` opens a row's folder with: VS Code, Visual Studio, Rider or a program of your own (F3 browses for it).
- **Default base:** new worktrees branch from origin's default branch or from HEAD. A branch from origin's default branch is updated first, as Before branching says.
- **Before branching:** what happens before a new worktree branches from origin's default branch; the wizard's Before branching choice starts on it and changes it for that one session. It does nothing for a base of HEAD or a session on the current branch, and `cader here` never updates anything.
  - **Fetch origin (the default):** fetches that one branch, so the session starts on an up-to-date base. It never prompts: a remote that needs signing in fails at once, and an SSH key that needs a passphrase waits for up to a minute, so use an SSH agent.
  - **Pull default branch:** fetches, then fast-forwards your local branch of the same name too. It only ever fast-forwards, and git refuses when your local changes or commits are in the way, but it does change the files of the checkout that has the branch, so it can change files under a session running there. A branch checked out in another worktree is left alone and the status line says so.
  - **Nothing:** the old behaviour; the worktree branches from origin's default branch as of your last `git fetch`.
  - **When it does not work:** the session still starts, from the branch as last fetched, and the status line says what failed. If origin's default branch has been renamed, run `git remote set-head origin --auto` in the repository.
- **Default place:** where the wizard starts a session: a new worktree, or the repository's current branch.
- **Sessions refresh:** how often the Sessions tab lists the sessions on its own, in whole seconds from 0 to 3600; every 5 seconds until you change it. 0 turns the timer off: the list still refreshes as the console opens, after an action and on F5, but the bell for a session that starts waiting rings only at the next refresh.
- **Worktrees refresh:** how often the Worktrees tab rescans while it is shown, in whole seconds from 0 to 3600; every 15 seconds until you change it. 0 turns the timer off: it still rescans when shown, after an action and on F5. A slow scan of many repositories is never joined by another.
- **Check for updates:** whether to ask the winget community repository once a day for a newer release.
- **Diagnostic log:** whether Cader also writes its diagnostic log to a file, named on the Diagnostic log file row; Enter turns it on or off at once. It is off until you turn it on.

Under the Setup heading the grey rows are read, not edited. **Claude folder** names the folder Cader reads Claude Code's transcripts and settings from, `~\.claude` unless Cader was started with `CLAUDE_CONFIG_DIR` set to a full path (the row says when it came from there, and when the value was not a full path, such as a relative or `~` one, and so was not used); the Claude bell row and `cader setup-terminal` read and write that folder's `settings.json`. **Settings file** and **Diagnostic log file** show the full path of `config.json` and of `%LOCALAPPDATA%\Cader\cader.log` as plain text you can select and copy from the terminal; the log row also says whether the log is on or off and the file's size (KB below 1 MB, then MB to one decimal), or that nothing has been written to it yet. When `cader.log` reaches 10 MB it becomes `cader.log.1` and a new one starts; while that older file exists a **Diagnostic log file (older)** row after the log row shows its size and path. Every row answers `c` (copy the path) and `f`: on the Settings file and both diagnostic log file rows `f` opens the file itself in the editor chosen above (a log that does not exist yet is reported, and Diagnostic log turns it on), and on any other row it opens the folder. `d` on either diagnostic log file row clears the log: it asks first, naming each file and its size, then deletes `cader.log` and `cader.log.1` and says how much went. The events a problem report carries are kept, and while Diagnostic log is on a new `cader.log` starts with the next event; a file another program has open is kept, and the status line says to try again.

### Accounts

Cader reads who is logged in to Claude Code with `claude auth status` when it starts and then at most once a minute, and remembers each account it sees in `%LOCALAPPDATA%\Cader\accounts.json` under a short label: `1` for the first, `2` for the next, and so on. Each session's transcript records the account it was started under, which the Sessions tab's ACCOUNT column shows by that label.

The Config tab's Accounts rows are the legend: each shows the label, the account's email and organisation, and logged in now for the current one. Enter on a row renames its label (1 to 8 characters, and no two accounts may share one in any case), for example to `work` or `personal`.

- **Two logins in one organisation:** Claude Code names only the organisation of the current login, so two seats in the same Team or Enterprise organisation share one label.
- **Not seen yet:** a session whose account Cader has never seen logged in shows `?`; log in as that account once (`/login` in any Claude Code session) and Cader names it within a minute.
- **Forgetting an account:** edit or delete `accounts.json` while Cader is closed.

## Notifications and profiles

`cader setup-terminal` is the one step that sets up both bells and the profiles:

- **The Cader profile:** a Windows Terminal profile that starts Cader, with its own bell and icon. Choose it from the new-tab menu, or make it Windows Terminal's startup profile to have Cader open every time.
- **The Cader session profile:** the profile every session tab opens with, with a different bell, so you can tell Cader's bell from a session's.
- **Claude Code's terminal bell:** it sets `preferredNotifChannel` to `terminal_bell` in `~\.claude\settings.json`, so a session's tab rings when it finishes or pauses for permission while you are away.
- **Cader's own bell:** the console's tab rings when a background session starts waiting for you, and its row turns amber.

Restart Windows Terminal afterwards. It shows what it will write and asks first; `--sound` and `--session-sound` choose your own `.wav` files, `--yes` writes without asking and `--remove` removes the profiles and the entries setup added to Terminal's settings file. There are no toasts and no Claude Code hooks.

It leaves Windows Terminal's own settings file alone, with one exception: if you deleted a Cader profile in Windows Terminal's settings, Terminal keeps it hidden however often the profiles are written, so `setup-terminal` shows the one-line entry that brings it back and, if you agree, adds it to Terminal's profile list, leaving everything else in the file, comments included, as it was. Terminal shows the profile again as soon as it reads the file, with no restart.

Before it changes Terminal's settings file, setup keeps a copy of it as it was beside it, named `settings.json.<date>T<time>.cader.backup` with the UTC time, and names the copy; copy it back over `settings.json` to undo the change. It writes nothing, and says why, when:

- **The file is a link or read-only:** as a dotfiles set-up or a locked-down machine makes it; setup gives the entries to add or take out by hand.
- **Terminal changed the file meanwhile:** most likely you saved Terminal's settings while setup was writing; run the command again.
- **The new file does not check out:** setup reads the new file back first, and its profile list was not exactly the old one with the entries added or taken out, as happens when the file names `profiles` twice.

`--remove` also takes out of Terminal's settings file each entry setup added, after showing them and asking (`--yes` answers for you), and only an entry that still has exactly the four properties setup wrote: if you customised the profile in Terminal, its entry has more and is left alone.

## Every key

The key bar shows the keys for the selected row. Left and Right switch tabs on every tab, `<` and `>` scroll a table too wide for the terminal, F5 refreshes the shown tab now (Sessions lists the sessions, Worktrees rescans, Config rereads the setup; nothing starts while that tab is already refreshing, and the help lists it, not the key bar), and `U` (update) and `!` (report a problem) work everywhere.

| Key | Sessions | Worktrees | Config |
| --- | --- | --- | --- |
| Enter | Open in its own tab, or switch to it | Open its session's tab | Edit the setting, or say how to set up |
| n | New session | New session in this worktree | |
| r | Rename (restarts the session) | | |
| x | Stop | | |
| d | Delete, and its worktree if nothing is lost | Remove, or delete a branch row's branch (refused when work would be lost) | |
| D | | Force remove, or delete a branch row's branch, after typing its name | |
| R | Resume every stopped or failed session shown | | |
| P | | Prune stale registrations | |
| C | Close tabs of unused terminal sessions | | |
| b | Adopt a terminal session | | |
| o | | Open a PowerShell tab there | |
| m | What its memory is made of | | |
| c | Copy the folder path | Copy the folder path | Copy the path |
| f | Open the folder in your editor | Open the folder in your editor | Open the folder, or the settings or log file |
| s | Open its solution | Open its solution | |
| p | Open its pull request | Open its pull request | |
| / | Filter | Filter | Filter |
| Esc | Clear the filter | Clear the filter, or back to sessions | Clear the filter, or back to sessions |
| ? | Help | Help | Help |
| q | Quit; sessions keep running | Quit | Quit |

## Privacy

It sends nothing but a daily, anonymous check of the winget community repository on GitHub for a newer release, and of that release's changelog (turn it off with Check for updates), and the problem reports you choose to submit. It stores no credential and records nothing personal: its diagnostics are numbers, durations, versions and names of its own, never a path, a prompt or program output. The accounts it remembers (email, organisation and label) stay in `accounts.json` on your machine and never reach the diagnostic log or a problem report.

## Troubleshooting

- **Report a problem:** `!` previews the diagnostics it would send, then opens a GitHub issue with them filled in; nothing is sent until you submit it. A report holds the latest diagnostic events kept in memory (whether or not Diagnostic log is on), the last crash and the counts: the child programs, the refreshes, the stalls and how many handles and how much private memory Cader holds now. Cader never attaches cader.log to a report. cader.log is a separate, longer history that rolls at 10 MiB: open it with f on the Diagnostic log file row (Config tab), then give it to Claude or attach it to the issue by hand.
- **After a crash:** `%LOCALAPPDATA%\Cader\last-crash.log` is written; the next problem report includes it.
- **A diagnostic log:** turn on Diagnostic log on the Config tab to keep one in `%LOCALAPPDATA%\Cader\cader.log`; the Diagnostic log file row shows its path and size, `f` opens it, and `d` clears it and `cader.log.1` when you want a log that holds only the problem you are about to reproduce. Every 10 minutes, and when Cader exits, it also writes its counts, ending with a process now line that gives the handles and private memory Cader holds at that moment, so a console that grows over a day shows it between two marks.
- **The Cader profiles do not appear after `cader setup-terminal`:** they were deleted in Windows Terminal's settings, and Terminal keeps a deleted profile hidden. The Config tab's Windows Terminal profiles row says so; run `cader setup-terminal` again and it offers to show them.
- **Monochrome tabs:** Cader was started inside a Claude Code session; start it from an ordinary tab.
- **"Workspace not trusted":** Claude Code asks for trust once per repository; Cader opens a tab in that folder so you can accept it, then start the session again.
- **Security:** to report a vulnerability, see [SECURITY.md](../SECURITY.md).
