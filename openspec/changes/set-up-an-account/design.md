# Design

## Context

- After `accounts-in-the-console`: `Domain\Accounts`, `AccountName`, `ClaudeAccount`, the `accounts` and `rootAccounts` entries in `config.json` through `CurrentSettings`, and `TabScripts`' account line all exist.
- `cader setup-terminal` is the pattern: a `TerminalSetupRunner` in `Application\TerminalSetup` over ports, reading from a `TextReader` and writing to `TextWriter`s, one question per write, `--yes` and `--remove`, exit 0, 1 or 2. Its `ConfirmedAsync` and `TryWriteAsync` shapes are reused.
- The Claude Code docs support what the modes rely on: `@path` imports in `CLAUDE.md` (absolute or `~`, up to four hops, ignored inside code); `rules\` following symbolic links to folders and files, a link outside the project counting as an external import that may ask for approval once; a `skills\<name>` that is a link, loaded once however many places point at it, one link per skill. They describe symbolic links, not junctions.
- Windows directory junctions need no administrator rights or Developer Mode; directory symbolic links need Developer Mode (on since 3 November 2026 on the owner's machine, off by default for most people). Junctions only point at folders, so a single rule file cannot be one.

## Goals / Non-Goals

**Goals:**
- The person always knows, before the question, exactly what will change and what will not.
- The default account is changed only by a Shared folder start, only after a backup, and can be put back.
- Every write is recorded so `--remove` can undo precisely that and nothing else.

**Non-Goals:**
- Splitting one `CLAUDE.md` into shared and private parts: the person edits it afterwards.
- Watching or repairing links the person changes later.

## Decisions

### 1. Verify junctions and imports first, with a fallback chosen now

Task group 1 (owner, a logged-in second account): does Claude Code read `rules\<junction>\*.md` and `skills\<junction>\SKILL.md` through a junction; does an `@~/.claude/CLAUDE.md` or `@~/.claude-shared/CLAUDE.md` import load, and does it ask for approval; does `/memory` open the imported file; does removing a junction with `Directory.Delete(path, recursive: false)` leave its target whole; does Claude Code rewrite or follow a junction when it updates a skill or rule; and does Windows Terminal 1.25 load two JSON files from one fragment folder (decision 8). Recorded with dates and versions in `claude-code.md` and `windows.md`.

If Claude Code does not follow junctions, Link to default and Shared folder use directory symbolic links, offered only when Windows lets the person make one (Developer Mode or elevation), and otherwise listed but refused with a sentence saying why and that Copy works without it. The spec does not change: it says "junction" for the link Cader makes, and the wording changes with the fact.

### 2. One planner, a plan of steps, one runner

`Application\AccountSetup\AccountSetupPlanner` turns (name, start, chosen items, roots, what exists on disc) into an `AccountSetupPlan`: an ordered list of `SetupStep` cases (`MakeFolder`, `Copy`, `Move`, `Backup`, `MakeLink`, `AddImport`, `RegisterAccount`, `GiveRoot`, `WriteProfile`, `OfferBell`, `OpenLoginTab`), each knowing whether it is already done. The preview is `Wording` over the plan; the runner executes the steps in order, records each as it succeeds, and stops at the first failure with what was done and what was not. The same planner serves `accounts-in-the-config-tab`'s dialog.

- **Alternative:** a runner that decides and writes as it goes, as `TerminalSetupRunner` does. With up to a dozen kinds of step and four starts, a pure plan is what makes "nothing is written before the preview" and "re-running is safe" testable in memory.

### 3. The modes in detail

| Start | Default account | New account | Edits shared |
|---|---|---|---|
| Clean | untouched | empty folder | no |
| Copy | untouched (read only) | copies of chosen items | no |
| Link to default | untouched | `CLAUDE.md` importing `@~/.claude/CLAUDE.md`; `rules\default` junction to the default rules folder (all rules or none, as one link); one junction per chosen skill | yes |
| Shared folder | backed up; chosen rule files and skills moved to `.claude-shared`; `rules\shared` and skill junctions; import at top of its `CLAUDE.md` | the same junctions and import | yes |

For Shared folder's `CLAUDE.md` the person chooses: move the default's whole text into the shared file (the default's file becomes the import line alone), or start the shared file empty (the default's file keeps its text under the new import). A default account with no `CLAUDE.md` is offered neither. `--yes` always takes the second, which moves no text, and never answers yes to `--remove`'s restore question: both are the smallest change available.

### 4. Moves are one item at a time, link straight after

Each Shared folder move is `Move` then `MakeLink` for that item before the next, so an interruption leaves at most one item moved without its link, which the record names and a re-run completes. Moves stay on the same volume (`%USERPROFILE%`), so they are renames, not copies.

### 5. The record

`%LOCALAPPDATA%\Cader\accounts\<name>.json` (through the state store, so `stricter-state-files`' rules apply) holds the start, the backup folder, and every link (path and target) and import line (file and exact text) Cader made. `--remove` acts only on what the record lists and what is still as recorded; an account with no record (registered from an existing folder) only loses its profile and `config.json` entries.

### 6. The import line

`@~/.claude/CLAUDE.md` or `@~/.claude-shared/CLAUDE.md` as the first line, followed by one blank line, in the file's own line ending (the first line ending found, LF for a new file), never inside a code fence. Removal deletes that line and the blank line after it only when they are still exactly as recorded.

### 7. Paths are checked in Domain before any port sees them

`Domain\ClaudeFolderPaths` holds the four allowed roots (`.claude`, `.claude-<name>`, `.claude-shared`, Cader's state folder) and the never-touched names (`.credentials.json`, `.claude.json`, `settings.json` except through `IClaudeSettings`, `projects`, `sessions`, `plugins`, `history.jsonl`, `todos`, `statsig`, `shell-snapshots`, `jobs`, `daemon`). The planner asks it for every step, so a path outside is a plan that cannot be built, not a runtime check in an adapter. An item that is already a link to elsewhere is offered to link, never to move.

### 8. Account profiles get a fragment file of their own

`WindowsTerminal` adds `--profile "Cader session"` to every tab while `cader.json` exists, so a `setup-account` that wrote the Cader profiles would quietly change how every later tab opens, before Windows Terminal had even been restarted to load the profile (`windows.md` records that case as unverified). Instead the account profiles go in `Fragments\Cader\accounts.json`, written whole from the configured accounts by `setup-account` alone; `setup-terminal` writes only `cader.json`, and `--remove` still deletes the folder with both. Task 1.1 checks that Windows Terminal loads two files from one fragment folder; if it does not, the account file moves to a folder of its own, `Fragments\Cader accounts\`, and `setup-terminal --remove` removes that too.

Each account profile's command line is `pwsh -NoExit -EncodedCommand` over a `TabScripts` script holding the account line, so the folder is quoted as every tab value is. The login tab does not use the profile (it is not loaded until Windows Terminal restarts); it is an ordinary `wt new-tab` with the account line, as every account's tab is after `accounts-in-the-console`.

### 9. The record decides what is Cader's

A folder with a setup record is Cader's to carry on with, whatever Claude Code has written in it since (a login, settings, projects); a folder without one that holds anything is someone else's and is only registered. This is what lets a re-run finish an interrupted Shared folder move (decision 4) without ever writing into a folder set up by hand.

### 10. `default` is always `%USERPROFILE%\.claude`

Imports and links name `~/.claude`, so the account they read from must be that folder, not whatever `CLAUDE_CONFIG_DIR` the shell running the command has. The command ignores the variable for its source and for the `default` entry, and says so when it is set.

### 11. Junctions are an adapter of their own

`IJunctions` (`Make(link, target)`, `TryReadTarget(link, out FolderPath)`, `RemoveLink(link, expectedTarget)`) is held only by the setup and removal runners (a `LayeringShould` fact, like `IFolderDeleter`). `RemoveLink` checks the reparse point is a junction to the expected target and removes it without recursion; anything else answers `NotOurs`.

## Risks / Trade-offs

- [Claude Code does not follow junctions] → decision 1's fallback; Copy and Clean never need links.
- [A person edits a shared skill expecting only one account to change] → the preview and the guide say edits are shared for Link and Shared; Copy is offered for the other wish.
- [An older PowerShell `Remove-Item -Recurse` on a junction deletes the target's contents] → the guide says to remove an account with `--remove`, and to delete a junction in Explorer or with `cmd /c rmdir`, never `Remove-Item -Recurse`.
- [Shared folder moves files under a running session of the default account] → the preview counts the default account's running sessions and advises stopping them first; it does not refuse, because the files stay readable through the junctions.
- [The first session asks to approve an external import] → task 1 records it and the preview says so when it happens.
- [The default account is renamed `default` in `config.json`] → only the ACCOUNT column shows it, and `accounts-in-the-config-tab` lets the person rename it.

## Migration Plan

Nothing to migrate. An account set up by hand before this change is registered by running the command for its name (the existing-folder rule).
