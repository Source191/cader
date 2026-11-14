using Source191.Cader.Domain;
using Source191.Cader.Ports;
using System.Diagnostics;

namespace Source191.Cader.Application.Launch;

/// <summary>
/// Carries out a launch plan: brings origin's default branch up to date and creates the planned worktree from it when one
/// is wanted, copies the include files, starts the named background session, and records what the console needs to
/// remember about it. A base that could not be updated never stops the launch; the outcome says so.
/// </summary>
public sealed class LaunchService(IClaudeCli claude, IGit git, BaseUpdater updater, IRecentRepositories recent, ICreatedWorktrees created, IWorktreeIncludeCopier copier, TimeProvider clock)
{
    public async Task<LaunchOutcome> LaunchAsync(LaunchPlan plan, CancellationToken cancellationToken)
    {
        BaseUpdate update = new BaseUpdate.Current();
        try
        {
            if (plan.Place is LaunchPlace.NewWorktree fresh)
            {
                update = await CreateWorktreeAsync(plan, fresh.Worktree, cancellationToken);
            }
        }
        catch (Exception failure) when (failure is GitException or IOException or UnauthorizedAccessException)
        {
            return new LaunchOutcome.Failed(failure.Message);
        }

        StartResult start = await claude.StartBackgroundAsync(plan.Directory, plan.Name, plan.OpeningPrompt, cancellationToken);

        return start switch
        {
            StartResult.Started started => await StartedAsync(plan, started.JobId, update, cancellationToken),
            StartResult.WorkspaceNotTrusted => new LaunchOutcome.WorkspaceNotTrusted(),
            StartResult.Failed failed => new LaunchOutcome.Failed(failed.Message),
            _ => throw new UnreachableException(),
        };
    }

    private async Task<LaunchOutcome.Started> StartedAsync(LaunchPlan plan, JobId jobId, BaseUpdate update, CancellationToken cancellationToken)
    {
        await recent.RecordRecentRepositoryAsync(plan.Repository.Path);
        Session.Background? listed = await ListedSessionAsync(jobId, cancellationToken);

        return listed is null ? new LaunchOutcome.NotListedYet(jobId, update) : new LaunchOutcome.Listed(jobId, listed, update);
    }

    /// <summary>Origin's default branch is brought up to date only when the worktree branches from it; HEAD, or no origin/HEAD, is branched from as it is.</summary>
    private async Task<BaseUpdate> CreateWorktreeAsync(LaunchPlan plan, PlannedWorktree worktree, CancellationToken cancellationToken)
    {
        FolderPath repository = plan.Repository.Path;
        string? remoteDefault = plan.Base == WorktreeBase.RemoteDefault ? await git.DefaultRemoteBranchAsync(repository.Value, cancellationToken) : null;
        if (!RemoteBranch.TryParse(remoteDefault, out RemoteBranch? remoteBranch))
        {
            await AddWorktreeAsync(repository, worktree, "HEAD", cancellationToken);

            return new BaseUpdate.Current();
        }

        BaseUpdate update = await updater.UpdateAsync(repository, remoteBranch, plan.BeforeBranching, cancellationToken);
        await AddWorktreeAsync(repository, worktree, remoteBranch.Value, cancellationToken);

        return update;
    }

    private async Task AddWorktreeAsync(FolderPath repository, PlannedWorktree worktree, string baseRef, CancellationToken cancellationToken)
    {
        await git.AddWorktreeAsync(repository.Value, worktree.Path.Value, worktree.Branch.Value, baseRef, cancellationToken);
        await created.RecordCreatedWorktreeAsync(new CreatedWorktree(worktree.Path, worktree.Branch, repository, clock.GetUtcNow()));
        await copier.CopyAsync(repository, worktree.Path, cancellationToken);
    }

    /// <summary>The new session as the supervisor lists it, which is where its full session id comes from.</summary>
    private async Task<Session.Background?> ListedSessionAsync(JobId jobId, CancellationToken cancellationToken)
    {
        SessionListing listing = await claude.ListSessionsAsync(cancellationToken);

        return listing.Sessions.OfType<Session.Background>().FirstOrDefault(session => session.JobId == jobId);
    }
}
