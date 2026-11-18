## ADDED Requirements

### Requirement: A repository root that is not found is marked
When the Config tab reads its setup, it SHALL check whether each repository root's folder exists, and a root whose folder is not found SHALL read `<path> (not found)` in VALUE, with a tooltip saying to create the folder or remove the root. While the setup is being read, every root SHALL read as its path alone.

#### Scenario: A root that does not exist
- **WHEN** the repository roots are C:\Users\me\source\repos and C:\temp\source\repos, and only the first exists
- **THEN** the second root's VALUE reads C:\temp\source\repos (not found) and the first reads C:\Users\me\source\repos

#### Scenario: Its tooltip
- **WHEN** the user hovers over the root that is not found
- **THEN** the tooltip says the folder was not found and to create it or remove the root, and that Enter edits every root

#### Scenario: Before the setup is read
- **WHEN** the Config tab is shown and its setup is still being read
- **THEN** every root reads as its path alone
