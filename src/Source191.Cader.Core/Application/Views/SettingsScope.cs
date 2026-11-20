namespace Source191.Cader.Application.Views;

/// <summary>Which one setting a dialog edits, and the field it opens on; the Config tab raises one per setting row.</summary>
public abstract record SettingsScope(SettingsField Field)
{
    /// <summary>Every field the dialog shows, which is what focus may be on; Core's statement of what SettingsControls builds per scope.</summary>
    public virtual IReadOnlyList<SettingsField> Fields => [Field];

    /// <summary>Every repository root, one per line, to change, add or remove together.</summary>
    public sealed record Roots() : SettingsScope(SettingsField.Roots);

    public sealed record WorktreeRoot() : SettingsScope(SettingsField.WorktreeRoot);

    public sealed record Editor() : SettingsScope(SettingsField.Editor)
    {
        public override IReadOnlyList<SettingsField> Fields => [SettingsField.Editor, SettingsField.Executable, SettingsField.Arguments];
    }

    public sealed record Base() : SettingsScope(SettingsField.Base);

    public sealed record Place() : SettingsScope(SettingsField.Place);

    public sealed record BeforeBranching() : SettingsScope(SettingsField.BeforeBranching);

    public sealed record SessionsRefresh() : SettingsScope(SettingsField.SessionsRefresh);

    public sealed record WorktreesRefresh() : SettingsScope(SettingsField.WorktreesRefresh);

    /// <summary>The scope that edits a setting row; the update check and the setup rows have no dialog.</summary>
    public static SettingsScope For(ConfigRow row)
    {
        return row switch
        {
            ConfigRow.RepositoryRoot => new Roots(),
            ConfigRow.WorktreeRoot => new WorktreeRoot(),
            ConfigRow.OpenFolderWith => new Editor(),
            ConfigRow.DefaultBase => new Base(),
            ConfigRow.DefaultPlace => new Place(),
            ConfigRow.BeforeBranching => new BeforeBranching(),
            ConfigRow.SessionsRefresh => new SessionsRefresh(),
            ConfigRow.WorktreesRefresh => new WorktreesRefresh(),
            _ => throw new ArgumentException($"A {row.GetType().Name} row has no dialog.", nameof(row)),
        };
    }
}