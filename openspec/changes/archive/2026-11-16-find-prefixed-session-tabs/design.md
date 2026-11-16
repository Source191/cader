# Design

## Context

- **The switch:** `OpenTabHandler` finds a live registration for the session and calls `IWindowsTerminal.FocusTabAsync(window, Wording.Name(session.Name))`, which runs `UiAutomationTabs.Select` on a pool thread. `SelectIn` asks UI Automation for the first descendant of each window of the owning terminal that is a tab item whose name equals the title exactly (`CreateAndCondition` of control type and name), and selects it.
- **What the tab is called:** on 16 November 2026 (Windows Terminal 1.25.3131.0, Claude Code 2.1.325) UI Automation read a working session's Cader tab as `◐ c-finish-proposals`, then `◑ c-finish-proposals`, while an idle session's Cader tab read `c-mem-leak`. Claude Code's own tabs read `✳ <topic>` and `◑ <topic>`. Probes opened with Cader's exact `wt new-tab` arguments (profile, title, colour, `--suppressApplicationTitle`, folder) kept their title through OSC 0, OSC 1, OSC 2 and `SetConsoleTitle`, so the glyph arrives by a route those probes did not reproduce.
- **The rule's home:** `Domain` holds pure rules over plain values with facts in `Core.Tests\Domain` (`TabShade`, `TerminalTab`), which Stryker mutates; `UiAutomationTabs` has no tests of its matching today, only `UiAutomationTabsShould`'s repeated searches.

## Goals / Non-Goals

**Goals:**
- Enter switches to a session's live tab whether or not Claude Code has put its status glyph before the title.
- No other tab is ever taken for the session's: the name must be the whole title, or the whole title after exactly one glyph and a space.

**Non-Goals:**
- Finding or blocking the route by which Claude Code retitles a suppressed tab.
- Matching a renamed tab: a rename replaces the tab (session-attach), so its title is always the current name.

## Decisions

1. **A pure rule, `Domain\TabTitle.Shows(string tabName, string sessionTitle)`.** True when `tabName` equals `sessionTitle` ordinally, or when it is one Unicode scalar value (one `Rune`, so a glyph outside the Basic Multilingual Plane counts as one) that is neither a letter, a digit nor white space, then one space, then exactly `sessionTitle`. `Shows` answers false for a blank `sessionTitle`, which no session has but no rule should match.
   - *Rejected: a fixed list of Claude's glyphs (◐ ◓ ◑ ◒ ✳).* Claude Code chooses them and has changed them before; one symbol and a space is the shape, and the rule keeps the false-match surface as small as the list would.
   - *Rejected: "ends with the session's name".* It would take `notes on fix-123` for `fix-123`'s tab.
   - *Rejected: trimming any leading symbols.* `◐◑ fix-123` is not a shape Claude writes, and a looser rule is a wider target for another tab's title.
2. **`UiAutomationTabs` lists the tab items and applies the rule in code.** UI Automation's conditions match a whole property value, so `SelectIn` now finds all descendants that are tab items (the cached tab-item condition from dispose-tidy) in each owned window, reads each `CurrentName`, and selects the first whose name `TabTitle.Shows` the title. Every element obtained is kept in the call's `ComObjects` and released as before. A window holds tens of tabs at most, so listing them costs nothing noticeable on an Enter.
   - *Rejected: two searches, the exact name and then the glyph forms.* The glyph is not known in advance, so the second search would still have to list every tab.
3. **Tests.** `TabTitleShould` in `Core.Tests\Domain` states the rule: the exact name; each of ◐ ◓ ◑ ◒ ✳ before it; a glyph outside the Basic Multilingual Plane; and refusals for a different name, the name inside a longer title (`fix-1234`, `notes on fix-123`), two glyphs, a letter or digit as the "glyph", no space, two spaces, and a blank title. `UiAutomationTabsShould`'s two repeated searches stay; only the search of every terminal reaches the tab listing (the test process owns no window), and only where a Windows Terminal window is open, so on a hosted runner it covers the no-window path alone. Selecting a real prefixed tab stays a manual check, since a test cannot make Claude Code retitle a tab.
4. **The facts are recorded where they belong.** The probe (what Windows Terminal 1.25.3131.0 still suppresses with Cader's arguments) goes in `windows.md`; the titles Claude Code 2.1.325 writes while working and idle, and that they reach a suppressed tab, go in `claude-code.md`.

## Risks / Trade-offs

- **[Risk] Another tab titled with a glyph, a space and the session's name.** → It would have to be in the same terminal process's windows as the session's live tab; Cader's own session tabs carry distinct names, so only a hand-titled tab could collide, and selecting it is harmless.
- **[Risk] Claude Code changes the shape (two glyphs, a suffix).** → The switch then reports the tab could not be found, as today; the scenarios and `claude-code.md` say what shape was seen, so the next change knows where to look.
- **[Trade-off] Listing every tab instead of one search.** A handful of extra COM calls per Enter, all released at the end of the call.
