---
paths:
  - "src/Source191.Cader.Infrastructure/Terminal/**"
  - "**/*WindowsTerminal*.cs"
  - "**/TerminalFragment*.cs"
  - "**/TerminalSettings*.cs"
  - "**/TerminalSetup*.cs"
  - "**/TerminalProfile*.cs"
  - "**/UiAutomationTabs*.cs"
  - "**/TabShade*.cs"
  - "**/TabTitle*.cs"
  - "**/TerminalTab*.cs"
  - "**/TerminalWindow*.cs"
  - "tools/ReadmeScene/**"
  - "tools/New-ReadmeScreenshot.ps1"
  - "**/TabRegistration*.cs"
  - "**/TabScripts*.cs"
  - "**/TabEscapes*.cs"
  - "**/DialogTabTitle*.cs"
  - "**/SessionTabs*.cs"
  - "**/*Desktop*.cs"
  - "**/FileAssociations*.cs"
  - "**/OpenFileScript*.cs"
  - "**/StartProgramScript*.cs"
  - "**/NoisyEditor/**"
  - "**/UpdateCheck*.cs"
  - "**/WorktreeRemover*.cs"
  - "**/StartAndForgetShould.cs"
---

# Windows Terminal and ShellExecute facts

The Windows process facts are in `windows-processes.md` and the winget facts in `winget.md`. The probes and measurements behind these are in `docs\verified-facts\windows.md`.

## Tabs (verified 2026-10-24, Windows Terminal 1.25)

- **Finding a tab:** [`wt.exe`](https://learn.microsoft.com/windows/terminal/command-line-arguments) focuses tabs only by index and cannot list them. Windows [UI Automation](https://learn.microsoft.com/windows/win32/winauto/entry-uiauto-win32) can: a `WindowsTerminal.exe` window's tab items carry the titles as names and support `SelectionItemPattern`, which is how Enter switches to an open tab (`UiAutomationTabs`).
- **`Interop.UIAutomationClient`** (ComImport-only, net35) builds clean on net11.0 (14 November 2026) and works from a pool thread; one shared client selected a tab repeatedly (16 November 2026, 1.25.3131.0).
- **Glyph titles:** a working session's tab can read `◐ <name>` (Claude Code's spinner), so `Domain\TabTitle.Shows` accepts the name alone or one glyph and a space before it (16 November 2026, 0.22.0-preview.34).
- **Semicolons:** `wt.exe` splits its command line on every `;`, even inside a quoted argument. `\;` passes a literal one and every other backslash is left as it is, in `--startingDirectory` too; a double quote reaches the title intact (2026-10-28, Windows Terminal 1.25.2733, CLI 2.1.312).
- So `WindowsTerminalArguments` replaces every `;` in its arguments with `\;`, and `SessionName` refuses only a blank name. Titles such as `fix a;b` and `say "hi"` are found by Enter, attached by name and survive rename (2026-10-28, 0.17.0-preview.1).
- **A tab's parents:** a tab shell's chain is `pwsh <- WindowsTerminal <- explorer` (no OpenConsole). `Domain\TerminalTab` walks it (any case, eight steps) for the shell and its terminal; `TabRegistration.OwningTerminal` answers a `Domain\TerminalWindow` (`Owning`, or `Any` while claiming or outside Windows Terminal) that `FocusTabAsync` searches.
- **Which window:** `wt -w 0 new-tab` opens the tab in the most recently used window, so run from a tab of a window opened with `wt -w new` it lands in that window (4 November 2026, 1.25.2733.0). `tools\ReadmeScene` relies on it.
- **Attach tabs close themselves:** `claude attach` exits 0 after a Ctrl+Z detach, a `claude stop` or a `claude rm` from elsewhere (29 October 2026, CLI 2.1.312), so the attach script ends with `if ($LASTEXITCODE -eq 0) { exit }` and the tab closes; its registration goes stale with the shell.
- **Closing a session's tabs:** stop, rename and delete close them through `Application\Actions\SessionTabs` (`EndCleanly` on each claimed shell, every registration forgotten); adopt ends the old tab's shell found before claude ends; Enter reads a registration's liveness at the key.

## Removing a worktree in use (13 November 2026, git 2.56.0.windows.4)

