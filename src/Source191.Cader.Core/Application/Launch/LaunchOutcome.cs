using Source191.Cader.Domain;

namespace Source191.Cader.Application.Launch;

/// <summary>What carrying out a launch plan came to.</summary>
public abstract record LaunchOutcome
{
    /// <summary>The session started under this job id, whether or not the supervisor lists it yet; <paramref name="Update"/> says whether its base was brought up to date first.</summary>
    public abstract record Started(JobId JobId, BaseUpdate Update) : LaunchOutcome;

    /// <summary>The supervisor listed the new job by the time the start returned.</summary>
    public sealed record Listed(JobId JobId, Session.Background Session, BaseUpdate Update) : Started(JobId, Update);

    /// <summary>The supervisor had not listed the new job by the time the start returned.</summary>
    public sealed record NotListedYet(JobId JobId, BaseUpdate Update) : Started(JobId, Update);

    /// <summary>The folder's repository has not been trusted interactively yet.</summary>
    public sealed record WorkspaceNotTrusted : LaunchOutcome;

    /// <summary>Git could not create the worktree, the file system refused the include copy or the record of the worktree, or the CLI could not start the session; the message says why.</summary>
    public sealed record Failed(string Message) : LaunchOutcome;
}