---
paths:
  - "**/*Claude*.cs"
  - "**/*Session*Handler*.cs"
  - "**/AutomatedRun*.cs"
  - "**/ITranscripts.cs"
  - "**/TabScripts*.cs"
  - "**/AccountCatalogue*.cs"
  - "src/Source191.Cader.Infrastructure/Claude/**"
  - "src/Source191.Cader.Core/Application/Launch/**"
  - "test/Source191.Cader.Infrastructure.Tests/Contract/**"
  - ".github/workflows/claude-contract.yml"
  - "openspec/specs/session-*/**"
---

# Claude Code CLI facts (verified 2026-10-29, CLI 2.1.310)

Re-verify these, and move the date and version above, whenever a change accepts new help snapshots or raises `ClaudeCodeSupport.Minimum` (the settings row's below-minimum warning, "2.1.309 or later" in the README and `openspec\config.yaml`, kept in step by `DocumentedMinimumShould`). The captures and probes behind these are in `docs\verified-facts\claude-code.md`.

## Listing

- **Version:** `claude --version` prints `2.1.310 (Claude Code)`; `IClaudeCli.VersionAsync` takes the leading dotted number (`NotRead` on failure) once per run for the settings row.
- **`claude agents --json [--all]`** is the scripting surface. Fields: pid, id, cwd, kind (interactive or background), startedAt, sessionId, name, status (busy, idle, waiting), state (working, blocked, done, failed, stopped), waitingFor. The job `id` is the first 8 characters of `sessionId`. A stopped session is a row without pid or status.
- **Automated runs:** an interactive row with a pid and no `status` is an automated run (`Domain\AutomatedRun`, sorted out in `ClaudeAgentsListing`), never a session row: the Agent SDK (the security-guidance plugin's background review is one) and `claude -p` are listed that way for their whole lives.
- **A terminal `claude`** registers without a status and has `idle` about 130 ms later (2026-10-30, CLI 2.1.314). `claude logs <id>` never registers. Automated runs' count and memory end the Sessions counts (`◇ 1 automated 216 MB`); how long a `claude -p` stays listed is in `checks.md`.
- **Names:** the CLI names a session started without `-n` after its folder (`repos-93`, 2026-10-23, 2.1.309; `Northwind-9d` for `claude -p`, 2026-10-30, 2.1.314), so an unnamed session row has not been seen. `ListedName.Unnamed` is a shape the JSON allows (a missing or blank `name`), shown blank through `Wording.Name` and resumed with no `-n`.

## Starting, attaching and resuming

- **`claude --bg -n <name> [prompt]`** starts a supervised session; with no prompt it waits at its own prompt. `attach` accepts a name; `stop` and `rm` need the id. Attaching to a session with no messages lands at its prompt, and Ctrl+Z detaches as from any conversation.
- **No end-of-options marker:** `claude attach -- --help` answers `unknown option '--'` (2026-10-29), so a prompt starting with `-` would be read as options and Cader refuses it. The npm package installs the same native binary as the installer; Cader never runs a `claude.cmd`.
- **Transcripts:** `~\.claude\projects\<cwd-encoded>\<sessionId>.jsonl` is written on the first message. Resuming one with no transcript fails with "source session not found", so adopt (`b`), rename (`r`), open-in-tab (Enter) and resume all check `ITranscripts` first.
- **A transcript is not proof of messages** (2026-11-16, CLI 2.1.325): `/bg` starts a job under a new session id whose transcript holds only an `ai-title` and an `agent-name` line until its next turn, and resuming that id fails. So `ClaudeTranscripts` counts messages only from lines whose top-level `type` is `user` or `assistant`, and reads nothing else but the first `session-owner` line's `organizationUuid`.
- **Adopting:** `claude --bg --resume <sessionId> -n <name>` continues a *non-background* transcript under the supervisor with the same id (how `b` adopts a terminal session after ending its process).
- **A listed background job resumed with flags starts a copy** ([the docs](https://code.claude.com/docs/en/agent-view): "keeps its own saved options"). `claude rm <id>` first (the transcript stays) and the resume then continues the same id under the new name, which is how `r` renames.
- **Stopped and failed jobs** are copied the same way (2026-11-01, CLI 2.1.316), so `R` removes each first too. A job just started or resumed is listed with a pid and no status for a moment, shown as starting; on a runner a first listing had neither (2026-11-02), so the contract fact polls until every field is there.
- **Never run `claude --bg --resume --help`:** it starts a session.
- **Interactive sessions** register in `~\.claude\sessions\<pid>.json` and cannot be attached, stopped or removed from outside. Windows Terminal closes a tab when its root shell exits 0 and keeps it after a kill, so `d` and `C` end claude and then the tab's shell cleanly (`TerminalTab`, `IProcessRegistry.EndCleanly`).
- **On detach** Claude leaves focus reporting on and the cursor hidden and sends a late terminal-version query; `ConsoleRecovery` resets these after every child (only `cader here` runs one now).

## Files Cader watches but never reads (seen 18 November 2026, CLI 2.1.327)

- `<Claude folder>\sessions\<pid>.json` is one file per running session, background ones included, rewritten when its status changes; `<pid>.<hash>.key` files are written beside it at start.
- `<Claude folder>\jobs\<id>\state.json` is one per background job, written as `state.json.tmp.<hex>` then renamed over it, so a watcher sees Created and Changed on the temporary name and Deleted and Renamed on `state.json`. A listing writes nothing for any job but its own.
- Both formats are undocumented, so Cader never opens these files: `Application\ChangeFilter` takes a changed name only as a hint to run the documented listing, and the timers stay as the safety net.

## Trust, notifications and titles

- **Workspace trust** is per repository (linked worktrees inherit it), granted only interactively; `claude --bg` in an untrusted folder exits with "Workspace not trusted".
- **Never script edits to the trust records on a developer machine.** The exceptions are the CI `handoff` job and `claude-contract.yml`, which seed `projects.<path>.hasTrustDialogAccepted` in the throwaway runner's `~\.claude.json` (keys are forward-slash paths with a capital drive letter, `D:/a/cader/cader`).
- **Background sessions** do not commit or push on their own (bgIsolation none, sibling worktrees). permission_prompt and idle_prompt notifications fire unattended; agent_needs_input and agent_completed only while the native agent view is open, which is why Cader rings its own bell for background rows.
- **The bell:** `"preferredNotifChannel": "terminal_bell"` in settings.json ([terminal configuration](https://code.claude.com/docs/en/terminal-config)) rings the bell when Claude finishes or pauses for a permission prompt while the user seems away. Claude Code writes `settings.json` with bare LF; anything that rewrites it must keep that.
- **Tab titles** (2026-11-16, CLI 2.1.325): a working session's attach tab reads a spinning glyph, a space and its name (`◐ <name>`); an idle one the name alone. The glyph reaches a tab opened with `--suppressApplicationTitle` (`windows.md`), so Enter finds tabs through `Domain\TabTitle.Shows`.
- **Turning the glyph off** (2026-11-17, CLI 2.1.326): `CLAUDE_CODE_DISABLE_TERMINAL_TITLE=1` on the `claude attach` or `claude --bg` client changes nothing, because **a background session's environment is its supervisor's, never the client's**.
- What reaches the session is `claude --bg --settings '{"env":{"CLAUDE_CODE_DISABLE_TERMINAL_TITLE":"1"}}'`, also on `--bg --resume <id> --settings ...` after `claude rm`. So `ClaudeCli` adds that pair to every start and resume. The owner's manual check of a real tab is still due.

## Accounts (checked 18 November 2026, CLI 2.1.327, field names only)

- **`claude auth status --json`** (about 1.2 s) answers `loggedIn`, `authMethod`, `apiProvider`, `email`, `orgId`, `orgName`, `subscriptionType`, `planTier`, `configDirectory`, `projectsDirectory`, `loginExpiresAt` and a few flags, with no account id. It exits 0 only when `loggedIn` is true and 1 otherwise, printing the JSON either way.
- **Session owners:** from CLI 2.1.325 a transcript carries `{"type":"session-owner","accountUuid":…,"organizationUuid":…,"sessionId":…}`, usually early and repeated; none seen names two owners. `ClaudeTranscripts.OwnerOf` reads only the first such line's `organizationUuid`, and `AccountCatalogue` matches it to `orgId`, so two seats in one organisation share a label.
- **No transcript yet:** a `--bg` session with no prompt wrote no transcript in its first eight seconds, so the contract fact `ReadTheLoginFromAuthStatus` checks `auth status` alone (an owner line would need a model call).
- **A token login:** with an unverified `CLAUDE_CODE_OAUTH_TOKEN` it answers `loggedIn: true`, `authMethod: oauth_token`, no `email` or `orgId`, exit 0. Cader's children never see that variable (`InheritedEnvironment` drops every `CLAUDE_CODE_*`), so on the contract runner the fact reads the logged-out answer (issue #338).

## CLAUDE_CONFIG_DIR (checked 2026-11-14, CLI 2.1.325)

- A full path is used as given (a trailing `\` makes no difference) and a missing folder is created; a relative value is read against the CLI's own working directory; `~` is NOT expanded.
- So Cader treats a value as usable only when it is a full path: no one folder holds every session's files otherwise.
