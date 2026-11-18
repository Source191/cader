using Source191.Cader.Adapters.Claude;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Infrastructure.Tests;

public sealed class ClaudeCliShould
{
    private const string Executable = @"C:\Users\me\.local\bin\claude.exe";
    private const string Directory = @"C:\repos\Northwind.Orders";

    // A session's own environment is its supervisor's, never the client's (seen 17 November 2026, CLI 2.1.326), so the
    // variable that turns Claude Code's title updates off reaches it only as a value in the settings passed at its start.
    private const string TitleSettings = """{"env":{"CLAUDE_CODE_DISABLE_TERMINAL_TITLE":"1"}}""";

    private const string StartedOutput = """
        Starting background service…
        backgrounded · 81cdb0f9 · cc-spike (idle — send a prompt to start)
          claude agents             list sessions
          claude attach 81cdb0f9    open in this terminal
        """;

    private readonly FakeProcessRunner processes = new FakeProcessRunner().AnswerAll(new ProcessResult(0, string.Empty, string.Empty));
    private readonly SessionName name = SessionName.From("Refund retry policy");

    private ClaudeCli Cli => new(processes, Executable);

    [Fact]
    public async Task ListSessionsByRunningTheJsonListingIncludingCompletedOnes()
    {
        processes.Answer(request => request.Arguments.Contains("agents"), new ProcessResult(0, """[{"sessionId":"81cdb0f9-0000-4000-8000-000000000001","id":"81cdb0f9","name":"one","cwd":"C:\\x","kind":"background","startedAt":1,"status":"idle","state":"done","pid":5}]""", string.Empty));

        IReadOnlyList<Session> sessions = (await Cli.ListSessionsAsync(CancellationToken.None)).Sessions;

        processes.CapturedRequests.Single().ShouldSatisfyAllConditions(
            request => request.FileName.ShouldBe(Executable),
            request => request.Arguments.ShouldBe(["agents", "--json", "--all"]));
        sessions.Single().Name.ShouldBe(Listed.As("one"));
    }

    [Theory]
    [InlineData("agents")]
    [InlineData("stop")]
    [InlineData("rm")]
    public async Task ThrowWithClaudesWordsWhenTheListingStopOrRemoveFails(string subcommand)
    {
        processes.AnswerAll(new ProcessResult(1, string.Empty, "Couldn't connect to the background service\n"));

        ClaudeCliException exception = await Should.ThrowAsync<ClaudeCliException>(() => RunAsync(subcommand));

        exception.Message.ShouldBe("Couldn't connect to the background service");
    }

    [Fact]
    public async Task NameTheCommandAndExitCodeWhenClaudePrintsNothing()
    {
        processes.AnswerAll(new ProcessResult(2, string.Empty, string.Empty));

        ClaudeCliException exception = await Should.ThrowAsync<ClaudeCliException>(() => Cli.StopAsync(JobId.From("81cdb0f9"), CancellationToken.None));

        exception.Message.ShouldBe("claude stop exited with code 2");
    }

    private Task RunAsync(string subcommand) => subcommand switch
    {
        "agents" => Cli.ListSessionsAsync(CancellationToken.None),
        "stop" => Cli.StopAsync(JobId.From("81cdb0f9"), CancellationToken.None),
        _ => Cli.RemoveAsync(JobId.From("81cdb0f9"), CancellationToken.None),
    };

    [Fact]
    public async Task ReadTheVersionFromVersionOutput()
    {
        processes.AnswerAll(new ProcessResult(0, "2.1.310 (Claude Code)\n", string.Empty));

        ClaudeVersionReport version = await Cli.VersionAsync(CancellationToken.None);

        version.ShouldBe(new ClaudeVersionReport.Printed("2.1.310"));
        processes.CapturedRequests.Single().ShouldSatisfyAllConditions(
            request => request.FileName.ShouldBe(Executable),
            request => request.Arguments.ShouldBe(["--version"]));
    }

    [Fact]
    public async Task AnswerNotReadWhenTheRunTimesOut()
    {
        processes.Fail(new ProcessTimedOutException("claude.exe", TimeSpan.FromSeconds(60)));

        (await Cli.VersionAsync(CancellationToken.None)).ShouldBe(new ClaudeVersionReport.NotRead());
    }

