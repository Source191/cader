using Microsoft.Extensions.Time.Testing;
using Source191.Cader.Adapters.Git;
using Source191.Cader.Application;
using Source191.Cader.Application.Detail;
using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;
using Source191.Cader.Tui;

namespace Source191.Cader.Core.Tests.Tui;

public sealed class WorktreesScreenDefinitionShould
{
    private static readonly FolderPath Repository = FolderPath.From(@"C:\Users\me\source\repos\Northwind.Billing");
    private static readonly FolderPath Worktree = FolderPath.From(@"C:\Users\me\source\worktrees\Northwind.Billing-invoice-pdf");

    private readonly WorktreesScreenDefinition definition;
    private readonly RefreshSchedule schedule;

    public WorktreesScreenDefinitionShould()
    {
        FakeTimeProvider clock = new(Fixtures.Noon);
        FakeProcessRunner processes = new FakeProcessRunner().AnswerAll(new ProcessResult(0, string.Empty, string.Empty));
        DetailLoader loader = new(new GitCli(processes, "git"), new InMemoryState(), new SessionActivity(clock), new DiagnosticLog(new FakeTimeProvider(Fixtures.Noon), []));
        FolderLabels folders = new(TestSettings.Current(TestSettings.For([@"C:\Users\me\source\repos"], @"D:\work")), FolderPath.From(@"C:\Users\me"));
        schedule = new RefreshSchedule(() => Settings.WorktreesRefreshWhenUnset, clock, new LaunchHold());
        definition = new WorktreesScreenDefinition(folders, new WorktreeDetails(loader, clock), schedule);
    }

    [Fact]
    public void ListItsColumnsAsWordingDoesWithWorktreeElastic()
    {
        definition.Columns.Select(column => column.Help).ShouldBe(Wording.WorktreesColumns);
        definition.Columns.Single(column => column.Fit is ColumnFit.Elastic).Help.ShouldBeSameAs(Wording.WorktreeColumn);
    }

    [Fact]
    public void RightAlignLastCommit()
    {
        TableColumn<WorktreeRow> lastCommit = definition.Columns.Single(column => column.Help == Wording.LastCommitColumn);

        lastCommit.Fit.ShouldBe(new ColumnFit.RightAligned());
        lastCommit.Cell(Row(new LastCommit.Made(Fixtures.Noon.AddMinutes(-5))), Fixtures.Noon, int.MaxValue).ShouldBe("5m");
        lastCommit.Cell(Row(new LastCommit.Unknown()), Fixtures.Noon, int.MaxValue).ShouldBe("-");
    }

    [Fact]
    public void ShowEveryColumnOfARow()
    {
        WorktreeRow row = Row(new LastCommit.Made(Fixtures.Noon.AddHours(-2))) with { Session = new WorktreeSession.Running(SessionId.From("81cdb0f9-0000-4000-8000-000000000001"), Listed.As("invoice-pdf")) };

        definition.Columns.Select(column => column.Cell(row, Fixtures.Noon, int.MaxValue)).ShouldBe(
        [
            @"~\source\worktrees\Northwind.Billing-invoice-pdf",
            Wording.HealthCell(WorktreeHealthKind.Ok),
            string.Empty,
            "2h00m",
            "invoice-pdf",
        ]);
    }

    [Fact]
    public void NameTheBranchInTheWorktreeCellOfABranchRow()
    {
        WorktreeRow branch = Row(new LastCommit.Unknown()) with { Path = Repository, Head = new Head.OnBranch(BranchName.From("spike-refunds")), Health = WorktreeHealth.CheckedOutNowhere };
        WorktreeRow origin = branch with { Head = new Head.OnBranch(BranchName.From("c-new-bug")), Health = WorktreeHealth.OnlyOnOrigin };

        definition.Columns[0].Cell(branch, Fixtures.Noon, int.MaxValue).ShouldBe("spike-refunds");
        definition.Columns[0].Cell(origin, Fixtures.Noon, int.MaxValue).ShouldBe("c-new-bug");
    }

    [Theory]
    [InlineData(@"D:\work\Northwind.Billing-invoice-pdf", "invoice-pdf")]
    [InlineData(@"C:\Users\me\source\repos\Northwind.Billing\src\Api", @"src\Api")]
    [InlineData(@"D:\work\spike", "spike")]
    [InlineData(@"C:\Users\me\scratch\Northwind.Billing-old", @"~\scratch\Northwind.Billing-old")]
    public void ShowNoRepositoryInTheWorktreeCell(string folder, string cell)
    {
        WorktreeRow row = Row(new LastCommit.Unknown()) with { Path = FolderPath.From(folder) };

        definition.Columns[0].Cell(row, Fixtures.Noon, int.MaxValue).ShouldBe(cell);
        definition.Columns[0].Cell(row, Fixtures.Noon, 4).ShouldBe(TextFit.CutEnd(cell, 4));
    }

    [Fact]
    public void NameAHeadingByRepository()
    {
        IScreenDefinition<WorktreesViewState, WorktreeRow> screen = definition;

        screen.Heading(Row(new LastCommit.Unknown())).ShouldBe("Northwind.Billing");
    }

