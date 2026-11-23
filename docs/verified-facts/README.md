# Verified facts: the evidence

The rules in `.claude\rules` hold each verified fact about Claude Code, Terminal.Gui, Windows Terminal, Windows, winget and the checks as one short bullet: what is true, the date and version it was checked, and what Cader does because of it. Claude Code loads a rule whenever a file its paths match is read, so the rules stay small (`.github\scripts\Assert-ClaudeContextBudget.ps1`: 12 KB a file, 600 characters a line).

This folder holds what those bullets rest on: the measurements, run ids, probes, what was tried and ruled out, and the history of a decision. It loads only when read.

- **One file per rule:** `docs\verified-facts\<rule>.md` pairs with `.claude\rules\<rule>.md`, under headings that match the rule's bullets or sections.
- **A new fact:** its bullet goes in the rule, with its date and version; anything more (the run, the numbers, the narrative) goes here under a heading the bullet names.
- **Moved, not rewritten:** the text here was moved from the rules on 22 November 2026 (slim-claude-context) as it stood, so its dates and versions are those of the original checks.

| File | Pairs with |
|---|---|
| [checks.md](checks.md) | CI workflows, the hand-off, smoke and contract checks |
| [mutation-testing.md](mutation-testing.md) | Stryker, the mutation workflow, the break score and the close-mutation-gaps ledger |
| [windows.md](windows.md) | Windows Terminal and ShellExecute |
| [windows-processes.md](windows-processes.md) | Windows processes, git fetches, .NET 11's process APIs |
| [winget.md](winget.md) | winget, wingetcreate and Arm Windows |
| [terminal-gui.md](terminal-gui.md) | Terminal.Gui 2.5.0 and 2.6.0 |
| [console-screen.md](console-screen.md) | The console's layout, refreshing and dialogs |
| [claude-code.md](claude-code.md) | The Claude Code CLI |
