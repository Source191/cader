using Source191.Cader.Domain;

namespace Source191.Cader.Application.Views;

/// <summary>Put the folder's full path on the clipboard.</summary>
public sealed record CopyFolderPath(FolderPath Folder) : ViewEffect;