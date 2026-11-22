---
paths:
  - "src/Source191.Cader.Core/Domain/**"
  - "src/Source191.Cader.Core/Ports/**"
  - "test/Source191.Cader.Core.Tests/Architecture/**"
  - "test/Source191.Cader.Core.Tests/Domain/**"
  - "test/Source191.Cader.Infrastructure.Tests/SourceConventionsShould.cs"
  - "**/JsonFileStore*.cs"
  - "**/UserConfig*.cs"
  - "**/EditorSetting*.cs"
  - "**/TabFile*.cs"
  - "**/UpdateCheckFile*.cs"
  - "**/LastRunFile*.cs"
  - "**/SettingsFile*.cs"
  - "**/IGit.cs"
  - "**/IClaudeCli.cs"
---

# Values, sessions and null

The shapes the solution's own types take. The one-line rules every file follows are in `CLAUDE.md`.

## Text values

- **Values that look like strings are types:** folders are `FolderPath` (the only place two paths are compared); ids are `SessionId`, `JobId` and `BranchName`; a session's listed name is a `ListedName`, `Named(SessionName)` or `Unnamed`, decided once in `ClaudeAgentsListing`; its worktree part is a `WorktreeSlug`; a tab's owner is a `TabOwner`.
- **Ports take the Domain value** where one exists (`SessionId`, `JobId`, `FolderPath`, `BranchName`, `SessionName`, `ListedName`, `OpeningPrompt`) and the adapter reads `.Value`. `PortParametersShould` fails for a string parameter named for one of them; `IGit` is kept and migrates in its own change.
- **An unnamed session** shows blank through `Wording.Name`.
- **Three doors, one spelling each:** each text value implements `ITextValue<T>`. The static `From` builds it or throws, for text the code already trusts (the constructor is private); `TryParse(string?, out T)` answers false, for text from outside; `Validate` returns the reason as a value, for text a person typed (`SessionName`, `ReportTitle`).
- **No other door:** no static member answers a null instance and there is no `Parse` or `Of`; `TextValuesShould` enforces both, and `Parsed<T>()` keeps the texts of a sequence that parse.
- **Try methods:** a Try method's `out` parameter is a non-nullable reference type `T` marked `[MaybeNullWhen(false)]`, never `[NotNullWhen(true)] out T?`, as `ITextValue` and the base class library's `IParsable` declare it; callers declare `out T? x`. `NullFreeModelsShould` keeps it in Core, review in `Infrastructure` and `Tui`.

## Sessions

- **A session** is a `Session.Background` (a job id and a `JobProcess`: `Running` with its id, or `None` once stopped) or a `Session.Interactive` (always a process).
- **An effect carries the kind it acts on,** so nothing checks a kind flag or forgives a null job id.

## Null

- **Null never means anything** in the solution's own models and results: absence is a named case or a null object (`WizardEffect.None`, `StatusMessage.Blank`). Null is met only on the line that receives it from outside (JSON, Terminal.Gui, System.CommandLine, the base class library, the nullable ports). `!` is never used (`SourceConventionsShould.UseNoNullForgivingOperator`).
- **State files:** `JsonFileStore` refuses a file that holds `null` or leaves out a value its record has no default for, or names a key twice (`RespectNullableAnnotations`, `RespectRequiredConstructorParameters`, `AllowDuplicateProperties = false`), and reads it as corrupt, so a state record's nullable member takes an explicit `= null`.
- **Those records** (`UserConfig`, `EditorSetting`, `TabFile`, `UpdateCheckFile`, `LastRunFile`) are the one exception to "no parameter has a default value": the `= null` marks an optional JSON member.
- **`NullFreeModelsShould`** fails for a nullable public member in `Domain`, `Ports` or `Application` unless it is a house shape (`TryParse`, a Try method's `[MaybeNullWhen(false)]` `out`, `Equals`, `CompareTo`, the equality operators, compiler-generated record members) or on its `Kept` list with a one-line reason; `KeepNoStaleEntry` fails for an entry no longer nullable. A new nullable is argued for there, in review.

## Port contracts

- **`IGit`'s** commands throw `GitException` when git fails or answers with something unreadable, its nullable queries answer null, and `IsMerged` answers false; each method says which.
- **`IClaudeCli`'s** `ListSessionsAsync`, `StopAsync` and `RemoveAsync` throw `ClaudeCliException` with claude's own words when claude exits with an error; the start and resume answer a result and `VersionAsync` answers `NotRead`; each method says which.

## Closed hierarchies and switches

- **A closed enum:** a switch expression over one names every member and has no discard arm (CS8524 is off), so a new member fails the build with CS8509 at every switch.
- **Outside Core** a hierarchy is a `closed` record (C# 15), and a switch expression over one names every case with no discard arm (`SourceConventionsShould.DeclareEveryHierarchyOutsideCoreClosed` and `KeepNoThrowingDiscardInASwitchOverAClosedType`). A switch statement over one keeps its throwing `default`, because the compiler checks no statement.
- **In Core,** held at C# 14 until a Stryker release compiles C# 15 (`mutation-testing.md`), a closed hierarchy's cases live inside their base (`ClosedHierarchiesShould`), and a switch over a two-case record hierarchy (`SessionNameValidation`, `LaunchPlanResult`, `NewestRelease`) names both cases with a discard arm that throws `UnreachableException`. `DiagnosticLine`'s discard throws it too.
- **Open hierarchies** (`StatusMessage`, `Outcome`, `LaunchProblem`, `WizardProblem`, `WizardInput`) keep a delegating or fallback arm: `effects-and-views.md`.
