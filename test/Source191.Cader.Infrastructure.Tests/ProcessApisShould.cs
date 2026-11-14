using Microsoft.Win32.SafeHandles;
using Source191.Cader.Adapters.Processes;
using Source191.Cader.Adapters.Terminal;
using Source191.Cader.Ports;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Versioning;

namespace Source191.Cader.Infrastructure.Tests;

/// <summary>
/// Facts about process APIs .NET 11 added, kept from the upgrade-to-net11 spikes: they show what the platform does, not
/// Cader, and the change's design records which were adopted and why the rest were declined. A run-and-capture start is
/// given exactly what <see cref="ProcessRunner"/> gives its captured children. A descendant a fact leaves running on
/// purpose is a sleeper found by the id its starter writes, never by name, held by a handle so its id cannot be reused,
/// and ended when the fact finishes.
/// </summary>
[Trait("Category", "Integration")]
[SupportedOSPlatform("windows")]
public sealed class ProcessApisShould : IDisposable
{
    private const string Pwsh = "pwsh.exe";
    private const int GoneId = int.MaxValue - 7;
    private const int SystemProcessId = 4;
    private static readonly string[] AliasNames = ["pwsh.exe", "winget.exe"];

    private readonly string folder = Path.Combine(Path.GetTempPath(), "test-process-apis-" + Guid.NewGuid().ToString("N"));
    private readonly List<Process> sleepers = [];

    public ProcessApisShould()
    {
        Directory.CreateDirectory(folder);
    }

    /// <summary>Cancelling kills only the child (<c>Process.Kill()</c>), so its own child keeps running and holding the pipes.</summary>
    [Fact]
    public async Task LeaveADescendantRunningWhenRunAndCaptureTextIsCancelled()
    {
        string idFile = Path.Combine(folder, "sleeper.txt");
        using CancellationTokenSource cancel = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        Task<ProcessTextOutput> run = Process.RunAndCaptureTextAsync(Captured(SleeperScript(idFile)), cancel.Token);
        Process sleeper = await SleeperAsync(idFile);

        await cancel.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(() => run);
        sleeper.WaitForExit(TimeSpan.FromSeconds(2)).ShouldBeFalse("only the child was ended, so its own child kept running");
    }

    /// <summary>The synchronous form's limit ends the child the same way, and leaves its descendants.</summary>
    [Fact]
    public async Task LeaveADescendantRunningWhenRunAndCaptureTextRunsOutOfTime()
    {
        string idFile = Path.Combine(folder, "sleeper.txt");

        Should.Throw<TimeoutException>(() => Process.RunAndCaptureText(Captured(SleeperScript(idFile)), TimeSpan.FromSeconds(5)));

        Process sleeper = await SleeperAsync(idFile);
        sleeper.WaitForExit(TimeSpan.FromSeconds(2)).ShouldBeFalse("only the child was ended, so its own child kept running");
    }

    /// <summary>What Cader does today with the same child, for comparison: the whole tree is ended.</summary>
    [Fact]
    public async Task EndTheDescendantWhenProcessRunnerIsCancelled()
    {
        string idFile = Path.Combine(folder, "sleeper.txt");
        using CancellationTokenSource cancel = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        Task<ProcessResult> run = new ProcessRunner(TimeSpan.FromMinutes(1)).RunAsync(Request(SleeperScript(idFile)), cancel.Token);
        Process sleeper = await SleeperAsync(idFile);

        await cancel.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(() => run);
        sleeper.WaitForExit(TimeSpan.FromSeconds(5)).ShouldBeTrue("the runner ends the child's whole tree");
    }

    [Fact]
    public async Task DecodeAndHandOverTheEnvironmentAsTheStartInfoSays()
    {
        ProcessStartInfo start = Captured("[Console]::OutputEncoding = [System.Text.Encoding]::UTF8; [Console]::Out.Write(\"$env:CADER_SPIKE|$env:COMPUTERNAME|\" + [char]0x00B7 + [char]0x00E9)");
        start.Environment.Remove("COMPUTERNAME");
        start.Environment["CADER_SPIKE"] = "Northwind";

        ProcessTextOutput output = await Process.RunAndCaptureTextAsync(start, TestContext.Current.CancellationToken);

        output.ExitStatus.ExitCode.ShouldBe(0);
        output.StandardOutput.ShouldBe("Northwind||·é");
    }

