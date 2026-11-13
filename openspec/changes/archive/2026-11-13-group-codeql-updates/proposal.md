# Proposal

## Why

Dependabot opened #220 (`github/codeql-action/analyze` 4.39.1 to 4.39.3) and #221 (`github/codeql-action/init` 4.39.1 to 4.39.3) as separate pull requests. Both fail `analyse (csharp)` and `analyse (actions)` with CodeQL's own error: "Loaded a configuration file for version '4.39.3', but running version '4.39.1'. All steps in a workflow that use `github/codeql-action` must use the same version". Neither can pass on its own, and every later CodeQL release would split the same way.

## What Changes

- **`.github\workflows\codeql.yml`:** `init` and `analyze` both pinned to `33d7b85da1a04171e211befe2c739303b9a4c066 # v4.39.3`, the commit the `v4.39.3` tag points at.
- **`.github\dependabot.yml`:** a `codeql-action` group under `github-actions` matching `github/codeql-action*`, so later releases arrive as one pull request.
- **Docs:** `MAINTAINING.md` (Dependabot) and `.claude\rules\checks.md` (pins) say the CodeQL steps are grouped and why.
- **#220 and #221:** closed as superseded once this merges.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None: CI configuration and documentation only, so `.openspec.yaml` declares `skip_specs: true`.

## Impact

- **Version and changelog:** nothing under `src\`, `Directory.Build.props`, `global.json` or `nuget.config` changes, so `Version` does not rise and no `CHANGELOG.md` line is needed.
- **Users:** none.

## Out of scope

- Grouping any other actions or the NuGet packages.
