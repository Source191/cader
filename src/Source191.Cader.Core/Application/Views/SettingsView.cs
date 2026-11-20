using Source191.Cader.Domain;
using System.Diagnostics;

namespace Source191.Cader.Application.Views;

/// <summary>
/// A setting's dialog as a pure function of state and input; the dialog only binds controls to it. The value being
/// edited is held as typed inside the saved settings, so Save checks the whole with <see cref="Settings.Validate"/> and
/// either yields the settings to save or shows the problem with focus in the field it is about.
/// </summary>
public static class SettingsView
{
    public static SettingsUpdate Reduce(SettingsState state, SettingsInput input)
    {
        return input switch
        {
            SettingsInput.RootsChanged changed => Changed(state with { Text = state.Text with { Roots = changed.Text } }),
            SettingsInput.FoldersChosen { Folders.Count: 0 } => Changed(state),
            SettingsInput.FoldersChosen chosen when state.Browses == SettingsBrowse.OneFolder => Changed(state with { Text = state.Text with { WorktreeRoot = chosen.Folders[0] } }),
            SettingsInput.FoldersChosen chosen => Changed(state with { Text = state.Text with { Roots = WithLines(state.Text.Roots, chosen.Folders) } }),
            SettingsInput.FileChosen { Path.Length: 0 } => Changed(state),
            SettingsInput.FileChosen chosen => Changed(state with { Text = state.Text with { Executable = chosen.Path } }),
            SettingsInput.WorktreeRootChanged changed => Changed(state with { Text = state.Text with { WorktreeRoot = changed.Text } }),
            SettingsInput.PresetChanged changed => Changed(state with { Text = state.Text with { Preset = changed.Preset } }),
            SettingsInput.PresetsLocated located => Changed(state with { Presets = new PresetSearch.Looked(located.Missing) }),
            SettingsInput.ExecutableChanged changed => Changed(state with { Text = state.Text with { Executable = changed.Text } }),
            SettingsInput.ArgumentsChanged changed => Changed(state with { Text = state.Text with { Arguments = changed.Text } }),
            SettingsInput.BaseChanged changed => Changed(state with { Text = state.Text with { Base = changed.Base } }),
            SettingsInput.PlaceChanged changed => Changed(state with { Text = state.Text with { Place = changed.Place } }),
            SettingsInput.BeforeBranchingChanged changed => Changed(state with { Text = state.Text with { BeforeBranching = changed.Choice } }),
            SettingsInput.SessionsRefreshChanged changed => Changed(state with { Text = state.Text with { SessionsRefresh = changed.Text } }),
            SettingsInput.WorktreesRefreshChanged changed => Changed(state with { Text = state.Text with { WorktreesRefresh = changed.Text } }),
            SettingsInput.FieldFocused focused => Changed(state with { Focus = focused.Field }),
            SettingsInput.Save => Saved(state),
            SettingsInput.Cancel => new SettingsUpdate(state, new SettingsEffect.Close()),
            _ => Changed(state),
        };
    }

    private static SettingsUpdate Saved(SettingsState state)
    {
        Func<string, bool> folderExists = state.Scope.Fields.Contains(SettingsField.Roots) ? state.FolderExists : _ => true;
        Func<string, bool> fileExists = state.Scope.Fields.Contains(SettingsField.Executable) ? state.FileExists : _ => true;

        return Settings.Validate(state.Text, folderExists, fileExists) switch
        {
            SettingsValidation.Valid valid => new SettingsUpdate(state with { Problem = new SettingsProblemLine.None() }, new SettingsEffect.Save(valid.Settings)),
            SettingsValidation.Refused refused => Changed(state with { Problem = new SettingsProblemLine.Shows(refused.Problem), Focus = FocusAfter(state, refused.Problem) }),
            _ => throw new UnreachableException(),
        };
    }

    /// <summary>The problem's field when the dialog shows it, otherwise where focus already is, which the dialog always shows.</summary>
    private static SettingsField FocusAfter(SettingsState state, SettingsProblem problem) => state.Scope.Fields.Contains(FieldOf(problem)) ? FieldOf(problem) : state.Focus;

    /// <summary>The field a problem is about, so focus goes where the fix is typed.</summary>
    private static SettingsField FieldOf(SettingsProblem problem)
    {
        return problem switch
        {
            SettingsProblem.NoRoots or SettingsProblem.RootNotFullPath or SettingsProblem.RootMissing or SettingsProblem.RootAlreadyListed => SettingsField.Roots,
            SettingsProblem.WorktreeRootNotFullPath => SettingsField.WorktreeRoot,
            SettingsProblem.NoExecutable or SettingsProblem.ExecutableNotFullPath or SettingsProblem.BatchFileExecutable or SettingsProblem.ExecutableMissing => SettingsField.Executable,
            SettingsProblem.NoFolderPlaceholder => SettingsField.Arguments,
            SettingsProblem.RefreshIntervalNotWholeSeconds { Tab: RefreshedTab.Sessions } => SettingsField.SessionsRefresh,
            SettingsProblem.RefreshIntervalNotWholeSeconds => SettingsField.WorktreesRefresh,
            _ => throw new UnreachableException(),
        };
    }

    /// <summary>The roots as typed with each folder on a line of its own after them; trailing blank lines are dropped first, so no blank line sits between.</summary>
    private static string WithLines(string roots, IReadOnlyList<string> folders) => string.Join('\n', [.. roots.TrimEnd() is { Length: > 0 } typed ? [typed] : Array.Empty<string>(), .. folders]);

    private static SettingsUpdate Changed(SettingsState state) => new(state, new SettingsEffect.None());
}