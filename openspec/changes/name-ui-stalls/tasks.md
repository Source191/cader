# Tasks

**Implement with:** Opus 5.5 at medium effort: a small diagnostic addition, but it first needs a spike into Terminal.Gui 2.5.0's draw and layout events (decompiled with ilspycmd, as `terminal-gui.md` records), and the stall rules have subtle timing tests.

Each task is red, green, refactor, with a commit per step, except the spike. Heartbeat facts use `FakeTimeProvider`.

## 1. Spike

- [ ] 1.1 Decompile Terminal.Gui 2.5.0 with ilspycmd and find the window events that begin and end a whole layout pass and a whole draw pass; observe them in a throwaway program in the headless terminal (a 250 ms timeout and a forced redraw). Record what was read and what was observed, with the date, in `.claude\rules\terminal-gui.md` (the decompilation and probe in `docs\verified-facts\terminal-gui.md`). Delete the throwaway program. Commit the rule file.

## 2. Heartbeat

- [ ] 2.1 `HeartbeatShould.NameWorkBegunAndEndedOutsideAMark` and `NestABeginInsideAMark`: `Begin` and `End`, with `Mark` built on them. Verify the existing `HeartbeatShould` facts still pass. Commit.
- [ ] 2.2 `HeartbeatShould.GiveTheCollectorsPauseSinceTheLastBeat` over a scripted `Func<TimeSpan>`: `UiThreadStalled` and `UiThreadStalledRunning` gain the pause; `DiagnosticLineShould` words it. Commit.
- [ ] 2.3 `PerformanceCountsShould.WriteTheDrawCounts`: a `DurationSample` for draws in `PerformanceCounts`, written by `PeriodicCounts` and the crash file. `DiagnosticEventsShould` still passes. Commit.

## 3. Screen wiring

- [ ] 3.1 `ConsoleScreen` begins and ends `CodeName`s for layout and drawing on the events the spike found, adding each draw's time to the draw counts; the composition passes `GC.GetTotalPauseDuration`. Verify with the headless smoke check (`dotnet run -- smoke <exe> <worktree-folder>` from `tools\HeadlessTerminal`, exit 0); the owner sees the draw count line in the timing run at the implementation plan's checkpoint B (turning the log on is a saving key). Commit.

## 4. Release

- [ ] 4.1 Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` one preview step above the previous step of the implementation plan and add a CHANGELOG `Unreleased` entry under Changed (stall lines name drawing, layout and the collector's pauses). Verify `.github\scripts\Assert-VersionBumped.ps1`, `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes` and `openspec validate name-ui-stalls`. Commit.

## Workflow follow-up

- Archive the change in the pull request that completes it.
