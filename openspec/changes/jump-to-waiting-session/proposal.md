## Why

When Cader's bell rings for a session that needs input, finding it takes several key presses: the Sessions rows are grouped by repository and ordered by attention only within each group, so a waiting session can sit under the third or fourth heading, and with many sessions the user scrolls or filters to reach it before pressing Enter. The commonest thing to do after the bell should be two keys.

**Implement with:** Sonnet 5.5 at medium effort: one reducer-only key that moves the selection, following the documented four-piece recipe for a new key (here with no handler, as it raises no effect), with `KeyHelpParityShould`, `StatusTonesShould` and the `Wording` facts failing for any missing piece.

## What Changes

- `w` on the Sessions tab moves the selection to the next session that needs input after the selected row, in table order, wrapping to the top; Enter then opens it or switches to its tab as today.
- With no session needing input, `w` leaves the selection where it is and the status line says no session needs input.
- `w` follows the filter: only the rows shown are stepped through.
- The key bar's second row, the help and the tooltips list it.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `needs-input-notification`: a key moves to the next session that needs input.

## Delivery

- **Order:** step 7 of the stack in #379. Depends on nothing; `fit-narrow-and-short-terminals` (step 2) fits its new key into a narrow key bar.
- **Together:** ships alone, in one pull request.

## Out of scope

- Opening the session directly with one key (Enter after `w` keeps one key per action, and opening a tab from a bell would surprise).
- A key to repeat the last launch with the same repository and options (considered; the wizard already remembers recent repositories, and how often a launch repeats the last should be known first).
- Multiple Claude accounts beyond naming a session's owner, a later change.

## Impact

- `Application\Views\SessionsView`: the `w` case and its test; a `StatusMessage` case for no session needing input, with its words in `Wording`, its tone in `Wording.ToneOf` and a line in `StatusTonesShould`.
- `Application\WordingOfScreens.SessionsKeys` and the second bar row; `docs\user-guide.md` and the README's key list.
- `CHANGELOG.md` and `Version` in `src\Source191.Cader\Source191.Cader.csproj`.
- **Implement with:** Sonnet 5.5 at medium effort (reason above).
