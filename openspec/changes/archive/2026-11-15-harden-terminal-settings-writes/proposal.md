# Proposal

## Why

`restore-hidden-terminal-profiles` (#275) made `cader setup-terminal` add the entry of a Cader profile Windows Terminal hides as deleted to Terminal's own `settings.json`. Its review found five gaps in that write, and the owner's rule is that it must never cause an engineer problems or damage their Windows Terminal set-up:

- Terminal rewrites `settings.json` itself (a save in its Settings page), and a save landing between Cader's read and its move is lost.
- No copy of the file is kept, so a mistake cannot be undone by hand.
- The new bytes are written without checking that they hold the profile list they should.
- A read-only file is reported as a failed write, with no way forward.
- `setup-terminal --remove` deletes the fragment but leaves the lines setup added in `settings.json`.

## What Changes

- **One guarded write** for every change to `settings.json`: refuse a link or a read-only file; read; build the new bytes; parse them with the same comment-tolerant reader and confirm the profile list holds every original element, in order, plus exactly the added entries (or minus exactly the removed ones); write a temporary file beside it; read the file again and write nothing when its bytes have changed; then `File.Replace` it, keeping a backup.
- **A backup beside the file:** `settings.json.<yyyy-MM-ddTHH-mm-ss>.cader.backup`, named after Terminal's own backups with the UTC time from `TimeProvider`, and named in setup's output.
- **Refusals that write nothing and say why:** the file changed meanwhile (run the command again), the new file did not check out, a link (as today) and a read-only file, the last two with the lines to add or take out by hand.
- **`--remove` takes out what setup added:** after the fragment, it shows the entries in `settings.json` that are exactly the four-property entry setup adds and, once the user agrees or with `--yes`, takes only those out through the same guarded write, keeping every other byte. An entry with any other property, which Terminal writes when someone customises the profile, is left alone.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `needs-input-notification`: setup's write to Terminal's settings file is guarded and backed up, refuses a read-only file, and `--remove` takes out the entries setup added.

## Impact

- **Code:** `Ports\ITerminalSettings` (both writes answer a `TerminalSettingsEdit`; a `RemoveProfileEntriesAsync`), `Ports\TerminalProfileRecords.Read` (the ids of entries exactly as setup adds them), `Ports\TerminalSettingsEdit` (new), `SettingsFileIsALinkException` (removed, now a case), `Adapters\Terminal\TerminalSettings` (the guarded write, a `TimeProvider`), `Application\TerminalSetup\TerminalSetupRunner`, `Wording`, `CommandLine`'s `--remove` description; wiring in `Composition\SharedParts` and `tools\ReadmeScene\SceneHost.cs`.
- **Tests:** byte-exact `TerminalSettingsShould` facts over a temporary folder (never Terminal's real files); runner facts over `FakeTerminalSettings`; `Wording` facts.
- **Docs:** `.claude\rules\security.md` (the `settings.json` bullet), `.claude\rules\windows.md` (the `File.Replace` facts), `docs\user-guide.md`, `CHANGELOG.md`.

## Out of scope

- Multiple Claude accounts (the `accounts-*` changes).
- Restoring a backup: the output names it, and copying it back is the user's.
- Pruning old backups: each is small and Terminal keeps its own the same way.
- Any change to the Config tab's Windows Terminal profiles row, which reads the same records and answers as before.
