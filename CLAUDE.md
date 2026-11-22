# Cader

A Terminal.Gui console for Claude Code sessions and git worktrees on Windows, installed with winget as the self-contained program `cader` (`Source191.Cader`); there is no NuGet package. The README is the front page, `docs\user-guide.md` the user guide, `CONTRIBUTING.md` the contributor's guide and `MAINTAINING.md` the owner's; `openspec/` holds the specifications and the changes made against them.

## Rules by area

Facts that matter only in part of the code are path-scoped rules in `.claude\rules`, loaded when a matching file is read. Read the one that applies before reasoning about its area from anywhere else, and **read `effects-and-views.md` before planning a new key, effect, view or dialog**. The evidence behind the verified facts (runs, probes, measurements) is in `docs\verified-facts`.

- **`effects-and-views.md`:** reducers, effect routing, the four pieces of a new key, keys and columns as data, Wording.
- **`values-and-null.md`:** text values and their three doors, sessions, null, the port contracts, closed hierarchies and switches.
- **`security.md`:** the detail behind the security and privacy invariants below.
- **`console-screen.md`** and **`console-refreshing.md`:** the layout, detail pane, status line and dialogs; refreshing, worktree scanning and the Config tab.
- **`claude-code.md`**, **`terminal-gui.md`**, **`windows.md`**, **`windows-processes.md`**, **`winget.md`:** the verified facts about the Claude Code CLI, Terminal.Gui 2.5.0, Windows Terminal and ShellExecute, Windows processes, and winget.
- **`checks.md`** and **`mutation-testing.md`:** the workflows and the hand-off, smoke and contract checks; Stryker and the break score.
- **`locale.md`:** what must stay invariant, and what another culture would need.

## Build, test, release

