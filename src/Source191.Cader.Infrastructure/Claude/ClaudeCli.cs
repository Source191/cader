using Source191.Cader.Domain;
using Source191.Cader.Ports;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Source191.Cader.Adapters.Claude;

/// <summary>
/// The Claude CLI adapter: runs claude by its full path for the listing, the version, the login, a <c>--bg</c> start and resume,
/// attach, stop and rm, and reads the CLI's words in its answers (the version, the job id, an untrusted workspace) here.
/// </summary>
public sealed class ClaudeCli(IProcessRunner processes, string executable) : IClaudeCli
{
    // The tab keeps the session's name only if its own process turns Claude Code's title updates off, and a session's
    // environment is its supervisor's, not the client's (claude-code.md), so the variable travels as settings for that session.
    private static readonly string[] TitleSettings = ["--settings", """{"env":{"CLAUDE_CODE_DISABLE_TERMINAL_TITLE":"1"}}"""];

    public async Task<SessionListing> ListSessionsAsync(CancellationToken cancellationToken)
    {
        ProcessResult result = await RunOrThrowAsync(["agents", "--json", "--all"], cancellationToken);

        return ClaudeAgentsListing.Parse(result.StandardOutput);
    }

    public async Task<ClaudeVersionReport> VersionAsync(CancellationToken cancellationToken)
    {
        try
        {
            ProcessResult result = await processes.RunAsync(Request(["--version"], WorkingFolder.Inherit), cancellationToken);
            string? word = Words(result.StandardOutput).FirstOrDefault();

            return result.ExitCode == 0 && word is not null && ReleaseVersion.TryParse(word, out _) ? new ClaudeVersionReport.Printed(word) : new ClaudeVersionReport.NotRead();
        }
        catch (Exception exception) when (exception is Win32Exception or InvalidOperationException or IOException or OperationCanceledException or ProcessTimedOutException)
        {
            // A cosmetic header value: a program that cannot start, fails to answer or is given up on reads as unknown.
            return new ClaudeVersionReport.NotRead();
        }
    }

    public async Task<ClaudeLogin> LoginAsync(CancellationToken cancellationToken)
    {
        try
        {
            ProcessResult result = await processes.RunAsync(Request(["auth", "status", "--json"], WorkingFolder.Inherit), cancellationToken);

            return result.ExitCode == 0 ? InterpretLogin(result.StandardOutput) : new ClaudeLogin.NotRead();
        }
        catch (Exception exception) when (exception is Win32Exception or InvalidOperationException or IOException or OperationCanceledException or ProcessTimedOutException)
        {
            // Only names an account on the screen: a program that cannot start or answer reads as no login known.
            return new ClaudeLogin.NotRead();
        }
    }

    public async Task<StartResult> StartBackgroundAsync(FolderPath directory, SessionName name, OpeningPrompt openingPrompt, CancellationToken cancellationToken)
    {
        List<string> arguments = ["--bg", "-n", name.Value, .. TitleSettings];
        if (openingPrompt is OpeningPrompt.Text text)
        {
            arguments.Add(text.Words);
        }

        ProcessResult result = await processes.RunAsync(Request(arguments, new WorkingFolder.At(directory.Value)), cancellationToken);

        return InterpretStart(result);
    }

    public Task<int> AttachAsync(JobId jobId, CancellationToken cancellationToken) => processes.RunInteractiveAsync(Request(["attach", jobId.Value], WorkingFolder.Inherit), cancellationToken);

    public Task StopAsync(JobId jobId, CancellationToken cancellationToken) => RunOrThrowAsync(["stop", jobId.Value], cancellationToken);

    public Task RemoveAsync(JobId jobId, CancellationToken cancellationToken) => RunOrThrowAsync(["rm", jobId.Value], cancellationToken);

    public Task<ResumeResult> ResumeBackgroundAsync(SessionId sessionId, FolderPath directory, ListedName name, CancellationToken cancellationToken) =>
        ResumeAsync(["--bg", "--resume", sessionId.Value, .. NameArguments(name), .. TitleSettings], directory, cancellationToken);

