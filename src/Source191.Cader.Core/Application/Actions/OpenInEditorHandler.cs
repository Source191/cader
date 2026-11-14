using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.Application.Actions;

/// <summary>
/// Opens a folder in the editor chosen in settings. The folder is checked first because a stopped session's folder may
/// have been deleted and the editor would otherwise open an empty window.
/// </summary>
public sealed class OpenInEditorHandler(EditorLauncher launcher, IFolders folders) : IEffectHandler<OpenFolderInEditor>
{
    public async Task<Outcome> HandleAsync(OpenFolderInEditor effect, CancellationToken cancellationToken)
    {
        FolderPath folder = effect.Folder;

        return folders.DirectoryExists(folder.Value)
            ? await launcher.OpenAsync(folder, cancellationToken)
            : new OpenInEditorOutcome.FolderMissing(folder.Name);
    }
}