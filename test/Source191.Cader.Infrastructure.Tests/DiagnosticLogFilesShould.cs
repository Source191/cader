using Source191.Cader.Adapters.Diagnostics;
using Source191.Cader.Ports;

namespace Source191.Cader.Infrastructure.Tests;

public sealed class DiagnosticLogFilesShould : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), $"test-log-files-{Guid.NewGuid():N}");
    private readonly string path;

    public DiagnosticLogFilesShould()
    {
        Directory.CreateDirectory(folder);
        path = Path.Combine(folder, "cader.log");
    }

    public void Dispose()
    {
        Directory.Delete(folder, recursive: true);
    }

    [Fact]
    public void ReadBothFilesWithTheirSizesAndPaths()
    {
        FileOfBytes(path, 300);
        FileOfBytes(OlderPath, 512_000);
        DiagnosticLogFiles files = new(path);

        files.Read().ShouldBe(new LogFiles(new LogFileOnDisc.Written(path, 300), new LogFileOnDisc.Written(OlderPath, 512_000)));
    }

    [Fact]
    public void ReadAMissingFileAsAbsent()
    {
        FileOfBytes(path, 300);
        DiagnosticLogFiles files = new(path);

        files.Read().ShouldBe(new LogFiles(new LogFileOnDisc.Written(path, 300), new LogFileOnDisc.Absent(OlderPath)));
    }

    [Fact]
    public void ReadBothFilesAsAbsentWhenTheFolderIsMissing()
    {
        string elsewhere = Path.Combine(folder, "missing", "cader.log");
        DiagnosticLogFiles files = new(elsewhere);

        files.Read().ShouldBe(new LogFiles(new LogFileOnDisc.Absent(elsewhere), new LogFileOnDisc.Absent(elsewhere + ".1")));
    }

    [Fact]
    public void NameTheOlderFileAsTheRolloverDoes()
    {
        DiagnosticFile.OlderPath(path).ShouldBe(OlderPath);
    }

    [Fact]
    public void DeleteBothFilesAndCountTheirBytes()
    {
        FileOfBytes(path, 2_516_582);
        FileOfBytes(OlderPath, 10_485_700);
        DiagnosticLogFiles files = new(path);

        files.Clear().ShouldBe(new LogFilesCleared(Removed: 2, Bytes: 13_002_282, Kept: 0));

        File.Exists(path).ShouldBeFalse();
        File.Exists(OlderPath).ShouldBeFalse();
    }

    [Fact]
    public void DeleteTheOneFileThatIsThere()
    {
        FileOfBytes(path, 300);
        DiagnosticLogFiles files = new(path);

        files.Clear().ShouldBe(new LogFilesCleared(Removed: 1, Bytes: 300, Kept: 0));

        File.Exists(path).ShouldBeFalse();
    }

    [Fact]
    public void ClearNothingWhenNeitherFileNorTheFolderIsThere()
    {
        DiagnosticLogFiles files = new(Path.Combine(folder, "missing", "cader.log"));

        files.Clear().ShouldBe(new LogFilesCleared(Removed: 0, Bytes: 0, Kept: 0));
    }

    [Fact]
    public void KeepAFileHeldOpenAndStillDeleteTheOther()
    {
        FileOfBytes(path, 300);
        FileOfBytes(OlderPath, 512_000);
        DiagnosticLogFiles files = new(path);
        LogFilesCleared cleared;

        using (FileStream held = new(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            cleared = files.Clear();
        }

        cleared.ShouldBe(new LogFilesCleared(Removed: 1, Bytes: 512_000, Kept: 1));
        File.Exists(path).ShouldBeTrue();
        File.Exists(OlderPath).ShouldBeFalse();
    }

    [Fact]
    public void LeaveTheCrashFileAndEveryOtherFileAlone()
    {
        FileOfBytes(path, 300);
        File.WriteAllText(Path.Combine(folder, "last-crash.log"), "a crash");
        File.WriteAllText(Path.Combine(folder, "config.json"), "{}");
        File.WriteAllText(Path.Combine(folder, "cader.log.2"), "not ours");
        DiagnosticLogFiles files = new(path);

        files.Clear();

        Directory.GetFiles(folder).Select(Path.GetFileName).Order().ShouldBe(["cader.log.2", "config.json", "last-crash.log"]);
        File.ReadAllText(Path.Combine(folder, "last-crash.log")).ShouldBe("a crash");
    }

    [Fact]
    public void LetTheLogStartANewFileHoldingOnlyItsNextLine()
    {
        DiagnosticFile sink = new(path, () => true);
        sink.Write("2026-11-15T09:00:00.000Z [4242] view Sessions");
        FileOfBytes(OlderPath, 512_000);
        DiagnosticLogFiles files = new(path);

        files.Clear();
        sink.Write("2026-11-15T09:00:01.000Z [4242] view Config");

        File.ReadAllLines(path).ShouldBe(["2026-11-15T09:00:01.000Z [4242] view Config"]);
        File.Exists(OlderPath).ShouldBeFalse();
    }

    private string OlderPath => path + ".1";

    private static void FileOfBytes(string file, long length)
    {
        using FileStream stream = new(file, FileMode.Create, FileAccess.Write);
        stream.SetLength(length);
    }
}