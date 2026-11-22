using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using System.Diagnostics;

namespace Source191.Cader.Application.Launch;

/// <summary>
/// Carries out a launch plan: brings origin's default branch up to date and creates the planned worktree from it when one
/// is wanted, copies the include files, starts the named background session, and records what the console needs to
/// remember about it. A base that could not be updated never stops the launch; the outcome says so. Each step is
/// reported to <see cref="LaunchProgress"/> as it begins, numbered out of the steps this launch runs.
/// </summary>
public sealed class LaunchService(IClaudeCli claude, IGit git, BaseUpdater updater, IRecentRepositories recent, ICreatedWorktrees created, IWorktreeIncludeCopier copier, TimeProvider clock, IProgress<LaunchStep> progress, DiagnosticLog log)
{
    public async Task<LaunchOutcome> LaunchAsync(LaunchPlan plan, CancellationToken cancellationToken)
    {
        Prepared prepared = new(LaunchSteps.WithoutANewWorktree(plan.Name), new BaseUpdate.Current());
        try
        {
            if (plan.Place is LaunchPlace.NewWorktree fresh)
            {
                prepared = await CreateWorktreeAsync(plan, fresh.Worktree, cancellationToken);
            }
        }
        catch (Exception failure) when (failure is GitException or IOException or UnauthorizedAccessException)
        {
            return new LaunchOutcome.Failed(failure.Message);
        }

        progress.Report(prepared.Steps.Step(LaunchStage.StartingClaude));
        StartResult start = await claude.StartBackgroundAsync(plan.Directory, plan.Name, plan.OpeningPrompt, cancellationToken);

        return start switch
        {
            StartResult.Started started => await StartedAsync(plan, started.JobId, prepared, cancellationToken),
            StartResult.WorkspaceNotTrusted => new LaunchOutcome.WorkspaceNotTrusted(),
            StartResult.Failed failed => new LaunchOutcome.Failed(failed.Message),
            _ => throw new UnreachableException(),
        };
    }

    private async Task<LaunchOutcome.Started> StartedAsync(LaunchPlan plan, JobId jobId, Prepared prepared, CancellationToken cancellationToken)
    {
        progress.Report(prepared.Steps.Step(LaunchStage.FindingSession));
        await RecordRecentRepositoryAsync(plan.Repository.Path);
        Session.Background? listed = await ListedSessionAsync(jobId, cancellationToken);

        return listed is null ? new LaunchOutcome.NotListedYet(jobId, prepared.Update) : new LaunchOutcome.Listed(jobId, listed, prepared.Update, prepared.Steps);
    }

    /// <summary>Origin's default branch is brought up to date only when the worktree branches from it; HEAD, or no origin/HEAD, is branched from as it is.</summary>
    private async Task<Prepared> CreateWorktreeAsync(LaunchPlan plan, PlannedWorktree worktree, CancellationToken cancellationToken)
    {
        FolderPath repository = plan.Repository.Path;
        string? remoteDefault = plan.Base == WorktreeBase.RemoteDefault ? await git.DefaultRemoteBranchAsync(repository.Value, cancellationToken) : null;
        if (!RemoteBranch.TryParse(remoteDefault, out RemoteBranch? remoteBranch))
        {
            LaunchSteps fromHead = LaunchSteps.ForNewWorktree(plan.Name);
            await AddWorktreeAsync(repository, worktree, "HEAD", fromHead, cancellationToken);

            return new Prepared(fromHead, new BaseUpdate.Current());
        }

        LaunchSteps steps = LaunchSteps.ForNewWorktree(plan.Name, remoteBranch, plan.BeforeBranching);
        ReportUpdate(steps, plan.BeforeBranching);
        BaseUpdate update = await updater.UpdateAsync(repository, remoteBranch, plan.BeforeBranching, cancellationToken);
        await AddWorktreeAsync(repository, worktree, remoteBranch.Value, steps, cancellationToken);

        return new Prepared(steps, update);
    }

    private void ReportUpdate(LaunchSteps steps, BeforeBranching choice)
    {
        switch (choice)
        {
            case BeforeBranching.Fetch:
                progress.Report(steps.Step(LaunchStage.Fetching));
                break;
            case BeforeBranching.Pull:
                progress.Report(steps.Step(LaunchStage.Pulling));
                break;
            case BeforeBranching.None:
                break;
            default:
                throw new UnreachableException();
        }
    }

    private async Task AddWorktreeAsync(FolderPath repository, PlannedWorktree worktree, string baseRef, LaunchSteps steps, CancellationToken cancellationToken)
    {
        progress.Report(steps.Step(LaunchStage.CreatingWorktree));
        await git.AddWorktreeAsync(repository.Value, worktree.Path.Value, worktree.Branch.Value, baseRef, cancellationToken);
        await created.RecordCreatedWorktreeAsync(new CreatedWorktree(worktree.Path, worktree.Branch, repository, clock.GetUtcNow()));
        await copier.CopyAsync(repository, worktree.Path, cancellationToken);
    }

    /// <summary>The session is started whatever the state folder says, so a refused write is logged, never raised.</summary>
    private async Task RecordRecentRepositoryAsync(FolderPath repository)
    {
        try
        {
            await recent.RecordRecentRepositoryAsync(repository);
        }
        catch (Exception failure) when (failure is IOException or UnauthorizedAccessException)
        {
            log.Record(new Failed(FailureSite.Effect, CodeName.For(typeof(LaunchService)), ExceptionSummary.From(failure)));
        }
    }

    /// <summary>
    /// The new session as the supervisor lists it, which is where its full session id comes from; null when it is not
    /// listed yet, or the listing failed (the session is running either way, and the next timed listing shows the failure).
    /// </summary>
    private async Task<Session.Background?> ListedSessionAsync(JobId jobId, CancellationToken cancellationToken)
    {
        try
        {
            SessionListing listing = await claude.ListSessionsAsync(cancellationToken);

            return listing.Sessions.OfType<Session.Background>().FirstOrDefault(session => session.JobId == jobId);
        }
        catch (Exception failure) when (failure is ClaudeCliException or ProcessTimedOutException)
        {
            return null;
        }
    }

    /// <summary>The steps this launch runs and what bringing its base up to date came to, once its worktree, if any, is made.</summary>
    private sealed record Prepared(LaunchSteps Steps, BaseUpdate Update);
}