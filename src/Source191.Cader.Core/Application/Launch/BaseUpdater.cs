using Source191.Cader.Domain;
using Source191.Cader.Ports;

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
            BeforeBranching.Pull => await FetchAsync(repository, remoteBranch, cancellationToken),
        };
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
