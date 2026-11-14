using System.Diagnostics;
using Source191.Cader.Adapters.Git;
using Source191.Cader.Adapters.Processes;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Infrastructure.Tests;

/// <summary>
/// Pins what the real git does when a program holds a worktree's folder, which <c>WorktreeRemover</c> relies on: git
/// deletes the contents and the registration before it fails on the folder itself, so nothing is left to remove again.
/// </summary>
[Trait("Category", "Integration")]
public sealed class GitWorktreeRemoveShould : IDisposable
{
    private readonly TemporaryGitRoot root = new();

    private static GitCli Git => new(new ProcessRunner(TimeSpan.FromMinutes(1)), "git");

    [Fact]
    public async Task DropTheRegistrationAndEmptyTheFolderWhenAProgramHoldsIt()
    {
        string repository = await root.CreateRepositoryAsync("Northwind.Orders");
        string worktree = await root.AddWorktreeUnderTheWorktreeRootAsync(repository, "fix-123");
        using Process holder = StartSleeperIn(worktree);
        try
        {
            GitException exception = await Should.ThrowAsync<GitException>(() => Git.RemoveWorktreeAsync(repository, worktree, force: false, TestContext.Current.CancellationToken));

            exception.InUse.ShouldBeTrue();
            Directory.Exists(worktree).ShouldBeTrue();
            Directory.EnumerateFileSystemEntries(worktree).ShouldBeEmpty();
            (await Git.ListWorktreesAsync(repository, TestContext.Current.CancellationToken)).ShouldHaveSingleItem().IsMain.ShouldBeTrue();
        }
        finally
        {
            holder.Kill(entireProcessTree: true);
            holder.WaitForExit(TimeSpan.FromSeconds(5));
        }
    }

    /// <summary>A process whose working directory is the folder, as a tab's shell or a session's claude has.</summary>
    private static Process StartSleeperIn(string folder)
    {
        ProcessStartInfo start = new("cmd.exe", ["/c", "ping.exe -n 60 127.0.0.1 > nul"]) { UseShellExecute = false, CreateNoWindow = true, WorkingDirectory = folder };

        return Process.Start(start) ?? throw new InvalidOperationException("cmd did not start");
    }

    public void Dispose()
    {
        root.Dispose();
    }
}
