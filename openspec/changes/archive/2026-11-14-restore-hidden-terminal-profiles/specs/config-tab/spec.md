# Spec Delta

## ADDED Requirements

### Requirement: The profiles row says when Windows Terminal hides them
While the fragment exists and Windows Terminal is hiding either Cader profile as deleted, as the setup command reports it, the Windows Terminal profiles row SHALL read hidden by Windows Terminal, run cader setup-terminal, and SHALL count as setup to do. When Terminal's files are missing or cannot be read, the row SHALL read as it does without them.

#### Scenario: Deleted in Windows Terminal
- **WHEN** the fragment exists and the user has deleted the "Cader session" profile in Windows Terminal's settings
- **THEN** Windows Terminal profiles reads hidden by Windows Terminal, run cader setup-terminal

#### Scenario: Counted as setup to do
- **WHEN** every other piece of setup is in place and Windows Terminal is hiding the "Cader" profile
- **THEN** the counts end 1 to set up, in amber

#### Scenario: Shown again
- **WHEN** the user runs cader setup-terminal in another tab, agrees to show the hidden profiles again, and then shows the Config tab
- **THEN** Windows Terminal profiles reads installed, with the fragment's path

#### Scenario: Windows Terminal has never run
- **WHEN** the fragment exists and Terminal's state file does not exist
- **THEN** Windows Terminal profiles reads installed, with the fragment's path

#### Scenario: No fragment
- **WHEN** the fragment does not exist and Terminal's state file lists both Cader profiles
- **THEN** Windows Terminal profiles reads not set up, run cader setup-terminal