- **Test loop:** `dotnet build -c Debug` (warnings are errors); `dotnet test test\Source191.Cader.Core.Tests -c Debug --no-build` for the fast in-memory suite (xunit.v3 on Microsoft.Testing.Platform); `dotnet test -c Debug --no-build` at the solution for everything, real git and processes included; `dotnet format --verify-no-changes`; `Invoke-Pester test\Scripts` for the scripts.
- **Projects:** `src\Source191.Cader.Core` (Domain, Ports, Application; one package, System.CommandLine), `src\Source191.Cader.Infrastructure` (adapters), `src\Source191.Cader.Tui` (Terminal.Gui), `src\Source191.Cader` (`Program.cs` and `Composition`, the packed tool). Tests: `Core.Tests`, `Infrastructure.Tests`, `TestDoubles` (shared fakes), the `NoisyEditor` and `ParentStandIn` stand-ins; tools: `HeadlessTerminal`, `ReadmeScene`, each with tests.
- **Local install:** `tools\install-local.ps1` publishes `<Version>-local.<timestamp>` to `%LOCALAPPDATA%\Cader\local` and prints its `cader.exe`, never touching the PATH or a winget install; `-FromArtifact <folder>` installs a CI or prerelease zip. The winget build is `dotnet publish src\Source191.Cader -p:PublishProfile=win-x64` (or `win-arm64`) into `artifacts\publish`.
- **Never start `cader`**, or anything that opens tabs, from inside a Claude Code session without clearing NO_COLOR, CLAUDECODE, CLAUDE_PID and CLAUDE_CODE_*: Windows Terminal hands the caller's environment to new tabs. Cader drops them from its children (`InheritedEnvironment`), not from itself.
- **Hand-off check** after any change to attach, `cader here` or the pseudo-console: `dotnet build src\Source191.Cader -c Release`, then `dotnet run -- check` from `tools\HeadlessTerminal`; exit 0 passes. **Smoke check** of an installed tool: `dotnet run -- smoke <exe> <worktree-folder>` there. Both are in `checks.md`.
- **README images:** `tools\New-ReadmeScreenshot.ps1` remakes `assets\readme\sessions.png` and `new-session.gif` from `tools\ReadmeScene` over the made-up `scene.json`, never your own sessions. `SceneHost` copies `ConsoleComposition`'s wiring, so a change to the console's composition is made there too.
- **Contract and mutation tests** run as `checks.md` and `mutation-testing.md` say. Never add `--bg --resume --help` to the contract tests: it starts a session.
- **Release:** a pull request that changes the packaged tool (anything under `src\`, `Directory.Build.props`, `global.json`, `nuget.config`) raises `Version` in `src\Source191.Cader\Source191.Cader.csproj` (CONTRIBUTING's Versions; `0.y.z` while pre-1.0) and adds a `CHANGELOG.md` line; each merge of one is a GitHub Release, and the owner submits stable ones to winget with `publish.yml`.

## Working rules

- **OpenSpec:** every piece of work, refactors and tooling included, starts as an OpenSpec change (`/opsx:propose`); one with no behaviour change declares `skip_specs: true`. Archive it in the pull request that completes it, so `openspec\specs` describes `main`.
- **The archive is history:** search `openspec\changes\archive` only when asked or when checking why something was decided, never to orient yourself, and never read an archived design as the current code.
- **TDD:** red, green, refactor, a commit per step; no untested code except the tool project's wiring (`Program.cs` and `Composition`). Sociable tests; fakes only at boundaries (`TestDoubles`, one hand-written fake per port), never a mocking library.
- **Fixtures:** made-up Northwind.* repositories and `C:\Users\me` paths, never real ones. Never read or write the real `%LOCALAPPDATA%\Cader\config.json`, Terminal's Fragments folder, `settings.json` or `state.json`, or `~\.claude\settings.json` from a test or a session: use `InMemoryState`, `TestSettings` and a temporary root. Tests never reach GitHub (`FakePackageVersions` or a stub `HttpMessageHandler`).
- **Project files:** a `.csproj` carries no comments; what is worth recording goes in the project's `README.md`. The two `Directory.Build.props` files keep their comments beside their settings.
- **New facts:** a newly verified fact about Claude Code, Terminal.Gui, Windows Terminal or Windows is one bullet with its date and version in its rule; its evidence goes in `docs\verified-facts`. `Assert-ClaudeContextBudget.ps1` holds this file and each rule to 12 KB and 600-character lines.
- **Ownership:** Cader is the owner's personal project; every account, subscription and certificate used for it is the owner's or the project's (MAINTAINING.md, Ownership).
- **Commit only when asked.**
- **Pull requests** open with **Why** (the problem or risk today), then **Benefit** (what is better afterwards), then **What changed**, each change saying which benefit it delivers; "stricter" or "cleaner" is not a reason. Write it as a normal pull request, not a record of how it was decided.

## Code rules

Each line names the test or analyser that holds it; the failure says the rest.

- **Layers,** one namespace each, `Domain`, `Ports`, `Application`, `Adapters`, `Tui`, held by `LayeringShould`. The tool project is the one composition root, untested and wired by hand with no container; every decision it needs is a tested `Application` class. The command line is one System.CommandLine `RootCommand` in `Application\CommandLine.Build` (`effects-and-views.md` has all three).
- **C# idioms** held at `warning` in `.editorconfig`: concrete types, primary constructors, records, collection expressions, file-scoped namespaces, target-typed `new`, `is null` and `is not`, `System.Threading.Lock`, `[LibraryImport]`. A private constructor behind `From` and `TryParse` stays explicit.
- **Disposal:** whoever creates a disposable disposes it (CA2000, CA2213, off only in the Tui); a handle a call opens is released when it ends. `HandleCounts` and the allocation budgets (`TestDoubles\Allocations`) guard the hot paths.
- **Sealed** unless derived from, else `abstract` or `static` (CA1852, `SealedTypesShould`).
- **No parameter default values** under `src` (`SourceConventionsShould.GiveNoParameterADefaultValue`), but for the state-file records' `= null` (`values-and-null.md`).
- **One top-level type per file,** named after it without its arity (`OneTypePerFileShould`); a closed hierarchy's cases nest in their base; a partial type's files start with its name.
- **Banned symbols** (`src\BannedSymbols.txt`, RS0030): the clock (use `TimeProvider`), blocking calls, `Volatile`, `ArgumentNullException`, `Process.Start(string)`. A value one thread replaces and another reads is an `Application\SharedSnapshot<T>` (`SourceConventionsShould.KeepVolatileInsideSharedSnapshot`).
- **No null guards inside the solution:** nullable reference types carry null; a null from outside is handled on the line that receives it. Range and blank-text checks stay, with the throw helpers. `!` is never used.
- **Braces always;** a one-line method body is expression-bodied (test projects keep block bodies). British English in text (`SpellBritish`).
- **Closed switches:** a switch expression over a closed enum or a `closed` record names every case with no discard, so a new case fails with CS8509; Core is held at C# 14 for Stryker (`values-and-null.md`).
- **Invariant culture:** the tool runs in invariant globalisation mode. Parsing and formatting name `CultureInfo.InvariantCulture`, comparisons `Ordinal` or `OrdinalIgnoreCase`, number-aware order is `Domain\NumberAwareOrder`; times are stored in UTC and shown through `TimeZoneInfo.ConvertTime` with `TimeProvider.LocalTimeZone` (`locale.md`).
- **User-facing text** lives only in `Application\Wording` (`SentenceLiteralsShould`), never from an exception message (`effects-and-views.md`).
- **One home per fact:** settings in `Application\CurrentSettings`, read per use so a save applies at once; git's worktree facts in `WorktreeInspector`; every tab's script, title, folder and colour in `TabScripts`; the Claude CLI's state words in `Adapters\Claude\ClaudeStates`, `gh`'s and `az`'s in their pull request adapters; pull requests reach rows only through `PullRequestCatalogue`, which never makes a row wait.

## Security and privacy

The detail is in `security.md`; these hold everywhere:

- **Programs** are found once by `ProgramLocator` on the PATH's full-path folders, as an `.exe`, and run by full path; the console and `here` exit 1 when `RequiredPrograms` finds Claude Code or git missing.
- **No batch file** is given text from a person, a repository or a listing (`BatchFileArgument.IsSafe` for `az.cmd`; a custom editor is a full path to an executable). An opening prompt that starts with `-` is refused.
- **PowerShell:** user text is never interpolated into source; a tab's script is built by `TabScripts`, every value through `PowerShellScript.Quote`, and reaches `wt.exe` Base64-encoded with every semicolon escaped.
- **Processes** are ended only as a `ConfirmedProcess`, made only by its three doors; **folders** are deleted only through `IFolderDeleter`, which only `WorktreeRemover` holds, strictly inside a root, never one whose `.git` file points at a live checkout.
- **Nothing personal is recorded:** a diagnostic fact is a `DiagnosticEvent` of numbers, durations, versions, enums, `CodeName`, `ExceptionSummary` or `ChildCommand` (`DiagnosticEventsShould`, `DiagnosticsPrivacyShould`); a command line never leaves `ProcessRegistry.UsageOf`. Never log an effect's `ToString()`, an exception's message, program output or anything typed.

## Claude Code CLI

The verified facts are in `claude-code.md`. Three hold everywhere: never run `claude --bg --resume --help` (it starts a session); resuming a *listed* background job with flags starts a copy, so `claude rm <id>` it first (the transcript stays); and never script edits to Claude Code's trust records on a developer machine (only CI's throwaway runners seed them).
