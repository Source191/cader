## Context

See proposal.md, Why. `ConsoleComposition.RunAsync` starts `launch.Claude.VersionAsync` with a five-second timeout, does the rest of its set-up, awaits the version, records `Started` with it and builds the screen context (`HeaderSettingsRow` takes a `ClaudeCodeVersion`). The Sessions tab's first `TriggerLoad` runs in the frame's constructor (`StartRefreshing`), after `app.Init`.

## Decisions

### 1. The version remembered by the executable's stamp

The record holds the executable's length and last write time (UTC) and the version text, never the path, so it is not personal; `IFolders` or a small `IFileStamps` port reads the stamp. At start a match answers the version at once; a mismatch or no record answers "being read" and starts `claude --version` on a pool thread, which writes the record when it answers. Why the stamp: an update replaces the executable, which changes its size or write time, and reading the stamp costs no process.

Alternative considered: draw first and always read the version; rejected because it still spawns a 2-second process on every start for a value that rarely changes.

### 2. A header that updates once, however late

`HeaderSettingsRow` reads the version through a `SharedSnapshot<ClaudeCodeVersion>`, set once when the background read answers, and the screen redraws its settings row on the next frame (`ShowSettings` already compares rows). The read is started with no five-second cancellation (the child program time limit still ends it), so a slow cold start still reaches the header; a read that fails or times out answers unknown. A tested `Application\ClaudeVersionWatch` holds it: `Current` answers being read until the task completes, then the version judged by `ClaudeCodeSupport.Check`, and `WithinAsync(TimeSpan, TimeProvider)` waits up to five seconds. `Started` is recorded at once on a match, else when the read answers or after five seconds with unknown, so the start record is never held back longer than today.

Why: the bug hunt's two cold starts both took over five seconds, and a version given up on stayed unknown until the next start (first proposed as `guard-overlapping-actions`' design 8).

### 3. The first listing before the screen

`LatestSessions` gains `StartFirst(CancellationToken)`, which starts the listing and keeps its task; the Sessions tab's first load awaits that task instead of starting its own, and the reservation in `RunningLoads` covers it as today. The composition calls it right after `LaunchParts` has found Claude Code, so it runs while settings, accounts and the screen are built.

## Risks / Trade-offs

- [Claude Code updates itself while Cader runs] → the remembered version is for the next start; the header shows what was read at start, as today.
- [The state file is corrupt or names a value out of shape] → `JsonFileStore` reads it as corrupt, which answers no record, so the version is read as today.
