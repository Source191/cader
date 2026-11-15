# Spec Delta

## MODIFIED Requirements

### Requirement: Setup can be undone
`cader setup-terminal --remove` SHALL delete Cader's fragment folder and, from Windows Terminal's settings file, only the entries that are exactly the entry setup adds (a Cader profile's `guid`, `"hidden": false`, its `name` and the `source` `Cader`, and no other property), after showing them and asking (or with `--yes`), through the same guarded write as adding one. It SHALL keep every other byte of the file, SHALL leave an entry with any other property alone, SHALL leave `~\.claude\settings.json` alone, and SHALL report when there was nothing to remove.

#### Scenario: Remove
- **WHEN** the fragment exists and the user runs `cader setup-terminal --remove`
- **THEN** the fragment folder is gone, `~\.claude\settings.json` is unchanged, and the output says to restart Windows Terminal

#### Scenario: Nothing installed
- **WHEN** no fragment exists, Terminal's settings file holds no entry setup added, and the user runs `cader setup-terminal --remove`
- **THEN** nothing changes, the output says there was nothing to remove, and the command exits 0

#### Scenario: The entries setup added are taken out
- **WHEN** setup added the "Cader session" entry to Terminal's settings file and the user runs `cader setup-terminal --remove --yes`
- **THEN** that entry and the comma setup added with it are gone, every other byte of the file is as it was before setup added it, the output names the backup, and the command exits 0

#### Scenario: A customised entry is kept
- **WHEN** Terminal's settings file holds the "Cader" entry with a `colorScheme` added, and the user runs `cader setup-terminal --remove --yes`
- **THEN** that entry is unchanged and nothing is offered for taking out

#### Scenario: Declined
- **WHEN** Terminal's settings file holds an entry setup added and the user answers n
- **THEN** Terminal's settings file is unchanged and the command exits 0

#### Scenario: Taking out of a read-only settings file
- **WHEN** setup added the "Cader session" entry, Terminal's settings file is now read-only, and the user runs `cader setup-terminal --remove --yes`
- **THEN** the fragment folder is gone, nothing is written to Terminal's settings file, the error output says it is read-only and gives the entry to take out by hand, and the command exits 1

#### Scenario: The first, a middle, the last or the only element
- **WHEN** the entry setup added is the first, a middle, the last or the only element of the profile list
- **THEN** after it is taken out the file is still read as JSON and its profile list holds every other element, in order

### Requirement: Setup leaves Windows Terminal's own settings alone
The setup command SHALL NOT change the user's Windows Terminal settings file except to add, once the user agrees, an entry for a Cader profile Windows Terminal is hiding as deleted, or, with `--remove`, to take out such an entry it added; it SHALL NOT give the "Cader" profile a tab colour, and SHALL tell the user to restart Windows Terminal. Every change SHALL be written beside the file and checked before it replaces it: the new file SHALL be read back with the same reader and SHALL hold every element of the profile list in order plus exactly the entries added (or minus exactly those taken out), and the settings file SHALL be read again just before it is replaced. When either check fails, nothing SHALL be written and the command SHALL say why and exit 1. Before replacing the file the command SHALL keep a copy of it beside it, named `settings.json.<yyyy-MM-ddTHH-mm-ss>.cader.backup` with the UTC time, and SHALL name that copy in its output.

#### Scenario: Settings file and tab colour untouched
- **WHEN** the user runs `cader setup-terminal --yes` and Windows Terminal is hiding neither Cader profile
- **THEN** Windows Terminal's settings file is unchanged, the "Cader" profile in the fragment has no tab colour, and the output says to restart Windows Terminal

#### Scenario: The one change it makes
- **WHEN** the user runs `cader setup-terminal --yes` and Windows Terminal is hiding the "Cader" profile
- **THEN** the only change to Windows Terminal's settings file is the one entry added for "Cader", and the fragment's "Cader" profile still has no tab colour

#### Scenario: A backup is kept
- **WHEN** the user adds a hidden profile's entry at 09:30:15 UTC on 15 November 2026
- **THEN** `settings.json.2026-11-15T09-30-15.cader.backup` beside the settings file holds the file as it was, the settings file keeps its creation time and permissions, and the output names the backup

#### Scenario: Windows Terminal saves the file meanwhile
- **WHEN** Windows Terminal saves its settings file after setup has read it and before setup replaces it
- **THEN** Terminal's save is kept, setup writes nothing and keeps no backup, the error output says the file changed while setup was writing it and to run the command again, and the command exits 1

#### Scenario: The new file does not check out
- **WHEN** Terminal's settings file names `profiles` twice, so the entry would land in a list the reader does not use
- **THEN** nothing is written, the error output says the new file did not hold the profile list expected, and the command exits 1

### Requirement: Setup can show a hidden profile again
When a Cader profile is hidden, the setup command SHALL show the entries it will add and ask before writing; on yes, or with `--yes`, it SHALL add to the profile list in Terminal's settings file one entry for each hidden profile only, holding that profile's id, `"hidden": false`, its name and the source `Cader`, and SHALL leave every other character of the file as it was. A failed write SHALL be reported with the file's path and exit 1, and a settings file that is a link, or is read-only, SHALL NOT be written: the command SHALL say which, give the entries to add by hand and exit 1.

#### Scenario: Show it again
- **WHEN** Windows Terminal is hiding "Cader session" and the user answers y
- **THEN** Terminal's settings file's profile list holds one more entry, `{ "guid": "{cdf8708d-a821-5c97-95d0-fea560807dea}", "hidden": false, "name": "Cader session", "source": "Cader" }`, every other character of the file is as it was, the output says Windows Terminal shows the profile again once it reads the file, with no restart, and names the backup, and the command exits 0

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
- **WHEN** Windows Terminal is hiding "Cader", the user answers y and Terminal's settings file is in use by a program that will not share it
- **THEN** the error output names the file and why it could not be written, the fragment is kept, and the command exits 1

#### Scenario: The settings file is read-only
- **WHEN** Windows Terminal is hiding "Cader", the user answers y and Terminal's settings file is read-only
- **THEN** nothing is written and the file is still read-only, the error output says the file is read-only and gives the entry to add by hand, Claude Code's bell is still offered, and the command exits 1

#### Scenario: The settings file is a link
- **WHEN** Windows Terminal is hiding "Cader", the user answers y and Terminal's settings file is a symbolic link to a file elsewhere, as a dotfiles set-up makes it
- **THEN** nothing is written and the link is still a link, the error output says the file is a link and gives the entry to add by hand to the file it links to, Claude Code's bell is still offered, and the command exits 1
