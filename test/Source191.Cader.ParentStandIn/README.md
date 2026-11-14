# Source191.Cader.ParentStandIn

A stand-in for Cader as the parent of a captured child, and for that child as it starts a program of its own, which `ProcessApisShould` uses to see what `ProcessStartInfo.KillOnParentExit` (new in .NET 11, Windows only) ends when the parent is killed. Its arguments are how to start the program (`kill-on-parent-exit`, `plain` or `shell-execute`, the last hidden), what to do then (`wait` to be killed, or `exit`), the file to write the started program's id to, and the program with its arguments. No test framework here, and nothing in it is shipped.

- **Its own program:** the property is .NET 11's, so the parent must run on .NET 11; the test process cannot be killed to show it, and the runner image's PowerShell (7.6.9 on 3 November 2026) still runs on .NET 10. It is its own child too, because a start under `KillOnParentExit` is refused for a packaged program's app execution alias, such as the Microsoft Store PowerShell's `pwsh.exe`.
