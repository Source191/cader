# needs-input-notification Specification

## Purpose
Tells the user, without blocking the session or the terminal, that a session needs them.

## Requirements

### Requirement: The console rings the terminal bell when a session starts waiting
Because the agent_needs_input and agent_completed hook types fire only while the native agent view is open, Cader SHALL write the terminal bell character to its own terminal whenever a refresh shows a background session in needs input that was in another state at the previous refresh, and SHALL NOT ring for a session that was already waiting, for completed or failed transitions, for interactive sessions, or on the first load.

#### Scenario: Session starts waiting
- **WHEN** a refresh shows a background session in needs input that was working at the previous refresh
- **THEN** the bell rings once in the Cader tab, so Windows Terminal flags it according to the profile's bell style

#### Scenario: No duplicate bell
- **WHEN** a session was already in needs input at the previous refresh
- **THEN** no bell rings for it until its state changes again

#### Scenario: Interactive sessions ring their own tab
- **WHEN** an interactive session starts waiting
- **THEN** Cader stays silent, because Claude Code rings the bell in that session's own tab

### Requirement: Claude Code's own bell covers attached tabs
The user guide SHALL have one short notifications section, and the README's getting started one step, that tell the user to run `cader setup-terminal`, which sets Claude Code's `preferredNotifChannel` to `terminal_bell` and so makes Claude ring the bell in the tab a session is attached in when it finishes a task or pauses for a permission prompt while the user appears away.

#### Scenario: Attached session needs permission
- **WHEN** a session attached in a Cader-opened tab pauses for a permission prompt while the user is in another tab
- **THEN** Claude Code rings the bell in that tab and Windows Terminal flags it

#### Scenario: Choosing a sound by hand
- **WHEN** the user picks `C:\Windows\Media\Windows Message Nudge.wav` as the Cader profile's bell sound in Windows Terminal's settings
- **THEN** a bell in the Cader tab plays that sound and bells in other profiles keep theirs

#### Scenario: Reading the notifications section
- **WHEN** the user reads the user guide's notifications section
- **THEN** it names `cader setup-terminal` as the one step that sets up both bells

#### Scenario: Reading the getting started
- **WHEN** the user reads the README's getting started
- **THEN** one step names `cader setup-terminal` as what adds the Cader profiles and both bells, and links to the user guide's notifications section

### Requirement: No toasts and no hooks
Cader SHALL NOT raise Windows toast notifications and SHALL NOT install Claude Code hooks; attention is the terminal bell, the header's needs-input count and the amber rows.

#### Scenario: Install
- **WHEN** Cader is installed
- **THEN** nothing is written to the user's Claude Code hooks

#### Scenario: Terminal setup
- **WHEN** the user runs `cader setup-terminal --yes`
- **THEN** nothing is written to the user's Claude Code hooks

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

### Requirement: Setup asks before writing
Unless `--yes` is given, the setup command SHALL show every file it will write or change and what it will put there, and SHALL write nothing until the user answers yes; any other answer, or the end of input, SHALL leave every file as it was and exit 0.

#### Scenario: User declines
- **WHEN** the user runs `cader setup-terminal` and answers n
- **THEN** no file is written or changed and the output says nothing was written

#### Scenario: Input is not interactive
- **WHEN** the command runs without `--yes` and its input ends before an answer
- **THEN** no file is written or changed

### Requirement: Setup chooses the bell sounds
The "Cader" profile's bell sound SHALL default to `C:\Windows\Media\Windows Notify Messaging.wav` and the "Cader session" profile's to `C:\Windows\Media\Windows Proximity Notification.wav`; `--sound` and `--session-sound` SHALL replace them. A default that does not exist on the machine SHALL be left out, so that profile uses Windows Terminal's own bell sound, and the output SHALL say so.

#### Scenario: Custom sound
- **WHEN** the user runs `cader setup-terminal --yes --sound "C:\Windows\Media\chimes.wav"`
- **THEN** the "Cader" profile's bell sound is chimes.wav and the "Cader session" profile keeps its default

