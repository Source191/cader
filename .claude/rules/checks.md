---
paths:
  - ".github/**"
  - "tools/HeadlessTerminal/**"
  - "tools/HeadlessTerminal.Tests/**"
  - "test/Source191.Cader.Infrastructure.Tests/Contract/**"
---

# The checks and the workflows

What each check does and how CI runs it. The commands, and when each check is due, are in `CLAUDE.md`; `CONTRIBUTING.md` is the contributor's account. Mutation testing (Stryker, `mutation.yml`, the break score) moved to `mutation-testing.md`. Measurements and incidents behind these facts are in `docs\verified-facts\checks.md`.

## Workflows

- **`pipeline.yml`:** runs on pull requests and pushes to `main` on Windows and is required by branch protection; a pull request the owner did not open also needs the owner's approval (`.github\CODEOWNERS` and the "Owner approves main" ruleset, which the owner bypasses).
- **The SDK:** a pinned `actions/setup-dotnet` installs the SDK `global.json` names, in `check`, `handoff`, `claude-contract.yml` and `mutation.yml`, because `windows-latest` carried no 11.0 SDK (added 14 November 2026); the step can go once both runner images carry one.
- **What `check` does:** builds Release, tests, checks whitespace only (`dotnet format whitespace --folder --exclude openspec`, because the build already fails on style), publishes the winget build (`Properties\PublishProfiles\win-x64.pubxml`), zips its five files with `Compress-WingetBuild.ps1` and uploads the zip.
- **The zip check:** `Compress-WingetBuild.ps1` fails on a missing file or any other (a PDB above all), or a `libonigwrap.dll` of the other architecture.
- **The matrix:** the `win-x64` leg on `windows-latest` is named `check` (the name branch protection requires) and also runs the tests, Pester (`test\Scripts`) and the whitespace check; `check (win-arm64)` on `windows-11-arm` builds, publishes and smoke-checks only. Each leg uploads `cader-<version>-<rid>`.
- **The NuGet cache:** `actions/cache` of the global packages folder, keyed on every `*.csproj`, `Directory.Build.props`, `nuget.config` and `global.json`; caches are scoped by ref, so only runs on `main` write the cache a release restores. Nothing prunes the entries, which grow (sizes in the evidence).
- **The `DOTNET_` first-run variables** are kept because they cost nothing, not for speed (2 November 2026).
- **Documentation only:** a pull request touching only `openspec\` or `.md` files outside `.github\` skips the build after the version, changelog and budget checks (`Test-DocumentationOnly.ps1`); pushes to `main` run everything; a new commit cancels a pull request's older run.
- **The context budget:** `Assert-ClaudeContextBudget.ps1` runs before that skip and holds `CLAUDE.md` and each `.claude\rules\*.md` to 12,288 bytes and 600-character lines (`test\Scripts\Assert-ClaudeContextBudget.Tests.ps1`).
- **Pinned actions:** every action is pinned to its release commit with the version in a comment. Dependabot groups the `github/codeql-action` steps into one pull request, because CodeQL fails when `init` and `analyze` differ (seen 13 November 2026), and NuGet updates into one weekly `nuget` group.
- **`codeql.yml`:** CodeQL over the C# (build mode none) and the workflows on pull requests, pushes to `main` and weekly, never a required check.

## Releases

- **The release:** `pipeline.yml` publishes a GitHub Release tagged `v<version>` when a push to `main` changes `Version` in the csproj (`release.yml` did until 3 November 2026; the workflow was `check.yml`, and its job keeps the name `check` for branch protection).
- **The version step:** `check`'s "Decide the release version" compares the csproj with the first parent, names the zip for the plain `Version` instead of `-ci.N`, smoke-tests that zip and uploads it as `release-<rid>` (both legs; x64 adds the notes).
- **The `release` job:** `needs: check`, no checkout, holds `contents: write`, `id-token: write` and `attestations: write`; it finds exactly one zip per architecture, attaches a build provenance attestation to each and runs `gh release create`. A commit whose check failed is never released.
- **Release notes:** for a stable version `check` writes its `CHANGELOG.md` section (`Write-ReleaseNotes.ps1`); the release passes it with `--notes-file` beside `--generate-notes` and `--notes-start-tag` the previous stable tag, which prepends it to the generated notes (gh 2.103.3, 1 November 2026).
- **Immutable releases are off** (1 November 2026); a fix is a new version all the same.
- **`publish.yml`:** nothing reaches winget automatically. It is `workflow_dispatch` with a `tag` input, one `winget` job in the `winget` environment (the owner's approval, `main` only, the `WINGET_PKGS_TOKEN` secret; `contents: read`, `attestations: read`).
- **What it checks:** it refuses a prerelease tag, downloads both zips (failing naming a missing architecture), runs `gh attestation verify` on each with `--signer-workflow` `pipeline.yml` and `--source-ref refs/heads/main`, and fails when winget-pkgs has no `manifests/s/Source191/Cader` (the first version is submitted by hand, MAINTAINING.md).
- **The submission:** a pinned, hash-checked `wingetcreate update --urls "<x64 zip>|x64" "<arm64 zip>|arm64" --submit`, failing loudly without the token.

## Version and changelog gates

- **`Assert-VersionBumped.ps1`** (the first step of `check`) fails a pull request that changes the packaged tool without raising `Version`, so each merge of one is a release.
- **`Assert-ChangelogUpdated.ps1`** (sharing `ToolPaths.ps1`) fails one that leaves `CHANGELOG.md` unchanged, or raises `Version` to a stable version without that version's section and an empty Unreleased.
- **Dependabot's bump:** a step run by `DEPENDABOT_BUMP_TOKEN` (a Dependabot secret; it fails naming the secret when missing) runs `Step-Version.ps1 -Preview`, adds the pull request's Internal line with `Add-InternalChangelogEntry.ps1` and pushes with `[dependabot skip]` so Dependabot keeps rebasing.
- **The checkout it leaves:** a following step removes the `dependabot-branch` checkout so the whitespace check and the NuGet cache key never read it (four test-only Dependabot pull requests failed on 13 November 2026 when it did).

## Hand-off check

- **What it does:** `dotnet run --project tools\HeadlessTerminal -- check` runs `cader here` with a throwaway name and no prompt (no model call), waits through `StepRunner` for Claude's `Claude Code v` banner (about 12 s), sends Ctrl+Z, waits for `here` to exit, then stops and removes the job by name.
- **Exit codes:** 1 means `here` did not exit after the detach, 2 that the check could not start (no Release exe), 3 that the banner never appeared (31 October 2026, CLI 2.1.316).
- **Where to run it:** its defaults are relative to the current folder, so run it from `tools\HeadlessTerminal` (CI: `dotnet run --project . -- check` there) after `dotnet build src\Source191.Cader -c Release`.
- It is the one automated test of a real `claude attach` and detach; the console itself never hands over.
- **Pseudo-console handles:** a [pseudo-console](https://learn.microsoft.com/windows/console/creating-a-pseudoconsole-session) child is started with [`STARTF_USESTDHANDLES`](https://learn.microsoft.com/windows/win32/api/processthreadsapi/ns-processthreadsapi-startupinfow) and no handles, or its output would go to the driver's own stdout whenever that is piped, as in CI.
- **In CI:** the `handoff` job in `pipeline.yml` runs it when the `CLAUDE_CODE_OAUTH_TOKEN` secret exists, and never blocks a merge.
- **The other commands:** `drive` (scripted keystrokes into a ConPTY, captured to drive-raw.txt), `grid` (replay a capture as a screen) and `count` (plain-text markers). Keys sent before Terminal.Gui has initialised (about 8 s cold) are lost, so scripts start at 8000 ms.
- **`drive` writes real state:** `LOCALAPPDATA` does not move Cader's state folder (`GetFolderPath` asks Windows; 1 November 2026, .NET 10), so a `drive` of a local cader.exe reads and writes the real `%LOCALAPPDATA%\Cader` and the Terminal fragments folder; it is for the owner, never an unattended session.

## Smoke check of the packed tool

- **What it opens:** `smoke <exe> <worktree-folder>` runs at 120 by 36 (`SmokeCheck.Options`) and opens and cancels the Keys help, the wizard, the problem report, the first root's dialog and its folder picker (F3), Open folder with (Custom, file picker), Before branching, Sessions refresh, the worktrees view, its help, force remove, the fixed-folder wizard (`n`) and the solution chooser (`s`), then quits.
- **What it waits for:** the wizard's and Before branching's caption and last choice (so a dialog too short fails); dialogs found by filtering for `with`, `branching` and `Sessions`.
- **Exit codes:** 0 passes, 1 a blocked UI thread, 2 no exe, 4 a step whose marker never appeared (named in the output).
- **The owner's smokes:** `smoke-update <exe>` (U opens What's new) and `smoke-whats-new <exe>` (What's new before the console) need a stable build with a recorded newer release or older `last-run.json`, which CI's `-ci.N` never is, so they are the owner's, with seeded state in the real `%LOCALAPPDATA%\Cader`.
- **Its keys and markers** come from `Wording` (hints, dialog tab titles, a column only each view has, the worktrees title's ", selected: " as the scan-finished signal), so a new dialog is one step in `SmokeScript`.
- **In CI:** `check` installs the Claude Code CLI signed out (`claude agents --json` prints `[]`), seeds `~\source\repos\Northwind.Smoke` with the linked worktree `Northwind.Smoke-smoke`, extracts the winget zip and runs its `cader.exe`: the one test of the bits users install.

## Claude Code contract tests

- **How to run them:** they start real sessions, so they carry `Category=ClaudeContract` and skip unless `CADER_CLAUDE_CONTRACT=1`: `$env:CADER_CLAUDE_CONTRACT = 1; dotnet test --project test\Source191.Cader.Infrastructure.Tests -c Debug -- --filter-trait "Category=ClaudeContract"` from a trusted repository. Never add `--bg --resume --help`: it starts a session.
- **What they cover:** `Contract` drives the solution's own Claude adapters against the installed CLI (version, a `--bg -n` start with no prompt, the listing's fields and state, stop and rm) and compares `claude --help` and `claude agents --help` with `Contract\Snapshots`.
- **Snapshots:** a difference leaves `<name>.received.txt` in the output folder (and the `contract-results` artefact); accepting one is copying it over the snapshot, in a change that re-checks the CLI facts in `claude-code.md`.
- **Nightly:** `claude-contract.yml` runs them against the latest CLI with the hand-off check (`contents: read`); when the install or a test fails, a `report` job (`issues: write`) comments on the open `claude-cli` issue or opens one assigned to `CLAUDE_CLI_WATCHER`.
- **Automated runs:** a prompt-less `claude -p` with stdin redirected and never written fails without a model call and exits 1; on a clean machine it registers as an automated run at about 7 s and exits at about 8.5 s, before a listing can see it (2026-10-31, CLI 2.1.316). A user's SessionEnd hooks lengthen that.
- **Holding one open:** the contract fact holds the run open with a SessionEnd hook passed through `--settings` that sleeps 20 s (listed at about 6 s, exit at about 27 s). Start such a run after the console is up, since its first listing takes about 10 s.
