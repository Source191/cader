# Spec Delta

## ADDED Requirements

### Requirement: The picker explains a filter it cannot show
When the repository filter holds at least one character and every character in it is whitespace, a control character or a format character, the wizard's problem line SHALL say that the filter holds only blank or invisible characters and that Backspace clears it. The filter SHALL be kept exactly as received, and the message SHALL go as soon as the filter is empty or holds a visible character.

#### Scenario: A space in the filter
- **WHEN** the wizard lists Northwind.Orders and Northwind.Billing and the filter receives a single space
- **THEN** the list is empty, the filter still holds the space, and the problem line says the filter holds only blank or invisible characters and that Backspace clears it

#### Scenario: An invisible character in the filter
- **WHEN** the filter receives a zero-width space (U+200B) or an escape character (U+001B)
- **THEN** the problem line says the filter holds only blank or invisible characters and that Backspace clears it

#### Scenario: Backspace clears it
- **WHEN** the problem line shows the invisible-filter message and the user presses Backspace until the filter is empty
- **THEN** the message goes and the list shows every repository again

#### Scenario: A visible character with it
- **WHEN** the filter holds a space and the user types "o"
- **THEN** the message goes and the list narrows to repositories whose names contain " o", which may be none

#### Scenario: A filter that matches nothing visibly
- **WHEN** the user types "zzz" and no repository name contains it
- **THEN** the list is empty and the problem line does not show the invisible-filter message
