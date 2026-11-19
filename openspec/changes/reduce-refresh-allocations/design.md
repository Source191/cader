# Design

## Context

See proposal.md, Why. The measured figures are from a scratch program over `ProcessRegistry` (19 November 2026, runtime 11.0.0, 482 processes, Release, after three warm-up passes): `Snapshot` 71,928 B, `UsageOf` for every process 1,205,704 B, `ParentOf` for every process 517,200 B, `StartTimeOf` for every process 23,176 B. The paint, cell, listing and transcript figures have not been measured; each task measures its own before and after.

Constraints that shape the approach:

- **No behaviour change:** every existing fact stays green unchanged, and no port, Domain or Application type changes, so Core (held at C# 14 for Stryker) is not touched.
- **Threads:** the process registry and transcripts are read from pool threads, and refreshes can overlap; the Tui getters run on the UI thread, but Core.Tests runs test classes in parallel against the same static `Palette`.
- **Transcripts:** the project context allows reading only each line's top-level `type` until a user or assistant line, and the first `session-owner` line's `organizationUuid`; that stays exactly so.

## Goals / Non-Goals

**Goals:**

- Remove the allocations each refresh and redraw makes that the result does not keep.
- Hold each removal with a budget fact that fails on today's code (the red step) and passes after.

**Non-Goals:**

- Faster code for its own sake: CPU time may stay the same; only the garbage changes.
- Allocation facts for code a refresh does not run often (dialogs, set-up, start-up).

## Decisions

### 1. Measuring: `GC.GetAllocatedBytesForCurrentThread` around a synchronous call

`TestDoubles\Allocations.Of(Action)` runs the action once to warm it (static initialisers, tiered compilation, a pool's first rent), then answers the bytes the current thread allocated during a second run. Every path measured here is synchronous, so the count is the call's own and other tests running in parallel do not disturb it. Budgets are set from the measured figure after the change with room for machine differences, and always below today's figure, which each red step shows.

- Alternative: BenchmarkDotNet's memory diagnoser. Rejected: a new dependency and a separate run, where a fact in the normal suite catches a regression on every build.
- Alternative: `GC.GetTotalAllocatedBytes(precise: true)`. Rejected: process-wide, so parallel tests add to it.

### 2. Process names into a stack buffer

`NameOf` takes `Span<char> buffer = stackalloc char[1024]` (2 KB, the same length as today) and hands it to `QueryFullProcessImageNameW` through `[LibraryImport]`, which pins a span of a blittable type, then makes one string with `Path.GetFileNameWithoutExtension(ReadOnlySpan<char>)`. If the generator refuses a `Span<char>` parameter, the parameter becomes `ref char` given `ref MemoryMarshal.GetReference(buffer)`.

- Alternative: `ArrayPool<char>.Shared`. Works, but a rent and return per call where the stack costs nothing; the reads run on several pool threads at once, so one shared static buffer is ruled out.

`ExecutableName.ToString()`, whose only caller is `ReadEntries`, becomes `FileNameWithoutExtension()`, which takes the span up to the first zero and makes the one string the entry keeps. `ProcessParent`'s closure over the parent id stays: one small object per call, under the budget.

### 3. One scheme and one paint per colour

`Palette.Plain(Color)` and `Palette.SelectedRow(Color)` answer from a `ConcurrentDictionary<Color, Scheme>` each, filled through `GetOrAdd` with a static factory so a hit allocates nothing. Sharing is safe because Terminal.Gui 2.5.0's `Scheme` is a record with init-only roles (read from the assembly on 19 November 2026: `<Clone>$` present, `IsExternalInit` on the setters), so no view can change a scheme another holds. The set is bounded: the palette's constants and the tab shades of `TabPalette`. `Focusable`, `TabFrame`, `Highlight` and `Pair` are called once per dialog or view and stay as they are.

The Sessions TAB paint keeps one `CellPaint.Text(Palette.Grey)` and one `CellPaint.None` and a `ConcurrentDictionary<TabColour, CellPaint>` for the fills (`TabColour` is a record, so it compares by value); the Worktrees BRANCH paint keeps one paint per `BranchFlagKind`, built once over `Enum.GetValues`.

- Alternative: one dictionary inside `ColumnSizing`. Rejected: `Palette` is where colours live, and the other callers of `Plain` gain too.

### 4. Each cell once per source

`TableSource` keeps a `string?[]` of lines times columns and fills a cell the first time it is asked (the first column's indent included), so the width pass and every draw of that source share one string. It gains `Widest(int column)`, the longest cell of a column over the row lines (a heading's cells stay empty, as today), and `ColumnSizing.WidestCell` reads it instead of calling the column's cell itself. The elastic column's width is not known until the others are measured, so `ColumnSizing.Source` builds the source with a cell function that reads a local width it assigns once the others are measured; the elastic column's cells are asked for only after that, by the table.

- Alternative: build every cell eagerly in `Source`. Rejected: cells scrolled out of view or never drawn would be built for nothing.

### 5. Worktree listing over spans

`Parse` walks `porcelain.AsSpan().EnumerateLines()` (which takes `\n` and `\r\n` alike, so the `Replace` goes), trims each line, and closes a block at an empty line; a block that saw no line is never made, as `RemoveEmptyEntries` does today. The key is matched as a span in a `switch` on string constants, and only the values kept become strings: the path through `string.Create` and `MemoryExtensions.Replace(source, destination, '/', '\\')` (one string, not two), the commit, the branch and a lock's reason.

Trimming every line, where today only a block's first and last lines are trimmed, differs only for a line inside a block with spaces at its ends, which git's porcelain format never prints.

### 6. Transcripts as bytes

One reader walks a transcript's lines as UTF-8 bytes: `File.OpenHandle` with the same `FileShare.ReadWrite | FileShare.Delete`, `RandomAccess.Read` into a buffer rented from `ArrayPool<byte>.Shared` (16 KB, doubled while a line does not fit, returned at the end), each line ending at `\n` (a `\r` before it is JSON whitespace) and the last one at the end of the file, a UTF-8 byte order mark at the start skipped as `StreamReader` skips it today.

Each line is read with a `Utf8JsonReader` with the default options, the same as `JsonDocument.Parse` today: the root must be an object, a top-level property's value is skipped unless its name is the one asked for, and the line is read to its end, so a line still being written, or with anything after its object, fails as it does today and counts as nothing. With a name given twice the last one wins, as `JsonElement.TryGetProperty` does. `IsMessage` compares the value with `ValueTextEquals("user"u8)` and `"assistant"u8`, so a message line makes no string; the owner line keeps its byte search for `"session-owner"` before it is read, then makes one string of `organizationUuid` for `OrganisationId.TryParse`.

An `IOException` or `UnauthorizedAccessException` from the handle or a read is caught where it is today, so an unreadable transcript still has messages and no owner.

- Alternative: keep `StreamReader` and swap `JsonDocument` for `Utf8JsonReader` over `Encoding.UTF8.GetBytes(line)`. Rejected: the line string and its bytes would still be allocated, which is most of the cost of a long first line.
- Alternative: `System.IO.Pipelines`. Rejected: more machinery than one growing buffer needs.

## Risks / Trade-offs

- [A budget fact is flaky on another machine] → budgets keep room for different process names and command lines (the `Snapshot` budget is worked out from the entries it answers), and every measured path is synchronous on the test's own thread.
- [A cached scheme is changed by Terminal.Gui] → it cannot be: `Scheme`'s roles are init-only (decision 3); a fact pins that two calls answer the same instance.
- [A memoised cell goes stale] → a source is built per redraw from the state of that moment and is never reused for another state, so a cell is never asked of a later state.
- [The byte reader reads a line differently from `JsonDocument`] → the existing transcript facts (non-JSON lines, a line still being written, an owner after many messages, an unreadable file) stay unchanged, and new facts cover a long first line, a byte order mark, a line with `\r\n`, an escaped `type` and a property named twice.
- [A stack buffer overflows] → 2 KB on a pool thread's 1.5 MB stack, in a method that recurses nowhere.

## Migration Plan

None: no state file, setting or output changes. Rolling back is reverting the pull request.
