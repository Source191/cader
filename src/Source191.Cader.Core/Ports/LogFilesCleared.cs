namespace Source191.Cader.Ports;

/// <summary>What a clear did: how many of the log's files it deleted and their bytes, and how many it kept because another program had one open.</summary>
public sealed record LogFilesCleared(int Removed, long Bytes, int Kept);