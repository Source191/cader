# Tasks

## 1. The rule

- [x] 1.1 **Red:** `SourceConventionsShould.GiveNoParameterADefaultValue` fails, naming each parameter under `src` with a default value outside the state-file records. **Green:** remove every default and pass each call site its value, naming the argument where a literal is unclear. **Verify:** the fact, `dotnet build -c Debug` and `dotnet test -c Debug --no-build` pass.

## 2. Documentation and release

- [x] 2.1 CLAUDE.md C# idioms: no default parameter values, the state-file records excepted.
- [x] 2.2 Raise `Version` one step and add the `### Internal` changelog line; verify `Assert-VersionBumped.ps1` and `Assert-ChangelogUpdated.ps1` pass.
- [x] 2.3 Verify `dotnet format --verify-no-changes` and `openspec validate --all` pass, and archive the change in this pull request.
