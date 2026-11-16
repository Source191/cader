## MODIFIED Requirements

### Requirement: Open a session in its own Windows Terminal tab
Enter on a background session is the open-in-tab key. It SHALL open a new tab in the current Windows Terminal window that runs the attach command for the session, titled with the session name, with application title changes suppressed and Claude Code's own terminal title updates turned off in it, so the title stays the session name while the session works, starting in the session's working directory, coloured from the palette, and in the "Cader session" profile when Cader's Windows Terminal fragment is installed, otherwise in the default profile.

#### Scenario: Tab opened
- **WHEN** the user presses Enter on a background session
- **THEN** a new tab appears with the session's name as its title and the attach command running inside it, and the console stays on screen

#### Scenario: Tab opened with the setup profile
- **WHEN** `cader setup-terminal` has installed the fragment and the user presses Enter on a background session
- **THEN** the new tab uses the "Cader session" profile, so a bell in it plays that profile's sound

#### Scenario: Tab opened without the setup profile
- **WHEN** the fragment is not installed and the user presses Enter on a background session
- **THEN** the new tab uses Windows Terminal's default profile, as before

#### Scenario: Tab already live
- **WHEN** a Cader-opened tab for that session is still alive and the user presses Enter on it
- **THEN** no second tab is opened, the Windows Terminal tab titled with the session name is selected in the window that owns it, and the status line says Cader switched to it

#### Scenario: Title stays the name while the session works
- **WHEN** the session fix-123, open in its Cader tab, starts working on a prompt
- **THEN** the tab's title stays fix-123, with no status glyph before it

#### Scenario: Tab already live while the session works
- **WHEN** a Cader-opened tab for the working session fix-123 is still alive, Claude Code has titled it ◐ fix-123, and the user presses Enter on the session
- **THEN** no second tab is opened, that tab is selected, and the status line says Cader switched to it

#### Scenario: Another tab merely containing the name
- **WHEN** a Cader-opened tab for fix-123 is recorded as alive but the only tabs in its window are titled fix-1234, notes on fix-123 and ◐◑ fix-123
- **THEN** none of them is selected, no tab is opened, and the status line says the tab is open but could not be found

#### Scenario: Live tab cannot be found
- **WHEN** a Cader-opened tab for that session is recorded as alive but no tab in its Windows Terminal window is titled with the session's name, alone or after one status glyph and a space
- **THEN** no tab is opened and the status line says the tab is open but could not be found

#### Scenario: Tab closed by hand
- **WHEN** the user closes the session's tab with the terminal's close button and, once the tab's shell has gone, presses Enter on the session before the next refresh
- **THEN** the stale record is dropped, a new tab opens, and the status line says a tab was opened, not switched to

#### Scenario: Earlier record dropped
- **WHEN** a record for the session remains from a tab that has gone and the user presses Enter on the session
- **THEN** the new tab's record is the only one for the session and the old record's colour is free again

#### Scenario: Empty session
- **WHEN** the user presses Enter on a running background session that has never been sent a message
- **THEN** its tab shows that session's own prompt, not an agent list, and Ctrl+Z in it closes the tab as for any session

#### Scenario: o is not a key
- **WHEN** the user presses o on a background session
- **THEN** nothing happens and no tab is opened
