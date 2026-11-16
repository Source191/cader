namespace Source191.Cader.Ports;

/// <summary>One of the diagnostic log's files: its full path, and whether it is there and how many bytes it holds.</summary>
public abstract record LogFileOnDisc(string Path)
{
    public sealed record Absent(string Path) : LogFileOnDisc(Path);

    public sealed record Written(string Path, long Bytes) : LogFileOnDisc(Path);
}