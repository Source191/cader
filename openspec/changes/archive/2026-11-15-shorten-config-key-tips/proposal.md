# Proposal

## Why

Every key's tooltip on the Sessions and Worktrees tabs is one sentence of at most 80 characters, so it reads at a glance and fits beside the pointer. Two of the Config tab's key tips run well past that (Enter's is 113 characters and `f`'s 119) because the rule was written when the console had two tabs and the fact that holds it never read the Config keys. Nothing caught them, and nothing would catch the next long one.

## What Changes

- **Enter's tip** becomes "Edit the setting (all roots, one per line), or say how to change the setup." (75 characters).
- **`f`'s tip** becomes "Open the folder, or the settings or log file, in the editor chosen above." (73 characters). It no longer spells out that on a row naming another file `f` opens the folder holding it; the help line for `f` and the row's own description still say what the row opens.
- **One rule for every tab:** the requirement that each key's tip is a sentence of at most 80 characters names the Config tab beside the other two, and the fact that holds it reads all three tabs' keys, replacing the Config-only fact that checked a tip without its length.

## Out of scope

- **Every other wording:** the other Config tips, the help lines, the row descriptions and the key bar's words stay as they are.
- **What the keys do:** Enter and `f` behave exactly as before.
- **Multiple Claude accounts**, which are later changes.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `session-listing`: the requirement "Key hints explain themselves on hover" covers every tab's keys, the Config tab included, each tip at most 80 characters.

## Impact

- **Code:** the two tips in `src\Source191.Cader.Core\Application\WordingOfScreens.cs`; `WordingShould`'s tip facts.
- **Docs:** the changelog (Changed); the user guide quotes neither tip.
- **Version:** a preview raise, because files under `src\` change.
