using Source191.Cader.Application.Effects;

namespace Source191.Cader.Application.Actions;

/// <summary>The folder's full path is on the clipboard.</summary>
public sealed record FolderPathCopied(string Path) : Outcome;