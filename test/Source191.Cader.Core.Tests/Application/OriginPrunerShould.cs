using Microsoft.Extensions.Time.Testing;
using Source191.Cader.Adapters.Git;
using Source191.Cader.Application;
using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application;

/// <summary>The prune after a scan over a scripted git and a fake clock: once per repository, at most once an hour.</summary>
public sealed class OriginPrunerShould
{
    private static readonly FolderPath Orders = FolderPath.From(@"C:\Users\me\source\repos\Northwind.Orders");
    private static readonly FolderPath Billing = FolderPath.From(@"C:\Users\me\source\repos\Northwind.Billing");

    private readonly FakeTimeProvider clock = new(Fixtures.Noon);
    private readonly FakeProcessRunner processes = new FakeProcessRunner().AnswerAll(new ProcessResult(0, string.Empty, string.Empty));
    private readonly DiagnosticLog log;
    private readonly OriginPruner pruner;

    public OriginPrunerShould()
    {
        log = new DiagnosticLog(clock, []);
        pruner = new OriginPruner(new GitCli(processes, "git"), clock, log);
    }

    [Fact]
    public async Task PruneEachRepositoryOnce()
    {
        await pruner.PruneDueAsync([Orders, Orders, Billing]);

        PrunedFolders().ShouldBe([Orders.Value, Billing.Value], ignoreOrder: true);
    }

    private IReadOnlyList<string> PrunedFolders()
    {
        return [.. processes.CapturedRequests
            .Where(request => request.Arguments.SequenceEqual(["remote", "prune", "origin"]))
            .Select(request => request.WorkingDirectory.ShouldBeOfType<WorkingFolder.At>().Path)];
    }
}
