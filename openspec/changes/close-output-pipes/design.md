# Design

## Context

`ProcessRunner.RunAsync` (`src\Source191.Cader.Infrastructure\Processes\ProcessRunner.cs`) starts a captured child and reads both streams with `ReadToEndAsync`. It then waits for the child to exit, within its time limit, honouring the caller's token. On cancellation or timeout, `EndAsync` kills the child's tree and waits up to five seconds for the reads to end. Only the `Process` is disposed, and that leaves the readers open (proposal.md, Why). `Program.cs` hooks `TaskScheduler.UnobservedTaskException`, and `CrashRecorder` records anything that reaches it.

## Goals / Non-Goals

**Goals:**
- Both readers, and with them the pipe handles, are closed when `RunAsync` returns or throws.
- No new unobserved task exception, even when a surviving descendant still holds a pipe at the time limit.

**Non-Goals:**
- Changing how a timed-out or cancelled child is ended, or the five-second wait for its pipes.

## Decisions

1. **Declare both readers with `using` straight after `Start`.** For example, `using StreamReader standardOutput = process.StandardOutput;`, then start the reads from those locals. Declared after `using Process`, they are disposed before it, on the success, cancellation and time-limit paths alike, with no `finally` to keep in step.
   - *Rejected: disposing them in a `finally`.* It means more code for the same paths.
   - *Rejected: .NET 11's [`Process.RunAndCaptureTextAsync`](https://learn.microsoft.com/dotnet/api/system.diagnostics.process.runandcapturetextasync).* It owns its pipes, but on cancellation or timeout it ends only the child, so a grandchild keeps running and holding the pipes. `windows.md` records this from `ProcessApisShould`, and the runner would lose its tree kill.
2. **On the time-limit path, observe reads that are still pending.** When `EndAsync`'s five-second wait times out, a descendant the tree kill could not reach (an orphan whose parent had already exited) still holds a pipe. The read is then pending when the reader is disposed, and may fault later. `EndAsync` keeps the `Task.WhenAll` of the two reads, and after a timeout attaches a continuation that only reads the fault (`OnlyOnFaulted`), so nothing reaches `UnobservedTaskException`. This is added only if the red test in tasks.md shows the fault. If disposing a reader with a read pending does not fault it on .NET 11, no code is added, and the finding is recorded in `windows.md` instead.
3. **The handle test counts this process's handles around many runs.** It runs one captured child first, so first-run allocations don't count, then settles the GC (collect, wait for finalisers, collect) and reads `Process.GetCurrentProcess().HandleCount`. It then runs 50 captured children, settles again, and requires growth below 25. The leak adds at least two per run, about 100 in all; the fixed runner adds none that a collection does not reclaim. It joins `ProcessRunnerIntegrationShould`, whose "Machine-wide pings" collection already runs alone, so no other test opens handles meanwhile. The collection definition's summary gains that reason.

## Risks / Trade-offs

- **[Risk] Disposing a reader while its read is pending could block the caller.** → On Windows, a pending read holds a reference on the `SafeHandle`, so the close waits for the read rather than the caller doing so. The time-limit test asserts that `RunAsync` still throws within its existing bound.
- **[Risk] The test host opens handles of its own during the count.** → The collection runs alone, and the margin of 25 against about 100 absorbs stray ones.
- **[Trade-off] The orphan test takes about ten seconds.** It waits for the orphan to end so that it leaves no ping behind for the collection's other checks.
