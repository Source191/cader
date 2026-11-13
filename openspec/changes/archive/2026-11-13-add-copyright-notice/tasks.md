# Tasks

## 1. Add the notice

- [x] 1.1 Add `NOTICE` with the project name, `Copyright (c) 2026 Simon Vane`, the Apache-2.0 reference and a pointer to `THIRD-PARTY-NOTICES.txt`. Verify the copyright line matches the winget manifest's `Copyright`.
- [x] 1.2 Archive the change in this pull request with `openspec archive add-copyright-notice --skip-specs`. Verify `openspec validate --strict` passes.
