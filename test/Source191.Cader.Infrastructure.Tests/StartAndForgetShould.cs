using System.Diagnostics;
using System.Globalization;

namespace Source191.Cader.Infrastructure.Tests;

/// <summary>
/// Facts about .NET 11's detached starts, kept from the upgrade-to-net11 spikes: whether a windowed program that prints,
/// as VS Code does, reaches this process's standard handles or console when started directly rather than through the
/// PowerShell child <c>Desktop.StartAsync</c> uses. The noisy editor attaches to its parent's console only when it was
/// handed no standard output, as Chromium does.
/// </summary>
[Trait("Category", "Integration")]
[Collection("Process environment")]
public sealed class StartAndForgetShould : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "test-start-and-forget-" + Guid.NewGuid().ToString("N"));

    public StartAndForgetShould()
    {
        Directory.CreateDirectory(folder);
    }

    public enum Start
    {
        AndForget,
        Detached,
        WithNullHandles,
        Plain,
    }

    /// <summary>
    /// The detached starts hand the program the null device for each standard handle, so it writes there and attaches to
    /// nothing. A plain start, the one #210 replaced, hands it this process's handles, and its lines land in them.
    /// </summary>
    [Theory]
    [InlineData(Start.AndForget, false)]
    [InlineData(Start.Detached, false)]
    [InlineData(Start.WithNullHandles, false)]
    [InlineData(Start.Plain, true)]
    public async Task KeepAWindowedProgramsOutputOffThisProcessWhenStartedDetached(Start start, bool reachesThisProcess)
    {
        string editor = Path.Combine(AppContext.BaseDirectory, "Source191.Cader.NoisyEditor.exe");
        string marker = Path.Combine(folder, "ran.txt");
        string console = Path.Combine(folder, "console.txt");

        using (StandardHandles.RedirectedTo(console))
        {
            Launch(start, new ProcessStartInfo(editor, [marker]));
            await WaitForAsync(marker);
        }

        string written = await File.ReadAllTextAsync(console, TestContext.Current.CancellationToken);
        written.Contains(NoisyEditor.Program.StandardOutputLine, StringComparison.Ordinal).ShouldBe(reachesThisProcess);
        string[] consoleProcesses = (await File.ReadAllLinesAsync(marker, TestContext.Current.CancellationToken))[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        consoleProcesses.ShouldNotContain(Environment.ProcessId.ToString(CultureInfo.InvariantCulture), "the editor attached to this process's console");
    }

    public void Dispose()
    {
        Directory.Delete(folder, recursive: true);
    }

    private static void Launch(Start start, ProcessStartInfo info)
    {
        switch (start)
        {
            case Start.AndForget:
                Process.StartAndForget(info);
                break;
            case Start.Detached:
                info.UseShellExecute = false;
                info.StartDetached = true;
                Process.Start(info)?.Dispose();
                break;
            case Start.WithNullHandles:
                using (Microsoft.Win32.SafeHandles.SafeFileHandle input = File.OpenNullHandle(), output = File.OpenNullHandle(), error = File.OpenNullHandle())
                {
                    info.StandardInputHandle = input;
                    info.StandardOutputHandle = output;
                    info.StandardErrorHandle = error;
                    info.CreateNoWindow = true;
                    Process.StartAndForget(info);
                }

                break;
            case Start.Plain:
                info.UseShellExecute = false;
                info.CreateNoWindow = true;
                Process.Start(info)?.Dispose();
                break;
        }
    }

    /// <summary>Returns as soon as the file is there; the 30 seconds only bound a child that never writes it.</summary>
    private static async Task WaitForAsync(string file)
    {
        for (int attempt = 0; attempt < 300 && !File.Exists(file); attempt++)
        {
            await Task.Delay(100, TestContext.Current.CancellationToken);
        }

        File.Exists(file).ShouldBeTrue($"{Path.GetFileName(file)} never appeared within 30 seconds");
        await Task.Delay(200, TestContext.Current.CancellationToken);
    }
}