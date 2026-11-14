# Spec Delta

## ADDED Requirements

### Requirement: Setup offers the bell in every account
With more than one account, `cader setup-terminal` SHALL offer the Claude bell in each account's `settings.json` in turn, naming the account and the file in each plan and asking once for each, with the same rules for each file as for a single account.

#### Scenario: One account already on
- **WHEN** work's settings already say terminal_bell and personal's have no preferredNotifChannel, and the user accepts
- **THEN** the output says work's bell is already on, personal's `settings.json` gains `"preferredNotifChannel": "terminal_bell"`, and work's file is not rewritten
