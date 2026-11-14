using Source191.Cader.Domain;
using Source191.Cader.Ports;
using System.Diagnostics;

namespace Source191.Cader.Application.Launch;

/// <summary>
/// Brings origin's default branch up to date before a new worktree branches from it, as Before branching says. Nothing
/// here fails a launch: git's refusals and the time limit every captured child shares become a <see cref="BaseUpdate"/>
/// whose words say what was not updated, and their messages are shown only to say what failed.
/// </summary>
public sealed class BaseUpdater(IGit git)
{
    public async Task<BaseUpdate> UpdateAsync(FolderPath repository, RemoteBranch remoteBranch, BeforeBranching choice, CancellationToken cancellationToken)
    {
        return choice switch
        {
            BeforeBranching.None => new BaseUpdate.Current(),
            BeforeBranching.Fetch => await FetchAsync(repository, remoteBranch, cancellationToken),
            BeforeBranching.Pull => await PullAsync(repository, remoteBranch, cancellationToken),
        };
    }

    /// <summary>The fetch, then the local branch of the same name fast-forwarded where it can be; never merged, rebased or created.</summary>
    private async Task<BaseUpdate> PullAsync(FolderPath repository, RemoteBranch remoteBranch, CancellationToken cancellationToken)
    {
        BaseUpdate fetched = await FetchAsync(repository, remoteBranch, cancellationToken);

        return fetched is BaseUpdate.Current ? await FastForwardAsync(repository, remoteBranch, cancellationToken) : fetched;
    }

    private async Task<BaseUpdate> FastForwardAsync(FolderPath repository, RemoteBranch remoteBranch, CancellationToken cancellationToken)
    {
        BranchName branch = remoteBranch.Branch;
        try
        {
            if (!await git.BranchExistsAsync(repository, branch, cancellationToken))
            {
                return new BaseUpdate.Current();
            }

            FastForwardPlace place = FastForwardPlace.Of(await git.ListWorktreesAsync(repository.Value, cancellationToken), branch);

            return await FastForwardAsync(repository, remoteBranch, place, cancellationToken);
        }
        catch (Exception failure) when (failure is GitException or ProcessTimedOutException)
        {
            return new BaseUpdate.NotFastForwarded(branch, new FastForwardRefusal.Refused(failure.Message));
        }
    }

    private async Task<BaseUpdate> FastForwardAsync(FolderPath repository, RemoteBranch remoteBranch, FastForwardPlace place, CancellationToken cancellationToken)
    {
        switch (place)
        {
            case FastForwardPlace.ElsewhereAt elsewhere:
                return new BaseUpdate.NotFastForwarded(remoteBranch.Branch, new FastForwardRefusal.CheckedOutElsewhere(elsewhere.Folder));
            case FastForwardPlace.MainCheckout:
                await git.FastForwardCheckoutAsync(repository, remoteBranch, cancellationToken);
                break;
            case FastForwardPlace.NotCheckedOut:
                await git.FastForwardBranchAsync(repository, remoteBranch.Branch, remoteBranch, cancellationToken);
                break;
            default:
                throw new UnreachableException();
        }

        return new BaseUpdate.Current();
    }

    private async Task<BaseUpdate> FetchAsync(FolderPath repository, RemoteBranch remoteBranch, CancellationToken cancellationToken)
    {
        try
        {
            await git.FetchBranchAsync(repository, remoteBranch, cancellationToken);

            return new BaseUpdate.Current();
        }
        catch (Exception failure) when (failure is GitException or ProcessTimedOutException)
        {
            return new BaseUpdate.NotFetched(remoteBranch, failure.Message);
        }
    }
}