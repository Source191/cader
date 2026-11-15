# Design

## Context

Stryker writes each mutant as a runtime switch (`ActiveMutation == n ? mutated : original`). A switch inside a method is read on every call, so the mutant is tested whenever a covering test runs. A switch inside a static initialiser is read once, when the type is first touched in the test process, and the value is cached for every later mutant; Stryker marks such mutants `static`, runs them against all tests and records no `coveredBy` (see proposal.md, Why). It also marks as static any mutant in Core code that an initialiser calls, which is why `ReleaseVersion`, `ReportTitle`, `CodeName`, `EditorChoice`, `DisplayState` and the view states' list policies appear in the full run's 369 static mutants beside the literals.

Core holds about 120 static initialisers. About 50 produce mutants today; the rest (`TimeSpan` constants, `Key` values, sentinels such as `AutomatedRuns.None`) produce none, but would as soon as someone added a string or a call.

## Goals / Non-Goals

**Goals:**

- No mutant in Core is static, so a `--since` run and a full run agree, and every kill names a test that reads the value.
- One rule, simple enough to hold by a syntax check.

**Non-Goals:**

- Caching any value for speed. Nothing read through these properties is on a path where an allocation per read can be measured.

## Decisions

- **Every Core static initialiser becomes a computed property, not only those with mutants today.** The alternative, converting only the ~50 that produce mutants, needs a guard that knows what Stryker mutates (literals, collections, booleans, any call into Core, any read of a Core property) and would miss the next case it does not know. "No static initialiser in Core" is one syntax check. `const` stays: Stryker does not mutate a `const` declaration, and a constant is already compiled into each reader.
- **A kept list for identity.** `StatusMessage.None` stays a field because `ConfigView`, `SessionsView`, `WorktreesView` and `ConsoleScreen` compare messages with `ReferenceEquals`, and `DetailRequests`' `NoRow` is an `object` whose only meaning is its identity. Neither holds anything Stryker mutates. The guard names both with a one-line reason, as `NullFreeModelsShould` keeps its `Kept` list.
- **Arrays become `IReadOnlyList<T>`** where a field becomes a property, so a property never hands out an array (CA1819) and a reader cannot change a table it was given.
- **Sentinels keep value equality.** Every converted sentinel is a record whose members are values, enums or empty collections; an empty collection expression for `IReadOnlyList<T>` is `Array.Empty<T>()`, one shared instance, so `ReleaseNotes.None == ReleaseNotes.None` stays true and `UpdateCheck`'s `recorded.Notes != ReleaseNotes.None` keeps its meaning. A sentinel that turns out not to compare equal after conversion is found by the existing facts that compare against it (`ShouldBe(ConsoleState.Empty)`, `ShouldBe(UserConfig.Missing)` and others), and is fixed by giving the comparison a named question rather than by keeping the field.
- **`BatchFileArgument`'s `SearchValues` is built per call.** `IsSafe` runs once per `az` argument list, so building the search set each time costs nothing measurable, and the alternative (a `const` string behind a cached `SearchValues`) would keep the literal out of mutation altogether.
- **The order: a spike, then the guard, then the files.** `Wording.cs` is converted first and Stryker is run with `--since:main`, the release run's own mode (a full run already kills these mutants, so it proves nothing): its mutants must show a `coveredBy`, no `static` flag and status Killed. If they do not, the change stops and falls back to excluding static mutants from release runs. The guard fact is written next, red against the remaining files, and each group of files then turns part of it green.

## Risks / Trade-offs

- [A property read inside a test class's static field still runs the value once per process] → the guard covers only Core; a fact that needs a converted value calls it inside the fact, as `checks.md` already advises.
- [Value equality breaks for a sentinel with a non-empty list member] → the facts above compare the sentinels; any failure is fixed at the comparison.
- [Removing the Accepted marks exposes a mutant no fact kills] → the marks' reasons name the facts that pin each value; a scoped run of the four files must show every mutant killed before the marks' removal is committed.
- [A local scoped run is unreliable for static mutants] → after conversion there are none; the proof is that `coveredBy` is filled, which a local run shows reliably, and a runner full run confirms.

## Migration Plan

None: an internal refactor released as a preview. Rolling back is reverting the pull request.
