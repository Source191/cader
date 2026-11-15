# Spec Delta

## MODIFIED Requirements

### Requirement: Key hints explain themselves on hover
Each item of the key bar SHALL show a tooltip while the mouse pointer rests on it, saying in one sentence what the key does and when to use it, drawn in its own colours apart from the bar's text.

#### Scenario: Hover over a key hint
- **WHEN** the mouse pointer rests on x Stop in the key bar of the Sessions tab
- **THEN** a tooltip in colours distinct from the bar says that x stops the session, which stays listed as stopped and can be resumed

#### Scenario: Every key has a tip
- **WHEN** the help lists a key of the Sessions, Worktrees or Config tab
- **THEN** that key has a tooltip sentence of at most 80 characters

#### Scenario: Config tab's longest tips fit
- **WHEN** the mouse pointer rests on Enter Edit or f Open folder in the Config tab's key bar
- **THEN** the tooltip is one sentence of at most 80 characters saying what the key does on a setting, the roots or a setup row
