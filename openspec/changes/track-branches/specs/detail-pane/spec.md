# Spec Delta

## MODIFIED Requirements

### Requirement: The pane shows the selected worktree
On the Worktrees tab the pane SHALL show, for the selected row and titled with its folder's name: the health with its reason in full, the branch and its merge state against origin's default branch, or the branch's flag's reason in place of the merge state when the branch is flagged, the pull request, the full folder, whether and when Cader created it, its changed files and unpushed commits, and beneath them its unpushed commits as far as five, or its changed files when none are unpushed, then how many more. For a branch row or an origin row the pane SHALL be titled with the branch, SHALL read no git, and SHALL show the health with its reason, the branch with its flag's reason, the pull request, the repository's folder in place of the folder and Created, and the branch's unpushed commits (or that an origin row has no local branch), with nothing listed beneath.

#### Scenario: A worktree with unpushed commits
- **WHEN** Northwind.Billing-invoice-pdf, created by Cader on 23 October, with twelve unpushed commits on invoice-pdf, is selected
- **THEN** the pane reads Health ok, Branch invoice-pdf, not merged into origin/main, Created by Cader, 23 Oct 09:14, Changes 0 changed files · 12 unpushed commits, lists the five newest commits with hash, age and subject, and ends with 7 more

#### Scenario: A worktree with changes and nothing unpushed
- **WHEN** a worktree with two changed files and no unpushed commits is selected
- **THEN** the pane lists the two changed files in place of commits

#### Scenario: A broken leftover
- **WHEN** a broken leftover folder is selected
- **THEN** the pane's Health line gives the reason in full, and the git lines read unavailable

#### Scenario: A locked worktree
- **WHEN** a worktree git reports as locked with a reason is selected
- **THEN** the pane's Health line reads locked and the reason

#### Scenario: A flagged worktree
- **WHEN** the worktree Northwind.Orders-fix-123, whose branch's upstream is gone, is selected
- **THEN** the pane reads Branch fix-123 · origin no longer has it, as of the last fetch

#### Scenario: A branch row
- **WHEN** the branch row spike-refunds of Northwind.Orders, with two commits on no remote branch and no pull request, is selected
- **THEN** the pane is titled spike-refunds and reads Branch spike-refunds · no pull request on its host, Repository C:\Users\me\source\repos\Northwind.Orders and Changes 2 unpushed commits, and lists no commits or changed files of the main checkout
