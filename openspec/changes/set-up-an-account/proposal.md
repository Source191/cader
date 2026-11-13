# Proposal

## Why

After `accounts-in-the-console`, Cader drives several Claude accounts, but each one still has to be made by hand: a folder, a `CLAUDE_CONFIG_DIR` in the right shell, the `accounts` entry in `config.json`, a login, the bell. Issue #203 asks for one command that does this. The owner's worry is that even Anthropic's documented set-up is unusual, so the command must make each choice plain, write nothing the person did not agree to, and be undoable.

The owner's first design (2 November 2026) moved the default account's rules and skills into a shared folder. That changes the default account, which many people will not want, so the command now lets the person choose how the new account starts, from leaving everything untouched to that shared folder, and says in plain words what each choice touches.

## What Changes

- **`cader setup-account <name>`:** makes `%USERPROFILE%\.claude-<name>`, adds it to `config.json`'s `accounts` (with the existing folder as `default` when there is no list yet), and walks the person through the rest. `--yes` answers yes, `--mode` chooses the start, `--root <folder>` (repeatable) gives roots to the account, `--remove` undoes it.
- **Four ways to start, chosen by the person,** each described before it is chosen:
  - **Clean:** an empty account, like a fresh Claude Code install. Nothing outside the new folder is touched. The default for `--yes` with no `--mode`.
  - **Copy:** copies of the chosen `CLAUDE.md`, rule files and skills. Both accounts stay standard; later edits are not shared.
  - **Link to default:** the new `CLAUDE.md` imports the default one, and junctions point at the default account's rules and chosen skills. The default account is untouched; edits are shared.
  - **Shared folder:** chosen items move to `%USERPROFILE%\.claude-shared`, and both accounts import and link them. The default account changes, so everything is backed up first.
- **A preview before anything is written:** every folder, copy, move, edit, junction, profile and `config.json` change, then the question. It also says what is never shared (settings and hooks, plugins, MCP servers and trust, login, history) and that Console sign-ins without an API key are not separated by the folder.
- **An existing folder is never rewritten:** if `%USERPROFILE%\.claude-<name>` already exists with files in it (someone followed Anthropic's guide), the command only registers it.
- **To use the account outside Cader:** a "Claude (<name>)" Windows Terminal profile in Cader's own fragment that opens PowerShell with the account set, and the one-line PowerShell to do the same anywhere. Nothing in the person's `$PROFILE` or environment variables is changed.
- **Then:** a tab opens running `claude` as the new account for its first login, and the bell is offered in its `settings.json`.
- **Re-running is safe** (done steps are skipped), and **`--remove <name>`** takes out only what Cader added (profile, junctions, import lines, the `config.json` entries) and never deletes a Claude folder, a copy or a moved file; after a Shared folder start it offers to put the default account back as it was.
- **Recorded facts first:** whether Claude Code reads rules and skills through a junction, whether an import outside the project asks for approval, and what a junction's removal leaves, verified and recorded before the adapters are written.

## Capabilities

### New Capabilities

- `account-setup`: the `setup-account` command, its start modes, preview, records, undo and the safety rules for the files it touches.

### Modified Capabilities

- `command-line`: the help lists `setup-account`, and it explains itself.
- `needs-input-notification`: the fragment keeps each account's profile beside Cader's own, and `setup-terminal --remove` says it removes them too.

## Impact

- **Code:**
  - Core Domain: `AccountStart` (the four modes), `ShareableItem` (CLAUDE.md, a rule file, a skill folder), `ImportLine`, `AccountSetupPlan` and its steps, the path rule that keeps every target inside the person's Claude folders.
  - Core Ports: `IClaudeFolderFiles` (list, copy, move, read and write `CLAUDE.md`, back up), `IJunctions` (make, read target, remove the link only), the setup record store.
  - Core Application: `AccountSetup\AccountSetupRunner` and `AccountRemovalRunner`, the planner, `CommandLine.Build` (the command and options), `TabScripts.Login`, `TerminalProfiles` (account profiles), `CurrentSettings` writes, `Wording` and a new `WordingOfAccountSetup`.
  - Infrastructure: `ClaudeFolderFiles`, `Junctions` (Windows directory junctions), the record file in `%LOCALAPPDATA%\Cader\accounts`, `TerminalFragment`.
  - Tool project wiring: an `AccountSetupComposition` and its line in `Program.cs`.
- **Tests:** sociable runner tests over a fake file system and fake junctions; adapter tests for junction creation and removal in a temporary folder, including that removal never touches the target; never the real `%USERPROFILE%\.claude*`, `config.json` or Fragments folder.
- **Docs:** README "Several Claude accounts" gains the command; `docs\user-guide.md` a section per start mode with what it touches and how to undo it; `.claude\rules\security.md` (junctions, moves, backups and where they may point); `.claude\rules\claude-code.md` (the verified facts); `.claude\rules\windows.md` (junction facts); `CHANGELOG.md` with a minor `Version` raise.

## Delivery

- **Pull request:** PR 3 of 4 for issue #203, one pull request.
- **Order:** after `accounts-in-the-console` has merged, because the command writes the `accounts` and `rootAccounts` entries that change reads and relies on the console acting as each account.
- **Owner step first:** the junction and import spike in task group 1 needs a logged-in second account.
- **Implementation:** Opus 5.5 at high effort, with xhigh for the junction and move adapters (task groups 3 and 4) if the session allows a change of effort. It moves and edits files in the person's own Claude folders, where a mistake loses someone's instructions.

## Out of scope

- The Config tab dialog that runs the same steps (`accounts-in-the-config-tab`).
- Sharing `settings.json`, hooks, permissions, plugins, MCP servers, output styles, agents or keybindings: the preview says to copy them by hand if wanted.
- Editing the person's PowerShell `$PROFILE` or user environment variables.
- Deleting a Claude configuration folder, the shared folder, or anything Cader copied or moved.
- Keeping copies in step after a Copy start.
