using Source191.Cader.Domain;
using System.Diagnostics;

namespace Source191.Cader.Application.Views;

/// <summary>
/// A listed session with what git, the state store and the transcript folder add to it. Location says whether it runs
/// in a repository (and which main checkout), outside one, or in a folder that has gone; Repository is the name each gives.
/// Head is Unknown outside a repository, in a folder that has gone, or when git did not name a branch. Tab says whether a live session tab shows the session, and in which
/// colour. HasTranscript is false for a session never sent a message: Claude writes nothing to resume from until then,
/// so every caller must say which it is.
/// </summary>
public sealed record SessionRow(Session Session, SessionLocation Location, Head Head, LiveTab Tab, bool HasTranscript)
{
    /// <summary>The repository's name, or the folder's outside a repository or once it has gone: what REPOSITORY, the filter and the folder keys read.</summary>
    public string Repository => Location switch
    {
        SessionLocation.Outside outside => outside.Folder,
        SessionLocation.InRepository inRepository => inRepository.Name,
        SessionLocation.FolderGone gone => gone.Folder,
        _ => throw new UnreachableException(),
    };

    /// <summary>The branch's pull request, or why there is none; a row outside a repository has no branch to look up.</summary>
    public PullRequestLookup PullRequest { get; init; } = new PullRequestLookup.Missing(PullRequestGap.NoBranch);

    /// <summary>What the session's process and everything it started use; nothing to measure for a session with no process, or whose process has gone.</summary>
    public SessionMemory Memory { get; init; } = new SessionMemory.NothingToMeasure();

    /// <summary>The account the session belongs to, as far as its transcript and the accounts Cader has seen say.</summary>
    public RowAccount Account { get; init; } = new RowAccount.Unrecorded();

}