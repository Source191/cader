using Source191.Cader.Domain;
using System.Diagnostics;

namespace Source191.Cader.Application.Views;

public sealed record SessionsViewState(
    ListSelection<SessionRow> List,
    LoadBanner Banner,
    StatusMessage Message)
    : IListViewState<SessionRow>
{
    private static readonly ListPolicy<SessionRow> Policy = new(
        KeyOf: row => row.Session.SessionId.Value,
        FilterFields: row => [row.Repository, .. FilterText.Of(row.Session.Name), .. FilterText.Of(row.Head)],
        Order: ByGroupThenAttention,
        KeyComparison: StringComparison.Ordinal,
        Selectable: _ => true);

    /// <summary>Nothing listed yet: the first listing has not arrived, so the screen shows that it is waiting rather than an empty table.</summary>
    public static readonly SessionsViewState Empty = new(Policy.EmptyList(), new LoadBanner.None(), StatusMessage.None);

    /// <summary>The automated runs the last listing held, for the counts; kept when a listing fails, as the rows are.</summary>
    public AutomatedRuns AutomatedRuns { get; init; } = AutomatedRuns.None;

    public IReadOnlyList<SessionRow> VisibleRows => List.Visible;

    public SessionRow? Selected => List.Selected;

    public TableTitle Title => List.Title("session", row => Wording.Name(row.Session.Name));

    public IReadOnlyList<TableLine<SessionRow>> Lines => TableLines.Ungrouped(List.Visible);

    /// <summary>Grouped by where the rows run, each group a run of rows in attention order: the sort is stable, so the group keys keep the attention order within a group.</summary>
    private static IEnumerable<SessionRow> ByGroupThenAttention(IEnumerable<SessionRow> rows)
    {
        return ByAttention(rows)
            .OrderBy(row => GroupOf(row).Rank)
            .ThenBy(row => GroupOf(row).Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => GroupOf(row).Key, StringComparer.Ordinal);
    }

    /// <summary>
    /// The group a row is headed by: a repository, named by its main checkout and keyed by it so two of one name stay apart, or after every
    /// repository the folder it runs in, by its path, a folder that has gone kept apart from a live folder of the same path.
    /// </summary>
    private static (int Rank, string Name, string Key) GroupOf(SessionRow row) => row.Location switch
    {
        SessionLocation.InRepository inRepository => (0, inRepository.Name, $"repository|{inRepository.Checkout.Key}"),
        SessionLocation.Outside => (1, row.Session.WorkingDirectory.Value, $"folder|{row.Session.WorkingDirectory.Key}"),
        SessionLocation.FolderGone => (1, row.Session.WorkingDirectory.Value, $"gone|{row.Session.WorkingDirectory.Key}"),
        _ => throw new UnreachableException(),
    };

    private static IEnumerable<SessionRow> ByAttention(IEnumerable<SessionRow> rows)
    {
        List<SessionRow> matching = [.. rows];
        Sessions ordered = new([.. matching.Select(row => row.Session)]);
        Dictionary<SessionId, SessionRow> bySession = matching.ToDictionary(row => row.Session.SessionId);

        return ordered.OrderedByAttention().Select(session => bySession[session.SessionId]);
    }
}