#### Scenario: Default sound absent
- **WHEN** `Windows Notify Messaging.wav` does not exist on the machine
- **THEN** the "Cader" profile has no bell sound and the output says Windows Terminal's default sound will play

### Requirement: A chosen sound must be a wav file that exists
A sound given with `--sound` or `--session-sound` that is not an existing `.wav` file SHALL be a usage error with exit code 2, and nothing SHALL be written.

#### Scenario: Missing sound file
- **WHEN** the user runs `cader setup-terminal --sound C:\Users\me\missing.wav`
- **THEN** the command prints the usage, names the missing file, writes nothing and exits 2

#### Scenario: Not a wav file
- **WHEN** the user runs `cader setup-terminal --session-sound C:\Windows\Media\notes.txt` and that file exists
- **THEN** the command prints the usage, writes nothing and exits 2

### Requirement: Setup can turn on Claude Code's terminal bell
The setup command SHALL offer to set `preferredNotifChannel` to `terminal_bell` in `~\.claude\settings.json`, creating the file when it is missing, keeping every other setting and its order, and writing bare LF line endings. When the setting already has that value the command SHALL say so and leave the file untouched; when the file is not valid JSON it SHALL leave it untouched and say why.

#### Scenario: Other settings kept
- **WHEN** settings.json holds a model and a permissions block and the user accepts
- **THEN** settings.json holds the same model and permissions block in the same order, plus `"preferredNotifChannel": "terminal_bell"`, with LF line endings only

#### Scenario: Already set
- **WHEN** settings.json already sets `preferredNotifChannel` to `terminal_bell`
- **THEN** the file is not rewritten and the output says the terminal bell is already on

#### Scenario: Unreadable settings
- **WHEN** settings.json is not valid JSON
- **THEN** the file is unchanged, the output says it could not be read, and the profiles are still written

### Requirement: Setup can be undone
`cader setup-terminal --remove` SHALL delete Cader's fragment folder and nothing else, and SHALL report when there was nothing to remove.

#### Scenario: Remove
- **WHEN** the fragment exists and the user runs `cader setup-terminal --remove`
- **THEN** the fragment folder is gone, `~\.claude\settings.json` is unchanged, and the output says to restart Windows Terminal

#### Scenario: Nothing installed
- **WHEN** no fragment exists and the user runs `cader setup-terminal --remove`
- **THEN** nothing changes, the output says there was nothing to remove, and the command exits 0

### Requirement: The profiles' icon lives in Cader's fragment folder
The setup command SHALL copy the icon file into Cader's fragment folder, so that removing the folder removes it; when the tool carries no icon file the profiles SHALL be written without an icon and setup SHALL still succeed.

#### Scenario: The icon beside the fragment
- **WHEN** the user runs `cader setup-terminal --yes` and the tool carries its icon file
- **THEN** a copy of the icon sits in the fragment's own folder and both profiles name that copy

### Requirement: Setup leaves Windows Terminal's own settings alone
The setup command SHALL NOT change the user's Windows Terminal settings file except to add, once the user agrees, an entry for a Cader profile Windows Terminal is hiding as deleted; it SHALL NOT give the "Cader" profile a tab colour, and SHALL tell the user to restart Windows Terminal.

#### Scenario: Settings file and tab colour untouched
- **WHEN** the user runs `cader setup-terminal --yes` and Windows Terminal is hiding neither Cader profile
- **THEN** Windows Terminal's settings file is unchanged, the "Cader" profile in the fragment has no tab colour, and the output says to restart Windows Terminal

#### Scenario: The one change it makes
- **WHEN** the user runs `cader setup-terminal --yes` and Windows Terminal is hiding the "Cader" profile
- **THEN** the only change to Windows Terminal's settings file is the one entry added for "Cader", and the fragment's "Cader" profile still has no tab colour

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
