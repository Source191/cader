# Tasks

## 1. Replace the fixture

- [x] 1.1 **Red:** change the `ProgramLabelShould` inline data to use `northwind`; the label stays `@azure-devops/mcp`. It passes before and after, because the label ignores the last argument, so it is a rename and not a behaviour change. **Green:** nothing to build. **Verify:** `ProgramLabelShould` passes.
- [x] 1.2 Edit the four archived changes' copies of the sentence in place.

## 2. Specification

- [x] 2.1 Archive this change so `openspec\specs\session-listing\spec.md` takes the modified requirement.
- [x] 2.2 **Verify:** build, the Core tests, `dotnet format --verify-no-changes`, `openspec validate --all`, and a case-insensitive search for the company name finds nothing.
