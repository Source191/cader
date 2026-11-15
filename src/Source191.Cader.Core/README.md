# Source191.Cader.Core

`Domain`, `Ports` and `Application`: the rules, the boundary interfaces, the reducers and the use cases. No file system, no processes and no Terminal.Gui; the architecture test in `Core.Tests` enforces the layering.

One package: System.CommandLine, for the command line in `Application\CommandLine`. Microsoft.Extensions.Logging.Abstractions, used only by `Application\Diagnostics\TerminalGuiBridge` (the `ILogger` Terminal.Gui writes to), comes with the .NET 11 shared framework, so it needs no package reference (a reference to it fails the build with NU1510).

- **`LangVersion` 14:** Stryker.NET 5.1.0, the newest on 14 November 2026, compiles the mutated Core with its own Roslyn 5.9.0, which refuses C# 15 syntax (CS8652), so one `closed`, `union`, `with(...)` or labelled `break` here would stop every `mutation.yml` run. Holding the project at C# 14 makes such syntax a build error instead (CS9327 for `closed`), and keeps the C# 15 style rules from asking for it here. Raise it to 15 once a Stryker release compiles C# 15 (`.claude\rules\checks.md`); the rest of the solution is already on C# 15.
- **No `InvariantGlobalization`:** only an executable's `runtimeconfig.json` carries the setting at run time, so on a library it would do nothing but switch off the culture analysers (CA1304, CA1305, CA1309, CA1310, CA1311), which then check this project's code. The tool project, `src\Source191.Cader`, keeps it, so the shipped tool still runs invariant.