    private static string[] NameArguments(ListedName name) => name switch
    {
        ListedName.Named named => ["-n", named.Name.Value],
        ListedName.Unnamed => [],
        _ => throw new UnreachableException(),
    };

    private async Task<ResumeResult> ResumeAsync(IReadOnlyList<string> arguments, FolderPath directory, CancellationToken cancellationToken)
    {
        ProcessResult result = await processes.RunAsync(Request(arguments, new WorkingFolder.At(directory.Value)), cancellationToken);
        if (result.ExitCode != 0)
        {
            return new ResumeResult.Failed((result.StandardError + result.StandardOutput).Trim());
        }

        string? note = result.StandardOutput
            .Split('\n')
            .Select(line => line.Trim())
            .FirstOrDefault(line => line.StartsWith("note:", StringComparison.OrdinalIgnoreCase));

        return note is null ? new ResumeResult.Continued(result.StandardOutput.Trim()) : new ResumeResult.Failed(note);
    }

    private async Task<ProcessResult> RunOrThrowAsync(IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        ProcessResult result = await processes.RunAsync(Request(arguments, WorkingFolder.Inherit), cancellationToken);
        if (result.ExitCode == 0)
        {
            return result;
        }

        throw new ClaudeCliException(RefusalWords(arguments[0], result));
    }

    private static string RefusalWords(string subcommand, ProcessResult result)
    {
        string words = string.IsNullOrWhiteSpace(result.StandardError) ? result.StandardOutput.Trim() : result.StandardError.Trim();

        return words.Length > 0 ? words : $"claude {subcommand} exited with code {result.ExitCode}";
    }

    private ProcessRequest Request(IReadOnlyList<string> arguments, WorkingFolder folder) => new(executable, arguments, folder);

    private static StartResult InterpretStart(ProcessResult result)
    {
        if (TryReadJobId(result.StandardOutput, out JobId? jobId))
        {
            return new StartResult.Started(jobId, result.StandardOutput.Trim());
        }

        string message = string.IsNullOrWhiteSpace(result.StandardError) ? result.StandardOutput.Trim() : result.StandardError.Trim();

        return message.Contains("Workspace not trusted", StringComparison.OrdinalIgnoreCase)
            ? new StartResult.WorkspaceNotTrusted(message)
            : new StartResult.Failed(message);
    }

    // The word after "backgrounded" is the separator (a middle dot, or whatever a mis-decoded byte became), and the next is the id.
    private static bool TryReadJobId(string output, [MaybeNullWhen(false)] out JobId jobId)
    {
        string[] afterKeyword = [.. Words(output).SkipWhile(word => word != "backgrounded").Skip(2).Take(1)];
        if (afterKeyword.Length == 1)
        {
            return JobId.TryParse(afterKeyword[0], out jobId);
        }

        jobId = null;
        return false;
    }
    /// <summary>Only loggedIn, email, orgId and orgName are read; a login with no email or organisation id (an API key) names no account.</summary>
    private static ClaudeLogin InterpretLogin(string output)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(output);
            JsonElement root = document.RootElement;
            if (root.ValueKind is not JsonValueKind.Object)
            {
                return new ClaudeLogin.NotRead();
            }

            bool loggedIn = root.TryGetProperty("loggedIn", out JsonElement flag) && flag.ValueKind is JsonValueKind.True;
            string? email = Text(root, "email");

            return loggedIn && !string.IsNullOrWhiteSpace(email) && OrganisationId.TryParse(Text(root, "orgId"), out OrganisationId? organisation)
                ? new ClaudeLogin.LoggedIn(organisation, email, Text(root, "orgName") ?? string.Empty)
                : new ClaudeLogin.LoggedOut();
        }
        catch (JsonException)
        {
            return new ClaudeLogin.NotRead();
        }
    }

    private static string? Text(JsonElement root, string name) => root.TryGetProperty(name, out JsonElement value) && value.ValueKind is JsonValueKind.String ? value.GetString() : null;

    private static string[] Words(string text) => text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
}