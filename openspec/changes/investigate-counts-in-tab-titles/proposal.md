# Proposal

## Why

Each tab's counts (Sessions: need input, working, completed, failed, stopped; Worktrees: health, changes, unpushed commits; Config: settings, to set up) sit on the row of the tab titles, after the strip, and show only the shown tab's. So they read as if they belong to the whole console rather than one tab, and from the Worktrees or Config tab nothing on screen says that a session needs input until the user goes back to Sessions.

Showing them in the tabs themselves might fix both, but `Tabs` in Terminal.Gui 2.5.0 has limits that are likely to get in the way, and we do not yet know enough to say whether it is worth doing:

- **Measuring:** a title that changed left the strip measured for the old one (`.claude\rules\console-screen.md`), which is why the titles are fixed today and `ConsoleScreen.HeaderSpan()` assumes they never change.
- **Colour:** `Tabs` draws a title in one role (Focus), so the counts' own colours (amber for need input, red for failed) may be lost.
- **Movement:** a title whose length follows the counts moves every tab after it, and with them the place a click lands and the room left for the table's title.
- **Frame:** text drawn by hand over the strip or the tab's top edge is the overlay the waiting spinner broke (`.claude\rules\terminal-gui.md`).

This change is an investigation: it finds out whether the idea is a good one before anything is built.

## Benefit

- **Maintainers:** a decision made on verified Terminal.Gui facts rather than guesses, recorded with its reasons, so the idea is either built on a sound footing or not reopened without new evidence.
- **Users:** nothing changes in this change. If the investigation recommends going ahead, a later change could let them see that a session needs input from any tab and read each tab's counts as that tab's.

## What Changes

- **Spike:** a throwaway Terminal.Gui 2.5.0 program, outside the solution and never committed, run in the headless terminal and in Windows Terminal, answers the open questions in the design: whether a changed title can be re-measured, whether a title can carry more than one colour, how the strip moves and where clicks land as a title grows and shrinks, and whether text over the tab's top edge survives a redraw. Where a question can be read from the decompiled assembly, it is read first.
- **Facts:** each answer is recorded with its date and version in `.claude\rules\terminal-gui.md`, observed or read, as the house rule asks.
- **Options weighed:** four layouts, set out in the design and mocked up at 120 x 36 and at a narrow width: (A) each tab's counts in its own title, (B) one attention marker on the Sessions title only, (C) the counts inside the tab's frame, (D) today's layout, unchanged.
- **Recommendation:** a Findings section added to `design.md` naming the option recommended, why, and what it would cost; if it is A, B or C, it lists the requirements a follow-up change would modify (`console-layout`'s "Tab titles stay put" scenario and "The counts and the table's title sit beside the tabs").
- **No code:** nothing under `src`, `test` or `tools` changes, so `Version` stays where it is.

Out of scope:

- Building any option: a follow-up change, proposed on its own only if the recommendation is to go ahead.
- Cader's own Windows Terminal tab title, which could also carry the attention count; worth a separate look.
- Changing the counts' wording, glyphs or order.
- Multiple Claude accounts beyond naming a session's owner, which is a later change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None. An investigation changes no requirement; the change declares `skip_specs: true`. A follow-up, if any, carries the spec deltas.

## Impact

- **Implement with:** Opus 5.5 at medium effort: a spike and some reading of the decompiled assembly, with no shipped code; the care needed is in telling observed facts from read ones.
- **`src`, `test`, `tools`:** no change.
- **Docs:** `.claude\rules\terminal-gui.md` gains the verified `Tabs` facts; `design.md` gains the findings and the recommendation.
- **Version and changelog:** no change, since nothing in the packaged tool changes.
- **Users:** nothing visible.
