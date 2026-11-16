# Proposal

## Why

A session's tab is meant to be titled with the session's name, and Cader opens it with `--suppressApplicationTitle` for that reason, yet while a session works Claude Code puts a spinning status glyph before the title (`◐ fix-123`). #326 made Enter find the tab anyway, but only for the one shape seen so far, one glyph and a space: if Claude Code changes it, Enter again reports an open tab it cannot find. Claude Code's documentation explains the glyph and how to turn it off: in a terminal that does not report support for its OSC 7501 status protocol, as Windows Terminal does not, it puts a status glyph in front of the title, and [`CLAUDE_CODE_DISABLE_TERMINAL_TITLE=1`](https://code.claude.com/docs/en/env-vars) disables its terminal title updates, its status reports to the terminal and the background request to a small model that writes a session title.

## What Changes

- The attach in a session's tab runs with `CLAUDE_CODE_DISABLE_TERMINAL_TITLE=1`, so the tab keeps the session's name as its title while the session works.
- If the attach alone does not stop the glyph (the session process, not the attach, may write the title), the sessions Cader starts and resumes get the variable too; the first task finds which.
- Enter keeps accepting a glyph before the name (#326), for tabs opened before this change and for any session the variable does not reach.

Out of scope:
- **Sessions Cader did not start or attach:** a session the user runs in their own terminal keeps Claude Code's titles.
- **Why the glyph passes `--suppressApplicationTitle`:** with the updates turned off at the source it no longer matters; `windows.md` keeps the probe.
- **Multiple Claude accounts:** a later change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `session-attach`: the open-in-tab requirement turns Claude Code's own title updates off in the tab, so its title stays the session's name while the session works.

## Impact

- **`Application\TabScripts.Attach`:** sets the variable for the attach, a constant with no user text in it; possibly `LaunchService` or the Claude CLI start and resume requests, through `ProcessRequest.Variables`, which `ProcessRunner` applies after the `InheritedEnvironment` drop.
- **Tests:** `TabScriptsShould` for the script line; a request-variables fact if the start and resume carry it.
- **`.claude\rules\claude-code.md`:** the documented variable and OSC 7501, and which process writes the title, dated, with the CLI version.
- **Version:** `src\Source191.Cader\Source191.Cader.csproj` raises its `Version`; `CHANGELOG.md` gets a Changed entry.
- **Manual check:** a working session's tab keeps its plain title, and its bell still rings.
- **Users:** session tabs keep their names; Claude Code no longer spends a small-model request writing a title for sessions Cader names.
