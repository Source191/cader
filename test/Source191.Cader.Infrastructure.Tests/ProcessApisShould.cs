using Microsoft.Win32.SafeHandles;
using Source191.Cader.Adapters.Processes;
using Source191.Cader.Adapters.Terminal;
using Source191.Cader.Ports;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Versioning;

namespace Source191.Cader.Infrastructure.Tests;

/// <summary>
/// Facts about process APIs .NET 11 added, kept from the upgrade-to-net11 spikes: they show what the platform does, not
/// Cader, and the change's design records which were adopted and why the rest were declined. A run-and-capture start is
/// given exactly what <see cref="ProcessRunner"/> gives its captured children. A descendant a fact leaves running on
/// purpose is a <c>waitfor</c> sleeper, started by the parent stand-in, found by the id the stand-in writes (never by
/// name), held by a handle so its id cannot be reused, and ended when the fact finishes.
/// </summary>
[Trait("Category", "Integration")]
[SupportedOSPlatform("windows")]
public sealed class ProcessApisShould : IDisposable
{
    private const int GoneId = int.MaxValue - 7;
    private const int SystemProcessId = 4;
    private static readonly string StandIn = Path.Combine(AppContext.BaseDirectory, "Source191.Cader.ParentStandIn.exe");

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
        Task<ProcessTextOutput> run = Process.RunAndCaptureTextAsync(ProcessRunner.StartInfo(SleeperStarter(idFile), capture: true), cancel.Token);
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

        Should.Throw<TimeoutException>(() => Process.RunAndCaptureText(ProcessRunner.StartInfo(SleeperStarter(idFile), capture: true), TimeSpan.FromSeconds(3)));

        Process sleeper = await SleeperAsync(idFile);
        sleeper.WaitForExit(TimeSpan.FromSeconds(2)).ShouldBeFalse("only the child was ended, so its own child kept running");
    }

    /// <summary>What Cader does today with the same child, for comparison: the whole tree is ended.</summary>
    [Fact]
    public async Task EndTheDescendantWhenProcessRunnerIsCancelled()
    {
        string idFile = Path.Combine(folder, "sleeper.txt");
        using CancellationTokenSource cancel = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        Task<ProcessResult> run = new ProcessRunner(TimeSpan.FromMinutes(1)).RunAsync(SleeperStarter(idFile), cancel.Token);
        Process sleeper = await SleeperAsync(idFile);

        await cancel.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(() => run);
        sleeper.WaitForExit(TimeSpan.FromSeconds(5)).ShouldBeTrue("the runner ends the child's whole tree");
    }

    [Fact]
    public async Task DecodeAndHandOverTheEnvironmentAsTheStartInfoSays()
    {
        string script = "[Console]::OutputEncoding = [System.Text.Encoding]::UTF8; [Console]::Out.Write(\"$env:CADER_SPIKE|$env:COMPUTERNAME|\" + [char]0x00B7 + [char]0x00E9)";
        ProcessStartInfo start = ProcessRunner.StartInfo(new ProcessRequest("pwsh.exe", ["-NoProfile", "-NonInteractive", "-EncodedCommand", EncodedCommand.Encode(script)], WorkingFolder.Inherit), capture: true);
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
        string idFile = Path.Combine(folder, "sleeper.txt");
        string[] arguments = [parentStart, "wait", Path.Combine(folder, "child.txt"), StandIn, childStart, "exit", idFile, .. Sleeper()];
        using Process parent = Process.Start(new ProcessStartInfo(StandIn, arguments) { UseShellExecute = false, CreateNoWindow = true }) ?? throw new InvalidOperationException("The stand-in did not start.");
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

    /// <summary>A sleeper that ends on its own after a minute; the name it waits for is never signalled.</summary>
    private static string[] Sleeper()
    {
        return [Path.Combine(Environment.SystemDirectory, "waitfor.exe"), "/t", "60", "CaderSpike" + Guid.NewGuid().ToString("N")];
    }

    /// <summary>The stand-in as a captured child: it starts a sleeper, which gets its redirected standard handles, writes the sleeper's id and waits.</summary>
    private static ProcessRequest SleeperStarter(string idFile)
    {
        return new ProcessRequest(StandIn, ["plain", "wait", idFile, .. Sleeper()], WorkingFolder.Inherit);
    }

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