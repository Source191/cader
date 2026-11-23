using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.TestDoubles;

/// <summary>
/// Every state port in memory, with the same rules as the file store: recent repositories most recent first and
/// capped at ten, one created-worktree record per path.
/// </summary>
public sealed class InMemoryState : ITabRegistry, IRecentRepositories, ICreatedWorktrees, IUserConfig, IUpdateCheckRecords, ILastRunVersion, IKnownAccounts
{
    private const int RecentLimit = 10;

    private readonly Lock gate = new();
    private readonly Dictionary<Guid, TabRegistration> tabs = [];
    private readonly List<FolderPath> recent = [];
    private readonly List<CreatedWorktree> created = [];
    private UpdateCheckRecord updateCheck = new UpdateCheckRecord.Never();
    private UserConfig config = UserConfig.Missing;
    private LastRun lastRun = new LastRun.Never();
    private IReadOnlyList<KnownAccount> accounts = [];
    private Exception? nextRecordFailure;
    private Task nextAccountWrite = Task.CompletedTask;
    private Task nextConfigWrite = Task.CompletedTask;

    public Task<IReadOnlyList<TabRegistration>> ReadTabsAsync()
    {
        lock (gate)
        {
            return Task.FromResult<IReadOnlyList<TabRegistration>>([.. tabs.Values]);
        }
    }

    public Task WriteTabAsync(TabRegistration registration)
    {
        lock (gate)
        {
            tabs[registration.Id] = registration;
        }

        return Task.CompletedTask;
    }

    public Task RemoveTabAsync(Guid id)
    {
        lock (gate)
        {
            tabs.Remove(id);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<FolderPath>> ReadRecentRepositoriesAsync()
    {
        lock (gate)
        {
            return Task.FromResult<IReadOnlyList<FolderPath>>([.. recent]);
        }
    }

    public Task RecordRecentRepositoryAsync(FolderPath repository)
    {
        lock (gate)
        {
            recent.RemoveAll(path => path.Equals(repository));
            recent.Insert(0, repository);
            if (recent.Count > RecentLimit)
            {
                recent.RemoveRange(RecentLimit, recent.Count - RecentLimit);
            }
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<CreatedWorktree>> ReadCreatedWorktreesAsync()
    {
        lock (gate)
        {
            return Task.FromResult<IReadOnlyList<CreatedWorktree>>([.. created]);
        }
    }

    /// <summary>Makes the next <c>RecordCreatedWorktreeAsync</c> throw the exception, as a state file the file system refuses would.</summary>
    public void FailNextCreatedWorktreeRecord(Exception exception)
    {
        nextRecordFailure = exception;
    }

    public Task RecordCreatedWorktreeAsync(CreatedWorktree worktree)
    {
        if (nextRecordFailure is Exception failure)
        {
            nextRecordFailure = null;
            return Task.FromException(failure);
        }

        lock (gate)
        {
            created.RemoveAll(existing => existing.Path == worktree.Path);
            created.Add(worktree);
        }

        return Task.CompletedTask;
    }

    public Task ForgetCreatedWorktreeAsync(FolderPath path)
    {
        lock (gate)
        {
            created.RemoveAll(existing => existing.Path == path);
        }

        return Task.CompletedTask;
    }

    public Task<UserConfig> ReadConfigAsync()
    {
        lock (gate)
        {
            return Task.FromResult(config);
        }
    }

    /// <summary>Makes the next <c>WriteConfigAsync</c> wait for the task before writing, as a slow state file would.</summary>
    public void HoldNextConfigWrite(Task until)
    {
        lock (gate)
        {
            nextConfigWrite = until;
        }
    }

    public async Task WriteConfigAsync(UserConfig config)
    {
        Task held;
        lock (gate)
        {
            held = nextConfigWrite;
            nextConfigWrite = Task.CompletedTask;
        }

        await held;
        lock (gate)
        {
            this.config = config;
        }
    }

    public Task<UpdateCheckRecord> ReadUpdateCheckAsync()
    {
        return Task.FromResult(updateCheck);
    }

    public Task WriteUpdateCheckAsync(UpdateCheckRecord.Checked record)
    {
        updateCheck = record;
        return Task.CompletedTask;
    }

    /// <summary>How many times the running version was recorded.</summary>
    public int LastRunWrites { get; private set; }

    public Task<LastRun> ReadLastRunAsync()
    {
        return Task.FromResult(lastRun);
    }

    public Task WriteLastRunAsync(ReleaseVersion version)
    {
        lastRun = new LastRun.Ran(version);
        LastRunWrites++;
        return Task.CompletedTask;
    }

    /// <summary>How many times the remembered accounts were written.</summary>
    public int AccountWrites { get; private set; }

    public Task<IReadOnlyList<KnownAccount>> ReadAccountsAsync()
    {
        lock (gate)
        {
            return Task.FromResult(accounts);
        }
    }

    /// <summary>Makes the next <c>WriteAccountsAsync</c> wait for the task before writing, and fail if it faults, as a slow or refusing state file would.</summary>
    public void HoldNextAccountWrite(Task until)
    {
        lock (gate)
        {
            nextAccountWrite = until;
        }
    }

    public async Task WriteAccountsAsync(IReadOnlyList<KnownAccount> accounts)
    {
        Task held;
        lock (gate)
        {
            held = nextAccountWrite;
            nextAccountWrite = Task.CompletedTask;
        }

        await held;
        lock (gate)
        {
            this.accounts = [.. accounts];
            AccountWrites++;
        }
    }
}