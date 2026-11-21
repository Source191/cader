## MODIFIED Requirements

### Requirement: The status line sits under the settings row
The console SHALL show the status line directly under the settings row and above the tabs, its mark in the column where the versions start, outside every tab, only while the shown tab has a message, any action it started is still running, or a console-wide message stands; the spinner SHALL turn until every running action has reported. The tabs, their counts and the table's title SHALL move down to make room and back up when it goes. The tabs' frame SHALL end directly above the key bar.

#### Scenario: A message under the settings row
- **WHEN** the user presses c on fix-123 in the Sessions tab
- **THEN** the row under the versions says the folder was copied, the tab titles are on the row under it, and the detail pane's last line is the last row inside the frame, directly above its bottom edge and the key bar

#### Scenario: Nothing to say
- **WHEN** no key has reported anything on the Sessions tab and no action is running
- **THEN** the tab strip starts on the row under the versions and the tabs' frame ends on the row just above the key bar

#### Scenario: Nothing to say on Config
- **WHEN** the Config tab is shown with Worktree root selected and no key has reported anything
- **THEN** there is no status line and the tab strip starts on the row under the versions

#### Scenario: The line follows the tab shown
- **WHEN** the Sessions tab says a folder was copied, the Worktrees tab has no message, and the user presses Right
- **THEN** there is no status line while the Worktrees tab is shown, and pressing Left within ten seconds shows the copied message again

#### Scenario: An action running in the Worktrees tab
- **WHEN** the user removes a worktree
- **THEN** the row under the versions shows a spinner before "Removing name" until the removal reports its outcome

#### Scenario: Nothing under the detail pane
- **WHEN** the status line is showing
- **THEN** nothing is drawn under the detail pane inside the tab's frame

#### Scenario: Two actions overlap
- **WHEN** the user presses d on one worktree and, before it reports, d on another, and the first reports
- **THEN** the spinner keeps turning before the second's working text until the second reports, and only then is the mark drawn

## ADDED Requirements

### Requirement: Console-wide messages stay on every tab
A message about work that outlives its tab (a launch's progress and outcome, the update check's notices and answers, a settings save or toggle, and background work that threw) SHALL show whichever tab is shown, until it is closed, clears by itself or is replaced. A newer message on the shown tab SHALL take the line, and the console-wide message SHALL show again once that one clears.

#### Scenario: A launch followed to the Sessions tab
- **WHEN** the user starts a session from the wizard on the Worktrees tab and presses Left while it reads Starting c-new-message: starting Claude Code (3 of 5)...
- **THEN** the Sessions tab shows the same line with its spinner, and the steps and the final ✓ Started message follow there

#### Scenario: A newer tab message takes the line
- **WHEN** a launch's progress shows on the Sessions tab and the user presses x with no session selected
- **THEN** the line reads ! No session selected., and closing it shows the launch's progress again

#### Scenario: A newly found release on any tab
- **WHEN** the update check finds a newer release while the Config tab is shown and the user presses Left
- **THEN** the Worktrees tab shows the same newer-release message

#### Scenario: A setting saved
- **WHEN** the user saves the worktree root on the Config tab and presses Left
- **THEN** the Worktrees tab shows the saved message until it clears ten seconds after it appeared

### Requirement: A tab's own message returns only while recent
A message about a row or a key's answer on one tab SHALL show only on that tab. When the user shows that tab again, its message SHALL show again only if the user left the tab no more than ten seconds before, the message arrived while the tab was hidden, or its action is still running; otherwise it SHALL be cleared as the tab shows, and never a newer message.

#### Scenario: A quick look away
- **WHEN** the Worktrees tab reads ! the reason a worktree was not removed, the user presses Right, and presses Left again five seconds later
- **THEN** the Worktrees tab still reads the same message

#### Scenario: Back after a while
- **WHEN** the Worktrees tab reads ! the reason a worktree was not removed, the user presses Right, and presses Left a minute later
- **THEN** the Worktrees tab shows no status line

#### Scenario: Finished while hidden
- **WHEN** the user removes a worktree, presses Left before it reports, and presses Right a minute after it reported
- **THEN** the Worktrees tab shows the removal's outcome

#### Scenario: Still running on return
- **WHEN** a removal started on the Worktrees tab is still running a minute after the user left the tab, and the user comes back
- **THEN** the Worktrees tab shows the spinner and the removal's working text
