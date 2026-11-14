# Proposal

## Why

Cader is a personal project and its repository should not name an employer. The company name appears only as the last word of a made-up command line (`cmd /c npx -y @azure-devops/mcp <organisation>`) in one test and in the specification scenarios built from it. The project's own convention is that fixtures use made-up Northwind names.

## What Changes

- The organisation argument in that made-up command line becomes `northwind`, in the `ProgramLabelShould` inline data (the expected label stays `@azure-devops/mcp`), in the `session-listing` scenario "A session with MCP servers", and in the archived changes that copied the same sentence.
- The same requirement in `openspec\specs\session-listing` is brought up to date through a MODIFIED delta; its text is otherwise unchanged.

Out of scope: git history and the text of earlier pull requests, which still contain the name and cannot be changed from the repository.
