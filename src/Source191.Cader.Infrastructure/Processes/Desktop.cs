using Source191.Cader.Adapters.Terminal;
using Source191.Cader.Ports;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Source191.Cader.Adapters.Processes;

/// <summary>
/// Hands things to Windows: a file or URL to its associated application
/// (<see href="https://learn.microsoft.com/windows/win32/api/shellapi/nf-shellapi-shellexecutew">ShellExecute</see>), a
/// program to run on its own. Neither is waited for to close, and both get the same environment as every other child of
/// the console, without the Claude Code markers. ShellExecute
/// <see href="https://learn.microsoft.com/dotnet/api/system.diagnostics.processstartinfo.useshellexecute">cannot be given
/// an environment</see>, so a short-lived PowerShell child that has the cleaned one
/// makes the call (see <see cref="OpenFileScript"/>); the call waits for that child, which ends once Windows has started the
/// application, so a file that has gone is reported rather than lost. PowerShell is run by the full path it was given
/// (missing when pwsh.exe is not on the PATH, which fails each open with a sentence naming it).
/// </summary>
public sealed class Desktop(IProcessRunner processes, ProgramLocation powerShell) : IDesktop
{
    public async Task OpenAsync(string file, CancellationToken cancellationToken)
    {
        // Thrown inside the task, so a caller holding the task sees the failure when it awaits.
        string executable = powerShell switch
        {
            ProgramLocation.Found found => found.Path,
            ProgramLocation.Missing => throw new ProgramNotStartedException("PowerShell 7 (pwsh.exe) was not found on the PATH, so nothing can be opened."),
            _ => throw new UnreachableException(),
        };

        // Only a file is asked about: an address's text after its last dot is no file type.
        if (Path.IsPathFullyQualified(file) && Path.GetExtension(file) is { Length: > 0 } extension && FileAssociations.NoneFor(extension))
        {
            throw new NoApplicationException(extension);
        }

        ProcessRequest request = new(executable, ["-NoProfile", "-NonInteractive", "-EncodedCommand", EncodedCommand.Encode(OpenFileScript.For(file))], WorkingFolder.Inherit);
        ProcessResult result = await processes.RunAsync(request, cancellationToken);
        if (result.ExitCode != 0)
        {
            string reason = result.StandardError.Trim();
            throw new ProgramNotStartedException(reason.Length > 0 ? reason : $"PowerShell stopped with exit code {result.ExitCode} before Windows opened it.");
        }
    }

    /// <summary>
    /// Starts the program detached, through ShellExecute from the cleaned PowerShell child (see
    /// <see cref="StartProgramScript"/>), so it inherits none of the console's handles; the call waits only for that child.
    /// </summary>
    public async Task StartAsync(ProcessRequest request, CancellationToken cancellationToken)
    {
        string executable = powerShell switch
        {
            ProgramLocation.Found found => found.Path,
            ProgramLocation.Missing => throw new ProgramNotStartedException("PowerShell 7 (pwsh.exe) was not found on the PATH, so nothing can be opened."),
            _ => throw new UnreachableException(),
        };

        ProcessRequest child = new(executable, ["-NoProfile", "-NonInteractive", "-EncodedCommand", EncodedCommand.Encode(StartProgramScript.For(request.FileName, request.Arguments))], request.WorkingDirectory);
        ProcessResult result = await processes.RunAsync(child, cancellationToken);
        if (result.ExitCode != 0)
        {
            string reason = result.StandardError.Trim();
            if (int.TryParse(reason, NumberStyles.Integer, CultureInfo.InvariantCulture, out int code))
            {
                throw new ProgramNotStartedException(new Win32Exception(code).Message);
            }

            throw new ProgramNotStartedException(reason.Length > 0 ? reason : $"PowerShell stopped with exit code {result.ExitCode} before Windows started it.");
        }
    }
}
