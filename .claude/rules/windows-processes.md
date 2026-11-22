---
paths:
  - "src/Source191.Cader.Infrastructure/Processes/**"
  - "**/*ProcessRegistry*.cs"
  - "**/ProcessTree*.cs"
  - "**/ProcessRunner*.cs"
  - "**/ConfirmedProcess*.cs"
  - "**/ProgramLabel*.cs"
  - "**/InheritedEnvironment*.cs"
  - "**/GitCli*.cs"
  - "**/BaseUpdater*.cs"
  - "**/HandleCounts*.cs"
  - "**/ProcessApisShould.cs"
  - "**/StartAndForgetShould.cs"
  - "**/ParentStandIn/**"
---

# Windows process facts

The facts behind `ProcessRegistry`, `ProcessTree`, `ProcessRunner` and git's fetch. The measurements and probes are in `docs\verified-facts\windows-processes.md`.

## Reading processes (verified 2026-10-29, Windows 11 26100)

- **Memory:** a process's memory (the detail pane's Memory line, the `m` breakdown) is its private working set, from [`GetProcessMemoryInfo`](https://learn.microsoft.com/windows/win32/api/psapi/nf-psapi-getprocessmemoryinfo) with `PROCESS_MEMORY_COUNTERS_EX2` (`PrivateWorkingSetSize`), the figure Task Manager shows. A megabyte is 1,048,576 bytes throughout.
- **Why not another figure:** it counts resident pages only (an idle MCP server reads a few kilobytes against 90 MB of commit, `PrivateUsage`), and summing `WorkingSet64` double-counts the shared runtime image pages of many node processes.
- **No `GetProcessById` for reads:** [`Process.GetProcessById`](https://learn.microsoft.com/dotnet/api/system.diagnostics.process.getprocessbyid) walks the whole process table, about 20 ms a call. Every read in `ProcessRegistry` (`StartTimeOf`, `ParentOf`, `UsageOf`) opens its own `OpenProcess` handle with `PROCESS_QUERY_LIMITED_INFORMATION` and calls `GetProcessTimes`, which also reads processes a full-access handle is refused for.
- **The same limited right** answers `NtQueryInformationProcess` with `ProcessBasicInformation` (the parent id) and `QueryFullProcessImageNameW` (2026-11-02); `ProcessRegistryIntegrationShould` keeps two hundred reads of each under a second. `GetProcessById` is used only to end a process.
- **Listing:** [`CreateToolhelp32Snapshot`](https://learn.microsoft.com/windows/win32/api/tlhelp32/nf-tlhelp32-createtoolhelp32snapshot) lists every process with its parent id in a few milliseconds. The parent id is never cleared when the parent exits and Windows reuses ids, so a process may list a younger parent: `ProcessTree` keeps a child only when it started after its nearest measured ancestor.
- **Command lines carry secrets:** [`NtQueryInformationProcess`](https://learn.microsoft.com/windows/win32/api/winternl/nf-winternl-ntqueryinformationprocess) class 60 answers a command line (`STATUS_INFO_LENGTH_MISMATCH` and the length with no buffer). An MCP server's API key was in one, so `ProgramLabel` reduces it to a package or executable name and nothing else is kept, logged or shown.

## Fetching without a prompt (checked 14 November 2026, git 2.56.0.windows.4, Git Credential Manager 2.9.1)

- `git fetch` against an HTTPS remote with no stored credential, run with `GIT_TERMINAL_PROMPT=0` and [`GCM_INTERACTIVE=false`](https://github.com/git-ecosystem/git-credential-manager/blob/main/docs/environment.md), exits 128 in under a second with "Cannot prompt because user interactivity has been disabled" and no window.
- So `GitCli.FetchBranchAsync` sets both in its request's `Variables`, which `ProcessRunner` applies after the `InheritedEnvironment` drop. Captured runs leave stdin inherited. An SSH key's passphrase prompt is not covered and waits for the 60-second limit.

## .NET 11's process APIs (checked 14 November 2026, runtime 11.0.0, `ProcessApisShould`)

- **`RunAndCaptureTextAsync`** ([docs](https://learn.microsoft.com/dotnet/api/system.diagnostics.process.runandcapturetextasync)) and `RunAndCaptureText` kill only the child on cancellation or timeout, so a grandchild keeps the redirected pipes open: `ProcessRunner` keeps its own `Kill(entireProcessTree: true)`.
- **`KillOnParentExit`** ([docs](https://learn.microsoft.com/dotnet/api/system.diagnostics.processstartinfo.killonparentexit)) puts the child in a kill-on-close job, ending every descendant with the parent. Unelevated, it was refused (Access denied, 5) for the Store PowerShell's and winget's app execution aliases. Cader sets it on no start, because its job would also end the programs Cader starts to outlive it.
- **`Process.TryGetProcessById`** answers false for a gone id and true for the System process (4), which then refuses to be ended (`ProcessRegistry.WithProcess` uses it); `SafeProcessHandle.TryOpen` throws `UnauthorizedAccessException` for System.

## Pipes and trees

- **Redirected streams outlive the `Process`** (16 November 2026): a stream the caller has read is not closed by `Process.Dispose` ([`Process.Close`](https://learn.microsoft.com/dotnet/api/system.diagnostics.process.close)), and undisposed readers leaked handles (far more on 11.0.0 than 10.0.15).
- So `ProcessRunner.RunAsync` declares both readers with `using` straight after `Start`; `ProcessRunnerIntegrationShould.CloseTheOutputPipesOfEveryCapturedRun` keeps 50 runs under 25 extra handles.
- **A pending read is safe to dispose:** disposing a reader whose `ReadToEndAsync` is pending, because an orphan holds the pipe past `EndAsync`'s five-second wait, neither blocks nor leaves an unobserved task exception (`LeaveNoUnobservedFaultWhenAnOrphanHoldsAPipePastTheLimit`).
- **Ending a tree** (14 November 2026, runtime 11.0.0): `Kill(entireProcessTree: true)` lists every process, kills the root, then ends recursively each process whose parent id is the root's and whose start time is strictly later. Start times are precise to well under a millisecond.
- A grandchild left running by killing only the child holds its inherited redirected pipes, so `ProcessRunner` would wait its five seconds for them before reporting the cancellation.
