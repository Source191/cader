# Tasks

## 1. Spikes

- [ ] 1.1 From a throwaway console that composes `ConsoleScreen` with the TestDoubles fakes, open two tabs through the real `WindowsTerminal` adapter from a freshly created `wt -w new` window and confirm both land in that window, not an older one; record the result with its date and Windows Terminal version in `.claude\rules\windows.md`, and stop and re-plan with the owner if they land elsewhere
- [ ] 1.2 In the same throwaway console, drive the new-session wizard with `NewKeyDownEvent` (`n`, filter text, Tab, a name, F5) and confirm it reaches the scripted launch exactly as typed keys do; record the result in `.claude\rules\terminal-gui.md` (Terminal.Gui 2.5.0), and fall back to `WriteConsoleInput` after `AttachConsole` if it does not

## 2. Scene model (tools\ReadmeScene, test first)

- [ ] 2.1 Create `tools\ReadmeScene` and `tools\ReadmeScene.Tests` (xunit.v3, Shouldly, invariant globalisation, referencing Core, Infrastructure, Tui and TestDoubles), add both to the solution and verify `dotnet build -c Debug` passes with warnings as errors
- [ ] 2.2 Test `SceneFileShould.ReadTheRepositoriesSessionsAndPullRequests`, then write `SceneFile` reading `scene.json`; verify the test passes
- [ ] 2.3 Test `SceneFileShould.PlaceEachStartTimeMinutesBeforeNow` against a fixed `FakeTimeProvider`, then the relative-time rule; verify it passes
- [ ] 2.4 Test `SceneFileShould.RefuseARealLookingPath` (any path outside the scene root, or a repository name other than Northwind.* and `vantage`), then the check; verify it passes
- [ ] 2.5 Write `scene.json` with the sessions, states, kinds, automated run and pull requests the design lists, and verify `SceneFileShould.ReadTheShippedScene` loads it with every session state present

## 3. Scripted programs (test first)

