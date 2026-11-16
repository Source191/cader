# Design

## Decisions

1. **Tests, plus one removal.** Each survivor but one is behaviour that is already right; the fix is a test that fails when the mutant is applied. The odd-key-list survivor guards dead code, so the code goes.
2. **Red means the mutant.** The code is already correct, so a new test passes at once. To show it fails for the right reason, apply the survivor's mutation by hand (the replacement in issue #315), run the test, see it fail, then revert. Stryker is not run locally (`checks.md` says when it is).
3. **Sociable tests through the public surface.** `WordingOfScreens.WorktreesHelp` and the other public members, `WordingOfDetail` through its public entry, `WorktreeInventory` through the real scan over `TestDoubles`' fake git, `CommitId.From` for the message. Assert the exact text a user sees.
4. **Odd key list.** `KeyColumns` is private and fed by three fixed even-length lists, so no sociable test can reach the lone-key case. It is removed: `Entry(pair[0]).PadRight(width) + string.Concat(pair.Skip(1).Select(Entry))` has no branch, gives the same text for pairs, and lays an odd last key out with trailing padding instead of throwing. The existing help tests hold the pair layout.
5. **CommitId message.** Assert it contains the text and "40 or 64 hex digits". It is a diagnostic message, never shown to a person (`CLAUDE.md`, user text), so asserting it is the lightest way to kill the mutant; a Stryker ignore is the fallback if that is judged noise.

## Risks

- **Brittle text assertions:** limited to the exact words the survivors changed.
- **Some survivors may be equivalent mutants** (for example `WordingOfDetail` line 74, `> 0` against `>= 0`, if both arms give the same text). Check each; an equivalent one is recorded in the pull request and ignored in place, not tested.
