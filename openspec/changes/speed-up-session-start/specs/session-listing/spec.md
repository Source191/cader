# Spec Delta

## MODIFIED Requirements

### Requirement: The list refreshes when Claude Code records a change
Whichever tab is shown, Cader SHALL list the sessions within about two seconds of Claude Code writing, adding or removing a session's record in the `sessions` folder or a job's `state.json` in the `jobs` folder of the Claude folder Cader uses. Changes in a burst SHALL start at most one listing a second, never two at once, and a change seen during a listing SHALL start one more after it. Cader SHALL NOT open or read those files. While a session started from the wizard is being launched, a change SHALL be kept rather than start a listing, and the sessions SHALL be listed after the launch ends.

#### Scenario: A session starts waiting while another tab is shown
- **WHEN** Sessions refresh is 0, the Worktrees tab is shown, and a background session stops to ask for permission
- **THEN** the sessions are listed within about two seconds and Cader's bell rings for the waiting row

#### Scenario: A session started in another terminal
- **WHEN** the user starts `claude` in another terminal while the Sessions tab is shown
- **THEN** the session's row appears within about two seconds, not at the next timed refresh

#### Scenario: A busy session
- **WHEN** a background session rewrites its `state.json` every 200 milliseconds for a minute
- **THEN** no more than one `claude agents` process runs at a time, no more than one starts in any second, and the last change is followed by a listing

#### Scenario: No Claude folder yet
- **WHEN** the Claude folder has no `sessions` or `jobs` folder
- **THEN** the console opens as before, refreshes the list on its timer, and shows no error

#### Scenario: Temporary and key files
- **WHEN** Claude Code writes `state.json.tmp.4f0d6ab5` and then renames it to `state.json`
- **THEN** one listing follows, started by the rename

#### Scenario: A change during a launch
- **WHEN** a session started from the wizard writes its record while its launch is still running
- **THEN** no listing starts until the launch ends, and a listing starts after it

