# Tasks

## 1. Record the first submission

- [x] 1.1 Add what the first submission taught to `MAINTAINING.md`, under the first-submission step. Verify a read-through against the pull request's manifests.
- [x] 1.2 Add the verified install facts to `.claude\rules\windows.md`, dated with the winget and wingetcreate versions. Verify each statement was seen on 13 November 2026.
- [x] 1.3 Tick `install-with-winget` task 5.2: `gh attestation verify` on the v0.21.0 zip passes for pipeline.yml from main. Verify by running it.

## 2. Close the change

- [x] 2.1 Archive the change in this pull request with `openspec archive record-first-winget-submission --skip-specs`. Verify `openspec validate --strict` passes.
