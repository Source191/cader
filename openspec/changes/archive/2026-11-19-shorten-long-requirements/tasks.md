# Tasks

**Implement with:** Claude Sonnet 5.5 (`claude-sonnet-5-5`) at medium effort.

## 1. Shorten the requirements

- [x] 1.1 `detail-pane`: split the worktree pane requirement from the branch and origin row one; verify `openspec validate detail-pane --strict` passes.
- [x] 1.2 `folder-actions`: split the folder label and branch rules from the heading rule, and the editor failure rule from the editor key; verify `openspec validate folder-actions --strict` passes.
- [x] 1.3 `session-attach`: split the tab's title and profile rules from opening it; verify `openspec validate session-attach --strict` passes.
- [x] 1.4 `settings`: split the fallback to defaults and the defaults themselves from the file; verify `openspec validate settings --strict` passes.
- [x] 1.5 `needs-input-notification`: split the bell, undo, settings-file and hidden-profile requirements; verify `openspec validate needs-input-notification --strict` passes.
- [x] 1.6 `worktree-management`: split removal's folder-in-use rule, the WORKTREE column's heading and LAST COMMIT rules, and the scan's rescan and ordering rules; verify `openspec validate worktree-management --strict` passes.

## 2. Check

- [x] 2.1 Re-read every hunk of `git diff` under `openspec\specs` and verify no rule was lost or changed; verify `openspec validate --all --strict` passes.

## Workflow follow-up

- Archive the change in the same pull request and verify that `openspec validate` passes.
