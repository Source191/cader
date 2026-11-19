using Microsoft.Extensions.Time.Testing;
using Source191.Cader.Adapters.Git;
using Source191.Cader.Application;
using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application;

/// <summary>The repositories a scan found, which the console watches for worktrees coming and going.</summary>
public sealed class WorktreeInventoryRepositoriesShould : IDisposable
{
    private const string Root = @"C:\Users\me\source\repos";

    private readonly FakeFolders folders = new FakeFolders()
        .WithDirectory(Path.Combine(Root, "Northwind.Orders", ".git", "objects"))
        .WithDirectory(Path.Combine(Root, "Northwind.Billing", ".git", "objects"))
        .WithDirectory(Path.Combine(Root, "Northwind.Notes", "docs"));

    private readonly PullRequestCatalogue catalogue = new(new GitCli(new FakeProcessRunner().AnswerAll(new ProcessResult(2, string.Empty, string.Empty)), "git"), new FakePullRequests(), new FakeTimeProvider(Fixtures.Noon), new RefreshCounts(new FakeTimeProvider(Fixtures.Noon), new DiagnosticLog(new FakeTimeProvider(Fixtures.Noon), [])));

    private readonly WorktreeInventory inventory;

    public WorktreeInventoryRepositoriesShould()
    {
        FakeProcessRunner processes = new FakeProcessRunner().AnswerAll(new ProcessResult(0, string.Empty, string.Empty));
        inventory = new WorktreeInventory(new GitCli(processes, "git"), folders, TestSettings.Current(Root), catalogue, new DiagnosticLog(new FakeTimeProvider(Fixtures.Noon), []));
    }

    [Fact]
    public void KnowNoRepositoryBeforeTheFirstScan()
    {
        inventory.Repositories.ShouldBeEmpty();
    }

    [Fact]
    public async Task KnowTheRepositoriesTheLatestScanFound()
    {
        await inventory.ScanAsync([], CancellationToken.None);

        inventory.Repositories.ShouldBe([FolderPath.From(Path.Combine(Root, "Northwind.Billing")), FolderPath.From(Path.Combine(Root, "Northwind.Orders"))]);
    }

    public void Dispose()
    {
        catalogue.Dispose();
    }
}