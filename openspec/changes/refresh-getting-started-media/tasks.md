# Tasks

## 1. Preconditions

- [ ] 1.1 Confirm the package is live: `winget search Source191.Cader` lists it and `winget install Source191.Cader` on a clean machine or Windows Sandbox installs a `cader` that runs; record the version in the pull request, and stop and wait if it does not resolve
- [ ] 1.2 Confirm `readme-front-page` is archived (or rebase on it), so the README sections this change edits are final; verify with `openspec list` showing it gone

## 2. Key script (tools\ReadmeScene, test first)

- [ ] 2.1 Test `KeyScriptShould.ReadARestartStep`, then add the `restart` step to `KeyScript` and `KeyStep`; verify the test passes
- [ ] 2.2 Test `KeyScriptShould.RefuseARestartWithArguments`, then the parse rule; verify it passes
- [ ] 2.3 Add the scene's recording `IDesktop` (opens nothing, remembers the URL) and a test `SceneDesktopShould.RememberTheUrlAndOpenNothing`; verify it passes
- [ ] 2.4 Wire `restart` into `KeyPlayer` and `SceneHost` (stop the console, start it again over the same scene state); verify with `dotnet build -c Debug` and by running the script by hand that the second console lists the same rows

## 3. Scene and script

- [ ] 3.1 Add a pull request to the new session in `scene.json` and extend `SceneFileShould` for it; verify `dotnet test tools\ReadmeScene.Tests -c Debug` passes
- [ ] 3.2 Extend `new-session.keys`: wizard, pull request key, `restart`, Enter to resume, holding each result about 1.5 seconds; verify the recording is 15 to 20 seconds
- [ ] 3.3 Make `profiles.png` from the scene window's new-tab menu through `tools\New-ReadmeScreenshot.ps1` (a new `Profiles` value for `-Only`); verify by eye that it names only the Cader profiles and made-up entries
- [ ] 3.4 Run `tools\New-ReadmeScreenshot.ps1`; verify `new-session.gif` is under 1.5 MB, every frame shows only Northwind.* names and made-up paths, and the owner's own state, `~\.claude` and Windows Terminal fragments are untouched

## 4. Docs

- [ ] 4.1 README: check the install and `cader setup-terminal` blocks against the live package, add `profiles.png` beside the setup step and update the GIF's alt text to the new story; verify by rendering the README on GitHub in the pull request preview
- [ ] 4.2 Update `tools\ReadmeScene\README.md` (the `restart` step, the recording `IDesktop`, `profiles.png`) and CONTRIBUTING's "when to remake the images"; verify the links resolve
- [ ] 4.3 Add a "Walkthrough video" section to `docs\user-guide.md` with the shot list and the pre-flight checklist from the design; verify the checklist covers a clean Cader state, a throwaway Northwind repository, a made-up profile list, no visible personal data and no audio

## 5. Walkthrough video (owner, optional)

- [ ] 5.1 Owner records the MP4 or WebM from `winget install` to resuming, following the checklist, and uploads it as a GitHub attachment; verify by playing it back in full before linking
- [ ] 5.2 Link the video from `docs\user-guide.md` and the README's getting started; verify the link plays when signed out of GitHub

## 6. Finish

- [ ] 6.1 `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes` and `openspec validate refresh-getting-started-media` pass; archive the change in the same pull request (no `src\` file changes, so no version bump)
