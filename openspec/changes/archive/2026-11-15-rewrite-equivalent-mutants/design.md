# Design

## Approach

Each mark is taken in turn, in groups of the same pattern, test first: a red commit with the fact that the mutant fails (or, for a pure rewrite, a fact that pins the behaviour), then a green commit with the rewrite and without the mark. Every group is checked with a Stryker run scoped to its files (`--mutate` per file): the mutant the mark ignored must now be Killed, and the Ignored list must shrink by exactly the mutants meant. The whole change is then checked by a runner full run of `mutation.yml` on the branch.

## Patterns

- **A null fallback that never matters** (`text ?? string.Empty` before parsing): a `TryParse` answers false at once for null, so removing that line crashes on the null fact. Where a value can never be null, use the form that needs no fallback (the span overload of `Path.GetPathRoot`, a type pattern on a nullable namespace).
- **An early `return` the code below makes no difference to:** restructure as `if` / `else if`, or drop it when the code below already answers the same.
- **A branch for a state that never happens** (a tone that is never Failure, a key with no hint, an empty sentence): drop the branch; the fact that holds the state already exists or is added.
- **A guard the next line repeats:** keep the guard (range checks stay) and assert the exception's `ParamName`, which the slice's own exception does not carry. Where the callee already owns the same guard with the same parameter name, the caller's copy is the belt and braces and goes.
- **`First` after `DefaultIfEmpty`:** `FirstOrDefault(fallback)`.
- **A value nobody reads:** remove the parameter or the flag when the code shows it is dead; when a Domain value feeds the removal rules, rewrite only if every removal fact still passes and the mutant now fails one.

## Decisions

- **Security-relevant code** (`WorktreeRemover`, `DeleteSessionHandler`, anything that ends a process or deletes a folder) is rewritten only when the behaviour is provably unchanged, with a fact that would fail on the old mutant; otherwise the mark stays with its reason.
- **New mutants count:** a rewrite that brings a new surviving mutant is not done until a fact kills it.
- **True equivalents stay marked:** for example `>>` against `>>>` on a value that is never negative (`TabShade`). Each kept mark is listed in the pull request with why.
