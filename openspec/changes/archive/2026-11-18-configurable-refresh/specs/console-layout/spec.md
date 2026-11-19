# Spec Delta

## MODIFIED Requirements

### Requirement: The key bar names the keys for the selected row
The last two rows of the screen SHALL be a key bar of items, each a key and a word: the first row the keys that act on the selected row, then New session and, when a release is known, Update, on the Sessions and Worktrees tabs; the second row the folder and view keys. The bar SHALL show only the keys that apply to the selected and listed rows, SHALL fit 120 columns for every kind of row, SHALL run a clicked item's key and show its tooltip on hover. The help SHALL list every key.

#### Scenario: A background session is selected
- **WHEN** a running background session is selected and no stopped session or empty terminal session is listed
- **THEN** the first row reads Enter Open, x Stop, d Delete, r Rename, m Memory and n New session, and the second row c Copy path, f Open folder, s Open solution, p Open PR, / Filter, ? Help, ! Report and q Quit

#### Scenario: A terminal session is selected
- **WHEN** a plain claude session that has never been sent a message is selected
- **THEN** the first row reads b Adopt, d Close tab and C Close empty before n New session

#### Scenario: A stopped session is listed
- **WHEN** some listed session is stopped or failed
- **THEN** R Resume is in the first row

#### Scenario: An update is known
- **WHEN** a newer release is known
- **THEN** U Update ends the first row, and otherwise the key is listed only in the help

#### Scenario: Click an item
- **WHEN** the user clicks Enter Open
- **THEN** the selected session is opened in its own tab, as pressing Enter does

#### Scenario: The bar fits
- **WHEN** the terminal is 120 columns wide and any kind of row is selected on any tab
- **THEN** both bar rows end within the terminal's width

#### Scenario: The help names the tab keys
- **WHEN** the user presses ? on any tab
- **THEN** the help lists ← and → as choosing a tab and < and > as scrolling the columns, and lists none of 1, 2, 3, w and ,

#### Scenario: The help names Enter as Open
- **WHEN** the user presses ? on the Sessions tab
- **THEN** the help lists Enter as opening the session in its own tab or switching to it, and lists no o key and no attach in this tab

#### Scenario: The help names the folder keys
- **WHEN** the user presses ? on the Sessions or Worktrees tab
- **THEN** the help lists f as opening the folder and s as opening its solution, and lists neither e nor v

#### Scenario: The help names F5
- **WHEN** the user presses ? on any tab
- **THEN** the help lists F5 as refreshing this tab now, and the key bar has no F5 item

## ADDED Requirements

### Requirement: F5 refreshes the shown tab
F5 on the console SHALL refresh the shown tab at once, with no spinner and no status message: the Sessions tab lists the sessions and the Worktrees tab rescans, neither while its own refresh is already running, and the Config tab rereads its setup as showing it does. Inside a dialog F5 SHALL keep the dialog's own meaning and refresh nothing.

#### Scenario: F5 on Config
- **WHEN** the user runs `cader setup-terminal` in another tab while the Config tab is shown, then presses F5
- **THEN** the Windows Terminal profiles row reads installed

#### Scenario: F5 in the wizard
- **WHEN** the new-session wizard is open with a name typed and the user presses F5
- **THEN** the session starts as before, and no tab is refreshed by the key
