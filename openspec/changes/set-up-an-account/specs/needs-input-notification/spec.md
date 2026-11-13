# Spec Delta

## ADDED Requirements

### Requirement: The fragment keeps the account profiles
Cader's fragment SHALL hold one "Claude (<name>)" profile per account `setup-account` added, beside the Cader profiles, and both `setup-terminal` and `setup-account` SHALL write the whole fragment from the configured accounts so neither drops the other's profiles. `setup-terminal --remove` SHALL say that the account profiles go with the fragment.

#### Scenario: setup-terminal after setup-account
- **WHEN** personal was set up and the user runs `cader setup-terminal --yes` with a new sound
- **THEN** the fragment holds the Cader profiles with the new sound and still holds Claude (personal)

#### Scenario: Removing the fragment
- **WHEN** personal was set up and the user runs `cader setup-terminal --remove`
- **THEN** the fragment folder is gone and the output says the Claude (personal) profile went with it and that `cader setup-account personal` adds it back
