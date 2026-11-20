## MODIFIED Requirements

### Requirement: The pane lists a worktree's unpushed commits or changed files
Beneath the worktree's details the pane SHALL list its unpushed commits, or its changed files when none are unpushed, as many as the pane's lines hold after the details and one line for how many more, then how many more.

#### Scenario: A worktree with changes and nothing unpushed
- **WHEN** a worktree with two changed files and no unpushed commits is selected
- **THEN** the pane lists the two changed files in place of commits

#### Scenario: A taller pane lists more
- **WHEN** the Worktrees pane has fifteen lines under its divider and a worktree with twelve unpushed commits is selected
- **THEN** the pane shows its four rows of details, lists the ten newest commits and ends with 2 more

#### Scenario: Everything fits
- **WHEN** the Worktrees pane has fifteen lines under its divider and a worktree with eight unpushed commits is selected
- **THEN** the pane lists all eight commits and no line of how many more

### Requirement: The pane fits its facts
The pane SHALL sit under the table, behind a divider titled with the selected row's name, and SHALL keep the height its tab was last given, whatever row is selected, so the table's height never changes with the selection. Until the user changes it, the pane SHALL have six lines under its divider on the Sessions tab and ten on the Worktrees tab. A pane shorter than its facts SHALL show the facts that fit, from the top. The Config tab SHALL have no pane. The spinner shown while a table waits for its first rows SHALL sit in the table, above the pane.

#### Scenario: Moving between rows
- **WHEN** the user moves from a worktree with twelve unpushed commits to one with none
- **THEN** the table keeps the same number of rows on screen

#### Scenario: The first worktrees scan
- **WHEN** the Worktrees tab is first shown and its scan has not finished
- **THEN** the waiting spinner and its words are drawn in the middle of the table's area, not in the pane

#### Scenario: Never resized
- **WHEN** Cader starts with no pane height in `config.json`
- **THEN** the Sessions pane has six lines under its divider and the Worktrees pane ten

#### Scenario: A pane shorter than its facts
- **WHEN** the Sessions pane has four lines under its divider and a session is selected
- **THEN** the pane shows its first four rows of facts, from Waiting and Started down to Branch and Tab, and not Changes, Memory or Account

## ADDED Requirements

### Requirement: The pane's divider can be dragged
On the Sessions and Worktrees tabs the user SHALL be able to drag the pane's divider up or down with the mouse; the pane SHALL take the rows the table gives up, and give back the rows it gives up, within the pane's limits.

#### Scenario: Dragging up
- **WHEN** the Worktrees pane has ten lines under its divider and the user drags the divider up four rows
- **THEN** the pane has fourteen lines, the table four rows fewer, and the pane lists four more commits or changed files where the worktree has them

#### Scenario: Dragging past a limit
- **WHEN** the user drags the Worktrees pane's divider to the top of the table
- **THEN** the pane stops where the table keeps its heading and three rows

#### Scenario: Dragging the Sessions pane up
- **WHEN** the Sessions pane has six lines under its divider and the user drags the divider up
- **THEN** the pane keeps its six lines, because it has no more facts to show

### Requirement: The pane can be resized from the keyboard
On the Sessions and Worktrees tabs `+` SHALL grow the shown tab's pane by one line and `-` SHALL shrink it by one, within the pane's limits, except while a filter is being typed, when both are typed into the filter. A press at a limit SHALL change nothing. The help SHALL list both keys, and they SHALL NOT take room in the key bar.

#### Scenario: Growing by a key
- **WHEN** the Worktrees pane has ten lines under its divider and the user presses `+`
- **THEN** the pane has eleven lines and the table one row fewer

#### Scenario: Shrinking by a key
- **WHEN** the Sessions pane has six lines under its divider and the user presses `-` twice
- **THEN** the pane has four lines and the table two rows more

#### Scenario: Typing a filter
- **WHEN** the user has pressed `/` on the Worktrees tab and types `fix-1-2`
- **THEN** the filter reads fix-1-2 and the pane keeps its height

#### Scenario: The help
- **WHEN** the user presses `?` on the Sessions or the Worktrees tab
- **THEN** the help lists `+` as making the detail pane taller and `-` as making it shorter

### Requirement: Each tab's pane has its own height
The Sessions and Worktrees panes SHALL each keep their own height: resizing one SHALL NOT change the other.

#### Scenario: Resizing one tab
- **WHEN** the user makes the Worktrees pane fifteen lines tall and shows the Sessions tab
- **THEN** the Sessions pane has the height it had before

### Requirement: The pane keeps within its limits
The pane SHALL keep at least one line under its divider, and the table SHALL keep its heading and at least three rows. The Sessions pane SHALL grow no taller than its six rows of facts. A terminal too short for a tab's height SHALL show the pane as tall as the limits allow, and SHALL return to that height when the terminal is tall enough again.

#### Scenario: The smallest pane
- **WHEN** the Worktrees pane has one line under its divider and the user presses `-`
- **THEN** the pane keeps its one line

#### Scenario: The tallest Sessions pane
- **WHEN** the Sessions pane has six lines under its divider and the user presses `+`
- **THEN** the pane keeps its six lines

#### Scenario: A shorter terminal
- **WHEN** the Worktrees pane is twenty lines tall and the user makes the terminal too short to leave the table three rows
- **THEN** the pane shrinks so that the table keeps its heading and three rows, and grows back to twenty lines when the terminal is made tall again
