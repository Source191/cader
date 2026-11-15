# Proposal

## Why

`close-mutation-gaps` left 42 `// Stryker disable once ... : Equivalent:` marks in Core, ignoring 70 mutants. A mark is honest only when no test can tell the mutant apart, but most of these point at code that is belt and braces: a null fallback that is never read, an early `return` the code below already covers, a branch for a state that never happens, a flag nobody reads, `First` after `DefaultIfEmpty`. Each mark hides a mutant from every future run, so a later change to that line is untested, and each one asks the next reader to trust a written argument instead of a fact.

## What Changes

- **Rewrite the code a mark points at** so the mutant is gone or a fact kills it: return failure at once for a null input, restructure early returns as `if` / `else if`, drop branches and parameters that are never read (including `cader here`'s repository check), take `FirstOrDefault(fallback)` instead of `DefaultIfEmpty().First()`, and merge duplicated loops and wrapping code.
- **Add the fact that kills a mutant** where the code is right as it is, such as the parameter name a range guard reports.
- **Keep a mark only for a true equivalent**, saying in the mark why no rewrite or test reaches it.
- **Record the rule** in `.claude\rules\checks.md` and `CONTRIBUTING.md`: before marking a mutant Equivalent, try a rewrite or a test; a mark is for a true equivalent only. The ledger gets the new Equivalent count from a runner full run.

## Out of scope

- **The `Accepted:` marks:** the 61 static-initialiser mutants are Stryker's own limit (stryker-mutator/stryker-net#3742) and the line-0 frame of `ExceptionSummary` stays.
- **Projects other than Core**, which Stryker does not mutate.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None: nothing a user can see changes, so `.openspec.yaml` declares `skip_specs: true`.

## Impact

- **Code:** the Core files that carry an Equivalent mark, their facts, and the callers of any parameter removed (`HereCommandRunner`'s composition).
- **Docs:** `.claude\rules\checks.md` (Mutation testing) and `CONTRIBUTING.md` (the mark convention).
- **Version:** a preview raise with an Internal changelog line, because files under `src\` change.
