# Proposal

## Why

A release run of `mutation.yml` mutates only the code changed since the last run (`--since`), and it reported 223 survivors in v0.22.0-preview.19 (run 43720610875) that the full run on the same code (43713303950) kills: every one is a static mutant, 219 of them in the `Wording` tables. A static field or auto-property initialiser runs once per test process, and Stryker's MTP runner switches mutants without restarting it (stryker-mutator/stryker-net#3742), so a mutant there is only killed when the process happens to start with it active, and no test can be credited with the kill. Today every release that touches a `Wording*` file fails its run with findings that are not real, and the four diagnostic tables carry 15 `Accepted:` marks that hide 61 mutants for the same reason.

## What Changes

- **Core declares no static initialiser:** each `static readonly` field and `{ get; } =` property in `src\Source191.Cader.Core` becomes a computed property (`static T Name => ...`), so its value is built each time it is read and Stryker sees its mutants as ordinary covered code, attributed to the tests that read them. Code that ran only inside an initialiser (`ReleaseVersion` parsing for `ClaudeCodeSupport.Minimum`, `ReportTitle` for `ProblemReport`, `CodeName`, `EditorChoice.Of`, the list policies of the view states) stops being static as well.
- **A short kept list:** an initialiser stays only where identity is the point (`StatusMessage.None`, which the reducers and the screen compare by reference, and `DetailRequests`' `NoRow` object), each named with its reason.
- **A guard:** a `SourceConventionsShould` fact fails for a static initialiser in Core that is not on the kept list, so the problem cannot come back.
- **The Accepted marks go:** the 15 static-initialiser marks in `ChildCommand`, `ChildCommandWords`, `DiagnosticLine` and `ExceptionSummary` are removed; facts already pin every value.
- **Docs:** `.claude\rules\checks.md` (the static-mutant entries, the follow-up on #3742, the ledger's Accepted count back to 1), `CLAUDE.md`'s C# idioms, and an Internal changelog line.

## Out of scope

- **Projects other than Core**, which Stryker does not mutate; their static fields stay.
- **The mutation workflow and its scripts:** with no static mutants left in Core there is nothing for a release run to report wrongly, so `Write-MutationRun.ps1` is not changed to skip them.
- **The `Accepted:` mark on `ExceptionSummary`'s line-0 frame**, which is not about static initialisers.
- **Multiple Claude accounts**, which are later changes.
- **Closing #262:** that follows the runner runs this change records.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None: nothing a user can see changes, so `.openspec.yaml` declares `skip_specs: true`.

## Impact

- **Code:** about 120 declarations in about 50 Core files, mostly `Wording*`, `TabPalette`, `Key`, `ChildCommandWords`, `DisplayState`, `Heartbeat` and the view states; callers are unchanged, because a property reads like the field it replaces.
- **Tests:** a new fact in `test\Source191.Cader.Infrastructure.Tests\SourceConventionsShould.cs`; existing facts that compare against a sentinel keep passing because the sentinels are records with value equality.
- **Runtime:** small allocations where a table is read (the key bar and help on each render, `TabPalette.Default`, the diagnostic word sets per child command), which do not matter at Cader's scale.
- **Version:** a preview raise with an Internal changelog line, because files under `src\` change.
