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

    [Fact]
    public void StartANewFileWhenALineWouldTakeTheFilePastTenMegabytes()
    {
        string line = new('x', 98);
        FileOfBytes(10_485_700);
        DiagnosticFile file = new(path, () => on);

        file.Write(line);

        new FileInfo(RolledPath).Length.ShouldBe(10_485_700);
        File.ReadAllText(path).ShouldBe(line + Environment.NewLine);
        new FileInfo(path).Length.ShouldBe(98 + NewLineBytes);
    }

    [Fact]
    public void KeepALineThatTakesTheFileToExactlyTenMegabytes()
    {
        FileOfBytes(10_485_760 - (100 + NewLineBytes));
        DiagnosticFile file = new(path, () => on);

        file.Write(new string('x', 100));

        new FileInfo(path).Length.ShouldBe(10_485_760);
        File.Exists(RolledPath).ShouldBeFalse();
    }

    [Fact]
    public void RollOverWhenALineTakesTheFileOneByteOverTenMegabytes()
    {
        FileOfBytes(10_485_760 - (100 + NewLineBytes) + 1);
        DiagnosticFile file = new(path, () => on);

        file.Write(new string('x', 100));

        new FileInfo(RolledPath).Length.ShouldBe(10_485_760 - (100 + NewLineBytes) + 1);
        new FileInfo(path).Length.ShouldBe(100 + NewLineBytes);
    }

    [Fact]
    public void CountBytesNotCharacters()
    {
        FileOfBytes(10_485_760 - (100 + NewLineBytes) + 1);
        DiagnosticFile file = new(path, () => on);

        file.Write(new string('é', 50));

        File.Exists(RolledPath).ShouldBeTrue("fifty characters are a hundred bytes in UTF-8");
        new FileInfo(path).Length.ShouldBe(100 + NewLineBytes);
    }

    [Fact]
    public void ReplaceAnEarlierRolledFile()
    {
        File.WriteAllText(RolledPath, "an earlier rollover");
        FileOfBytes(10_485_760);
        DiagnosticFile file = new(path, () => on);

        file.Write("2026-10-29T12:00:00.000Z view Sessions");

        new FileInfo(RolledPath).Length.ShouldBe(10_485_760);
        Directory.GetFiles(folder).Select(Path.GetFileName).Order().ShouldBe(["cader.log", "cader.log.1"]);
    }

    [Fact]
    public void KeepWritingToTheNewFileAfterARollover()
    {
        FileOfBytes(10_485_760);
        DiagnosticFile file = new(path, () => on);

        file.Write("first");
        file.Write("second");

        File.ReadAllLines(path).ShouldBe(["first", "second"]);
        new FileInfo(RolledPath).Length.ShouldBe(10_485_760);
    }

    [Fact]
    public void LeaveTheLineInTheOldFileWhenTheMoveFails()
    {
        FileOfBytes(10_485_760);
        File.WriteAllText(RolledPath, "an earlier rollover");
        using FileStream held = new(RolledPath, FileMode.Open, FileAccess.Read, FileShare.None);
        DiagnosticFile file = new(path, () => on);

        Should.NotThrow(() => file.Write("2026-10-29T12:00:00.000Z view Sessions"));

        new FileInfo(path).Length.ShouldBe(10_485_760 + "2026-10-29T12:00:00.000Z view Sessions".Length + NewLineBytes);
    }

    private static int NewLineBytes => Environment.NewLine.Length;

    private string RolledPath => path + ".1";

    private void FileOfBytes(long length)
    {
        using FileStream stream = new(path, FileMode.Create, FileAccess.Write);
        stream.SetLength(length);
    }
}
