using Microsoft.Extensions.Time.Testing;
using Source191.Cader.Application.Actions;
using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Reporting;
using Source191.Cader.Application.Views;
using Source191.Cader.Core.Tests.Application.Reporting;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application.Actions;

public sealed class OpenProblemReportHandlerShould
{
    private static readonly Started Start = new(new ToolVersion("0.17.0-preview.6"), new Version(2, 1, 312), new Version(10, 0, 26100), new Version(10, 0, 14), 120, 36);
    private static readonly ReportTitle Title = ReportTitle.From("Attach froze after Ctrl+Z");
    private readonly FakeCrashFile crashFile = new();
    private readonly FakeDesktop desktop = new();
    private readonly DiagnosticLog log = new(new FakeTimeProvider(new DateTimeOffset(2026, 10, 29, 12, 0, 0, TimeSpan.Zero)), []);

    [Fact]
    public async Task OpenTheFittedAddressAndRemoveTheCrash()
    {
        crashFile.Write(CrashLine.At(new DateTimeOffset(2026, 10, 28, 9, 0, 1, TimeSpan.Zero)) + "\n");
        ProblemReport report = await Report();

        Outcome outcome = await new OpenProblemReportHandler(desktop, crashFile).HandleAsync(new OpenProblemReport(Title, report), CancellationToken.None);

        outcome.ShouldBe(new ProblemReportOutcome.Opened());
        desktop.Opened.ShouldBe([ProblemReportAddress.For(Title, report.FitWithin(Title, ProblemReport.AddressLimit)).AbsoluteUri]);
        crashFile.Removed.ShouldBeTrue();
    }

    [Fact]
    public async Task LeaveTheCrashFileAloneWithoutACrash()
    {
        ProblemReport report = await Report();

        await new OpenProblemReportHandler(desktop, crashFile).HandleAsync(new OpenProblemReport(Title, report), CancellationToken.None);

        crashFile.Removed.ShouldBeFalse();
    }

    [Fact]
    public async Task LeaveTheCrashWhenTheBrowserFails()
    {
        crashFile.Write(CrashLine.At(new DateTimeOffset(2026, 10, 28, 9, 0, 1, TimeSpan.Zero)) + "\n");
        desktop.OpenFailure = new ProgramNotStartedException("No application is associated with the address.");
        ProblemReport report = await Report();

        Outcome outcome = await new OpenProblemReportHandler(desktop, crashFile).HandleAsync(new OpenProblemReport(Title, report), CancellationToken.None);

        outcome.ShouldBe(new ProblemReportOutcome.NotOpened("No application is associated with the address."));
        crashFile.Removed.ShouldBeFalse();
    }

    [Fact]
    public async Task LetAnUndeclaredFailureThrough()
    {
        desktop.OpenFailure = new InvalidOperationException("Not a failure IDesktop declares.");
        ProblemReport report = await Report();

        await Should.ThrowAsync<InvalidOperationException>(() => new OpenProblemReportHandler(desktop, crashFile).HandleAsync(new OpenProblemReport(Title, report), CancellationToken.None));
    }

    private Task<ProblemReport> Report()
    {
        return new ProblemReports(log, new ProcessCounts(), crashFile, Start, TestSettings.Current(@"C:\Users\me\source\repos"), new FakeTimeProvider(Fixtures.Noon)).BuildAsync();
    }

    [Fact]
    public async Task KeepTheFittedTextWhenTheTitleLeavesRoomForMore()
    {
        ProblemReport report = await ReportOfManyEvents();

        await new OpenProblemReportHandler(desktop, crashFile).HandleAsync(new OpenProblemReport(Title, report), CancellationToken.None);

        report.FitWithin(Title, ProblemReport.AddressLimit).ShouldNotBe(report.Fitted, "a short title would keep more lines if the text were fitted again");
        desktop.Opened.ShouldBe([ProblemReportAddress.For(Title, report.Fitted).AbsoluteUri]);
    }

    [Fact]
    public async Task FitTheTextAgainForATitleThatEscapesLongerThanTheRoomLeftForIt()
    {
        ReportTitle wide = ReportTitle.From(new string('€', ReportTitle.MostCharacters));
        ProblemReport report = await ReportOfManyEvents();

        await new OpenProblemReportHandler(desktop, crashFile).HandleAsync(new OpenProblemReport(wide, report), CancellationToken.None);

        ProblemReportAddress.Length(wide, report.Fitted).ShouldBeGreaterThan(ProblemReport.AddressLimit);
        desktop.Opened.ShouldBe([ProblemReportAddress.For(wide, report.FitWithin(wide, ProblemReport.AddressLimit)).AbsoluteUri]);
    }

    private async Task<ProblemReport> ReportOfManyEvents()
    {
        foreach (int index in Enumerable.Range(0, 500))
        {
            log.Record(new ViewShown(ViewKind.Sessions));
        }

        return await Report();
    }
}