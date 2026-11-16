# Design

## Context

- **`ProcessRegistry`** opens a limited-information handle for every start time, parent and usage it reads (`Opened<T>`), and a Toolhelp snapshot for `Snapshot`. Both are raw `nint`s closed by `CloseHandle` in a `finally`, and every query (`StartOf`, `ParentOfOpened`, `NameOf`, `PrivateMemoryOf`, `CommandLineOf`, `ReadEntries`) takes the `nint`. `ProcessRegistryIntegrationShould` already runs the reads against real processes, including two hundred of each under a second.
- **`UiAutomationTabs.Select`** runs on a pool thread for each Enter on a session with an open tab. Each call creates a `CUIAutomation8`, three conditions, an element array and its elements, and releases none of them. It has no test today; it was checked by hand at a real Windows Terminal.
- **The analysers:** a one-off build with CA2000 and CA2213 at warning, on 16 November 2026, raised about 50 warnings. With `dispose_ownership_transfer_at_constructor` on, the only ones outside `src\Source191.Cader.Tui` were `ProcessApisShould`'s `out SafeProcessHandle` and `FrameRecorder`'s bitmap. Every one inside the Tui was a view that Terminal.Gui disposes with its superview, or a view field the window disposes (`terminal-gui.md`).

## Goals / Non-Goals

**Goals:**
- Every handle and COM object these adapters take is released when the call that took it ends, without relying on the collector.
- The build fails when new code outside the Tui leaves a disposable it created undisposed.

**Non-Goals:**
- Changing what `ProcessRegistry` or `UiAutomationTabs` answer, how fast, or which processes or tabs they reach.
- The Tui layer's disposal, and a handle or memory count in the diagnostic log.

## Decisions

1. **The rules sit in the root `.editorconfig`'s `[*.cs]` section, and a `[src/Source191.Cader.Tui/**.cs]` section turns them off.** `dotnet_diagnostic.CA2000.severity = warning`, `dotnet_diagnostic.CA2213.severity = warning` and `dotnet_code_quality.CA2000.dispose_ownership_transfer_at_constructor = true`; warnings are errors, so a hit fails the build. The Tui section sets both to `none`, with a comment naming why (a superview owns its subviews).
   - *Rejected: `dispose_ownership_transfer_at_method_call`.* It would silence the Tui's `dialog.Add(...)` too, but by treating every object passed to any method as handed over, it hides exactly the leaks the rule exists for.
   - *Rejected: the IDisposableAnalyzers package.* It is a new dependency for a gap CA2000 and CA2213 nearly cover, and neither it nor they track a disposable from a property getter, which is how the pipe leak arose.
   - The fixes the rules call for land before the rules, so the build never breaks between commits.
2. **`OpenProcess` returns a `SafeProcessHandle`, `CreateToolhelp32Snapshot` a new `ProcessSnapshotHandle`, and the queries take the `SafeHandle`.** `[LibraryImport]` marshals a `SafeHandle` return and parameter (adding a reference for the call), so `Opened<T>` becomes `using SafeProcessHandle handle = OpenProcess(...); if (handle.IsInvalid) { return whenGone; }`, and `Snapshot` the same; the `CloseHandle` import goes.
   - **`ProcessSnapshotHandle`:** an `internal sealed class : SafeHandleZeroOrMinusOneIsInvalid` in its own file, whose `ReleaseHandle` calls its own `CloseHandle` import. Zero-or-minus-one matches the two failure values `Snapshot` checks today.
   - *Rejected: `SafeFileHandle` for the snapshot.* It also calls `CloseHandle`, but naming a snapshot a file handle misleads the reader.
   - *Rejected: .NET 11's `SafeProcessHandle.TryOpen`.* Which rights it asks for is not recorded in `windows.md`, and the registry needs exactly `PROCESS_QUERY_LIMITED_INFORMATION`, which reads processes a fuller handle is refused for.
   - `TerminateProcess` and `WithProcess` are untouched: ending a process goes through `Process`, which is already disposed.
3. **`UiAutomationTabs` holds one client, the desktop element and the tab-item control-type condition for the process's life (a `Lazy` with `LazyThreadSafetyMode.PublicationOnly`, so a client that failed to start is tried again rather than its exception cached), and releases the rest per call.** UI Automation's client objects are free-threaded, and the calls come from pool threads (MTA), so one instance serves them all. Each call releases what it obtained (the process and name conditions, the combined condition, the window array, each window element, the found tab and its pattern) with `Marshal.ReleaseComObject` in a `finally`, in reverse order of acquisition, through a small private `ComObjects` list declared with `using` so the method keeps one level of release logic. `Marshal.ReleaseComObject` is Windows-only to the platform analyser, so the list checks `OperatingSystem.IsWindows()`, as `ConsoleRecovery` does. The cached objects are never released, so a second call cannot find them dead.
   - *Rejected: `Marshal.FinalReleaseComObject`.* These RCWs are not shared, so one release is exact, and `FinalReleaseComObject` would hide an accidental double hold.
   - *Rejected: leaving it to the collector.* It works today, but that is the assumption the pipe leak broke.
4. **`UiAutomationTabsShould` proves the release cannot break a later call.** It calls `Select` twice in a row for a title no tab has (a fresh GUID), once with `TerminalWindow.Owning` this test process's id and once with `TerminalWindow.Any`, and asserts false each time. Releasing a cached object by mistake makes the second call throw `InvalidComObjectException`, which `Select` does not catch, so the test fails. It never selects a tab, so it changes nothing on a developer's machine, and on a runner with no Windows Terminal `Any` finds no window. Selecting a real tab stays a manual check.
5. **The registry keeps its existing tests as the safety net, plus one handle count.** `ProcessRegistryIntegrationShould.CloseEveryHandleItOpens` settles the collector, reads two hundred start times, parents and usages of this process, settles again and requires handle growth below 25, the same shape as close-output-pipes' test, and joins the "Machine-wide pings" collection so nothing else opens handles meanwhile. It passes before and after the change; it guards the refactor rather than driving it.
6. **The test and tool fixes are the small ones the sweep named.** `PingsStartedSince` answers the pings' ids and disposes every process it reads, so its callers hold nothing to dispose and a failure lists ids; `ProcessRegistryIntegrationShould` holds `Process.GetCurrentProcess()` in `using` declarations; `ProcessApisShould` declares the `TryOpen` handle in a `using`; `FrameRecorder` starts the save without the token, so a captured frame is always saved and disposed (cancellation still stops the next capture at `Task.Delay`), and hands the frame over in CA2000's recommended `try`/`finally` (the local set to null once the save owns it), which also disposes it if starting the save fails. `EndAProcessCleanlySoItsExitCodeIsZero` ends a childless `waitfor.exe` rather than a `cmd` with a ping, because a clean end stops only the process it is given, and a ping left running would trip the collection's checks that no ping outlives its test.

## Risks / Trade-offs

- **[Risk] A future Terminal.Gui-free view in the Tui leaks unseen.** → The Tui section turns the rules off for the whole folder; the sweep's reading of the Tui stands, and review covers new non-view disposables there.
- **[Risk] Releasing a COM object that UI Automation still uses.** → Only objects this call obtained are released, after their last use, and the repeated-call test catches a release of a cached one.
- **[Trade-off] `FrameRecorder`'s saves no longer stop on cancellation.** A save already captured runs to the end, about a few milliseconds per frame, and the frames on disc are then complete rather than cut off.
- **[Trade-off] Constructor ownership transfer trusts every constructor that takes a disposable.** A constructor that does not keep what it is given would hide a leak; the solution's own constructors that take one keep it.
