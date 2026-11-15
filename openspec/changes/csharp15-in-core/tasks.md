## 1. Gate

- [ ] 1.1 Confirm a Stryker.NET release compiles C# 15 (a local run scoped to a few Core types, then a `mutation.yml` dispatch); pin it in `.config\dotnet-tools.json`.

## 2. Core on C# 15

- [ ] 2.1 Raise Core's `LangVersion` to `latest` and remove the reason from its README. Verify the build and the Core tests pass.
- [ ] 2.2 Red: turn `ClosedHierarchiesShould` to the `IsClosedTypeAttribute` marker, failing for Core's hierarchies. Green: mark each Domain and Ports hierarchy `closed`, one type per commit.
- [ ] 2.3 Remove the throwing discard arms over Core's closed types (Core, Infrastructure, Tui, `LaunchParts`) and widen the source-convention fact. Verify there is no CS8509.
- [ ] 2.4 Check `ChildCommandWords`, `ChildCommand` and `PullRequestCatalogue` for `with(...)`; adopt or record why not.
- [ ] 2.5 Update `CLAUDE.md` and `.claude\rules\checks.md`; run the mutation check and record the scores.
