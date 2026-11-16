# Design

## Context

`ITranscripts.HasMessages` is the one place every action asks whether a session has anything to resume: rename, adopt, open-in-tab and resume refuse a session without messages, and close-empty closes a terminal session without them. `ClaudeTranscripts` answers it by checking that `<sessionId>.jsonl` exists in any project folder.

On 2026-11-16, with CLI 2.1.325, `/bg` in an interactive session started background job `7a1faf83` and wrote a `continued-in` line naming it into the interactive session's transcript (`c3b0f3b1`). The job's own `7a1faf83-….jsonl` was 246 bytes and held only an `ai-title` line and an `agent-name` line. After `claude rm 7a1faf83`, `claude --bg --resume 7a1faf83-… -n fix-stryker-net11` started job `3fe9ce0f`, which failed with `source session 7a1faf83-… not found`. `claude --bg --resume c3b0f3b1-… -n fix-stryker-net11` then continued the conversation.

The project context said Cader never reads transcript JSONL beyond checking that the file exists. That rule now has to give a little.

## Goals / Non-Goals

**Goals:**
- `HasMessages` answers true only for a transcript that holds a `user` or `assistant` line.
- Read the least: each line's top-level `type`, stopping at the first message line.

**Non-Goals:**
- Finding the conversation of a `/bg` session in its parent transcript, or resuming the parent instead.
- Watching a resumed job for a failure after the resume command returned.
- Any wording change: rename's existing refusal ("has no messages yet, so it cannot be renamed; send it something first") is the right advice for a just-backgrounded session, which still runs and can be opened.

## Decisions

- **Read each line's `type` with `JsonDocument`**, and nothing else. The first message line is usually within the first few lines, so even a multi-megabyte transcript costs one short read. A line that is not JSON, such as a half-written last line, is skipped. Alternatives: a size threshold (a guess that breaks when Claude Code changes its stub) or a substring search for `"type":"user"` (could match inside another line's text).
- **Open the file with `FileShare.ReadWrite | FileShare.Delete`**, because Claude may be appending to it while the console refreshes.
- **An `IOException` or `UnauthorizedAccessException` reading an existing file answers true**, the answer before this change: a file Cader cannot read is no reason to close a terminal session's tab, and the stub case is the only one this change sets out to catch.
- **Privacy:** nothing read is kept, logged or shown; the only value taken from a line is whether its `type` is `user` or `assistant`.

## Risks / Trade-offs

- [Claude Code renames its message types] → `ClaudeTranscriptsShould` pins the shapes, the `claude-code.md` fact names the version, and the failure is a refusal ("no messages yet"), never a lost session.
- [A session whose transcript holds only a local command such as `/bg`] → Claude records the command as a `user` line, so it counts as having messages, as it did before.
