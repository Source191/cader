# Proposal

## Why

Every captured run (git, claude) leaves its two output pipes open, so a console that runs all day slowly fills with handles and memory. One that had run for 16 hours on 0.22.0-preview.22 held 116,475 handles. 116,351 were `File` handles; all of the newest 200 were pipes, and they grew by about 3 a second. Its memory read more than twice its usual figure.

`ProcessRunner.RunAsync` reads `process.StandardOutput` and `StandardError` but never disposes them. [`Process.Close`](https://learn.microsoft.com/dotnet/api/system.diagnostics.process.close) leaves a redirected stream open once the caller has read it, because the caller then owns it. On .NET 10 the garbage collector closed those pipes anyway; on .NET 11 it does not. Measured on 16 November 2026 with 200 runs of `git --version` and the readers left undisposed:

- **.NET 10.0.15:** 42 handles were still open after a full collection.
- **.NET 11.0.0:** 434 were still open.

With the readers disposed, 1 handle stayed open on each runtime. So moving to .NET 11 (#280) turned a latent bug into a leak that grows. The code never used .NET 11's new process APIs; it relied on the finaliser.

## What Changes

- `ProcessRunner.RunAsync` disposes both readers on every path: success, the caller's cancellation and the time limit.
- On the time-limit path, a descendant that outlived the kill can still hold a pipe open, so a read may still be pending when the reader is disposed. Such a read must not end up recorded as an unobserved task exception in the crash log.
- A test runs many captured children and checks that the test process's handle count stays flat.
- The verified fact goes in `.claude\rules\windows.md`, dated, with the runtime versions.

Out of scope:
- **`RunInteractiveAsync`:** it redirects nothing, so it has no pipes.
- **The one handle per run that a collection reclaims on both runtimes:** it is not a leak.
- **Multiple Claude accounts:** a later change.
- **A memory or handle counter in the diagnostic log:** possibly a later change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None. No requirement changes: the console does what its specs say, without leaking handles. The change declares `skip_specs: true`.

## Impact

- **`src\Source191.Cader.Infrastructure\Processes\ProcessRunner.cs`:** the only change to shipped code.
- **`test\Source191.Cader.Infrastructure.Tests\ProcessRunnerIntegrationShould.cs`:** new tests. Its collection already runs alone, which a handle count needs.
- **Version:** `src\Source191.Cader\Source191.Cader.csproj` raises its `Version` (preview number), and `CHANGELOG.md` gets a Fixed entry under Unreleased.
- **Users:** a console left open stops growing in handles and memory. Nothing visible changes otherwise.
