using Source191.Cader.Domain;

namespace Source191.Cader.Application.Views;

/// <summary>Open the folder in the editor chosen in settings.</summary>
public sealed record OpenFolderInEditor(FolderPath Folder) : ViewEffect;