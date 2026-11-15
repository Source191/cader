## 1. Set up

- [x] 1.1 Raise the version with `.github\scripts\Step-Version.ps1 -Preview` and add a Changed line to `CHANGELOG.md`; verify `Assert-VersionBumped.ps1 -Base main -Head HEAD` passes.

## 2. Every tab's tips fit in 80 characters

- [x] 2.1 Red: `WordingShould.GiveEveryKeyATip` reads the Config keys too, and `GiveEveryConfigKeyATipOfItsOwn` goes; verify it fails naming Enter's tip as too long.
- [x] 2.2 Green: Enter's tip reads "Edit the setting (all roots, one per line), or say how to change the setup." and `f`'s "Open the folder, or the settings or log file, in the editor chosen above."; verify the Core suite passes.

## 3. Prove

- [x] 3.1 Run `dotnet build -c Debug`, `dotnet test -c Debug --no-build` and `dotnet format --verify-no-changes`; verify all pass and `openspec validate shorten-config-key-tips` passes.

## Workflow follow-up

- Archive the change in the pull request that completes it.