    [Fact]
    public async Task AnswerNotReadWhenVersionFails()
    {
        processes.AnswerAll(new ProcessResult(1, string.Empty, "boom"));

        (await Cli.VersionAsync(CancellationToken.None)).ShouldBe(new ClaudeVersionReport.NotRead());
    }

    [Fact]
    public async Task AnswerNotReadWhenNoVersionIsPrinted()
    {
        processes.AnswerAll(new ProcessResult(0, "Claude Code\n", string.Empty));

        (await Cli.VersionAsync(CancellationToken.None)).ShouldBe(new ClaudeVersionReport.NotRead());
    }

    [Fact]
    public async Task AnswerNotReadWhenTheProgramCannotBeStarted()
    {
        processes.Fail(new System.ComponentModel.Win32Exception("The system cannot find the file specified"));

        (await Cli.VersionAsync(CancellationToken.None)).ShouldBe(new ClaudeVersionReport.NotRead());
    }

    [Fact]
    public async Task AnswerNotReadWhenTheRunIsCancelled()
    {
        processes.Fail(new OperationCanceledException());

        (await Cli.VersionAsync(CancellationToken.None)).ShouldBe(new ClaudeVersionReport.NotRead());
    }

    [Fact]
    public async Task AnswerNotReadWhenTheRunFailsToReadItsOutput()
    {
        processes.Fail(new IOException("The pipe has been ended"));

        (await Cli.VersionAsync(CancellationToken.None)).ShouldBe(new ClaudeVersionReport.NotRead());
    }

    [Fact]
    public async Task StartABackgroundSessionNamedInTheChosenDirectory()
    {
        processes.AnswerAll(new ProcessResult(0, StartedOutput, string.Empty));

        StartResult result = await Cli.StartBackgroundAsync(FolderPath.From(Directory), name, new OpeningPrompt.None(), CancellationToken.None);

        processes.CapturedRequests.Single().ShouldSatisfyAllConditions(
            request => request.WorkingDirectory.ShouldBe(new WorkingFolder.At(Directory)),
            request => request.Arguments.ShouldBe(["--bg", "-n", "Refund retry policy", "--settings", TitleSettings]));
        StartResult.Started started = result.ShouldBeOfType<StartResult.Started>();
        started.JobId.ShouldBe(JobId.From("81cdb0f9"));
        started.Message.ShouldBe(StartedOutput.Trim());
    }

    [Fact]
    public async Task PassTheOpeningPromptAfterTheFlags()
    {
        processes.AnswerAll(new ProcessResult(0, StartedOutput, string.Empty));

        await Cli.StartBackgroundAsync(FolderPath.From(Directory), name, new OpeningPrompt.Text("Fix the flaky test"), CancellationToken.None);

        processes.CapturedRequests.Single().Arguments.ShouldBe(["--bg", "-n", "Refund retry policy", "--settings", TitleSettings, "Fix the flaky test"]);
    }

