# Tasks

## 1. Process handles in ProcessRegistry

- [x] 1.1 Guard: `ProcessRegistryIntegrationShould.CloseEveryHandleItOpens` (design decision 5) passes on today's code, and the class joins the "Machine-wide pings" collection. In the same class, hold each `Process.GetCurrentProcess()` in a `using` declaration (decision 6). Verify the whole class passes. Commit.
- [x] 1.2 Refactor: `ProcessSnapshotHandle` in its own file, `OpenProcess` returning `SafeProcessHandle`, `CreateToolhelp32Snapshot` returning `ProcessSnapshotHandle`, every query taking the `SafeHandle`, `Opened<T>` and `Snapshot` using `using` declarations, and the `CloseHandle` import removed from `ProcessRegistryNative` (decision 2). Verify `ProcessRegistryIntegrationShould`, `ProcessRegistryShould` (if present) and `OneTypePerFileShould` pass. Commit.

## 2. UI Automation objects

- [x] 2.1 Guard: `UiAutomationTabsShould.AnswerFalseTwiceForATitleNoTabHas`, for `TerminalWindow.Owning` this process's id and for `TerminalWindow.Any` (decision 4), passes on today's code. Commit.
- [x] 2.2 Refactor: the cached `Lazy<IUIAutomation>` and tab-item condition, and each call's COM objects released in reverse order through a `using` list (decision 3). Verify 2.1 passes, then temporarily release the cached condition and verify 2.1 fails with `InvalidComObjectException` before reverting that. Commit.
- [ ] 2.3 Manual: in a Cader built from this branch with the Claude Code variables cleared (CLAUDE.md), Enter on a session whose tab is open switches to that tab, twice. Record the result and date in `windows.md`'s UI Automation line, which today says this is not re-checked on .NET 11.

## 3. Tests and tools

- [x] 3.1 `ProcessRunnerIntegrationShould.PingsStartedSince` disposes every `Process` it does not return, and its three callers dispose what it returns (decision 6). Verify the class passes. Commit.
- [ ] 3.2 `ProcessApisShould.FindAProtectedProcessButNotOneThatHasGone` declares the `TryOpen` handle in a `using`, and `tools\ReadmeScene\FrameRecorder` starts each save without the token (decision 6). Verify `ProcessApisShould` passes and `tools\ReadmeScene` builds. Commit.

## 4. The analysers

- [ ] 4.1 Add CA2000 and CA2213 at `warning`, `dotnet_code_quality.CA2000.dispose_ownership_transfer_at_constructor = true`, and the `[src/Source191.Cader.Tui/**.cs]` section turning both off with its reason, to the root `.editorconfig` (decision 1). Verify `dotnet build -c Debug` passes with no CA2000 or CA2213 suppressed in place. Commit.
- [ ] 4.2 Record the reasons as comments beside the settings in `.editorconfig` (why the Tui is excepted; that neither rule tracks a disposable from a property getter, which the handle-count tests guard), and add one line to `CLAUDE.md`'s C# idioms saying CA2000 and CA2213 fail the build outside the Tui. Verify both read clearly and `openspec validate` still passes. Commit.

## 5. Release

- [ ] 5.1 Raise `Version` with `.github\scripts\Step-Version.ps1` and add an Internal entry under Unreleased in `CHANGELOG.md`. Verify `dotnet build -c Debug`, `dotnet test -c Debug --no-build` at the solution and `dotnet format --verify-no-changes` pass.

## Workflow follow-up

- Archive the change in the same pull request (`/opsx:archive`) and verify that `openspec validate` passes.
- Open the pull request stacked on #318 (base `c-mem-leak`) with Why, Benefit and What changed, linking the Learn pages for CA2000, CA2213 and `SafeHandle`.
