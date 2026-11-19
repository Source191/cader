using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.Application.Views;

/// <summary>The Config tab's rows: the settings in the order the tab lists them, then the Setup heading and the setup, read or still being read, then the Accounts heading and the remembered accounts when there are any.</summary>
public static class ConfigRows
{
    public static IReadOnlyList<ConfigRow> For(Settings settings, SetupFacts facts, IReadOnlyList<RowAccount.Known> accounts) =>
        [.. SettingRows(settings), new ConfigRow.Heading(ConfigSection.Setup), .. SetupRows(settings, facts), .. AccountRows(accounts)];

    private static IEnumerable<ConfigRow> AccountRows(IReadOnlyList<RowAccount.Known> accounts) =>
        accounts.Count == 0 ? [] : [new ConfigRow.Heading(ConfigSection.Accounts), .. accounts.Select(account => new ConfigRow.Account(account))];

    private static IEnumerable<ConfigRow> SettingRows(Settings settings)
    {
        return
        [
            .. settings.RepositoryRoots.Select((root, index) => new ConfigRow.RepositoryRoot(index, root)),
            new ConfigRow.WorktreeRoot(settings.WorktreeRoot),
            new ConfigRow.OpenFolderWith(settings.Editor),
            new ConfigRow.DefaultBase(settings.DefaultBase),
            new ConfigRow.BeforeBranching(settings.BeforeBranching),
            new ConfigRow.DefaultPlace(settings.DefaultPlace),
            new ConfigRow.SessionsRefresh(settings.SessionsRefresh),
            new ConfigRow.WorktreesRefresh(settings.WorktreesRefresh),
            new ConfigRow.CheckForUpdates(settings.CheckForUpdates),
            new ConfigRow.DiagnosticLog(settings.DiagnosticLog),
        ];
    }

    /// <summary>The last update check reads as off while the check is off, whatever was recorded before.</summary>
    private static IEnumerable<ConfigRow> SetupRows(Settings settings, SetupFacts facts)
    {
        return
        [
            new ConfigRow.Setup(SetupItem.SettingsFile, facts.SettingsFile),
            new ConfigRow.Setup(SetupItem.ClaudeFolder, facts.ClaudeFolder),
            new ConfigRow.Setup(SetupItem.DiagnosticLogFile, LogFileAs(settings, facts.DiagnosticLogFile)),
            .. OlderLogFile(facts.DiagnosticLogFile),
            new ConfigRow.Setup(SetupItem.TerminalProfiles, facts.TerminalProfiles),
            new ConfigRow.Setup(SetupItem.ClaudeBell, facts.ClaudeBell),
            new ConfigRow.Setup(SetupItem.LastUpdateCheck, settings.CheckForUpdates ? facts.LastUpdateCheck : new SetupState.Disabled()),
        ];
    }

    /// <summary>The older file's row, only once the log's files are read and <c>cader.log.1</c> is there, so it never shows while the setup is being read.</summary>
    private static IEnumerable<ConfigRow> OlderLogFile(SetupState read) => read is SetupState.LogFile { Files.Older: LogFileOnDisc.Written } file ? [new ConfigRow.Setup(SetupItem.OlderDiagnosticLogFile, new SetupState.OlderLog(file.Files))] : [];

    /// <summary>Whether the log is on comes from the settings, so the row is right the moment a save toggles it.</summary>
    private static SetupState LogFileAs(Settings settings, SetupState read) => read is SetupState.LogFile file && !settings.DiagnosticLog ? new SetupState.LogOff(file.Files) : read;
}