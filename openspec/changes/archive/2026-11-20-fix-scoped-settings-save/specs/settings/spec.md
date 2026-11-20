## MODIFIED Requirements

### Requirement: Settings are validated before they are saved
A setting's dialog SHALL refuse to save, name the problem and keep the dialog open when a repository root is not a full path, does not exist or is already a root, or when the worktree root is not a full path. A worktree root that does not exist yet SHALL be accepted. Saving the repository roots dialog with no root left SHALL be refused. Whether a repository root's folder or a custom editor's executable exists SHALL be checked only by the dialog that edits it, so a folder or file that has gone since it was saved never stops another setting being saved. A refusal SHALL keep the focus on a field the dialog shows.

#### Scenario: Batch file editor
- **WHEN** the user enters `C:\Tools\edit.cmd` as a custom editor's executable
- **THEN** nothing is saved and the dialog says a batch file cannot be used as the editor

#### Scenario: Bare-name editor
- **WHEN** the user enters `subl` as a custom editor's executable
- **THEN** nothing is saved and the dialog says the executable must be a full path

#### Scenario: Missing root
- **WHEN** the user enters `D:\nowhere` as a repository root and that folder does not exist
- **THEN** nothing is saved and the dialog says `D:\nowhere` does not exist

#### Scenario: Relative worktree root
- **WHEN** the user enters `worktrees` as the worktree root
- **THEN** nothing is saved and the dialog says the worktree root must be a full path

#### Scenario: New worktree root
- **WHEN** the user enters `D:\worktrees`, which does not exist yet
- **THEN** the settings are saved

#### Scenario: No root left
- **WHEN** the user deletes every line of the repository roots dialog and presses F5
- **THEN** nothing is saved and the dialog asks for a repository root

#### Scenario: A saved root has gone
- **WHEN** the saved repository roots include `D:\work`, which no longer exists, and the user changes Sessions refresh from 5 to 2 and presses F5
- **THEN** the settings are saved with Sessions refresh 2 and the repository roots unchanged

#### Scenario: A saved custom editor has gone
- **WHEN** the saved editor is a custom editor whose executable `E:\Tools\subl.exe` no longer exists, and the user changes Default place and presses F5
- **THEN** the settings are saved with the new place and the custom editor unchanged

#### Scenario: Another setting is refused
- **WHEN** `config.json` lists `D:\work` twice as a repository root and the user changes Worktrees refresh and presses F5
- **THEN** nothing is saved, the dialog stays open and says `D:\work` is already a root, and the cursor stays in the Worktrees refresh box
