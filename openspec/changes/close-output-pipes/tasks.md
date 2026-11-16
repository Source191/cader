# Tasks

## 1. Close the readers

- [ ] 1.1 Red: `ProcessRunnerIntegrationShould.CloseTheOutputPipesOfEveryCapturedRun` (design decision 3) fails, showing about 100 extra handles after 50 runs, not some other error. Add the handle-count reason to `MachineWidePingsDefinition`'s summary. Commit.
- [ ] 1.2 Green: declare both readers with `using` straight after `Start` in `ProcessRunner.RunAsync` (design decision 1). Verify the new test and every existing `ProcessRunnerIntegrationShould` test pass. Commit.

## 2. The time-limit path

- [ ] 2.1 Red: `ProcessRunnerIntegrationShould.LeaveNoUnobservedFaultWhenAnOrphanHoldsAPipePastTheLimit`. The child starts an orphan ping that holds stdout for about eight seconds (an inner `cmd` that starts it with `start /b` and exits at once), then pings for 30 seconds under a 1-second limit. The test asserts `ProcessTimedOutException` within the existing bound, waits for the orphan to end, settles the GC and asserts that `TaskScheduler.UnobservedTaskException` saw nothing. If it passes without a code change, add no code; record in 3.1 that disposing a reader with a read pending did not fault it (design decision 2). Commit.
- [ ] 2.2 Green (only if 2.1 failed for the unobserved fault): `EndAsync` keeps the reads' `Task.WhenAll` and, after the five-second wait times out, attaches an `OnlyOnFaulted` continuation that reads the fault. Verify 2.1 and the existing time-limit and cancellation tests pass. Commit.

## 3. Record and release

- [ ] 3.1 Record in `.claude\rules\windows.md` under the .NET 11 process APIs the fact measured on 16 November 2026: a redirected stream the caller has read is not closed by `Process.Dispose`. With 200 captured runs and the readers undisposed, 42 handles stayed open after a full collection on 10.0.15 and 434 on 11.0.0; disposing the readers left 1 on each. Add what 2.1 found, and that `ProcessRunner` disposes its readers. Verify the file reads clearly.
- [ ] 3.2 Raise `Version` with `.github\scripts\Step-Version.ps1` and add a Fixed entry under Unreleased in `CHANGELOG.md` saying a console left open no longer grows in handles and memory. Verify `dotnet build -c Debug`, `dotnet test -c Debug --no-build` at the solution and `dotnet format --verify-no-changes` pass.

## Workflow follow-up

- Archive the change in the same pull request (`/opsx:archive`) and verify that `openspec validate` passes.
- Open the pull request with Why, Benefit and What changed, linking the Learn pages for `Process.Close` and `Process.RunAndCaptureTextAsync`.
