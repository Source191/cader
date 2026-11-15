using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Reporting;
using Source191.Cader.Application.Views;
using Source191.Cader.Ports;

namespace Source191.Cader.Application.Actions;

/// <summary>
/// Opens the repository's problem report form in the browser, filled in with the title and the report fitted into the
/// address. The console sends nothing itself. A crash the report carried is removed once the page has opened, so the next
/// report does not repeat it; when the browser cannot be started the crash stays and the words suggest copying instead.
/// </summary>
public sealed class OpenProblemReportHandler(IDesktop desktop, ICrashFile crashFile) : IEffectHandler<OpenProblemReport>
{
    public async Task<Outcome> HandleAsync(OpenProblemReport effect, CancellationToken cancellationToken)
    {
        // The previewed text, unless this title escapes longer than the room it leaves.
        string fitted = effect.Report.Fitted;
        // Stryker disable once Equality : Equivalent: at exactly the limit the text fitted for the longest title already fits this one, and fitting again for this title drops the same lines (the mutant that fits again below the limit is killed by KeepTheFittedTextWhenTheTitleLeavesRoomForMore)
        if (ProblemReportAddress.Length(effect.Title, fitted) > ProblemReport.AddressLimit)
        {
            fitted = effect.Report.FitWithin(effect.Title, ProblemReport.AddressLimit);
        }

        try
        {
            await desktop.OpenAsync(ProblemReportAddress.For(effect.Title, fitted).AbsoluteUri, cancellationToken);
        }
        catch (ProgramNotStartedException failure)
        {
            return new ProblemReportOutcome.NotOpened(failure.Reason);
        }

        if (effect.Report.HasCrash)
        {
            await crashFile.RemoveAsync();
        }

        return new ProblemReportOutcome.Opened();
    }
}