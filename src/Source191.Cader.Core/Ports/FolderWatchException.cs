namespace Source191.Cader.Ports;

/// <summary>Windows refused to watch a folder; the cause is kept only for the record and never shown.</summary>
public sealed class FolderWatchException(Exception cause) : Exception(cause.GetType().Name, cause);