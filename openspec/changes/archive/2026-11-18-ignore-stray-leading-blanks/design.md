# Design

## Context

- The wizard's list comes from `RepositoryChoices.Matching(filter)`, which keeps each `RepositoryChoice` whose `Name.Contains(filter, OrdinalIgnoreCase)`, so a filter of `" "` keeps nothing.
- The tables' `/` filter is `Domain\RowFilter.Matches`: a filter that `IsNullOrWhiteSpace` keeps every row, any other is `Contains` against each field, so `" orders"` keeps only fields with a space before "orders".
- `WizardView.FilterProblem` raises `WizardProblem.InvisibleFilter` when `CharacterKinds.AllInvisible(text)`; `Wording.Describe` words it, the smoke script types a space and waits for those words, and `AllInvisible` has no other caller.
- Every typed session name passes `SessionName.Validate`: the wizard (both kinds) and `cader here` through `LaunchPlanBuilder.Build`, and rename through `SessionsView.RenameWith`, which already calls `.Trim()` first. `WorktreeSlug` already drops leading and trailing hyphens, so the wizard's preview of a worktree folder is unaffected by a leading space.
- `WizardFilterWatch` records the first filter change from the raw text through `CharacterKinds.First`; it stays as it is.

## Goals / Non-Goals

**Goals:**
- One rule for "which leading characters are ignored", shared by both filters, built on the existing `CharacterKinds.Of`.
- One place that trims a typed name, so the wizard, `cader here` and rename cannot disagree.

**Non-Goals:**
- Changing what the filter text box holds, or how keys reach it.
- Trimming a name read from Claude Code's listing (`SessionName.TryParse` and `From` are unchanged).

## Decisions

### Ignore leading invisible characters when matching, not when typing
`Domain\CharacterKinds.WithoutInvisibleStart(string)` answers the text from its first rune whose kind is neither `Whitespace`, `Control` nor `Format` (empty when there is none). `RepositoryChoices.Matching` applies it once and hands the result to each `RepositoryChoice.Matches`; `RowFilter.Matches` applies it and keeps every row when the result is empty, which replaces its `IsNullOrWhiteSpace` check. Trailing characters are left alone, because `"my "` is a deliberate word boundary.
Rejected: cancelling the filter's `TextChanging` for a leading blank, so it never appears. It needs the Tui binder to act on a reducer verdict and only hides a one-cell cursor offset; matching is a pure Core change under unit tests. Rejected: `string.TrimStart()`, which leaves a zero-width space (U+200B, a format character) in front and would bring back the empty list the removed problem line covered.

### Remove the invisible-filter problem line
With the rule above, a filter that `AllInvisible` held now lists every repository, so `WizardProblem.InvisibleFilter`, its arm in `WizardView.FilterProblem` (the method goes; `Filtered` stops touching `Problem`), its words in `Wording.Describe`, and `CharacterKinds.AllInvisible` become dead and are deleted with their tests. `WizardProblem` is an open hierarchy with a fallback arm in `Wording`, so removing a case is safe for the reflection facts.

### Trim typed names inside `SessionName.Validate`
`Validate(text)` checks and builds from `text.Trim()`; `SessionsView.RenameWith` stops trimming on its own. `Trim()` drops whitespace at both ends, matching rename's existing behaviour; a trailing blank in a name is as accidental as a leading one. `From` and `TryParse` are unchanged, so a listed name is never altered. Rejected: trimming in `WizardView` and `HereCommand` separately, which would leave three trims to keep in step.

### The smoke check stops typing a space
The smoke step that typed a space into the wizard's filter and waited for the problem line's words loses its marker. The wizard step sends Esc straight after the Before branching check instead; the behaviour is covered by Core tests, and the smoke check has no repository list to assert against.

## Risks / Trade-offs

- [A repository folder whose name starts with a space can no longer be singled out by typing that space] → Such a name is all but impossible to make on Windows by hand, and typing its visible part still finds it.
- [A filter of only a zero-width space shows everything while the box looks empty] → That is what an empty filter shows, so the screen reads truthfully; Backspace removes it.
- [The diagnostic still records a leading space as the first change] → Intended: the log keeps showing how often the slip happens.

## Migration Plan

None: no state file or setting changes. Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` as CONTRIBUTING's Versions says for a fix, and add a `## Unreleased` changelog line under Fixed citing #339.

## Implementation

Implement this change with **Claude Sonnet 5.5 (`claude-sonnet-5-5`) at medium effort**. It is a small, fully specified Core change (one Domain rule, three call sites, one deletion) with no adapter, Terminal.Gui or Windows behaviour to discover, so the deeper reasoning of Opus or high effort buys nothing; medium effort leaves room for the repository's conventions (TDD commits, `Wording` facts, the reflection tests, the smoke script and the archive in the same pull request).
