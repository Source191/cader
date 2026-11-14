using Source191.Cader.Domain;

namespace Source191.Cader.Application.Views;

/// <summary>Open the file itself in the editor chosen in settings; a <see cref="FolderPath"/> holds any full path, as <see cref="CopyFolderPath"/> relies on.</summary>
public sealed record OpenFileInEditor(FolderPath File) : ViewEffect;