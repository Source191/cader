# Design

## Context

PR L of `upgrade-to-net11` found that Stryker.NET compiles the mutated Core with its own Roslyn (5.9.0 in 5.0.0 and 5.1.0), which does not know C# 15, so Core stays at `<LangVersion>14</LangVersion>` (reason in the README of `Source191.Cader.Core`). The evidence, the scratch-project checks and the 53-mutant Stryker run are in the C# 15 section of the archived `upgrade-to-net11` design and in `.claude\rules\checks.md`.

## Decisions

- **Gate first:** do not start until a Stryker release bundling a Roslyn that reads C# 15 exists. Confirm with a local run scoped to a few Core types, then a `mutation.yml` dispatch on the branch; bump the pin in `.config\dotnet-tools.json` in the same pull request.
- **Order:** raise `LangVersion`, then each hierarchy `closed` one type per commit, then remove the discard arms, then the `with(...)` check, each red-green as the repository's TDD rule says.
- **Facts follow the code:** `ClosedHierarchiesShould` expects the marker on every Domain and Ports hierarchy; `DeclareEveryHierarchyOutsideCoreClosed` is widened (and renamed if it no longer says "outside Core").
- **Decline rule:** a `with(...)` that IDE0028 does not ask for, or whose target (`IReadOnlySet`, `ConcurrentDictionary`, an indexer initialiser) cannot take it, is declined and recorded.

## Risks

- A Stryker release that reads C# 15 may change mutant counts; record the before and after scores as `upgrade-to-net11` did.
- Open hierarchies that grow a case per feature keep their fallback arms (`CLAUDE.md`); do not "fix" them.
