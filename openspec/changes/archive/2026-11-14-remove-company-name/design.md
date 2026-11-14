# Design

## Context

The only hits for the company name are one test's inline data, one scenario in `openspec\specs\session-listing\spec.md` and the archived copies of that scenario in four earlier changes (session-memory, tabs-and-key-bar, rename-footer-to-status-message, harden-after-review).

## Decisions

- **Replace the argument with `northwind`:** the fixture name the project already uses. The label is the package name, so the behaviour and every expectation stay the same.
- **Edit archived files in place:** the change is a one-word fixture edit, not a decision, so the history keeps its meaning.
- **MODIFIED delta with the whole requirement block:** so the archive step rewrites the requirement in `openspec\specs` and nothing else.
