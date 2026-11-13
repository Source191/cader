# Proposal

## Why

The nightly `claude-contract.yml` run has failed every night since 3 November 2026 (issue #215, CLI 2.1.317 to 2.1.322), always in `ClaudeContractShould.RemoveTheSessionByItsJobId` with `ClaudeCliException : No job matching '<id>'` thrown from its `finally` block. The fault is the test's, not the CLI's: the fact stops and removes its job, then its `finally` stops and removes it again, on the grounds (its comment) that "a second stop and rm of an id already gone is harmless: their results are not read". That stopped being true with efdea639 ("Throw claude's own words when listing, stop or rm fails"), which reached `main` on 2 November and made `ClaudeCli.StopAsync` and `RemoveAsync` throw on a non-zero exit. The exception from `finally` also replaces any failure of the fact's own `ShouldNotContain` check, so a real regression in `claude rm` would be hidden behind the same message.

## What Changes

- `RemoveTheSessionByItsJobId` ends its job in `finally` only when the fact did not get as far as ending it itself, so the cleanup no longer stops and removes an id that is already gone, and a failure of the listing check is reported as itself.
- The misleading comment goes; the new one says why the cleanup is conditional.
- `skip_specs: true`: one contract test changes; no key, screen, message, state file or command line behaves differently.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None: a test only, so `.openspec.yaml` declares `skip_specs: true`.

## Impact

- **Tests:** `test\Source191.Cader.Infrastructure.Tests\Contract\ClaudeContractShould.cs` only. The other contract facts end their job once, in `finally`, and are unaffected.
- **Version and changelog:** nothing under `src\`, `Directory.Build.props`, `global.json` or `nuget.config` changes (`.github\scripts\ToolPaths.ps1`), so `Version` does not rise and `Assert-ChangelogUpdated.ps1` asks for no `CHANGELOG.md` line.
- **Stacking:** on PR #219 (`v-rename`), so the paths are the Cader ones and the switch is `CADER_CLAUDE_CONTRACT`.
- **Issue #215:** closed by hand once the nightly run passes on `main`; the report job comments on the open `claude-cli` issue rather than closing it.
- **Users:** none.

## Out of scope

- Making `StopAsync` or `RemoveAsync` forgive "No job matching": the console reports claude's own words when a stop or rm fails, which efdea639 chose deliberately.
- Changing how the `report` job words its issue (it names the CLI version even when the test is at fault).
- Multiple Claude accounts, which is a later change.
