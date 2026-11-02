# Proposal

## Why

The nightly Claude Code contract run failed on 2 November 2026 (run 41432945174, issue #135) in `GiveTheStartedRowEveryFieldVantageReads`: the first listing of a freshly started job had no `pid` or `status`. The same CLI (2.1.316) passed the day before, so this is a race in the test, not a change in the CLI. A fact that fails on timing makes the nightly check cry wolf.

## What Changes

- The contract fact polls `claude agents --json --all` (up to the existing 30 s listing deadline) until the started job's row carries every field Vantage reads, then asserts; at the deadline it asserts on the last row, so a genuinely missing field still fails.
- `.claude\rules\claude-code.md` records the observation (2 November 2026, CLI 2.1.316) that a new job's first listing can lack both pid and status.
- Out of scope: any change to `ClaudeAgentsListing` or to how Vantage shows a starting job (it already tolerates a row without pid or status), and multiple Claude accounts.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None. No behaviour changes, so the change declares `skip_specs: true`.

## Impact

- `test\Source191.Vantage.Infrastructure.Tests\Contract\ClaudeContractShould.cs` and `.claude\rules\claude-code.md` only; nothing under `src\`, so no version bump.
