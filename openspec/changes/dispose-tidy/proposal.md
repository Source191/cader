# Proposal

## Why

The sweep that followed close-output-pipes found no other leak that grows while the console runs, but it found code that relies on the garbage collector, or on a `finally` that has to be kept in step, to release what it holds, and nothing that would warn of the next one. The pipe leak showed that a runtime upgrade can stop the collector from tidying up after code that never released its resources, so code that depends on it is a risk even when today's figures look fine.

## What Changes

- **Analysers:** CA2000 (dispose objects before losing scope) and CA2213 (disposable fields should be disposed) are held at `warning`, so they fail the build, everywhere except `src\Source191.Cader.Tui`, with CA2000 treating an object passed to a constructor as owned by the new object (`dispose_ownership_transfer_at_constructor`). Terminal.Gui disposes a view's subviews with the view, which neither rule can see, so in the Tui layer they would only raise false alarms. Neither rule would have caught the pipe leak (a stream from a property getter); the handle-count test from close-output-pipes guards that.
- **`ProcessRegistry`:** its native process and snapshot handles become `SafeHandle`s (`SafeProcessHandle` from `OpenProcess`, a small snapshot handle type from `CreateToolhelp32Snapshot`) held in `using` declarations, replacing the raw `nint` handles and `CloseHandle` in `finally`. A handle can then not be closed while a call still uses it, and none can be left open by a path that forgets the `finally`.
- **`UiAutomationTabs`:** holds one `IUIAutomation` and its fixed tab-item condition for the process's life, as UI Automation intends, and releases each call's own conditions, element arrays and elements when the call ends, instead of creating a new automation object on every Enter and leaving every COM object for the collector.
- **Tests and tools:** `ProcessRunnerIntegrationShould.PingsStartedSince` disposes every `Process` it reads, `ProcessRegistryIntegrationShould` disposes its `Process.GetCurrentProcess()` objects, `ProcessApisShould` disposes the handle `SafeProcessHandle.TryOpen` answers, and `tools\ReadmeScene\FrameRecorder` always saves and disposes a captured frame, which it skipped when cancelled before the save started.

Out of scope:
- **A handle or memory count in the diagnostic log's periodic counts:** the early warning for a leak in use; a later change.
- **Multiple Claude accounts:** a later change.
- **The Tui layer's disposal:** Terminal.Gui's own view ownership already disposes it; the sweep found no gap there.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None. No requirement changes: nothing a user can see behaves differently. The change declares `skip_specs: true`.

## Impact

- **`.editorconfig`:** the two rules, their option and the Tui exception.
- **`src\Source191.Cader.Infrastructure\Processes\ProcessRegistry.cs` and `ProcessRegistryNative.cs`:** handles as `SafeHandle`s; one new file for the snapshot handle type.
- **`src\Source191.Cader.Infrastructure\Terminal\UiAutomationTabs.cs`:** the cached automation object and the release of each call's objects.
- **Tests:** `ProcessRunnerIntegrationShould`, `ProcessRegistryIntegrationShould`, `ProcessApisShould`, and a new `UiAutomationTabsShould`.
- **`tools\ReadmeScene\FrameRecorder.cs`:** the save no longer depends on the token.
- **Version:** `src\Source191.Cader\Source191.Cader.csproj` raises its `Version`; `CHANGELOG.md` gets an Internal entry.
- **Manual check:** Enter on a session with an open tab still switches to that tab at a real Windows Terminal (CONTRIBUTING's manual checks for tabs).
- **Users:** nothing visible changes.
