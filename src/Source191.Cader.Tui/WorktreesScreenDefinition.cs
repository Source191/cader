using Source191.Cader.Application;
using Source191.Cader.Application.Detail;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Terminal.Gui.Drawing;

namespace Source191.Cader.Tui;

/// <summary>The worktrees view as the shared table screen shows it: its layout, columns, colours and the effects its keys raise.</summary>
internal sealed class WorktreesScreenDefinition(FolderLabels folders, IDetailSource<WorktreeRow> detail, RefreshSchedule schedule) : IScreenDefinition<WorktreesViewState, WorktreeRow>
{
    public ConsoleTab Tab => ConsoleTab.Worktrees;

    public TabRefresh Refresh => new TabRefresh.Every(schedule);

    public TableDetail<WorktreeRow> Detail => new TableDetail<WorktreeRow>.Pane(detail);

    public string WaitingText => Wording.WaitingText(ConsoleTab.Worktrees);

    public string LoadFailurePrefix => Wording.LoadFailurePrefix(ConsoleTab.Worktrees);

    public string HelpText => Wording.WorktreesHelp;

    public IReadOnlyList<TableColumn<WorktreeRow>> Columns { get; } =
    [
        // The WORKTREE cell names the folder's slug, or a row with no folder's branch, under a heading naming the repository; BRANCH flags the branch, and the pane names it and gives the flag's reason.
        TableColumn<WorktreeRow>.Plain(Wording.WorktreeColumn, (row, _, width) => TextFit.CutEnd(WorktreeOf(folders, row), width), new ColumnFit.Elastic()),
        TableColumn<WorktreeRow>.Plain(Wording.HealthColumn, (row, _, _) => Wording.HealthCell(row.Health.Kind), new ColumnFit.Natural()),
        new(Wording.BranchColumn, (row, _, _) => Wording.BranchCell(row.Flag), new ColumnFit.Natural(), FlagPaint),
        TableColumn<WorktreeRow>.Plain(Wording.LastCommitColumn, (row, now, _) => row.LastCommitAt is LastCommit.Made made ? Durations.Age(now - made.At) : "-", new ColumnFit.RightAligned()),
        TableColumn<WorktreeRow>.Plain(Wording.SessionColumn, (row, _, _) => row.Session is WorktreeSession.Running running ? Wording.Name(running.Name) : string.Empty, new ColumnFit.Natural()),
    ];

    /// <summary>A flag keeps its own colour off the selected row; a blank cell takes the row's.</summary>
    private static CellPaint FlagPaint(WorktreeRow row) => FlagPaints[row.Flag];

    // The cell is painted on every draw, so each flag's paint is made once.
    private static readonly Dictionary<BranchFlagKind, CellPaint> FlagPaints = Enum.GetValues<BranchFlagKind>().ToDictionary(flag => flag, PaintOf);

    private static CellPaint PaintOf(BranchFlagKind flag) => flag is BranchFlagKind.None ? new CellPaint.None() : new CellPaint.Text(Palette.ForTone(Wording.ToneOf(flag)));

    /// <summary>The folder's part after its repository, which the heading names, or a row with no folder's branch.</summary>
    private static string WorktreeOf(FolderLabels folders, WorktreeRow row) => (row.Placement, row.Head) switch
    {
        (Placement.NoFolder, Head.OnBranch on) => on.Branch.Value,
        _ => WhereLabel.PlaceOf(row.RepositoryName, folders.For(row.Path)),
    };

    public string Heading(WorktreeRow first) => first.RepositoryName;

    public Color RowColour(WorktreeRow row)
    {
        return row.Health.Kind switch
        {
            WorktreeHealthKind.Ok => Palette.White,
            WorktreeHealthKind.Locked => Palette.Amber,
            WorktreeHealthKind.Empty => Palette.Grey,
            WorktreeHealthKind.Orphan => Palette.Red,
            WorktreeHealthKind.Broken => Palette.Red,
            WorktreeHealthKind.Elsewhere => Palette.Amber,
            WorktreeHealthKind.Branch => Palette.Grey,
            WorktreeHealthKind.Origin => Palette.Amber,
        };
    }

    public IReadOnlyList<TextPart> Counts(WorktreesViewState state) => Wording.WorktreeCounts(state.List.Rows);

    public KeyBar Bar(WorktreesViewState state, bool updateKnown) => Wording.WorktreesBar(updateKnown);

    public WorktreesViewState WithMessage(WorktreesViewState state, StatusMessage message) => state with { Message = message };

    public ViewUpdate<WorktreesViewState> Reduce(WorktreesViewState state, ViewInput input) => WorktreesView.Reduce(state, input);
}