# Tasks

## 1. The transcript check

- [x] 1.1 Red then green: `ClaudeTranscriptsShould.TreatATranscriptWithOnlyATitleAndANameAsNoMessages` writes a transcript of an `ai-title` line and an `agent-name` line and expects false; `ClaudeTranscripts.HasMessages` reads each line's top-level `type` and answers true at the first `user` or `assistant` line. Update the existing facts' `{}` files to hold a `user` line. Verify with `dotnet test test\Source191.Cader.Infrastructure.Tests -c Debug --filter-class *ClaudeTranscriptsShould`.
- [ ] 1.2 Red then green: `ClaudeTranscriptsShould.FindAnAssistantLineAfterLinesThatAreNotJson` (a half-written line and a metadata line before an `assistant` line) expects true. Verify with the same command.
- [ ] 1.3 Refactor: open the file with `FileShare.ReadWrite | FileShare.Delete`, answer true on `IOException` or `UnauthorizedAccessException`, and update the summaries of `ClaudeTranscripts` and `ITranscripts`. Verify with `dotnet build -c Debug` and the same tests.

## 2. Recorded facts and release

- [ ] 2.1 Record the `/bg` stub transcript fact (2026-11-16, CLI 2.1.325) in `.claude\rules\claude-code.md`, and allow reading a transcript line's `type` in `openspec\config.yaml`'s context. Verify by reading both.
- [ ] 2.2 Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj`. Verify with `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes` and `openspec validate fix-stub-transcript-check`.

## Workflow follow-up

- Archive the change in the same pull request, then open the pull request.
