using Source191.Cader.Application.Reporting;
using Source191.Cader.Domain;

namespace Source191.Cader.Application.Views;

/// <summary>Puts the title and the whole report on the clipboard instead.</summary>
public sealed record CopyProblemReport(ReportTitle Title, ProblemReport Report) : ViewEffect;