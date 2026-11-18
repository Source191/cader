# Tasks

Implement with Claude Sonnet 5.5 (`claude-sonnet-5-5`) at medium effort (design.md, Implementation).

## 1. The leading-invisible rule (Domain)

- [x] 1.1 Red: `CharacterKindsShould.DropOnlyTheInvisibleCharactersAtTheStart` (theory: `""` → `""`, `" "` → `""`, `"​\u001B "` → `""`, `" orders"` → `"orders"`, `"​orders"` → `"orders"`, `"d o"` → `"d o"`, `"my "` → `"my "`, `" fix login"` → `"fix login"`). Green: `CharacterKinds.WithoutInvisibleStart(string)` over runes with `Of`. Verify the Core suite passes.

## 2. The tables' `/` filter (Domain)

- [x] 2.1 Red: `RowFilterShould` facts that a filter of `" "` or `"​"` keeps every row, `" templates"` keeps the rows `"templates"` keeps, and `"fix login"` keeps `fix login` but not `fix-login`. Green: `RowFilter.Matches` applies `WithoutInvisibleStart` and keeps every row when it is empty, replacing its `IsNullOrWhiteSpace` check. Verify the Core suite passes, including the Sessions, Worktrees and Config view facts that filter.

## 3. The wizard's repository filter (Application)

- [x] 3.1 Red: `RepositoryCatalogueShould` (or the fact that already drives `RepositoryChoices.Matching`) asserts `" "` and `"​"` keep every choice in every group, `" orders"` keeps what `"orders"` keeps, and `"d o"` keeps `Northwind Orders` but not `Northwind.Orders`. Green: `RepositoryChoices.Matching` applies `WithoutInvisibleStart` once and passes the result to `RepositoryChoice.Matches`. Verify the Core suite passes.
- [x] 3.2 Red: `WizardViewShould.ListEveryRepositoryForAStraySpace` (the filter receives `" "`; `Filter` is still `" "`, every choice row is listed and `Problem` is `WizardProblem.None`), replacing `ExplainAFilterOfInvisibleCharacters` and `ClearTheInvisibleFilterProblemOnlyWhenTheFilterShowsSomething`. Green: delete `WizardView.FilterProblem` so `Filtered` leaves `Problem` alone, then delete `WizardProblem.InvisibleFilter`, its arm in `Wording.Describe` with `WordingShould.DescribeAnInvisibleFilter`, and `CharacterKinds.AllInvisible` with `CallATextInvisibleOnlyWhenEveryCharacterIs`. Verify the Core suite passes, the reflection facts (`StatusTonesShould`, `WordingShould`) included.

## 4. Typed session names (Domain and Application)

- [x] 4.1 Red: `SessionNameShould` facts that `Validate(" fix-login ")` is `Valid` with the name `fix-login`, `Validate(" fix the login ")` keeps `fix the login`, `Validate("   ")` is `Refused(NameProblem.Missing)`, and `TryParse(" listed ")` keeps the listed text unchanged. Green: `Validate` checks and builds from `text.Trim()`. Verify the Core suite passes.
- [x] 4.2 Red: `LaunchPlanShould` asserts a request named `" fix-login"` plans the session `fix-login` (the wizard's fixed-folder case), and a `HereCommand` fact asserts `cader here " fix-login"` plans `fix-login`. Green: nothing beyond 4.1 should be needed; then remove `.Trim()` from `SessionsView.RenameWith`, keeping its rename facts green. Verify the Core suite passes.

## 5. Smoke check, rules and release notes

- [x] 5.1 `tools\HeadlessTerminal\SmokeScript.cs`: the Before branching step sends Esc instead of a space and the invisible-filter step goes; `SmokeScriptShould` follows (its step texts and indices). Verify `dotnet test tools\HeadlessTerminal.Tests -c Debug` passes.
- [x] 5.2 `.claude\rules\terminal-gui.md` (the decompiled `TextField` fact ends by saying leading invisible characters are ignored by both filters, not that the problem line explains them) and `.claude\rules\checks.md` (the smoke list no longer types a space into the wizard's filter). Verify `Grep InvisibleFilter` finds nothing outside `openspec\changes\archive` and this change.
- [x] 5.3 `CHANGELOG.md` under Unreleased, Fixed: a stray space typed with `n` no longer empties the new-session wizard's list, a table's `/` filter ignores spaces before the text, and a new session's name loses spaces at either end (#339); raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` as CONTRIBUTING's Versions says. Verify `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes` and `openspec validate ignore-stray-leading-blanks` pass.

## Workflow follow-up

- Archive the change in the same pull request, so `openspec\specs` describes the code on `main`.
