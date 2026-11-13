using Source191.Cader.Adapters.Diagnostics;

namespace Source191.Cader.Infrastructure.Tests;

public sealed class DiagnosticFileShould : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), $"test-log-{Guid.NewGuid():N}");
    private readonly string path;
    private bool on = true;

    public DiagnosticFileShould()
    {
        Directory.CreateDirectory(folder);
        path = Path.Combine(folder, "cader.log");
    }

    public void Dispose()
    {
        Directory.Delete(folder, recursive: true);
    }

    [Fact]
    public void AppendEachLineWithoutAByteOrderMark()
    {
        DiagnosticFile file = new(path, () => on);

        file.Write("2026-10-29T12:00:00.000Z view Sessions");
        file.Write("2026-10-29T12:00:01.000Z hand-off returned");

        File.ReadAllLines(path).ShouldBe(["2026-10-29T12:00:00.000Z view Sessions", "2026-10-29T12:00:01.000Z hand-off returned"]);
        File.ReadAllBytes(path)[0].ShouldBe((byte)'2');
    }

    [Fact]
    public void WriteNothingWhileTheSwitchIsOff()
    {
        on = false;
        DiagnosticFile file = new(path, () => on);

        file.Write("2026-10-29T12:00:00.000Z view Sessions");

        File.Exists(path).ShouldBeFalse();
    }

    [Fact]
    public void WriteOnlyTheLinesRecordedWhileTheSwitchIsOn()
    {
        DiagnosticFile file = new(path, () => on);

        file.Write("2026-10-29T12:00:00.000Z first, on");
        on = false;
        file.Write("2026-10-29T12:00:01.000Z second, off");
        on = true;
        file.Write("2026-10-29T12:00:02.000Z third, on");

        File.ReadAllLines(path).ShouldBe(["2026-10-29T12:00:00.000Z first, on", "2026-10-29T12:00:02.000Z third, on"]);
    }

    [Fact]
    public void NotThrowWhenTheFileIsLocked()
    {
        DiagnosticFile file = new(path, () => on);
        using FileStream locked = new(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);

        Should.NotThrow(() => file.Write("2026-10-29T12:00:00.000Z view Sessions"));
    }
}
