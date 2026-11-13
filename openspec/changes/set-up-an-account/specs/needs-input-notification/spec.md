# Spec Delta

## ADDED Requirements

### Requirement: Account profiles live in a fragment file of their own
The "Claude (<name>)" profiles SHALL be written to a fragment file of their own in Cader's fragment folder, so `setup-account` never adds the Cader profiles and never makes Cader open its tabs with the Cader session profile, and `setup-terminal` never drops an account profile. `setup-terminal --remove` SHALL say that the account profiles go with the folder.

#### Scenario: setup-account before setup-terminal
- **WHEN** `cader setup-terminal` has never been run and the user sets up personal
- **THEN** Cader's fragment folder holds the Claude (personal) profile only, and the next tab Cader opens is opened as before, without the Cader session profile

#### Scenario: setup-terminal after setup-account
- **WHEN** personal was set up and the user runs `cader setup-terminal --yes` with a new sound
- **THEN** the Cader profiles have the new sound and the Claude (personal) profile is unchanged

#### Scenario: Removing the fragment
- **WHEN** personal was set up and the user runs `cader setup-terminal --remove`
- **THEN** the fragment folder is gone and the output says the Claude (personal) profile went with it and that `cader setup-account personal` adds it back
