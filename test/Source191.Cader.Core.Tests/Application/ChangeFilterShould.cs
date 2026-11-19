using Source191.Cader.Application;
using Source191.Cader.Ports;

namespace Source191.Cader.Core.Tests.Application;

public sealed class ChangeFilterShould
{
    [Theory]
    [InlineData(WatchKind.ClaudeSessions, "4242.json", FolderChangeKind.Created)]
    [InlineData(WatchKind.ClaudeSessions, "4242.json", FolderChangeKind.Changed)]
    [InlineData(WatchKind.ClaudeSessions, "4242.json", FolderChangeKind.Deleted)]
    [InlineData(WatchKind.ClaudeSessions, "4242.json", FolderChangeKind.Renamed)]
    [InlineData(WatchKind.ClaudeJobs, @"aba4b38e\state.json", FolderChangeKind.Created)]
    [InlineData(WatchKind.ClaudeJobs, @"aba4b38e\state.json", FolderChangeKind.Changed)]
    [InlineData(WatchKind.ClaudeJobs, @"aba4b38e\state.json", FolderChangeKind.Deleted)]
    [InlineData(WatchKind.ClaudeJobs, @"aba4b38e\state.json", FolderChangeKind.Renamed)]
    [InlineData(WatchKind.Repository, "worktrees", FolderChangeKind.Created)]
    [InlineData(WatchKind.Repository, "worktrees", FolderChangeKind.Deleted)]
    [InlineData(WatchKind.Repository, "worktrees", FolderChangeKind.Renamed)]
    [InlineData(WatchKind.Repository, @"worktrees\feature", FolderChangeKind.Created)]
    [InlineData(WatchKind.Repository, @"worktrees\feature", FolderChangeKind.Deleted)]
    [InlineData(WatchKind.Repository, @"worktrees\feature", FolderChangeKind.Renamed)]
    [InlineData(WatchKind.Repository, "HEAD", FolderChangeKind.Changed)]
    [InlineData(WatchKind.Repository, "HEAD", FolderChangeKind.Renamed)]
    [InlineData(WatchKind.Repository, @"worktrees\feature\HEAD", FolderChangeKind.Changed)]
    [InlineData(WatchKind.Repository, @"worktrees\feature\HEAD", FolderChangeKind.Renamed)]
    [InlineData(WatchKind.Root, "Northwind.Billing", FolderChangeKind.Created)]
    [InlineData(WatchKind.Root, "Northwind.Billing", FolderChangeKind.Deleted)]
    [InlineData(WatchKind.Root, "Northwind.Billing", FolderChangeKind.Renamed)]
    public void CountARecordOfWhatTheTabShows(WatchKind watch, string path, FolderChangeKind kind)
    {
        ChangeFilter.Counts(watch, new FolderChange(path, kind)).ShouldBeTrue();
    }

    [Theory]
    [InlineData(WatchKind.ClaudeSessions, "4242.1f9c03.key", FolderChangeKind.Created)]
    [InlineData(WatchKind.ClaudeSessions, @"archive\4242.json", FolderChangeKind.Created)]
    [InlineData(WatchKind.ClaudeJobs, @"aba4b38e\state.json.tmp.4f0d6ab5", FolderChangeKind.Created)]
    [InlineData(WatchKind.ClaudeJobs, @"aba4b38e\state.json.tmp.4f0d6ab5", FolderChangeKind.Changed)]
    [InlineData(WatchKind.ClaudeJobs, @"aba4b38e\timeline.jsonl", FolderChangeKind.Changed)]
    [InlineData(WatchKind.ClaudeJobs, "aba4b38e", FolderChangeKind.Created)]
    [InlineData(WatchKind.ClaudeJobs, "state.json", FolderChangeKind.Changed)]
    [InlineData(WatchKind.Repository, "worktrees", FolderChangeKind.Changed)]
    [InlineData(WatchKind.Repository, @"worktrees\feature", FolderChangeKind.Changed)]
    [InlineData(WatchKind.Repository, @"worktrees\feature\index.lock", FolderChangeKind.Created)]
    [InlineData(WatchKind.Repository, @"worktrees\feature\index", FolderChangeKind.Changed)]
    [InlineData(WatchKind.Repository, @"worktrees\feature\HEAD.lock", FolderChangeKind.Created)]
    [InlineData(WatchKind.Repository, @"worktrees\feature\logs\HEAD", FolderChangeKind.Changed)]
    [InlineData(WatchKind.Repository, "index", FolderChangeKind.Changed)]
    [InlineData(WatchKind.Repository, "index", FolderChangeKind.Renamed)]
    [InlineData(WatchKind.Repository, "index.lock", FolderChangeKind.Created)]
    [InlineData(WatchKind.Repository, "HEAD.lock", FolderChangeKind.Created)]
    [InlineData(WatchKind.Repository, "HEAD", FolderChangeKind.Deleted)]
    [InlineData(WatchKind.Repository, "FETCH_HEAD", FolderChangeKind.Changed)]
    [InlineData(WatchKind.Repository, "ORIG_HEAD", FolderChangeKind.Changed)]
    [InlineData(WatchKind.Repository, "packed-refs", FolderChangeKind.Renamed)]
    [InlineData(WatchKind.Repository, @"refs\heads\main", FolderChangeKind.Renamed)]
    [InlineData(WatchKind.Repository, @"logs\HEAD", FolderChangeKind.Changed)]
    [InlineData(WatchKind.Repository, @"objects\4f\0d6ab5", FolderChangeKind.Created)]
    [InlineData(WatchKind.Root, "Northwind.Billing", FolderChangeKind.Changed)]
    [InlineData(WatchKind.Root, @"Northwind.Billing\src", FolderChangeKind.Created)]
    public void IgnoreEverythingElse(WatchKind watch, string path, FolderChangeKind kind)
    {
        ChangeFilter.Counts(watch, new FolderChange(path, kind)).ShouldBeFalse();
    }

    [Theory]
    [InlineData(WatchKind.ClaudeSessions)]
    [InlineData(WatchKind.ClaudeJobs)]
    [InlineData(WatchKind.Repository)]
    [InlineData(WatchKind.Root)]
    public void AlwaysCountAnOverflowBecauseChangesWereLost(WatchKind watch)
    {
        ChangeFilter.Counts(watch, new FolderChange(string.Empty, FolderChangeKind.Overflowed)).ShouldBeTrue();
    }
}