## Why

Cader's Core project is held at C# 14 because the mutation testing tool cannot compile C# 15 yet: Stryker.NET 5.0.0 and 5.1.0 (the last release on nuget.org on 14 November 2026) bundle Roslyn 5.9.0, which reads C# 15 syntax in Core as an error and stops the whole `mutation.yml` run. As a result Core's type hierarchies are not `closed`, its switches keep throwing fallback arms, and the rest of the code base (Infrastructure, Tui, the tool project, the tests and tools) is on C# 15 while Core is not. Left alone, the gap is easy to forget once `upgrade-to-net11` is archived.

## What Changes

- Raise Core's `LangVersion` from 14 to `latest` (C# 15) once a Stryker release compiles C# 15.
- Mark Core's Domain and Ports hierarchies `closed`, and turn `ClosedHierarchiesShould` to the `IsClosedTypeAttribute` marker.
- Drop the `UnreachableException` discard arms over Core's closed types, including those in Infrastructure, Tui and `LaunchParts`, and widen `SourceConventionsShould.DeclareEveryHierarchyOutsideCoreClosed` to Core's two-case hierarchies.
- Check whether `ChildCommandWords`, `ChildCommand` and `PullRequestCatalogue` can take collection expression arguments (`with(...)`), and adopt it only where IDE0028 asks and the targets allow it.
- No change a user can see.

## Impact

- **Gate:** a Stryker.NET release that compiles C# 15 (checked by a local run, then a `mutation.yml` dispatch on the branch). Until then nothing here starts.
- **Evidence:** `openspec\changes\archive\<date>-upgrade-to-net11\design.md` (the C# 15 section and the Stryker risk) and `.claude\rules\checks.md`, which records the steps and the reason Core is held.
- **Code:** `src\Source191.Cader.Core` and the switches over its types elsewhere; the architecture and source-convention facts named above.
