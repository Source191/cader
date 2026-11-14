namespace Source191.Cader.Application.Views;

/// <summary>Everything the setup rows show, read together; see <see cref="SetupFactsReader"/>.</summary>
public sealed record SetupFacts(SetupState SettingsFile, SetupState DiagnosticLogFile, SetupState TerminalProfiles, SetupState ClaudeBell, SetupState LastUpdateCheck)
{
    /// <summary>The setup before it has been read: every piece still <see cref="SetupState.Reading"/>.</summary>
    public static readonly SetupFacts Reading = new(new SetupState.Reading(), new SetupState.Reading(), new SetupState.Reading(), new SetupState.Reading(), new SetupState.Reading());
}