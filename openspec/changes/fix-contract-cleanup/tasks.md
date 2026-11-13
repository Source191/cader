# Tasks

The fact under change is itself the test, and it only runs against the real CLI, so red and green are contract runs: `$env:CADER_CLAUDE_CONTRACT = 1; dotnet test --project test\Source191.Cader.Infrastructure.Tests -c Debug -- --filter-trait "Category=ClaudeContract"` from this trusted repository, with NO_COLOR, CLAUDECODE, CLAUDE_PID and CLAUDE_CODE_* cleared. Each step is a commit.

## 1. The cleanup

- [x] 1.1 Red: run the contract facts on the current code and see `ClaudeContractShould.RemoveTheSessionByItsJobId` fail with `ClaudeCliException : No job matching '<id>'` from `EndAsync` called in its `finally` block (`ClaudeContractShould.cs:134`), as the nightly run does. Record the CLI version (`claude --version`). Seen 13 November 2026 on CLI 2.1.323: 8 of 9 passed, this fact failed with `No job matching '1dfd81ac'`.
- [ ] 1.2 Green: in `RemoveTheSessionByItsJobId`, set `bool ended = false;` before `try`, set `ended = true;` straight after the fact's own `await EndAsync(cli, jobId);`, and in `finally` call `EndAsync` only while `ended` is false; replace the "harmless" comment with one saying stop and rm throw for an id already gone. Verify `dotnet build -c Debug` (warnings are errors), `dotnet format --verify-no-changes`, and the contract run: every `ClaudeContract` fact passes and `claude agents --json --all` lists no `contract-` job afterwards.
- [ ] 1.3 Verify the fast suite is untouched: `dotnet test test\Source191.Cader.Core.Tests -c Debug --no-build`.

## 2. Close out

- [ ] 2.1 Archive the change (`/opsx:archive`), `openspec validate --all` passes, and the pull request is stacked on #219 (`v-rename`) and names issue #215, to be closed by hand after the first green nightly run on `main`.
