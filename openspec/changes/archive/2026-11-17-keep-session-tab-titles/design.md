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
   - **Result (17 November 2026, CLI 2.1.326):** neither worked, and a third route did. UI Automation read a plain title throughout even without the variable (the control did not reproduce the 16 November glyph, with the workstation apparently locked), so the evidence is `claude attach` captured in the headless terminal's pseudo-console: the control writes about thirty `OSC 0;◐ <name>` and `OSC 0;◑ <name>` titles; the variable on the attach, on the `claude --bg` start, or on both leaves them all. A session asked to print its own environment read a variable set by the `claude --bg` client as unset, so a background session's environment is its supervisor's, and `Variables` on a request never reaches it. `claude --bg --settings '{"env":{"CLAUDE_CODE_DISABLE_TERMINAL_TITLE":"1"}}'` leaves no glyph title in the capture, on a start and on a resume.
2. **Set it where it works, and nowhere else: as settings on the session's start and resume.** `ClaudeCli` adds the constant pair `--settings` and `{"env":{"CLAUDE_CODE_DISABLE_TERMINAL_TITLE":"1"}}` to `--bg` and to `--bg --resume`, after the existing flags and before an opening prompt. The JSON is a constant with no user text, `--settings` applies to that session alone, and Cader writes nothing to the user's `settings.json`. The session then skips the small-model title request too, which Cader's named sessions do not need.
   - *Rejected: the variable in the attach script* (the original 2.1). It changes only the attach client's status report, not the title.
   - *Rejected: `Variables` on the start and resume requests* (the original 2.2). The client's environment never reaches the session.
   - *Rejected: `terminalTitleFromRename` in settings.* It covers only `/rename` and `--name`, not the status glyph, and Cader never writes the user's settings for a preference.
3. **Keep `TabTitle.Shows`.** Tabs opened before this change, and sessions started by an older Cader, still carry the glyph until they are reopened.

## Risks / Trade-offs

- **[Risk] The variable also turns off status reports to the terminal.** → Cader reads session states from `claude agents --json`, never from the terminal, and the bell comes from `preferredNotifChannel`, a separate setting; the manual check confirms the bell still rings.
- **[Risk] A later CLI changes how `--settings` reaches a background session.** → The nightly contract fact starts a session through `ClaudeCli`, so a refused flag fails there; a silent change brings the glyph back, which #326's rule still finds.
- **[Risk] A later CLI renames or drops the variable.** → The glyph then returns and #326's rule still finds the tab; `claude-code.md` records the version it was checked with.
- **[Trade-off] Claude Code's generated session titles stop for sessions that carry it.** Cader names every session it starts, so nothing it shows depends on them.
