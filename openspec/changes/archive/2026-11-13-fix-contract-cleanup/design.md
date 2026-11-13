# Design

## Context

See proposal.md, Why. `ClaudeContractShould.EndAsync` runs `claude stop <id>` then `claude rm <id>` through `ClaudeCli`, and both now throw `ClaudeCliException` when claude exits non-zero. Every other contract fact calls `EndAsync` exactly once, in `finally`; `RemoveTheSessionByItsJobId` alone calls it inside `try`, because ending the job is what it tests.

## Goals / Non-Goals

**Goals:** the fact cleans up after itself when it fails before ending the job, never ends the job twice, and lets its own assertion's failure through.

**Non-Goals:** any change to `ClaudeCli` or to the other facts.

## Decisions

- **A flag set after the fact's own `EndAsync`, and `finally` ending the job only while it is unset.** It keeps the existing `try`/`finally` shape, and the flag says plainly what the cleanup depends on.
  - *Swallowing `ClaudeCliException` in a cleanup-only `TryEndAsync`:* rejected. It would hide a stop or rm that genuinely fails in the other facts' cleanup too, and leaving a stray job on the runner then passes unnoticed.
  - *`catch { await EndAsync(...); throw; }` around the listing wait, with `EndAsync` and the assertion after it:* rejected. It does the same thing with a catch-and-rethrow, which reads less directly than a flag.
- **The flag is set straight after `EndAsync` returns, before the listing is read.** If `stop` or `rm` throws, the flag is still unset and `finally` tries the pair again, as every other fact's cleanup does.

## Risks / Trade-offs

- [After a half-done `EndAsync` (stop succeeded, rm threw), the retry's `stop` may itself throw on a job that is already stopped, and its exception replaces the first] → acceptable: both carry claude's own words for the same job, the stack points at the fact, and that path was already possible before this change. Whether `claude stop` accepts a stopped job is not verified here.
