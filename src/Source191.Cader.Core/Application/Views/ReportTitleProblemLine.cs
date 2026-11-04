using Source191.Cader.Domain;

namespace Source191.Cader.Application.Views;

/// <summary>What the problem report dialog shows under the title: nothing, or why the title is refused.</summary>
public abstract record ReportTitleProblemLine
{
    /// <summary>Nothing is wrong: the problem line is blank.</summary>
    public sealed record None : ReportTitleProblemLine;

    public sealed record Shows(ReportTitleProblem Problem) : ReportTitleProblemLine;
}