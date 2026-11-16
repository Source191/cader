# Proposal

## Why

Renaming a session that had just been moved to the background with `/bg` destroyed it. Claude Code 2.1.325 writes the new background session's transcript straight away, but with only its title and name in it; the conversation is written there on the session's next turn. Cader takes "the transcript file exists" to mean "the session has messages", so rename removed the job and resumed a session Claude could not find ("source session … not found"). The new job failed 15 seconds later, the old one was gone, and Cader had already said the rename worked.

## What Changes

- A session counts as having messages only when its transcript holds a user or assistant line, not merely when the file exists. Each line's top-level `type` is the only thing read, and reading stops at the first message line.
- Rename, adopt, open-in-tab, resume and close-empty all use the one check, so each now treats a title-and-name stub as a session never sent a message: rename is refused with the existing "send it something first" words, and the job keeps running.
- The project context in `openspec\config.yaml` allows reading a transcript line's `type`, and nothing else.
- `.claude\rules\claude-code.md` records the `/bg` stub transcript fact with its date and CLI version.

Out of scope: noticing that a resumed job failed after the resume command itself succeeded (rename still reports success in that case); changing any wording; multiple Claude accounts, which is a later change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `session-attach`: "Sessions with nothing to resume are refused" and "Rename is refused while working or before the first message" judge a session by whether its transcript holds a message, not by whether the file exists.

## Impact

- `src\Source191.Cader.Infrastructure\Claude\ClaudeTranscripts.cs` and its tests in `ClaudeTranscriptsShould`.
- `ITranscripts`' summary, `openspec\config.yaml`'s context, `.claude\rules\claude-code.md`.
- `Version` in `src\Source191.Cader\Source191.Cader.csproj`.
