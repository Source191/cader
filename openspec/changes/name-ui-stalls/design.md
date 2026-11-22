## Context

See proposal.md, Why. `Heartbeat.Mark(CodeName, Action)` times work Cader runs and keeps the longest since the last beat; `Beat` judges lateness and records `UiThreadStalled` or `UiThreadStalledRunning`. Terminal.Gui runs layout and drawing inside its own main-loop iteration, which Cader does not call, so no `Mark` can wrap it. `FrameChanged` sets the whole window to redraw after every load, detail answer and launch step.

## Decisions

### 1. Begin and end marks for work Cader does not call

`Heartbeat.Begin(CodeName)` and `End()` keep one open mark (nesting as `Mark` does), timed and kept exactly as `Mark` keeps its work; `Mark` becomes `Begin`, run, `End` in a `finally`. The console begins a mark when the window starts laying out or drawing and ends it when that finishes. Which Terminal.Gui 2.5.0 events bracket a whole pass (candidates read from the view base: `DrawingContent` or `Drawing` with `DrawComplete`, and `SubViewLayout` with `SubViewsLaidOut`) is settled by a spike first (task 1.1), decompiled with ilspycmd and observed in the headless terminal, and recorded in `terminal-gui.md`.

Alternative considered: a stopwatch around `app.Run`'s iteration through a custom driver; rejected as far more code for the same answer.

### 2. The collector's pause through a delegate

`Heartbeat` takes a `Func<TimeSpan>` answering the process's total collector pause, wired to `GC.GetTotalPauseDuration` in the composition, and keeps the reading at each beat; a stall line carries the difference. A duration is a fact the privacy rules allow.

### 3. Drawing counted like a refresh part

Each draw pass adds its duration to a `DurationSample` written with the periodic counts (`draws since start: runs, median, p95, longest`), so a drawing cost that never reaches a freeze still shows.

## Risks / Trade-offs

- [Marking every draw adds work to every frame] → two timestamps and a comparison per pass; `Allocations` facts keep it allocation-free.
- [The spike finds no event pair that brackets a whole pass] → mark each top-level view's draw instead (the tabs, the status rows, the key bar) and say so in `terminal-gui.md`; the spec's words ("drawing") do not change.
