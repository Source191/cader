## ADDED Requirements

### Requirement: The wizard fits a smaller terminal or says it is too small
The new-session wizard SHALL fit the terminal it opens in, its repository list taking the rows its form leaves, with no control drawn over another. When the terminal is too small for every control and one repository row, the wizard SHALL NOT open, and the status line SHALL name the size the wizard needs and the size the terminal is.

#### Scenario: Windows Terminal's default size
- **WHEN** the user presses n on a 120 by 30 terminal with twelve repositories under the roots
- **THEN** the filter, at least one repository row, every form control and the buttons are on screen, and no control is drawn over another

#### Scenario: Four repositories at 120 by 36
- **WHEN** the user presses n on a 120 by 36 terminal with twelve repositories under the roots
- **THEN** at least four repository rows are listed whole, none of them under the Session name caption

#### Scenario: A narrow pane
- **WHEN** the user presses n on a 70 by 24 terminal
- **THEN** no dialog opens and the status line says the new-session window needs at least 80 columns and its rows, and that this terminal is 70 x 24

#### Scenario: The fixed-folder wizard on a narrow pane
- **WHEN** the user presses n on a worktree row on a 70 by 24 terminal
- **THEN** no dialog opens and the status line says the window needs at least 84 x 23 and this terminal is 70 x 24