- [ ] 3.1 Test `SceneProcessRunnerShould.AnswerTheAgentsListing` (the JSON `ClaudeAgentsListing` reads back into the scene's rows through the real `ClaudeCli`), then `SceneProcessRunner`'s listing rule; verify it passes
- [ ] 3.2 Test `SceneProcessRunnerShould.AnswerTheClaudeVersion`, then the rule; verify it passes
- [ ] 3.3 Test `SceneProcessRunnerShould.ListAStartedSessionNextTime` (a `--bg -n` start appears in the next listing as starting, then working), then the rule; verify it passes
- [ ] 3.4 Test `SceneProcessRunnerShould.AnswerGitHubPullRequests` through the real `GitHubPullRequests`, then the `gh pr list` rule; verify it passes
- [ ] 3.5 Test `SceneProcessRunnerShould.AnswerAzureDevOpsPullRequests` through the real `AzureDevOpsPullRequests`, then the `az repos pr list` rule; verify it passes
- [ ] 3.6 Test `SceneProcessRunnerShould.RefuseAnythingElse` (stop, rm, attach and any other program exit non-zero and are never run), then the refusal; verify it passes

## 4. Keys and frames (test first)

- [ ] 4.1 Test `KeyScriptShould.ReadDelaysKeysAndText`, then `KeyScript` reading `new-session.keys`; verify it passes
- [ ] 4.2 Test `FrameScheduleShould.TakeAFrameEveryHundredMilliseconds` and `StopAfterTheLastKeyAndHold`, then `FrameSchedule`; verify it passes
- [ ] 4.3 Write `new-session.keys` (filter to Northwind.Inventory, name `add-stock-alerts`, a short prompt, F5, a pause on the new tab, back to Vantage) and verify `KeyScriptShould.ReadTheShippedScript` loads it

## 5. The host and capture (untested wiring)

- [ ] 5.1 Write the `run` command composing `ConsoleScreen` and `ScreenLoop` as the design says (scene runner, real git, `InMemoryState` seeded, `FakeTranscripts`, `FakeProcessRegistry`, real `WindowsTerminal` and `TabScripts` with `ReadmeScene.exe` as both executables, update check off, no log); verify by running it in a `wt -w new --size 160,40` window against a hand-built demo folder and seeing every scene row
- [ ] 5.2 Write the `attach` placeholder and the no-op `claim-tab`; verify a session tab opened by the host shows the placeholder and its colour matches the row's TAB swatch
- [ ] 5.3 Write the start-up sequence (open the scene's tabs, focus Vantage's tab with `FocusTabAsync`, select the waiting row, write the ready marker); verify the marker appears and the detail pane shows the waiting reason
- [ ] 5.4 Write the `capture` command (`PrintWindow` with `PW_RENDERFULLCONTENT`, one PNG or a frame series on `FrameSchedule`); verify a single capture of the host's window matches what is on screen, with the workstation locked
- [ ] 5.5 Write the key player that feeds `KeyScript` through the mechanism chosen in 1.2 while frames are taken; verify the frames show the wizard and the new tab
- [ ] 5.6 Write `tools\ReadmeScene\README.md` (what it is, what it never touches, the untested wiring and why, how to change the scene) and verify `OneTypePerFileShould` and `dotnet format --verify-no-changes` pass

## 6. The script and the images

- [ ] 6.1 Write `tools\New-ReadmeScreenshot.ps1`: refuse under `CLAUDECODE`, check `wt`, git and ffmpeg, build the demo repositories under `-SceneRoot` (default `C:\VantageDemo`) with the made-up author and remotes, open the window with the cleared environment, wait for the marker, capture the PNG, play and capture the GIF, close the window and delete the scene folder; verify a run leaves only `assets\readme\sessions.png` and `assets\readme\new-session.gif` changed and nothing in `%LOCALAPPDATA%\Vantage` or `~\.claude` touched (compare their last-write times before and after)
- [ ] 6.2 Run the script and commit both images; verify by eye that no real repository other than `vantage`, person, organisation or user path appears, and that the GIF is under 2 MB

## 7. User guide

- [ ] 7.1 Write `docs\user-guide.md` with the sections the design lists, taking the keys table, notifications and profiles, without winget, previews, upgrading and uninstalling from the README and the key and column lines from `Wording`; verify every key in `Wording.SessionsKeys`, `WorktreesKeys` and `ConfigKeys` appears in it
- [ ] 7.2 Verify the notifications section names `vantage setup-terminal` as the one step for both bells (spec: Reading the notifications section)

## 8. README

- [ ] 8.1 Rewrite `README.md` in the design's order: the problem, the screenshot, what it does, why I built it, support with the QR code, install (winget first, then Claude Code), getting started with the GIF and its five steps, learn more with the user guide first; verify the install section still names only `winget install Source191.Vantage` and Claude Code 2.1.309 or later (installation spec) and `DocumentedMinimumShould` passes
- [ ] 8.2 Verify the getting started's profiles step names `vantage setup-terminal` and links to the guide's notifications section (spec: Reading the getting started), that `SpellBritish`-style British spelling holds by reading it through, and that no em dash appears
- [ ] 8.3 Add the "Built by Simon Vane" badge beside the sponsor badge and the signed line at the end of "Why I built it", both linking to `https://www.linkedin.com/in/simonvane/`; verify both render and open the profile on GitHub's preview
- [ ] 8.4 Verify every link and image in the README and the guide resolves (the raw image URLs once merged; relative links now)

## 9. Contributor docs

- [ ] 9.1 Change `CLAUDE.md`'s opening and `CONTRIBUTING.md` to say the user guide is `docs\user-guide.md` and the README the front page, and add to CONTRIBUTING how to regenerate the images with `tools\New-ReadmeScreenshot.ps1`; verify by reading both
- [ ] 9.2 Add `tools\ReadmeScene` and `tools\ReadmeScene.Tests` to `CLAUDE.md`'s project list; verify by reading it

## 10. Checks

- [ ] 10.1 Run `dotnet build -c Debug`, `dotnet test -c Debug --no-build` and `dotnet format --verify-no-changes`; verify all pass
- [ ] 10.2 Run `openspec validate readme-front-page`; verify it passes
- [ ] 10.3 Confirm nothing under `src\` changed, so `Assert-VersionBumped.ps1` asks for no version raise

## Workflow follow-up

- Archive the change in the pull request that completes it, after review.
- Retarget the pull request to `main` once #206 has merged.