    [Fact]
    public async Task TurnClaudeCodesTitleUpdatesOffInASessionItStartsAndInOneItResumes()
    {
        processes.AnswerAll(new ProcessResult(0, StartedOutput, string.Empty));

        await Cli.StartBackgroundAsync(FolderPath.From(Directory), name, new OpeningPrompt.None(), CancellationToken.None);
        await Cli.ResumeBackgroundAsync(SessionId.From("81cdb0f9-8c3f-4c9b-b478-ca0a5ebf85c6"), FolderPath.From(Directory), Listed.None(), CancellationToken.None);

        processes.CapturedRequests.Count.ShouldBe(2);
        processes.CapturedRequests.ShouldAllBe(request => string.Join(' ', request.Arguments).Contains($"--settings {TitleSettings}", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ReportAnUntrustedWorkspaceWhenStartingIsRefused()
    {
        processes.AnswerAll(new ProcessResult(1, string.Empty, "Workspace not trusted. Run `claude` in C:\\repos\\Northwind.Orders once and accept the trust prompt, then retry."));

        StartResult result = await Cli.StartBackgroundAsync(FolderPath.From(Directory), name, new OpeningPrompt.None(), CancellationToken.None);

        result.ShouldBeOfType<StartResult.WorkspaceNotTrusted>().Message.ShouldStartWith("Workspace not trusted.");
    }

    [Fact]
    public async Task ReportAnyOtherStartFailureWithItsMessage()
    {
        processes.AnswerAll(new ProcessResult(1, string.Empty, "Couldn't start the background service"));

        StartResult result = await Cli.StartBackgroundAsync(FolderPath.From(Directory), name, new OpeningPrompt.None(), CancellationToken.None);

        result.ShouldBeOfType<StartResult.Failed>().Message.ShouldContain("background service");
    }

    [Fact]
    public async Task AttachInteractivelyByJobId()
    {
        processes.InteractiveExitCode = 0;

        int exitCode = await Cli.AttachAsync(JobId.From("81cdb0f9"), CancellationToken.None);

        processes.InteractiveRequests.Single().Arguments.ShouldBe(["attach", "81cdb0f9"]);
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task StopAndRemoveByJobId()
    {
        await Cli.StopAsync(JobId.From("81cdb0f9"), CancellationToken.None);
        await Cli.RemoveAsync(JobId.From("81cdb0f9"), CancellationToken.None);

        processes.CapturedRequests.Select(request => request.Arguments).ShouldBe([["stop", "81cdb0f9"], ["rm", "81cdb0f9"]]);
    }

    [Fact]
    public async Task ResumeABackgroundSessionBySessionIdInItsDirectory()
    {
        processes.AnswerAll(new ProcessResult(0, "backgrounded · 81cdb0f9 · cc-spike", string.Empty));

        ResumeResult result = await Cli.ResumeBackgroundAsync(SessionId.From("81cdb0f9-8c3f-4c9b-b478-ca0a5ebf85c6"), FolderPath.From(Directory), Listed.None(), CancellationToken.None);

        processes.CapturedRequests.Single().ShouldSatisfyAllConditions(
            request => request.WorkingDirectory.ShouldBe(new WorkingFolder.At(Directory)),
            request => request.Arguments.ShouldBe(["--bg", "--resume", "81cdb0f9-8c3f-4c9b-b478-ca0a5ebf85c6", "--settings", TitleSettings]));
        result.ShouldBeOfType<ResumeResult.Continued>();
    }

    [Theory]
    [InlineData("cc-spike", new[] { "--bg", "--resume", "81cdb0f9-8c3f-4c9b-b478-ca0a5ebf85c6", "-n", "cc-spike", "--settings", TitleSettings })]
    [InlineData(null, new[] { "--bg", "--resume", "81cdb0f9-8c3f-4c9b-b478-ca0a5ebf85c6", "--settings", TitleSettings })]
    public async Task ResumeWithANameOnlyForANamedSession(string? name, string[] expected)
    {
        processes.AnswerAll(new ProcessResult(0, "backgrounded · 81cdb0f9 · cc-spike", string.Empty));
        ListedName listed = name is null ? Listed.None() : Listed.As(name);

        await Cli.ResumeBackgroundAsync(SessionId.From("81cdb0f9-8c3f-4c9b-b478-ca0a5ebf85c6"), FolderPath.From(Directory), listed, CancellationToken.None);

        processes.CapturedRequests.Single().Arguments.ShouldBe(expected);
    }

    [Fact]
    public async Task ReportWhenResumeStartedACopyInstead()
    {
        processes.AnswerAll(new ProcessResult(0, "backgrounded · 99aa88bb · cc-spike\nnote: the session is already running, so a copy was started", string.Empty));

        ResumeResult result = await Cli.ResumeBackgroundAsync(SessionId.From("81cdb0f9-8c3f-4c9b-b478-ca0a5ebf85c6"), FolderPath.From(Directory), Listed.None(), CancellationToken.None);

        result.ShouldBeOfType<ResumeResult.Failed>().Message.ShouldContain("copy");
    }

    [Fact]
    public async Task ReportAResumeThatFailedOutrightWithWhatClaudeSaid()
    {
        processes.AnswerAll(new ProcessResult(1, string.Empty, "Session 81cdb0f9 can't start: source session not found\n"));

        ResumeResult result = await Cli.ResumeBackgroundAsync(SessionId.From("81cdb0f9-8c3f-4c9b-b478-ca0a5ebf85c6"), FolderPath.From(Directory), Listed.None(), CancellationToken.None);

        result.ShouldBeOfType<ResumeResult.Failed>().Message.ShouldBe("Session 81cdb0f9 can't start: source session not found");
    }

    [Fact]
    public async Task RefuseAStartAnswerWhoseIdIsNotEightHexCharacters()
    {
        processes.AnswerAll(new ProcessResult(0, "backgrounded · 81cdb0f9x · cc-spike", string.Empty));

        StartResult result = await Cli.StartBackgroundAsync(FolderPath.From(Directory), name, new OpeningPrompt.None(), CancellationToken.None);

        result.ShouldBeOfType<StartResult.Failed>();
    }

    [Fact]
    public async Task AnswerNotReadWhenTheVersionWordIsNotAVersion()
    {
        processes.AnswerAll(new ProcessResult(0, "2.1.310abc (Claude Code)\n", string.Empty));

        (await Cli.VersionAsync(CancellationToken.None)).ShouldBe(new ClaudeVersionReport.NotRead());
    }

    private const string LoggedIn = """
        {"loggedIn":true,"authMethod":"claude.ai","apiProvider":"firstParty","email":"me@northwind.example","orgId":"6F1C2A8E-4B7D-4C1E-9A3F-2D5E8B0C7A91","orgName":"Northwind Traders","subscriptionType":"max"}
        """;

    [Fact]
    public async Task ReadTheLoginFromAuthStatus()
    {
        processes.AnswerAll(new ProcessResult(0, LoggedIn, string.Empty));

        ClaudeLogin login = await Cli.LoginAsync(CancellationToken.None);

        login.ShouldBe(new ClaudeLogin.LoggedIn(OrganisationId.From("6f1c2a8e-4b7d-4c1e-9a3f-2d5e8b0c7a91"), "me@northwind.example", "Northwind Traders"));
        processes.CapturedRequests.Single().ShouldSatisfyAllConditions(
            request => request.FileName.ShouldBe(Executable),
            request => request.Arguments.ShouldBe(["auth", "status", "--json"]));
    }

    [Theory]
    [InlineData("""{"loggedIn":false}""")]
    [InlineData("""{"loggedIn":true,"authMethod":"apiKey"}""")]
    [InlineData("""{"loggedIn":true,"email":"me@northwind.example","orgName":"Northwind Traders"}""")]
    [InlineData("""{"loggedIn":true,"email":"me@northwind.example","orgId":"northwind","orgName":"Northwind Traders"}""")]
    [InlineData("""{"loggedIn":true,"email":" ","orgId":"6f1c2a8e-4b7d-4c1e-9a3f-2d5e8b0c7a91"}""")]
    [InlineData("""{"loggedIn":"yes","email":"me@northwind.example","orgId":"6f1c2a8e-4b7d-4c1e-9a3f-2d5e8b0c7a91"}""")]
    public async Task AnswerLoggedOutForALoginWithNoAccount(string output)
    {
        processes.AnswerAll(new ProcessResult(0, output, string.Empty));

        (await Cli.LoginAsync(CancellationToken.None)).ShouldBe(new ClaudeLogin.LoggedOut());
    }

    [Fact]
    public async Task KeepALoginWithNoOrganisationName()
    {
        processes.AnswerAll(new ProcessResult(0, """{"loggedIn":true,"email":"me@northwind.example","orgId":"6f1c2a8e-4b7d-4c1e-9a3f-2d5e8b0c7a91"}""", string.Empty));

        (await Cli.LoginAsync(CancellationToken.None)).ShouldBe(new ClaudeLogin.LoggedIn(OrganisationId.From("6f1c2a8e-4b7d-4c1e-9a3f-2d5e8b0c7a91"), "me@northwind.example", string.Empty));
    }

    [Theory]
    [InlineData(0, "Not JSON")]
    [InlineData(0, "[]")]
    [InlineData(1, "")]
    public async Task AnswerNotReadForAnAnswerItCannotRead(int exitCode, string output)
    {
        processes.AnswerAll(new ProcessResult(exitCode, output, string.Empty));

        (await Cli.LoginAsync(CancellationToken.None)).ShouldBe(new ClaudeLogin.NotRead());
    }

    [Fact]
    public async Task AnswerNotReadWhenTheLoginRunTimesOut()
    {
        processes.Fail(new ProcessTimedOutException("claude.exe", TimeSpan.FromSeconds(60)));

        (await Cli.LoginAsync(CancellationToken.None)).ShouldBe(new ClaudeLogin.NotRead());
    }
}