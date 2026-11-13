using Source191.Cader.Adapters.Terminal;
using Source191.Cader.Ports;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Source191.Cader.Adapters.Processes;

/// <summary>
/// Hands things to Windows: a file or URL to its associated application, and a program, such as an editor, to run on its
/// own. Neither is waited for to close, and both get the same environment as every other child of the console, without
/// the Claude Code markers. Both go through
/// <see href="https://learn.microsoft.com/windows/win32/api/shellapi/nf-shellapi-shellexecutew">ShellExecute</see>, which
/// <see href="https://learn.microsoft.com/dotnet/api/system.diagnostics.processstartinfo.useshellexecute">cannot be given
/// an environment</see> and hands the started program none of its caller's standard handles, so a short-lived PowerShell
/// child that has the cleaned environment and a console of its own makes the call (see <see cref="OpenFileScript"/> and
/// <see cref="StartProgramScript"/>); nothing the program prints reaches the console's screen. The call waits for that
/// child, which ends once Windows has started the application, so a file that has gone or a program Windows refuses is
/// reported rather than lost. PowerShell is run by the full path it was given (missing when pwsh.exe is not on the PATH,
/// which fails each call with a sentence naming it).
/// </summary>
public sealed class Desktop(IProcessRunner processes, ProgramLocation powerShell) : IDesktop
{
    public async Task OpenAsync(string file, CancellationToken cancellationToken)
    {
        // Thrown inside the task, so a caller holding the task sees the failure when it awaits.
        string executable = PowerShell();

        // Only a file is asked about: an address's text after its last dot is no file type.
        if (Path.IsPathFullyQualified(file) && Path.GetExtension(file) is { Length: > 0 } extension && FileAssociations.NoneFor(extension))
        {
            throw new NoApplicationException(extension);
        }

        ProcessResult result = await RunAsync(executable, OpenFileScript.For(file), WorkingFolder.Inherit, cancellationToken);
        if (result.ExitCode != 0)
        {
            throw new ProgramNotStartedException(ReasonOf(result, "opened"));
        }
    }

    /// <summary>
    /// Starts the program detached, from the request's folder. A refusal (access denied, not a program, a broken install)
    /// arrives as Windows' native error code alone and becomes <see cref="ProgramNotStartedException"/> with Windows'
    /// sentence for it, since .NET's own message also names the path; any other failure gives the child's words.
    /// </summary>
    public async Task StartAsync(ProcessRequest request, CancellationToken cancellationToken)
    {
        string executable = PowerShell();
        ProcessResult result = await RunAsync(executable, StartProgramScript.For(request.FileName, request.Arguments), request.WorkingDirectory, cancellationToken);
        if (result.ExitCode == 0)
        {
            return;
        }

        if (int.TryParse(result.StandardError.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int code))
        {
            throw new ProgramNotStartedException(new Win32Exception(code).Message);
        }

        throw new ProgramNotStartedException(ReasonOf(result, "started"));
    }

    private string PowerShell() => powerShell switch
    {
        ProgramLocation.Found found => found.Path,
        ProgramLocation.Missing => throw new ProgramNotStartedException("PowerShell 7 (pwsh.exe) was not found on the PATH, so nothing can be opened."),
        _ => throw new UnreachableException(),
    };

    private Task<ProcessResult> RunAsync(string executable, string script, WorkingFolder folder, CancellationToken cancellationToken)
    {
        ProcessRequest request = new(executable, ["-NoProfile", "-NonInteractive", "-EncodedCommand", EncodedCommand.Encode(script)], folder);

        return processes.RunAsync(request, cancellationToken);
    }

    private static string ReasonOf(ProcessResult result, string done)
    {
        string reason = result.StandardError.Trim();

        return reason.Length > 0 ? reason : $"PowerShell stopped with exit code {result.ExitCode} before Windows {done} it.";
    }
}