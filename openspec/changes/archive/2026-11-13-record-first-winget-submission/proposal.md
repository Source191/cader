# Proposal

## Why

0.21.0 is the first Cader release submitted to winget (microsoft/winget-pkgs#479528, 13 November 2026). The submission found things `MAINTAINING.md` did not say, and `install-with-winget` still lists a task as open that is done. Written down now, the next submission does not repeat the detour.

## What Changes

- `MAINTAINING.md`: what the first submission taught, as a short list under the first-submission step (prompts that only go forward, `ArchiveBinariesDependOnPath`, tags, dependencies, icons, sign-in, the contributor licence, the checks).
- `.claude\rules\windows.md`: the facts verified by installing the first manifest locally, dated with the winget and wingetcreate versions.
- `install-with-winget`: task 5.2 is ticked, because the v0.21.0 Release carries the zip and `gh attestation verify` passes on it.

## Capabilities

### New Capabilities

### Modified Capabilities

## Impact

- **Code:** none. **Docs:** `MAINTAINING.md`, `.claude\rules\windows.md`, and one task line of the open `install-with-winget` change. No `Version` bump or changelog line, because nothing under the packaged tool paths changes.

## Out of scope

- The README install section and the copyright line, which wait for the package to merge (a separate pull request).
- The other open `install-with-winget` tasks (4.4, 5.3, 7.1, 8.1 to 8.3), which need the owner's checks.
- Multiple Claude accounts, which is a later change.