    [Theory]
    [InlineData(BranchFlagKind.LeftBehind, "▲ left behind", TextTone.Red)]
    [InlineData(BranchFlagKind.Gone, "✗ gone", TextTone.Amber)]
    [InlineData(BranchFlagKind.Closed, "✗ closed", TextTone.Amber)]
    [InlineData(BranchFlagKind.NoPullRequest, "· no PR", TextTone.Grey)]
    public void ShowTheBranchFlagInItsTone(BranchFlagKind flag, string cell, TextTone tone)
    {
        TableColumn<WorktreeRow> branch = definition.Columns.Single(column => column.Help == Wording.BranchColumn);
        WorktreeRow row = Row(new LastCommit.Unknown()) with { Flag = flag };

        branch.Cell(row, Fixtures.Noon, int.MaxValue).ShouldBe(cell);
        branch.Paint(row).ShouldBe(new CellPaint.Text(Palette.ForTone(tone)));
    }

    [Fact]
    public void LeaveTheBranchCellBlankInTheRowsColourWhenNothingIsFlagged()
    {
        TableColumn<WorktreeRow> branch = definition.Columns.Single(column => column.Help == Wording.BranchColumn);

        branch.Cell(Row(new LastCommit.Unknown()), Fixtures.Noon, int.MaxValue).ShouldBeEmpty();
        branch.Paint(Row(new LastCommit.Unknown())).ShouldBe(new CellPaint.None());
    }

    [Fact]
    public void PaintEachFlagWithOnePaintOnceSeen()
    {
        TableColumn<WorktreeRow> branch = definition.Columns.Single(column => column.Help == Wording.BranchColumn);
        WorktreeRow[] rows = [.. Enum.GetValues<BranchFlagKind>().Select(flag => Row(new LastCommit.Unknown()) with { Flag = flag })];

        rows.ShouldAllBe(row => ReferenceEquals(branch.Paint(row), branch.Paint(row)));
        Allocations.Of(() =>
        {
            foreach (WorktreeRow row in rows)
            {
                branch.Paint(row);
            }
        }).ShouldBe(0);
    }

    [Fact]
    public void FitTheBranchColumnToItsWidestFlag()
    {
        TableColumn<WorktreeRow> branch = definition.Columns.Single(column => column.Help == Wording.BranchColumn);

        branch.Fit.ShouldBe(new ColumnFit.Natural());
        Enum.GetValues<BranchFlagKind>().Max(flag => branch.Cell(Row(new LastCommit.Unknown()) with { Flag = flag }, Fixtures.Noon, int.MaxValue).Length).ShouldBe("▲ left behind".Length);
    }

    [Fact]
    public void ShowNoSessionAsNothing()
    {
        definition.Columns.Single(column => column.Help == Wording.SessionColumn).Cell(Row(new LastCommit.Unknown()), Fixtures.Noon, int.MaxValue).ShouldBeEmpty();
    }

    [Theory]
    [InlineData(WorktreeHealthKind.Ok)]
    [InlineData(WorktreeHealthKind.Locked)]
    [InlineData(WorktreeHealthKind.Empty)]
    [InlineData(WorktreeHealthKind.Orphan)]
    [InlineData(WorktreeHealthKind.Broken)]
    [InlineData(WorktreeHealthKind.Elsewhere)]
    [InlineData(WorktreeHealthKind.Branch)]
    [InlineData(WorktreeHealthKind.Origin)]
    public void ColourARowByItsHealth(WorktreeHealthKind kind)
    {
        WorktreeRow row = Row(new LastCommit.Unknown()) with { Health = new WorktreeHealth(kind, new HealthReason.None()) };

        definition.RowColour(row).ShouldBe(kind switch
        {
            WorktreeHealthKind.Ok => Palette.White,
            WorktreeHealthKind.Locked => Palette.Amber,
            WorktreeHealthKind.Empty => Palette.Grey,
            WorktreeHealthKind.Orphan => Palette.Red,
            WorktreeHealthKind.Broken => Palette.Red,
            WorktreeHealthKind.Elsewhere => Palette.Amber,
            WorktreeHealthKind.Branch => Palette.Grey,
            WorktreeHealthKind.Origin => Palette.Amber,
        });
    }

    [Fact]
    public void NameItsTab()
    {
        definition.Tab.ShouldBe(ConsoleTab.Worktrees);
    }

    [Fact]
    public void RefreshOnTheScheduleItWasGiven()
    {
        definition.Refresh.ShouldBe(new TabRefresh.Every(schedule));
    }

    private static WorktreeRow Row(LastCommit lastCommit) => new(Repository, Worktree, new Head.OnBranch(BranchName.From("invoice-pdf")), new WorktreeHealth(WorktreeHealthKind.Ok, new HealthReason.None()), 0, 0, lastCommit, new WorktreeSession.None(), LeftoverFileCount: 0, OwnerExists: true, FolderExists: true);
}