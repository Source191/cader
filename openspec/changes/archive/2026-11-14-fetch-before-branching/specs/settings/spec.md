## ADDED Requirements

### Requirement: Before branching is kept in the settings file
`config.json` SHALL hold Before branching as `beforeBranching`, one of `Fetch`, `Pull` or `None`, and a file written by Cader SHALL always name it. A missing property SHALL read as `Fetch` and leave every other setting as the file says; a name that is not one of those three, or a number, SHALL read as a corrupt file does, every setting its default, as for the default base. A saved change SHALL apply to the next wizard and launch without a restart.

#### Scenario: File from an earlier version
- **WHEN** `config.json` holds only `repositoryRoots` of `D:\work` and `defaultBase` of `Head`
- **THEN** Before branching reads Fetch origin, Cader scans `D:\work` and branches from HEAD

#### Scenario: Pull by hand
- **WHEN** `config.json` holds `beforeBranching` of `Pull`
- **THEN** the Before branching row reads Pull default branch and the wizard opened with n starts on it

#### Scenario: An unknown value
- **WHEN** `config.json` holds `beforeBranching` of `Rebase` and `repositoryRoots` of `D:\work`
- **THEN** Cader starts with every default, as it does for a corrupt file, so Before branching reads Fetch origin, and nothing exits

#### Scenario: Written back
- **WHEN** the user saves any setting from the Config tab
- **THEN** `config.json` names `beforeBranching` with the current value
