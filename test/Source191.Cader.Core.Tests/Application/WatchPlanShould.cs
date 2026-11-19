using Source191.Cader.Application;
using Source191.Cader.Domain;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application;

public sealed class WatchPlanShould
{
    private const string Repos = @"C:\Users\me\source\repos";
    private const string Work = @"D:\work";

    [Fact]
    public void WatchTheClaudeFoldersEachRepositoryAndEachRoot()
    {
        IReadOnlyList<FolderPath> repositories = [Folder(@$"{Repos}\Northwind.Orders"), Folder(@$"{Repos}\Northwind.Billing"), Folder(@$"{Work}\Northwind.Shipping")];

        IReadOnlyList<PlannedWatch> plan = WatchPlan.For(TestSettings.For(Repos, Work), TestSettings.DefaultClaudeFolder, repositories);

        plan.ShouldBe(
        [
            new PlannedWatch(WatchKind.ClaudeSessions, Folder(@"C:\Users\me\.claude\sessions")),
            new PlannedWatch(WatchKind.ClaudeJobs, Folder(@"C:\Users\me\.claude\jobs")),
            new PlannedWatch(WatchKind.Repository, Folder(@$"{Repos}\Northwind.Orders\.git")),
            new PlannedWatch(WatchKind.Repository, Folder(@$"{Repos}\Northwind.Billing\.git")),
            new PlannedWatch(WatchKind.Repository, Folder(@$"{Work}\Northwind.Shipping\.git")),
            new PlannedWatch(WatchKind.Root, Folder(Repos)),
            new PlannedWatch(WatchKind.Root, Folder(Work)),
        ]);
    }

    [Fact]
    public void WatchTheClaudeFolderTheVariableNames()
    {
        ClaudeFolder named = new ClaudeFolder.FromVariable(Folder(@"D:\claude"));

        IReadOnlyList<PlannedWatch> plan = WatchPlan.For(TestSettings.For(Repos), named, []);

        plan.Where(watch => watch.Feeds is RefreshedTab.Sessions).ShouldBe([new PlannedWatch(WatchKind.ClaudeSessions, Folder(@"D:\claude\sessions")), new PlannedWatch(WatchKind.ClaudeJobs, Folder(@"D:\claude\jobs"))]);
    }

    [Fact]
    public void WatchEachFolderOnce()
    {
        IReadOnlyList<PlannedWatch> plan = WatchPlan.For(TestSettings.For(Repos), TestSettings.DefaultClaudeFolder, [Folder(@$"{Repos}\Northwind.Orders"), Folder(@$"{Repos}\NORTHWIND.ORDERS")]);

        plan.Count(watch => watch.Kind is WatchKind.Repository).ShouldBe(1);
    }

    [Theory]
    [InlineData(WatchKind.ClaudeSessions, false, RefreshedTab.Sessions)]
    [InlineData(WatchKind.ClaudeJobs, true, RefreshedTab.Sessions)]
    [InlineData(WatchKind.Repository, true, RefreshedTab.Worktrees)]
    [InlineData(WatchKind.Root, false, RefreshedTab.Worktrees)]
    public void SayWhetherSubfoldersAreWatchedAndWhichTabAChangeRefreshes(WatchKind kind, bool subfolders, RefreshedTab feeds)
    {
        PlannedWatch watch = new(kind, Folder(Repos));

        watch.Subfolders.ShouldBe(subfolders);
        watch.Feeds.ShouldBe(feeds);
    }

    private static FolderPath Folder(string path)
    {
        return FolderPath.From(path);
    }
}