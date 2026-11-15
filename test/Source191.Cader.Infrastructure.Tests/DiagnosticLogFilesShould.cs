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

    private string OlderPath => path + ".1";

    private static void FileOfBytes(string file, long length)
    {
        using FileStream stream = new(file, FileMode.Create, FileAccess.Write);
        stream.SetLength(length);
    }
}