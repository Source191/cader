using Source191.Cader.Domain;

namespace Source191.Cader.Application.Views;

/// <summary>
/// A row of the Config tab: a setting kept in config.json, a piece of setup it reads but does not edit, or an account it remembers.
/// Each case carries the facts its words are made from; <c>Wording</c> names the row and says its value.
/// </summary>
/// <param name="Key">The selection key, stable across a save so the row just edited stays selected.</param>
public abstract record ConfigRow(string Key)
{
    public sealed record RepositoryRoot(int Index, FolderPath Path) : ConfigRow($"root:{Index}")
    {
        /// <summary>False once the setup read found no folder at the path; true until then.</summary>
        public bool Found { get; init; } = true;
    }

    public sealed record WorktreeRoot(FolderPath Path) : ConfigRow("worktree-root");

    public sealed record OpenFolderWith(EditorChoice Editor) : ConfigRow("open-folder-with");

    public sealed record DefaultBase(WorktreeBase Base) : ConfigRow("default-base");

    public sealed record BeforeBranching(Domain.BeforeBranching Choice) : ConfigRow("before-branching");

    public sealed record DefaultPlace(LaunchMode Place) : ConfigRow("default-place");

    public sealed record CheckForUpdates(bool On) : ConfigRow("check-for-updates");

    public sealed record DiagnosticLog(bool On) : ConfigRow("diagnostic-log");

    /// <summary>The heading over the setup rows or the account rows: a row the selection steps over and any filter hides.</summary>
    public sealed record Heading(ConfigSection Section) : ConfigRow(Section is ConfigSection.Setup ? "heading:setup" : "heading:accounts");

    /// <summary>A piece of setup: what it is and what was read about it.</summary>
    public sealed record Setup(SetupItem Item, SetupState State) : ConfigRow($"setup:{Item}");

    /// <summary>A Claude account Cader remembers, and whether it is logged in now; keyed by its organisation, so a rename keeps it selected.</summary>
    public sealed record Account(RowAccount.Known Known) : ConfigRow($"account:{Known.Account.Organisation.Value}");
}