# Proposal

## Why

The winget manifest for 0.21.0 names `Copyright (c) 2026 Simon Vane`, but the repository states no copyright holder: `LICENSE` is the stock Apache-2.0 text. The manifest and the repository should say the same thing, and Apache-2.0 expects a `NOTICE` file for exactly this.

## What Changes

- A `NOTICE` file at the repository root names the project, the copyright line and the licence, and points to `THIRD-PARTY-NOTICES.txt`.

## Capabilities

### New Capabilities

### Modified Capabilities

## Impact

- **Code:** none; `NOTICE` is not under the packaged tool paths and is not in the zip, so no `Version` bump or changelog line. **Docs:** one new file.

## Out of scope

- Changing `LICENSE`, `THIRD-PARTY-NOTICES.txt` or the README, whose "Built by" line already names the author.
- Multiple Claude accounts, which is a later change.
