using Microsoft.Extensions.Time.Testing;
using Source191.Vantage.Adapters.Git;
using Source191.Vantage.Application;
using Source191.Vantage.Application.Detail;
using Source191.Vantage.Application.Views;
using Source191.Vantage.Domain;
using Source191.Vantage.Ports;
using Source191.Vantage.TestDoubles;
using Source191.Vantage.Tui;

namespace Source191.Vantage.Core.Tests.Tui;

public sealed class SessionsScreenDefinitionShould
{
    private static readonly DateTimeOffset Noon = new(2026, 10, 30, 12, 0, 0, TimeSpan.Zero);
    private static readonly FolderPath Folder = FolderPath.From(@"C:\Users\me\source\worktrees\Northwind.Billing-invoice-pdf");

    private readonly FakeTimeProvider clock = new(Noon);
    private readonly SessionsScreenDefinition definition;

    public SessionsScreenDefinitionShould()
    {
        FakeProcessRunner processes = new FakeProcessRunner().AnswerAll(new ProcessResult(0, string.Empty, string.Empty));
        SessionActivity activity = new(clock);
        DetailLoader loader = new(new GitCli(processes, "git"), new InMemoryState(), activity);
        FolderLabels folders = new(() => [FolderPath.From(@"C:\Users\me\source\repos")], FolderPath.From(@"C:\Users\me"));
        definition = new SessionsScreenDefinition(new StateTransitionNotifier(new StringWriter()), activity, @"C:\Users\me\.local\bin\claude.exe", folders, new SessionDetails(loader, clock));
    }

    [Fact]
    public void ListItsColumnsAsWordingDoesWithRepositoryElastic()
    {
        definition.Columns.Select(column => column.Help).ShouldBe(Wording.SessionsColumns);
        definition.Columns.Single(column => column.Fit is ColumnFit.Elastic).Help.ShouldBeSameAs(Wording.RepositoryColumn);
    }

    [Fact]
    public void CapNameAtTwentyEightCells()
    {
        TableColumn<SessionRow> name = definition.Columns.Single(column => column.Help == Wording.NameColumn);

        name.Fit.ShouldBe(new ColumnFit.Capped(28));
    }

    [Fact]
    public void RightAlignAge()
    {
        TableColumn<SessionRow> age = definition.Columns.Single(column => column.Help == Wording.AgeColumn);

        age.Fit.ShouldBe(new ColumnFit.RightAligned());
        age.Cell(Row(new BackgroundSession(SessionId.From("81cdb0f9-0000-4000-8000-000000000001"), JobId.From("81cdb0f9"), "fix-123", Folder, Noon.AddMinutes(-5), DisplayState.Idle, new JobProcess.None()), new LiveTab.None()), Noon, int.MaxValue).ShouldBe("5m");
    }

    [Fact]
    public void PaintOnlyTheTabCell()
    {
        SessionRow open = Row(new BackgroundSession(SessionId.From("81cdb0f9-0000-4000-8000-000000000001"), JobId.From("81cdb0f9"), "fix-123", Folder, Noon, DisplayState.Idle, new JobProcess.None()), new LiveTab.Open(new TabColour("Yellow", "#FFB900")));
        SessionRow plain = Row(new InteractiveSession(SessionId.From("81cdb0f9-0000-4000-8000-000000000002"), "plain", Folder, Noon, DisplayState.Idle, ProcessId: 4242), new LiveTab.None());

        definition.Columns.Where(column => column.Help != Wording.TabColumn).Select(column => column.Paint(open)).ShouldAllBe(paint => paint is CellPaint.None);
        definition.Columns.Single(column => column.Help == Wording.TabColumn).Paint(open).ShouldBeOfType<CellPaint.Fill>();
        definition.Columns.Single(column => column.Help == Wording.TabColumn).Paint(plain).ShouldBe(new CellPaint.Text(Palette.Grey));
    }

    private static SessionRow Row(Session session, LiveTab tab) => new(session, new SessionLocation.Outside("Northwind.Billing"), new Head.OnBranch(BranchName.From("invoice-pdf")), tab, HasTranscript: true);
}
