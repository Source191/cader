# Design

## Context

Each tab's keys are a `KeyHelp` list in `Wording` (`SessionsKeys`, `WorktreesKeys`, `ConfigKeys`); the key bar's tooltips and the help are generated from them. `WordingShould.GiveEveryKeyATip` holds the 80-character rule for the Sessions and Worktrees lists only, and `GiveEveryConfigKeyATipOfItsOwn` (added by skip-static-mutants-in-release-runs) holds that each Config tip exists and is not just its help line, deliberately leaving the length out until this change.

## Goals / Non-Goals

**Goals:**

- One fact, over all three lists, holds every rule a tip keeps.

**Non-Goals:**

- Rewording any other text.

## Decisions

- **Fold the Config keys into `GiveEveryKeyATip` and delete `GiveEveryConfigKeyATipOfItsOwn`:** the two facts would check the same thing for different lists; one fact over every list means a fourth tab is one more list to concatenate, not another fact to remember.
- **Keep "all roots, one per line" in Enter's tip:** it is the one thing about Enter on a root row a person cannot guess, and it fits in 75 characters. The setup-row case shrinks to "say how to change the setup", which is what Enter does there.
- **Drop the "folder holding another file" case from `f`'s tip:** it is the least common of the three, the help line still says "open the folder, or the settings or log file", and keeping it would not fit in 80 characters.

## Risks / Trade-offs

- [The shorter `f` tip no longer says what happens on a row naming some other file] → the row opens its folder, which is what "Open the folder" says.
