using Source191.Cader.Domain;

namespace Source191.Cader.Ports;

/// <summary>
/// The git commands the console needs. Every call names the directory it runs in; nothing depends on the current directory.
/// The convention: every method but IsMerged, BranchExists and the nullable queries throws <see cref="GitException"/> when
/// git fails or answers with something unreadable; the queries that return a nullable answer null instead; IsMerged and
/// BranchExists answer false. The fetch and the prune are the commands that reach the network, and they never prompt: a
/// remote that needs signing in fails them at once.
/// </summary>
public interface IGit
{
    /// <summary>
    /// Fetches <paramref name="branch"/>'s branch from its remote into that remote-tracking branch alone. Throws
    /// <see cref="GitException"/> when git fails, as it does offline or when the remote needs credentials that are not stored.
    /// </summary>
    Task FetchBranchAsync(FolderPath repository, RemoteBranch branch, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the repository's records of origin's branches that origin no longer has, downloading nothing and never
    /// prompting. Throws <see cref="GitException"/> when git fails, as it does offline or when origin needs credentials
    /// that are not stored.
    /// </summary>
    Task PruneOriginAsync(FolderPath repository, CancellationToken cancellationToken);

    /// <summary>Whether the repository has the local branch; false when git fails.</summary>
    Task<bool> BranchExistsAsync(FolderPath repository, BranchName branch, CancellationToken cancellationToken);

    /// <summary>
    /// Fast-forwards the branch checked out in <paramref name="checkout"/> to <paramref name="target"/>, updating its files,
    /// and never merges. Throws <see cref="GitException"/> when git refuses: the branch has commits of its own, or a local
    /// change would be overwritten.
    /// </summary>
    Task FastForwardCheckoutAsync(FolderPath checkout, RemoteBranch target, CancellationToken cancellationToken);

    /// <summary>
    /// Moves <paramref name="branch"/>, checked out in no worktree, to <paramref name="target"/> without touching any files.
    /// Throws <see cref="GitException"/> when git refuses: the move is not a fast-forward, or a worktree has the branch checked out.
    /// </summary>
    Task FastForwardBranchAsync(FolderPath repository, BranchName branch, RemoteBranch target, CancellationToken cancellationToken);

    /// <summary>
    /// Every local branch with its upstream, and every branch origin has with no local branch of its name, as git last
    /// recorded them: no fetch. Throws <see cref="GitException"/> when git fails.
    /// </summary>
    Task<IReadOnlyList<GitBranch>> ListBranchesAsync(FolderPath repository, CancellationToken cancellationToken);

    /// <summary>Throws <see cref="GitException"/> when git fails.</summary>
    Task<IReadOnlyList<GitWorktreeEntry>> ListWorktreesAsync(string repository, CancellationToken cancellationToken);

    /// <summary>Throws <see cref="GitException"/> when git fails, for example when the branch is checked out elsewhere.</summary>
    Task AddWorktreeAsync(string repository, string path, string branch, string baseRef, CancellationToken cancellationToken);

    /// <summary>Throws <see cref="GitException"/> when git fails, for example on changes without force.</summary>
    Task RemoveWorktreeAsync(string repository, string path, bool force, CancellationToken cancellationToken);

    /// <summary>How many stale registrations went. Throws <see cref="GitException"/> when git fails.</summary>
    Task<int> PruneWorktreesAsync(string repository, CancellationToken cancellationToken);

    /// <summary>Throws <see cref="GitException"/> when git fails.</summary>
    Task<int> CountChangedFilesAsync(string worktree, CancellationToken cancellationToken);

    /// <summary>Commits on no remote branch. Throws <see cref="GitException"/> when git fails or its count is not a number.</summary>
    Task<int> CountUnpushedCommitsAsync(string worktree, CancellationToken cancellationToken);

    /// <summary>
    /// How many commits <paramref name="revision"/>, a full ref name such as refs/heads/fix-123 or
    /// refs/remotes/origin/fix-123, has that <paramref name="head"/> does not contain and whose changes origin's default
    /// branch lacks: a merge commit, a commit on that branch or one whose changes match one there is not counted, unless
    /// the repository records no default branch for origin, when every such commit counts. Null when git fails, as it
    /// does for a head it has never fetched, or when the revision is not a full ref name.
    /// </summary>
    Task<int?> CountCommitsAfterAsync(FolderPath repository, string revision, CommitId head, CancellationToken cancellationToken);

    /// <summary>The local branch's commits on no remote branch. Throws <see cref="GitException"/> when git fails or its count is not a number.</summary>
    Task<int> CountCommitsOnNoRemoteAsync(FolderPath repository, BranchName branch, CancellationToken cancellationToken);
    /// <summary>
    /// When git recorded creating the local <paramref name="branch"/>: the oldest entry of its reflog, when that entry is
    /// git's own "branch: Created from" record, in UTC. Null when git fails, the branch has no reflog, or its oldest entry
    /// is anything else (a clone, or an entry left after the creation expired).
    /// </summary>
    Task<DateTimeOffset?> BranchCreatedAtAsync(FolderPath repository, BranchName branch, CancellationToken cancellationToken);

    /// <summary>Null when git fails or there are no commits.</summary>
    Task<DateTimeOffset?> LastCommitAtAsync(string worktree, CancellationToken cancellationToken);

    /// <summary>The newest commits on no remote branch, up to <paramref name="limit"/>. Throws <see cref="GitException"/> when git fails.</summary>
    Task<IReadOnlyList<UnpushedCommit>> UnpushedCommitsAsync(string worktree, int limit, CancellationToken cancellationToken);

    /// <summary>The changed and untracked paths, up to <paramref name="limit"/>. Throws <see cref="GitException"/> when git fails.</summary>
    Task<IReadOnlyList<string>> ChangedFilesAsync(string worktree, int limit, CancellationToken cancellationToken);

    /// <summary>Throws <see cref="GitException"/> when git fails, for example on an unmerged branch without force.</summary>
    Task DeleteBranchAsync(string repository, string branch, bool force, CancellationToken cancellationToken);

    /// <summary>Origin's HEAD, such as origin/main; null when git fails or origin has none.</summary>
    Task<string?> DefaultRemoteBranchAsync(string repository, CancellationToken cancellationToken);

    /// <summary>Whether <paramref name="branch"/> is an ancestor of <paramref name="into"/>; false when git fails.</summary>
    Task<bool> IsMergedAsync(string repository, string branch, string into, CancellationToken cancellationToken);

    /// <summary>Null when git fails or the head is detached.</summary>
    Task<string?> CurrentBranchAsync(string directory, CancellationToken cancellationToken);

    /// <summary>The checkout's root as a Windows path; null when git fails, as it does outside a repository.</summary>
    Task<string?> RepositoryRootAsync(string directory, CancellationToken cancellationToken);

    /// <summary>The main checkout a linked worktree belongs to; null when git fails.</summary>
    Task<string?> MainCheckoutOfAsync(string directory, CancellationToken cancellationToken);

    /// <summary>The origin remote's URL as git has it; null when git fails, as it does when there is no origin.</summary>
    Task<string?> OriginUrlAsync(string repository, CancellationToken cancellationToken);

    /// <summary>Repository-relative paths, with forward slashes, of every file git ignores in the checkout. Throws <see cref="GitException"/> when git fails.</summary>
    Task<IReadOnlyList<string>> ListIgnoredFilesAsync(string repository, CancellationToken cancellationToken);
}