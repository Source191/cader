## ADDED Requirements

### Requirement: A session whose folder has gone is not resumed
A key that would resume a session whose working directory no longer exists SHALL be refused: the open-in-tab key for such a session without a process, on the Sessions tab and through a worktree's row alike, and the rename key for any such session. No tab SHALL be opened, nothing SHALL be removed or resumed, and the status line SHALL say that the session's folder no longer exists and that delete removes it. The key bar SHALL offer only delete for such a session without a process.

#### Scenario: Enter on a stopped session whose folder has gone
- **WHEN** the user presses Enter on the stopped session fix-123, whose folder C:\Users\me\source\worktrees\Northwind.Orders-fix-123 has been deleted
- **THEN** no tab opens, the job is neither removed nor resumed, and the status line says fix-123's folder no longer exists and that d deletes it

#### Scenario: Enter on a running session whose folder reads as gone
- **WHEN** the session fix-123 is running with a live Cader tab and its folder, on a network drive that has gone offline, reads as gone
- **THEN** Enter switches to its tab as for any running session

#### Scenario: Rename a session whose folder has gone
- **WHEN** the user presses r on the session fix-123, whose folder has been deleted
- **THEN** no dialog opens, the job is neither removed nor resumed, and the status line says fix-123's folder no longer exists and that d deletes it

#### Scenario: The key bar of a stopped session whose folder has gone
- **WHEN** the selected row is the stopped session fix-123, whose folder has been deleted
- **THEN** the key bar offers d Delete for the row, and neither Enter nor r

## MODIFIED Requirements

### Requirement: Resume every shown stopped or failed session in its own tab
The resume-all key SHALL, without a dialog, resume every background session in the stopped or failed state that the table shows and whose folder still exists, in the table's order, and SHALL open a Cader tab for each one the CLI resumes. It SHALL remove each job before resuming it by session ID. A resume that fails after the removal SHALL get no tab and be reported with the command that resumes it. With none shown, the status line SHALL say there is nothing to resume.

#### Scenario: Resume after restart
- **WHEN** three sessions show failed after a reboot and the user presses resume-all
- **THEN** each job is removed and resumed by session ID, a tab opens for each, and the same three rows, with the same session IDs, return to idle or working on the next refresh

#### Scenario: Stopped sessions resume too
- **WHEN** two sessions show stopped and one shows failed when the user presses resume-all
- **THEN** no dialog opens, all three are resumed in the order the table shows them, each in its own tab, and the status line says three of three resumed in their own tabs

#### Scenario: The filter narrows the resume
- **WHEN** four sessions show stopped, a filter shows only two of them, and the user presses resume-all
- **THEN** only the two shown are resumed and given tabs, and the other two stay stopped

#### Scenario: A resume the CLI refuses
- **WHEN** the user presses resume-all on two stopped sessions and the CLI refuses to resume one of them
- **THEN** the other resumes in its own tab, no tab opens for the refused one, and the status line says one of two resumed and names the refused one with the CLI's reason

#### Scenario: No duplicate after a resume
- **WHEN** the user resumes a stopped session named fix-123 with session ID sess-1
- **THEN** the next refresh lists exactly one row named fix-123, with session ID sess-1, and no stopped copy of it

#### Scenario: Resume fails after the job was removed
- **WHEN** the user resumes a failed session named fix-123 with session ID sess-1 and the resume command fails with "Workspace not trusted"
- **THEN** no tab opens for it, and the status line names fix-123, gives the failure, and says to resume it with claude --bg --resume sess-1

#### Scenario: A session never sent a message
- **WHEN** the user presses resume-all while a stopped session named repos-f6 has no transcript
- **THEN** its job is neither removed nor resumed, no tab opens for it, and the status line names repos-f6 and says there is nothing to resume

#### Scenario: A session whose folder has gone
- **WHEN** the user presses resume-all while two sessions show stopped and the folder of one of them, fix-123, has been deleted
- **THEN** only the other is removed, resumed and given a tab, and fix-123's job is left listed as it was

#### Scenario: Nothing to resume
- **WHEN** no shown session is stopped or failed and the user presses resume-all
- **THEN** nothing is resumed, no tab opens, and the status line says there is nothing to resume
