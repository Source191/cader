## ADDED Requirements

### Requirement: A refresh keeps the table's place
Each table (Sessions, Worktrees and Config) SHALL keep its scroll position when its rows are refreshed or redrawn, so the rows on screen stay where they were and nothing visibly jumps. The table SHALL scroll only when the selection or the filter has changed, and then only as far as keeps the selected row on screen.

#### Scenario: A rescan partway down
- **WHEN** the Worktrees tab is scrolled down so its 30th row is selected and a rescan returns the same rows
- **THEN** the same rows are on screen and the 30th row is still selected

#### Scenario: A view scrolled with the mouse wheel
- **WHEN** the user scrolls a table with the mouse wheel away from the selected row and a refresh lands
- **THEN** the table stays where the wheel left it

#### Scenario: Down past the last row on screen
- **WHEN** the selected row is the last on screen and the user presses Down
- **THEN** the table scrolls by one row and the newly selected row is on screen

#### Scenario: Up within the rows on screen
- **WHEN** the table is scrolled down and the user presses Up with the selected row in the middle of the table
- **THEN** the selection moves up and the table does not scroll

#### Scenario: A refresh with fewer rows
- **WHEN** the table is scrolled near the end of 100 rows and a refresh returns 20
- **THEN** the table scrolls back only as far as shows the last of the 20 at its bottom
