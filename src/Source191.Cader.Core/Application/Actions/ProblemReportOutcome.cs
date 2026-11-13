using Source191.Cader.Application.Effects;

namespace Source191.Cader.Application.Actions;

/// <summary>What became of a problem report: opened in the browser, not opened (with Windows' reason), or copied.</summary>
public abstract record ProblemReportOutcome : Outcome
{
    public sealed record Opened : ProblemReportOutcome;

    public sealed record NotOpened(string Reason) : ProblemReportOutcome;

    public sealed record Copied : ProblemReportOutcome;
}