## Context

See proposal.md, Why. What the code does today:

- Each tab already owns its message: `SessionsViewState`, `WorktreesViewState` and `ConfigViewState` each hold a `Message`, and `TableBody` exposes it as `ITabBody.Status` with `Running` (its own count of actions in flight). `ConsoleScreen.FrameChanged` calls `ShowStatus(Active.Status, Active.Running)`, so the line is always the shown tab's message and nothing else.
- `ShowStatus` passes the message through `LaunchProgress.Shown` (a `Working(StartSession)` becomes `Launching(step)`), compares by reference to spot a new message, and records the owner in `StatusShown.Message` (today always `Active`) for the close mark and for `ClearLater`, which reduces `MessageDismissed` in that tab after `StatusLayout.SuccessShownFor` (ten seconds) for a success. `MessageDismissed` clears only while the tab still holds that same instance.
- `Announce` hands `NewerReleaseFound` to the shown tab's `ShowMessage`, so it belongs to whichever tab happened to be shown.
- The reducers clear their own tab's message when its selection moves or its filter changes. Nothing else ages a message: refusals, failures, hints and notices stay until closed.
- The only message lifetime in the code is `StatusLayout.SuccessShownFor`, ten seconds.

## Goals / Non-Goals

**Goals:** console-wide messages follow the user across tabs; a tab's own message does not greet the user long after they left it; all of it decided by pure, tested code in `Application\Views`.

**Non-Goals:** moving where messages are stored (each tab keeps its `Message`, and every reducer's clearing rule stays); changing `TableBody`, the reducers, `Wording` or the rendering in `ShowStatus` beyond which message and owner it is handed.

## Decisions

### 1. Every case is classified once: `StatusScope`

A closed enum `StatusScope { Tab, Console }` and a static `StatusScopes.Of(StatusMessage)` in `Application\Views`, beside `Wording.ToneOf`. `StatusMessage` is an open hierarchy that grows a case per feature, so, as with `ToneOf`, the switch keeps a fallback arm (`Tab`), and `StatusScopesShould` names every case in a theory table with a reflection fact that fails for a case the table leaves out, the way `StatusTonesShould` does.

| Scope | Cases |
| --- | --- |
| Console | `Launching`; `NewerReleaseFound`, `UpToDate`, `UpdateCheckOff`; `Failed`; `Working(effect)` and `Reported(outcome)` when the effect is `StartSession`, `SaveSettings`, `ToggleUpdateCheck`, `ToggleDiagnosticLog`, `RenameAccount`, `StartUpdate` or `OpenReleasePage` (for `Reported`, the outcome record that effect's handler returns) |
| Tab | `Blank`; every refusal or hint about the selected row or the tab's keys: `NoSessionSelected`, `RunsInOwnTerminal`, `NeverSentAMessage`, `FolderGone`, `NoEmptyTerminals`, `NoMessagesToRename`, `WorkingSoRenameWaits`, `RenameCancelled`, `InvalidName`, `InvalidAccountLabel`, `AlreadyInBackground`, `WorkingSoAdoptWaits`, `NoWorktreeSelected`, `NotRemoved`, `ForceCancelled`, `NothingTyped`, `TypedWrongName`, `NothingToPrune`, `LeftoverFolder`, `NoSessionIn`, `NoWorktree`, `NotDeletedOnOrigin`, `SessionNotListed`, `NoPullRequest`, `NothingToMeasure`, `NoSolution`, `Kept`, `NothingToResume`, `NoDialogFor`, `SetupAdvice`, `NoPathHere`, `NoSettingSelected`, `NoLogToClear`, `LogFilesNotRead`, `ClearsOnlyTheLog`; `BranchKept`, `Followed`; and `Working` and `Reported` for every other effect (copy, remove, rename, stop, measure, clear the log and the rest) |

Why `Failed` is console-wide: it is background work that threw, which the owner listed among the messages that must not vanish on a switch, and its words name what failed, so it reads correctly on any tab. Why `RenameAccount` is console-wide: the label it saves shows on the Sessions tab's ACCOUNT column, where the user goes to see it. Alternative considered: classify outcomes by their own case rather than the effect's; rejected because the effect is what says whether the work outlives the tab, and the outcome records are one per handler anyway.

### 2. One pure record decides the line: `StatusLine`

`Application\Views\StatusLine`, an immutable record the screen holds, of per-tab entries: the tab's current message (by reference), when the screen first saw it, and when the user last left the tab. Its functions take the time from the caller (the screen's `TimeProvider`), so the tests drive them with `FakeTimeProvider`:

- `Seen(ConsoleTab, StatusMessage, DateTimeOffset now)`: records a tab's current message and the time it was first seen (unchanged while it is the same instance, reset by a new one). The screen calls it for all three tabs on every `FrameChanged`, so a message settled on a hidden tab is stamped no later than the next redraw.
- `Left(ConsoleTab, DateTimeOffset now)`: the tab stopped being shown.
- `Choose(ConsoleTab shown, running per tab)`: answers a `StatusChoice(StatusMessage Message, ConsoleTab Owner)`: of the shown tab's message (any scope) and every other tab's console-wide message, the one first seen latest; a tie goes to the shown tab; `StatusMessage.None` owned by the shown tab when there is none. A hidden tab's tab-scoped message is never a candidate.
- `KeepsOnReturn(ConsoleTab, bool running, DateTimeOffset now)`: whether the tab's message survives the tab being shown again: always for a console-wide message or a running action; otherwise only if it was first seen after the tab was left (it arrived unseen) or the user left the tab no more than `StatusLayout.SuccessShownFor` before.

Why ten seconds: it is the one lifetime the line already has, it covers a quick look at another tab, and a success never outlives it anyway, so the rule changes only refusals, failures, hints and notices, which today never age. Alternatives considered: measuring from when the message appeared (rejected: an outcome that settles while the tab is hidden would be lost unseen), and a new, longer constant (rejected: YAGNI, nothing argues for a second number).

### 3. The screen only wires it

- `ConsoleScreen.Show(tab)`: before switching, `Left(previous)`; after, when `KeepsOnReturn` is false, reduce `MessageDismissed(message)` in that tab, so a newer message is never cleared (the existing input's rule).
- `ShowStatus`: hand `Choose`'s message and owner to the existing code; the owner replaces `Active` in `StatusShown.Message`, so the ✕ and the ten-second clear act on the tab that holds the message, and the spinner follows the owner's `Running`. `LaunchProgress.Shown` is still applied to the chosen message, so a launch's steps show on any tab.
- `Announce` is unchanged: `NewerReleaseFound` lands in the shown tab and, being console-wide, then shows everywhere.

## Risks / Trade-offs

- [A hidden tab's settle may not raise a redraw, so `Seen` stamps it late] → harmless: stamped at the return redraw, it still counts as arrived unseen, so it shows.
- [A console-wide message from a tab is cleared by that tab's reducer when its selection moves] → accepted: the user moved on there; the same rule holds today.
- [Two clocks: the reducers' clearing and `StatusLine`'s times] → `StatusLine` only stamps and decides; every clear still goes through `MessageDismissed` or a reducer, so a tab's state stays the one truth.

## Migration Plan

None: nothing is persisted. Rollback is reverting the change.
