# Spec Delta

## MODIFIED Requirements

### Requirement: The Config tab shows the setup
After the settings, under the Setup heading row, the table SHALL have a grey row for each piece of setup Cader reads but does not edit: Settings file (the full path of `config.json`), Diagnostic log file (the full path of `cader.log`, whether the Diagnostic log setting is on or off, and its size or not written yet), Windows Terminal profiles (installed, with the fragment's path, or not set up), Claude bell (on, the other channel named, or not set) and Last update check (when, and the newest release found, or never, or off).

#### Scenario: Everything set up
- **WHEN** the fragment exists, Claude's settings say terminal_bell and the update source was asked at 09:12 today and named 0.18.0
- **THEN** the setup rows read the settings file's path, the diagnostic log file's path, installed with the fragment's path, on and 09:12 today, newest 0.18.0

#### Scenario: Nothing set up
- **WHEN** the fragment does not exist and Claude's settings file has no preferredNotifChannel
- **THEN** Windows Terminal profiles reads not set up, run cader setup-terminal, and Claude bell reads not set, run cader setup-terminal

#### Scenario: Another channel
- **WHEN** Claude's settings say iterm2
- **THEN** Claude bell reads iterm2, not the terminal bell

#### Scenario: The log file's path is always shown
- **WHEN** the local application data folder is C:\Users\me\AppData\Local, the Diagnostic log setting is off, `cader.log` does not exist and the user shows the Config tab
- **THEN** the Diagnostic log file row reads off, C:\Users\me\AppData\Local\Cader\cader.log

#### Scenario: The log is on but nothing is written yet
- **WHEN** the Diagnostic log setting is on and `cader.log` does not exist
- **THEN** the Diagnostic log file row reads on, not written yet, then the full path

#### Scenario: The log is on and written
- **WHEN** the Diagnostic log setting is on and `cader.log` holds 2,516,582 bytes
- **THEN** the Diagnostic log file row reads on, 2.4 MB, then the full path

#### Scenario: The log is off and a file is left
- **WHEN** the Diagnostic log setting is off and `cader.log` holds 512,000 bytes from when it was on
- **THEN** the Diagnostic log file row reads off, 500 KB, then the full path

#### Scenario: A small file
- **WHEN** the Diagnostic log setting is on and `cader.log` holds 300 bytes
- **THEN** the Diagnostic log file row reads on, 1 KB, then the full path

#### Scenario: A full file
- **WHEN** `cader.log` holds 10,485,700 bytes
- **THEN** the Diagnostic log file row shows 10.0 MB

### Requirement: The Config tab's counts and keys
Beside the tab titles the Config tab SHALL count its settings and, when any is missing, the setup to do, the latter in amber. Its key bar SHALL read Enter Edit, then c Copy, f Open folder, d Clear log, / Filter, ? Help, ! Report and q Quit. c SHALL copy a path value whole; f SHALL open a folder value in the editor, the file itself for the Settings file and both diagnostic log file rows, and the folder holding any other file value; on a value that is not a path both SHALL say so.

#### Scenario: Counts
- **WHEN** there are two roots and the Windows Terminal profiles are not set up
- **THEN** the counts read 9 settings · 1 to set up, the second part in amber

#### Scenario: Copy a long root
- **WHEN** a repository root row's value is elided and the user presses c
- **THEN** the clipboard holds the full path

#### Scenario: Open the settings file's folder
- **WHEN** the user presses f on Settings file
- **THEN** `config.json` itself opens in the editor chosen in settings, where it used to open the folder holding it

#### Scenario: Open the diagnostic log
- **WHEN** `cader.log` exists and the user presses f on Diagnostic log file
- **THEN** `cader.log` opens in the editor chosen in settings, whether or not the Diagnostic log setting is on

#### Scenario: Copy the log file's path
- **WHEN** the user presses c on Diagnostic log file
- **THEN** the clipboard holds the full path of `cader.log` and the status line says it was copied

#### Scenario: The log file does not exist
- **WHEN** `cader.log` does not exist and the user presses f on Diagnostic log file
- **THEN** no editor is started and the status line says the log file does not exist yet and that Diagnostic log turns it on

#### Scenario: The editor cannot be found
- **WHEN** the chosen editor cannot be found and the user presses f on Settings file
- **THEN** nothing is started and the status line says which editor could not be found and that Open folder with chooses another, as it does for a folder

#### Scenario: Another file value keeps opening its folder
- **WHEN** the user presses f on Windows Terminal profiles while it reads installed
- **THEN** the folder holding the fragment opens in the editor

#### Scenario: The key bar names the clear
- **WHEN** the user shows the Config tab
- **THEN** the key bar's second row reads c Copy, f Open folder, d Clear log, / Filter, ? Help, ! Report and q Quit

## ADDED Requirements

### Requirement: The older log file has its own row
While `cader.log.1` exists, the Setup section SHALL have a grey Diagnostic log file (older) row directly after the Diagnostic log file row, reading the older file's size and full path, whether the Diagnostic log setting is on or off. While it does not exist the row SHALL NOT be shown. It SHALL count neither as a setting nor as setup to do.

#### Scenario: A rollover has happened
- **WHEN** `cader.log.1` holds 10,485,700 bytes in C:\Users\me\AppData\Local\Cader
- **THEN** the row after Diagnostic log file reads Diagnostic log file (older) and 10.0 MB, C:\Users\me\AppData\Local\Cader\cader.log.1

#### Scenario: No rollover yet
- **WHEN** `cader.log.1` does not exist
- **THEN** the row after Diagnostic log file is Windows Terminal profiles

#### Scenario: Open the older file
- **WHEN** the user presses f on Diagnostic log file (older)
- **THEN** `cader.log.1` itself opens in the editor chosen in settings

#### Scenario: Copy the older file's path
- **WHEN** the user presses c on Diagnostic log file (older)
- **THEN** the clipboard holds the full path of `cader.log.1`

#### Scenario: Not counted
- **WHEN** there is one root, every piece of setup is in place and `cader.log.1` exists
- **THEN** the counts read 8 settings

### Requirement: d clears the diagnostic log after confirming
d on the Diagnostic log file row or the Diagnostic log file (older) row SHALL ask to confirm, naming each file that exists and its size. On Delete it SHALL delete `cader.log` and `cader.log.1`, the status line SHALL say how many files and how much was deleted, and the rows SHALL be read again. Cancel SHALL delete nothing and say so. The diagnostic events kept in memory for a problem report SHALL be kept.

#### Scenario: Clear both files
- **WHEN** `cader.log` holds 2.4 MB and `cader.log.1` 10.0 MB, the user presses d on Diagnostic log file and chooses Delete
- **THEN** both files are deleted, the status line says 2 files (12.4 MB) were deleted, and the Diagnostic log file (older) row is gone

#### Scenario: The question names the files
- **WHEN** `cader.log` holds 2.4 MB and `cader.log.1` does not exist and the user presses d on Diagnostic log file
- **THEN** the dialog asks to delete cader.log (2.4 MB), says the events kept for a problem report stay, and does not name cader.log.1

#### Scenario: From the older row
- **WHEN** the user presses d on Diagnostic log file (older) and chooses Delete
- **THEN** both files are deleted, as from the Diagnostic log file row

#### Scenario: Cancel
- **WHEN** the user presses d on Diagnostic log file and chooses Cancel
- **THEN** both files are as they were and the status line says they were kept

#### Scenario: The log is on
- **WHEN** the Diagnostic log setting is on and the user clears the log
- **THEN** `cader.log` holds only lines recorded after the clear, and the Diagnostic log file row shows its new, small size

#### Scenario: The log is off
- **WHEN** the Diagnostic log setting is off, `cader.log` exists and the user clears the log
- **THEN** no `cader.log` or `cader.log.1` is left, and the Diagnostic log file row reads off, then the full path

#### Scenario: The report keeps its events
- **WHEN** the user clears the log and then presses !
- **THEN** the report holds the same diagnostic events from before the clear as it would have without it

#### Scenario: Nothing to clear
- **WHEN** neither `cader.log` nor `cader.log.1` exists and the user presses d on Diagnostic log file
- **THEN** no dialog opens and the status line says there is no log file to delete

#### Scenario: Gone before Delete
- **WHEN** another program deletes both files after the dialog opens and the user chooses Delete
- **THEN** the status line says there was no log file to delete

#### Scenario: A file in use
- **WHEN** another program holds `cader.log.1` open so that it cannot be deleted, and the user clears the log
- **THEN** `cader.log` is deleted, `cader.log.1` is kept, the status line says one file could not be deleted because another program may have it open and to try again, and nothing fails

#### Scenario: Every file in use
- **WHEN** neither file can be deleted and the user clears the log
- **THEN** both are kept and the status line says no log file could be deleted and to try again, without saying anything was deleted

### Requirement: d elsewhere on the Config tab changes nothing
d on any Config row other than the two diagnostic log file rows SHALL change nothing and the status line SHALL say that d clears the diagnostic log from its file rows.

#### Scenario: d on a setting
- **WHEN** the user presses d on Worktree root
- **THEN** nothing changes and the status line says d clears the diagnostic log on the Diagnostic log file row

#### Scenario: d on the Diagnostic log setting
- **WHEN** the user presses d on Diagnostic log
- **THEN** the setting is unchanged and the status line says the same
