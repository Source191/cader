namespace Source191.Cader.ReadmeScene;

/// <summary>
/// One session row: where it runs (the repository's main checkout when <see cref="Worktree"/> is blank, otherwise the
/// worktree with that slug), what Claude Code says it is doing, whether the console opens a tab for it, and the label of the scene account that owns it (none when blank).
/// </summary>
public sealed record SceneSession(
    string Name,
    string Repository,
    SceneKind Kind,
    SceneState State,
    int StartedMinutesAgo,
    string Worktree = "",
    string WaitingFor = "",
    bool Tab = false,
    int MemoryMegabytes = 0,
    string Account = "")
{
    public DateTimeOffset StartedAt(TimeProvider clock) => clock.GetUtcNow().AddMinutes(-StartedMinutesAgo);
}