## MODIFIED Requirements

### Requirement: A session name is mandatory
The wizard SHALL require a non-blank name, SHALL accept any other name, including one holding a semicolon or a double quote, and SHALL use the name as typed, with any blank characters at its start and end dropped, as the session name, the tab title and the Remote Control name. A refused name SHALL be explained in a plain sentence, in the wizard and by `cader here`, with nothing after it.

#### Scenario: Blank name
- **WHEN** the user submits a blank name
- **THEN** the wizard refuses to continue and explains that a name is required

#### Scenario: Name with a semicolon
- **WHEN** the user submits the name `fix a;b` and opens the session in a tab
- **THEN** the session is named `fix a;b`, exactly one tab opens, and it is titled `fix a;b`

#### Scenario: Name with a double quote
- **WHEN** the user submits the name `say "hi"` and opens the session in a tab
- **THEN** the session is named `say "hi"` and its tab is titled `say "hi"`

#### Scenario: A stray space before the name
- **WHEN** the user presses `n` on a worktree row, a space lands in the name box with it, and the user types `fix-login` and starts the session
- **THEN** the session is named `fix-login` and its tab is titled `fix-login`

#### Scenario: Spaces inside the name are kept
- **WHEN** the user submits the name ` fix the login ` (with a space at each end)
- **THEN** the session is named `fix the login`

#### Scenario: The explanation is only the sentence
- **WHEN** the wizard or `cader here` refuses a blank name
- **THEN** the explanation is exactly "A session name is required.", with no parameter name or other technical text after it, and it fits on the wizard's problem line on a 120-column terminal

#### Scenario: A blank name given to cader here
- **WHEN** `cader here ""` is run
- **THEN** it prints exactly "A session name is required." and starts nothing

#### Scenario: A name of only spaces given to cader here
- **WHEN** `cader here "   "` is run
- **THEN** it prints exactly "A session name is required." and starts nothing

## ADDED Requirements

### Requirement: The picker ignores blank characters before the filter
The repository filter SHALL ignore whitespace, control and format characters at its start when choosing which repositories to list, so a filter holding only such characters lists every repository. Characters after the first visible one, spaces included, SHALL still count. The filter's text SHALL be kept exactly as received.

#### Scenario: A stray space as the wizard opens
- **WHEN** the wizard lists Northwind.Orders and Northwind.Billing and the filter receives a single space
- **THEN** the filter still holds the space, both repositories are listed, and the problem line says nothing about the filter

#### Scenario: An invisible character in the filter
- **WHEN** the filter receives a zero-width space (U+200B) or an escape character (U+001B)
- **THEN** every repository is listed

#### Scenario: A space before the text
- **WHEN** the filter holds a space followed by "orders"
- **THEN** the list shows Northwind.Orders, as it would for "orders"

#### Scenario: A space inside the text still counts
- **WHEN** the wizard lists Northwind Orders and Northwind.Orders and the user types "d o"
- **THEN** only Northwind Orders is listed

#### Scenario: A filter that matches nothing visibly
- **WHEN** the user types "zzz" and no repository name contains it
- **THEN** the list is empty and the problem line says nothing about the filter

## REMOVED Requirements

### Requirement: The picker explains a filter it cannot show
**Reason**: A filter holding only blank or invisible characters now lists every repository (The picker ignores blank characters before the filter), so there is no empty list to explain.
**Migration**: None: the stray character no longer hides anything; Backspace still removes it.
