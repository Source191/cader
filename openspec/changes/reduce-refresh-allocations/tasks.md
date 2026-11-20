# Tasks

**Implement with:** Opus 5.5 at high effort: interop buffers, a byte-level JSON reader over files another process is writing, and the drawing path, where a mistake shows only on screen.

Each budget fact is red against today's code first (its message shows today's figure), then green; record both figures in the commit message.

## 1. Measuring

- [x] 1.1 `TestDoubles\Allocations.Of(Action)`: warm once, then answer the current thread's allocated bytes over a second run (design 1); its first user below is its test. Verify `dotnet build -c Debug`. Commit.

## 2. Process registry

- [x] 2.1 Red: `ProcessRegistryIntegrationShould.NameAParentWithoutABufferPerCall`: `ParentOf(Environment.ProcessId)` allocates under 512 bytes. Commit.
- [x] 2.2 Green: `NameOf` reads into `stackalloc char[1024]` through a span (or `ref char`) parameter and makes one string through the span overload of `GetFileNameWithoutExtension` (design 2); 2.1 and the existing registry facts pass. Commit.
- [x] 2.3 Red: `ProcessRegistryIntegrationShould.ListTheTableWithOneStringPerProcess`: `Snapshot` allocates no more than its entries, their names and the list need (worked out from the answer) plus 4 KB. Commit.
- [x] 2.4 Green: `ExecutableName.FileNameWithoutExtension()` replaces `ToString()` in `ReadEntries` (design 2); 2.3 and the existing registry facts pass. Commit.

## 3. Worktree listing

- [x] 3.1 Red: `GitWorktreeListingShould.ParseWithoutCopyingTheOutput`: a three-block listing (main, a locked worktree, a detached prunable one) parses within a budget set from what the kept strings and entries need. Commit.
- [x] 3.2 Green: `Parse` walks `EnumerateLines`, matches keys as spans and makes strings only of kept values, the path through `string.Create` (design 5); 3.1 and the twelve existing facts pass unchanged. Commit.
- [x] 3.3 `GitWorktreeListingShould.ReadWindowsLineEndings`: the same listing with `\r\n` parses to the same entries (held today by the `Replace`; it must stay green before and after). Commit.

## 4. Transcripts

- [x] 4.1 Red: `ClaudeTranscriptsShould.FindAMessageAfterALongLineWithoutDecodingIt`: a transcript whose first line is a 200 KB `summary` line and whose second is a `user` line has messages, and `HasMessages` allocates under 32 KB. Commit.
- [x] 4.2 Green: the byte reader and `Utf8JsonReader` for `HasMessages` (design 6); 4.1 and every existing transcript fact pass. Commit.
- [x] 4.3 Facts that hold today's reading and stay green: a byte order mark before the first line, `\r\n` endings, a `type` written with an escape (`"user"`), a line naming `type` twice (the last wins), and a `user` line with text after its object (counts as nothing). Commit.
- [x] 4.4 Red then green: `ClaudeTranscriptsShould.ReadTheOwnerWithoutDecodingMessages`: an owner line after a 200 KB message line is read within 32 KB; `ReadOwner` moves onto the same reader, keeping its byte search for `"session-owner"`. The existing owner facts pass. Commit.
- [x] 4.5 Refactor: remove `StreamReader`, `Lines` and `JsonDocument` from `ClaudeTranscripts` and check its summary still says what it reads. Verify `dotnet test test\Source191.Cader.Infrastructure.Tests -c Debug --no-build`. Commit.

## 5. Tables

- [x] 5.1 Red: `PaletteShould.HandOutOneSchemePerColour`: `Plain` and `SelectedRow` of one colour answer the same instance twice, and the second call allocates nothing. Commit.
- [x] 5.2 Green: `Palette`'s two caches (design 3); 5.1 and every Tui fact pass. Commit.
- [x] 5.3 Red then green: `SessionsScreenDefinitionShould` and `WorktreesScreenDefinitionShould`: the TAB paint of an open tab, of a plain claude and of none, and the BRANCH paint of each flag, answer the same instance for the same row twice and allocate nothing the second time; the paints are cached (design 3) and the existing paint facts pass. Commit.
- [x] 5.4 Red: `TableSourceShould.WorkEachCellOutOnce`: reading every cell three times, and `Widest` of each column, calls the cell function once per row cell and allocates nothing after the first read. Commit.
- [x] 5.5 Green: `TableSource` memoises its cells and answers `Widest` (design 4); 5.4 and the existing facts pass. Commit.
- [ ] 5.6 Refactor: `ColumnSizing` measures through `Widest` and builds its source before the elastic width is known (design 4). Verify the smoke check over a Release build (`dotnet run -- smoke <exe> <worktree-folder>` from `tools\HeadlessTerminal`) exits 0, so both tables still draw with their headings and widths. Commit.

## 6. Finish

- [ ] 6.1 `CLAUDE.md`'s disposal rule names the allocation-budget facts (`Allocations`) beside `HandleCounts`. Commit.
- [ ] 6.2 Re-run the scratch probe of design's Context against the new registry and put the before and after figures in the pull request. Raise `Version` to 0.22.0-preview.49 (one step above #355) with `.github\scripts\Step-Version.ps1` and add an Internal entry under Unreleased in `CHANGELOG.md`. Verify `dotnet build -c Debug`, `dotnet test -c Debug --no-build` at the solution and `dotnet format --verify-no-changes` pass. Commit.

## Workflow follow-up

- Archive the change in the same pull request (`/opsx:archive`) and verify that `openspec validate` passes.
- The proposal is opened first as a pull request against `c-scroll-facts` (stacked on #355); the implementation lands on the same branch, and its description then gains What changed, the measured figures and a **.NET references** list linking each API used to its Microsoft Learn page.
