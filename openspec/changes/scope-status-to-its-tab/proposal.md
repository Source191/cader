## Why

The console has one status line, above the tabs, and each tab already keeps its own message there: switching tabs shows the new tab's message and coming back shows the old one again, however old it is. That is right for a key's answer (copied, removed, a refusal) but wrong both ways round: a failure or refusal left on a tab greets the user again minutes later, long after it means anything, while work that outlives the tab that started it vanishes exactly when the user switches to see its result. A launch started from the Worktrees wizard reports "Starting c-new-message: starting Claude Code (3 of 5)..." only on Worktrees, so switching to Sessions to watch the new row appear hides its progress; a newly found release, an update check's answer, a settings save and a toggle on Config disappear the same way.

Moving the line inside each tab was considered and rejected: it costs a table row on every tab and makes the console-wide messages worse, not better.

**Implement with:** Opus 5.5 at medium effort: a pure classification and a small, clock-driven decision over immutable state, with the existing status-line rendering left untouched.

## What Changes

- Every status message is either **tab-scoped** (a key's answer about a row or the tab, kept by the tab that raised it) or **console-wide** (a launch's progress and outcome, the update check's notices and answers, settings saves and toggles, background work that threw); `design.md` classifies every case.
- A console-wide message shows on whichever tab is shown until it is closed, clears by itself, or is replaced; a newer message on the shown tab takes the line over and the console-wide one comes back when that clears.
- A tab-scoped message stays with its tab, as today, but when the user comes back to the tab it shows again only if it is still recent: the user left the tab no more than ten seconds before (the same ten seconds a success already lasts), or the message arrived while the tab was hidden and so has not been seen. Otherwise it is cleared as the tab shows.
- A running action's message always shows on its own tab, whatever its age.
- No layout, rendering, wording or Terminal.Gui change: the line, its tone, wrap, close mark and ten-second success stay as they are.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `console-layout`: the status line shows a console-wide message on every tab, and a tab's own message is shown again on return only while still recent.

## Out of scope

- Moving the status line into the tabs, a message history, or more than one message on the line at once.
- Changing any message's words, tone or the ten-second success lifetime.
- `cader here`, which has no status line.
- Multiple Claude accounts beyond naming a session's owner, a later change.

## Impact

- `Application\Views`: a classification of `StatusMessage` cases (tab-scoped or console-wide) and a pure status-line record and rules deciding what the line shows and what a tab clears as it shows; their tests with `FakeTimeProvider`.
- `Tui\ConsoleScreen`: `ShowStatus` asks the new rules which message to draw and which tab owns it (for its close mark and its success timer), and `Show` applies the return rule; no new views.
- `CHANGELOG.md` and the version in `src\Source191.Cader\Source191.Cader.csproj`; `.claude\rules\console-screen.md`'s Status line section.
