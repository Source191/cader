# Tasks

## 1. The rule and its words (Application)

- [x] 1.1 Red: `ConsoleLoadsShould.MarkARepositoryRootThatIsNotFound` reads the Config rows with two roots, one a folder in `FakeFolders`, and asserts the other's `Found` is false and the first's true, and `ConfigRowsWhileReading` marks neither. Green: `ConfigRow.RepositoryRoot.Found` (true unless the read found no folder) and `ConsoleLoads` checking each root through `IFolders`. Verify the Core suite passes.
- [x] 1.2 Red: `WordingOfConfigShould` asserts a root not found reads `D:\work (not found)` and its description names the fix. Green: `Wording.ConfigValue` and `ConfigDescription`. Verify the Core suite passes.

## 2. Wiring and documents

- [x] 2.1 `ConsoleComposition` and `SceneHost` pass the folders to `ConsoleLoads`; a `CHANGELOG.md` entry; the version raised. Verify `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes` and `openspec validate config-marks-missing-root --strict` pass.

## Workflow follow-up

- Archive the change in the same pull request.
