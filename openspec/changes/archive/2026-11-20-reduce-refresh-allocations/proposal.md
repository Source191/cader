# Proposal

## Why

Every Sessions refresh (every 5 seconds by default) and every table redraw allocates far more memory than the work needs, so the collector runs more often than it should while the console sits open all day.

- **Process table:** `ProcessRegistry.NameOf` makes a new `char[1024]` (about 2 KB) for every process it names. Measured on 19 November 2026 over this machine's 482 processes: `UsageOf` for each allocated 1.21 MB and `ParentOf` for each 0.52 MB, about 80% of it that buffer. `Snapshot`, called twice per refresh, makes two strings per process (72 KB for the table) where one would do.
- **Tables:** every redraw builds a new `Scheme` for every cell and row (`Palette.Plain`, `Palette.SelectedRow`), although Terminal.Gui's `Scheme` is an immutable record; the TAB cell makes and parses a colour string per cell per draw, and the BRANCH and TAB cells build a new `CellPaint` each time; `ColumnSizing` works out every non-elastic cell to measure the columns and the table then works each cell out again on every draw.
- **Worktree listing:** `GitWorktreeListing.Parse` copies the whole output (`Replace`), splits it into blocks and each block into lines, then cuts each line into a key and a value, for every repository on every scan.
- **Transcripts:** `ClaudeTranscripts.HasMessages` runs for every session on every refresh and decodes each line it reads into a string and a `JsonDocument`, although it needs only the line's top-level `type`; a first line holding a long pasted message is decoded whole.

## Benefit

- **Users:** a console left open allocates a fraction of what it does today per refresh, so less garbage collection, steadier memory and fewer pauses on slow machines. Nothing they see or do changes.
- **Maintainers:** each piece is held by an allocation-budget fact beside the existing handle-count facts, so a change that brings the allocations back fails a test rather than going unnoticed.

## What Changes

- **Process names:** `NameOf` reads the image name into a stack buffer and makes one string of its file name through the span overload of `Path.GetFileNameWithoutExtension`; `Snapshot`'s entries make one string per process the same way.
- **Table paint:** `Palette.Plain` and `Palette.SelectedRow` hand out one scheme per colour; the Sessions TAB paint and the Worktrees BRANCH paint reuse one `CellPaint` per colour and the shared `None`.
- **Table cells:** `TableSource` works each cell out once per source, and `ColumnSizing` measures the columns from those same cells.
- **Worktree listing:** `GitWorktreeListing` walks the output's lines as spans and makes a string only for the values it keeps.
- **Transcripts:** `ClaudeTranscripts` reads a transcript's bytes through `RandomAccess` into a pooled buffer and reads each line with `Utf8JsonReader`, taking only the top-level `type` (and, on the first `session-owner` line, `organizationUuid`), as it does today.
- **Budget facts:** one allocation-budget test per piece, measured with `GC.GetAllocatedBytesForCurrentThread`.
- **Version and changelog:** a preview step and an Internal entry.

Out of scope:

- Runtime async (`<Features>runtime-async=on</Features>`) outside Core: deferred until Core can take it too (Stryker), as the .NET 11 upgrade recorded.
- Remembering that a transcript has messages once seen: it would change what a key sees for a transcript deleted later.
- Listing the project folders once per refresh instead of once per session, and reducing the command line `ProgramLabel` reads to a span (a Core signature change).
- C# 15 unions, `closed`, extension indexers and labelled jumps: none saves an allocation here.
- Multiple Claude accounts beyond naming a session's owner, which is a later change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None. No requirement changes; the change declares `skip_specs: true`.

## Impact

- **Implement with:** Opus 5.5 at high effort: interop buffers, a byte-level JSON reader over files another process is writing, and the drawing path, where a mistake shows only on screen.
- **`src`:** `Infrastructure\Processes\ProcessRegistry` and `ProcessRegistryNative`, `Infrastructure\Git\GitWorktreeListing`, `Infrastructure\Claude\ClaudeTranscripts`, `Tui\Palette`, `Tui\TableSource`, `Tui\ColumnSizing`, `Tui\SessionsScreenDefinition`, `Tui\WorktreesScreenDefinition`. No port, Domain or Application type changes.
- **Tests:** `ProcessRegistryIntegrationShould`, `GitWorktreeListingShould`, `ClaudeTranscriptsShould` (Infrastructure.Tests); `PaletteShould`, `TableSourceShould`, `SessionsScreenDefinitionShould`, `WorktreesScreenDefinitionShould` (Core.Tests); one `Allocations` helper in `TestDoubles`, used by both.
- **Docs:** `CLAUDE.md`'s disposal rule, which names the handle-count facts, names the allocation-budget facts beside them.
- **Version and changelog:** `Version` rises from 0.22.0-preview.48 to 0.22.0-preview.49; `CHANGELOG.md` gets an Internal entry.
- **Stacking:** stacked on #355 (`c-scroll-facts`); the table code it changes is the grouped tables of #348, now on `main`.
- **Users:** nothing visible.
