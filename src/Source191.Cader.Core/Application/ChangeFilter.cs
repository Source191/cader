using Source191.Cader.Ports;

namespace Source191.Cader.Application;

/// <summary>
/// Which changes in a watched folder start a refresh, decided from the changed name alone, never the file's contents. An
/// overflow always counts, since the changes it lost may have. A repository's git folder counts only a worktree's
/// record coming or going and a checked-out branch changing (<c>HEAD</c>): its index, objects, references, logs and lock
/// files change with git's everyday work and Cader's own reads, so counting them would make every scan start the next.
/// </summary>
public static class ChangeFilter
{
    private const string Worktrees = "worktrees";
    private const string Head = "HEAD";

    public static bool Counts(WatchKind watch, FolderChange change) => change.Kind is FolderChangeKind.Overflowed || watch switch
    {
        WatchKind.ClaudeSessions => Parts(change) is [string record] && record.EndsWith(".json", StringComparison.OrdinalIgnoreCase),
        WatchKind.ClaudeJobs => Parts(change) is [_, string state] && Named(state, "state.json"),
        WatchKind.Repository => IsWorktreeRecord(change) || IsHead(change),
        WatchKind.Root => Parts(change) is [_] && ComesOrGoes(change),
    };

    /// <summary><c>worktrees</c> itself, made with the first linked worktree and removed with the last, or one worktree's folder in it.</summary>
    private static bool IsWorktreeRecord(FolderChange change) => ComesOrGoes(change) && Parts(change) is { Length: 1 or 2 } and [string folder, ..] && Named(folder, Worktrees);

    /// <summary>The main checkout's <c>HEAD</c> or a linked worktree's, written when a checkout changes its branch.</summary>
    private static bool IsHead(FolderChange change) =>
        change.Kind is FolderChangeKind.Changed or FolderChangeKind.Renamed
        && Parts(change) switch
        {
            [string head] => Named(head, Head),
            [string folder, _, string head] => Named(folder, Worktrees) && Named(head, Head),
            _ => false,
        };

    private static bool ComesOrGoes(FolderChange change) => change.Kind is FolderChangeKind.Created or FolderChangeKind.Deleted or FolderChangeKind.Renamed;

    private static bool Named(string part, string name) => string.Equals(part, name, StringComparison.OrdinalIgnoreCase);

    private static string[] Parts(FolderChange change) => change.Path.Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries);
}