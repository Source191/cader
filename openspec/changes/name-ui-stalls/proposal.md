## Why

The UI thread stalled 342 times in the measured hour (95th percentile 398 ms, longest 2.3 s), and most stall lines say "no marked work" or name a piece of work that took 0 to 2 ms. The heartbeat marks the work Cader runs on the UI thread (a refresh's rows, a detail, a key, an outcome) but not Terminal.Gui's layout and drawing, nor the garbage collector, so the log cannot say what froze the screen. Fixing the stalls blind risks optimising the wrong thing; this change makes the log name the cause, and the later changes are measured by it.

**Implement with:** Opus 5.5 at medium effort: a small diagnostic addition, but it first needs a spike into Terminal.Gui 2.5.0's draw and layout events (decompiled with ilspycmd, as `terminal-gui.md` records), and the stall rules have subtle timing tests.

## What Changes

- The console's layout and drawing passes are marked UI-thread work, so a stall line names "drawing" or "layout" with its duration when that was the longest piece since the beat before.
- Every stall line also gives how long the garbage collector paused the process since the beat before, so a pause the collector caused is told apart from one Cader's code or Terminal.Gui caused.
- The periodic counts gain the time spent drawing (runs, median, 95th percentile, longest), as the refresh parts are counted.
- Nothing on screen changes.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `diagnostics`: a stall line can name drawing or layout, and gives the collector's pause time; drawing is counted.

## Delivery

- **Order:** step 8 of the stack in #379. Depends on nothing. It lands before the scan and screen changes so the timing runs at checkpoints B and C can name what stalls.
- **Together:** ships alone, in one pull request.

## Out of scope

- Fixing what the stall lines then show (for example redrawing less than the whole window after every load); that is a later change the lines justify, once the wave 2 changes have cut the background load that may be causing much of it.
- Multiple Claude accounts beyond naming a session's owner, a later change.

## Impact

- `Application\Diagnostics\Heartbeat`: a begin and end form of marking for work Cader does not call itself; the collector's pause since the last beat (`GC.GetTotalPauseDuration`, [docs](https://learn.microsoft.com/dotnet/api/system.gc.gettotalpauseduration)) handed in through a delegate so the tests drive it.
- `Application\Diagnostics`: the stall events gain the pause; a `DrawCounts` sample in `PerformanceCounts`.
- `Tui\ConsoleScreen`: the window's layout and draw events begin and end their marks.
- `.claude\rules\terminal-gui.md`: the events used, with the date and version.
- `CHANGELOG.md` and `Version` in `src\Source191.Cader\Source191.Cader.csproj`.
- **Implement with:** Opus 5.5 at medium effort (reason above).