    /// <summary>
    /// The stand-in plays Cader and is killed alone, as a crash would end it. Its child, the stand-in again, starts a
    /// sleeper and exits at once, as a daemon's starter does; the sleeper is started either directly or through
    /// ShellExecute, as Cader's PowerShell child starts an editor.
    /// </summary>
    [Theory]
    [InlineData("kill-on-parent-exit", "plain", true)]
    [InlineData("kill-on-parent-exit", "shell-execute", true)]
    [InlineData("plain", "plain", false)]
    public async Task EndEveryDescendantWithTheParentOnlyUnderKillOnParentExit(string parentStart, string childStart, bool ended)
    {
        string standIn = Path.Combine(AppContext.BaseDirectory, "Source191.Cader.ParentStandIn.exe");
        string idFile = Path.Combine(folder, "sleeper.txt");
        string sleeper = Path.Combine(Environment.SystemDirectory, "waitfor.exe");
        string[] arguments = [parentStart, "wait", Path.Combine(folder, "child.txt"), standIn, childStart, "exit", idFile, sleeper, "/t", "60", "CaderSpike" + Guid.NewGuid().ToString("N")];
        using Process parent = Process.Start(new ProcessStartInfo(standIn, arguments) { UseShellExecute = false, CreateNoWindow = true }) ?? throw new InvalidOperationException("The stand-in did not start.");
        try
        {
            Process descendant = await SleeperAsync(idFile);

            parent.Kill();
            await parent.WaitForExitAsync(TestContext.Current.CancellationToken);

            descendant.WaitForExit(TimeSpan.FromSeconds(5)).ShouldBe(ended);
        }
        finally
        {
            parent.Kill();
        }
    }

    /// <summary>
    /// A packaged program's app execution alias in WindowsApps, as Windows Terminal's wt.exe, winget and the Microsoft
    /// Store PowerShell are found on the PATH, cannot be started into the parent's job; the start fails instead.
    /// </summary>
    [Fact]
    public void RefuseToStartAnAppExecutionAliasUnderKillOnParentExit()
    {
        string aliases = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "WindowsApps");
        string? alias = AliasNames.Select(name => Path.Combine(aliases, name)).FirstOrDefault(File.Exists);
        Assert.SkipWhen(alias is null, "No PowerShell or winget app execution alias on this machine.");
        ProcessStartInfo start = new(alias, ["--version"]) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, KillOnParentExit = true };

        Win32Exception refusal = Should.Throw<Win32Exception>(() => Process.Start(start)?.Dispose());

        refusal.NativeErrorCode.ShouldBe(5);
    }

    /// <summary>A lookup by id answers false for an id that has gone, and finds a protected process, which then refuses to be ended.</summary>
    [Fact]
    public void FindAProtectedProcessButNotOneThatHasGone()
    {
        Process.TryGetProcessById(GoneId, out Process? gone).ShouldBeFalse();
        gone.ShouldBeNull();
        Process.TryGetProcessById(SystemProcessId, out Process? system).ShouldBeTrue();
        using (system)
        {
            system.ProcessName.ShouldBe("System");
        }

        SafeProcessHandle.TryOpen(GoneId, out SafeProcessHandle? handle).ShouldBeFalse();
        handle.ShouldBeNull();
        Should.Throw<UnauthorizedAccessException>(() => SafeProcessHandle.TryOpen(SystemProcessId, out _), "TryOpen answers false only for an id that has gone");
    }

    public void Dispose()
    {
        foreach (Process sleeper in sleepers)
        {
            if (!sleeper.HasExited)
            {
                sleeper.Kill();
            }

            sleeper.Dispose();
        }

        Directory.Delete(folder, recursive: true);
    }

    private static ProcessRequest Request(string script) => new(Pwsh, PowerShellArguments(script), WorkingFolder.Inherit);

    private static ProcessStartInfo Captured(string script) => ProcessRunner.StartInfo(Request(script), capture: true);

    private static string[] PowerShellArguments(string script) => ["-NoProfile", "-NonInteractive", "-EncodedCommand", EncodedCommand.Encode(script)];

    /// <summary>
    /// Starts a PowerShell that sleeps for a minute, which shares the child's console and its redirected standard handles,
    /// writes its id to the file, then sleeps as long itself.
    /// </summary>
    private static string SleeperScript(string idFile) => $$"""
        $info = [System.Diagnostics.ProcessStartInfo]::new('{{Pwsh}}')
        foreach ($argument in '-NoProfile', '-NonInteractive', '-Command', 'Start-Sleep -Seconds 60') { $info.ArgumentList.Add($argument) }
        $info.UseShellExecute = $false
        $sleeper = [System.Diagnostics.Process]::Start($info)
        Set-Content -LiteralPath '{{idFile}}.tmp' -Value $sleeper.Id
        Move-Item -LiteralPath '{{idFile}}.tmp' -Destination '{{idFile}}'
        Start-Sleep -Seconds 60
        """;

    /// <summary>Waits up to 30 seconds for the sleeper's id, then holds a handle to it so its id cannot be reused.</summary>
    private async Task<Process> SleeperAsync(string idFile)
    {
        for (int attempt = 0; attempt < 300 && !File.Exists(idFile); attempt++)
        {
            await Task.Delay(100, TestContext.Current.CancellationToken);
        }

        File.Exists(idFile).ShouldBeTrue("the sleeper's id never appeared within 30 seconds");
        int id = int.Parse(await File.ReadAllTextAsync(idFile, TestContext.Current.CancellationToken), CultureInfo.InvariantCulture);
        Process sleeper = Process.GetProcessById(id);
        _ = sleeper.SafeHandle;
        sleepers.Add(sleeper);

        return sleeper;
    }
}