# Spec Delta

## ADDED Requirements

### Requirement: The new-session wizard records what it offered
When the new-session wizard opens from the overview, Cader SHALL record how many repositories it offers. A wizard opened on a fixed folder SHALL record nothing extra.

#### Scenario: The wizard opens with repositories
- **WHEN** the user presses `n` and the wizard offers Northwind.Orders, Northwind.Billing and Northwind.Stock
- **THEN** the log records that the wizard opened offering 3 repositories, and none of their names appears in it

#### Scenario: The wizard opens with none
- **WHEN** the user presses `n` and no repository is found under the roots
- **THEN** the log records that the wizard opened offering 0 repositories

### Requirement: The new-session wizard records its first filter change
Cader SHALL record the first change to the wizard's repository filter, once per wizard: the time since the dialog opened, the kind of the filter's first character (letter or digit, whitespace, control, format or other), how many characters the filter then holds, and whether the dialog had filled its list before it. No character of the filter SHALL be recorded.

#### Scenario: A stray space on opening
- **WHEN** a space reaches the filter 12 ms after the wizard opens, after the list was filled
- **THEN** the log records a first filter change after 12 ms, of kind whitespace, holding 1 character, with the list filled, and no later change is recorded for that wizard

#### Scenario: The user types a name
- **WHEN** the user types "Northwind.Orders" into the filter, starting 1.8 seconds after the wizard opens
- **THEN** the log records one first filter change after 1800 ms, of kind letter or digit, holding 1 character, and neither "Northwind" nor "Orders" appears in the log

#### Scenario: The filter is never changed
- **WHEN** the user opens the wizard and closes it with Esc without typing
- **THEN** the log records the wizard opening and no filter change

#### Scenario: Shared in a problem report
- **WHEN** the user closes the wizard and presses `!`
- **THEN** the report's log lines include the wizard's opening and its first filter change
