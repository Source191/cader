namespace Source191.Cader.Ports;

/// <summary>A name that changed in a watched folder: its path relative to that folder (the new name for a rename, empty for an overflow) and what happened. Nothing is read from the file.</summary>
public sealed record FolderChange(string Path, FolderChangeKind Kind);