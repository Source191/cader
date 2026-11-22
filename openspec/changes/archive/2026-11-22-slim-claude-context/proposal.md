## Why

Every Claude Code session in this repository starts by reading `CLAUDE.md` (28.5 KB, about 7,100 tokens), and reading one file pulls in up to three path-scoped rules on top: a Tui file adds `console-screen.md` and `terminal-gui.md` (about 11,000 tokens), `TabScripts.cs` adds `claude-code.md`, `windows.md` and `security.md` (about 14,000), and any `.github` file adds `checks.md` (about 10,500), most of it the batch-by-batch ledger of close-mutation-gaps. Much of that text is evidence (run ids, probe narratives, a withdrawn feature's spike) rather than the fact and its consequence, bullets run to 3,400 characters, and `CLAUDE.md` restates in full the rules that 33 named architecture and convention tests already hold. The cost is paid on every turn of every session, the facts that matter are harder to find inside the narrative, and nothing stops the files growing back.

## What Changes

- The evidence behind the verified facts (measurements, run ids, probe narratives, the close-mutation-gaps ledger, withdrawn spikes) moves out of `.claude\rules` into `docs\verified-facts\`, one file per rule area, which loads only when read. Each rule keeps one bullet per fact: what is true, the date and version it was checked, and what the code does because of it, with a pointer to the evidence.
- `checks.md` keeps the workflows, hand-off, smoke and contract checks; its mutation testing section becomes `mutation-testing.md`, scoped to `mutation.yml`, `stryker-config.json` and the mutation scripts, so editing `pipeline.yml` no longer loads it.
- `windows.md` keeps the Windows Terminal and ShellExecute facts; its Windows process facts become `windows-processes.md` and its winget facts `winget.md`, each scoped to the files that use them.
- Narrower scopes: `claude-code.md` stops loading for every file under `openspec\changes`, and `locale.md` for every `*.csproj`.
- `CLAUDE.md` comes down to about 10 KB: the build, test and release loop, the working rules, the code rules every file follows as one line each naming the test that holds it, the security invariants as one-liners, and the index of rules. The recipes that apply to one area move to two new scoped rules, `effects-and-views.md` (reducers, effects, keys, Wording) and `values-and-null.md` (text values, sessions, null), and what `security.md` and `claude-code.md` already say is no longer repeated.
- `openspec\config.yaml`'s `context` keeps the product constraints and the documented CLI surface, and points at `CLAUDE.md` for the layering, style and projects it repeats today (its projects list is already stale).
- A budget, held by `.github\scripts\Assert-ClaudeContextBudget.ps1` in `pipeline.yml` before the documentation-only skip, so it runs on the pull requests that change only these files: `CLAUDE.md` and each `.claude\rules\*.md` at most 12 KB, no line longer than 600 characters. Its Pester tests sit in `test\Scripts`.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None: the change alters the contributor guidance only, so it declares `skip_specs: true`.

## Delivery

- **Order:** independent of every open change. Assumed to land before the open-changes stack's first implementation step, because those steps add facts to the rule files and are easier to rebase onto the new layout than this is to rebase onto them; the owner may place it elsewhere in `openspec\implementation-plan.md`.
- **Together:** one pull request. It changes nothing under `src\`, so it raises no `Version` and makes no release.

## Out of scope

- The generated OpenSpec commands and skills under `.claude\commands` and `.claude\skills`.
- The owner's personal auto-memory outside the repository.
- Re-verifying any fact: facts move with their dates and versions as they are.
- Multiple Claude accounts beyond naming a session's owner, a later change.

## Impact

- `CLAUDE.md`; `.claude\rules\` (the seven files trimmed, and five added: `mutation-testing.md`, `windows-processes.md`, `winget.md`, `effects-and-views.md` and `values-and-null.md`).
- New `docs\verified-facts\` (one file per rule area).
- `openspec\config.yaml`'s `context`.
- New `.github\scripts\Assert-ClaudeContextBudget.ps1` and `test\Scripts\Assert-ClaudeContextBudget.Tests.ps1`; one step in `.github\workflows\pipeline.yml`.
- `CONTRIBUTING.md`, where it calls `CLAUDE.md`'s code rules the authority and describes the rules.
- References to rule files in `src\` comments stay valid, because `checks.md` and `windows.md` keep their names and each says where its moved sections went.
- **Implement with:** Opus 5.5 at medium effort: no code behaviour changes, but every moved fact must survive with its date and version, and judging what is evidence and what is the fact needs care; the budget script is small and test-first.
