# Design

## Context

`TerminalSettings.AddProfileEntriesAsync` (#275) reads Terminal's `settings.json`, inserts the entries as text after the profile list's last element, writes the result beside the file and moves it over with `File.Move(..., overwrite: true)`. It refuses a link with `SettingsFileIsALinkException`; every other problem is an `IOException` or `UnauthorizedAccessException` that `TerminalSetupRunner` reports with exit 1. `--remove` deletes only the fragment folder.

## Decisions

### 1. The write answers a result, not an exception

Both writes answer a `Ports\TerminalSettingsEdit`: `Written(Backup)`, `Unchanged` (removal found nothing to take out), `IsALink`, `ReadOnly`, `ChangedMeanwhile` and `NotVerified`. Each refusal writes nothing. `SettingsFileIsALinkException` becomes the `IsALink` case. `IOException` and `UnauthorizedAccessException` stay for real input and output failures (a file in use, a denied ACL, a file with no readable profile list), which the runner reports as before. The runner has one switch over the result; the words are `Wording`'s.

### 2. One guarded write, shared by add and remove

`TerminalSettings` keeps one private path for both changes:

1. Refuse a link or other reparse point (`IsALink`), then a file with the read-only attribute (`ReadOnly`), before reading anything.
2. Read the bytes.
3. Build the new bytes (insertion or removal, decision 4); a removal that finds nothing answers `Unchanged`.
4. Verify (decision 3); a failure answers `NotVerified`.
5. Take the backup's name from `TimeProvider`, then write the new bytes to `settings.json.cader-tmp` beside the file.
6. Read the file again; if its bytes differ from step 2's, delete the temporary file and answer `ChangedMeanwhile`.
7. `File.Replace(temporary, settings, backup)` and answer `Written(backup)`.

**The window that remains:** a save by Terminal between step 6's read and step 7's replace is still lost. That is the time to compare two buffers of a few kilobytes and make one system call, against the time to verify, write and flush the whole file that #275 left open. Closing it fully would need Terminal to lock the file, which it does not.

**Why `File.Replace`:** `ReplaceFile` keeps the original's creation time, DACL, short name, object identifier and named streams ([ReplaceFileW](https://learn.microsoft.com/windows/win32/api/winbase/nf-winbase-replacefilew)), so a file the user secured keeps its permissions, and it renames the original to the backup in the same call, so no copy is read and written twice. A fact checks the creation time, an added access rule and the backup's bytes on this machine (`windows.md`). It overwrites an existing backup of the same name, which only two runs in the same second could meet.

**A residual failure:** `ReplaceFile`'s `ERROR_UNABLE_TO_MOVE_REPLACEMENT_2` (1177) leaves the original under the backup name and no `settings.json`. .NET reports it as an `IOException`, which the runner prints with the file's path. It needs the temporary file to be unmovable after the original was renamed, which nothing in this flow does; no recovery code is added for a case no test can produce.

**The race fact:** the adapter test stands in for Terminal with a `TimeProvider` whose clock, when read, rewrites the file. The clock is read after step 2 and before step 6, so the fact also pins that order.

### 3. Verify the new bytes before writing them

The new bytes are parsed with the same comment-tolerant `JsonDocument` options and the profile list found as `ReadAsync` finds it. The list must hold exactly the expected elements, compared with `JsonElement.DeepEquals`: every original element in order, then the added entries (add), or every original element but the removed ones (remove). A file that names `profiles` twice is the real case today: the text insertion goes into the first list, the parser reads the last (`TryGetProperty` answers the last duplicate, checked 15 November 2026), so the check fails and nothing is written.

### 4. What `--remove` takes out, and how

An element is taken out only when it is exactly the entry setup adds: an object of exactly four properties, `guid` naming a Cader profile (compared as a `TerminalProfileId`, so letter case does not matter), `"hidden": false`, and `name` and `source` with Cader's values. Terminal adds properties to an entry when someone customises the profile, and that entry is theirs, so anything else is left alone. `ReadAsync` reports these ids as `Read.Added`, so the runner can show them and ask; the adapter decides again from the bytes it reads when it writes.

Each element is removed by text, one at a time, the file read again after each:

- The element itself goes, with the spaces and tabs before it and, when it starts its line, the line break before that.
- One comma goes with it: the one before it when it is not the first element, otherwise the one after it, if any (a trailing comma after an only element included). The comma is found by stepping over white space and comments, so a comma inside a comment is never taken.

Every other byte stays, comments, other commas and the byte order mark included. Removing what setup added gives back the original bytes for each shape setup adds to (after the last element, an empty list, a one-line list, a trailing comma, CRLF with a byte order mark); a fact holds that.

### 5. Setup's flow

- **Adding:** unchanged until the write; then `Written` names the backup after the existing sentence, and each refusal goes to the error output with exit 1. A link or read-only file gives the entries to add by hand.
- **Removing:** read Terminal's records first, remove the fragment as today (a failure there still exits 1 at once), then, when `Read.Added` names any entry, show them, ask (`--yes` answers), and take them out through the guarded write. A link or read-only file gives the entries to take out by hand. "Nothing to remove" is said only when there was neither a fragment nor an entry; a declined question with no fragment says nothing was written. Unreadable Terminal files are reported and left alone, as setup does.

## Risks

- **A Terminal that reformats entries:** an entry Terminal has rewritten with the same four properties is still taken out; one with different letter case in a name is left, which is the safe side.
- **Backups accumulate:** one small file per confirmed write, beside Terminal's own backups.
