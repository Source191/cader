# Source191.Cader.Core

`Domain`, `Ports` and `Application`: the rules, the boundary interfaces, the reducers and the use cases. No file system, no processes and no Terminal.Gui; the architecture test in `Core.Tests` enforces the layering.

One package: System.CommandLine, for the command line in `Application\CommandLine`. Microsoft.Extensions.Logging.Abstractions, used only by `Application\Diagnostics\TerminalGuiBridge` (the `ILogger` Terminal.Gui writes to), comes with the .NET 11 shared framework, so it needs no package reference (a reference to it fails the build with NU1510).

- **No `InvariantGlobalization`:** only an executable's `runtimeconfig.json` carries the setting at run time, so on a library it would do nothing but switch off the culture analysers (CA1304, CA1305, CA1309, CA1310, CA1311), which then check this project's code. The tool project, `src\Source191.Cader`, keeps it, so the shipped tool still runs invariant.
