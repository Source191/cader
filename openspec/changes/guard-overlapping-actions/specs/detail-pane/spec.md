## ADDED Requirements

### Requirement: A tab reads one row's detail at a time
A tab SHALL read at most one row's detail at a time; a selection move while a read runs SHALL wait for it, and SHALL then read only the row selected by then. A read SHALL NOT be stopped part-way for the selection moving on.

#### Scenario: Holding Down
- **WHEN** the user holds Down across twenty worktrees while the first one's detail is being read
- **THEN** no more than one row's detail is read at once on that tab, and once the key is let go the pane shows the row it rests on

#### Scenario: Passed rows are never read
- **WHEN** the user moves from fix-123 past fix-456 to fix-789 while fix-123's detail is being read
- **THEN** fix-456's detail is never read and fix-789's is read next
