# Tasks

## 1. Settle the started row

- [x] 1.1 `GiveTheStartedRowEveryFieldVantageReads` polls the listing until the job's row has every field in `FieldsVantageReads`, or the 30 s deadline passes; verified by the nine contract tests passing with `VANTAGE_CLAUDE_CONTRACT=1`.
- [x] 1.2 Record the 2 November 2026 observation (CLI 2.1.316) in `.claude\rules\claude-code.md`; verified by reading the line beside the starting-job fact.

## 2. Close out

- [x] 2.1 `openspec validate contract-test-settle-started-row` passes, `dotnet build -c Debug` has no warnings and `dotnet format --verify-no-changes` is clean.
- [x] 2.2 Archive the change in this pull request (`/opsx:archive`, no spec sync as `skip_specs` is set); verified by the change appearing under `openspec\changes\archive`.
- [ ] 2.3 After merge, run `claude-contract.yml` by hand to confirm it is green, then close issue #135.
