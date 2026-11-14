using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.Application.Actions;

/// <summary>Opens a file in the editor chosen in settings, once it is known to exist: a log that is not written yet is an outcome, not an empty editor.</summary>
public sealed class OpenFileInEditorHandler(EditorLauncher launcher, IFolders folders) : IEffectHandler<OpenFileInEditor>
{
    public async Task<Outcome> HandleAsync(OpenFileInEditor effect, CancellationToken cancellationToken)
    {
        FolderPath file = effect.File;

        return folders.FileExists(file.Value)
            ? await launcher.OpenAsync(file, cancellationToken)
            : new OpenInEditorOutcome.FileMissing(file.Name);
    }
}