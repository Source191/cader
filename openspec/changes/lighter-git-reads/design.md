## Context

See proposal.md, Why. Today `GitCli.ListBranchesAsync` runs `for-each-ref` and then `config --get user.email` in the repository, compares the email inside the adapter and answers each branch's `Mine`. `WorktreeInspector.CountAsync` runs `status --porcelain`, `rev-list --count HEAD --not --remotes` and `log -1 --format=%cI` for each live worktree; `RegisteredRowsAsync` starts those counts and the repository's branch listing at the same time. `SessionRowEnricher.AskGitAsync` runs `rev-parse --show-toplevel`, `rev-parse --path-format=absolute --git-common-dir` and `branch --show-current` for each uncached folder.

## Goals / Non-Goals

**Goals:** fewer git processes and no index writes, with every row, column and pane exactly as today.

**Non-Goals:** concurrency (`parallel-scan-reads`), incremental scans (`rescan-only-what-changed`).

## Decisions

### 1. Optional locks off for every run

`GIT_OPTIONAL_LOCKS=0` ([git's environment variables](https://git-scm.com/docs/git#Documentation/git.txt-codeGITOPTIONALLOCKScode)) is set in `GitCli`'s request variables for every command. It only stops locks that are optional, which in practice is `git status` refreshing the index; a fetch, a merge or a worktree add still takes the locks it needs, so one rule for every run is simpler than a list of reads and cannot be wrong for a write. `FetchBranchAsync` and `PruneOriginAsync` already pass variables; theirs are merged with this one.

Alternative considered: `--no-optional-locks` on each read's argument list; rejected because the variable needs no change to each call and also reaches any git that git itself starts.

### 2. The email cache stays in the adapter

The email is compared inside `GitCli` today so it never leaves the adapter (`GitBranchListing`'s comment), so the cache sits there too: a dictionary from the repository's folder key to the email read and when, kept ten minutes, behind a lock, `GitCli` taking a `TimeProvider`. A failed read is not kept, so the next listing tries again. Ten minutes because an email change is rare and only flags an origin branch as yours; the scan needs no other freshness.

Alternative considered: one global read for every repository; rejected because `includeIf` gives work and personal repositories different emails.

### 3. Last commit from the branch listing, `log -1` only as a fallback

A worktree whose head is on a branch takes its last commit from that branch's tip time in its repository's listing (the committer date `for-each-ref` reads, which is what `log -1 --format=%cI` reads for that head). So the scan's per-worktree count no longer runs `log -1` for such a worktree; once a repository's rows and its branch listing have both returned, each row on a branch gets its tip's time, and a row whose branch the listing did not cover (the listing failed, or the head is detached or unknown) runs `log -1` as today. `WorktreeInspector` gains a count without the last commit; the detail pane keeps the full count.

### 4. One `rev-parse` for a session folder

`git rev-parse --path-format=absolute --show-toplevel --git-common-dir` prints the two answers on two lines in the order asked ([git rev-parse](https://git-scm.com/docs/git-rev-parse)); `IGit` gains one query answering both or null when git fails, and `AskGitAsync` uses it. The two old queries stay for their other callers.

## Risks / Trade-offs

- [`status` with optional locks off could count a file whose timestamp changed but whose content did not] → git is expected to refresh the index in memory and only skip writing it back; the real-git fact in task 1.2 checks that a touched, unchanged file is not counted, and records the result in `.claude\rules\windows-processes.md` (the evidence in `docs\verified-facts\windows-processes.md`) with git's version.
- [A user changes `user.email` and an origin branch's yours-flag lags up to ten minutes] → accepted; stated in the spec.
