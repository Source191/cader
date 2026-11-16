# Proposal

## Why

Enter on a session whose tab is already open fails whenever that session is working: the status line says the tab "is open in another tab, but it could not be found. Is it in another window?", although the tab sits in the same window. While a session works, Claude Code 2.1.325 puts a spinning status glyph and a space before its tab's title (`◐ c-finish-proposals`, then `◑`, `✳`), and Cader looks for a tab named exactly the session name, so it finds none. An idle session's tab keeps its plain title and still switches. Cader opens session tabs with `--suppressApplicationTitle`, which on 16 November 2026, at Windows Terminal 1.25.3131.0, still blocked OSC 0, OSC 1, OSC 2 and `SetConsoleTitle` from a probe in a tab opened with Cader's exact arguments, so how Claude Code's glyph gets through is not yet known; the switch must work either way.

## What Changes

- Enter on a session whose Cader-opened tab is alive finds that tab when its title is the session's name or the session's name after one status glyph and a space, as Claude Code writes it while the session works.
- A tab whose title only contains the session's name elsewhere, or has more than one character before it, is still not that session's tab.
- The probe results go in `windows.md`, and the observed glyph titles in `claude-code.md`.

Out of scope:
- **Stopping Claude Code from changing the title:** its route through `--suppressApplicationTitle` is not known; finding it needs Claude's own output captured during an attach, a later change if the prefix ever stops being enough.
- **The other places Cader reads a tab by its title:** none; tab ownership is decided by process, never by title.
- **Multiple Claude accounts:** a later change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `session-attach`: the open-in-tab requirement's live-tab scenarios now find a tab whose title carries Claude Code's status glyph before the session name.

## Impact

- **`src\Source191.Cader.Core\Domain`:** one new pure rule for whether a tab's title shows a session's name.
- **`src\Source191.Cader.Infrastructure\Terminal\UiAutomationTabs.cs`:** lists the window's tab items and applies the rule, instead of asking UI Automation for an exact name.
- **Tests:** the rule's facts in `Core.Tests`; `UiAutomationTabsShould` keeps its repeated searches.
- **`.claude\rules\windows.md` and `claude-code.md`:** the probe and the glyph titles, dated, with versions.
- **Version:** `src\Source191.Cader\Source191.Cader.csproj` raises its `Version`; `CHANGELOG.md` gets a Fixed entry.
- **Manual check:** Enter on a working session whose tab is open switches to it.
- **Also in this pull request:** the `dispose-tidy` change is archived, with its manual tab check recorded in `windows.md`.
