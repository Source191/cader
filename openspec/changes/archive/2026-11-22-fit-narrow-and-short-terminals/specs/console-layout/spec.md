## ADDED Requirements

### Requirement: The table's title never covers the counts
When the row of the tab titles has no room for the table's title after the counts, the console SHALL leave the title out rather than draw any part of it, an ellipsis included, over the counts.

#### Scenario: A 60-column pane
- **WHEN** the Sessions tab is shown on a 60 by 20 terminal and its counts reach the right edge
- **THEN** the counts read as they would at 120 columns, cut only by the terminal's edge, and no ellipsis is drawn among them

### Requirement: The key bar keeps Help and Quit on a narrow terminal
When a key bar row is wider than the terminal, the console SHALL drop items from that row's end until it fits, never dropping ? Help or q Quit, which SHALL move left to follow the last item kept. A row that fits SHALL be unchanged, and the help SHALL still list every key.

#### Scenario: A 60-column pane
- **WHEN** the Sessions tab is shown on a 60 by 20 terminal with a background session selected
- **THEN** the second row of the key bar starts c Copy path and ends ? Help and q Quit, every item it shows whole

#### Scenario: Dropped keys still answer
- **WHEN** the key bar on a 60-column terminal has dropped / Filter and the user presses /
- **THEN** the filter starts as it does at 120 columns

#### Scenario: Wide enough
- **WHEN** the Sessions tab is shown on a 120 by 36 terminal
- **THEN** the key bar is the same as before this change