- Git's "Permission denied" on `git worktree remove` is `GitException.InUse`, read only in `GitCli`.
- Git deletes the worktree's contents, its `.git` file and its registration before failing on a folder a process has as its working directory, and exits 255 (`GitWorktreeRemoveShould` keeps it).
- So `WorktreeRemover` tries to delete the emptied folder for about five seconds, then finishes the removal and its branch either way, and the worktrees view lists any empty folder under the worktree root as empty.

## Tab colour and titles

- **The unselected shade:** an unselected coloured tab is that colour at alpha 77 of 255 over the tab row, #2E2E2E in the default dark theme ([`Tab.cpp`](https://github.com/microsoft/terminal/blob/v1.25.2733.0/src/cascadia/TerminalApp/Tab.cpp#L2387)), so `Domain\TabShade.Unselected` works out `(colour × 77 + 46 × 178) / 255` per channel (measured 30 October 2026, 1.25.2733.0). A light theme or custom tab row is not followed.
- **Escapes:** a program sets its tab title with OSC 2 and the tab background with OSC 4 on palette entry 264 (`ESC ] 4 ; 264 ; rgb:rr/gg/bb ESC \`); DECAC `ESC [ 2 ; 263 ; 264 , |` resets the tab's colour indices. The colour is ignored for a tab launched with an explicit `--tabColor`.
- **The console's title:** Terminal.Gui writes the top-level's `Title` as OSC 0 on every `Run`, so `Window.Title` and `TerminalConsole` get one value, `ScreenContext.Title` from `Wording.TabTitle` (plain `Cader`), and `TabEscapes` writes title and colour once as the console starts.
- **Dialog titles:** every `Runnable` sets `BorderSettings.TerminalTitle` and writes its own `Title` when it becomes modal, so every dialog calls `Tui\DialogTabTitle.Apply` with its `DialogName`, which clears the flag and writes `Wording.TabTitleDuring` (`Cader (Keys)`) on `IsModalChanged`; the main window writes `Cader` back when it closes.
- **`--suppressApplicationTitle`** blocked OSC 0, 1 and 2 and `SetConsoleTitle` in a probe tab with Cader's exact arguments (16 November 2026, 1.25.3131.0), yet a session's tab once showed Claude's glyph title by a route not reproduced. Claude's own output is the reliable evidence (`claude-code.md`), hence `TabTitle.Shows`.

## The fragment and Terminal's settings

- **The fragment:** `cader setup-terminal` writes a [fragment](https://learn.microsoft.com/windows/terminal/json-fragment-extensions) at `%LOCALAPPDATA%\Microsoft\Windows Terminal\Fragments\Cader\cader.json` (UTF-8, no BOM) adding the "Cader" and "Cader session" profiles with `bellStyle`, `bellSound` and `icon`.
- **The icon** is `logo.ico`, shipped from `assets\icon` and copied into the fragment's folder so a tool update cannot break the path; `--remove` deletes it with the folder. Windows Terminal has no escape sequence for an icon.
- **Loading:** Terminal reads fragments at start-up (a restart is needed), and a fragment cannot change `profiles.defaults`. The profile names live once in `Ports\TerminalProfiles`. While the fragment exists, `WindowsTerminal` adds `--profile "Cader session"` to every `wt new-tab`. What `wt` does with an unloaded profile is unverified.
- **Profiles hidden as deleted** (issue #218, 4 November 2026, 1.25.2733.0): Terminal lists every generated profile it has loaded in `generatedProfiles` in `state.json`, and hides one as deleted whenever `settings.json` has no entry with its `guid`. For the Store package both files are in `%LOCALAPPDATA%\Packages\Microsoft.WindowsTerminal_8wekyb3d8bbwe\LocalState`.
- **The profile id:** a fragment profile's `guid` is a version 5 UUID of its name (UTF-16LE) under the version 5 UUID of the fragment's folder name under `{f65ddb7e-706b-4499-8a50-40313caf510a}` (`Domain\TerminalProfileId`, which reproduces the issue's observed ids).
- **The fix:** adding the four-property entry (`guid`, `"hidden": false`, `name`, `source`) by hand brought the profile back with no restart. `setup-terminal` adds it after asking (`TerminalSettings.AddProfileEntriesAsync`) and `--remove` takes out an entry that still has exactly those four properties.
- **Computed, not yet seen in a real `state.json`** (13 November 2026): Cader `{3d7a06d2-710f-51d9-90c9-dacae2c3ab46}`, Cader session `{cdf8708d-a821-5c97-95d0-fea560807dea}`; Windows PowerShell `{61c54bbd-c2c6-5271-96e7-009a87ff44bf}` is the hash of its name directly under `{2bde4a90-d05f-401c-9492-e40884ead1d8}`.
- **Replacing `settings.json`** (15 November 2026, `TerminalSettingsShould.KeepTheFilesCreationTimeAndAccessRules`): [`File.Replace`](https://learn.microsoft.com/dotnet/api/system.io.file.replace) keeps the original's creation time, access rules and hidden attribute; `File.Move(..., overwrite: true)` keeps none of them.
- `File.Replace` over a read-only file throws `UnauthorizedAccessException` and changes nothing. So `TerminalSettings` refuses a read-only file before reading it, writes with `File.Replace` and names the backup `settings.json.<yyyy-MM-ddTHH-mm-ss>.cader.backup` in UTC.

## Updating

- **A running `cader.exe` locks its own files**, so an upgrade fails while any Cader runs (`winget.md`). `U` upgrades from a tab of its own (`TabScripts.Update`: `Wait-Process` on this Cader's id, `winget upgrade --id Source191.Cader --exact --source winget` by winget's full path, then cader only when `$LASTEXITCODE` is 0) and Cader quits once the tab is open.
- With no winget on the PATH no tab opens and Cader stays (`WingetMissingOutcome`).
- **The check:** `UpdateCheck` over `IPackageVersions` (`WingetPackageVersions`, reading winget-pkgs' version folders and the changelog at a tag) is started by `ScreenLoop` on a pool thread as the console opens, never awaited (a fault is only logged), at most once a day, never for a prerelease.

## ShellExecute and editors

- **ShellExecute's environment:** [ShellExecute](https://learn.microsoft.com/windows/win32/api/shellapi/nf-shellapi-shellexecutew) (`UseShellExecute = true`, which takes no environment) starts the app with the environment its caller was created with; removing variables from the live process changes nothing (2026-10-25). `cmd /c start` shows a blocking dialog for a missing file.
- So `Desktop.OpenAsync` and `StartAsync` make the call from a short-lived `pwsh` child started through `IProcessRunner` (cleaned like every child), the target as Base64 in its script (`OpenFileScript`). Test it with a file type under `HKCU\Software\Classes` whose handler writes its environment to a file.
- **Editors printing on the console** (#210): a windowed child gets no console of its own, and Electron (so VS Code) attaches to its parent's ([`ELECTRON_NO_ATTACH_CONSOLE`](https://github.com/electron/electron/blob/main/docs/api/environment-variables.md)), so an editor started as the console's child printed over its screen (13 November 2026, .NET 10, `DesktopShould.KeepAStartedProgramsOutputOffTheConsole`).
- **On .NET 11** a windowed child is handed the parent's standard handles instead; the detached starts (`Process.StartAndForget`, `StartDetached`, or each handle set to `File.OpenNullHandle()`) hand it the null device, so it neither writes nor attaches (14 November 2026, runtime 11.0.0, `StartAndForgetShould`).
- So `Desktop.StartAsync` starts the program through ShellExecute from the short-lived cleaned `pwsh` child (`StartProgramScript`, executable and arguments as Base64), whose console is hidden and soon gone; it keeps that child on .NET 11 (upgrade-to-net11's design).
- **Refusals:** in that script `catch [System.ComponentModel.Win32Exception]` catches `Process.Start`'s own refusal, so `$_.Exception.NativeErrorCode` is set: a text file named `.exe` gives 216 and a missing file 2, with no dialog (13 November 2026, pwsh 7.6).
- **An unassociated file type does not fail:** ShellExecute on one returns without an exception and Windows opens its "How do you want to open this file?" picker (1 November 2026, Windows 11 26100.9711). Never run that start to test it again: it opens the picker.
- **Asking first is harmless:** [`AssocQueryString`](https://learn.microsoft.com/windows/win32/api/shlwapi/nf-shlwapi-assocquerystringw) with `ASSOCF_INIT_IGNOREUNKNOWN` (0x400), `ASSOCSTR_PROGID` (20) and no verb answers `0x80070483` for an unassociated extension and S_OK for `.cmd`, `.exe`, `.txt`, `.sln`, `.ps1` and others; an empty extension answers `0x80070057`.
- `ASSOCSTR_COMMAND` and `ASSOCSTR_EXECUTABLE` need the verb `open`. `Desktop.OpenAsync` asks `FileAssociations` for a full file path before starting the `pwsh` child and reports only that exact HRESULT as no application; URLs are never asked about.
