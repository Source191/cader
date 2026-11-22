## Context

See proposal.md, Why, for the sizes. Constraints that shape the approach:

- Claude Code loads `CLAUDE.md` every session and a `.claude\rules\*.md` file only when a file matching its `paths` is read or edited; the generated skills cost only their descriptions.
- Five files under `src\` cite rule files by name in comments or a README (`windows.md` in `TerminalProfileId`, `ProgramFolder`, `GitCli` and `FileAssociations`; `checks.md` in `src\Source191.Cader.Core\README.md`; `claude-code.md`, `security.md` and `terminal-gui.md` once each). Any edit under `src\` is a tool change, which needs a `Version` raise, a changelog line and a release.
- `.github\workflows\pipeline.yml`, `mutation.yml`, `claude-contract.yml`, `Format-MutationComment.ps1`, `MAINTAINING.md`, `CONTRIBUTING.md`, two contract tests and many open changes' tasks cite `checks.md`, `claude-code.md` and their sections.
- `Test-DocumentationOnly.ps1` treats every `.md` file outside `.github\` as documentation, so a pull request that changes only `CLAUDE.md` or a rule skips the build, the tests and Pester.
- `CLAUDE.md`'s own rule: a newly verified fact is recorded with its date and version in its file in `.claude\rules`.

## Goals / Non-Goals

**Goals:**

- Every verified fact stays findable with its date and version; only its evidence moves.
- No file under `src\` changes, so nothing is released.
- The budget is checked on exactly the pull requests that could break it.

**Non-Goals:**

- Rewording facts beyond splitting a long bullet or cutting its evidence.
- A general Markdown linter, or a budget for `docs\`, `openspec\` or the generated skills.

## Decisions

### 1. Keep the old file names and split out the new ones

`checks.md` and `windows.md` keep their names and the larger half of their content (the workflows and checks; the Windows Terminal and ShellExecute facts), and each opens with one line naming where its moved sections went. The split-out files are new names.

- **Why:** every existing reference in `src\`, the workflows, the maintainer documents and the open changes still lands on the right file, or one line from it, with no `src\` edit and no release.
- **Alternative:** rename to `ci-workflows.md` and `windows-terminal.md` and update every reference. Rejected: it touches five `src\` files for comments only, forcing a release, and breaks the open changes' task references.

### 2. Evidence goes to `docs\verified-facts\<rule>.md`, one per rule file

Each fact's rule bullet keeps: the fact, its date and version, and its consequence in the code (the type or check that relies on it). Everything else (run ids, measurements beyond the one number the code depends on, what was tried and ruled out, the close-mutation-gaps ledger, `TopResizable`'s spike for the withdrawn resizable-detail-pane) moves to `docs\verified-facts\<rule>.md` under a heading matching the rule's bullet, and the bullet ends with `(evidence: docs\verified-facts\<rule>.md, <heading>)` where there is any.

- **Why:** the owner chose `docs\verified-facts` over relying on git history, so the evidence stays searchable and linkable while costing nothing until read. One file per rule keeps the mapping obvious.
- **Rule for what stays:** a number stays when the code or a check is built on it (the five seconds a listed process may start after its session, the 8 s before Terminal.Gui reads keys in a headless script, the break score of 95), or when it is the reason for a choice (the 4.4-point runner gap that puts the break score on the runner). An instruction stays even when it reads as history ("never run that start again: it opens the picker").
- **Alternative:** keep the evidence in the rules under a collapsed heading. Rejected: Claude Code loads the whole file regardless.

### 3. `CLAUDE.md` keeps what every file needs, scoped rules take the rest

- **Stays, one line each:** the build, test and release loop; the working rules; each code rule that applies to every file under `src\` (sealed, no default parameters, one type per file, no null guards, invariant culture, braces, `SharedSnapshot`, closed switches), naming the test that holds it; the security invariants; the three Claude Code facts that hold everywhere; the index of rules, which gains the two new files and says to read `effects-and-views.md` before planning a key, effect, view or dialog.
- **Moves to `effects-and-views.md`** (scoped to `Application\Views`, `Effects`, `Actions`, `Wording*`, `Tui\DialogEffects*`, `Tui\TableBody*` and their tests): Reducers, effects and keys; Wording; One home per fact's routing of keys to tools.
- **Moves to `values-and-null.md`** (scoped to `Domain`, `Ports`, `*Should` architecture tests, `JsonFileStore`, the state-file records): Values, sessions and null; the Try-method `out` shape; the closed-hierarchy detail for Core at C# 14.
- **Removed as a repeat:** the detail of the security invariants (in `security.md`), the contract-test command (in `checks.md`) and the Claude Code CLI paragraph beyond its three facts.
- **Why:** the index line keeps the moved recipes reachable at planning time, when no matching file has been read yet.
- **Alternative:** move every code rule into scoped rules. Rejected: the rules every file follows would then load only by accident.

### 4. The budget runs before the documentation-only skip

`Assert-ClaudeContextBudget.ps1` takes the repository root, measures `CLAUDE.md` and each `.claude\rules\*.md` in UTF-8 bytes (at most 12,288) and each line in characters (at most 600, front matter included), prints `::error file=...,line=...::` for each breach and exits 1 on any. `pipeline.yml` runs it as a step of `check` straight after the changelog check and before "Skip the build for a documentation-only change", so it runs on a documentation-only pull request too. Pester tests in `test\Scripts\Assert-ClaudeContextBudget.Tests.ps1` run it over a temporary folder: a file at the limit passes, one byte over fails naming the file, a long line fails naming the line, a missing `.claude\rules` passes.

- **Why 12 KB and 600:** about 3,000 tokens per file, so a Tui edit stays under about 9,000 tokens with `CLAUDE.md`; 600 characters is about three wrapped lines in a 200-column editor, long enough for a fact with its date and consequence.
- **Alternative:** make `CLAUDE.md` and `.claude\` count as code in `Test-DocumentationOnly.ps1`. Rejected: a three-minute build for a text edit, to run a one-second check.
- **Alternative:** a token count. Rejected: no tokenizer on the runner, and bytes are a stable stand-in.

### 5. `openspec\config.yaml` context keeps the product, not the code rules

It keeps the platform, the documented CLI surface and the files Cader must never read, the worktree placement, the deletion rule, attention as the bell, and the technology line; the layering, projects list, coding style and TDD lines become one line pointing at `CLAUDE.md`. Its `rules` stay as they are.

## Risks / Trade-offs

- [A fact is lost or loses its date in the move] → Each rule is moved in its own commit, and the task's check compares the rule's dated facts before and after (each date and version string in the old file appears in the new rule or its evidence file).
- [A recipe moved to a scoped rule is not loaded when Claude plans a key from a proposal] → The `CLAUDE.md` index names `effects-and-views.md` as the first read for a key, effect, view or dialog, as it already does for the other rules.
- [Open changes' tasks cite a section that moved] → The pointer line at the top of `checks.md` and `windows.md` names the new file for each moved section; the tasks are not edited.
- [12 KB is too tight for `console-screen.md` or `terminal-gui.md` once evidence is out] → Measured after the move; a file still over is split by section (Layout and Refreshing, say) rather than given a higher budget.
- [Conflicts with the open-changes stack] → Proposal, Delivery: landing first; a later step that adds a fact adds one bullet and, if needed, a heading in `docs\verified-facts`.

## Migration Plan

One pull request, no release. Rollback is a revert; nothing reads these files at run time.
