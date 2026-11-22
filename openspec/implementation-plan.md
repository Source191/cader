# Implementation plan: the open changes as one stack

The fifteen open OpenSpec changes are built as one stack of pull requests, bottom first. This file is the plan: the order, the checkpoints where building pauses, and the rules each pull request follows.

- **A step's box:** ticked in that step's own pull request when it is opened, so the file always matches the branch it is on.
- **A checkpoint's box:** ticked when the stack reaches it. The owner's testing and follow-on actions are tracked separately.
- **When the stack is done:** the last step's pull request deletes this file.

## Order

- [x] **Step 0:** reconcile the proposals (OpenSpec only, no version raise). Opus 5.5, medium. #380.

**Bug fixes**

- [x] **Step 1:** `report-what-removals-and-launches-did`. Sonnet 5.5, medium.
- [x] **Step 2:** `fit-narrow-and-short-terminals`. Opus 5.5, medium.
- [x] **Step 3:** `guard-overlapping-actions`. Opus 5.5, high.
- [x] **Step 4:** `ignore-reused-branch-pull-requests`. Opus 5.5, medium.
- [x] **Checkpoint A:** the owner reviews the four fixes, above all `guard-overlapping-actions`' `RunningEffects` in `TableBody`, which later steps also touch, and does its manual check of tabs (Enter twice on a session with no tab).

**Cheap wins**

- [ ] **Step 5:** `lighter-git-reads`. Sonnet 5.5, medium.
- [ ] **Step 6:** `fewer-background-polls`. Sonnet 5.5, medium.
- [ ] **Step 7:** `jump-to-waiting-session`. Sonnet 5.5, medium.
- [ ] **Step 8:** `name-ui-stalls`. Opus 5.5, medium.
- [ ] **Step 9:** `pull-requests-per-repository`. Opus 5.5, medium.
- [ ] **Checkpoint B:** the owner runs step 9's build for an hour with the diagnostic log on and compares it with the hour measured on 21 November 2026 (worktrees scan, sessions enrichment, pull-request refresh, git, `az` and `claude` runs, stalls), then decides whether steps 10 to 12 go ahead as written, cut back or dropped.

**The scan**

- [ ] **Step 10:** `parallel-scan-reads`. Opus 5.5, high.
- [ ] **Step 11:** `rescan-only-what-changed`. Opus 5.5, high.
- [ ] **Step 12:** `show-refresh-progress`. Opus 5.5, high.
- [ ] **Checkpoint C:** the same hour with step 12's build; the owner reviews before the user-facing steps.

**How it feels**

- [ ] **Step 13:** `detail-pane-reads-less`. Opus 5.5, medium.
- [ ] **Step 14:** `faster-start`. Opus 5.5, medium. The owner starts a scratch build from a clean environment and sees the screen appear before `claude --version` answers, and with the version at once on the second start.
- [ ] **Step 15:** `open-new-session-tab-sooner`. Opus 5.5, high.
- [ ] **Checkpoint D:** the owner runs the manual checks `CONTRIBUTING.md` asks for tabs and attach, in a real Windows Terminal.

## Why this order

- **Bug fixes first.**
- **The scan changes one after another,** with nothing else on the scan between them.
- **`name-ui-stalls` before the scan,** so the stall lines in the timing runs can be compared.
- **`ignore-reused-branch-pull-requests` before `pull-requests-per-repository`,** so its catalogue facts guard the branch's creation time through the rework.
- **`report-what-removals-and-launches-did` before `open-new-session-tab-sooner`,** which then removes the listing it guards.
- **The riskiest, owner-checked change last.**

## Rules for each pull request

- **One at a time:** one implementation agent, in its own worktree, never the owner's checkout; the next step starts only once the previous pull request builds, passes its tests, is formatted and passes `openspec validate`. The stack stops at the first failure, and at each checkpoint until the owner says carry on.
- **Stacked:** each pull request is opened against the previous step's branch, says "Stacked on #N", raises `Version` one preview step above the previous step, adds its CHANGELOG line and ticks its step here.
- **Measurements:** before-and-after timings need the diagnostic log, which only a saving key turns on, so agents never take them; checkpoints B and C do.
- **Dependabot:** hold Dependabot merges while the stack is open, since a raised `Version` on `main` makes every step's version line conflict.
- **Archive:** each pull request archives its change.
