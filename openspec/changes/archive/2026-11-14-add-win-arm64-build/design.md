# Design

## Context

`pipeline.yml` builds one zip on `windows-latest` (x64), smoke-tests it and attaches it; `publish.yml` downloads that one zip, verifies it and runs `wingetcreate update --urls "<zip>|x64"`. Both look for exactly one `cader_*_win-x64.zip`. The one native dependency, `libonigwrap.dll` (the `onigwrap` package, 1.0.11), already ships `runtimes\win-arm64\native`.

## Decisions

- **A profile per architecture:** `win-arm64.pubxml` beside `win-x64.pubxml`, differing only in `RuntimeIdentifier`; each setting's reason stays in `src\Source191.Cader\README.md`. A profile each keeps the publish command the same shape (`-p:PublishProfile=<rid>`) and `build-test-pack` takes the rid as an input.
- **Build and test on the architecture's own runner:** the smoke check starts the real executable, so an Arm zip cannot be checked on an x64 runner. The check job becomes a matrix of `windows-latest` and the hosted `windows-11-arm` runner; each leg builds, zips, unzips, checks `--version` and runs the smoke check. Cross-publishing the Arm zip from x64 and trusting it unrun was rejected: the point of the change is a build that is known to start.
- **The test suite runs once:** the unit and adapter tests do not depend on the architecture, so only the x64 leg runs them; the Arm leg builds, publishes and smoke-checks. This keeps the second runner short.
- **One release, two zips:** the release job waits for both legs, attests each zip and attaches both in one `gh release create`. It looks for exactly one zip per architecture, not one zip in all.
- **One manifest, two installers:** `wingetcreate update --urls "<x64 zip>|x64" "<arm64 zip>|arm64"`. The `publish.yml` provenance check runs on each zip.
- **Portable zip layout is unchanged:** same five files, same alias.
- **Alternative: ship x64 only and rely on emulation:** the status quo; rejected for the reasons in the proposal, though it stays acceptable if the Arm runner proves unavailable.
- **Alternative: a universal build:** .NET has no fat binary, so two builds are the only way.

## Risks and open questions

- **[Arm runner availability]** `windows-11-arm` hosted runners are free for public repositories only; if the repository or its plan cannot use one, the Arm leg cannot run, and the fallback is the status quo above. Confirm before the first task.
- **[Terminal.Gui and the pseudo-console on Arm]** the hand-off check and smoke check are the evidence; a failure there is a real Arm bug and is fixed or recorded in `windows.md` before release.
- **[Dependency availability on Arm]** `Microsoft.WindowsTerminal`, `Microsoft.PowerShell` and `Git.Git` must each have an `arm64` installer in winget; verify with `winget show` on an Arm machine, because a missing one would break the dependency on Arm.
- **[Release time]** the check now waits for the slower of two legs.
