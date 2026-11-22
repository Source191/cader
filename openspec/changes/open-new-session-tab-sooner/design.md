## Context

See proposal.md, Why. `LaunchService.StartedAsync` reports "finding the session", records the repository as recent, then runs `ListSessionsAsync` and answers `Listed` (with the `Session.Background`) or `NotListedYet`. `StartSessionHandler` opens a tab only for `Listed`, through `OpenTabHandler.OpenAsync(session)`, which writes a pending `TabRegistration` owned by `TabOwner.ForSession(session.SessionId)` and opens `TabScripts.Attach(session, registration)`: the name, the colour, the folder and `claude attach <job id>`, with `cader claim-tab <registration id>` first. Registrations are found by session id (`TabRegistrations.For(SessionId)`) for the TAB block, Enter, and the tab closing in stop, rename, delete and adopt.

## Decisions

### 1. A registration owned by the job until the session is known

`TabOwner` gains `ForJob(JobId)`. The wizard's launch writes its pending registration as `ForJob(jobId)` and opens the tab from the plan (name, folder) and the job id. `TabRegistrations.For(Session.Background)` answers registrations owned by the session's id or by its job id; an interactive session has no job, so nothing changes for it. The first refresh whose listing holds a background session with a job-owned registration rewrites that registration as `ForSession(sessionId)` (same id, colour and claim), so job-owned registrations are short-lived and the rest of the code keeps finding tabs by session id.

Why not wait for the session id in parallel: the listing is the 2.7 s the change removes; opening first and re-owning later is the only way to save it. Why the job id is safe as a key: it is the first eight characters of the session id (`claude-code.md`), unique among live jobs, and a stale job-owned registration goes stale with its shell exactly as a session-owned one does (`IsLive`).

Alternative considered: a `TabOwner.ForSession` with an optional session id; rejected because null for meaning is against the house rules and a separate case makes every match say which it handles.

### 2. The launch stops listing

`LaunchService` answers one `LaunchOutcome.Started(JobId, BaseUpdate, LaunchSteps)` and no longer lists; the launch hold's release already makes the first tick list the sessions, which shows the new row and re-owns its tab.

### 3. Security review

No process is ended through a registration that is not claimed and confirmed (`TabRegistration.TryConfirmShell` is unchanged), and the owner never takes part in confirming a shell, so a job-owned registration can end only the shell that claimed it, as today. The review checks every caller of `TabRegistrations.For` and every switch over `TabOwner`, which a closed hierarchy in Domain makes the compiler list (`ClosedHierarchiesShould`).

## Risks / Trade-offs

- [Two jobs whose ids share their first eight characters] → impossible for live jobs, which the supervisor names by those eight characters.
- [The re-owning write races a key that closes the tab] → both go through the tab registry's one writer; a fact runs them in both orders.
- [`claude attach` by job id may refuse a job the supervisor has not listed yet] → not yet verified; task 2.2's contract-style check runs `claude attach` straight after `--bg` answers in the headless terminal before the handler relies on it, records the result in `claude-code.md`, and if attach refuses, the tab's script retries the attach for a few seconds before giving up. The manual check in task 4.1 confirms it in a real Windows Terminal.
