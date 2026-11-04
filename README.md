# Vantage

![Vantage: mission control for Claude Code sessions, every session in one tab, still there after a reboot. Beside it, the icon: two branches merging into one node, shaped as a V](https://raw.githubusercontent.com/Source191/vantage/main/assets/header/readme-banner.png)

[![Buy me a sliver of a Claude Max plan](https://img.shields.io/badge/Buy_me_a_sliver_of_a-Claude_Max_plan-FFDD00?logo=buymeacoffee&logoColor=black)](https://buymeacoffee.com/simonvane)

Vantage is mission control for Claude Code on Windows: one Windows Terminal tab that shows every session on the machine, so you no longer have to remember which tab is doing what or which one is waiting for you.

- **See every session at once:** each one is listed with its state, repository, branch and pull request.
- **Start in a fresh worktree:** a new session gets its own git worktree, so sessions never trip over each other's changes.
- **Open a session in its own tab:** Enter opens it in a coloured tab named after it, or switches to that tab when it is already open.
- **Hear when one needs you:** the terminal bell rings when a session finishes or pauses for you.
- **Keep working when the terminal closes:** sessions run under Claude Code's own background supervisor, so they carry on with no terminal showing them and are still listed after a reboot.
- **Tidy up with one key:** delete a session and its worktree together, unless that would lose uncommitted or unpushed work.

> **To do:** add a screenshot of the sessions view here before the repository is made public.

## Prerequisites

Vantage runs on Windows 10 or 11 and needs one program you install yourself: Claude Code 2.1.309 or later, with its native installer (winget's own Claude Code package can be older than Vantage needs, and does not update itself):

```powershell
irm https://claude.ai/install.ps1 | iex   # Claude Code
[Environment]::SetEnvironmentVariable('Path', [Environment]::GetEnvironmentVariable('Path', 'User') + ";$HOME\.local\bin", 'User')   # puts claude on the PATH
```

Open a new terminal, run `claude` once to sign in, then check with `claude --version`. Vantage carries its own .NET runtime, and installing it with winget brings anything else it uses that is missing.

Optional: the GitHub CLI (`gh auth login`) or the Azure CLI (`az extension add --name azure-devops`, `az login`) fills in the PR column.

## Install

```powershell
winget install Source191.Vantage
vantage setup-terminal   # optional but recommended: see Notifications below
```

winget installs Windows Terminal, PowerShell 7 and git first if any of them is missing. A shell that was open before the install needs restarting to find `vantage`. Vantage tells you when a newer release is out; press `U` to see what changed and upgrade it with winget in a tab of its own (or run `winget upgrade Source191.Vantage` with no Vantage open). `winget uninstall Source191.Vantage` removes it and leaves your settings and Claude Code sessions alone.

- **Without winget:** download `vantage_<version>_win-x64.zip` from the [latest release](https://github.com/Source191/vantage/releases/latest), check it with `gh attestation verify <zip> --repo Source191/vantage`, extract it to a folder of your own and add that folder to your PATH. Windows Terminal, PowerShell 7 and git are then yours to install, and `U` can only show what changed and the release page.
- **Previews:** each preview is a [GitHub prerelease](https://github.com/Source191/vantage/releases) with the same zip; extract it to any folder and run `vantage.exe` from there. It never replaces the winget install, and previews never reach winget.

## Quick start

1. **Start it:** run `vantage` in an ordinary Windows Terminal tab (not inside a Claude Code session, which turns colour off in the tabs it opens).
2. **Point it at your repositories:** it looks under `~\source\repos`. If yours live elsewhere, press Right twice for the Config tab and Enter on Repository roots.
3. **Start a session:** press `n`, choose a repository, type a name and, if you like, an opening prompt, then F5. The session gets its own worktree and opens in its own tab.
4. **Come back to it:** Enter on its row switches to its tab. A row turns amber, and the Vantage tab rings, when a session needs you.
5. **After a reboot:** sessions show as stopped; `R` resumes them all.
6. **When the work is done:** `d` deletes the session and the worktree Vantage made for it, unless that would lose work.

Already in a folder? `vantage here` starts a supervised session there on the current branch, in the same tab, and lists it in Vantage.

## Everyday keys

The key bar at the bottom shows the keys for the selected row, and `?` on any tab lists every key, column and detail line.

| Key | Action |
| --- | --- |
| Left, Right | Show the Sessions, Worktrees or Config tab |
| Enter | Open the session in its own tab, or switch to it |
| n | New session |
| x | Stop the session; it stays listed and can be resumed |
| d | Delete the session (or, on the Worktrees tab, remove the worktree) when nothing would be lost |
| R | Resume every stopped or failed session shown |
| / | Filter the rows |
| ? | Help |
| ! | Report a problem, with privacy-safe diagnostics filled in |
| q | Quit. Quitting Vantage never stops a session |

## Notifications

`vantage setup-terminal` sets up the bells: it adds two Windows Terminal profiles so Vantage's own bell and a session's bell sound different, and turns on Claude Code's terminal bell (`preferredNotifChannel` in `~\.claude\settings.json`), so a session's tab rings when it finishes or needs you. Restart Windows Terminal afterwards. `vantage setup-terminal --help` shows the options, including your own sounds and `--remove`.

## Learn more

- **In the tool:** `?` on any tab, and a tooltip on every column header and key; `vantage --help` for the commands.
- **What changed:** [CHANGELOG.md](https://github.com/Source191/vantage/blob/main/CHANGELOG.md).
- **Privacy:** Vantage sends nothing but a daily, anonymous check of the winget community repository on GitHub for a newer release, and of that release's changelog (turn it off on the Config tab), and the problem reports you choose to submit. It stores no credential and records nothing personal.
- **Security:** to report a vulnerability, see [SECURITY.md](https://github.com/Source191/vantage/blob/main/SECURITY.md).
- **Contributing:** [CONTRIBUTING.md](https://github.com/Source191/vantage/blob/main/CONTRIBUTING.md).

## Why I built it

I built Vantage because I wanted a tool that worked the way I work. Before starting it I tried [Orca](https://github.com/stablyai/orca) and [Kepler](https://gitkraken.com/kepler), and there is a lot I like about both of them. But I live in the terminal and I want to stay in the terminal, and both of them also do a great deal that I was not interested in. Perhaps my mind will change over time; for now I wanted something that fits how I use Claude Code and nothing more. By all means try the others, but give Vantage a go too 😊

## Support Vantage

Vantage is built in the gaps between rate limits. Help close the gaps: [buy me a sliver of a Claude Max plan](https://buymeacoffee.com/simonvane).

![QR code for buymeacoffee.com/simonvane](https://raw.githubusercontent.com/Source191/vantage/main/assets/support/buy-me-a-coffee-qr.png)
