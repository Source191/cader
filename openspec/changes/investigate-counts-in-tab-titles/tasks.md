# Tasks

**Implement with:** Opus 5.5 at medium effort: a spike and some reading of the decompiled assembly, with no shipped code; the care needed is in telling observed facts from read ones.

This is an investigation: no task changes `src`, `test` or `tools`, so there is no red-green-refactor and no `Version` bump. Each task ends with a commit of what it recorded.

## 1. Read

- [ ] 1.1 Decompile Terminal.Gui 2.5.0's `Tabs`, `Tab` (or the SubView's title drawing) and `Border` with `ilspycmd` into a scratch folder under `%TEMP%`, and write down a read answer to each of design's questions 1, 2, 4 and 5, naming the members that decide it. Verify each answer names a member. Nothing committed.

## 2. Spike

- [ ] 2.1 A throwaway Terminal.Gui 2.5.0 program in a scratch folder under `%TEMP%` (design 3): three tabs, a timer that grows and shrinks the first title through count-like text, a log of `ValueChanged`, focus and the strip's width. Verify it starts in `tools\HeadlessTerminal` at 120 x 36 with NO_COLOR, CLAUDECODE, CLAUDE_PID and CLAUDE_CODE_* cleared.
- [ ] 2.2 Question 1, re-measuring: try a changed title with no further call, then each candidate call from 1.1; verify by a headless capture whether the strip's width follows the title, and from the log whether the shown tab, focus and `ValueChanged` stay as they were.
- [ ] 2.3 Questions 2 and 3, colour and glyphs: run the program in Windows Terminal by hand with the counts' glyphs in the title; verify by a screenshot whether a title can take more than one colour and whether each glyph is one cell.
- [ ] 2.4 Question 4, movement and clicks: grow the first title by ten cells while the pointer rests on Config, then click; verify from the log which tab was selected, and capture the strip with titles wider than the screen.
- [ ] 2.5 Questions 5 and 6, overlay and cost: draw text on the tab's top edge from `DrawComplete`, then switch tabs and run a spinner on the hidden tab; verify by captures whether the text survives, and note whether a title change redraws the window and whether it flickers at a 5 s timer and at ten changes a second.
- [ ] 2.6 Record each answer in `.claude\rules\terminal-gui.md` under the `Tabs` fact, with the date, Terminal.Gui 2.5.0 and Windows Terminal's version, saying for each whether it was observed or read; correct the existing "a title that changed left the strip measured for the old one" fact if the spike finds a call that re-measures. Verify the file reads as facts, not as a log of the spike. Commit.

## 3. Recommend

- [ ] 3.1 Mock up options A, B and C at 120 x 36 and at 80 columns from the spike's findings (headless captures or text mock-ups); verify each shows the Sessions tab with need input, working and failed rows.
- [ ] 3.2 Add a Findings section to `design.md`: each option scored on design 5's criteria, the recommended option and why, and its cost; for A, B or C, the follow-up change's name and the `console-layout` requirements and scenarios it would modify. Verify `openspec validate investigate-counts-in-tab-titles` passes. Commit.

## Workflow follow-up

- Put the recommendation, the mock-ups and the Windows Terminal screenshot in the pull request description, and ask the owner whether to propose the follow-up.
- Archive the change in the same pull request (`/opsx:archive`) and verify that `openspec validate` passes; a follow-up, if agreed, is its own change and pull request.
