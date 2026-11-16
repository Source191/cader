# Design

## Context

- **The tab script:** `TabScripts.Attach` builds the attach tab's PowerShell: claim the registration, `Clear-Host`, `claude attach <job id>`, exit on 0. Every value goes through `PowerShellScript.Quote`; the script reaches `wt.exe` Base64-encoded.
- **The session's own process:** background sessions are started and resumed by `ClaudeCli` through `IProcessRunner` (`claude --bg -n <name>`, `claude --bg --resume`), whose requests can carry `Variables`, applied after `InheritedEnvironment` drops every `CLAUDE_CODE_*` the console inherited (`ProcessRunner.StartInfo`).
- **What Claude Code documents** (code.claude.com, read 16 November 2026): `CLAUDE_CODE_DISABLE_TERMINAL_TITLE` "Set to `1` to disable automatic terminal title updates based on conversation context"; it also skips the small-model title request and turns off status reports to the terminal; in a terminal that does not support the OSC 7501 Program Status Protocol, Claude Code puts a status glyph in front of the title. Which process writes the title for an attached background session, the session or the attach client, is not documented.
- **What was seen** (`claude-code.md`, 16 November 2026, CLI 2.1.325): a working session's Cader tab read `◐ <name>`, an idle one the name alone.

## Goals / Non-Goals

**Goals:**
- A session's Cader tab keeps the session's name as its title, working or idle.
- The variable reaches only the processes that need it, with no user text in the script.

**Non-Goals:**
- Changing titles in tabs Cader did not open, or dropping #326's tolerance.

## Decisions

1. **First find which process writes the title, by trying the attach alone.** Set `$env:CLAUDE_CODE_DISABLE_TERMINAL_TITLE = '1'` in a hand-run attach of a working throwaway session and read the tab's name through UI Automation, as the 16 November probes did; if the glyph remains, start a throwaway session with the variable and attach without it. The result decides decision 2 and is recorded in `claude-code.md`.
2. **Set it where it works, and nowhere else.**
   - **The attach is enough:** `TabScripts.Attach` adds the assignment as its own line before the attach. It is a constant, so it needs no quoting, and it affects only that tab's shell.
   - **The session writes the title:** the start and resume requests in `ClaudeCli` carry it in `Variables`, as the fetch carries `GIT_TERMINAL_PROMPT=0`. The session then skips the small-model title request too, which Cader's named sessions do not need.
   - *Rejected: both always.* Each place it is set without effect is a line to explain and test.
   - *Rejected: `terminalTitleFromRename` in settings.* It covers only `/rename` and `--name`, not the status glyph, and Cader never writes the user's settings for a preference.
3. **Keep `TabTitle.Shows`.** Tabs opened before this change, and sessions started by an older Cader, still carry the glyph until they are reopened.

## Risks / Trade-offs

- **[Risk] The variable also turns off status reports to the terminal.** → Cader reads session states from `claude agents --json`, never from the terminal, and the bell comes from `preferredNotifChannel`, a separate setting; the manual check confirms the bell still rings.
- **[Risk] A later CLI renames or drops the variable.** → The glyph then returns and #326's rule still finds the tab; `claude-code.md` records the version it was checked with.
- **[Trade-off] Claude Code's generated session titles stop for sessions that carry it.** Cader names every session it starts, so nothing it shows depends on them.
