# Spec Delta

## MODIFIED Requirements

### Requirement: The setup command adds Cader's Windows Terminal profiles
`cader setup-terminal` SHALL write a Windows Terminal JSON fragment for the current user that adds a "Cader" profile, which runs the installed Cader, and a "Cader session" profile, which runs PowerShell, each with a bell style that sounds, flashes the window and flashes the taskbar, and each with the Cader icon as its tab icon.

#### Scenario: First run
- **WHEN** the user runs `cader setup-terminal --yes`, no fragment exists and Windows Terminal is hiding neither Cader profile
- **THEN** the fragment exists with the two profiles, each naming the copied icon file, Windows Terminal's settings file is unchanged, the output says to restart Windows Terminal, and the command exits 0

#### Scenario: Run again
- **WHEN** the fragment already exists and the user runs the command again with other sounds
- **THEN** the fragment is replaced with the new sounds and still holds exactly the two profiles, each with the icon

#### Scenario: No icon file beside the tool
- **WHEN** the user runs `cader setup-terminal --yes` and the tool's icon file is missing
- **THEN** the fragment holds the two profiles without an icon property, no error is shown, and the command exits 0

#### Scenario: Remove takes the icon too
- **WHEN** the fragment exists with its icon and the user runs `cader setup-terminal --remove`
- **THEN** the fragment folder, including the icon copy, is gone

#### Scenario: Cader tab colour still applies
- **WHEN** Cader starts in a tab opened with the "Cader" profile
- **THEN** the tab is titled and coloured as the session-attach spec describes for Cader's own tab

### Requirement: Setup leaves Windows Terminal's own settings alone
The setup command SHALL NOT change the user's Windows Terminal settings file except to add, once the user agrees, an entry for a Cader profile Windows Terminal is hiding as deleted; it SHALL NOT give the "Cader" profile a tab colour, and SHALL tell the user to restart Windows Terminal.

#### Scenario: Settings file and tab colour untouched
- **WHEN** the user runs `cader setup-terminal --yes` and Windows Terminal is hiding neither Cader profile
- **THEN** Windows Terminal's settings file is unchanged, the "Cader" profile in the fragment has no tab colour, and the output says to restart Windows Terminal

#### Scenario: The one change it makes
- **WHEN** the user runs `cader setup-terminal --yes` and Windows Terminal is hiding the "Cader" profile
- **THEN** the only change to Windows Terminal's settings file is the one entry added for "Cader", and the fragment's "Cader" profile still has no tab colour

## ADDED Requirements

### Requirement: Setup reports a Cader profile Windows Terminal is hiding
Once the fragment exists, `cader setup-terminal` SHALL report each Cader profile that Windows Terminal is hiding as deleted: one whose id Terminal's state file lists among the profiles it has generated and that has no entry with that id in Terminal's settings file. A profile Terminal has not loaded yet, or one with an entry, SHALL NOT be reported. When Terminal's files are missing or cannot be read, the command SHALL write nothing to them and SHALL carry on.

#### Scenario: Deleted in Windows Terminal
- **WHEN** the user has deleted the "Cader session" profile in Windows Terminal's settings and runs `cader setup-terminal`
- **THEN** the output says Windows Terminal is hiding "Cader session" because it keeps a deleted profile hidden, and names Terminal's settings file and the entry it would add

#### Scenario: Never loaded
- **WHEN** the user runs `cader setup-terminal --yes` for the first time and Terminal's state file lists neither Cader profile
- **THEN** nothing is reported as hidden and Windows Terminal's settings file is unchanged

#### Scenario: Hidden by the user's own choice
- **WHEN** the "Cader" profile's entry in Terminal's settings file says `"hidden": true`
- **THEN** it is not reported as hidden and its entry is unchanged

#### Scenario: Windows Terminal has never run
- **WHEN** Terminal's state file does not exist and the user runs `cader setup-terminal --yes`
- **THEN** nothing is reported as hidden, no Windows Terminal file is created, the fragment is written and the command exits 0

#### Scenario: Unreadable Windows Terminal settings
- **WHEN** Terminal's settings file is not valid JSON and the user runs `cader setup-terminal --yes`
- **THEN** the output says it could not be read and was left alone, the fragment is still written, Claude Code's bell is still offered, and the command exits 0

#### Scenario: Fragment declined
- **WHEN** no fragment exists, Terminal's state file lists a Cader profile with no entry, and the user answers n to writing the profiles
- **THEN** nothing is reported as hidden and Windows Terminal's settings file is unchanged

### Requirement: Setup can show a hidden profile again
When a Cader profile is hidden, the setup command SHALL show the entries it will add and ask before writing; on yes, or with `--yes`, it SHALL add to the profile list in Terminal's settings file one entry for each hidden profile only, holding that profile's id, `"hidden": false`, its name and the source `Cader`, and SHALL leave every other character of the file as it was. A failed write SHALL be reported with the file's path and exit 1.

#### Scenario: Show it again
- **WHEN** Windows Terminal is hiding "Cader session" and the user answers y
- **THEN** Terminal's settings file's profile list holds one more entry, `{ "guid": "{cdf8708d-a821-5c97-95d0-fea560807dea}", "hidden": false, "name": "Cader session", "source": "Cader" }`, every other character of the file is as it was, the output says Windows Terminal shows the profile again once it reads the file, with no restart, and the command exits 0

#### Scenario: Both hidden
- **WHEN** Windows Terminal is hiding both Cader profiles and the user runs `cader setup-terminal --yes`
- **THEN** the profile list holds one entry for "Cader", with the id `{3d7a06d2-710f-51d9-90c9-dacae2c3ab46}`, and one for "Cader session", and nothing is asked

#### Scenario: Comments kept
- **WHEN** Terminal's settings file holds `//` comments and Windows Terminal is hiding "Cader"
- **THEN** after the entry is added every comment is still in the file, where it was

#### Scenario: Declined
- **WHEN** Windows Terminal is hiding "Cader" and the user answers n
- **THEN** Terminal's settings file is unchanged and the command exits 0

#### Scenario: The settings file cannot be written
- **WHEN** Windows Terminal is hiding "Cader", the user answers y and Terminal's settings file is read-only
- **THEN** the error output names the file and why it could not be written, the fragment is kept, and the command exits 1
