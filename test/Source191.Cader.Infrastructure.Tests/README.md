# Source191.Cader.Infrastructure.Tests

The adapter suite: real git, real processes, real files and the console, on throwaway folders.

`Contract\Snapshots` holds the Claude Code help snapshots that `ClaudeHelpSurfaceShould` compares against; the contract tests and how to accept a new snapshot are in `.claude\rules\checks.md`.

- **`InvariantGlobalization`:** the tests run in the invariant globalisation mode the shipped tool runs in, so a call that behaves differently there (such as `CompareOptions.NumericOrdering`, which invariant mode ignores) fails here rather than only for users. It also switches off the culture analysers for this project's own code; the libraries under test keep them.
- **`AllowUnsafeBlocks`:** the `[LibraryImport]` source generator behind `StandardHandles` (which points this process's standard handles at a file, so `DesktopShould` can see what a started program would have written to the console) refuses to run without it; the project writes no `unsafe` code of its own.
- **`Microsoft.CodeAnalysis.CSharp`:** `OneTypePerFileShould` parses every C# file under `src`, `test` and `tools` with Roslyn's syntax API to check that each holds one top-level type named after it, and `SourceConventionsShould` reads every file under `src` the same way (through `SourceFiles`) for `!`, `volatile`, `catch (ArgumentException` and American spellings in `Wording`; a syntax tree ignores nested types, comments and string literals, which a text match would not. Only this project references it, because only this suite reads the repository's own files.
